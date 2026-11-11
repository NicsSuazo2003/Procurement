<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { Search, ClipboardCheck, Eye, RefreshCw } from '@lucide/vue'
import { RouterLink } from 'vue-router'

import { ppmpApiService } from '../services/ppmpApiService'
import type { PPMPListApi } from '../models/ppmp-api.types'

import { useUnitHeadApprovalsStore } from '@/stores/unit-head-approvals'

const PENDING_STATUS = 'For Unit Head Approval'

const searchQuery = ref('')
const records = ref<PPMPListApi[]>([])

const page = ref(1)
const pageSize = ref(25)
const totalCount = ref(0)
const totalPages = ref(0)

const isLoading = ref(false)
const errorMessage = ref('')

const unitHeadApprovals = useUnitHeadApprovalsStore()

const filteredRecords = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()

  if (!query) return records.value

  return records.value.filter(
    record =>
      record.ppmpReference.toLowerCase().includes(query) ||
      record.departmentName.toLowerCase().includes(query),
  )
})

const totalBudget = computed(() =>
  records.value.reduce(
    (total, record) => total + (Number(record.estimatedBudget) || 0),
    0,
  ),
)

async function loadPPMPs(): Promise<void> {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const [response, summary] = await Promise.all([
      ppmpApiService.getPaged({
        page: page.value,
        pageSize: pageSize.value,
        status: PENDING_STATUS,
      }),
      ppmpApiService.getSummary(),
    ])

    records.value = response.items
    totalCount.value = response.totalCount
    totalPages.value = response.totalPages

    // Badge reflects the true total, not the current page.
    unitHeadApprovals.setPendingCount(
      summary.byStatus[PENDING_STATUS] ?? 0,
    )
  } catch (error) {
    console.error('Unable to load Unit Head approval queue:', error)

    errorMessage.value = error instanceof Error
      ? error.message
      : 'Unable to load PPMP approval records.'

    records.value = []
    totalCount.value = 0
    totalPages.value = 0
  } finally {
    isLoading.value = false
  }
}

function goToPage(next: number): void {
  if (next < 1 || next > totalPages.value) return
  page.value = next
  void loadPPMPs()
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-PH', {
    style: 'currency',
    currency: 'PHP',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount)
}

function formatDate(date?: string | null): string {
  if (!date) return 'Not recorded'

  const parsedDate = new Date(date)

  if (Number.isNaN(parsedDate.getTime())) {
    return date
  }

  return new Intl.DateTimeFormat('en-PH', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  }).format(parsedDate)
}

onMounted(() => {
  void loadPPMPs()
})
</script>

<template>
  <div class="space-y-6">
    <div class="flex flex-wrap items-start justify-between gap-4">
      <div>
        <div class="flex items-center gap-2">
          <ClipboardCheck :size="25" class="text-emerald-700" />

          <h1 class="text-2xl font-bold text-slate-900">
            Unit Head Approval Queue
          </h1>
        </div>

        <p class="mt-1 text-sm text-slate-500">
          Review PPMP submissions from your department and approve them before
          they proceed to Procurement.
        </p>
      </div>

      <button
        type="button"
        @click="loadPPMPs"
        :disabled="isLoading"
        class="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
      >
        <RefreshCw
          :size="16"
          :class="{ 'animate-spin': isLoading }"
        />
        Refresh
      </button>
    </div>

    <!-- SUMMARY -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
      <div class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <p class="text-sm text-slate-500">
          Awaiting Your Approval
        </p>

        <p class="mt-2 text-2xl font-bold text-amber-600">
          {{ totalCount }}
        </p>
      </div>

      <div class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <p class="text-sm text-slate-500">
          Total Estimated Budget (current page)
        </p>

        <p class="mt-2 text-2xl font-bold text-slate-900">
          {{ formatCurrency(totalBudget) }}
        </p>
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
        @click="loadPPMPs"
        class="mt-2 text-sm font-semibold text-red-700 hover:underline"
      >
        Try Again
      </button>
    </div>

    <!-- APPROVAL QUEUE -->
    <section class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
      <div class="mb-4">
        <h2 class="font-semibold text-slate-900">
          PPMP Submissions for Approval
        </h2>

        <p class="mt-1 text-xs text-slate-500">
          Only PPMPs submitted for Unit Head approval are displayed here.
        </p>
      </div>

      <!-- SEARCH -->
      <div class="relative max-w-md">
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

      <!-- LOADING -->
      <div
        v-if="isLoading"
        class="py-12 text-center"
      >
        <RefreshCw
          :size="28"
          class="mx-auto mb-3 animate-spin text-emerald-700"
        />

        <p class="text-sm text-slate-500">
          Loading PPMP approval records...
        </p>
      </div>

      <!-- TABLE -->
      <div
        v-else
        class="mt-5 overflow-x-auto"
      >
        <table class="w-full text-left text-sm">
          <thead class="bg-slate-50 text-xs uppercase text-slate-500">
            <tr>
              <th class="whitespace-nowrap px-4 py-3 font-semibold">Reference No.</th>
              <th class="whitespace-nowrap px-4 py-3 font-semibold">Department</th>
              <th class="whitespace-nowrap px-4 py-3 font-semibold">Planning Year</th>
              <th class="whitespace-nowrap px-4 py-3 font-semibold">Items</th>
              <th class="whitespace-nowrap px-4 py-3 font-semibold">Estimated Budget</th>
              <th class="whitespace-nowrap px-4 py-3 font-semibold">Submitted to Unit Head</th>
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
              <td class="whitespace-nowrap px-4 py-4 font-medium text-emerald-700">
                {{ record.ppmpReference }}
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
                <span class="rounded-full bg-blue-50 px-3 py-1 text-xs font-medium text-blue-700">
                  {{ record.status }}
                </span>
              </td>

              <td class="px-4 py-4 text-center">
                <RouterLink
                  :to="{ name: 'ppmp-details', params: { id: record.ppmpId } }"
                  class="inline-flex items-center gap-2 rounded-lg bg-emerald-700 px-3 py-2 text-xs font-medium text-white hover:bg-emerald-800"
                >
                  <Eye :size="15" />
                  Review
                </RouterLink>
              </td>
            </tr>

            <tr v-if="filteredRecords.length === 0">
              <td colspan="8" class="px-4 py-12 text-center">
                <ClipboardCheck :size="32" class="mx-auto mb-3 text-slate-300" />

                <p class="font-medium text-slate-700">
                  {{ searchQuery
                    ? 'No matching PPMP submissions found'
                    : 'No PPMP submissions awaiting approval'
                  }}
                </p>

                <p class="mt-1 text-xs text-slate-500">
                  PPMPs submitted by End-user Departments will appear here.
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
  </div>
</template>