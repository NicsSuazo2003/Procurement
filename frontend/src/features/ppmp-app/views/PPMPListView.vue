<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import {
  Search,
  Plus,
  ClipboardList,
  ChevronLeft,
  ChevronRight,
  Clock,
  CheckCircle2,
  X,
  RotateCcw,
  ArrowUpRight,
} from '@lucide/vue'

import { useAuthStore } from '@/stores/auth'
import { ppmpApiService } from '../services/ppmpApiService'
import { formatDatePH, formatPHP, getPPMPStatusStyle } from '@/shared/utilities/ppmpUiHelpers'

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

const router = useRouter()
const authStore = useAuthStore()

// Filters
const searchQuery = ref('')
const selectedDepartment = ref('All Units')
const selectedStatus = ref<'All Statuses' | PPMPStatus>('All Statuses')

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
const isEndUser = computed(() => authStore.hasRole('End-user'))
const isUnitHead = computed(() => authStore.hasRole('Unit Head'))
const canViewAllDepartments = computed(() =>
  authStore.hasAnyRole(['Procurement Staff', 'System Administrator']),
)

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

const departments = computed(() => {
  const departmentNames = [
    ...new Set(ppmpRecords.value.map((record) => record.department)),
  ].sort((a, b) => a.localeCompare(b))

  return ['All Units', ...departmentNames]
})

const hasActiveFilters = computed(
  () =>
    searchQuery.value.trim().length > 0 ||
    selectedDepartment.value !== 'All Units' ||
    selectedStatus.value !== 'All Statuses',
)

function resetFilters(): void {
  searchQuery.value = ''
  selectedDepartment.value = 'All Units'
  selectedStatus.value = 'All Statuses'
  page.value = 1
}

function applySummaryQuickFilter(type: 'all' | 'awaiting' | 'completed'): void {
  searchQuery.value = ''
  selectedDepartment.value = 'All Units'
  page.value = 1

  if (type === 'all') {
    selectedStatus.value = 'All Statuses'
  } else if (type === 'awaiting') {
    selectedStatus.value = isUnitHead.value ? 'For Unit Head Approval' : 'Under Review'
  } else {
    selectedStatus.value = 'Reviewed'
  }
}

function mapApiRecord(record: PPMPListApi): PPMPListRecord {
  return {
    id: record.ppmpId,
    reference: record.ppmpReference,
    department: record.departmentName,
    planningYear: record.planningYear,
    dateSubmitted: record.submittedAt
      ? formatDatePH(record.submittedAt)
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
      selectedDepartment.value !== 'All Units' &&
      !departments.value.includes(selectedDepartment.value)
    ) {
      selectedDepartment.value = 'All Units'
    }
  } catch (error) {
    console.error('Unable to load PPMP records:', error)
    errorMessage.value =
      error instanceof Error
        ? error.message
        : 'Unable to load PPMP records. Please try again.'
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  void Promise.all([loadSummary(), loadRecords()])
})

watch([selectedStatus, pageSize], () => {
  if (page.value !== 1) {
    page.value = 1
    return
  }
  void loadRecords()
})

watch(page, () => {
  void loadRecords()
})

function goToPage(next: number): void {
  if (next < 1 || next > totalPages.value) return
  page.value = next
}

function openRecord(id: number): void {
  void router.push({ name: 'ppmp-details', params: { id } })
}

const filteredRecords = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()

  return ppmpRecords.value.filter((record) => {
    const matchesSearch =
      record.reference.toLowerCase().includes(query) ||
      record.department.toLowerCase().includes(query) ||
      String(record.planningYear).includes(query)

    const matchesDepartment =
      !canViewAllDepartments.value ||
      selectedDepartment.value === 'All Units' ||
      record.department === selectedDepartment.value

    return matchesSearch && matchesDepartment
  })
})
</script>

