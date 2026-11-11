
<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import {
  ClipboardList,
  Eye,
  RefreshCw,
  Clock,
  CheckCircle,
} from '@lucide/vue'

import { useAuthStore } from '@/stores/auth'
import { ppmpApiService } from '../services/ppmpApiService'

import type { PPMPListApi } from '../models/ppmp-api.types'

const authStore = useAuthStore()

const ppmpRecords = ref<PPMPListApi[]>([])
const isLoading = ref(false)
const errorMessage = ref('')

const pendingApprovals = computed(() =>
  ppmpRecords.value.filter(
    ppmp => ppmp.status === 'For Unit Head Approval',
  ),
)

const approvedCount = computed(() =>
  ppmpRecords.value.filter(
    ppmp => ppmp.status === 'Approved by Unit Head',
  ).length,
)

async function loadPPMPs(): Promise<void> {
  isLoading.value = true
  errorMessage.value = ''

  try {
    // The backend already restricts Unit Heads to their department.
    ppmpRecords.value = await ppmpApiService.getAll()
  } catch (error) {
    console.error('Failed to load PPMP approvals:', error)

    errorMessage.value = error instanceof Error
      ? error.message
      : 'Unable to load PPMP records.'
  } finally {
    isLoading.value = false
  }
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-PH', {
    style: 'currency',
    currency: 'PHP',
    minimumFractionDigits: 2,
  }).format(amount)
}

function formatDate(value: string | null): string {
  if (!value) return 'Not recorded'

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) return value

  return new Intl.DateTimeFormat('en-PH', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  }).format(date)
}

onMounted(loadPPMPs)
</script>

<template>
  <div class="space-y-6">

    <!-- HEADER -->
    <div class="flex flex-wrap items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">
          PPMP Approval Queue
        </h1>

        <p class="mt-1 text-sm text-gray-500">
          Review and approve Procurement Project Management Plans submitted by your department.
        </p>

        <p class="mt-2 text-sm font-medium text-gray-700">
          {{ authStore.user?.departmentName || 'Your Department' }}
        </p>
      </div>

      <button
        type="button"
        @click="loadPPMPs"
        :disabled="isLoading"
        class="inline-flex items-center gap-2 rounded-lg border border-gray-300 bg-white px-4 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
      >
        <RefreshCw :size="16" />
        Refresh
      </button>
    </div>

    <!-- SUMMARY -->
    <div class="grid gap-4 sm:grid-cols-2">
      <div class="rounded-xl border border-gray-200 bg-white p-5">
        <div class="flex items-center gap-3">
          <div class="rounded-lg bg-amber-50 p-3">
            <Clock :size="21" class="text-amber-700" />
          </div>

          <div>
            <p class="text-sm text-gray-500">
              Awaiting Your Approval
            </p>

            <p class="text-2xl font-bold text-gray-900">
              {{ pendingApprovals.length }}
            </p>
          </div>
        </div>
      </div>

      <div class="rounded-xl border border-gray-200 bg-white p-5">
        <div class="flex items-center gap-3">
          <div class="rounded-lg bg-emerald-50 p-3">
            <CheckCircle :size="21" class="text-emerald-700" />
          </div>

          <div>
            <p class="text-sm text-gray-500">
              Approved PPMPs
            </p>

            <p class="text-2xl font-bold text-gray-900">
              {{ approvedCount }}
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- ERROR -->
    <div
      v-if="errorMessage"
      class="rounded-lg border border-red-200 bg-red-50 p-4"
    >
      <p class="text-sm text-red-800">
        {{ errorMessage }}
      </p>
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
        Loading approval queue...
      </p>
    </div>

    <!-- APPROVAL TABLE -->
    <section
      v-else
      class="overflow-hidden rounded-xl border border-gray-200 bg-white"
    >
      <div class="border-b border-gray-200 p-5">
        <div class="flex items-center gap-2">
          <ClipboardList :size="19" class="text-emerald-700" />

          <h2 class="font-semibold text-gray-900">
            Pending Unit Head Approval
          </h2>
        </div>

        <p class="mt-1 text-sm text-gray-500">
          PPMPs submitted by End-users that require your review.
        </p>
      </div>

      <div
        v-if="pendingApprovals.length > 0"
        class="overflow-x-auto"
      >
        <table class="w-full text-left text-sm">
          <thead class="bg-gray-50 text-xs uppercase text-gray-500">
            <tr>
              <th class="px-5 py-3 font-medium">PPMP Reference</th>
              <th class="px-5 py-3 font-medium">Department</th>
              <th class="px-5 py-3 font-medium">Planning Year</th>
              <th class="px-5 py-3 font-medium">Date Submitted</th>
              <th class="px-5 py-3 text-right font-medium">Estimated Budget</th>
              <th class="px-5 py-3 text-center font-medium">Action</th>
            </tr>
          </thead>

          <tbody class="divide-y divide-gray-100">
            <tr
              v-for="ppmp in pendingApprovals"
              :key="ppmp.ppmpId"
              class="hover:bg-gray-50"
            >
              <td class="px-5 py-4 font-semibold text-gray-900">
                {{ ppmp.ppmpReference }}
              </td>

              <td class="px-5 py-4 text-gray-700">
                {{ ppmp.departmentName }}
              </td>

              <td class="px-5 py-4 text-gray-700">
                {{ ppmp.planningYear }}
              </td>

              <td class="px-5 py-4 text-gray-600">
                {{ formatDate(ppmp.submittedAt) }}
              </td>

              <td class="px-5 py-4 text-right font-medium text-gray-800">
                {{ formatCurrency(ppmp.estimatedBudget) }}
              </td>

              <td class="px-5 py-4 text-center">
                <RouterLink
                  :to="`/ppmp/${ppmp.ppmpId}`"
                  class="inline-flex items-center gap-2 rounded-lg bg-emerald-700 px-3 py-2 text-xs font-medium text-white hover:bg-emerald-800"
                >
                  <Eye :size="15" />
                  Review
                </RouterLink>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- EMPTY STATE -->
      <div
        v-else
        class="p-12 text-center"
      >
        <ClipboardList
          :size="36"
          class="mx-auto mb-3 text-gray-400"
        />

        <h3 class="font-semibold text-gray-800">
          No Pending Approvals
        </h3>

        <p class="mt-1 text-sm text-gray-500">
          There are currently no PPMPs awaiting your approval.
        </p>
      </div>
    </section>

  </div>
</template>
