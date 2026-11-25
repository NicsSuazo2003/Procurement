<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import {
  ClipboardList,
  Eye,
  RefreshCw,
  Clock,
  CheckCircle,
  Building2,
} from '@lucide/vue'

import { useAuthStore } from '@/stores/auth'
import { ppmpApiService } from '../services/ppmpApiService'
import type { PPMPListApi } from '../models/ppmp-api.types'
import { formatDatePH, formatPHP } from '@/shared/utilities/ppmpUiHelpers'

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
    // The backend already restricts Unit Heads to their unit.
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

onMounted(loadPPMPs)
</script>

<template>
  <div class="mx-auto max-w-7xl space-y-6">
    <!-- HEADER -->
    <div class="flex flex-wrap items-start justify-between gap-4">
      <div>
        <div class="flex flex-wrap items-center gap-2.5">
          <h1 class="text-2xl font-bold tracking-tight text-slate-900">
            PPMP Approval Queue
          </h1>

          <span class="inline-flex items-center gap-1.5 rounded-full bg-brand-50 px-3 py-1 text-xs font-semibold text-brand-700 ring-1 ring-inset ring-brand-600/20">
            <Building2 :size="13" />
            {{ authStore.user?.departmentName || 'Your Unit' }}
          </span>
        </div>

        <p class="mt-1 text-sm text-slate-500">
          Review and approve Project Procurement Management Plans submitted by your unit.
        </p>
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

    <!-- SUMMARY -->
    <div class="grid gap-4 sm:grid-cols-2">
      <div class="drmc-card flex items-center gap-4 p-5">
        <div class="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-amber-50 text-amber-700 ring-1 ring-inset ring-amber-600/20">
          <Clock :size="22" />
        </div>

        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
            Awaiting Your Approval
          </p>
          <p class="mt-1 text-2xl font-bold text-slate-900 tabular-nums">
            {{ pendingApprovals.length }}
          </p>
        </div>
      </div>

      <div class="drmc-card flex items-center gap-4 p-5">
        <div class="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/20">
          <CheckCircle :size="22" />
        </div>

        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
            Approved PPMPs
          </p>
          <p class="mt-1 text-2xl font-bold text-slate-900 tabular-nums">
            {{ approvedCount }}
          </p>
        </div>
      </div>
    </div>

    <!-- ERROR -->
    <div
      v-if="errorMessage"
      role="alert"
      class="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-800"
    >
      {{ errorMessage }}
    </div>

    <!-- LOADING -->
    <div
      v-if="isLoading"
      class="drmc-card p-14 text-center"
    >
      <RefreshCw :size="26" class="mx-auto mb-3 animate-spin text-brand-700" />
      <p class="text-sm font-medium text-slate-600">
        Loading approval queue...
      </p>
    </div>

    <!-- APPROVAL TABLE -->
    <section
      v-else
      class="drmc-card overflow-hidden"
    >
      <div class="border-b border-slate-200/80 p-5">
        <div class="flex items-center gap-2">
          <ClipboardList :size="18" class="text-brand-700" />
          <h2 class="text-base font-semibold text-slate-900">
            Pending Unit Head Approval
          </h2>
        </div>

        <p class="mt-0.5 text-xs text-slate-500">
          PPMPs submitted by End-users that require your review.
        </p>
      </div>

      <div v-if="pendingApprovals.length > 0" class="overflow-x-auto">
        <table class="w-full text-left text-sm">
          <thead class="border-b border-slate-200/80 bg-slate-50/80 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
            <tr>
              <th class="px-5 py-3.5">PPMP Reference</th>
              <th class="px-5 py-3.5">Unit</th>
              <th class="px-5 py-3.5">Planning Year</th>
              <th class="px-5 py-3.5">Date Submitted</th>
              <th class="px-5 py-3.5 text-right">Estimated Budget</th>
              <th class="px-5 py-3.5 text-center">Action</th>
            </tr>
          </thead>

          <tbody class="divide-y divide-slate-100">
            <tr
              v-for="ppmp in pendingApprovals"
              :key="ppmp.ppmpId"
              class="transition-colors hover:bg-slate-50/80"
            >
              <td class="px-5 py-4 font-semibold text-brand-700">
                <RouterLink :to="`/ppmp/${ppmp.ppmpId}`" class="hover:underline">
                  {{ ppmp.ppmpReference }}
                </RouterLink>
              </td>

              <td class="px-5 py-4 font-medium text-slate-800">
                {{ ppmp.departmentName }}
              </td>

              <td class="px-5 py-4 text-slate-600 tabular-nums">
                {{ ppmp.planningYear }}
              </td>

              <td class="px-5 py-4 text-slate-600">
                {{ formatDatePH(ppmp.submittedAt, { fallback: 'Not recorded' }) }}
              </td>

              <td class="px-5 py-4 text-right font-semibold text-slate-900 tabular-nums">
                {{ formatPHP(ppmp.estimatedBudget) }}
              </td>

              <td class="px-5 py-4 text-center">
                <RouterLink
                  :to="`/ppmp/${ppmp.ppmpId}`"
                  class="drmc-btn-primary px-3 py-1.5 text-xs"
                >
                  <Eye :size="14" />
                  Review
                </RouterLink>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- EMPTY STATE -->
      <div v-else class="p-14 text-center">
        <ClipboardList :size="36" class="mx-auto mb-3 text-slate-300" />
        <h3 class="font-semibold text-slate-800">
          No Pending Approvals
        </h3>
        <p class="mt-1 text-xs text-slate-500">
          There are currently no PPMPs awaiting your approval.
        </p>
      </div>
    </section>
  </div>
</template>