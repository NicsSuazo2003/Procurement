<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { Search, Plus, ClipboardList, ChevronLeft, ChevronRight } from '@lucide/vue'
import { RouterLink } from 'vue-router'

import { useAuthStore } from '@/stores/auth'
import { ppmpApiService } from '../services/ppmpApiService'

import type { PPMPStatus } from '../models/ppmp.types'
import type { PPMPListApi } from '../models/ppmp-api.types'

interface PPMPListRecord {
  id: number
  reference: string
  department: string
  planningYear: number
  dateSubmitted: string
  totalItems: number
  estimatedBudget: number
  status: PPMPStatus
}

const authStore = useAuthStore()

// Filters
const searchQuery = ref('')
const selectedDepartment = ref('All Departments')
const selectedStatus = ref('All Statuses')

// Page state
const page = ref(1)
const pageSize = ref(25)
const totalCount = ref(0)
const totalPages = ref(0)

// API records and page state
const ppmpRecords = ref<PPMPListRecord[]>([])
const isLoading = ref(false)
const errorMessage = ref('')

// Summary cards
const summary = ref({
  total: 0,
  awaitingAction: 0,
  completedReview: 0,
})

// Role permissions
const isEndUser = computed(() =>
  authStore.hasRole('End-user'),
)

const isUnitHead = computed(() =>
  authStore.hasRole('Unit Head'),
)

const canViewAllDepartments = computed(() =>
  authStore.hasAnyRole([
    'Procurement Staff',
    'System Administrator',
  ]),
)

// Available statuses
const statuses: Array<'All Statuses' | PPMPStatus> = [
  'All Statuses',
  'Draft',
  'For Unit Head Approval',
  'Approved by Unit Head',
  'Under Review',
  'Updated',
  'Reviewed',
  'Included in APP',
]

// Department options are based on records on the current page.
const departments = computed(() => {
  const departmentNames = [
    ...new Set(
      ppmpRecords.value.map(record => record.department),
    ),
  ].sort((a, b) => a.localeCompare(b))

  return ['All Departments', ...departmentNames]
})

function mapApiRecord(record: PPMPListApi): PPMPListRecord {
  return {
    id: record.ppmpId,
    reference: record.ppmpReference,
    department: record.departmentName,
    planningYear: record.planningYear,
    dateSubmitted: record.submittedAt
      ? formatDate(record.submittedAt)
      : 'Not submitted',
    totalItems: record.itemCount,
    estimatedBudget: record.estimatedBudget,
    status: record.status as PPMPStatus,
  }
}

async function loadSummary(): Promise<void> {
  try {
    const data = await ppmpApiService.getSummary()
    summary.value = {
      total: data.total,
      awaitingAction: data.awaitingAction,
      completedReview: data.completedReview,
    }
  } catch (error) {
    console.error('Unable to load PPMP summary:', error)
  }
}

async function loadRecords(): Promise<void> {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const response = await ppmpApiService.getPaged({
      page: page.value,
      pageSize: pageSize.value,
      status:
        selectedStatus.value === 'All Statuses'
          ? undefined
          : selectedStatus.value,
    })

    ppmpRecords.value = response.items.map(mapApiRecord)
    totalCount.value = response.totalCount
    totalPages.value = response.totalPages

    if (
      selectedDepartment.value !== 'All Departments' &&
      !departments.value.includes(selectedDepartment.value)
    ) {
      selectedDepartment.value = 'All Departments'
    }
  } catch (error) {
    console.error('Unable to load PPMP records:', error)

    errorMessage.value = error instanceof Error
      ? error.message
      : 'Unable to load PPMP records. Please try again.'
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  void Promise.all([loadSummary(), loadRecords()])
})

// Re-fetch when status or page changes.
watch([selectedStatus, page, pageSize], () => {
  void loadRecords()
})

function goToPage(next: number): void {
  if (next < 1 || next > totalPages.value) return
  page.value = next
}

// Client-side search + department filter applied to the current page.
const filteredRecords = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()

  return ppmpRecords.value.filter(record => {
    const matchesSearch =
      record.reference.toLowerCase().includes(query) ||
      record.department.toLowerCase().includes(query) ||
      String(record.planningYear).includes(query)

    const matchesDepartment =
      !canViewAllDepartments.value ||
      selectedDepartment.value === 'All Departments' ||
      record.department === selectedDepartment.value

    return matchesSearch && matchesDepartment
  })
})

function getStatusClass(status: PPMPStatus): string {
  const classes: Record<PPMPStatus, string> = {
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

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-PH', {
    style: 'currency',
    currency: 'PHP',
    maximumFractionDigits: 2,
  }).format(amount)
}

function formatDate(value: string): string {
  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return value
  }

  return new Intl.DateTimeFormat('en-PH', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  }).format(date)
}
</script>

