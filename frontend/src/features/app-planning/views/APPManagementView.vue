<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import {
  ArrowLeft,
  ClipboardList,
  Plus,
  RefreshCw,
  FileSpreadsheet,
  Info,
  Eye,
  Search,
  Columns,
  X,
  CheckCircle2,
  AlertTriangle,
  Layers,
} from '@lucide/vue'

import { useAuthStore } from '@/stores/auth'
import { masterDataApiService } from '@/features/ppmp-app/services/masterDataApiService'
import type { PlanningYearCategoryDto, PlanningYearDto } from '@/features/ppmp-app/models/master-data.types'
import { ppmpApiService } from '@/features/ppmp-app/services/ppmpApiService'
import { appApiService } from '../services/appApiService'
import { formatDatePH, formatPHP, getPPMPStatusStyle } from '@/shared/utilities/ppmpUiHelpers'

import type { APPDetail, APPListRecord } from '../models/app.types'

type APPItemRecord = APPDetail['categories'][number]['items'][number]
type APPApprovalAction = 'prepare' | 'recommend' | 'approve'
type APPItemSourceRecord = {
  appItemSourceId?: number
  ppmpId: number
  ppmpReference: string
  contributedQuantity?: number
  contributedBudget?: number
}

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const apps = ref<APPListRecord[]>([])
const planningYears = ref<PlanningYearDto[]>([])
const planningYearCategories = ref<PlanningYearCategoryDto[]>([])
const app = ref<APPDetail | null>(null)
const reviewedPPMPCounts = ref<Record<number, number>>({})

const isLoading = ref(true)
const isGenerating = ref(false)
const isAddingReviewedPPMPs = ref(false)
const isRecordingApproval = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

// Detail view UX controls
const itemSearch = ref('')
const showAllColumns = ref(false)
const selectedAppItem = ref<{ item: APPItemRecord; categoryName: string } | null>(null)

// In-app confirmation modal
const pendingConfirmAction = ref<{
  title: string
  description: string
  confirmLabel: string
  run: () => Promise<void>
} | null>(null)

const isProcurementStaff = computed(() => authStore.hasRole('Procurement Staff'))
const isAPPPreparer = computed(() =>
  authStore.hasRole('Bids and Awards Committee Secretariat'),
)
const isAPPRecommender = computed(() =>
  authStore.hasRole('Bids and Awards Committee Chairperson'),
)
const isAPPApprover = computed(() =>
  authStore.hasRole('Head of the Procuring Entity'),
)
const appId = computed(() => Number(route.params.id))
const isDetailView = computed(() => route.params.id !== undefined)

const categoryNames = computed(() => new Map(
  planningYearCategories.value.map(category => [category.categoryId, category.categoryName]),
))

const appByYearId = computed(() => new Map(
  apps.value.map(record => [record.planningYearId, record]),
))

const totalConsolidatedItems = computed(() =>
  app.value?.categories.reduce((total, category) => total + category.items.length, 0) ?? 0,
)

const approvalStages = computed(() => {
  const currentApp = app.value
  if (!currentApp) return []

  return [
    {
      action: 'prepare' as const,
      title: 'Prepared By',
      position: currentApp.preparedByPosition ?? 'Supervising Administrative Officer',
      role: 'Bids and Awards Committee Secretariat',
      userName: currentApp.preparedByName,
      actedAt: currentApp.preparedAt,
      isComplete: Boolean(currentApp.preparedByUserId),
    },
    {
      action: 'recommend' as const,
      title: 'Recommended By',
      position: currentApp.recommendedByPosition ?? 'Chief Administrative Officer',
      role: 'Bids and Awards Committee Chairperson',
      userName: currentApp.recommendedByName,
      actedAt: currentApp.recommendedAt,
      isComplete: Boolean(currentApp.recommendedByUserId),
    },
    {
      action: 'approve' as const,
      title: 'Approved By',
      position: currentApp.approvedByPosition ?? 'Medical Center Chief II',
      role: 'Head of the Procuring Entity',
      userName: currentApp.approvedByName,
      actedAt: currentApp.approvedAt,
      isComplete: Boolean(currentApp.approvedByUserId),
    },
  ]
})

function canRecordApproval(action: APPApprovalAction): boolean {
  if (!app.value || isRecordingApproval.value) return false

  if (action === 'prepare') {
    return isAPPPreparer.value && app.value.status === 'Draft'
  }
  if (action === 'recommend') {
    return isAPPRecommender.value && app.value.status === 'Prepared'
  }
  return isAPPApprover.value && app.value.status === 'Recommended'
}

// --------------------------------------------------
// RESILIENT FIELD RESOLVERS (HANDLES DTO VARIATIONS)
// --------------------------------------------------

function readString(record: unknown, keys: string[]): string | null {
  if (!record || typeof record !== 'object') return null
  const obj = record as Record<string, unknown>
  for (const key of keys) {
    const val = obj[key]
    if (typeof val === 'string' && val.trim() !== '') {
      return val.trim()
    }
  }
  return null
}

function getProjectName(item: APPItemRecord): string | null {
  return readString(item, [
    'procurementProjectName',
    'projectName',
    'itemName',
    'itemDescription',
    'title',
  ])
}

function getGeneralDescription(item: APPItemRecord): string | null {
  return readString(item, [
    'generalDescription',
    'generalDescriptionAndObjective',
    'description',
    'specifications',
  ])
}

function getEndUser(item: APPItemRecord): string | null {
  return readString(item, [
    'endUser',
    'endUserUnit',
    'departmentName',
    'implementingUnit',
    'unitName',
  ])
}

function getClassification(item: APPItemRecord, categoryName: string): string | null {
  return (
    readString(item, [
      'classification',
      'projectType',
      'itemType',
      'procurementClassification',
    ]) || categoryName
  )
}

function getModeOfProcurement(item: APPItemRecord): string | null {
  return readString(item, [
    'modeOfProcurement',
    'recommendedModeOfProcurement',
    'procurementMode',
  ])
}

function getActivityStart(item: APPItemRecord): string | null {
  return readString(item, [
    'startOfProcurementActivity',
    'activityStart',
    'procurementStart',
  ])
}

function getActivityEnd(item: APPItemRecord): string | null {
  return readString(item, [
    'endOfProcurementActivity',
    'activityEnd',
    'procurementEnd',
  ])
}

function getSourceOfFunds(item: APPItemRecord): string | null {
  return readString(item, ['sourceOfFunds', 'fundSource', 'fundingSource'])
}

