
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
  RefreshCw,
  ChevronDown,
} from '@lucide/vue'

import { useAuthStore } from '@/stores/auth'


import { ppmpApiService } from '../services/ppmpApiService'

import type {
  PPMPDetailApi,
} from '../models/ppmp-api.types'

import type {
  PPMPChangeHistoryApi,
} from '../models/ppmp-history.types'

import type {
  PPMPStatus,
} from '../models/ppmp.types'

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

// --------------------------------------------------
// AUTHENTICATED USER ROLES
// --------------------------------------------------

const isEndUser = computed(() =>
  authStore.hasRole('End-user'),
)

const isUnitHead = computed(() =>
  authStore.hasRole('Unit Head'),
)

const isProcurementStaff = computed(() =>
  authStore.hasRole('Procurement Staff'),
)

const isSystemAdministrator = computed(() =>
  authStore.hasRole('System Administrator'),
)

// --------------------------------------------------
// PPMP INFORMATION
// --------------------------------------------------

const ppmpId = computed(() => {
  const id = Number(route.params.id)

  return Number.isInteger(id) && id > 0 ? id : null
})

const currentStatus = computed<PPMPStatus | null>(() => {
  return (ppmp.value?.status as PPMPStatus) ?? null
})

const isDraft = computed(() =>
  currentStatus.value === 'Draft',
)

const canUnitHeadApprove = computed(() =>
  isUnitHead.value &&
  currentStatus.value === 'For Unit Head Approval',
)

const canProcurementStartReview = computed(() =>
  isProcurementStaff.value &&
  currentStatus.value === 'Approved by Unit Head',
)

const canProcurementEdit = computed(() =>
  isProcurementStaff.value &&
  currentStatus.value === 'Under Review',
)

const canProcurementCompleteReview = computed(() =>
  isProcurementStaff.value &&
  (
    currentStatus.value === 'Under Review' ||
    currentStatus.value === 'Updated'
  ),
)

const shouldShowChangeHistory = computed(() =>
  currentStatus.value === 'Updated' ||
  currentStatus.value === 'Reviewed' ||
  currentStatus.value === 'Included in APP' ||
  changeHistory.value.length > 0,
)

const formattedBudget = computed(() =>
  formatCurrency(ppmp.value?.estimatedBudget ?? 0),
)

// --------------------------------------------------
// SPREADSHEET ITEM GROUPING
// --------------------------------------------------

const groupedItems = computed(() => {
  if (!ppmp.value) {
    return []
  }

  const groups = new Map<
    string,
    PPMPDetailApi['items']
  >()

  for (const item of ppmp.value.items) {
    const category = item.categoryName || 'Uncategorized'

    if (!groups.has(category)) {
      groups.set(category, [])
    }

    groups.get(category)!.push(item)
  }

  return Array.from(groups.entries()).map(
    ([categoryName, items]) => ({
      categoryName,
      items,
      totalQuantity: items.reduce(
        (total, item) => total + Number(item.quantity || 0),
        0,
      ),
      totalBudget: items.reduce(
        (total, item) => total + Number(item.estimatedBudget || 0),
        0,
      ),
    }),
  )
})

// --------------------------------------------------
// SPREADSHEET HELPERS
// --------------------------------------------------

function toggleItem(itemId: number): void {
  if (expandedItems.value.includes(itemId)) {
    expandedItems.value = expandedItems.value.filter(
      id => id !== itemId,
    )
  } else {
    expandedItems.value.push(itemId)
  }
}

function isItemExpanded(itemId: number): boolean {
  return expandedItems.value.includes(itemId)
}

function getItemAllocationTotal(
  item: PPMPDetailApi['items'][number],
): number {
  return item.sectionAllocations.reduce(
    (total, allocation) =>
      total + Number(allocation.quantity || 0),
    0,
  )
}

// --------------------------------------------------
// LOAD PPMP CHANGE HISTORY
// --------------------------------------------------

async function loadChangeHistory(id: number): Promise<void> {
  isLoadingChangeHistory.value = true
  changeHistoryError.value = ''

  try {
    changeHistory.value =
      await ppmpApiService.getChangeHistory(id)
  } catch (error) {
    console.error(
      'Failed to load PPMP change history:',
      error,
    )

    changeHistory.value = []

    changeHistoryError.value = error instanceof Error
      ? error.message
      : 'Unable to load PPMP change history.'
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
    console.error(
      'Failed to load PPMP details:',
      error,
    )

    ppmp.value = null

    errorMessage.value = error instanceof Error
      ? error.message
      : 'Unable to load PPMP details.'
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
// WORKFLOW ACTIONS
// --------------------------------------------------

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
    console.error(
      'PPMP workflow action failed:',
      error,
    )

    errorMessage.value = error instanceof Error
      ? error.message
      : 'The requested action could not be completed.'
  } finally {
    isSubmitting.value = false
  }
}

