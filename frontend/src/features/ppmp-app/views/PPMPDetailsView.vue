<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'

import {
  ArrowLeft,
  Pencil,
  ClipboardList,
  Package,
  Clock,
  CheckCircle,
  Check,
  RefreshCw,
  ChevronDown,
  Printer,
  ChevronsUpDown,
} from '@lucide/vue'

import { useAuthStore } from '@/stores/auth'
import { ppmpApiService } from '../services/ppmpApiService'
import { formatPHP, getPPMPStatusStyle } from '@/shared/utilities/ppmpUiHelpers'

import type { PPMPDetailApi } from '../models/ppmp-api.types'
import type { PPMPChangeHistoryApi } from '../models/ppmp-history.types'
import type { PPMPStatus } from '../models/ppmp.types'

interface WorkflowStageConfig {
  status: PPMPStatus
  activePill: string
  activeBadge: string
  ring: string
}

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

// --------------------------------------------------
// STATE
// --------------------------------------------------

const ppmp = ref<PPMPDetailApi | null>(null)
const changeHistory = ref<PPMPChangeHistoryApi[]>([])

const isLoading = ref(false)
const isLoadingChangeHistory = ref(false)
const isSubmitting = ref(false)

const errorMessage = ref('')
const successMessage = ref('')
const changeHistoryError = ref('')

// Expanded spreadsheet rows
const expandedItems = ref<number[]>([])

// In-app workflow confirmation dialog state
const pendingConfirmAction = ref<{
  title: string
  description: string
  confirmLabel: string
  run: () => Promise<void>
} | null>(null)

// --------------------------------------------------
// AUTHENTICATED USER ROLES
// --------------------------------------------------

const isEndUser = computed(() => authStore.hasRole('End-user'))
const isUnitHead = computed(() => authStore.hasRole('Unit Head'))
const isProcurementStaff = computed(() => authStore.hasRole('Procurement Staff'))
const isSystemAdministrator = computed(() => authStore.hasRole('System Administrator'))

// --------------------------------------------------
// PPMP INFORMATION & WORKFLOW PROGRESS
// --------------------------------------------------

const workflowStages: WorkflowStageConfig[] = [
  {
    status: 'Draft',
    activePill: 'border-slate-600 bg-slate-700 text-white',
    activeBadge: 'bg-white/20 text-white',
    ring: 'ring-2 ring-slate-600/30 shadow-xs',
  },
  {
    status: 'For Unit Head Approval',
    activePill: 'border-amber-500 bg-amber-600 text-white',
    activeBadge: 'bg-white/20 text-white',
    ring: 'ring-2 ring-amber-500/35 shadow-xs',
  },
  {
    status: 'Approved by Unit Head',
    activePill: 'border-teal-600 bg-teal-600 text-white',
    activeBadge: 'bg-white/20 text-white',
    ring: 'ring-2 ring-teal-500/35 shadow-xs',
  },
  {
    status: 'Under Review',
    activePill: 'border-blue-600 bg-blue-600 text-white',
    activeBadge: 'bg-white/20 text-white',
    ring: 'ring-2 ring-blue-500/35 shadow-xs',
  },
  {
    status: 'Reviewed',
    activePill: 'border-indigo-600 bg-indigo-600 text-white',
    activeBadge: 'bg-white/20 text-white',
    ring: 'ring-2 ring-indigo-500/35 shadow-xs',
  },
  {
    status: 'Included in APP',
    activePill: 'border-emerald-600 bg-emerald-600 text-white',
    activeBadge: 'bg-white/20 text-white',
    ring: 'ring-2 ring-emerald-500/35 shadow-xs',
  },
]

const ppmpId = computed(() => {
  const id = Number(route.params.id)
  return Number.isInteger(id) && id > 0 ? id : null
})

const currentStatus = computed<PPMPStatus | null>(() => {
  return (ppmp.value?.status as PPMPStatus) ?? null
})

const currentStageIndex = computed(() => {
  if (!currentStatus.value) return 0
  if (currentStatus.value === 'Updated') return 3
  const idx = workflowStages.findIndex((stage) => stage.status === currentStatus.value)
  return idx >= 0 ? idx : 0
})

const isDraft = computed(() => currentStatus.value === 'Draft')

const isFinalPPMP = computed(() =>
  currentStatus.value === 'Reviewed' || currentStatus.value === 'Included in APP',
)

const canEndUserSubmitToUnitHead = computed(() => isEndUser.value && isDraft.value)

const canUnitHeadApprove = computed(() =>
  isUnitHead.value && currentStatus.value === 'For Unit Head Approval',
)

const canProcurementStartReview = computed(() =>
  isProcurementStaff.value && currentStatus.value === 'Approved by Unit Head',
)

const canProcurementEdit = computed(() =>
  isProcurementStaff.value && currentStatus.value === 'Under Review',
)

const canProcurementCompleteReview = computed(() =>
  isProcurementStaff.value &&
  (currentStatus.value === 'Under Review' || currentStatus.value === 'Updated'),
)

const hasAvailableActions = computed(
  () =>
    canUnitHeadApprove.value ||
    canProcurementStartReview.value ||
    canProcurementEdit.value ||
    canProcurementCompleteReview.value ||
    canEndUserSubmitToUnitHead.value,
)

const shouldShowChangeHistory = computed(() =>
  currentStatus.value === 'Updated' ||
  currentStatus.value === 'Reviewed' ||
  currentStatus.value === 'Included in APP' ||
  changeHistory.value.length > 0,
)

const formattedBudget = computed(() => formatPHP(ppmp.value?.estimatedBudget ?? 0))

const preparedByName = computed(() => {
  const record = ppmp.value as Record<string, unknown> | null
  const fromApi =
    (typeof record?.createdByName === 'string' && record.createdByName) ||
    (typeof record?.submittedByName === 'string' && record.submittedByName) ||
    (typeof record?.preparedByName === 'string' && record.preparedByName)
  return fromApi || authStore.user?.fullName || ''
})

const preparedByPosition = computed(() => {
  const positionFromApi = ppmp.value?.createdByPosition?.trim()
  if (positionFromApi) {
    return positionFromApi
  }

  return ppmp.value?.createdBy === authStore.user?.userId
    ? authStore.user?.position?.trim() || ''
    : ''
})

// --------------------------------------------------
// SPREADSHEET ITEM GROUPING
// --------------------------------------------------

const groupedItems = computed(() => {
  if (!ppmp.value) {
    return []
  }

  const groups = new Map<string, PPMPDetailApi['items']>()

  for (const item of ppmp.value.items) {
    const category = item.categoryName || 'Uncategorized'

    if (!groups.has(category)) {
      groups.set(category, [])
    }

    groups.get(category)!.push(item)
  }

  return Array.from(groups.entries()).map(([categoryName, items]) => ({
    categoryName,
    items,
    totalQuantity: items.reduce((total, item) => total + Number(item.quantity || 0), 0),
    totalBudget: items.reduce((total, item) => total + Number(item.estimatedBudget || 0), 0),
  }))
})

// --------------------------------------------------
// SPREADSHEET HELPERS
// --------------------------------------------------

function toggleItem(itemId: number): void {
  if (expandedItems.value.includes(itemId)) {
    expandedItems.value = expandedItems.value.filter((id) => id !== itemId)
  } else {
    expandedItems.value.push(itemId)
  }
}

function toggleAllItems(): void {
  if (!ppmp.value) return
  if (expandedItems.value.length === ppmp.value.items.length) {
    expandedItems.value = []
  } else {
    expandedItems.value = ppmp.value.items.map((item) => item.ppmpItemId)
  }
}

function isItemExpanded(itemId: number): boolean {
  return expandedItems.value.includes(itemId)
}

