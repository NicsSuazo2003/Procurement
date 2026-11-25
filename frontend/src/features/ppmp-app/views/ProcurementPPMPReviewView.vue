<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import {
  ClipboardList,
  Search,
  Eye,
  Clock,
  FileCheck,
  Play,
  Pencil,
  RefreshCw,
  CheckCircle,
} from '@lucide/vue'

import { ppmpApiService } from '../services/ppmpApiService'
import type { PPMPListApi } from '../models/ppmp-api.types'

import { useProcurementReviewCountStore } from '@/stores/procurement-review-count'
import { formatDatePH, formatPHP, getPPMPStatusStyle } from '@/shared/utilities/ppmpUiHelpers'

type ReviewStatus =
  | 'Approved by Unit Head'
  | 'Under Review'
  | 'Updated'
  | 'Reviewed'

// The three statuses this view cares about.
const REVIEW_STATUSES: ReviewStatus[] = [
  'Approved by Unit Head',
  'Under Review',
  'Updated',
]

const searchQuery = ref('')
const selectedDepartment = ref('All Units')
const selectedStatus = ref('All Review Statuses')

const page = ref(1)
const pageSize = ref(25)
const totalCount = ref(0)
const totalPages = ref(0)

const ppmpRecords = ref<PPMPListApi[]>([])
const isLoading = ref(false)
const isProcessing = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const summary = ref({
  awaitingProcurement: 0,
  underReview: 0,
  updated: 0,
})

const procurementReview = useProcurementReviewCountStore()

const statuses = [
  'All Review Statuses',
  'Approved by Unit Head',
  'Under Review',
  'Updated',
]

const departments = computed(() => [
  'All Units',
  ...new Set(ppmpRecords.value.map(record => record.departmentName)),
])

async function loadSummary(): Promise<void> {
  try {
    const data = await ppmpApiService.getSummary()

    const get = (key: string) => data.byStatus[key] ?? 0

    summary.value = {
      awaitingProcurement: get('Approved by Unit Head'),
      underReview: get('Under Review'),
      updated: get('Updated'),
    }

    procurementReview.setPendingCount(summary.value.awaitingProcurement)
  } catch (error) {
    console.error('Unable to load Procurement PPMP summary:', error)
  }
}

async function loadRecords() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const statusFilter =
      selectedStatus.value === 'All Review Statuses'
        ? REVIEW_STATUSES
        : [selectedStatus.value]

    const response = await ppmpApiService.getPaged({
      page: page.value,
      pageSize: pageSize.value,
      status: statusFilter,
    })

    ppmpRecords.value = response.items
    totalCount.value = response.totalCount
    totalPages.value = response.totalPages

    await loadSummary()
  } catch (error) {
    console.error('Unable to load Procurement PPMP records:', error)
    errorMessage.value =
      error instanceof Error
        ? error.message
        : 'Unable to load PPMP records. Please try again.'
  } finally {
    isLoading.value = false
  }
}

onMounted(loadRecords)

// Client-side search + unit filter, applied to the page.
const filteredRecords = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()

  return ppmpRecords.value.filter(record => {
    const matchesSearch =
      !query ||
      record.ppmpReference.toLowerCase().includes(query) ||
      record.departmentName.toLowerCase().includes(query)

    const matchesDepartment =
      selectedDepartment.value === 'All Units' ||
      record.departmentName === selectedDepartment.value

    return matchesSearch && matchesDepartment
  })
})

const totalForReview = computed(
  () =>
    summary.value.awaitingProcurement +
    summary.value.underReview +
    summary.value.updated,
)

const approvedCount = computed(() => summary.value.awaitingProcurement)
const underReviewCount = computed(() => summary.value.underReview)
const updatedCount = computed(() => summary.value.updated)

function goToPage(next: number): void {
  if (next < 1 || next > totalPages.value) return
  page.value = next
  void loadRecords()
}