<template>
  <div class="space-y-6">

    <!-- PAGE HEADING -->
    <div class="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
      <div>
        <div class="flex items-center gap-2">
          <ClipboardList :size="25" class="text-emerald-700" />

          <h1 class="text-2xl font-bold text-slate-900">
            PPMP Management
          </h1>
        </div>

        <p class="mt-1 text-sm text-slate-500">
          Manage and monitor Project Procurement Management Plans.
        </p>
      </div>

      <div class="flex flex-wrap items-center gap-2">
        <RouterLink
          v-if="isUnitHead"
          to="/unit-head/ppmp-approvals"
          class="inline-flex items-center justify-center gap-2 rounded-lg border border-emerald-700 px-4 py-2.5 text-sm font-medium text-emerald-700 hover:bg-emerald-50"
        >
          <ClipboardList :size="18" />
          Unit Head Approval Queue
        </RouterLink>

        <RouterLink
          v-if="isEndUser"
          to="/ppmp/create"
          class="inline-flex items-center justify-center gap-2 rounded-lg bg-emerald-700 px-4 py-2.5 text-sm font-medium text-white hover:bg-emerald-800"
        >
          <Plus :size="18" />
          New PPMP
        </RouterLink>
      </div>
    </div>

    <!-- SUMMARY -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
      <div class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <p class="text-sm text-slate-500">
          Total Records
        </p>

        <p class="mt-2 text-2xl font-bold text-slate-900">
          {{ summary.total }}
        </p>
      </div>

      <div class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <p class="text-sm text-slate-500">
          Awaiting Action
        </p>

        <p class="mt-2 text-2xl font-bold text-amber-600">
          {{ summary.awaitingAction }}
        </p>
      </div>

      <div class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <p class="text-sm text-slate-500">
          Reviewed / Included in APP
        </p>

        <p class="mt-2 text-2xl font-bold text-emerald-700">
          {{ summary.completedReview }}
        </p>
      </div>
    </div>

    <!-- PPMP RECORDS -->
    <section class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">

      <div class="mb-4">
        <h2 class="font-semibold text-slate-900">
          PPMP Records
        </h2>

        <p class="mt-1 text-xs text-slate-500">
          Search and filter the current page. Use the status filter
          or page controls to narrow results across all records.
        </p>
      </div>

      <!-- FILTERS -->
      <div
        :class="[
          'grid grid-cols-1 gap-3',
          canViewAllDepartments
            ? 'md:grid-cols-3'
            : 'md:grid-cols-2',
        ]"
      >
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
          v-if="canViewAllDepartments"
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

      <!-- TABLE -->
      <div class="mt-5 overflow-x-auto">
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
            </tr>
          </thead>

          <tbody class="divide-y divide-slate-100">
            <tr v-if="isLoading">
              <td colspan="7" class="px-4 py-12 text-center text-sm text-slate-500">
                Loading PPMP records...
              </td>
            </tr>

            <tr v-else-if="errorMessage">
              <td colspan="7" class="px-4 py-10 text-center">
                <p class="text-sm text-red-600">
                  {{ errorMessage }}
                </p>

                <button
                  type="button"
                  class="mt-3 rounded-lg bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-800"
                  @click="loadRecords"
                >
                  Try Again
                </button>
              </td>
            </tr>

            <template v-else>
              <tr
                v-for="record in filteredRecords"
                :key="record.id"
                class="hover:bg-slate-50"
              >
                <td class="whitespace-nowrap px-4 py-4">
                  <RouterLink
                    :to="{ name: 'ppmp-details', params: { id: record.id } }"
                    class="font-medium text-emerald-700 hover:underline"
                  >
                    {{ record.reference }}
                  </RouterLink>
                </td>

                <td class="whitespace-nowrap px-4 py-4 text-slate-600">
                  {{ record.department }}
                </td>

                <td class="px-4 py-4 text-slate-600">
                  {{ record.planningYear }}
                </td>

                <td class="px-4 py-4 text-slate-600">
                  {{ record.totalItems }}
                </td>

                <td class="whitespace-nowrap px-4 py-4 text-slate-600">
                  {{ formatCurrency(record.estimatedBudget) }}
                </td>

                <td class="whitespace-nowrap px-4 py-4 text-slate-600">
                  {{ record.dateSubmitted }}
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
              </tr>

              <tr v-if="filteredRecords.length === 0">
                <td colspan="7" class="px-4 py-12 text-center text-sm text-slate-500">
                  No PPMP records found on this page. Try changing the
                  status filter or navigating to another page.
                </td>
              </tr>
            </template>
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
            class="inline-flex items-center gap-1 rounded-lg border border-slate-300 px-3 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
            @click="goToPage(page - 1)"
          >
            <ChevronLeft :size="14" />
            Previous
          </button>

          <button
            type="button"
            :disabled="page >= totalPages || isLoading"
            class="inline-flex items-center gap-1 rounded-lg border border-slate-300 px-3 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
            @click="goToPage(page + 1)"
          >
            Next
            <ChevronRight :size="14" />
          </button>
        </div>
      </div>
    </section>
  </div>
</template>