function getItemSources(item: APPItemRecord): APPItemSourceRecord[] {
  const obj = item as unknown as Record<string, unknown>
  const rawSources = Array.isArray(obj.sources)
    ? obj.sources
    : Array.isArray(obj.ppmpSources)
      ? obj.ppmpSources
      : Array.isArray(obj.sourcePPMPs)
        ? obj.sourcePPMPs
        : []

  if (rawSources.length > 0) {
    return rawSources
      .filter((s): s is Record<string, unknown> => Boolean(s && typeof s === 'object'))
      .map((s, idx) => ({
        appItemSourceId: Number(s.appItemSourceId ?? s.id ?? idx + 1),
        ppmpId: Number(s.ppmpId ?? 0),
        ppmpReference: String(s.ppmpReference ?? s.reference ?? `PPMP #${s.ppmpId ?? idx + 1}`),
        contributedQuantity:
          typeof s.contributedQuantity === 'number'
            ? s.contributedQuantity
            : typeof s.quantity === 'number'
              ? s.quantity
              : undefined,
        contributedBudget:
          typeof s.contributedBudget === 'number'
            ? s.contributedBudget
            : typeof s.estimatedBudget === 'number'
              ? s.estimatedBudget
              : undefined,
      }))
  }

  // Fallback if single ppmpId / ppmpReference is flattened on the item
  const flatId = Number(obj.ppmpId ?? 0)
  const flatRef = readString(obj, ['ppmpReference', 'sourcePpmpReference'])
  if (flatId > 0 || flatRef) {
    return [
      {
        appItemSourceId: flatId || 1,
        ppmpId: flatId,
        ppmpReference: flatRef ?? `PPMP #${flatId}`,
      },
    ]
  }

  return []
}

const filteredCategories = computed(() => {
  if (!app.value) return []
  const q = itemSearch.value.trim().toLowerCase()
  if (!q) return app.value.categories

  return app.value.categories
    .map(category => {
      const catName = getCategoryName(category.categoryId)
      return {
        ...category,
        items: category.items.filter(item =>
          [
            getProjectName(item),
            getGeneralDescription(item),
            getEndUser(item),
            getClassification(item, catName),
            item.papCode,
            item.objectCode,
            getModeOfProcurement(item),
            ...getItemSources(item).map(source => source.ppmpReference),
          ].some(value => value?.toLowerCase().includes(q)),
        ),
      }
    })
    .filter(category => category.items.length > 0)
})

function getPlanningYear(yearId: number): number | undefined {
  return planningYears.value.find(year => year.planningYearId === yearId)?.year
}

function getCategoryName(categoryId: number): string {
  return categoryNames.value.get(categoryId) ?? `Category ${categoryId}`
}

function displayValue(value: string | number | null | undefined): string {
  return value === null || value === undefined || value === ''
    ? '—'
    : String(value)
}

function displayYesNo(value: boolean | null | undefined): string {
  return value === null || value === undefined ? '—' : value ? 'Yes' : 'No'
}

async function loadList(): Promise<void> {
  const [loadedApps, loadedYears] = await Promise.all([
    appApiService.getAll(),
    masterDataApiService.getPlanningYears(),
  ])
  const reviewedCounts = await Promise.all(
    loadedYears.map(async year => {
      const response = await ppmpApiService.getPaged({
        page: 1,
        pageSize: 1,
        status: 'Reviewed',
        planningYear: year.year,
      })
      return [year.planningYearId, response.totalCount] as const
    }),
  )
  apps.value = loadedApps
  planningYears.value = loadedYears
  reviewedPPMPCounts.value = Object.fromEntries(reviewedCounts)
}

async function loadDetail(): Promise<void> {
  if (!Number.isInteger(appId.value) || appId.value <= 0) {
    throw new Error('Invalid APP record ID.')
  }

  const loadedApp = await appApiService.getById(appId.value)
  const loadedYears = await masterDataApiService.getPlanningYears()
  const planningYear = loadedYears.find(
    year => year.planningYearId === loadedApp.planningYearId,
  )

  if (!planningYear) {
    throw new Error('The APP planning year could not be found.')
  }

  const [loadedCategories, reviewedPPMPs] = await Promise.all([
    masterDataApiService.getPlanningYearCategories(loadedApp.planningYearId),
    ppmpApiService.getPaged({
      page: 1,
      pageSize: 1,
      status: 'Reviewed',
      planningYear: planningYear.year,
    }),
  ])
  app.value = loadedApp
  planningYears.value = loadedYears
  planningYearCategories.value = loadedCategories
  reviewedPPMPCounts.value = {
    ...reviewedPPMPCounts.value,
    [loadedApp.planningYearId]: reviewedPPMPs.totalCount,
  }
}

async function load(): Promise<void> {
  isLoading.value = true
  errorMessage.value = ''

  try {
    if (isDetailView.value) {
      app.value = null
      itemSearch.value = ''
      selectedAppItem.value = null
      await loadDetail()
    } else {
      app.value = null
      planningYearCategories.value = []
      await loadList()
    }
  } catch (error) {
    console.error('Unable to load APP records:', error)
    errorMessage.value = error instanceof Error
      ? error.message
      : 'Unable to load APP records.'
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  void load()
})

watch(
  () => route.params.id,
  () => {
    successMessage.value = ''
    void load()
  },
)

function requestConfirmation(
  title: string,
  description: string,
  confirmLabel: string,
  run: () => Promise<void>,
): void {
  pendingConfirmAction.value = { title, description, confirmLabel, run }
}

async function confirmPendingAction(): Promise<void> {
  if (!pendingConfirmAction.value) return
  const action = pendingConfirmAction.value.run
  pendingConfirmAction.value = null
  await action()
}

function generateIndicativeAPP(year: PlanningYearDto): void {
  if (!isProcurementStaff.value || isGenerating.value) return

  const reviewedCount = reviewedPPMPCounts.value[year.planningYearId] ?? 0
  if (reviewedCount === 0) return

  requestConfirmation(
    `Generate FY ${year.year} Indicative APP`,
    `Consolidate ${reviewedCount} reviewed PPMP(s) into the FY ${year.year} Indicative APP? Those PPMPs will automatically be marked as Included in APP.`,
    'Generate Draft APP',
    async () => {
      isGenerating.value = true
      errorMessage.value = ''
      successMessage.value = ''

      try {
        const result = await appApiService.generate(year.planningYearId)
        await router.push({ name: 'app-detail', params: { id: result.appId } })
        successMessage.value =
          `Indicative APP ${year.year} was generated from ${result.includedPPMPCount} reviewed PPMP(s).`
      } catch (error) {
        console.error('Unable to generate Indicative APP:', error)
        errorMessage.value = error instanceof Error
          ? error.message
          : 'Unable to generate the Indicative APP.'
      } finally {
        isGenerating.value = false
      }
    },
  )
}