async function startReview(record: PPMPListApi) {
  if (record.status !== 'Approved by Unit Head' || isProcessing.value) {
    return
  }

  const confirmed = window.confirm(
    `Start reviewing ${record.ppmpReference} from ${record.departmentName}?`,
  )

  if (!confirmed) return

  isProcessing.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    await ppmpApiService.startProcurementReview(record.ppmpId)

    successMessage.value =
      `${record.ppmpReference} is now Under Review.`

    await loadRecords()
  } catch (error) {
    console.error('Unable to start PPMP review:', error)

    errorMessage.value =
      error instanceof Error
        ? error.message
        : 'Unable to start the PPMP review. Please try again.'
  } finally {
    isProcessing.value = false
  }
}

async function completeReview(record: PPMPListApi) {
  if (record.status !== 'Updated' || isProcessing.value) {
    return
  }

  const confirmed = window.confirm(
    `Complete the Procurement review for ${record.ppmpReference}?`,
  )

  if (!confirmed) return

  isProcessing.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    await ppmpApiService.completeReview(record.ppmpId)

    successMessage.value =
      `${record.ppmpReference} has been marked as Reviewed.`

    await loadRecords()
  } catch (error) {
    console.error('Unable to complete PPMP review:', error)

    errorMessage.value =
      error instanceof Error
        ? error.message
        : 'Unable to complete the review. Please try again.'
  } finally {
    isProcessing.value = false
  }
}
</script>