// End-user submits the PPMP to the Unit Head.
async function submitPPMP(): Promise<void> {
  if (!isEndUser.value || !isDraft.value) {
    return
  }

  const confirmed = window.confirm(
    'Are you sure you want to submit this PPMP for Unit Head approval?',
  )

  if (!confirmed) {
    return
  }

  await performAction(
    () => ppmpApiService.submit(ppmpId.value!),
    'PPMP successfully submitted for Unit Head approval.',
  )
}


// Unit Head approves the PPMP.
async function approvePPMP(): Promise<void> {
  if (!canUnitHeadApprove.value) {
    return
  }

  const confirmed = window.confirm(
    'Are you sure you want to approve and digitally sign this PPMP?',
  )

  if (!confirmed) {
    return
  }

  await performAction(
    () => ppmpApiService.approve(ppmpId.value!),
    'PPMP successfully approved by the Unit Head.',
  )
}

// Procurement starts reviewing the approved PPMP.
async function startProcurementReview(): Promise<void> {
  if (!canProcurementStartReview.value) {
    return
  }

  const confirmed = window.confirm(
    'Start Procurement review of this PPMP?',
  )

  if (!confirmed) {
    return
  }

  await performAction(
    () => ppmpApiService.startProcurementReview(
      ppmpId.value!,
    ),
    'Procurement review has started.',
  )
}

// Procurement completes the review.
async function completeProcurementReview(): Promise<void> {
  if (!canProcurementCompleteReview.value) {
    return
  }

  const confirmed = window.confirm(
    'Are you sure you want to mark this PPMP as Reviewed?',
  )

  if (!confirmed) {
    return
  }

  await performAction(
    () => ppmpApiService.completeReview(
      ppmpId.value!,
    ),
    'PPMP review successfully completed.',
  )
}

// Procurement opens the existing edit page.
function editAsProcurement(): void {
  if (!canProcurementEdit.value || !ppmpId.value) {
    return
  }

  void router.push({
    name: 'procurement-edit-ppmp',
    params: { id: ppmpId.value },
  })
}

// --------------------------------------------------
// DISPLAY HELPERS
// --------------------------------------------------

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-PH', {
    style: 'currency',
    currency: 'PHP',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount)
}

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

function formatHistoricalBoolean(
  value: boolean | null,
): string {
  if (value === null) {
    return 'Not recorded'
  }

  return value ? 'Yes' : 'No'
}

function formatHistoricalText(
  value: string | null,
): string {
  return value?.trim() || 'Not recorded'
}

function getStatusClass(status: string): string {
  const classes: Record<string, string> = {
    Draft: 'bg-slate-100 text-slate-700',
    'For Unit Head Approval': 'bg-blue-50 text-blue-700',
    'Approved by Unit Head': 'bg-indigo-50 text-indigo-700',
    'Under Review': 'bg-amber-50 text-amber-700',
    Updated: 'bg-purple-50 text-purple-700',
    Reviewed: 'bg-emerald-50 text-emerald-700',
    'Included in APP': 'bg-teal-50 text-teal-700',
  }

  return classes[status] ?? 'bg-slate-100 text-slate-700'
}
</script>

