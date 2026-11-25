<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { Search, ClipboardCheck, Eye, RefreshCw, Clock, Wallet } from '@lucide/vue'
import { RouterLink } from 'vue-router'

import { ppmpApiService } from '../services/ppmpApiService'
import type { PPMPListApi } from '../models/ppmp-api.types'

import { useUnitHeadApprovalsStore } from '@/stores/unit-head-approvals'
import { formatDatePH, formatPHP, getPPMPStatusStyle } from '@/shared/utilities/ppmpUiHelpers'

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

onMounted(() => {
  void loadPPMPs()
})
</script>

<template>
  <div class="mx-auto max-w-7xl space-y-6">
    <!-- PAGE HEADER -->
    <div class="flex flex-wrap items-start justify-between gap-4">
      <div class="flex items-start gap-3">
        <div class="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/20">
          <ClipboardCheck :size="22" />
        </div>

        <div>
          <h1 class="text-2xl font-bold tracking-tight text-slate-900">
            Unit Head Approval Queue
          </h1>
          <p class="mt-1 text-sm text-slate-500">
            Review PPMP submissions from your unit and approve them before they proceed to Procurement.
          </p>
        </div>
      </div>

      <button
        type="button"
        :disabled="isLoading"
        class="drmc-btn-secondary"
        @click="loadPPMPs"
      >
        <RefreshCw :size="16" :class="{ 'animate-spin': isLoading }" />
        Refresh
      </button>
    </div>

    <!-- SUMMARY CARDS -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
      <div class="drmc-card flex items-center justify-between p-5">
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
            Awaiting Your Approval
          </p>
          <p class="mt-2 text-3xl font-bold tracking-tight text-amber-600 tabular-nums">
            {{ totalCount }}
          </p>
          <p class="mt-1 text-xs text-slate-500">
            Total pending unit submissions
          </p>
        </div>

        <div class="flex h-12 w-12 items-center justify-center rounded-xl bg-amber-50 text-amber-600 ring-1 ring-inset ring-amber-600/20">
          <Clock :size="22" />
        </div>
      </div>

      <div class="drmc-card flex items-center justify-between p-5">
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
            Estimated Budget (Current Page)
          </p>
          <p class="mt-2 text-2xl font-bold tracking-tight text-slate-900 tabular-nums sm:text-3xl">
            {{ formatPHP(totalBudget) }}
          </p>
          <p class="mt-1 text-xs text-slate-500">
            Combined value of displayed PPMPs
          </p>
        </div>

        <div class="flex h-12 w-12 items-center justify-center rounded-xl bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/20">
          <Wallet :size="22" />
        </div>
      </div>
    </div>

    <!-- ERROR MESSAGE -->
    <div
      v-if="errorMessage"
      role="alert"
      class="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-red-200 bg-red-50 px-4 py-3.5 text-sm text-red-800"
    >
      <p class="font-medium">{{ errorMessage }}</p>

      <button
        type="button"
        class="rounded-lg bg-red-100 px-3 py-1.5 text-xs font-semibold text-red-800 transition hover:bg-red-200"
        @click="loadPPMPs"
      >
        Try Again
      </button>
    </div>

    <!-- APPROVAL QUEUE -->
    <section class="drmc-card overflow-hidden">
      <div class="flex flex-col justify-between gap-4 border-b border-slate-200/80 p-5 sm:flex-row sm:items-center">
        <div>
          <h2 class="text-base font-semibold text-slate-900">
            PPMP Submissions for Approval
          </h2>
          <p class="mt-0.5 text-xs text-slate-500">
            Only PPMPs submitted for Unit Head approval are displayed here.
          </p>
        </div>

        <!-- SEARCH -->
        <div class="relative w-full sm:max-w-xs">
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
      </div>

      <!-- LOADING -->
      <div v-if="isLoading" class="py-14 text-center">
        <RefreshCw :size="26" class="mx-auto mb-3 animate-spin text-brand-700" />
        <p class="text-sm font-medium text-slate-600">
          Loading PPMP approval records...
        </p>
      </div>

      <!-- TABLE -->
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-sm">
          <thead class="border-b border-slate-200/80 bg-slate-50/80 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
            <tr>
              <th class="whitespace-nowrap px-5 py-3.5">Reference No.</th>
              <th class="whitespace-nowrap px-5 py-3.5">Unit</th>
              <th class="whitespace-nowrap px-5 py-3.5">Planning Year</th>
              <th class="whitespace-nowrap px-5 py-3.5 text-right">Items</th>
              <th class="whitespace-nowrap px-5 py-3.5 text-right">Estimated Budget</th>
              <th class="whitespace-nowrap px-5 py-3.5">Submitted Date</th>
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
              <td class="whitespace-nowrap px-5 py-4 font-semibold text-brand-700">
                <RouterLink
                  :to="{ name: 'ppmp-details', params: { id: record.ppmpId } }"
                  class="hover:underline"
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
                {{ formatDatePH(record.submittedAt, { fallback: 'Not recorded' }) }}
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

              <td class="whitespace-nowrap px-5 py-4 text-center">
                <RouterLink
                  :to="{ name: 'ppmp-details', params: { id: record.ppmpId } }"
                  class="drmc-btn-primary px-3 py-1.5 text-xs"
                >
                  <Eye :size="14" />
                  Review
                </RouterLink>
              </td>
            </tr>

            <tr v-if="filteredRecords.length === 0">
              <td colspan="8" class="px-5 py-14 text-center">
                <ClipboardCheck :size="32" class="mx-auto mb-3 text-slate-300" />
                <p class="font-semibold text-slate-800">
                  {{ searchQuery
                    ? 'No matching PPMP submissions found'
                    : 'No PPMP submissions awaiting approval'
                  }}
                </p>
                <p class="mt-1 text-xs text-slate-500">
                  PPMPs submitted by end-user units will appear here.
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