<template>
  <div class="mx-auto max-w-7xl space-y-6">
    <!-- PAGE HEADING -->
    <div class="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
      <div class="flex items-start gap-3">
        <div class="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/20">
          <ClipboardList :size="22" />
        </div>

        <div>
          <h1 class="text-2xl font-bold tracking-tight text-slate-900">
            Procurement Requests
          </h1>
          <p class="mt-1 text-sm text-slate-500">
            Manage and monitor Project Procurement Management Plans.
          </p>
        </div>
      </div>

      <div class="flex flex-wrap items-center gap-2.5">
        <RouterLink
          v-if="isUnitHead"
          to="/unit-head/ppmp-approvals"
          class="drmc-btn-secondary w-full sm:w-auto"
        >
          <ClipboardList :size="17" class="text-brand-700" />
          Unit Head Approval Queue
        </RouterLink>

        <RouterLink
          v-if="isEndUser"
          to="/ppmp/create"
          class="drmc-btn-primary w-full sm:w-auto"
        >
          <Plus :size="17" />
          New PPMP
        </RouterLink>
      </div>
    </div>

    <!-- INTERACTIVE SUMMARY CARDS -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
      <button
        type="button"
        class="drmc-card flex items-center justify-between p-5 text-left transition hover:border-slate-300 hover:shadow-sm"
        :class="selectedStatus === 'All Statuses' ? 'ring-2 ring-brand-600/25' : ''"
        @click="applySummaryQuickFilter('all')"
      >
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
            Total Records
          </p>
          <p class="mt-2 text-2xl font-bold tracking-tight text-slate-900 tabular-nums sm:text-3xl">
            {{ summary.total }}
          </p>
        </div>
        <div class="flex h-11 w-11 items-center justify-center rounded-xl bg-slate-100 text-slate-600">
          <ClipboardList :size="20" />
        </div>
      </button>

      <button
        type="button"
        class="drmc-card flex items-center justify-between p-5 text-left transition hover:border-amber-300 hover:shadow-sm"
        @click="applySummaryQuickFilter('awaiting')"
      >
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
            Awaiting Action
          </p>
          <p class="mt-2 text-2xl font-bold tracking-tight text-amber-600 tabular-nums sm:text-3xl">
            {{ summary.awaitingAction }}
          </p>
        </div>
        <div class="flex h-11 w-11 items-center justify-center rounded-xl bg-amber-50 text-amber-600 ring-1 ring-inset ring-amber-600/20">
          <Clock :size="20" />
        </div>
      </button>

      <button
        type="button"
        class="drmc-card flex items-center justify-between p-5 text-left transition hover:border-brand-300 hover:shadow-sm"
        @click="applySummaryQuickFilter('completed')"
      >
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
            Reviewed / Included in APP
          </p>
          <p class="mt-2 text-2xl font-bold tracking-tight text-brand-700 tabular-nums sm:text-3xl">
            {{ summary.completedReview }}
          </p>
        </div>
        <div class="flex h-11 w-11 items-center justify-center rounded-xl bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/20">
          <CheckCircle2 :size="20" />
        </div>
      </button>
    </div>

    <!-- PPMP RECORDS -->
    <section class="drmc-card overflow-hidden">
      <div class="border-b border-slate-200/80 p-5">
        <div class="flex flex-wrap items-center justify-between gap-2">
          <div>
            <h2 class="text-base font-semibold text-slate-900">
              PPMP Records
            </h2>
            <p class="mt-0.5 text-xs text-slate-500">
              Click any row to inspect details, allocations, or take workflow actions.
            </p>
          </div>

          <button
            v-if="hasActiveFilters"
            type="button"
            class="inline-flex items-center gap-1.5 rounded-lg border border-slate-200 bg-slate-50 px-3 py-1.5 text-xs font-semibold text-slate-600 transition hover:bg-slate-100 hover:text-slate-900"
            @click="resetFilters"
          >
            <RotateCcw :size="13" />
            Reset filters
          </button>
        </div>

        <!-- FILTERS -->
        <div
          :class="[
            'mt-4 grid grid-cols-1 gap-3',
            canViewAllDepartments ? 'md:grid-cols-3' : 'md:grid-cols-2',
          ]"
        >
          <div class="relative">
            <Search
              :size="16"
              class="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400"
            />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Search reference, unit, or year..."
              class="drmc-input pl-10 pr-9"
            />
            <button
              v-if="searchQuery"
              type="button"
              class="absolute right-2.5 top-1/2 -translate-y-1/2 rounded-md p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-600"
              title="Clear search"
              @click="searchQuery = ''"
            >
              <X :size="14" />
            </button>
          </div>

          <select
            v-if="canViewAllDepartments"
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

      <!-- TABLE -->
      <div class="overflow-x-auto">
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
              <th class="w-10 px-3 py-3.5"></th>
            </tr>
          </thead>

          <tbody class="divide-y divide-slate-100">
            <tr v-if="isLoading">
              <td colspan="8" class="px-5 py-14 text-center text-sm font-medium text-slate-500">
                Loading PPMP records...
              </td>
            </tr>

            <tr v-else-if="errorMessage">
              <td colspan="8" role="alert" class="px-5 py-10 text-center">
                <p class="text-sm font-medium text-red-700">
                  {{ errorMessage }}
                </p>

                <button
                  type="button"
                  class="drmc-btn-primary mt-3 text-xs"
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
                class="group cursor-pointer transition-colors hover:bg-brand-50/35"
                @click="openRecord(record.id)"
              >
                <td class="whitespace-nowrap px-5 py-4">
                  <RouterLink
                    :to="{ name: 'ppmp-details', params: { id: record.id } }"
                    class="font-semibold text-brand-700 group-hover:underline"
                    @click.stop
                  >
                    {{ record.reference }}
                  </RouterLink>
                </td>

                <td class="whitespace-nowrap px-5 py-4 font-medium text-slate-800">
                  {{ record.department }}
                </td>

                <td class="whitespace-nowrap px-5 py-4 text-slate-600 tabular-nums">
                  FY {{ record.planningYear }}
                </td>

                <td class="whitespace-nowrap px-5 py-4 text-right text-slate-600 tabular-nums">
                  {{ record.totalItems }}
                </td>

                <td class="whitespace-nowrap px-5 py-4 text-right font-semibold text-slate-900 tabular-nums">
                  {{ formatPHP(record.estimatedBudget) }}
                </td>

                <td class="whitespace-nowrap px-5 py-4 text-slate-600">
                  {{ record.dateSubmitted }}
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

                <td class="whitespace-nowrap px-3 py-4 text-right text-slate-400">
                  <ArrowUpRight :size="16" class="inline transition-transform group-hover:translate-x-0.5 group-hover:-translate-y-0.5 group-hover:text-brand-700" />
                </td>
              </tr>

              <tr v-if="filteredRecords.length === 0">
                <td colspan="8" class="px-5 py-14 text-center">
                  <p class="text-sm font-semibold text-slate-700">
                    No matching PPMP records found
                  </p>
                  <p class="mt-1 text-xs text-slate-500">
                    Try adjusting your search query, status filter, or page number.
                  </p>
                  <button
                    v-if="hasActiveFilters"
                    type="button"
                    class="drmc-btn-secondary mt-4 text-xs"
                    @click="resetFilters"
                  >
                    Reset all filters
                  </button>
                </td>
              </tr>
            </template>
          </tbody>
        </table>
      </div>

      <!-- PAGINATION -->
      <div class="flex flex-col items-start justify-between gap-3 border-t border-slate-100 bg-slate-50/50 px-5 py-3.5 sm:flex-row sm:items-center">
        <div class="flex flex-wrap items-center gap-3 text-xs text-slate-500 tabular-nums">
          <span>
            Showing <span class="font-semibold text-slate-700">{{ filteredRecords.length }}</span> on this page ·
            <span class="font-semibold text-slate-700">{{ totalCount }}</span> total ·
            Page <span class="font-semibold text-slate-700">{{ page }}</span> of {{ Math.max(totalPages, 1) }}
          </span>

          <select
            v-model.number="pageSize"
            aria-label="Rows per page"
            class="rounded-lg border border-slate-200 bg-white px-2 py-1 text-xs font-medium text-slate-700"
          >
            <option :value="25">25 / page</option>
            <option :value="50">50 / page</option>
            <option :value="100">100 / page</option>
          </select>
        </div>

        <div class="flex items-center gap-2">
          <button
            type="button"
            :disabled="page <= 1 || isLoading"
            class="drmc-btn-secondary px-3 py-1.5 text-xs"
            @click="goToPage(page - 1)"
          >
            <ChevronLeft :size="14" />
            Previous
          </button>

          <button
            type="button"
            :disabled="page >= totalPages || isLoading"
            class="drmc-btn-secondary px-3 py-1.5 text-xs"
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