<template>
  <div class="space-y-6">

    <!-- PAGE HEADER -->
    <div class="flex flex-wrap items-center justify-between gap-4">
      <div>
        <RouterLink
          to="/ppmp"
          class="mb-3 inline-flex items-center gap-2 text-sm text-gray-500 hover:text-emerald-700"
        >
          <ArrowLeft :size="16" />
          Back to PPMP List
        </RouterLink>

        <h1 class="text-2xl font-bold text-gray-900">
          PPMP Details
        </h1>

        <p class="mt-1 text-sm text-gray-500">
          View and manage the details of this Procurement Project Management Plan.
        </p>
      </div>

      <button
        type="button"
        @click="loadPPMP"
        :disabled="isLoading"
        class="inline-flex items-center gap-2 rounded-lg border border-gray-300 bg-white px-4 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
      >
        <RefreshCw :size="16" />
        Refresh
      </button>
    </div>

    <!-- SUCCESS MESSAGE -->
    <div
      v-if="successMessage"
      class="rounded-lg border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-800"
    >
      <div class="flex items-center gap-2">
        <CheckCircle :size="18" />
        {{ successMessage }}
      </div>
    </div>

    <!-- ERROR MESSAGE -->
    <div
      v-if="errorMessage"
      class="rounded-lg border border-red-200 bg-red-50 p-4"
    >
      <p class="text-sm font-medium text-red-800">
        {{ errorMessage }}
      </p>

      <button
        type="button"
        @click="loadPPMP"
        class="mt-2 text-sm font-semibold text-red-700 hover:underline"
      >
        Try Again
      </button>
    </div>

    <!-- LOADING -->
    <div
      v-if="isLoading"
      class="rounded-xl border border-gray-200 bg-white p-12 text-center"
    >
      <RefreshCw
        :size="28"
        class="mx-auto mb-3 animate-spin text-emerald-700"
      />

      <p class="text-sm text-gray-500">
        Loading PPMP details...
      </p>
    </div>

    <!-- NOT FOUND -->
    <div
      v-else-if="!ppmp"
      class="rounded-xl border border-gray-200 bg-white p-10 text-center"
    >
      <ClipboardList
        :size="36"
        class="mx-auto mb-3 text-gray-400"
      />

      <h2 class="text-lg font-semibold text-gray-800">
        PPMP Record Not Found
      </h2>

      <p class="mt-1 text-sm text-gray-500">
        This record may be unavailable or you may not have permission to view it.
      </p>

      <RouterLink
        to="/ppmp"
        class="mt-5 inline-block text-sm font-medium text-emerald-700 hover:underline"
      >
        Return to PPMP List
      </RouterLink>
    </div>

    <template v-else>

      <!-- GENERAL INFORMATION -->
      <section class="rounded-xl border border-gray-200 bg-white p-6">

        <div class="mb-5 flex items-center gap-2">
          <ClipboardList :size="19" class="text-emerald-700" />

          <h2 class="font-semibold text-gray-900">
            General Information
          </h2>
        </div>

        <div class="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">

          <div>
            <p class="text-xs font-medium uppercase text-gray-500">
              PPMP Reference
            </p>

            <p class="mt-1 font-semibold text-gray-900">
              {{ ppmp.ppmpReference }}
            </p>
          </div>

          <div>
            <p class="text-xs font-medium uppercase text-gray-500">
              Department
            </p>

            <p class="mt-1 font-medium text-gray-900">
              {{ ppmp.departmentName }}
            </p>
          </div>

          <div>
            <p class="text-xs font-medium uppercase text-gray-500">
              Planning Year
            </p>

            <p class="mt-1 font-medium text-gray-900">
              {{ ppmp.planningYear }}
            </p>
          </div>

          <div>
            <p class="text-xs font-medium uppercase text-gray-500">
              Date Created
            </p>

            <p class="mt-1 font-medium text-gray-900">
              {{ formatDate(ppmp.createdAt) }}
            </p>
          </div>

          <div>
            <p class="text-xs font-medium uppercase text-gray-500">
              Date Submitted
            </p>

            <p class="mt-1 font-medium text-gray-900">
              {{ formatDate(ppmp.submittedAt) }}
            </p>
          </div>

          <div>
            <p class="text-xs font-medium uppercase text-gray-500">
              Status
            </p>

            <span
              :class="[
                'mt-1 inline-flex rounded-full px-3 py-1 text-xs font-semibold',
                getStatusClass(ppmp.status),
              ]"
            >
              {{ ppmp.status }}
            </span>
          </div>

          <div>
            <p class="text-xs font-medium uppercase text-gray-500">
              Total Items
            </p>

            <p class="mt-1 font-medium text-gray-900">
              {{ ppmp.items.length }}
            </p>
          </div>

          <div>
            <p class="text-xs font-medium uppercase text-gray-500">
              Estimated Budget
            </p>

            <p class="mt-1 font-semibold text-emerald-700">
              {{ formattedBudget }}
            </p>
          </div>

          <div>
            <p class="text-xs font-medium uppercase text-gray-500">
              Last Updated
            </p>

            <p class="mt-1 font-medium text-gray-900">
              {{ formatDate(ppmp.updatedAt) }}
            </p>
          </div>

        </div>
      </section>

      <!-- WORKFLOW ACTIONS -->
      <section
        v-if="
          canUnitHeadApprove ||
          canProcurementStartReview ||
          canProcurementEdit ||
          canProcurementCompleteReview ||
          (isEndUser && isDraft)
        "
        class="rounded-xl border border-gray-200 bg-white p-6"
      >

        <h2 class="mb-2 font-semibold text-gray-900">
          Available Actions
        </h2>

        <p class="mb-5 text-sm text-gray-500">
          Actions available to your authenticated role for this PPMP.
        </p>

        <div class="flex flex-wrap gap-3">

          <button
            v-if="isEndUser && isDraft"
            type="button"
            :disabled="isSubmitting"
            @click="submitPPMP"
            class="inline-flex items-center gap-2 rounded-lg bg-emerald-700 px-4 py-2.5 text-sm font-medium text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-50"
          >
            <CheckCircle :size="17" />
            Submit for Unit Head Approval
          </button>

          <button
            v-if="canUnitHeadApprove"
            type="button"
            :disabled="isSubmitting"
            @click="approvePPMP"
            class="inline-flex items-center gap-2 rounded-lg bg-emerald-700 px-4 py-2.5 text-sm font-medium text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-50"
          >
            <CheckCircle :size="17" />
            Approve
          </button>

          <button
            v-if="canProcurementStartReview"
            type="button"
            :disabled="isSubmitting"
            @click="startProcurementReview"
            class="inline-flex items-center gap-2 rounded-lg bg-blue-700 px-4 py-2.5 text-sm font-medium text-white hover:bg-blue-800 disabled:cursor-not-allowed disabled:opacity-50"
          >
            <ClipboardList :size="17" />
            Start Review
          </button>

          <button
            v-if="canProcurementEdit"
            type="button"
            @click="editAsProcurement"
            class="inline-flex items-center gap-2 rounded-lg bg-blue-700 px-4 py-2.5 text-sm font-medium text-white hover:bg-blue-800"
          >
            <Pencil :size="17" />
            Edit PPMP
          </button>

          <button
            v-if="canProcurementCompleteReview"
            type="button"
            :disabled="isSubmitting"
            @click="completeProcurementReview"
            class="inline-flex items-center gap-2 rounded-lg bg-emerald-700 px-4 py-2.5 text-sm font-medium text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-50"
          >
            <CheckCircle :size="17" />
            Complete Review
          </button>

        </div>

        <p
          v-if="isSubmitting"
          class="mt-4 text-sm text-gray-500"
        >
          Processing your request...
        </p>

      </section>

      <!-- DRAFT NOTICE -->
      <section
        v-if="isEndUser && isDraft"
        class="rounded-lg border border-amber-200 bg-amber-50 p-4"
      >
        <p class="text-sm font-medium text-amber-900">
          Draft PPMP
        </p>

        <p class="mt-1 text-sm text-amber-800">
          This PPMP has not yet been submitted. Draft editing through the backend is not implemented yet.
          You can submit it for Unit Head approval when it is ready.
        </p>
      </section>

      <!-- SUBMISSION INFORMATION -->
      <section
        v-if="ppmp.submittedAt"
        class="rounded-xl border border-gray-200 bg-white p-6"
      >
        <div class="flex items-center gap-2">
          <Clock :size="19" class="text-blue-700" />

          <h2 class="font-semibold text-gray-900">
            Submission Information
          </h2>
        </div>

        <p class="mt-3 text-sm text-gray-600">
          This PPMP was submitted on
          <strong>{{ formatDate(ppmp.submittedAt) }}</strong>.
        </p>
      </section>

      <!-- REQUESTED ITEMS: SPREADSHEET VIEW -->
      <section class="rounded-xl border border-gray-200 bg-white p-6">

        <div class="mb-5 flex flex-wrap items-center justify-between gap-3">
          <div class="flex items-center gap-2">
            <Package :size="19" class="text-emerald-700" />

            <div>
              <h2 class="font-semibold text-gray-900">
                Requested Items
              </h2>

              <p class="mt-1 text-xs text-gray-500">
                Spreadsheet-style view of the submitted PPMP items.
              </p>
            </div>
          </div>

          <span class="text-sm text-gray-500">
            {{ ppmp.items.length }} item(s)
          </span>
        </div>

        <div
          v-if="groupedItems.length > 0"
          class="space-y-6"
        >

          <!-- CATEGORY GROUP -->
          <div
            v-for="group in groupedItems"
            :key="group.categoryName"
            class="overflow-hidden rounded-lg border border-gray-200"
          >

            <!-- CATEGORY HEADER -->
            <div class="flex flex-wrap items-center justify-between gap-3 border-b border-gray-200 bg-gray-50 px-4 py-3">

              <div>
                <h3 class="text-sm font-semibold text-gray-900">
                  {{ group.categoryName }}
                </h3>

                <p class="mt-1 text-xs text-gray-500">
                  {{ group.items.length }} item(s)
                </p>
              </div>

              <p class="text-sm font-semibold text-emerald-700">
                {{ formatCurrency(group.totalBudget) }}
              </p>

            </div>

            <!-- SPREADSHEET TABLE -->
            <div class="overflow-x-auto">
              <table class="w-full min-w-[850px] border-collapse text-left text-sm">

                <thead class="bg-gray-100 text-xs uppercase tracking-wide text-gray-600">
                  <tr>
                    <th class="w-[38%] border-b border-gray-200 px-4 py-3 font-semibold">
                      Item Description
                    </th>

                    <th class="border-b border-gray-200 px-4 py-3 font-semibold">
                      Unit
                    </th>

                    <th class="border-b border-gray-200 px-4 py-3 text-right font-semibold">
                      ABC / Unit
                    </th>

                    <th class="border-b border-gray-200 px-4 py-3 text-right font-semibold">
                      Quantity
                    </th>

                    <th class="border-b border-gray-200 px-4 py-3 text-right font-semibold">
                      Estimated Total
                    </th>

                    <th class="w-24 border-b border-gray-200 px-4 py-3 text-center font-semibold">
                      Action
                    </th>
                  </tr>
                </thead>

                <tbody class="divide-y divide-gray-100">

                  <template
                    v-for="(item, index) in group.items"
                    :key="item.ppmpItemId"
                  >

                    <!-- ITEM ROW -->
                    <tr class="transition-colors hover:bg-gray-50">

                      <td class="px-4 py-3">
                        <p class="font-medium text-gray-900">
                          {{ item.itemDescription }}
                        </p>

                        <div class="mt-1 flex flex-wrap gap-x-3 gap-y-1 text-xs text-gray-500">
                          <span>
                            Code: {{ item.itemCode || '—' }}
                          </span>

                          <span v-if="item.itemType">
                            Type: {{ item.itemType }}
                          </span>
                        </div>

                        <p
                          v-if="item.specifications"
                          class="mt-1 whitespace-pre-wrap text-xs text-gray-500"
                        >
                          {{ item.specifications }}
                        </p>
                      </td>

                      <td class="px-4 py-3 text-gray-700">
                        {{ item.unit || '—' }}
                      </td>

                      <td class="px-4 py-3 text-right tabular-nums text-gray-700">
                        {{ formatCurrency(item.unitPrice) }}
                      </td>

                      <td class="px-4 py-3 text-right font-medium tabular-nums text-gray-900">
                        {{ item.quantity }}
                      </td>

                      <td class="px-4 py-3 text-right font-semibold tabular-nums text-emerald-700">
                        {{ formatCurrency(item.estimatedBudget) }}
                      </td>

                      <td class="px-4 py-3 text-center">
                        <button
                          type="button"
                          @click="toggleItem(item.ppmpItemId)"
                          :aria-expanded="isItemExpanded(item.ppmpItemId)"
                          class="inline-flex items-center gap-1 rounded-md border border-gray-200 px-2.5 py-1.5 text-xs font-medium text-gray-600 hover:border-emerald-300 hover:bg-emerald-50 hover:text-emerald-700"
                        >
                          <ChevronDown
                            :size="15"
                            :class="[
                              'transition-transform',
                              isItemExpanded(item.ppmpItemId)
                                ? 'rotate-180'
                                : '',
                            ]"
                          />

                          {{ isItemExpanded(item.ppmpItemId) ? 'Hide' : 'View' }}
                        </button>
                      </td>

                    </tr>

                    <!-- EXPANDED SECTION ALLOCATIONS -->
                    <tr
                      v-if="isItemExpanded(item.ppmpItemId)"
                      class="bg-slate-50"
                    >
                      <td colspan="6" class="px-4 py-4">

                        <div class="rounded-lg border border-gray-200 bg-white p-4">

                          <div class="mb-3 flex flex-wrap items-center justify-between gap-2">
                            <div>
                              <h4 class="text-sm font-semibold text-gray-800">
                                Section Quantity Allocation
                              </h4>

                              <p class="mt-1 text-xs text-gray-500">
                                {{ item.itemDescription }}
                              </p>
                            </div>

                            <span class="text-xs font-medium text-gray-500">
                              Read-only
                            </span>
                          </div>

                          <div
                            v-if="item.sectionAllocations.length > 0"
                            class="overflow-x-auto"
                          >
                            <table class="w-full min-w-[500px] text-left text-sm">

                              <thead class="bg-gray-50 text-xs uppercase text-gray-500">
                                <tr>
                                  <th class="px-3 py-2 font-medium">
                                    Section Code
                                  </th>

                                  <th class="px-3 py-2 font-medium">
                                    Section
                                  </th>

                                  <th class="px-3 py-2 text-right font-medium">
                                    Allocated Quantity
                                  </th>
                                </tr>
                              </thead>

                              <tbody class="divide-y divide-gray-100">

                                <tr
                                  v-for="allocation in item.sectionAllocations"
                                  :key="allocation.allocationId"
                                >
                                  <td class="px-3 py-3 text-gray-600">
                                    {{ allocation.sectionCode }}
                                  </td>

                                  <td class="px-3 py-3 text-gray-800">
                                    {{ allocation.sectionName }}
                                  </td>

                                  <td class="px-3 py-3 text-right font-medium tabular-nums text-gray-800">
                                    {{ allocation.quantity }}
                                  </td>
                                </tr>

                              </tbody>

                              <tfoot class="border-t border-gray-200 bg-gray-50">
                                <tr>
                                  <td
                                    colspan="2"
                                    class="px-3 py-3 text-right text-xs font-semibold text-gray-600"
                                  >
                                    Total Allocated Quantity
                                  </td>

                                  <td class="px-3 py-3 text-right font-semibold tabular-nums text-emerald-700">
                                    {{ getItemAllocationTotal(item) }}
                                  </td>
                                </tr>
                              </tfoot>

                            </table>
                          </div>

                          <p
                            v-else
                            class="rounded-md bg-gray-50 p-4 text-sm text-gray-500"
                          >
                            No section allocations recorded for this item.
                          </p>

                        </div>

                      </td>
                    </tr>

                  </template>

                </tbody>

                <!-- CATEGORY TOTAL -->
                <tfoot class="border-t-2 border-gray-200 bg-gray-50">
                  <tr>
                    <td
                      colspan="3"
                      class="px-4 py-3 text-right text-xs font-semibold uppercase text-gray-600"
                    >
                      Category Total
                    </td>

                    <td class="px-4 py-3 text-right font-semibold tabular-nums text-gray-800">
                      {{ group.totalQuantity }}
                    </td>

                    <td class="px-4 py-3 text-right font-bold tabular-nums text-emerald-700">
                      {{ formatCurrency(group.totalBudget) }}
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
          class="rounded-lg bg-gray-50 p-8 text-center"
        >
          <Package
            :size="30"
            class="mx-auto mb-3 text-gray-400"
          />

          <p class="text-sm text-gray-500">
            No requested items are recorded in this PPMP.
          </p>
        </div>

        <!-- TOTAL BUDGET -->
        <div class="mt-5 flex justify-end border-t border-gray-100 pt-5">
          <div class="text-right">
            <p class="text-xs font-medium uppercase text-gray-500">
              Total Estimated Budget
            </p>

            <p class="mt-1 text-xl font-bold text-emerald-700">
              {{ formattedBudget }}
            </p>
          </div>
        </div>

      </section>

      <!-- PROCUREMENT CHANGE HISTORY -->
      <section
        v-if="shouldShowChangeHistory"
        class="rounded-xl border border-gray-200 bg-white p-6"
      >

        <div class="mb-5 flex flex-wrap items-center justify-between gap-3">

          <div class="flex items-center gap-2">
            <ClipboardList
              :size="21"
              class="text-emerald-700"
            />

            <div>
              <h2 class="font-semibold text-gray-900">
                Procurement Change History
              </h2>

              <p class="mt-1 text-xs text-gray-500">
                Recorded changes made during Procurement review.
              </p>
            </div>
          </div>

          <span class="rounded-full bg-gray-100 px-3 py-1 text-xs font-medium text-gray-600">
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
            class="mx-auto mb-2 animate-spin text-emerald-700"
          />

          <p class="text-sm text-gray-500">
            Loading change history...
          </p>
        </div>

        <!-- HISTORY ERROR -->
        <div
          v-else-if="changeHistoryError"
          class="rounded-lg border border-red-200 bg-red-50 p-4"
        >
          <p class="text-sm text-red-700">
            {{ changeHistoryError }}
          </p>

          <button
            type="button"
            @click="ppmpId && loadChangeHistory(ppmpId)"
            class="mt-2 text-sm font-semibold text-red-700 hover:underline"
          >
            Retry Loading History
          </button>
        </div>

        <!-- EMPTY HISTORY -->
        <div
          v-else-if="changeHistory.length === 0"
          class="rounded-lg bg-gray-50 p-8 text-center"
        >
          <Clock
            :size="30"
            class="mx-auto mb-3 text-gray-400"
          />

          <p class="text-sm font-medium text-gray-700">
            No procurement changes recorded.
          </p>

          <p class="mt-1 text-xs text-gray-500">
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
            class="overflow-hidden rounded-lg border border-gray-200"
          >

            <!-- HISTORY HEADER -->
            <div class="border-b border-gray-200 bg-gray-50 p-4">

              <div class="flex flex-wrap items-start justify-between gap-3">

                <div>
                  <p class="text-sm font-semibold text-gray-900">
                    {{ history.changedByName }}
                  </p>

                  <p class="mt-1 text-xs text-gray-500">
                    {{ formatDate(history.changedAt) }}
                  </p>
                </div>

                <span class="rounded-full bg-purple-50 px-3 py-1 text-xs font-semibold text-purple-700">
                  Change #{{ history.changeHistoryId }}
                </span>

              </div>

              <div class="mt-4">
                <p class="text-xs font-semibold uppercase text-gray-500">
                  Reason for Changes
                </p>

                <p class="mt-1 whitespace-pre-wrap text-sm text-gray-800">
                  {{ history.reason }}
                </p>
              </div>

            </div>

            <!-- BUDGET COMPARISON -->
            <div class="grid gap-4 border-b border-gray-100 p-4 sm:grid-cols-2">

              <div>
                <p class="text-xs font-medium uppercase text-gray-500">
                  Previous Budget
                </p>

                <p class="mt-1 text-lg font-semibold text-gray-700">
                  {{ formatCurrency(history.previousBudget) }}
                </p>
              </div>

              <div>
                <p class="text-xs font-medium uppercase text-gray-500">
                  Updated Budget
                </p>

                <p class="mt-1 text-lg font-semibold text-emerald-700">
                  {{ formatCurrency(history.updatedBudget) }}
                </p>
              </div>

            </div>

            <!-- ITEM CHANGES -->
            <div class="p-4">

              <h3 class="mb-4 text-sm font-semibold text-gray-800">
                Item Changes
              </h3>

              <div
                v-if="history.items.length > 0"
                class="space-y-4"
              >

                <div
                  v-for="item in history.items"
                  :key="item.changeHistoryItemId"
                  class="rounded-lg border border-gray-200 p-4"
                >

                  <div class="mb-3 flex flex-wrap items-center justify-between gap-2">

                    <p class="text-sm font-semibold text-gray-900">
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
                      class="rounded-full bg-emerald-50 px-2 py-1 text-xs font-medium text-emerald-700"
                    >
                      Added
                    </span>

                    <span
                      v-else-if="!item.updatedDescription"
                      class="rounded-full bg-red-50 px-2 py-1 text-xs font-medium text-red-700"
                    >
                      Removed
                    </span>

                    <span
                      v-else
                      class="rounded-full bg-blue-50 px-2 py-1 text-xs font-medium text-blue-700"
                    >
                      Modified
                    </span>

                  </div>

                  <div class="grid gap-4 sm:grid-cols-2">

                    <!-- PREVIOUS DETAILS -->
                    <div class="rounded-md bg-gray-50 p-3">

                      <p class="mb-2 text-xs font-semibold uppercase text-gray-500">
                        Previous Details
                      </p>

                      <template v-if="item.previousDescription">

                        <p class="text-sm font-medium text-gray-900">
                          {{ item.previousDescription }}
                        </p>

                        <p class="mt-1 text-xs text-gray-500">
                          Code: {{ item.previousItemCode || '—' }}
                        </p>

                        <div class="mt-3 space-y-2 border-t border-gray-200 pt-3 text-xs text-gray-600">

  <div>
    <p class="font-semibold text-gray-500">
      General Description and Objective
    </p>

    <p class="mt-1 whitespace-pre-wrap">
      {{ formatHistoricalText(item.previousGeneralDescriptionAndObjective) }}
    </p>
  </div>

  <p>
    <span class="font-semibold text-gray-500">Project Type:</span>
    {{ formatHistoricalText(item.previousProjectType) }}
  </p>

  <p>
    <span class="font-semibold text-gray-500">
      Pre-Procurement Conference:
    </span>
    {{ formatHistoricalBoolean(item.previousPreProcurementConference) }}
  </p>