function getItemAllocationTotal(item: PPMPDetailApi['items'][number]): number {
  return item.sectionAllocations.reduce(
    (total, allocation) => total + Number(allocation.quantity || 0),
    0,
  )
}

function getPrintField(item: PPMPDetailApi['items'][number], key: string): string {
  const value = (item as unknown as Record<string, unknown>)[key]
  return typeof value === 'string' ? value : ''
}

function getPrintPreProc(item: PPMPDetailApi['items'][number]): string {
  const value = (item as unknown as Record<string, unknown>).preProcurementConference
  if (value === true) return 'YES'
  if (value === false) return 'NO'
  return 'NO'
}

function formatMonth(month: string): string {
  const match = /^(\d{4})-(\d{2})$/.exec(month)
  return match ? `${match[2]}/${match[1]}` : month || ''
}

function printPPMP(): void {
  window.print()
}

// --------------------------------------------------
// LOAD PPMP CHANGE HISTORY
// --------------------------------------------------

async function loadChangeHistory(id: number): Promise<void> {
  isLoadingChangeHistory.value = true
  changeHistoryError.value = ''

  try {
    changeHistory.value = await ppmpApiService.getChangeHistory(id)
  } catch (error) {
    console.error('Failed to load PPMP change history:', error)
    changeHistory.value = []
    changeHistoryError.value =
      error instanceof Error ? error.message : 'Unable to load PPMP change history.'
  } finally {
    isLoadingChangeHistory.value = false
  }
}

// --------------------------------------------------
// LOAD PPMP DETAILS
// --------------------------------------------------

async function loadPPMP(): Promise<void> {
  if (!ppmpId.value) {
    ppmp.value = null
    changeHistory.value = []
    errorMessage.value = 'Invalid PPMP record ID.'
    return
  }

  const id = ppmpId.value

  isLoading.value = true
  errorMessage.value = ''
  successMessage.value = ''
  changeHistoryError.value = ''
  changeHistory.value = []
  expandedItems.value = []

  try {
    ppmp.value = await ppmpApiService.getById(id)
    await loadChangeHistory(id)
  } catch (error) {
    console.error('Failed to load PPMP details:', error)
    ppmp.value = null
    errorMessage.value =
      error instanceof Error ? error.message : 'Unable to load PPMP details.'
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  void loadPPMP()
})

watch(
  () => route.params.id,
  () => {
    void loadPPMP()
  },
)

// --------------------------------------------------
// WORKFLOW ACTIONS & CONFIRMATION MODAL
// --------------------------------------------------

function requestConfirmation(
  title: string,
  description: string,
  confirmLabel: string,
  run: () => Promise<void>,
): void {
  pendingConfirmAction.value = { title, description, confirmLabel, run }
}

async function confirmPendingAction(): Promise<void> {
  if (!pendingConfirmAction.value || isSubmitting.value) return
  const action = pendingConfirmAction.value.run
  pendingConfirmAction.value = null
  await action()
}

async function performAction(
  action: () => Promise<PPMPDetailApi>,
  successText: string,
): Promise<void> {
  if (!ppmpId.value || isSubmitting.value) {
    return
  }

  isSubmitting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    ppmp.value = await action()
    successMessage.value = successText
    await loadChangeHistory(ppmpId.value)
  } catch (error) {
    console.error('PPMP workflow action failed:', error)
    errorMessage.value =
      error instanceof Error ? error.message : 'The requested action could not be completed.'
  } finally {
    isSubmitting.value = false
  }
}

function submitPPMP(): void {
  if (!canEndUserSubmitToUnitHead.value || !ppmp.value) return

  requestConfirmation(
    'Submit PPMP for Unit Head Approval',
    `Submit ${ppmp.value.ppmpReference} (${formattedBudget.value}) to your Unit Head? Once submitted, changes must go through Procurement review.`,
    'Yes, Submit PPMP',
    () =>
      performAction(
        () => ppmpApiService.submit(ppmpId.value!),
        'PPMP successfully submitted for Unit Head approval.',
      ),
  )
}

function approvePPMP(): void {
  if (!canUnitHeadApprove.value || !ppmp.value) return

  requestConfirmation(
    'Approve & Digitally Sign PPMP',
    `Are you sure you want to approve and digitally sign ${ppmp.value.ppmpReference} (${formattedBudget.value})?`,
    'Approve & Sign',
    () =>
      performAction(
        () => ppmpApiService.approve(ppmpId.value!),
        'PPMP successfully approved by the Unit Head.',
      ),
  )
}

function startProcurementReview(): void {
  if (!canProcurementStartReview.value || !ppmp.value) return

  requestConfirmation(
    'Start Procurement Review',
    `Begin official Procurement review for ${ppmp.value.ppmpReference}?`,
    'Start Review',
    () =>
      performAction(
        () => ppmpApiService.startProcurementReview(ppmpId.value!),
        'Procurement review has started.',
      ),
  )
}

function completeProcurementReview(): void {
  if (!canProcurementCompleteReview.value || !ppmp.value) return

  requestConfirmation(
    'Complete Procurement Review',
    `Mark ${ppmp.value.ppmpReference} as Reviewed and ready for APP consolidation?`,
    'Mark as Reviewed',
    () =>
      performAction(
        () => ppmpApiService.completeReview(ppmpId.value!),
        'PPMP review successfully completed.',
      ),
  )
}

function editAsProcurement(): void {
  if (!canProcurementEdit.value || !ppmpId.value) return

  void router.push({
    name: 'procurement-edit-ppmp',
    params: { id: ppmpId.value },
  })
}

// --------------------------------------------------
// DISPLAY HELPERS
// --------------------------------------------------

function formatDate(value: string | null | undefined): string {
  if (!value) {
    return 'Not yet recorded'
  }

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return value
  }

  return new Intl.DateTimeFormat('en-PH', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  }).format(date)
}

function formatHistoricalBoolean(value: boolean | null): string {
  if (value === null) {
    return 'Not recorded'
  }

  return value ? 'Yes' : 'No'
}

function formatHistoricalText(value: string | null): string {
  return value?.trim() || 'Not recorded'
}
</script>

