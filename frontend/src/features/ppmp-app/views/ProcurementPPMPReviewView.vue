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
const selectedDepartment = ref('All Departments')
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
  'All Departments',
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
    // Ask the server for the page of PPMPs in the three review
    // statuses. selectedStatus narrows to one of them if set.
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

// Client-side search + department filter, applied to the page.
const filteredRecords = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()

  return ppmpRecords.value.filter(record => {
    const matchesSearch =
      !query ||
      record.ppmpReference.toLowerCase().includes(query) ||
      record.departmentName.toLowerCase().includes(query)

    const matchesDepartment =
      selectedDepartment.value === 'All Departments' ||
      record.departmentName === selectedDepartment.value

    return matchesSearch && matchesDepartment
  })
})

// Card totals come from the server summary so they stay correct
// regardless of which page is visible.
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

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('en-PH', {
    style: 'currency',
    currency: 'PHP',
    maximumFractionDigits: 2,
  }).format(amount)
}

function formatDate(date: string | null) {
  if (!date) return '—'

  const parsedDate = new Date(date)

  if (Number.isNaN(parsedDate.getTime())) return '—'

  return parsedDate.toLocaleDateString('en-PH', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

function getStatusClass(status: string) {
  const classes: Record<string, string> = {
    'Approved by Unit Head': 'bg-blue-50 text-blue-700',
    'Under Review': 'bg-amber-50 text-amber-700',
    Updated: 'bg-purple-50 text-purple-700',
    Reviewed: 'bg-emerald-50 text-emerald-700',
  }

  return classes[status] ?? 'bg-slate-100 text-slate-700'
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
  <div class="space-y-6">
    <div>
      <div class="flex items-center gap-2">
        <ClipboardList :size="25" class="text-emerald-700" />

        <h1 class="text-2xl font-bold text-slate-900">
          Procurement PPMP Review
        </h1>
      </div>

      <p class="mt-1 text-sm text-slate-500">
        Review PPMPs approved by Unit Heads and monitor Procurement review progress.
      </p>
    </div>

    <div
      v-if="successMessage"
      class="rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700"
    >
      {{ successMessage }}
    </div>

    <div
      v-if="errorMessage"
      class="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"
    >
      {{ errorMessage }}
    </div>

    <!-- Summary cards (totals come from /summary, not from the page) -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
      <div class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <p class="text-sm text-slate-500">Total for Review</p>
          <ClipboardList :size="20" class="text-blue-600" />
        </div>

        <p class="mt-3 text-2xl font-bold text-slate-900">
          {{ totalForReview }}
        </p>

        <p class="mt-1 text-xs text-slate-500">
          Approved, under review, and updated
        </p>
      </div>

      <div class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <p class="text-sm text-slate-500">Awaiting Procurement</p>
          <FileCheck :size="20" class="text-blue-600" />
        </div>

        <p class="mt-3 text-2xl font-bold text-blue-700">
          {{ approvedCount }}
        </p>

        <p class="mt-1 text-xs text-slate-500">
          Approved by Unit Head
        </p>
      </div>

      <div class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <p class="text-sm text-slate-500">Under Review</p>
          <Clock :size="20" class="text-amber-600" />
        </div>

        <p class="mt-3 text-2xl font-bold text-amber-600">
          {{ underReviewCount }}
        </p>

        <p class="mt-1 text-xs text-slate-500">
          Currently being reviewed
        </p>
      </div>

      <div class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <p class="text-sm text-slate-500">Updated</p>
          <Pencil :size="20" class="text-purple-600" />
        </div>

        <p class="mt-3 text-2xl font-bold text-purple-700">
          {{ updatedCount }}
        </p>

        <p class="mt-1 text-xs text-slate-500">
          Awaiting review completion
        </p>
      </div>
    </div>

    <!-- Review records -->
    <section class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
      <div class="mb-5 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 class="font-semibold text-slate-900">
            PPMP Records for Procurement
          </h2>

          <p class="mt-1 text-xs text-slate-500">
            Start reviewing Unit Head-approved PPMPs, edit records under review, and complete updated reviews.
          </p>
        </div>

        <button
          type="button"
          @click="loadRecords"
          :disabled="isLoading || isProcessing"
          class="inline-flex items-center gap-2 rounded-lg border border-slate-200 px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
        >
          <RefreshCw :size="15" />
          Refresh
        </button>
      </div>

      <div class="grid grid-cols-1 gap-3 md:grid-cols-3">
        <div class="relative">
          <Search
            :size="18"
            class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
          />

          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search current page..."
            class="w-full rounded-lg border border-slate-300 py-2.5 pl-10 pr-3 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
          />
        </div>

        <select
          v-model="selectedDepartment"
          class="rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
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
          class="rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
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

      <div v-if="isLoading" class="py-12 text-center text-sm text-slate-500">
        Loading PPMP records...
      </div>

      <div v-else class="mt-5 overflow-x-auto">
        <table class="w-full text-left text-sm">
          <thead class="bg-slate-50 text-xs uppercase text-slate-500">
            <tr>
              <th class="whitespace-nowrap px-4 py-3 font-semibold">Reference No.</th>
              <th class="whitespace-nowrap px-4 py-3 font-semibold">Department</th>
              <th class="whitespace-nowrap px-4 py-3 font-semibold">Planning Year</th>
              <th class="whitespace-nowrap px-4 py-3 font-semibold">Items</th>
              <th class="whitespace-nowrap px-4 py-3 font-semibold">Estimated Budget</th>
              <th class="whitespace-nowrap px-4 py-3 font-semibold">Date Submitted</th>
              <th class="whitespace-nowrap px-4 py-3 font-semibold">Status</th>
              <th class="whitespace-nowrap px-4 py-3 text-center font-semibold">Action</th>
            </tr>
          </thead>

          <tbody class="divide-y divide-slate-100">
            <tr
              v-for="record in filteredRecords"
              :key="record.ppmpId"
              class="hover:bg-slate-50"
            >
              <td class="whitespace-nowrap px-4 py-4">
                <RouterLink
                  :to="{ name: 'ppmp-details', params: { id: record.ppmpId } }"
                  class="font-medium text-emerald-700 hover:underline"
                >
                  {{ record.ppmpReference }}
                </RouterLink>
              </td>

              <td class="whitespace-nowrap px-4 py-4 text-slate-600">
                {{ record.departmentName }}
              </td>

              <td class="px-4 py-4 text-slate-600">
                {{ record.planningYear }}
              </td>

              <td class="px-4 py-4 text-slate-600">
                {{ record.itemCount }}
              </td>

              <td class="whitespace-nowrap px-4 py-4 text-slate-600">
                {{ formatCurrency(record.estimatedBudget) }}
              </td>

              <td class="whitespace-nowrap px-4 py-4 text-slate-600">
                {{ formatDate(record.submittedAt) }}
              </td>

              <td class="whitespace-nowrap px-4 py-4">
                <span
                  :class="[
                    'rounded-full px-3 py-1 text-xs font-medium',
                    getStatusClass(record.status),
                  ]"
                >
                  {{ record.status }}
                </span>
              </td>

              <td class="whitespace-nowrap px-4 py-4">
                <div class="flex items-center justify-center gap-2">
                  <RouterLink
                    :to="{ name: 'ppmp-details', params: { id: record.ppmpId } }"
                    class="inline-flex items-center gap-1.5 rounded-lg border border-slate-200 px-3 py-2 text-xs font-medium text-slate-700 hover:border-emerald-600 hover:text-emerald-700"
                  >
                    <Eye :size="15" />
                    View
                  </RouterLink>

                  <button
                    v-if="record.status === 'Approved by Unit Head'"
                    type="button"
                    @click="startReview(record)"
                    :disabled="isProcessing"
                    class="inline-flex items-center gap-1.5 rounded-lg bg-emerald-700 px-3 py-2 text-xs font-medium text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    <Play :size="14" />
                    Start Review
                  </button>

                  <RouterLink
                    v-else-if="record.status === 'Under Review'"
                    :to="{ name: 'procurement-edit-ppmp', params: { id: record.ppmpId } }"
                    class="inline-flex items-center gap-1.5 rounded-lg bg-blue-700 px-3 py-2 text-xs font-medium text-white hover:bg-blue-800"
                  >
                    <Pencil :size="14" />
                    Edit PPMP
                  </RouterLink>

                  <button
                    v-else-if="record.status === 'Updated'"
                    type="button"
                    @click="completeReview(record)"
                    :disabled="isProcessing"
                    class="inline-flex items-center gap-1.5 rounded-lg bg-purple-700 px-3 py-2 text-xs font-medium text-white hover:bg-purple-800 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    <CheckCircle :size="14" />
                    Complete Review
                  </button>
                </div>
              </td>
            </tr>

            <tr v-if="filteredRecords.length === 0">
              <td colspan="8" class="px-4 py-12 text-center">
                <ClipboardList :size="32" class="mx-auto mb-3 text-slate-300" />

                <p class="font-medium text-slate-700">
                  No PPMP records found
                </p>

                <p class="mt-1 text-sm text-slate-500">
                  There are no matching PPMP records requiring Procurement action on this page.
                </p>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- PAGINATION -->
      <div class="mt-4 flex flex-col items-start justify-between gap-3 border-t border-slate-100 pt-4 sm:flex-row sm:items-center">
        <div class="text-xs text-slate-500">
          Showing
          {{ filteredRecords.length }} on this page ·
          {{ totalCount }} total ·
          Page {{ page }} of {{ Math.max(totalPages, 1) }}
        </div>

        <div class="flex items-center gap-2">
          <button
            type="button"
            :disabled="page <= 1 || isLoading"
            class="rounded-lg border border-slate-300 px-3 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
            @click="goToPage(page - 1)"
          >
            Previous
          </button>

          <button
            type="button"
            :disabled="page >= totalPages || isLoading"
            class="rounded-lg border border-slate-300 px-3 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
            @click="goToPage(page + 1)"
          >
            Next
          </button>
        </div>
      </div>
    </section>

    <p class="text-xs text-slate-400">
      Records are retrieved from the DRMC Procurement API.
    </p>
  </div>
</template>