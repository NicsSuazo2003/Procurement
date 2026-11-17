<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { ArrowLeft, ClipboardList, Plus, RefreshCw } from '@lucide/vue'

import { useAuthStore } from '@/stores/auth'
import { masterDataApiService } from '@/features/ppmp-app/services/masterDataApiService'
import type { PlanningYearCategoryDto, PlanningYearDto } from '@/features/ppmp-app/models/master-data.types'
import { ppmpApiService } from '@/features/ppmp-app/services/ppmpApiService'
import { appApiService } from '../services/appApiService'

import type { APPDetail, APPListRecord } from '../models/app.types'

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
const errorMessage = ref('')
const successMessage = ref('')

const isProcurementStaff = computed(() => authStore.hasRole('Procurement Staff'))
const appId = computed(() => Number(route.params.id))
const isDetailView = computed(() => route.params.id !== undefined)
const categoryNames = computed(() => new Map(
  planningYearCategories.value.map(category => [category.categoryId, category.categoryName]),
))

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-PH', {
    style: 'currency',
    currency: 'PHP',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount)
}

function formatDate(value: string): string {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value

  return new Intl.DateTimeFormat('en-PH', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  }).format(date)
}

function getPlanningYear(yearId: number): number | undefined {
  return planningYears.value.find(year => year.planningYearId === yearId)?.year
}