<template>
  <div class="mx-auto max-w-7xl space-y-6">
    <!-- PAGE HEADER -->
    <div class="flex flex-wrap items-start justify-between gap-4">
      <div class="flex items-start gap-3">
        <div class="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/20">
          <ClipboardList :size="22" />
        </div>

        <div>
          <h1 class="text-2xl font-bold tracking-tight text-slate-900">
            Procurement PPMP Review
          </h1>
          <p class="mt-1 text-sm text-slate-500">
            Review PPMPs approved by Unit Heads and monitor Procurement review progress.
          </p>
        </div>
      </div>

      <button
        type="button"
        :disabled="isLoading || isProcessing"
        class="drmc-btn-secondary"
        @click="loadRecords"
      >
        <RefreshCw :size="16" :class="{ 'animate-spin': isLoading }" />
        Refresh
      </button>
    </div>

    <!-- ALERTS -->
    <div
      v-if="successMessage"
      role="status"
      class="rounded-xl border border-brand-200 bg-brand-50 px-4 py-3 text-sm font-medium text-brand-800"
    >
      {{ successMessage }}
    </div>

    <div
      v-if="errorMessage"
      role="alert"
      class="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-800"
    >
      {{ errorMessage }}
    </div>

    <!-- SUMMARY CARDS -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
      <div class="drmc-card p-5">
        <div class="flex items-center justify-between">
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
            Total for Review
          </p>
          <div class="flex h-9 w-9 items-center justify-center rounded-lg bg-slate-100 text-slate-700">
            <ClipboardList :size="18" />
          </div>
        </div>
        <p class="mt-3 text-2xl font-bold text-slate-900 tabular-nums">
          {{ totalForReview }}
        </p>
        <p class="mt-1 text-xs text-slate-500">
          Approved, under review, and updated
        </p>
      </div>

      <div class="drmc-card p-5">
        <div class="flex items-center justify-between">
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
            Awaiting Procurement
          </p>
          <div class="flex h-9 w-9 items-center justify-center rounded-lg bg-blue-50 text-blue-600">
            <FileCheck :size="18" />
          </div>
        </div>
        <p class="mt-3 text-2xl font-bold text-blue-700 tabular-nums">
          {{ approvedCount }}
        </p>
        <p class="mt-1 text-xs text-slate-500">
          Approved by Unit Head
        </p>
      </div>

      <div class="drmc-card p-5">
        <div class="flex items-center justify-between">
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
            Under Review
          </p>
          <div class="flex h-9 w-9 items-center justify-center rounded-lg bg-purple-50 text-purple-600">
            <Clock :size="18" />
          </div>
        </div>
        <p class="mt-3 text-2xl font-bold text-purple-700 tabular-nums">
          {{ underReviewCount }}
        </p>
        <p class="mt-1 text-xs text-slate-500">
          Currently being reviewed
        </p>
      </div>

      <div class="drmc-card p-5">
        <div class="flex items-center justify-between">
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
            Updated
          </p>
          <div class="flex h-9 w-9 items-center justify-center rounded-lg bg-orange-50 text-orange-600">
            <Pencil :size="18" />
          </div>
        </div>
        <p class="mt-3 text-2xl font-bold text-orange-700 tabular-nums">
          {{ updatedCount }}
        </p>
        <p class="mt-1 text-xs text-slate-500">
          Awaiting review completion
        </p>
      </div>
    </div>

    <!-- REVIEW RECORDS TABLE -->
    <section class="drmc-card overflow-hidden">
      <div class="border-b border-slate-200/80 p-5">
        <div class="mb-4">
          <h2 class="text-base font-semibold text-slate-900">
            PPMP Records for Procurement
          </h2>
          <p class="mt-0.5 text-xs text-slate-500">
            End-users submit one PPMP. During Procurement review, add requested items or update details before completing the review.
          </p>
        </div>

        <div class="grid grid-cols-1 gap-3 md:grid-cols-3">
          <div class="relative">
            <Search
              :size="16"
              class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
            />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Search reference or unit..."
              class="drmc-input pl-9"
            />
          </div>

          <select
            v-model="selectedDepartment"
            class="drmc-input"
          >
            <option
              v-for="department in departments"
              :key="department"
              :value="department"
            >
              {{ department }}
            </option>
          </select>

          <select
            v-model="selectedStatus"
            class="drmc-input"
            @change="page = 1; loadRecords()"
          >
            <option
              v-for="status in statuses"
              :key="status"
              :value="status"
            >
              {{ status }}
            </option>
          </select>
        </div>
      </div>

      <div v-if="isLoading" class="py-14 text-center">
        <RefreshCw :size="26" class="mx-auto mb-3 animate-spin text-brand-700" />
        <p class="text-sm font-medium text-slate-600">Loading PPMP records...</p>
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-sm">
          <thead class="border-b border-slate-200/80 bg-slate-50/80 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
            <tr>
              <th class="whitespace-nowrap px-5 py-3.5">Reference No.</th>
              <th class="whitespace-nowrap px-5 py-3.5">Unit</th>
              <th class="whitespace-nowrap px-5 py-3.5">Planning Year</th>
              <th class="whitespace-nowrap px-5 py-3.5 text-right">Items</th>
              <th class="whitespace-nowrap px-5 py-3.5 text-right">Estimated Budget</th>
              <th class="whitespace-nowrap px-5 py-3.5">Date Submitted</th>
              <th class="whitespace-nowrap px-5 py-3.5">Status</th>
              <th class="whitespace-nowrap px-5 py-3.5 text-center">Action</th>
            </tr>
          </thead>

          <tbody class="divide-y divide-slate-100">
            <tr
              v-for="record in filteredRecords"
              :key="record.ppmpId"
              class="transition-colors hover:bg-slate-50/80"
            >
              <td class="whitespace-nowrap px-5 py-4">
                <RouterLink
                  :to="{ name: 'ppmp-details', params: { id: record.ppmpId } }"
                  class="font-semibold text-brand-700 hover:underline"
                >
                  {{ record.ppmpReference }}
                </RouterLink>
              </td>

              <td class="whitespace-nowrap px-5 py-4 font-medium text-slate-800">
                {{ record.departmentName }}
              </td>

              <td class="whitespace-nowrap px-5 py-4 text-slate-600 tabular-nums">
                {{ record.planningYear }}
              </td>

              <td class="whitespace-nowrap px-5 py-4 text-right text-slate-600 tabular-nums">
                {{ record.itemCount }}
              </td>

              <td class="whitespace-nowrap px-5 py-4 text-right font-semibold text-slate-900 tabular-nums">
                {{ formatPHP(record.estimatedBudget) }}
              </td>

              <td class="whitespace-nowrap px-5 py-4 text-slate-600">
                {{ formatDatePH(record.submittedAt) }}
              </td>

              <td class="whitespace-nowrap px-5 py-4">
                <span
                  :class="[
                    'inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ring-1 ring-inset',
                    getPPMPStatusStyle(record.status).pill,
                  ]"
                >
                  <span :class="['h-1.5 w-1.5 rounded-full', getPPMPStatusStyle(record.status).dot]" />
                  {{ record.status }}
                </span>
              </td>

              <td class="whitespace-nowrap px-5 py-4">
                <div class="flex items-center justify-center gap-2">
                  <RouterLink
                    :to="{ name: 'ppmp-details', params: { id: record.ppmpId } }"
                    class="drmc-btn-secondary px-2.5 py-1.5 text-xs"
                  >
                    <Eye :size="14" />
                    View
                  </RouterLink>

                  <button
                    v-if="record.status === 'Approved by Unit Head'"
                    type="button"
                    :disabled="isProcessing"
                    class="drmc-btn-primary px-3 py-1.5 text-xs"
                    @click="startReview(record)"
                  >
                    <Play :size="13" />
                    Start Review
                  </button>

                  <RouterLink
                    v-else-if="record.status === 'Under Review'"
                    :to="{ name: 'procurement-edit-ppmp', params: { id: record.ppmpId } }"
                    class="inline-flex items-center gap-1.5 rounded-xl bg-blue-700 px-3 py-1.5 text-xs font-semibold text-white shadow-2xs transition hover:bg-blue-800"
                  >
                    <Pencil :size="13" />
                    Edit PPMP
                  </RouterLink>

                  <button
                    v-else-if="record.status === 'Updated'"
                    type="button"
                    :disabled="isProcessing"
                    class="inline-flex items-center gap-1.5 rounded-xl bg-purple-700 px-3 py-1.5 text-xs font-semibold text-white shadow-2xs transition hover:bg-purple-800 disabled:cursor-not-allowed disabled:opacity-50"
                    @click="completeReview(record)"
                  >
                    <CheckCircle :size="13" />
                    Complete Review
                  </button>
                </div>
              </td>
            </tr>

            <tr v-if="filteredRecords.length === 0">
              <td colspan="8" class="px-5 py-14 text-center">
                <ClipboardList :size="32" class="mx-auto mb-3 text-slate-300" />
                <p class="font-semibold text-slate-800">
                  No PPMP records found
                </p>
                <p class="mt-1 text-xs text-slate-500">
                  There are no matching PPMP records requiring Procurement action on this page.
                </p>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- PAGINATION -->
      <div class="flex flex-col items-start justify-between gap-3 border-t border-slate-100 bg-slate-50/50 px-5 py-3.5 sm:flex-row sm:items-center">
        <div class="text-xs text-slate-500 tabular-nums">
          Showing <span class="font-semibold text-slate-700">{{ filteredRecords.length }}</span> on this page ·
          <span class="font-semibold text-slate-700">{{ totalCount }}</span> total ·
          Page <span class="font-semibold text-slate-700">{{ page }}</span> of {{ Math.max(totalPages, 1) }}
        </div>

        <div class="flex items-center gap-2">
          <button
            type="button"
            :disabled="page <= 1 || isLoading"
            class="drmc-btn-secondary px-3 py-1.5 text-xs"
            @click="goToPage(page - 1)"
          >
            Previous
          </button>

          <button
            type="button"
            :disabled="page >= totalPages || isLoading"
            class="drmc-btn-secondary px-3 py-1.5 text-xs"
            @click="goToPage(page + 1)"
          >
            Next
          </button>
        </div>
      </div>
    </section>
  </div>
</template>