</div>

                        <div class="mt-3 space-y-1 text-xs text-gray-600">

                          <p>
                            Quantity: {{ item.previousQuantity }}
                          </p>

                          <p>
                            Unit: {{ item.previousUnit || '—' }}
                          </p>

                          <p>
                            Unit Price:
                            {{ formatCurrency(item.previousUnitPrice ?? 0) }}
                          </p>

                          <p>
                            Estimated Budget:
                            {{ formatCurrency(item.previousEstimatedBudget ?? 0) }}
                          </p>

                        </div>

                      </template>

                      <p
                        v-else
                        class="text-sm italic text-gray-400"
                      >
                        Item did not previously exist.
                      </p>

                    </div>

                    <!-- UPDATED DETAILS -->
                    <div class="rounded-md bg-emerald-50/50 p-3">

                      <p class="mb-2 text-xs font-semibold uppercase text-emerald-700">
                        Updated Details
                      </p>

                      <template v-if="item.updatedDescription">

                        <p class="text-sm font-medium text-gray-900">
                          {{ item.updatedDescription }}
                        </p>

                        <p class="mt-1 text-xs text-gray-500">
                          Code: {{ item.updatedItemCode || '—' }}
                        </p>
                            <div class="mt-3 space-y-2 border-t border-emerald-100 pt-3 text-xs text-gray-600">

  <div>
    <p class="font-semibold text-gray-500">
      General Description and Objective
    </p>

    <p class="mt-1 whitespace-pre-wrap">
      {{ formatHistoricalText(item.updatedGeneralDescriptionAndObjective) }}
    </p>
  </div>

  <p>
    <span class="font-semibold text-gray-500">Project Type:</span>
    {{ formatHistoricalText(item.updatedProjectType) }}
  </p>

  <p>
    <span class="font-semibold text-gray-500">
      Pre-Procurement Conference:
    </span>
    {{ formatHistoricalBoolean(item.updatedPreProcurementConference) }}
  </p>

