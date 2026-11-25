<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import {
  ClipboardList,
  Clock3,
  CheckCircle2,
  AlertCircle,
  RefreshCw,
  ArrowUpRight,
  ChevronRight,
  Calendar,
  Building2,
  FileText,
} from '@lucide/vue'

import { ppmpApiService } from '@/features/ppmp-app/services/ppmpApiService'
import type { PPMPListApi } from '@/features/ppmp-app/models/ppmp-api.types'

const router = useRouter()

const ppmpRecords = ref<PPMPListApi[]>([])
const summary = ref({
  total: 0,
  awaitingAction: 0,
  completedReview: 0,
  byStatus: {} as Record<string, number>,
})

const isLoading = ref(true)
const isRefreshing = ref(false)
const errorMessage = ref('')
const lastUpdated = ref<Date | null>(null)

function normalizeStatus(status: string): string {
  return status.trim().toLowerCase()
}

const completionRate = computed(() => {
  if (!summary.value.total) return 0
  return Math.round((summary.value.completedReview / summary.value.total) * 100)
})

const summaryCards = computed(() => [
  {
    title: 'Total PPMPs',
    value: summary.value.total,
    subtitle: 'Across accessible planning years',
    badge: null,
    icon: ClipboardList,
    iconBg: 'bg-blue-50 text-blue-600 ring-blue-600/10',
    accentBorder: 'hover:border-blue-300',
  },
  {
    title: 'Pending Review',
    value: summary.value.awaitingAction,
    subtitle: 'Awaiting or undergoing Procurement review',
    badge: summary.value.awaitingAction > 0 ? 'Action needed' : null,
    badgeColor: 'bg-amber-50 text-amber-700 ring-amber-600/20',
    icon: Clock3,
    iconBg: 'bg-amber-50 text-amber-600 ring-amber-600/10',
    accentBorder: 'hover:border-amber-300',
  },
  {
    title: 'Reviewed',
    value: summary.value.completedReview,
    subtitle: `${completionRate.value}% of total submissions reviewed`,
    badge: null,
    icon: CheckCircle2,
    iconBg: 'bg-emerald-50 text-emerald-600 ring-emerald-600/10',
    accentBorder: 'hover:border-emerald-300',
  },
  {
    title: 'For Reconsideration',
    value: null,
    subtitle: 'Returned items requiring unit revision',
    badge: 'Coming Soon',
    badgeColor: 'bg-slate-100 text-slate-600 ring-slate-500/20',
    icon: AlertCircle,
    iconBg: 'bg-slate-100 text-slate-500 ring-slate-500/10',
    accentBorder: 'border-dashed border-slate-200',
    isDisabled: true,
  },
])

const recentPPMPs = computed(() => ppmpRecords.value)