function addReviewedPPMPs(): void {
  if (!app.value || app.value.status !== 'Draft' || !isProcurementStaff.value ||
    isAddingReviewedPPMPs.value) {
    return
  }

  const currentAppId = app.value.appId
  const reviewedCount = reviewedPPMPCounts.value[app.value.planningYearId] ?? 0
  if (reviewedCount === 0) return

  requestConfirmation(
    'Add Subsequently Reviewed PPMPs',
    `Consolidate all ${reviewedCount} newly reviewed PPMP(s) into this Indicative APP? Those PPMPs will be marked as Included in APP.`,
    'Add Reviewed PPMPs',
    async () => {
      isAddingReviewedPPMPs.value = true
      errorMessage.value = ''
      successMessage.value = ''

      try {
        const result = await appApiService.addReviewedPPMPs(currentAppId)
        successMessage.value =
          `Added ${result.includedPPMPCount} reviewed PPMP(s) to this Indicative APP.`
        await loadDetail()
      } catch (error) {
        console.error('Unable to add reviewed PPMPs to APP:', error)
        errorMessage.value = error instanceof Error
          ? error.message
          : 'Unable to add reviewed PPMPs to this APP.'
      } finally {
        isAddingReviewedPPMPs.value = false
      }
    },
  )
}

function recordAPPApproval(action: APPApprovalAction): void {
  if (!app.value || !canRecordApproval(action)) return

  const currentAppId = app.value.appId
  const currentAppReference = app.value.appReference
  const actionDetails = {
    prepare: {
      title: 'Mark APP as Prepared',
      description: `Record ${currentAppReference} as prepared by the Supervising Administrative Officer, Bids and Awards Committee Secretariat.`,
      confirmLabel: 'Mark Prepared',
      run: appApiService.markPrepared,
      success: 'Prepared',
    },
    recommend: {
      title: 'Recommend APP',
      description: `Record the Chief Administrative Officer, Bids and Awards Committee Chairperson's recommendation for ${currentAppReference}.`,
      confirmLabel: 'Recommend APP',
      run: appApiService.markRecommended,
      success: 'Recommended',
    },
    approve: {
      title: 'Approve APP',
      description: `Record the Medical Center Chief II, Head of the Procuring Entity's approval for ${currentAppReference}.`,
      confirmLabel: 'Approve APP',
      run: appApiService.markApproved,
      success: 'Approved',
    },
  }[action]

  requestConfirmation(
    actionDetails.title,
    actionDetails.description,
    actionDetails.confirmLabel,
    async () => {
      isRecordingApproval.value = true
      errorMessage.value = ''
      successMessage.value = ''

      try {
        await actionDetails.run(currentAppId)
        successMessage.value = `${currentAppReference} marked ${actionDetails.success}.`
        await loadDetail()
      } catch (error) {
        console.error('Unable to record APP approval:', error)
        errorMessage.value = error instanceof Error
          ? error.message
          : 'Unable to record the APP approval.'
      } finally {
        isRecordingApproval.value = false
      }
    },
  )
}
</script>