</div>
                        <div class="mt-3 space-y-1 text-xs text-gray-600">

                          <p>
                            Quantity: {{ item.updatedQuantity }}
                          </p>

                          <p>
                            Unit: {{ item.updatedUnit || '—' }}
                          </p>

                          <p>
                            Unit Price:
                            {{ formatCurrency(item.updatedUnitPrice ?? 0) }}
                          </p>

                          <p>
                            Estimated Budget:
                            {{ formatCurrency(item.updatedEstimatedBudget ?? 0) }}
                          </p>

                        </div>

                      </template>

                      <p
                        v-else
                        class="text-sm italic text-gray-400"
                      >
                        Item was removed.
                      </p>

                    </div>

                  </div>

                  <!-- SECTION ALLOCATION CHANGES -->
                  <div
                    v-if="item.sectionAllocations.length > 0"
                    class="mt-4 border-t border-gray-100 pt-3"
                  >

                    <h4 class="mb-3 text-xs font-semibold uppercase text-gray-600">
                      Section Allocation Changes
                    </h4>

                    <div class="space-y-2">

                      <div
                        v-for="allocation in item.sectionAllocations"
                        :key="allocation.changeHistorySectionAllocationId"
                        class="grid gap-3 rounded-md bg-gray-50 p-3 text-xs sm:grid-cols-2"
                      >

                        <div>
                          <p class="font-medium text-gray-500">
                            Previous Allocation
                          </p>

                          <p class="mt-1 text-gray-800">
                            {{
                              allocation.previousSectionName ||
                              'No previous allocation'
                            }}
                          </p>

                          <p
                            v-if="allocation.previousSectionName"
                            class="mt-1 text-gray-500"
                          >
                            {{ allocation.previousSectionCode }}
                            · Quantity: {{ allocation.previousQuantity }}
                          </p>
                        </div>

                        <div>
                          <p class="font-medium text-emerald-700">
                            Updated Allocation
                          </p>

                          <p class="mt-1 text-gray-800">
                            {{
                              allocation.updatedSectionName ||
                              'Allocation removed'
                            }}
                          </p>

                          <p
                            v-if="allocation.updatedSectionName"
                            class="mt-1 text-gray-500"
                          >
                            {{ allocation.updatedSectionCode }}
                            · Quantity: {{ allocation.updatedQuantity }}
                          </p>
                        </div>

                      </div>

                    </div>

                  </div>

                </div>

              </div>

              <p
                v-else
                class="text-sm text-gray-500"
              >
                No individual item changes were recorded.
              </p>

            </div>

          </article>

        </div>

      </section>

      <!-- ADMINISTRATOR INFORMATION -->
      <section
        v-if="isSystemAdministrator"
        class="rounded-lg border border-slate-200 bg-slate-50 p-4"
      >
        <p class="text-sm font-medium text-slate-800">
          System Administrator View
        </p>

        <p class="mt-1 text-xs text-slate-600">
          You are viewing this record with administrator access.
          Workflow actions remain restricted to their assigned roles.
        </p>
      </section>

    </template>

  </div>
</template>