function getCategoryName(categoryId: number): string {
  return categoryNames.value.get(categoryId) ?? `Category ${categoryId}`
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

async function generateIndicativeAPP(year: PlanningYearDto): Promise<void> {
  if (!isProcurementStaff.value || isGenerating.value) return

  const reviewedCount = reviewedPPMPCounts.value[year.planningYearId] ?? 0
  if (reviewedCount === 0) return

  const confirmed = window.confirm(
    `Generate the FY ${year.year} Indicative APP from ${reviewedCount} reviewed PPMP(s)? ` +
    'These PPMPs will be marked as Included in APP.',
  )
  if (!confirmed) return

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
}

async function addReviewedPPMPs(): Promise<void> {
  if (!app.value || app.value.status !== 'Draft' || !isProcurementStaff.value ||
    isAddingReviewedPPMPs.value) {
    return
  }

  const reviewedCount = reviewedPPMPCounts.value[app.value.planningYearId] ?? 0
  if (reviewedCount === 0) return

  const confirmed = window.confirm(
    `Add all ${reviewedCount} reviewed PPMP(s) to this Indicative APP? ` +
    'These PPMPs will be marked as Included in APP.',
  )
  if (!confirmed) return

  isAddingReviewedPPMPs.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    const result = await appApiService.addReviewedPPMPs(app.value.appId)
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
}
</script>

<template>
  <div class="mx-auto max-w-7xl space-y-6">
    <div class="flex flex-wrap items-start justify-between gap-4">
      <div>
        <RouterLink
          v-if="isDetailView"
          :to="{ name: 'app-management' }"
          class="mb-3 inline-flex items-center gap-2 text-sm font-medium text-slate-500 hover:text-emerald-700"
        >
          <ArrowLeft :size="16" />
          Back to APP Management
        </RouterLink>

        <div class="flex items-center gap-2">
          <ClipboardList :size="25" class="text-emerald-700" />
          <h1 class="text-2xl font-bold text-slate-900">
            {{ isDetailView ? 'Indicative APP Draft' : 'Annual Procurement Plans' }}
          </h1>
        </div>
        <p class="mt-1 text-sm text-slate-500">
          Consolidate Procurement-reviewed PPMPs into an Indicative Annual Procurement Plan.
        </p>
      </div>

      <button
        type="button"
        :disabled="isLoading || isGenerating || isAddingReviewedPPMPs"
        class="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
        @click="load"
      >
        <RefreshCw :size="16" :class="{ 'animate-spin': isLoading }" />
        Refresh
      </button>
    </div>

    <div
      v-if="successMessage"
      class="rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800"
      role="status"
    >
      {{ successMessage }}
    </div>

    <div
      v-if="errorMessage"
      class="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800"
      role="alert"
    >
      {{ errorMessage }}
    </div>

    <div
      v-if="isLoading"
      class="rounded-xl border border-slate-200 bg-white p-12 text-center text-sm text-slate-500"
    >
      Loading APP records...
    </div>

    <template v-else-if="!isDetailView">
      <div class="rounded-lg border border-blue-200 bg-blue-50 p-4 text-sm text-blue-900">
        This phase creates Indicative APP drafts only. Budget review, MCC/HoPE approval,
        publication, and APP revisions are not included yet.
      </div>

      <section class="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
        <div class="border-b border-slate-200 px-5 py-4">
          <h2 class="font-semibold text-slate-900">APPs by Planning Year</h2>
          <p class="mt-1 text-xs text-slate-500">
            Generate a draft from PPMPs that Procurement has marked Reviewed.
          </p>
        </div>

        <div v-if="planningYears.length === 0" class="p-8 text-center text-sm text-slate-500">
          No planning years are available.
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left text-sm">
            <thead class="bg-slate-50 text-xs uppercase text-slate-500">
              <tr>
                <th class="px-5 py-3 font-semibold">Planning Year</th>
                <th class="px-5 py-3 font-semibold">APP Reference</th>
                <th class="px-5 py-3 font-semibold">Status</th>
                <th class="px-5 py-3 text-right font-semibold">Items</th>
                <th class="px-5 py-3 text-right font-semibold">Estimated Budget</th>
                <th class="px-5 py-3 text-right font-semibold">Action</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr v-for="year in planningYears" :key="year.planningYearId">
                <td class="px-5 py-4 font-medium text-slate-800">
                  Fiscal Year {{ year.year }}
                </td>
                <template v-if="apps.find(record => record.planningYearId === year.planningYearId)">
                  <td class="px-5 py-4 text-slate-700">
                    {{ apps.find(record => record.planningYearId === year.planningYearId)?.appReference }}
                  </td>
                  <td class="px-5 py-4">
                    <span class="rounded-full bg-amber-50 px-3 py-1 text-xs font-medium text-amber-700">
                      {{ apps.find(record => record.planningYearId === year.planningYearId)?.status }}
                    </span>
                  </td>
                  <td class="px-5 py-4 text-right text-slate-600">
                    {{ apps.find(record => record.planningYearId === year.planningYearId)?.itemCount }}
                  </td>
                  <td class="px-5 py-4 text-right font-medium text-slate-800">
                    {{ formatCurrency(apps.find(record => record.planningYearId === year.planningYearId)?.totalEstimatedBudget ?? 0) }}
                  </td>
                  <td class="px-5 py-4 text-right">
                    <RouterLink
                      :to="{
                        name: 'app-detail',
                        params: { id: apps.find(record => record.planningYearId === year.planningYearId)?.appId },
                      }"
                      class="inline-flex items-center gap-2 rounded-lg border border-emerald-700 px-3 py-2 text-xs font-semibold text-emerald-700 hover:bg-emerald-50"
                    >
                      View draft
                    </RouterLink>
                  </td>
                </template>
                <template v-else>
                  <td colspan="4" class="px-5 py-4 text-sm text-slate-500">
                    No Indicative APP generated ·
                    {{ reviewedPPMPCounts[year.planningYearId] ?? 0 }} reviewed PPMP(s) ready.
                  </td>
                  <td class="px-5 py-4 text-right">
                    <button
                      v-if="isProcurementStaff"
                      type="button"
                      :disabled="isGenerating || (reviewedPPMPCounts[year.planningYearId] ?? 0) === 0"
                      class="inline-flex items-center gap-2 rounded-lg bg-emerald-700 px-3 py-2 text-xs font-semibold text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-50"
                      @click="generateIndicativeAPP(year)"
                    >
                      <Plus :size="14" />
                      {{
                        isGenerating
                          ? 'Generating...'
                          : reviewedPPMPCounts[year.planningYearId] === 0
                            ? 'No reviewed PPMPs'
                            : 'Generate draft'
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

    <template v-else-if="app">
      <div class="rounded-lg border border-blue-200 bg-blue-50 p-4 text-sm text-blue-900">
        Indicative APP draft only. It has not been submitted to Budget or the MCC/HoPE for approval.
      </div>

      <section class="grid gap-4 rounded-xl border border-slate-200 bg-white p-5 shadow-sm sm:grid-cols-2 lg:grid-cols-4">
        <div>
          <p class="text-xs uppercase text-slate-500">APP Reference</p>
          <p class="mt-1 font-semibold text-slate-900">{{ app.appReference }}</p>
        </div>
        <div>
          <p class="text-xs uppercase text-slate-500">Planning Year</p>
          <p class="mt-1 font-semibold text-slate-900">
            {{ getPlanningYear(app.planningYearId) ?? app.planningYearId }}
          </p>
        </div>
        <div>
          <p class="text-xs uppercase text-slate-500">Status</p>
          <p class="mt-1 font-semibold text-amber-700">{{ app.status }}</p>
        </div>
        <div>
          <p class="text-xs uppercase text-slate-500">Total Estimated Budget</p>
          <p class="mt-1 font-semibold text-emerald-700">
            {{ formatCurrency(app.totalEstimatedBudget) }}
          </p>
        </div>
      </section>

      <div class="flex flex-wrap items-center justify-between gap-3">
        <p class="text-sm text-slate-500">
          Created {{ formatDate(app.createdAt) }} · {{ app.categories.length }} category group(s)
        </p>
        <button
          v-if="isProcurementStaff && app.status === 'Draft'"
          type="button"
          :disabled="isAddingReviewedPPMPs || (reviewedPPMPCounts[app.planningYearId] ?? 0) === 0"
          class="inline-flex items-center gap-2 rounded-lg bg-emerald-700 px-4 py-2.5 text-sm font-semibold text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-50"
          @click="addReviewedPPMPs"
        >
          <Plus :size="16" />
          {{
            isAddingReviewedPPMPs
              ? 'Adding...'
              : reviewedPPMPCounts[app.planningYearId] === 0
                ? 'No reviewed PPMPs to add'
                : 'Add subsequently reviewed PPMPs'
          }}
        </button>
      </div>

      <section
        v-for="category in app.categories"
        :key="category.appCategoryId"
        class="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm"
      >
        <div class="flex flex-wrap items-center justify-between gap-3 border-b border-slate-200 px-5 py-4">
          <h2 class="font-semibold text-slate-900">{{ getCategoryName(category.categoryId) }}</h2>
          <span class="text-sm font-semibold text-emerald-700">
            {{ formatCurrency(category.totalEstimatedBudget) }}
          </span>
        </div>

        <div class="overflow-x-auto">
          <table class="w-full text-left text-sm">
            <thead class="bg-slate-50 text-xs uppercase text-slate-500">
              <tr>
                <th class="px-4 py-3 font-semibold">Project / Item</th>
                <th class="px-4 py-3 font-semibold">Unit</th>
                <th class="px-4 py-3 text-right font-semibold">Total Quantity</th>
                <th class="px-4 py-3 text-right font-semibold">Estimated Unit Price</th>
                <th class="px-4 py-3 text-right font-semibold">Estimated Budget</th>
                <th class="px-4 py-3 font-semibold">Source PPMP</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr v-for="item in category.items" :key="item.appItemId">
                <td class="max-w-sm px-4 py-4">
                  <p class="font-medium text-slate-800">{{ item.itemDescription }}</p>
                  <p v-if="item.itemCode" class="mt-1 text-xs text-slate-500">{{ item.itemCode }}</p>
                </td>
                <td class="px-4 py-4 text-slate-600">{{ item.unit }}</td>
                <td class="px-4 py-4 text-right tabular-nums text-slate-700">{{ item.totalQuantity }}</td>
                <td class="px-4 py-4 text-right tabular-nums text-slate-700">
                  {{ formatCurrency(item.estimatedUnitPrice) }}
                </td>
                <td class="px-4 py-4 text-right font-medium tabular-nums text-slate-800">
                  {{ formatCurrency(item.estimatedBudget) }}
                </td>
                <td class="px-4 py-4">
                  <div class="flex flex-wrap gap-2">
                    <RouterLink
                      v-for="source in item.sources"
                      :key="source.appItemSourceId"
                      :to="{ name: 'ppmp-details', params: { id: source.ppmpId } }"
                      class="text-xs font-medium text-emerald-700 hover:underline"
                      :title="`${source.contributedQuantity} ${item.unit} · ${formatCurrency(source.contributedBudget)}`"
                    >
                      {{ source.ppmpReference }}
                    </RouterLink>
                    <span v-if="item.sources.length === 0" class="text-xs text-slate-400">
                      No source PPMP
                    </span>
                  </div>
                </td>
              </tr>
              <tr v-if="category.items.length === 0">
                <td colspan="6" class="px-4 py-8 text-center text-sm text-slate-500">
                  No items in this category.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <div v-if="app.categories.length === 0" class="rounded-xl border border-slate-200 bg-white p-10 text-center text-sm text-slate-500">
        This APP draft has no category data.
      </div>
    </template>
  </div>
</template>