<template>
  <div class="mx-auto max-w-7xl space-y-6">
    <!-- PAGE HEADER -->
    <div class="flex flex-wrap items-start justify-between gap-4">
      <div>
        <RouterLink
          v-if="isDetailView"
          :to="{ name: 'app-management' }"
          class="mb-3 inline-flex items-center gap-1.5 text-xs font-semibold text-slate-500 transition hover:text-brand-700"
        >
          <ArrowLeft :size="15" />
          Back to APP Management
        </RouterLink>

        <div class="flex items-start gap-3.5">
          <div class="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/20">
            <FileSpreadsheet :size="24" />
          </div>

          <div>
            <h1 class="text-2xl font-bold tracking-tight text-slate-900">
              {{ isDetailView ? (app?.appReference || 'Indicative APP Draft') : 'Annual Procurement Plans' }}
            </h1>
            <p class="mt-0.5 text-sm text-slate-500">
              Consolidate Procurement-reviewed PPMPs into an Indicative Annual Procurement Plan.
            </p>
          </div>
        </div>
      </div>

      <button
        type="button"
        :disabled="isLoading || isGenerating || isAddingReviewedPPMPs || isRecordingApproval"
        class="drmc-btn-secondary"
        @click="load"
      >
        <RefreshCw :size="16" :class="{ 'animate-spin': isLoading }" />
        Refresh
      </button>
    </div>

    <!-- DISMISSIBLE ALERTS -->
    <div
      v-if="successMessage"
      role="status"
      class="flex items-center justify-between gap-3 rounded-xl border border-brand-200 bg-brand-50 px-4 py-3 text-sm font-medium text-brand-800"
    >
      <div class="flex items-center gap-2">
        <CheckCircle2 :size="18" class="shrink-0 text-brand-600" />
        <span>{{ successMessage }}</span>
      </div>
      <button type="button" class="rounded-lg p-1 text-brand-700 hover:bg-brand-100" @click="successMessage = ''">
        <X :size="15" />
      </button>
    </div>

    <div
      v-if="errorMessage"
      role="alert"
      class="flex items-center justify-between gap-3 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-800"
    >
      <div class="flex items-center gap-2">
        <AlertTriangle :size="18" class="shrink-0 text-red-600" />
        <span>{{ errorMessage }}</span>
      </div>
      <button type="button" class="rounded-lg p-1 text-red-700 hover:bg-red-100" @click="errorMessage = ''">
        <X :size="15" />
      </button>
    </div>

    <!-- LOADING -->
    <div
      v-if="isLoading"
      class="drmc-card p-14 text-center"
    >
      <RefreshCw :size="26" class="mx-auto mb-3 animate-spin text-brand-700" />
      <p class="text-sm font-medium text-slate-600">Loading APP records...</p>
    </div>

    <!-- LIST VIEW -->
    <template v-else-if="!isDetailView">
      <div class="flex items-start gap-3 rounded-xl border border-blue-200/80 bg-blue-50/70 p-4 text-sm text-blue-900">
        <Info :size="18" class="mt-0.5 shrink-0 text-blue-600" />
        <p>
          This phase creates <strong>Indicative APP drafts</strong> and records the required Prepared, Recommended, and Approved sign-offs. Budget review, publication, and APP revisions are not included yet.
        </p>
      </div>

      <section class="drmc-card overflow-hidden">
        <div class="border-b border-slate-200/80 bg-slate-50/50 px-5 py-4">
          <h2 class="text-base font-semibold text-slate-900">APPs by Planning Year</h2>
          <p class="mt-0.5 text-xs text-slate-500">
            Generate a draft from PPMPs that Procurement has marked Reviewed.
          </p>
        </div>

        <div v-if="planningYears.length === 0" class="p-12 text-center">
          <ClipboardList :size="32" class="mx-auto mb-3 text-slate-300" />
          <p class="text-sm font-semibold text-slate-700">No planning years are available.</p>
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left text-sm">
            <thead class="border-b border-slate-200/80 bg-slate-50/80 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
              <tr>
                <th class="px-5 py-3.5">Planning Year</th>
                <th class="px-5 py-3.5">APP Reference</th>
                <th class="px-5 py-3.5">Status</th>
                <th class="px-5 py-3.5 text-right">Items</th>
                <th class="px-5 py-3.5 text-right">Estimated Budget</th>
                <th class="px-5 py-3.5 text-right">Action</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr
                v-for="year in planningYears"
                :key="year.planningYearId"
                class="transition-colors hover:bg-slate-50/80"
              >
                <td class="whitespace-nowrap px-5 py-4 font-semibold text-slate-900 tabular-nums">
                  Fiscal Year {{ year.year }}
                </td>

                <template v-if="appByYearId.get(year.planningYearId)">
                  <td class="whitespace-nowrap px-5 py-4 font-semibold text-brand-700">
                    <RouterLink
                      :to="{
                        name: 'app-detail',
                        params: { id: appByYearId.get(year.planningYearId)?.appId },
                      }"
                      class="hover:underline"
                    >
                      {{ appByYearId.get(year.planningYearId)?.appReference }}
                    </RouterLink>
                  </td>
                  <td class="whitespace-nowrap px-5 py-4">
                    <span
                      :class="[
                        'inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ring-1 ring-inset',
                        getPPMPStatusStyle(appByYearId.get(year.planningYearId)?.status ?? 'Draft').pill,
                      ]"
                    >
                      <span
                        :class="[
                          'h-1.5 w-1.5 rounded-full',
                          getPPMPStatusStyle(appByYearId.get(year.planningYearId)?.status ?? 'Draft').dot,
                        ]"
                      />
                      {{ appByYearId.get(year.planningYearId)?.status }}
                    </span>
                  </td>
                  <td class="whitespace-nowrap px-5 py-4 text-right text-slate-600 tabular-nums">
                    {{ appByYearId.get(year.planningYearId)?.itemCount }}
                  </td>
                  <td class="whitespace-nowrap px-5 py-4 text-right font-semibold text-slate-900 tabular-nums">
                    {{ formatPHP(appByYearId.get(year.planningYearId)?.totalEstimatedBudget ?? 0) }}
                  </td>
                  <td class="whitespace-nowrap px-5 py-4 text-right">
                    <RouterLink
                      :to="{
                        name: 'app-detail',
                        params: { id: appByYearId.get(year.planningYearId)?.appId },
                      }"
                      class="drmc-btn-secondary px-3 py-1.5 text-xs"
                    >
                      <Eye :size="14" />
                      View Draft
                    </RouterLink>
                  </td>
                </template>

                <template v-else>
                  <td colspan="4" class="px-5 py-4 text-xs text-slate-500">
                    No Indicative APP generated ·
                    <span class="font-semibold text-slate-700 tabular-nums">
                      {{ reviewedPPMPCounts[year.planningYearId] ?? 0 }}
                    </span>
                    reviewed PPMP(s) ready.
                  </td>
                  <td class="whitespace-nowrap px-5 py-4 text-right">
                    <button
                      v-if="isProcurementStaff"
                      type="button"
                      :disabled="isGenerating || (reviewedPPMPCounts[year.planningYearId] ?? 0) === 0"
                      class="drmc-btn-primary px-3 py-1.5 text-xs"
                      @click="generateIndicativeAPP(year)"
                    >
                      <Plus :size="14" />
                      {{
                        isGenerating
                          ? 'Generating...'
                          : reviewedPPMPCounts[year.planningYearId] === 0
                            ? 'No reviewed PPMPs'
                            : 'Generate Draft'
                      }}
                    </button>
                    <span v-else class="text-xs text-slate-400">Procurement Staff only</span>
                  </td>
                </template>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>

    <!-- DETAIL VIEW -->
    <template v-else-if="app">
      <div class="flex items-start gap-3 rounded-xl border border-blue-200/80 bg-blue-50/70 p-4 text-sm text-blue-900">
        <Info :size="18" class="mt-0.5 shrink-0 text-blue-600" />
        <p>
          <strong>Indicative APP.</strong> The sign-off sequence is Prepared → Recommended → Approved. PPMP additions stop once it is prepared; budget review and publication remain outside this phase.
        </p>
      </div>

      <!-- METADATA SUMMARY CARDS -->
      <section class="drmc-card grid gap-5 p-5 sm:grid-cols-2 lg:grid-cols-6">
        <div>
          <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">APP Reference</p>
          <p class="mt-1 font-mono text-sm font-bold text-brand-700">{{ app.appReference }}</p>
        </div>
        <div>
          <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">Planning Year</p>
          <p class="mt-1 font-semibold text-slate-900 tabular-nums">
            FY {{ getPlanningYear(app.planningYearId) ?? app.planningYearId }}
          </p>
        </div>
        <div>
          <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">Status</p>
          <div class="mt-1">
            <span
              :class="[
                'inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset',
                getPPMPStatusStyle(app.status).pill,
              ]"
            >
              <span :class="['h-1.5 w-1.5 rounded-full', getPPMPStatusStyle(app.status).dot]" />
              {{ app.status }}
            </span>
          </div>
        </div>
        <div>
          <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">Revision</p>
          <p class="mt-1 font-semibold text-slate-900 tabular-nums">No. {{ app.revisionNo }}</p>
        </div>
        <div>
          <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">Consolidated Categories</p>
          <p class="mt-1 font-semibold text-slate-900 tabular-nums">{{ app.categories.length }}</p>
        </div>
        <div>
          <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">Total Estimated Budget</p>
          <p class="mt-1 text-base font-extrabold text-brand-700 tabular-nums">
            {{ formatPHP(app.totalEstimatedBudget) }}
          </p>
        </div>
      </section>

      <!-- REQUIRED APP SIGN-OFFS -->
      <section class="drmc-card p-5">
        <div class="mb-4">
          <h2 class="text-base font-semibold text-slate-900">APP Approval Sign-offs</h2>
          <p class="mt-1 text-xs text-slate-500">
            Each stage is completed in order by a different assigned user. The APP is final only after HoPE approval.
          </p>
        </div>

        <div class="grid gap-3 lg:grid-cols-3">
          <article
            v-for="stage in approvalStages"
            :key="stage.action"
            class="rounded-xl border border-slate-200 bg-slate-50/60 p-4"
          >
            <div class="flex items-start justify-between gap-2">
              <div>
                <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">
                  {{ stage.title }}
                </p>
                <p class="mt-1 text-sm font-semibold text-slate-900">
                  {{ stage.position }}
                </p>
                <p class="mt-0.5 text-xs text-slate-500">{{ stage.role }}</p>
              </div>
              <span
                :class="[
                  'rounded-full px-2.5 py-1 text-[11px] font-semibold',
                  stage.isComplete
                    ? 'bg-brand-50 text-brand-700'
                    : 'bg-slate-100 text-slate-500',
                ]"
              >
                {{ stage.isComplete ? 'Completed' : 'Pending' }}
              </span>
            </div>

            <p class="mt-4 text-sm font-medium text-slate-800">
              {{ stage.userName || 'Awaiting sign-off' }}
            </p>
            <p v-if="stage.actedAt" class="mt-1 text-xs text-slate-500">
              {{ formatDatePH(stage.actedAt, { includeTime: true }) }}
            </p>

            <button
              v-if="canRecordApproval(stage.action)"
              type="button"
              :disabled="isRecordingApproval"
              class="drmc-btn-primary mt-4 w-full justify-center px-3 py-2 text-xs"
              @click="recordAPPApproval(stage.action)"
            >
              {{
                isRecordingApproval
                  ? 'Recording...'
                  : stage.action === 'prepare'
                    ? 'Mark as Prepared'
                    : stage.action === 'recommend'
                      ? 'Recommend APP'
                      : 'Approve APP'
              }}
            </button>
          </article>
        </div>
      </section>

      <!-- SUBSEQUENTLY REVIEWED PPMPS CALLOUT / ACTION BAR -->
      <div
        v-if="isProcurementStaff && app.status === 'Draft' && (reviewedPPMPCounts[app.planningYearId] ?? 0) > 0"
        class="flex flex-col justify-between gap-3 rounded-xl border border-amber-200 bg-amber-50/80 p-4 sm:flex-row sm:items-center"
      >
        <div class="flex items-start gap-2.5">
          <Layers :size="18" class="mt-0.5 shrink-0 text-amber-700" />
          <div>
            <p class="text-sm font-semibold text-amber-950">
              {{ reviewedPPMPCounts[app.planningYearId] }} newly reviewed PPMP(s) available for FY {{ getPlanningYear(app.planningYearId) }}
            </p>
            <p class="text-xs text-amber-800">
              Consolidate them into this Indicative APP draft so all reviewed submissions are included.
            </p>
          </div>
        </div>

        <button
          type="button"
          :disabled="isAddingReviewedPPMPs"
          class="drmc-btn-primary shrink-0 shadow-xs"
          @click="addReviewedPPMPs"
        >
          <Plus :size="16" />
          {{
            isAddingReviewedPPMPs
              ? 'Consolidating...'
              : `Add Reviewed PPMPs (${reviewedPPMPCounts[app.planningYearId]})`
          }}
        </button>
      </div>

      <!-- SEARCH & COLUMN DENSITY BAR -->
      <div
        v-if="app.categories.length > 0"
        class="drmc-card flex flex-col justify-between gap-3 bg-slate-50/40 p-4 sm:flex-row sm:items-center"
      >
        <div class="relative flex-1">
          <Search
            :size="16"
            class="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400"
          />
          <input
            v-model="itemSearch"
            type="text"
            placeholder="Filter consolidated items by project, description, unit, UACS, or PPMP reference..."
            class="drmc-input pl-10 pr-9"
          />
          <button
            v-if="itemSearch"
            type="button"
            class="absolute right-2.5 top-1/2 -translate-y-1/2 rounded-md p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-600"
            title="Clear search"
            @click="itemSearch = ''"
          >
            <X :size="14" />
          </button>
        </div>

        <div class="flex flex-wrap items-center gap-2.5">
          <span class="text-xs font-medium text-slate-500 tabular-nums">
            Created {{ formatDatePH(app.createdAt) }} · {{ totalConsolidatedItems }} item(s)
          </span>

          <button
            type="button"
            class="drmc-btn-secondary px-3.5 py-2 text-xs"
            @click="showAllColumns = !showAllColumns"
          >
            <Columns :size="15" />
            {{ showAllColumns ? 'Compact View (12 cols)' : 'Full APP Columns (27 cols)' }}
          </button>
        </div>
      </div>

      <!-- QUICK CATEGORY JUMP PILLS -->
      <div
        v-if="filteredCategories.length > 1"
        class="flex flex-wrap items-center gap-2"
      >
        <span class="text-xs font-semibold text-slate-400">Jump to:</span>
        <a
          v-for="cat in filteredCategories"
          :key="`jump-${cat.appCategoryId}`"
          :href="`#app-cat-${cat.appCategoryId}`"
          class="rounded-full border border-slate-200 bg-white px-3 py-1 text-xs font-semibold text-slate-700 transition hover:border-brand-400 hover:bg-brand-50 hover:text-brand-700"
        >
          {{ getCategoryName(cat.categoryId) }} ({{ cat.items.length }})
        </a>
      </div>

      <!-- CATEGORY SECTIONS -->
      <section
        v-for="category in filteredCategories"
        :id="`app-cat-${category.appCategoryId}`"
        :key="category.appCategoryId"
        class="drmc-card scroll-mt-6 overflow-hidden"
      >
        <div class="flex flex-wrap items-center justify-between gap-3 border-b border-slate-200/80 bg-slate-50/70 px-5 py-4">
          <div>
            <h2 class="text-base font-semibold text-slate-900">{{ getCategoryName(category.categoryId) }}</h2>
            <p class="mt-0.5 text-xs text-slate-500 tabular-nums">
              {{ category.items.length }} consolidated item(s)
            </p>
          </div>
          <div class="text-right">
            <p class="text-[10px] font-semibold uppercase tracking-wider text-slate-400">Category Total</p>
            <span class="text-base font-extrabold text-brand-700 tabular-nums">
              {{ formatPHP(category.totalEstimatedBudget) }}
            </span>
          </div>
        </div>

        <div class="overflow-x-auto">
          <table class="w-full text-left text-xs">
            <thead class="border-b border-slate-200/80 bg-slate-50 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
              <tr>
                <th v-if="showAllColumns" class="whitespace-nowrap px-4 py-3">PAP Code</th>
                <th v-if="showAllColumns" class="whitespace-nowrap px-4 py-3">Object Code / UACS</th>
                <th class="sticky left-0 z-10 min-w-60 bg-slate-50 px-4 py-3 shadow-[1px_0_0_0_rgb(226,232,240)]">
                  Procurement Project
                </th>
                <th class="min-w-52 px-4 py-3">General Description</th>
                <th class="whitespace-nowrap px-4 py-3">End-User / Unit</th>
                <th class="whitespace-nowrap px-4 py-3">Classification</th>
                <th class="whitespace-nowrap px-4 py-3">Mode</th>
                <th v-if="showAllColumns" class="whitespace-nowrap px-4 py-3">Bid Criteria</th>
                <th v-if="showAllColumns" class="whitespace-nowrap px-4 py-3">Marginalized Sector</th>
                <th v-if="showAllColumns" class="whitespace-nowrap px-4 py-3">Green Specs</th>
                <th v-if="showAllColumns" class="whitespace-nowrap px-4 py-3">Target PR Date</th>
                <th class="whitespace-nowrap px-4 py-3">Activity Start</th>
                <th class="whitespace-nowrap px-4 py-3">Activity End</th>
                <th class="whitespace-nowrap px-4 py-3">Source of Funds</th>
                <th class="whitespace-nowrap px-4 py-3 text-right">ABC Total</th>
                <th v-if="showAllColumns" class="whitespace-nowrap px-4 py-3 text-right">MOOE</th>
                <th v-if="showAllColumns" class="whitespace-nowrap px-4 py-3 text-right">Capital Outlay</th>
                <th v-if="showAllColumns" class="whitespace-nowrap px-4 py-3 text-right">Multi-Year Cost</th>
                <th v-if="showAllColumns" class="whitespace-nowrap px-4 py-3 text-right">Life Cycle Cost</th>
                <th v-if="showAllColumns" class="whitespace-nowrap px-4 py-3">Strategy / Tools</th>
                <th v-if="showAllColumns" class="whitespace-nowrap px-4 py-3">Prospective Bidders</th>
                <th v-if="showAllColumns" class="whitespace-nowrap px-4 py-3">Observers</th>
                <th v-if="showAllColumns" class="whitespace-nowrap px-4 py-3">Procurement Agent</th>
                <th v-if="showAllColumns" class="whitespace-nowrap px-4 py-3">Collaborating Entities</th>
                <th class="whitespace-nowrap px-4 py-3">Remarks</th>
                <th class="whitespace-nowrap px-4 py-3">Source PPMP</th>
                <th class="whitespace-nowrap px-4 py-3 text-center">Details</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr
                v-for="item in category.items"
                :key="item.appItemId"
                class="group transition-colors hover:bg-slate-50/90"
              >
                <td v-if="showAllColumns" class="whitespace-nowrap px-4 py-3.5 font-mono text-slate-600">{{ displayValue(item.papCode) }}</td>
                <td v-if="showAllColumns" class="whitespace-nowrap px-4 py-3.5 font-mono text-slate-600">{{ displayValue(item.objectCode) }}</td>

                <td class="sticky left-0 z-10 max-w-xs bg-white px-4 py-3.5 font-semibold text-slate-900 shadow-[1px_0_0_0_rgb(241,245,249)] group-hover:bg-slate-50">
                  <p class="line-clamp-2" :title="getProjectName(item) || undefined">
                    {{ displayValue(getProjectName(item)) }}
                  </p>
                </td>

                <td class="max-w-xs px-4 py-3.5 text-slate-600">
                  <p class="line-clamp-2" :title="getGeneralDescription(item) || undefined">
                    {{ displayValue(getGeneralDescription(item)) }}
                  </p>
                </td>

                <td class="whitespace-nowrap px-4 py-3.5 text-slate-700">
                  {{ displayValue(getEndUser(item)) }}
                </td>

                <td class="whitespace-nowrap px-4 py-3.5 text-slate-600">
                  {{ displayValue(getClassification(item, getCategoryName(category.categoryId))) }}
                </td>

                <td class="whitespace-nowrap px-4 py-3.5 text-slate-600">
                  {{ displayValue(getModeOfProcurement(item)) }}
                </td>

                <td v-if="showAllColumns" class="whitespace-nowrap px-4 py-3.5 text-slate-600">{{ displayValue(item.bidEvaluationCriteria) }}</td>
                <td v-if="showAllColumns" class="whitespace-nowrap px-4 py-3.5 text-slate-600">{{ displayYesNo(item.marginalizedSectorConsideration) }}</td>
                <td v-if="showAllColumns" class="whitespace-nowrap px-4 py-3.5 text-slate-600">{{ displayYesNo(item.greenTechnicalSpecifications) }}</td>
                <td v-if="showAllColumns" class="whitespace-nowrap px-4 py-3.5 text-slate-600">{{ displayValue(item.targetPRSubmissionDate) }}</td>

                <td class="whitespace-nowrap px-4 py-3.5 text-slate-600 tabular-nums">
                  {{ displayValue(getActivityStart(item)) }}
                </td>
                <td class="whitespace-nowrap px-4 py-3.5 text-slate-600 tabular-nums">
                  {{ displayValue(getActivityEnd(item)) }}
                </td>
                <td class="whitespace-nowrap px-4 py-3.5 text-slate-600">
                  {{ displayValue(getSourceOfFunds(item)) }}
                </td>

                <td class="whitespace-nowrap px-4 py-3.5 text-right font-bold text-slate-900 tabular-nums">
                  {{ formatPHP(item.estimatedBudget) }}
                </td>

                <td v-if="showAllColumns" class="whitespace-nowrap px-4 py-3.5 text-right text-slate-600 tabular-nums">{{ item.mooe === null ? '—' : formatPHP(item.mooe) }}</td>
                <td v-if="showAllColumns" class="whitespace-nowrap px-4 py-3.5 text-right text-slate-600 tabular-nums">{{ item.capitalOutlay === null ? '—' : formatPHP(item.capitalOutlay) }}</td>
                <td v-if="showAllColumns" class="whitespace-nowrap px-4 py-3.5 text-right text-slate-600 tabular-nums">{{ item.multiYearContractCost === null ? '—' : formatPHP(item.multiYearContractCost) }}</td>
                <td v-if="showAllColumns" class="whitespace-nowrap px-4 py-3.5 text-right text-slate-600 tabular-nums">{{ item.lifeCycleCost === null ? '—' : formatPHP(item.lifeCycleCost) }}</td>
                <td v-if="showAllColumns" class="whitespace-nowrap px-4 py-3.5 text-slate-600">{{ displayValue(item.procurementStrategyTools) }}</td>
                <td v-if="showAllColumns" class="whitespace-nowrap px-4 py-3.5 text-slate-600">{{ displayValue(item.prospectiveBidders) }}</td>
                <td v-if="showAllColumns" class="whitespace-nowrap px-4 py-3.5 text-slate-600">{{ displayValue(item.potentialObservers) }}</td>
                <td v-if="showAllColumns" class="whitespace-nowrap px-4 py-3.5 text-slate-600">{{ displayValue(item.procurementAgent) }}</td>
                <td v-if="showAllColumns" class="whitespace-nowrap px-4 py-3.5 text-slate-600">{{ displayValue(item.collaboratingEntities) }}</td>

                <td class="max-w-44 px-4 py-3.5 text-slate-600">
                  <p class="line-clamp-2" :title="item.remarks || undefined">
                    {{ displayValue(item.remarks) }}
                  </p>
                </td>

                <td class="whitespace-nowrap px-4 py-3.5">
                  <div class="flex flex-wrap gap-1.5">
                    <template v-for="source in getItemSources(item)" :key="source.appItemSourceId">
                      <RouterLink
                        v-if="source.ppmpId > 0"
                        :to="{ name: 'ppmp-details', params: { id: source.ppmpId } }"
                        class="rounded-md bg-brand-50 px-2 py-0.5 text-xs font-semibold text-brand-700 ring-1 ring-inset ring-brand-600/20 hover:bg-brand-100"
                        :title="
                          source.contributedBudget !== undefined
                            ? `${source.contributedQuantity ?? ''} · ${formatPHP(source.contributedBudget)}`
                            : source.ppmpReference
                        "
                      >
                        {{ source.ppmpReference }}
                      </RouterLink>
                      <span
                        v-else
                        class="rounded-md bg-slate-100 px-2 py-0.5 text-xs font-semibold text-slate-700"
                      >
                        {{ source.ppmpReference }}
                      </span>
                    </template>

                    <span v-if="getItemSources(item).length === 0" class="text-xs text-slate-400">
                      —
                    </span>
                  </div>
                </td>

                <td class="whitespace-nowrap px-4 py-3.5 text-center">
                  <button
                    type="button"
                    class="inline-flex items-center gap-1 rounded-lg border border-blue-200 bg-blue-50/70 px-2.5 py-1 text-xs font-semibold text-blue-700 transition hover:bg-blue-100"
                    @click="selectedAppItem = { item, categoryName: getCategoryName(category.categoryId) }"
                  >
                    <Eye :size="13" />
                    Details
                  </button>
                </td>
              </tr>

              <tr v-if="category.items.length === 0">
                <td :colspan="showAllColumns ? 27 : 12" class="px-4 py-8 text-center text-sm text-slate-500">
                  No items in this category.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <div
        v-if="app.categories.length > 0 && filteredCategories.length === 0"
        class="drmc-card p-10 text-center"
      >
        <p class="text-sm font-semibold text-slate-700">No consolidated items match "{{ itemSearch }}".</p>
        <button
          type="button"
          class="drmc-btn-secondary mt-3 text-xs"
          @click="itemSearch = ''"
        >
          Clear search
        </button>
      </div>

      <div v-if="app.categories.length === 0" class="drmc-card p-10 text-center text-sm text-slate-500">
        This APP draft has no category data.
      </div>
    </template>
  </div>

  <!-- ================================================================ -->
  <!-- APP ITEM FULL DETAILS MODAL                                      -->
  <!-- ================================================================ -->
  <Teleport to="body">
    <div
      v-if="selectedAppItem"
      class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 p-4 backdrop-blur-2xs"
      @click.self="selectedAppItem = null"
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-label="APP Consolidated Item Details"
        class="flex max-h-[88vh] w-full max-w-3xl flex-col overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-2xl"
      >
        <div class="flex items-start justify-between gap-4 border-b border-slate-200 bg-slate-50/80 px-6 py-4">
          <div>
            <span class="rounded-lg bg-brand-50 px-2.5 py-0.5 text-xs font-semibold text-brand-700 ring-1 ring-inset ring-brand-600/20">
              {{ selectedAppItem.categoryName }}
            </span>
            <h3 class="mt-1.5 text-base font-bold text-slate-900">
              Consolidated APP Line Item Details
            </h3>
          </div>

          <button
            type="button"
            class="rounded-lg p-1.5 text-slate-400 transition hover:bg-slate-200/70 hover:text-slate-700"
            @click="selectedAppItem = null"
          >
            <X :size="20" />
          </button>
        </div>

        <div class="flex-1 space-y-5 overflow-y-auto p-6 text-xs">
          <!-- PROJECT & DESCRIPTION -->
          <div class="rounded-xl border border-slate-200/80 bg-slate-50/60 p-4">
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Procurement Project
            </p>
            <p class="mt-1 whitespace-pre-wrap text-sm font-bold text-slate-900">
              {{ displayValue(getProjectName(selectedAppItem.item)) }}
            </p>

            <div class="mt-3 border-t border-slate-200/80 pt-3">
              <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
                General Description
              </p>
              <p class="mt-1 whitespace-pre-wrap leading-relaxed text-slate-700">
                {{ displayValue(getGeneralDescription(selectedAppItem.item)) }}
              </p>
            </div>
          </div>

          <!-- BUDGET SUMMARY -->
          <div class="grid grid-cols-2 gap-3 sm:grid-cols-5">
            <div class="rounded-xl border border-brand-200 bg-brand-50/40 p-3">
              <p class="text-[10px] font-semibold uppercase tracking-wider text-brand-700">ABC Total</p>
              <p class="mt-1 text-sm font-extrabold text-brand-700 tabular-nums">
                {{ formatPHP(selectedAppItem.item.estimatedBudget) }}
              </p>
            </div>
            <div class="rounded-xl border border-slate-200/80 p-3">
              <p class="text-[10px] font-semibold uppercase tracking-wider text-slate-400">MOOE</p>
              <p class="mt-1 font-semibold text-slate-800 tabular-nums">
                {{ selectedAppItem.item.mooe === null ? '—' : formatPHP(selectedAppItem.item.mooe) }}
              </p>
            </div>
            <div class="rounded-xl border border-slate-200/80 p-3">
              <p class="text-[10px] font-semibold uppercase tracking-wider text-slate-400">Capital Outlay</p>
              <p class="mt-1 font-semibold text-slate-800 tabular-nums">
                {{ selectedAppItem.item.capitalOutlay === null ? '—' : formatPHP(selectedAppItem.item.capitalOutlay) }}
              </p>
            </div>
            <div class="rounded-xl border border-slate-200/80 p-3">
              <p class="text-[10px] font-semibold uppercase tracking-wider text-slate-400">Multi-Year Cost</p>
              <p class="mt-1 font-semibold text-slate-800 tabular-nums">
                {{ selectedAppItem.item.multiYearContractCost === null ? '—' : formatPHP(selectedAppItem.item.multiYearContractCost) }}
              </p>
            </div>
            <div class="rounded-xl border border-slate-200/80 p-3">
              <p class="text-[10px] font-semibold uppercase tracking-wider text-slate-400">Life Cycle Cost</p>
              <p class="mt-1 font-semibold text-slate-800 tabular-nums">
                {{ selectedAppItem.item.lifeCycleCost === null ? '—' : formatPHP(selectedAppItem.item.lifeCycleCost) }}
              </p>
            </div>
          </div>

          <!-- PROCUREMENT & SCHEDULE FIELDS -->
          <div class="grid grid-cols-1 gap-3 rounded-xl border border-slate-200/80 p-4 sm:grid-cols-2">
            <div>
              <span class="font-semibold text-slate-500">End-User / Implementing Unit:</span>
              <p class="mt-0.5 font-medium text-slate-900">{{ displayValue(getEndUser(selectedAppItem.item)) }}</p>
            </div>
            <div>
              <span class="font-semibold text-slate-500">Classification:</span>
              <p class="mt-0.5 font-medium text-slate-900">
                {{ displayValue(getClassification(selectedAppItem.item, selectedAppItem.categoryName)) }}
              </p>
            </div>
            <div>
              <span class="font-semibold text-slate-500">PAP Code / Object Code (UACS):</span>
              <p class="mt-0.5 font-mono font-medium text-slate-900">
                {{ displayValue(selectedAppItem.item.papCode) }} / {{ displayValue(selectedAppItem.item.objectCode) }}
              </p>
            </div>
            <div>
              <span class="font-semibold text-slate-500">Mode of Procurement:</span>
              <p class="mt-0.5 font-medium text-slate-900">{{ displayValue(getModeOfProcurement(selectedAppItem.item)) }}</p>
            </div>
            <div>
              <span class="font-semibold text-slate-500">Activity Timeline:</span>
              <p class="mt-0.5 font-medium tabular-nums text-slate-900">
                {{ displayValue(getActivityStart(selectedAppItem.item)) }} to {{ displayValue(getActivityEnd(selectedAppItem.item)) }}
              </p>
            </div>
            <div>
              <span class="font-semibold text-slate-500">Target PR Submission Date:</span>
              <p class="mt-0.5 font-medium text-slate-900">{{ displayValue(selectedAppItem.item.targetPRSubmissionDate) }}</p>
            </div>
            <div>
              <span class="font-semibold text-slate-500">Source of Funds:</span>
              <p class="mt-0.5 font-medium text-slate-900">{{ displayValue(getSourceOfFunds(selectedAppItem.item)) }}</p>
            </div>
            <div>
              <span class="font-semibold text-slate-500">Bid Evaluation Criteria:</span>
              <p class="mt-0.5 font-medium text-slate-900">{{ displayValue(selectedAppItem.item.bidEvaluationCriteria) }}</p>
            </div>
            <div>
              <span class="font-semibold text-slate-500">Marginalized Sector / Green Specs:</span>
              <p class="mt-0.5 font-medium text-slate-900">
                Marginalized: {{ displayYesNo(selectedAppItem.item.marginalizedSectorConsideration) }} ·
                Green Specs: {{ displayYesNo(selectedAppItem.item.greenTechnicalSpecifications) }}
              </p>
            </div>
            <div>
              <span class="font-semibold text-slate-500">Strategy / Tools:</span>
              <p class="mt-0.5 font-medium text-slate-900">{{ displayValue(selectedAppItem.item.procurementStrategyTools) }}</p>
            </div>
            <div>
              <span class="font-semibold text-slate-500">Prospective Bidders / Observers:</span>
              <p class="mt-0.5 font-medium text-slate-900">
                {{ displayValue(selectedAppItem.item.prospectiveBidders) }} · {{ displayValue(selectedAppItem.item.potentialObservers) }}
              </p>
            </div>
            <div>
              <span class="font-semibold text-slate-500">Procurement Agent / Collaborating Entities:</span>
              <p class="mt-0.5 font-medium text-slate-900">
                {{ displayValue(selectedAppItem.item.procurementAgent) }} · {{ displayValue(selectedAppItem.item.collaboratingEntities) }}
              </p>
            </div>
            <div class="sm:col-span-2">
              <span class="font-semibold text-slate-500">Remarks:</span>
              <p class="mt-0.5 whitespace-pre-wrap font-medium text-slate-900">{{ displayValue(selectedAppItem.item.remarks) }}</p>
            </div>
          </div>

          <!-- SOURCE PPMPS -->
          <div>
            <h4 class="mb-2 text-xs font-bold uppercase tracking-wider text-slate-500">
              Source PPMP Contributions ({{ getItemSources(selectedAppItem.item).length }})
            </h4>

            <div
              v-if="getItemSources(selectedAppItem.item).length > 0"
              class="overflow-hidden rounded-xl border border-slate-200"
            >
              <table class="w-full text-left text-xs">
                <thead class="bg-slate-50 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
                  <tr>
                    <th class="px-3.5 py-2">PPMP Reference</th>
                    <th class="px-3.5 py-2 text-right">Contributed Qty</th>
                    <th class="px-3.5 py-2 text-right">Contributed Budget</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-100">
                  <tr v-for="src in getItemSources(selectedAppItem.item)" :key="src.appItemSourceId">
                    <td class="px-3.5 py-2 font-semibold text-brand-700">
                      <RouterLink
                        v-if="src.ppmpId > 0"
                        :to="{ name: 'ppmp-details', params: { id: src.ppmpId } }"
                        class="hover:underline"
                        @click="selectedAppItem = null"
                      >
                        {{ src.ppmpReference }}
                      </RouterLink>
                      <span v-else>{{ src.ppmpReference }}</span>
                    </td>
                    <td class="px-3.5 py-2 text-right tabular-nums text-slate-700">
                      {{ src.contributedQuantity ?? '—' }}
                    </td>
                    <td class="px-3.5 py-2 text-right font-semibold tabular-nums text-slate-900">
                      {{ src.contributedBudget !== undefined ? formatPHP(src.contributedBudget) : '—' }}
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>

            <p v-else class="rounded-xl bg-slate-50 p-3 text-slate-500">
              No source PPMP breakdown recorded for this item.
            </p>
          </div>
        </div>

        <div class="flex justify-end border-t border-slate-200 bg-slate-50 px-6 py-3.5">
          <button
            type="button"
            class="drmc-btn-secondary px-4 py-2 text-sm"
            @click="selectedAppItem = null"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  </Teleport>

  <!-- CONFIRMATION MODAL -->
  <Teleport to="body">
    <div
      v-if="pendingConfirmAction"
      class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/55 p-4 backdrop-blur-2xs"
      @click.self="pendingConfirmAction = null"
    >
      <div
        role="dialog"
        aria-modal="true"
        :aria-label="pendingConfirmAction.title"
        class="w-full max-w-md rounded-2xl border border-slate-200 bg-white p-6 shadow-2xl"
      >
        <h3 class="text-lg font-bold text-slate-900">
          {{ pendingConfirmAction.title }}
        </h3>
        <p class="mt-2 text-sm leading-relaxed text-slate-600">
          {{ pendingConfirmAction.description }}
        </p>

        <div class="mt-6 flex justify-end gap-2.5">
          <button
            type="button"
            class="drmc-btn-secondary px-4 py-2 text-sm"
            @click="pendingConfirmAction = null"
          >
            Cancel
          </button>
          <button
            type="button"
            class="drmc-btn-primary px-4 py-2 text-sm"
            @click="confirmPendingAction"
          >
            {{ pendingConfirmAction.confirmLabel }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>