<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  ClipboardList,
  Clock3,
  CheckCircle2,
  AlertCircle,
} from '@lucide/vue'

import { ppmpApiService } from '@/features/ppmp-app/services/ppmpApiService'
import type { PPMPListApi } from '@/features/ppmp-app/models/ppmp-api.types'

const ppmpRecords = ref<PPMPListApi[]>([])
const summary = ref({
  total: 0,
  awaitingAction: 0,
  completedReview: 0,
  byStatus: {} as Record<string, number>,
})

const isLoading = ref(true)
const errorMessage = ref('')

function normalizeStatus(status: string): string {
  return status.trim().toLowerCase()
}

// Count PPMPs awaiting or undergoing Procurement review.
// (Fallback only: the server-provided byStatus is preferred.)
const pendingReviewCount = computed(() =>
  ppmpRecords.value.filter(ppmp =>
    [
      'approved by unit head',
      'under review',
      'updated',
    ].includes(normalizeStatus(ppmp.status)),
  ).length,
)

const summaryCards = computed(() => [
  {
    title: 'Total PPMPs',
    value: summary.value.total,
    description: 'Across accessible planning years',
    icon: ClipboardList,
    color: 'bg-blue-50 text-blue-700',
  },
  {
    title: 'Pending Review',
    value: summary.value.awaitingAction,
    description: 'Awaiting or undergoing Procurement review',
    icon: Clock3,
    color: 'bg-amber-50 text-amber-700',
  },
  {
    title: 'Reviewed',
    value: summary.value.completedReview,
    description: 'Reviewed by Procurement',
    icon: CheckCircle2,
    color: 'bg-emerald-50 text-emerald-700',
  },
  {
    title: 'For Reconsideration',
    value: null,
    description: 'Not yet supported by backend status',
    icon: AlertCircle,
    color: 'bg-red-50 text-red-700',
  },
])

// Backend returns the most recent 5 by SubmittedAt desc.
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

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-PH', {
    style: 'currency',
    currency: 'PHP',
  }).format(amount)
}

function getStatusClass(status: string): string {
  switch (normalizeStatus(status)) {
    case 'draft':
      return 'bg-slate-100 text-slate-700'
    case 'for unit head approval':
      return 'bg-amber-100 text-amber-700'
    case 'approved by unit head':
      return 'bg-blue-100 text-blue-700'
    case 'under review':
      return 'bg-purple-100 text-purple-700'
    case 'updated':
      return 'bg-orange-100 text-orange-700'
    case 'reviewed':
      return 'bg-emerald-100 text-emerald-700'
    default:
      return 'bg-slate-100 text-slate-700'
  }
}

async function loadDashboardData(): Promise<void> {
  isLoading.value = true
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

    // Only display records that have actually been submitted.
    ppmpRecords.value = recentPage.items.filter(
      ppmp => ppmp.submittedAt !== null,
    )
  } catch (error) {
    console.error('Failed to load dashboard PPMP records:', error)

    errorMessage.value =
      error instanceof Error
        ? error.message
        : 'Unable to load PPMP records. Please try again.'

    ppmpRecords.value = []
  } finally {
    isLoading.value = false
  }
}

onMounted(loadDashboardData)
</script>

<template>
  <div class="space-y-6">
    <div>
      <h1 class="text-2xl font-bold text-slate-900">
        Dashboard
      </h1>

      <p class="mt-1 text-sm text-slate-500">
        Overview of procurement planning and PPMP activities.
      </p>
    </div>

    <!-- SUMMARY CARDS -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
      <div
        v-for="card in summaryCards"
        :key="card.title"
        class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm"
      >
        <div class="flex items-start justify-between">
          <div>
            <p class="text-sm font-medium text-slate-500">
              {{ card.title }}
            </p>

            <p class="mt-2 text-3xl font-bold text-slate-900">
              {{
                isLoading
                  ? '...'
                  : card.value === null
                    ? '—'
                    : card.value
              }}
            </p>
          </div>

          <div :class="['rounded-lg p-3', card.color]">
            <component :is="card.icon" :size="22" />
          </div>
        </div>

        <p class="mt-3 text-xs text-slate-500">
          {{ card.description }}
        </p>
      </div>
    </div>

    <!-- RECENT PPMPs -->
    <section class="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
      <div class="border-b border-slate-200 px-5 py-4">
        <h2 class="font-semibold text-slate-900">
          Recent PPMP Submissions
        </h2>

        <p class="mt-1 text-xs text-slate-500">
          Latest submitted PPMP records (most recent 5)
        </p>
      </div>

      <div
        v-if="isLoading"
        class="px-5 py-12 text-center text-sm text-slate-500"
      >
        Loading PPMP records...
      </div>

      <div
        v-else-if="errorMessage"
        class="px-5 py-12 text-center"
      >
        <p class="font-medium text-red-600">
          Unable to load dashboard records
        </p>

        <p class="mt-2 text-sm text-slate-500">
          {{ errorMessage }}
        </p>

        <button
          type="button"
          class="mt-4 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
          @click="loadDashboardData"
        >
          Try Again
        </button>
      </div>

      <div
        v-else-if="recentPPMPs.length === 0"
        class="px-5 py-12 text-center"
      >
        <ClipboardList :size="36" class="mx-auto text-slate-300" />

        <p class="mt-3 font-medium text-slate-700">
          No submitted PPMP records to display
        </p>

        <p class="mt-1 text-sm text-slate-500">
          Submitted PPMP records will appear here.
        </p>
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-sm">
          <thead class="bg-slate-50 text-xs uppercase text-slate-500">
            <tr>
              <th class="px-5 py-3 font-medium">PPMP Reference</th>
              <th class="px-5 py-3 font-medium">Department</th>
              <th class="px-5 py-3 font-medium">Planning Year</th>
              <th class="px-5 py-3 font-medium">Items</th>
              <th class="px-5 py-3 font-medium">Estimated Budget</th>
              <th class="px-5 py-3 font-medium">Status</th>
              <th class="px-5 py-3 font-medium">Submitted</th>
            </tr>
          </thead>

          <tbody class="divide-y divide-slate-100">
            <tr
              v-for="ppmp in recentPPMPs"
              :key="ppmp.ppmpId"
              class="hover:bg-slate-50"
            >
              <td class="whitespace-nowrap px-5 py-4 font-medium text-slate-900">
                {{ ppmp.ppmpReference }}
              </td>

              <td class="whitespace-nowrap px-5 py-4 text-slate-600">
                {{ ppmp.departmentName }}
              </td>

              <td class="px-5 py-4 text-slate-600">
                {{ ppmp.planningYear }}
              </td>

              <td class="px-5 py-4 text-slate-600">
                {{ ppmp.itemCount }}
              </td>

              <td class="whitespace-nowrap px-5 py-4 text-slate-600">
                {{ formatCurrency(ppmp.estimatedBudget) }}
              </td>

              <td class="whitespace-nowrap px-5 py-4">
                <span
                  :class="[
                    'rounded-full px-2.5 py-1 text-xs font-medium',
                    getStatusClass(ppmp.status),
                  ]"
                >
                  {{ ppmp.status }}
                </span>
              </td>

              <td class="whitespace-nowrap px-5 py-4 text-slate-600">
                {{ formatDate(ppmp.submittedAt) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </div>
</template>