function formatDate(date: string | null): string {
  if (!date) return '—'
  const parsedDate = new Date(date)
  if (Number.isNaN(parsedDate.getTime())) return '—'

  return parsedDate.toLocaleDateString('en-PH', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

function formatTime(date: Date | null): string {
  if (!date) return ''
  return date.toLocaleTimeString('en-PH', {
    hour: 'numeric',
    minute: '2-digit',
  })
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-PH', {
    style: 'currency',
    currency: 'PHP',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount)
}

function getStatusMeta(status: string): { pill: string; dot: string } {
  switch (normalizeStatus(status)) {
    case 'draft':
      return {
        pill: 'bg-slate-50 text-slate-700 ring-slate-600/20',
        dot: 'bg-slate-400',
      }
    case 'for unit head approval':
      return {
        pill: 'bg-amber-50 text-amber-800 ring-amber-600/20',
        dot: 'bg-amber-500',
      }
    case 'approved by unit head':
      return {
        pill: 'bg-blue-50 text-blue-700 ring-blue-600/20',
        dot: 'bg-blue-500',
      }
    case 'under review':
      return {
        pill: 'bg-purple-50 text-purple-700 ring-purple-600/20',
        dot: 'bg-purple-500',
      }
    case 'updated':
      return {
        pill: 'bg-orange-50 text-orange-700 ring-orange-600/20',
        dot: 'bg-orange-500',
      }
    case 'reviewed':
      return {
        pill: 'bg-emerald-50 text-emerald-700 ring-emerald-600/20',
        dot: 'bg-emerald-500',
      }
    default:
      return {
        pill: 'bg-slate-50 text-slate-700 ring-slate-600/20',
        dot: 'bg-slate-400',
      }
  }
}

async function loadDashboardData(isManualRefresh = false): Promise<void> {
  if (isManualRefresh) {
    isRefreshing.value = true
  } else {
    isLoading.value = true
  }
  errorMessage.value = ''

  try {
    const [summaryData, recentPage] = await Promise.all([
      ppmpApiService.getSummary(),
      ppmpApiService.getPaged({ page: 1, pageSize: 5 }),
    ])

    summary.value = {
      total: summaryData.total,
      awaitingAction: summaryData.awaitingAction,
      completedReview: summaryData.completedReview,
      byStatus: summaryData.byStatus,
    }

    ppmpRecords.value = recentPage.items.filter(
      ppmp => ppmp.submittedAt !== null,
    )
    lastUpdated.value = new Date()
  } catch (error) {
    console.error('Failed to load dashboard PPMP records:', error)
    errorMessage.value =
      error instanceof Error
        ? error.message
        : 'Unable to load PPMP records. Please try again.'
    ppmpRecords.value = []
  } finally {
    isLoading.value = false
    isRefreshing.value = false
  }
}

function handleRowClick(ppmpId: string | number) {
  // Adjust route path to match your router configuration
  router.push({ name: 'ppmp-detail', params: { id: ppmpId } }).catch(() => {
    // Fallback if route name differs
  })
}

function handleViewAll() {
  router.push({ name: 'ppmp-list' }).catch(() => {})
}

onMounted(() => loadDashboardData(false))
</script>

<template>
  <div class="mx-auto max-w-7xl space-y-8 pb-8">
    <!-- PAGE HEADER -->
    <header class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h1 class="text-2xl font-bold tracking-tight text-slate-900">
          Procurement Planning Dashboard
        </h1>
        <p class="mt-1 text-sm text-slate-500">
          Monitor Project Procurement Management Plan (PPMP) submissions and review progress.
        </p>
      </div>

      <div class="flex items-center gap-3">
        <span
          v-if="lastUpdated && !isLoading"
          class="hidden text-xs text-slate-400 md:inline"
        >
          Updated {{ formatTime(lastUpdated) }}
        </span>

        <button
          type="button"
          :disabled="isLoading || isRefreshing"
          class="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-3.5 py-2 text-sm font-medium text-slate-700 shadow-xs transition hover:bg-slate-50 hover:text-slate-900 disabled:cursor-not-allowed disabled:opacity-50"
          @click="loadDashboardData(true)"
        >
          <RefreshCw
            :size="15"
            :class="{ 'animate-spin text-blue-600': isRefreshing }"
          />
          <span>Refresh</span>
        </button>

        <button
          type="button"
          class="inline-flex items-center gap-1.5 rounded-lg bg-slate-900 px-4 py-2 text-sm font-medium text-white shadow-xs transition hover:bg-slate-800"
          @click="handleViewAll"
        >
          <span>All PPMPs</span>
          <ArrowUpRight :size="16" />
        </button>
      </div>
    </header>

    <!-- SUMMARY KPI CARDS -->
    <section aria-label="PPMP Summary Metrics">
      <!-- Skeleton State -->
      <div
        v-if="isLoading"
        class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4"
      >
        <div
          v-for="n in 4"
          :key="n"
          class="animate-pulse rounded-xl border border-slate-200 bg-white p-5 shadow-xs"
        >
          <div class="flex items-start justify-between">
            <div class="space-y-3">
              <div class="h-4 w-24 rounded bg-slate-200" />
              <div class="h-8 w-16 rounded bg-slate-200" />
            </div>
            <div class="h-11 w-11 rounded-xl bg-slate-100" />
          </div>
          <div class="mt-4 h-3 w-40 rounded bg-slate-100" />
        </div>
      </div>

      <!-- Loaded State -->
      <div
        v-else
        class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4"
      >
        <div
          v-for="card in summaryCards"
          :key="card.title"
          :class="[
            'group relative flex flex-col justify-between rounded-xl border border-slate-200/90 bg-white p-5 shadow-xs transition-all duration-150',
            card.isDisabled ? 'bg-slate-50/50' : 'hover:-translate-y-0.5 hover:shadow-md',
            card.accentBorder,
          ]"
        >
          <div class="flex items-start justify-between gap-3">
            <div>
              <div class="flex items-center gap-2">
                <p class="text-sm font-medium text-slate-600">
                  {{ card.title }}
                </p>
              </div>

              <div class="mt-2.5 flex items-baseline gap-2.5">
                <p
                  v-if="card.value !== null"
                  class="text-3xl font-bold tracking-tight text-slate-900 tabular-nums"
                >
                  {{ card.value.toLocaleString('en-PH') }}
                </p>
                <span
                  v-else
                  class="inline-flex items-center rounded-md bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-500 ring-1 ring-inset ring-slate-500/15"
                >
                  Not Available
                </span>

                <span
                  v-if="card.badge && card.value !== null"
                  :class="[
                    'inline-flex items-center rounded-full px-2 py-0.5 text-[11px] font-medium ring-1 ring-inset',
                    card.badgeColor,
                  ]"
                >
                  {{ card.badge }}
                </span>
              </div>
            </div>

            <div
              :class="[
                'flex h-11 w-11 shrink-0 items-center justify-center rounded-xl ring-1 ring-inset transition-transform group-hover:scale-105',
                card.iconBg,
              ]"
            >
              <component :is="card.icon" :size="20" />
            </div>
          </div>

          <p class="mt-4 border-t border-slate-100 pt-3 text-xs text-slate-500">
            {{ card.subtitle }}
          </p>
        </div>
      </div>
    </section>

    <!-- RECENT PPMP SUBMISSIONS -->
    <section class="overflow-hidden rounded-xl border border-slate-200/90 bg-white shadow-xs">
      <!-- Table Card Header -->
      <div class="flex flex-col gap-2 border-b border-slate-200/80 px-6 py-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <div class="flex items-center gap-2.5">
            <h2 class="text-base font-semibold text-slate-900">
              Recent PPMP Submissions
            </h2>
            <span
              v-if="!isLoading && recentPPMPs.length > 0"
              class="rounded-full bg-slate-100 px-2.5 py-0.5 text-xs font-medium text-slate-600"
            >
              Latest {{ recentPPMPs.length }}
            </span>
          </div>
          <p class="mt-0.5 text-xs text-slate-500">
            Click any submission row to inspect line items and review history.
          </p>
        </div>

        <button
          v-if="!isLoading && recentPPMPs.length > 0"
          type="button"
          class="inline-flex items-center gap-1 self-start text-xs font-semibold text-blue-600 transition hover:text-blue-700 sm:self-auto"
          @click="handleViewAll"
        >
          <span>View full directory</span>
          <ChevronRight :size="14" />
        </button>
      </div>

      <!-- Loading Skeleton Table -->
      <div v-if="isLoading" class="divide-y divide-slate-100">
        <div
          v-for="row in 5"
          :key="row"
          class="flex animate-pulse items-center justify-between gap-4 px-6 py-4"
        >
          <div class="flex items-center gap-4">
            <div class="h-9 w-9 rounded-lg bg-slate-100" />
            <div class="space-y-2">
              <div class="h-4 w-36 rounded bg-slate-200" />
              <div class="h-3 w-24 rounded bg-slate-100" />
            </div>
          </div>
          <div class="hidden h-4 w-28 rounded bg-slate-100 md:block" />
          <div class="hidden h-4 w-24 rounded bg-slate-200 sm:block" />
          <div class="h-6 w-28 rounded-full bg-slate-100" />
        </div>
      </div>

      <!-- Error State -->
      <div
        v-else-if="errorMessage"
        class="flex flex-col items-center justify-center px-6 py-14 text-center"
      >
        <div class="flex h-12 w-12 items-center justify-center rounded-full bg-red-50 text-red-600">
          <AlertCircle :size="24" />
        </div>

        <h3 class="mt-3 text-sm font-semibold text-slate-900">
          Unable to load dashboard records
        </h3>

        <p class="mt-1 max-w-md text-sm text-slate-500">
          {{ errorMessage }}
        </p>

        <button
          type="button"
          class="mt-5 inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-xs transition hover:bg-blue-700"
          @click="loadDashboardData(false)"
        >
          <RefreshCw :size="15" />
          <span>Try Again</span>
        </button>
      </div>

      <!-- Empty State -->
      <div
        v-else-if="recentPPMPs.length === 0"
        class="flex flex-col items-center justify-center px-6 py-16 text-center"
      >
        <div class="flex h-14 w-14 items-center justify-center rounded-2xl bg-slate-50 text-slate-400 ring-1 ring-inset ring-slate-200/80">
          <ClipboardList :size="28" />
        </div>

        <h3 class="mt-4 text-sm font-semibold text-slate-900">
          No submitted PPMP records yet
        </h3>

        <p class="mt-1 max-w-sm text-sm text-slate-500">
          Once units finalize and submit their procurement plans, the latest submissions will appear here.
        </p>
      </div>

      <!-- Data Table -->
      <div v-else class="overflow-x-auto">
        <table class="w-full border-collapse text-left text-sm">
          <thead>
            <tr class="border-b border-slate-200/80 bg-slate-50/70 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
              <th class="px-6 py-3.5">PPMP Reference & Unit</th>
              <th class="px-4 py-3.5">Planning Year</th>
              <th class="px-4 py-3.5 text-right">Items</th>
              <th class="px-4 py-3.5 text-right">Estimated Budget</th>
              <th class="px-4 py-3.5">Status</th>
              <th class="px-4 py-3.5">Submitted</th>
              <th class="w-10 px-4 py-3.5"><span class="sr-only">Open</span></th>
            </tr>
          </thead>

          <tbody class="divide-y divide-slate-100">
            <tr
              v-for="ppmp in recentPPMPs"
              :key="ppmp.ppmpId"
              tabindex="0"
              role="button"
              class="group cursor-pointer transition-colors hover:bg-blue-50/40 focus:bg-blue-50/40 focus:outline-none"
              @click="handleRowClick(ppmp.ppmpId)"
              @keydown.enter="handleRowClick(ppmp.ppmpId)"
            >
              <!-- Reference + Unit grouped for stronger scannability -->
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <div class="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-slate-100 text-slate-500 transition-colors group-hover:bg-blue-100/70 group-hover:text-blue-600">
                    <FileText :size="17" />
                  </div>
                  <div class="min-w-0">
                    <p class="truncate font-semibold text-slate-900 group-hover:text-blue-700">
                      {{ ppmp.ppmpReference }}
                    </p>
                    <p class="mt-0.5 flex items-center gap-1 truncate text-xs text-slate-500">
                      <Building2 :size="12" class="shrink-0 text-slate-400" />
                      <span class="truncate">{{ ppmp.departmentName }}</span>
                    </p>
                  </div>
                </div>
              </td>

              <!-- Planning Year -->
              <td class="whitespace-nowrap px-4 py-4">
                <span class="inline-flex items-center gap-1.5 rounded-md bg-slate-100/80 px-2.5 py-1 text-xs font-medium text-slate-700 tabular-nums">
                  <Calendar :size="12" class="text-slate-400" />
                  FY {{ ppmp.planningYear }}
                </span>
              </td>

              <!-- Item Count (Right-aligned tabular) -->
              <td class="whitespace-nowrap px-4 py-4 text-right font-medium text-slate-700 tabular-nums">
                {{ ppmp.itemCount.toLocaleString('en-PH') }}
              </td>

              <!-- Estimated Budget (Right-aligned tabular) -->
              <td class="whitespace-nowrap px-4 py-4 text-right font-semibold text-slate-900 tabular-nums">
                {{ formatCurrency(ppmp.estimatedBudget) }}
              </td>

              <!-- Status Pill with Dot -->
              <td class="whitespace-nowrap px-4 py-4">
                <span
                  :class="[
                    'inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ring-1 ring-inset',
                    getStatusMeta(ppmp.status).pill,
                  ]"
                >
                  <span
                    :class="[
                      'h-1.5 w-1.5 rounded-full',
                      getStatusMeta(ppmp.status).dot,
                    ]"
                  />
                  {{ ppmp.status }}
                </span>
              </td>

              <!-- Submitted Date -->
              <td class="whitespace-nowrap px-4 py-4 text-xs text-slate-500 tabular-nums">
                {{ formatDate(ppmp.submittedAt) }}
              </td>

              <!-- Row Action Chevron -->
              <td class="whitespace-nowrap px-4 py-4 text-right text-slate-400">
                <ChevronRight
                  :size="16"
                  class="inline-block transition-transform group-hover:translate-x-0.5 group-hover:text-blue-600"
                />
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </div>
</template>