<template>
  <div class="ppmp-detail-shell space-y-6">
    <!-- PAGE HEADER (SCREEN ONLY) -->
    <div class="print:hidden flex flex-wrap items-start justify-between gap-4">
      <div>
        <RouterLink
          to="/ppmp"
          class="mb-3 inline-flex items-center gap-1.5 text-xs font-semibold text-slate-500 transition hover:text-brand-700"
        >
          <ArrowLeft :size="15" />
          Back to PPMP List
        </RouterLink>

        <h1 class="text-2xl font-bold tracking-tight text-slate-900">
          {{ ppmp ? ppmp.ppmpReference : 'PPMP Details' }}
        </h1>

        <p class="mt-1 text-sm text-slate-500">
          {{
            ppmp
              ? `${ppmp.departmentName} · Fiscal Year ${ppmp.planningYear}`
              : 'View and manage the details of this Project Procurement Management Plan.'
          }}
        </p>
      </div>

      <div class="flex flex-wrap items-center gap-2.5">
        <button
          v-if="ppmp"
          type="button"
          :disabled="isLoading || ppmp.items.length === 0"
          class="drmc-btn-primary"
          @click="printPPMP"
        >
          <Printer :size="16" />
          Print PPMP
        </button>

        <button
          type="button"
          :disabled="isLoading"
          class="drmc-btn-secondary"
          @click="loadPPMP"
        >
          <RefreshCw :size="16" :class="{ 'animate-spin': isLoading }" />
          Refresh
        </button>
      </div>
    </div>

    <!-- SUCCESS MESSAGE -->
    <div
      v-if="successMessage"
      role="status"
      class="print:hidden rounded-xl border border-brand-200 bg-brand-50 p-4 text-sm font-medium text-brand-800"
    >
      <div class="flex items-center gap-2">
        <CheckCircle :size="18" class="text-brand-600" />
        {{ successMessage }}
      </div>
    </div>

    <!-- ERROR MESSAGE -->
    <div
      v-if="errorMessage"
      role="alert"
      class="print:hidden rounded-xl border border-red-200 bg-red-50 p-4"
    >
      <p class="text-sm font-medium text-red-800">
        {{ errorMessage }}
      </p>

      <button
        type="button"
        class="mt-2 text-xs font-semibold text-red-700 hover:underline"
        @click="loadPPMP"
      >
        Try Again
      </button>
    </div>

    <!-- LOADING -->
    <div
      v-if="isLoading"
      class="print:hidden drmc-card p-14 text-center"
    >
      <RefreshCw
        :size="28"
        class="mx-auto mb-3 animate-spin text-brand-700"
      />
      <p class="text-sm font-medium text-slate-600">
        Loading PPMP details...
      </p>
    </div>

    <!-- NOT FOUND -->
    <div
      v-else-if="!ppmp"
      class="print:hidden drmc-card p-12 text-center"
    >
      <ClipboardList
        :size="36"
        class="mx-auto mb-3 text-slate-300"
      />

      <h2 class="text-lg font-semibold text-slate-800">
        PPMP Record Not Found
      </h2>

      <p class="mt-1 text-sm text-slate-500">
        This record may be unavailable or you may not have permission to view it.
      </p>

      <RouterLink
        to="/ppmp"
        class="mt-5 inline-block text-sm font-semibold text-brand-700 hover:underline"
      >
        Return to PPMP List
      </RouterLink>
    </div>

    <template v-else>
      <!-- WORKFLOW PROGRESS TRACKER (SCREEN ONLY) -->
      <section class="print:hidden drmc-card p-5">
        <div class="mb-3.5 flex flex-wrap items-center justify-between gap-2">
          <span class="text-xs font-semibold uppercase tracking-wider text-slate-400">
            Approval & Consolidation Progress
          </span>
          <span
            :class="[
              'inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-semibold ring-1 ring-inset',
              getPPMPStatusStyle(ppmp.status).pill,
            ]"
          >
            <span :class="['h-1.5 w-1.5 rounded-full', getPPMPStatusStyle(ppmp.status).dot]" />
            {{ ppmp.status }}
          </span>
        </div>

        <ol class="grid grid-cols-1 gap-2.5 sm:grid-cols-2 lg:grid-cols-6">
          <li
            v-for="(stage, index) in workflowStages"
            :key="stage.status"
            class="flex items-center gap-2.5 rounded-xl border px-3 py-2.5 text-xs font-semibold transition-all"
            :class="
              index <= currentStageIndex
                ? [stage.activePill, index === currentStageIndex ? stage.ring : 'opacity-95']
                : 'border-slate-200/80 bg-slate-50/70 text-slate-400'
            "
          >
            <span
              class="flex h-5 w-5 shrink-0 items-center justify-center rounded-full text-[11px] font-bold tabular-nums"
              :class="
                index <= currentStageIndex
                  ? stage.activeBadge
                  : 'bg-slate-200/80 text-slate-500'
              "
            >
              <Check v-if="index < currentStageIndex" :size="12" stroke-width="3" />
              <span v-else>{{ index + 1 }}</span>
            </span>

            <span class="leading-tight">{{ stage.status }}</span>
          </li>
        </ol>
      </section>

      <!-- PROMINENT WORKFLOW ACTIONS (SCREEN ONLY) -->
      <section
        v-if="hasAvailableActions"
        class="print:hidden drmc-card border-brand-200/80 bg-brand-50/25 p-5"
      >
        <div class="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
          <div>
            <h2 class="text-base font-semibold text-slate-900">
              Available Actions
            </h2>
            <p class="mt-0.5 text-sm text-slate-600">
              Next step for your role on this PPMP.
            </p>
          </div>

          <div class="flex flex-wrap gap-2.5">
            <button
              v-if="canEndUserSubmitToUnitHead"
              type="button"
              :disabled="isSubmitting"
              class="drmc-btn-primary"
              @click="submitPPMP"
            >
              <CheckCircle :size="17" />
              Submit for Unit Head Approval
            </button>

            <button
              v-if="canUnitHeadApprove"
              type="button"
              :disabled="isSubmitting"
              class="drmc-btn-primary"
              @click="approvePPMP"
            >
              <CheckCircle :size="17" />
              Approve
            </button>

            <button
              v-if="canProcurementStartReview"
              type="button"
              :disabled="isSubmitting"
              class="inline-flex items-center gap-2 rounded-xl bg-blue-700 px-4 py-2.5 text-sm font-semibold text-white shadow-xs transition hover:bg-blue-800 disabled:cursor-not-allowed disabled:opacity-50"
              @click="startProcurementReview"
            >
              <ClipboardList :size="17" />
              Start Review
            </button>

            <button
              v-if="canProcurementEdit"
              type="button"
              class="inline-flex items-center gap-2 rounded-xl bg-blue-700 px-4 py-2.5 text-sm font-semibold text-white shadow-xs transition hover:bg-blue-800"
              @click="editAsProcurement"
            >
              <Pencil :size="17" />
              Edit PPMP
            </button>

            <button
              v-if="canProcurementCompleteReview"
              type="button"
              :disabled="isSubmitting"
              class="drmc-btn-primary"
              @click="completeProcurementReview"
            >
              <CheckCircle :size="17" />
              Complete Review
            </button>
          </div>
        </div>

        <p
          v-if="isSubmitting"
          class="mt-3 text-xs font-medium text-slate-500"
        >
          Processing your request...
        </p>
      </section>

      <!-- GENERAL INFORMATION (SCREEN ONLY) -->
      <section class="print:hidden drmc-card p-6">
        <div class="mb-5 flex items-center gap-2">
          <ClipboardList :size="19" class="text-brand-700" />
          <h2 class="text-base font-semibold text-slate-900">
            General Information
          </h2>
        </div>

        <div class="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          <div>
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              PPMP Reference
            </p>
            <p class="mt-1 font-semibold text-brand-700">
              {{ ppmp.ppmpReference }}
            </p>
          </div>

          <div>
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Unit
            </p>
            <p class="mt-1 font-semibold text-slate-900">
              {{ ppmp.departmentName }}
            </p>
          </div>

          <div>
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Planning Year
            </p>
            <p class="mt-1 font-semibold text-slate-900 tabular-nums">
              FY {{ ppmp.planningYear }}
            </p>
          </div>

          <div>
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Date Created
            </p>
            <p class="mt-1 text-sm font-medium text-slate-800">
              {{ formatDate(ppmp.createdAt) }}
            </p>
          </div>

          <div>
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Date Submitted
            </p>
            <p class="mt-1 text-sm font-medium text-slate-800">
              {{ formatDate(ppmp.submittedAt) }}
            </p>
          </div>

          <div>
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Status
            </p>
            <div class="mt-1">
              <span
                :class="[
                  'inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-medium ring-1 ring-inset',
                  getPPMPStatusStyle(ppmp.status).pill,
                ]"
              >
                <span :class="['h-1.5 w-1.5 rounded-full', getPPMPStatusStyle(ppmp.status).dot]" />
                {{ ppmp.status }}
              </span>
            </div>
          </div>

          <div>
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Total Items
            </p>
            <p class="mt-1 font-semibold text-slate-900 tabular-nums">
              {{ ppmp.items.length }}
            </p>
          </div>

          <div>
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Estimated Budget
            </p>
            <p class="mt-1 font-bold text-brand-700 tabular-nums">
              {{ formattedBudget }}
            </p>
          </div>

          <div>
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Last Updated
            </p>
            <p class="mt-1 text-sm font-medium text-slate-800">
              {{ formatDate(ppmp.updatedAt) }}
            </p>
          </div>
        </div>
      </section>

      <!-- DRAFT NOTICE (SCREEN ONLY) -->
      <section
        v-if="isEndUser && isDraft"
        class="print:hidden rounded-xl border border-amber-200 bg-amber-50/80 p-4"
      >
        <p class="text-sm font-semibold text-amber-900">
          Draft PPMP
        </p>
        <p class="mt-1 text-sm text-amber-800">
          This PPMP has not yet been submitted. You can submit it for Unit Head approval when it is ready.
        </p>
      </section>

      <!-- SUBMISSION INFORMATION (SCREEN ONLY) -->
      <section
        v-if="ppmp.submittedAt"
        class="print:hidden drmc-card p-6"
      >
        <div class="flex items-center gap-2">
          <Clock :size="19" class="text-blue-700" />
          <h2 class="text-base font-semibold text-slate-900">
            Submission Information
          </h2>
        </div>

        <p class="mt-3 text-sm text-slate-600">
          This PPMP was submitted on
          <strong class="text-slate-900">{{ formatDate(ppmp.submittedAt) }}</strong>.
          End-users submit once; additional items or changes must be requested through Procurement during review.
        </p>
      </section>

      <!-- REQUESTED ITEMS: SPREADSHEET VIEW (SCREEN ONLY) -->
      <section class="print:hidden drmc-card p-6">
        <div class="mb-5 flex flex-wrap items-center justify-between gap-3">
          <div class="flex items-center gap-2.5">
            <Package :size="19" class="text-brand-700" />

            <div>
              <h2 class="text-base font-semibold text-slate-900">
                Requested Items
              </h2>
              <p class="mt-0.5 text-xs text-slate-500">
                Spreadsheet-style view of the submitted PPMP items.
              </p>
            </div>
          </div>

          <div class="flex flex-wrap items-center gap-2">
            <button
              v-if="ppmp.items.length > 0"
              type="button"
              class="drmc-btn-secondary px-3 py-1.5 text-xs"
              @click="toggleAllItems"
            >
              <ChevronsUpDown :size="14" />
              {{
                expandedItems.length === ppmp.items.length
                  ? 'Collapse All Sub-Units'
                  : 'Expand All Sub-Units'
              }}
            </button>

            <span class="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-700 tabular-nums">
              {{ ppmp.items.length }} item(s)
            </span>
          </div>
        </div>

        <div
          v-if="groupedItems.length > 0"
          class="space-y-6"
        >
          <!-- CATEGORY GROUP -->
          <div
            v-for="group in groupedItems"
            :key="group.categoryName"
            class="overflow-hidden rounded-xl border border-slate-200/90"
          >
            <!-- CATEGORY HEADER -->
            <div class="flex flex-wrap items-center justify-between gap-3 border-b border-slate-200/80 bg-slate-50/80 px-4 py-3">
              <div>
                <h3 class="text-sm font-semibold text-slate-900">
                  {{ group.categoryName }}
                </h3>
                <p class="mt-0.5 text-xs text-slate-500 tabular-nums">
                  {{ group.items.length }} item(s)
                </p>
              </div>

              <p class="text-sm font-bold text-brand-700 tabular-nums">
                {{ formatPHP(group.totalBudget) }}
              </p>
            </div>

            <!-- SPREADSHEET TABLE -->
            <div class="overflow-x-auto">
              <table class="w-full min-w-[850px] border-collapse text-left text-sm">
                <thead class="bg-slate-100/80 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
                  <tr>
                    <th class="w-[38%] border-b border-slate-200 px-4 py-3">
                      Item Description
                    </th>
                    <th class="border-b border-slate-200 px-4 py-3">
                      Unit
                    </th>
                    <th class="border-b border-slate-200 px-4 py-3 text-right">
                      ABC / Unit
                    </th>
                    <th class="border-b border-slate-200 px-4 py-3 text-right">
                      Quantity
                    </th>
                    <th class="border-b border-slate-200 px-4 py-3 text-right">
                      Estimated Total
                    </th>
                    <th class="w-28 border-b border-slate-200 px-4 py-3 text-center">
                      Sub-Units
                    </th>
                  </tr>
                </thead>

                <tbody class="divide-y divide-slate-100">
                  <template
                    v-for="item in group.items"
                    :key="item.ppmpItemId"
                  >
                    <!-- ITEM ROW -->
                    <tr class="transition-colors hover:bg-slate-50/80">
                      <td class="px-4 py-3.5">
                        <p class="font-semibold text-slate-900">
                          {{ item.itemDescription }}
                        </p>

                        <div class="mt-1 flex flex-wrap gap-x-3 gap-y-1 text-xs text-slate-500">
                          <span class="font-mono">
                            Code: {{ item.itemCode || '—' }}
                          </span>

                          <span v-if="item.itemType">
                            Type: {{ item.itemType }}
                          </span>
                        </div>

                        <p
                          v-if="item.specifications"
                          class="mt-1 whitespace-pre-wrap text-xs text-slate-500"
                        >
                          {{ item.specifications }}
                        </p>
                      </td>

                      <td class="px-4 py-3.5 text-slate-700">
                        {{ item.unit || '—' }}
                      </td>

                      <td class="px-4 py-3.5 text-right tabular-nums text-slate-700">
                        {{ formatPHP(item.unitPrice) }}
                      </td>

                      <td class="px-4 py-3.5 text-right font-semibold tabular-nums text-slate-900">
                        {{ item.quantity }}
                      </td>

                      <td class="px-4 py-3.5 text-right font-bold tabular-nums text-brand-700">
                        {{ formatPHP(item.estimatedBudget) }}
                      </td>

                      <td class="px-4 py-3.5 text-center">
                        <button
                          type="button"
                          :aria-expanded="isItemExpanded(item.ppmpItemId)"
                          class="inline-flex items-center gap-1 rounded-lg border border-slate-200 px-2.5 py-1.5 text-xs font-semibold text-slate-600 transition hover:border-brand-300 hover:bg-brand-50 hover:text-brand-700"
                          @click="toggleItem(item.ppmpItemId)"
                        >
                          <ChevronDown
                            :size="15"
                            :class="[
                              'transition-transform',
                              isItemExpanded(item.ppmpItemId) ? 'rotate-180' : '',
                            ]"
                          />
                          {{ isItemExpanded(item.ppmpItemId) ? 'Hide' : `View (${item.sectionAllocations.length})` }}
                        </button>
                      </td>
                    </tr>

                    <!-- EXPANDED SECTION ALLOCATIONS -->
                    <tr
                      v-if="isItemExpanded(item.ppmpItemId)"
                      class="bg-slate-50/70"
                    >
                      <td colspan="6" class="px-4 py-4">
                        <div class="rounded-xl border border-slate-200 bg-white p-4">
                          <div class="mb-3 flex flex-wrap items-center justify-between gap-2">
                            <div>
                              <h4 class="text-sm font-semibold text-slate-900">
                                Sub-Unit Quantity Allocation
                              </h4>
                              <p class="mt-0.5 text-xs text-slate-500">
                                {{ item.itemDescription }}
                              </p>
                            </div>

                            <span class="text-xs font-medium text-slate-400">
                              Read-only
                            </span>
                          </div>

                          <div
                            v-if="item.sectionAllocations.length > 0"
                            class="overflow-x-auto"
                          >
                            <table class="w-full min-w-[500px] text-left text-sm">
                              <thead class="bg-slate-50 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
                                <tr>
                                  <th class="px-3 py-2">Sub-Unit Code</th>
                                  <th class="px-3 py-2">Sub-Unit</th>
                                  <th class="px-3 py-2 text-right">Allocated Quantity</th>
                                </tr>
                              </thead>

                              <tbody class="divide-y divide-slate-100">
                                <tr
                                  v-for="allocation in item.sectionAllocations"
                                  :key="allocation.allocationId"
                                >
                                  <td class="px-3 py-2.5 font-mono text-xs text-slate-600">
                                    {{ allocation.sectionCode }}
                                  </td>
                                  <td class="px-3 py-2.5 font-medium text-slate-800">
                                    {{ allocation.sectionName }}
                                  </td>
                                  <td class="px-3 py-2.5 text-right font-semibold tabular-nums text-slate-900">
                                    {{ allocation.quantity }}
                                  </td>
                                </tr>
                              </tbody>

                              <tfoot class="border-t border-slate-200 bg-slate-50/80">
                                <tr>
                                  <td
                                    colspan="2"
                                    class="px-3 py-2.5 text-right text-xs font-semibold text-slate-600"
                                  >
                                    Total Allocated Quantity
                                  </td>
                                  <td class="px-3 py-2.5 text-right font-bold tabular-nums text-brand-700">
                                    {{ getItemAllocationTotal(item) }}
                                  </td>
                                </tr>
                              </tfoot>
                            </table>
                          </div>

                          <p
                            v-else
                            class="rounded-lg bg-slate-50 p-4 text-sm text-slate-500"
                          >
                            No sub-unit allocations recorded for this item.
                          </p>
                        </div>
                      </td>
                    </tr>
                  </template>
                </tbody>

                <!-- CATEGORY TOTAL -->
                <tfoot class="border-t-2 border-slate-200 bg-slate-50/90">
                  <tr>
                    <td
                      colspan="3"
                      class="px-4 py-3 text-right text-xs font-bold uppercase tracking-wider text-slate-600"
                    >
                      Category Total
                    </td>
                    <td class="px-4 py-3 text-right font-semibold tabular-nums text-slate-800">
                      {{ group.totalQuantity }}
                    </td>
                    <td class="px-4 py-3 text-right font-extrabold tabular-nums text-brand-700">
                      {{ formatPHP(group.totalBudget) }}
                    </td>
                    <td class="px-4 py-3"></td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
        </div>

        <!-- EMPTY ITEMS -->
        <div
          v-else
          class="rounded-xl bg-slate-50 p-8 text-center"
        >
          <Package
            :size="30"
            class="mx-auto mb-3 text-slate-400"
          />
          <p class="text-sm text-slate-500">
            No requested items are recorded in this PPMP.
          </p>
        </div>

        <!-- TOTAL BUDGET -->
        <div class="mt-5 flex justify-end border-t border-slate-100 pt-5">
          <div class="text-right">
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Total Estimated Budget
            </p>
            <p class="mt-1 text-xl font-extrabold text-brand-700 tabular-nums">
              {{ formattedBudget }}
            </p>
          </div>
        </div>
      </section>

      <!-- PROCUREMENT CHANGE HISTORY (SCREEN ONLY) -->
      <section
        v-if="shouldShowChangeHistory"
        class="print:hidden drmc-card p-6"
      >
        <div class="mb-5 flex flex-wrap items-center justify-between gap-3">
          <div class="flex items-center gap-2.5">
            <ClipboardList
              :size="21"
              class="text-brand-700"
            />
            <div>
              <h2 class="text-base font-semibold text-slate-900">
                Procurement Change History
              </h2>
              <p class="mt-0.5 text-xs text-slate-500">
                Recorded changes made during Procurement review.
              </p>
            </div>
          </div>

          <span class="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-600 tabular-nums">
            {{ changeHistory.length }} record(s)
          </span>
        </div>

        <!-- HISTORY LOADING -->
        <div
          v-if="isLoadingChangeHistory"
          class="py-8 text-center"
        >
          <RefreshCw
            :size="24"
            class="mx-auto mb-2 animate-spin text-brand-700"
          />
          <p class="text-sm text-slate-500">
            Loading change history...
          </p>
        </div>

        <!-- HISTORY ERROR -->
        <div
          v-else-if="changeHistoryError"
          class="rounded-xl border border-red-200 bg-red-50 p-4"
        >
          <p class="text-sm text-red-700">
            {{ changeHistoryError }}
          </p>
          <button
            type="button"
            class="mt-2 text-xs font-semibold text-red-700 hover:underline"
            @click="ppmpId && loadChangeHistory(ppmpId)"
          >
            Retry Loading History
          </button>
        </div>

        <!-- EMPTY HISTORY -->
        <div
          v-else-if="changeHistory.length === 0"
          class="rounded-xl bg-slate-50 p-8 text-center"
        >
          <Clock
            :size="30"
            class="mx-auto mb-3 text-slate-400"
          />
          <p class="text-sm font-semibold text-slate-700">
            No procurement changes recorded.
          </p>
          <p class="mt-1 text-xs text-slate-500">
            Changes will appear here when Procurement Staff updates this PPMP.
          </p>
        </div>

        <!-- HISTORY RECORDS -->
        <div
          v-else
          class="space-y-6"
        >
          <article
            v-for="history in changeHistory"
            :key="history.changeHistoryId"
            class="overflow-hidden rounded-xl border border-slate-200"
          >
            <!-- HISTORY HEADER -->
            <div class="border-b border-slate-200 bg-slate-50/80 p-4">
              <div class="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <p class="text-sm font-semibold text-slate-900">
                    {{ history.changedByName }}
                  </p>
                  <p class="mt-0.5 text-xs text-slate-500">
                    {{ formatDate(history.changedAt) }}
                  </p>
                </div>

                <span class="rounded-full bg-purple-50 px-3 py-1 text-xs font-semibold text-purple-700 ring-1 ring-inset ring-purple-600/20">
                  Change #{{ history.changeHistoryId }}
                </span>
              </div>

              <div class="mt-4">
                <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
                  Reason for Changes
                </p>
                <p class="mt-1 whitespace-pre-wrap text-sm text-slate-800">
                  {{ history.reason }}
                </p>
              </div>
            </div>

            <!-- BUDGET COMPARISON -->
            <div class="grid gap-4 border-b border-slate-100 p-4 sm:grid-cols-2">
              <div>
                <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
                  Previous Budget
                </p>
                <p class="mt-1 text-lg font-semibold text-slate-700 tabular-nums">
                  {{ formatPHP(history.previousBudget) }}
                </p>
              </div>

              <div>
                <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
                  Updated Budget
                </p>
                <p class="mt-1 text-lg font-bold text-brand-700 tabular-nums">
                  {{ formatPHP(history.updatedBudget) }}
                </p>
              </div>
            </div>

            <!-- ITEM CHANGES -->
            <div class="p-4">
              <h3 class="mb-4 text-sm font-semibold text-slate-800">
                Item Changes
              </h3>

              <div
                v-if="history.items.length > 0"
                class="space-y-4"
              >
                <div
                  v-for="item in history.items"
                  :key="item.changeHistoryItemId"
                  class="rounded-xl border border-slate-200 p-4"
                >
                  <div class="mb-3 flex flex-wrap items-center justify-between gap-2">
                    <p class="text-sm font-semibold text-slate-900">
                      {{
                        !item.previousDescription
                          ? 'Added Item'
                          : !item.updatedDescription
                            ? 'Removed Item'
                            : 'Updated Item'
                      }}
                    </p>

                    <span
                      v-if="!item.previousDescription"
                      class="rounded-full bg-brand-50 px-2.5 py-0.5 text-xs font-semibold text-brand-700"
                    >
                      Added
                    </span>

                    <span
                      v-else-if="!item.updatedDescription"
                      class="rounded-full bg-red-50 px-2.5 py-0.5 text-xs font-semibold text-red-700"
                    >
                      Removed
                    </span>

                    <span
                      v-else
                      class="rounded-full bg-blue-50 px-2.5 py-0.5 text-xs font-semibold text-blue-700"
                    >
                      Modified
                    </span>
                  </div>

                  <div class="grid gap-4 sm:grid-cols-2">
                    <!-- PREVIOUS DETAILS -->
                    <div class="rounded-lg bg-slate-50 p-3.5">
                      <p class="mb-2 text-[11px] font-semibold uppercase tracking-wider text-slate-400">
                        Previous Details
                      </p>

                      <template v-if="item.previousDescription">
                        <p class="text-sm font-semibold text-slate-900">
                          {{ item.previousDescription }}
                        </p>

                        <p class="mt-1 font-mono text-xs text-slate-500">
                          Code: {{ item.previousItemCode || '—' }}
                        </p>

                        <div class="mt-3 space-y-2 border-t border-slate-200 pt-3 text-xs text-slate-600">
                          <div>
                            <p class="font-semibold text-slate-500">
                              General Description and Objective
                            </p>
                            <p class="mt-1 whitespace-pre-wrap">
                              {{ formatHistoricalText(item.previousGeneralDescriptionAndObjective) }}
                            </p>
                          </div>

                          <p>
                            <span class="font-semibold text-slate-500">Project Type:</span>
                            {{ formatHistoricalText(item.previousProjectType) }}
                          </p>

                          <p>
                            <span class="font-semibold text-slate-500">
                              Pre-Procurement Conference:
                            </span>
                            {{ formatHistoricalBoolean(item.previousPreProcurementConference) }}
                          </p>
                        </div>

                        <div class="mt-3 space-y-1 text-xs text-slate-600 tabular-nums">
                          <p>Quantity: {{ item.previousQuantity }}</p>
                          <p>Unit: {{ item.previousUnit || '—' }}</p>
                          <p>Unit Price: {{ formatPHP(item.previousUnitPrice ?? 0) }}</p>
                          <p>Estimated Budget: {{ formatPHP(item.previousEstimatedBudget ?? 0) }}</p>
                        </div>
                      </template>

                      <p
                        v-else
                        class="text-sm italic text-slate-400"
                      >
                        Item did not previously exist.
                      </p>
                    </div>

                    <!-- UPDATED DETAILS -->
                    <div class="rounded-lg bg-brand-50/40 p-3.5">
                      <p class="mb-2 text-[11px] font-semibold uppercase tracking-wider text-brand-700">
                        Updated Details
                      </p>

                      <template v-if="item.updatedDescription">
                        <p class="text-sm font-semibold text-slate-900">
                          {{ item.updatedDescription }}
                        </p>

                        <p class="mt-1 font-mono text-xs text-slate-500">
                          Code: {{ item.updatedItemCode || '—' }}
                        </p>

                        <div class="mt-3 space-y-2 border-t border-brand-100 pt-3 text-xs text-slate-600">
                          <div>
                            <p class="font-semibold text-slate-500">
                              General Description and Objective
                            </p>
                            <p class="mt-1 whitespace-pre-wrap">
                              {{ formatHistoricalText(item.updatedGeneralDescriptionAndObjective) }}
                            </p>
                          </div>

                          <p>
                            <span class="font-semibold text-slate-500">Project Type:</span>
                            {{ formatHistoricalText(item.updatedProjectType) }}
                          </p>

                          <p>
                            <span class="font-semibold text-slate-500">
                              Pre-Procurement Conference:
                            </span>
                            {{ formatHistoricalBoolean(item.updatedPreProcurementConference) }}
                          </p>
                        </div>

                        <div class="mt-3 space-y-1 text-xs text-slate-600 tabular-nums">
                          <p>Quantity: {{ item.updatedQuantity }}</p>
                          <p>Unit: {{ item.updatedUnit || '—' }}</p>
                          <p>Unit Price: {{ formatPHP(item.updatedUnitPrice ?? 0) }}</p>
                          <p>Estimated Budget: {{ formatPHP(item.updatedEstimatedBudget ?? 0) }}</p>
                        </div>
                      </template>

                      <p
                        v-else
                        class="text-sm italic text-slate-400"
                      >
                        Item was removed.
                      </p>
                    </div>
                  </div>

                  <!-- SECTION ALLOCATION CHANGES -->
                  <div
                    v-if="item.sectionAllocations.length > 0"
                    class="mt-4 border-t border-slate-100 pt-3"
                  >
                    <h4 class="mb-3 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
                      Sub-Unit Allocation Changes
                    </h4>

                    <div class="space-y-2">
                      <div
                        v-for="allocation in item.sectionAllocations"
                        :key="allocation.changeHistorySectionAllocationId"
                        class="grid gap-3 rounded-lg bg-slate-50 p-3 text-xs sm:grid-cols-2"
                      >
                        <div>
                          <p class="font-semibold text-slate-500">
                            Previous Allocation
                          </p>
                          <p class="mt-1 text-slate-800">
                            {{ allocation.previousSectionName || 'No previous allocation' }}
                          </p>
                          <p
                            v-if="allocation.previousSectionName"
                            class="mt-1 text-slate-500 tabular-nums"
                          >
                            {{ allocation.previousSectionCode }} · Quantity: {{ allocation.previousQuantity }}
                          </p>
                        </div>

                        <div>
                          <p class="font-semibold text-brand-700">
                            Updated Allocation
                          </p>
                          <p class="mt-1 text-slate-800">
                            {{ allocation.updatedSectionName || 'Allocation removed' }}
                          </p>
                          <p
                            v-if="allocation.updatedSectionName"
                            class="mt-1 text-slate-500 tabular-nums"
                          >
                            {{ allocation.updatedSectionCode }} · Quantity: {{ allocation.updatedQuantity }}
                          </p>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <p
                v-else
                class="text-sm text-slate-500"
              >
                No individual item changes were recorded.
              </p>
            </div>
          </article>
        </div>
      </section>

      <!-- ADMINISTRATOR INFORMATION (SCREEN ONLY) -->
      <section
        v-if="isSystemAdministrator"
        class="print:hidden rounded-xl border border-slate-200 bg-slate-50 p-4"
      >
        <p class="text-sm font-semibold text-slate-800">
          System Administrator View
        </p>
        <p class="mt-1 text-xs text-slate-600">
          You are viewing this record with administrator access.
          Workflow actions remain restricted to their assigned roles.
        </p>
      </section>

      <!-- ================================================================ -->
      <!-- OFFICIAL DRMC EXCEL PRINT TEMPLATE (HOP-QR-PRO-001) - PRINT ONLY -->
      <!-- ================================================================ -->
      <section
        v-for="group in groupedItems"
        :key="`print-doc-${group.categoryName}`"
        class="ppmp-document hidden bg-white print:block"
      >
        <!-- TOP HEADER + DOCUMENT CONTROL BOX -->
        <div class="ppmp-header">
          <div class="ppmp-header__center">
            <div class="ppmp-header__republic">Republic of the Philippines</div>
            <div class="ppmp-header__dept">Department of Health</div>
            <div class="ppmp-header__agency">DAVAO REGIONAL MEDICAL CENTER</div>
            <div class="ppmp-header__title">
              PROJECT PROCUREMENT MANAGEMENT PLAN (PPMP) NO.
              <span class="ppmp-header__underline">{{ ppmp.ppmpReference || '______' }}</span>
            </div>

            <div class="ppmp-header__status-row">
              <span class="ppmp-status-option">
                <span class="ppmp-checkbox">{{ !isFinalPPMP ? '✓' : '&nbsp;' }}</span> INDICATIVE
              </span>
              <span class="ppmp-status-option">
                <span class="ppmp-checkbox">{{ isFinalPPMP ? '✓' : '&nbsp;' }}</span> FINAL
              </span>
            </div>
          </div>

          <!-- Top-Right Reference Box (Matches N1:N9 in Excel) -->
          <table class="ppmp-doc-control">
            <tbody>
              <tr>
                <td>
                  <div class="ppmp-doc-control__label">Reference Code:</div>
                  <div class="ppmp-doc-control__value">HOP-QR-PRO-001</div>
                </td>
              </tr>
              <tr>
                <td>
                  <div class="ppmp-doc-control__label">Revision No.:</div>
                  <div class="ppmp-doc-control__value">0</div>
                </td>
              </tr>
              <tr>
                <td>
                  <div class="ppmp-doc-control__label">Effective Date:</div>
                  <div class="ppmp-doc-control__value">January 1, 2026</div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- FISCAL YEAR & END-USER UNIT (Rows 14-15 in Excel) -->
        <div class="ppmp-meta-lines">
          <div class="ppmp-meta-row">
            <span class="ppmp-meta-label">Fiscal Year :</span>
            <span class="ppmp-meta-value">{{ ppmp.planningYear || '' }}</span>
          </div>
          <div class="ppmp-meta-row">
            <span class="ppmp-meta-label">End-User or Implementing Unit:</span>
            <span class="ppmp-meta-value ppmp-meta-value--wide">
              {{ ppmp.departmentName || '' }}
            </span>
          </div>
        </div>

        <!-- MAIN 14-COLUMN TABLE (Matches Rows 18-21 in Excel) -->
        <div class="ppmp-table-wrap">
          <table class="ppmp-table">
            <colgroup>
              <col style="width: 19%" /><!-- Col A: General Description -->
              <col style="width: 7%" /><!-- Col B: Type -->
              <col style="width: 5%" /><!-- Col C: Quantity -->
              <col style="width: 5%" /><!-- Col D: Unit of Issue -->
              <col style="width: 7%" /><!-- Col E: ABC/unit -->
              <col style="width: 8%" /><!-- Col F: Mode -->
              <col style="width: 5%" /><!-- Col G: Pre-Proc -->
              <col style="width: 6%" /><!-- Col H: Start -->
              <col style="width: 6%" /><!-- Col I: End -->
              <col style="width: 7%" /><!-- Col J: Delivery -->
              <col style="width: 5%" /><!-- Col K: Source of Funds -->
              <col style="width: 8%" /><!-- Col L: Estimated Budget -->
              <col style="width: 6%" /><!-- Col M: Supporting Docs -->
              <col style="width: 6%" /><!-- Col N: Remarks -->
            </colgroup>

            <thead>
              <tr class="ppmp-head-group">
                <th colspan="7">PROCUREMENT PROJECT DETAILS</th>
                <th colspan="3">PROJECTED TIMELINE (MM/YYYY)</th>
                <th colspan="2">FUNDING DETAILS</th>
                <th rowspan="2">ATTACHED SUPPORTING DOCUMENTS</th>
                <th rowspan="2">REMARKS</th>
              </tr>

              <tr class="ppmp-head-col">
                <th>General Description and Objective of the Project to be Procured</th>
                <th>Type of the Project to be Procured<br />(Goods, Infrastructure, Consulting)</th>
                <th colspan="3">Quantity and Size of the Project to be Procured</th>
                <th>Recommended Mode of Procurement</th>
                <th>Pre-Procurement Conference, if applicable (Yes/No)</th>
                <th>Start of Procurement Activity</th>
                <th>End of Procurement Activity</th>
                <th>Expected Delivery / Implementation Period</th>
                <th>Source of Funds</th>
                <th>Estimated Budget / Authorized Budgetary Allocation (PhP)</th>
              </tr>

              <tr class="ppmp-head-cols-num">
                <th rowspan="2">Column 1</th>
                <th rowspan="2">Column 2</th>
                <th colspan="3">Column 3</th>
                <th rowspan="2">Column 4</th>
                <th rowspan="2">Column 5</th>
                <th rowspan="2">Column 6</th>
                <th rowspan="2">Column 7</th>
                <th rowspan="2">Column 8</th>
                <th rowspan="2">Column 9</th>
                <th rowspan="2">Column 10</th>
                <th rowspan="2">Column 11</th>
                <th rowspan="2">Column 12</th>
              </tr>

              <tr class="ppmp-head-sub">
                <th>Quantity</th>
                <th>Unit of Issue</th>
                <th>ABC/unit</th>
              </tr>
            </thead>

            <tbody>
              <tr
                v-for="item in group.items"
                :key="`print-item-${item.ppmpItemId}`"
                class="ppmp-row"
              >
                <td class="ppmp-cell ppmp-cell--desc">
                  <div class="ppmp-item-name">
                    {{
                      getPrintField(item, 'generalDescriptionAndObjective') ||
                      item.itemDescription
                    }}
                  </div>
                  <div v-if="item.specifications" class="ppmp-item-spec">
                    {{ item.specifications }}
                  </div>
                  <div v-if="item.sectionAllocations?.length" class="ppmp-item-spec">
                    Sub-Unit allocations:
                    <span
                      v-for="(allocation, allocationIndex) in item.sectionAllocations"
                      :key="allocation.allocationId"
                    >
                      {{ allocation.sectionName }} ({{ allocation.quantity }})<span
                        v-if="allocationIndex < item.sectionAllocations.length - 1"
                        >,
                      </span>
                    </span>
                  </div>
                </td>

                <td class="ppmp-cell ppmp-cell--center">
                  {{ getPrintField(item, 'projectType') || item.itemType || 'Goods' }}
                </td>

                <td class="ppmp-cell ppmp-cell--center">
                  {{ item.quantity }}
                </td>

                <td class="ppmp-cell ppmp-cell--center">
                  {{ item.unit || '—' }}
                </td>

                <td class="ppmp-cell ppmp-cell--right">
                  {{ formatPHP(item.unitPrice) }}
                </td>

                <td class="ppmp-cell ppmp-cell--center">
                  {{ getPrintField(item, 'recommendedModeOfProcurement') }}
                </td>

                <td class="ppmp-cell ppmp-cell--center">
                  {{ getPrintPreProc(item) }}
                </td>

                <td class="ppmp-cell ppmp-cell--center">
                  {{ formatMonth(getPrintField(item, 'startOfProcurementActivity')) }}
                </td>

                <td class="ppmp-cell ppmp-cell--center">
                  {{ formatMonth(getPrintField(item, 'endOfProcurementActivity')) }}
                </td>

                <td class="ppmp-cell ppmp-cell--center">
                  {{ getPrintField(item, 'expectedDeliveryImplementationPeriod') }}
                </td>

                <td class="ppmp-cell ppmp-cell--center">
                  {{ getPrintField(item, 'sourceOfFunds') || 'GAA' }}
                </td>

                <td class="ppmp-cell ppmp-cell--right ppmp-cell--budget">
                  {{ formatPHP(item.estimatedBudget) }}
                </td>

                <td class="ppmp-cell ppmp-cell--center">
                  {{ getPrintField(item, 'attachedSupportingDocuments') }}
                </td>

                <td class="ppmp-cell ppmp-cell--center">
                  {{ group.categoryName }}
                </td>
              </tr>

              <!-- Spacer + TOTAL BUDGET Row (Row 26 in Excel) -->
              <tr class="ppmp-spacer-row">
                <td colspan="14">&nbsp;</td>
              </tr>
              <tr class="ppmp-total-row">
                <td colspan="11" class="ppmp-total-label">TOTAL BUDGET:</td>
                <td class="ppmp-total-value">
                  {{ formatPHP(group.totalBudget) }}
                </td>
                <td colspan="2"></td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- SIGNATURES (Matches Rows 29-36 in Excel) -->
        <div class="ppmp-signatures">
          <div class="ppmp-signature-col">
            <div class="ppmp-signature-label">Prepared by:</div>
            <div class="ppmp-signature-name">{{ preparedByName }}</div>
            <div class="ppmp-signature-line"></div>
            <div class="ppmp-signature-role">Signature over Printed Name</div>
            <div class="ppmp-signature-line ppmp-signature-line--spaced"></div>
            <div class="ppmp-signature-role">Designation</div>
            <div class="ppmp-signature-role">{{ preparedByPosition }}</div>
            <div class="ppmp-signature-role">
              [{{ ppmp.departmentName || 'End-User or Implementing Unit' }}]
            </div>
            <div class="ppmp-signature-date">Date : _________________</div>
          </div>

          <div class="ppmp-signature-col">
            <div class="ppmp-signature-label">Submitted by:</div>
            <div class="ppmp-signature-name">&nbsp;</div>
            <div class="ppmp-signature-line"></div>
            <div class="ppmp-signature-role">Signature over Printed Name</div>
            <div class="ppmp-signature-line ppmp-signature-line--spaced"></div>
            <div class="ppmp-signature-role">Designation</div>
            <div class="ppmp-signature-role">[Head of the End-User or Implementing Unit]</div>
            <div class="ppmp-signature-date">Date : ____________</div>
          </div>
        </div>
      </section>
    </template>
  </div>

  <!-- WORKFLOW CONFIRMATION MODAL -->
  <Teleport to="body">
    <div
      v-if="pendingConfirmAction"
      class="print:hidden fixed inset-0 z-50 flex items-center justify-center bg-slate-900/55 p-4 backdrop-blur-2xs"
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

<style scoped>
.ppmp-document {
  font-family: Arial, Helvetica, sans-serif;
  color: #000;
  background: #fff;
}

/* HEADER */
.ppmp-header {
  display: grid;
  grid-template-columns: 150px 1fr 150px;
  align-items: start;
  padding-top: 4px;
}

.ppmp-header__center {
  grid-column: 2;
  text-align: center;
  padding-top: 8px;
}

.ppmp-header__republic {
  font-size: 10pt;
  line-height: 1.25;
}

.ppmp-header__dept {
  font-size: 10pt;
  line-height: 1.25;
}

.ppmp-header__agency {
  font-size: 12pt;
  font-weight: 700;
  letter-spacing: 0.02em;
  line-height: 1.3;
}

.ppmp-header__title {
  margin-top: 8px;
  font-size: 10.5pt;
  font-weight: 700;
}

.ppmp-header__underline {
  display: inline-block;
  min-width: 48px;
  border-bottom: 1px solid #000;
  padding: 0 4px;
}

.ppmp-header__status-row {
  margin-top: 10px;
  display: flex;
  justify-content: center;
  gap: 36px;
  font-size: 9.5pt;
  font-weight: 700;
}

.ppmp-status-option {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.ppmp-checkbox {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 13px;
  height: 13px;
  border: 1px solid #000;
  font-size: 9pt;
  line-height: 1;
}

/* TOP-RIGHT DOCUMENT CONTROL BOX (N1:N9) */
.ppmp-doc-control {
  grid-column: 3;
  width: 100%;
  border-collapse: collapse;
  font-size: 8pt;
  text-align: left;
}

.ppmp-doc-control td {
  border: 1px solid #000;
  padding: 3px 6px;
  line-height: 1.25;
}

.ppmp-doc-control__label {
  font-weight: 400;
}

.ppmp-doc-control__value {
  font-weight: 700;
}

/* FISCAL YEAR & END-USER UNIT */
.ppmp-meta-lines {
  margin: 10px 0 8px;
  font-size: 9.5pt;
}

.ppmp-meta-row {
  display: flex;
  align-items: baseline;
  gap: 6px;
  margin-bottom: 3px;
}

.ppmp-meta-label {
  font-weight: 700;
  white-space: nowrap;
}

.ppmp-meta-value {
  display: inline-block;
  min-width: 110px;
  border-bottom: 1px solid #000;
  padding: 0 6px;
  font-weight: 600;
}

.ppmp-meta-value--wide {
  min-width: 280px;
}

/* TABLE */
.ppmp-table {
  width: 100%;
  table-layout: fixed;
  border-collapse: collapse;
  font-size: 8pt;
  color: #000;
}

.ppmp-table th,
.ppmp-table td {
  border: 1px solid #000;
  padding: 3px 4px;
  word-wrap: break-word;
  overflow-wrap: break-word;
}

.ppmp-head-group th {
  background: #d9e1f2;
  font-size: 7.5pt;
  font-weight: 700;
  text-align: center;
  text-transform: uppercase;
  line-height: 1.2;
}

.ppmp-head-col th {
  background: #ededed;
  font-size: 7.5pt;
  font-weight: 700;
  text-align: center;
  line-height: 1.15;
}

.ppmp-head-cols-num th {
  background: #f9fafb;
  font-size: 7pt;
  font-weight: 700;
  text-align: center;
}

.ppmp-head-sub th {
  background: #ededed;
  font-size: 7pt;
  font-weight: 700;
  text-align: center;
}

.ppmp-row td.ppmp-cell {
  vertical-align: middle;
  font-size: 8pt;
  line-height: 1.25;
}

.ppmp-cell--desc {
  vertical-align: top !important;
  text-align: left;
  padding: 4px 5px;
}

.ppmp-cell--center {
  text-align: center;
}

.ppmp-cell--right {
  text-align: right;
  white-space: nowrap;
}

.ppmp-cell--budget {
  font-weight: 700;
}

.ppmp-item-name {
  font-weight: 400;
}

.ppmp-item-spec {
  margin-top: 3px;
  font-size: 7pt;
  color: #334155;
}

.ppmp-spacer-row td {
  height: 10px;
  padding: 0;
}

.ppmp-total-row td {
  font-weight: 700;
  font-size: 8.5pt;
  padding: 4px 6px;
}

.ppmp-total-label {
  text-align: right;
}

.ppmp-total-value {
  text-align: right;
  white-space: nowrap;
}

/* SIGNATURES */
.ppmp-signatures {
  display: grid;
  grid-template-columns: 260px 260px;
  gap: 80px;
  padding: 18px 0 8px 32px;
  font-size: 8.5pt;
}

.ppmp-signature-col {
  display: flex;
  flex-direction: column;
}

.ppmp-signature-label {
  font-weight: 400;
  margin-bottom: 18px;
}

.ppmp-signature-name {
  font-weight: 700;
  text-align: center;
  text-transform: uppercase;
  min-height: 14px;
}

.ppmp-signature-line {
  border-bottom: 1px solid #000;
  margin-top: 2px;
}

.ppmp-signature-line--spaced {
  margin-top: 16px;
}

.ppmp-signature-role {
  font-size: 8pt;
  text-align: center;
  line-height: 1.25;
}

.ppmp-signature-date {
  margin-top: 12px;
  font-size: 8.5pt;
}

@media print {
  @page {
    size: A4 landscape;
    margin: 8mm;
  }

  .ppmp-detail-shell {
    max-width: none !important;
    padding: 0 !important;
    margin: 0 !important;
  }

  .ppmp-document {
    page-break-after: always;
    break-after: page;
  }

  .ppmp-document:last-child {
    page-break-after: auto;
    break-after: auto;
  }

  .ppmp-header {
    page-break-after: avoid;
    break-after: avoid;
  }

  .ppmp-table thead {
    display: table-header-group;
  }

  .ppmp-row {
    page-break-inside: avoid;
    break-inside: avoid;
  }

  .ppmp-signatures {
    page-break-inside: avoid;
    break-inside: avoid;
  }
}
</style>