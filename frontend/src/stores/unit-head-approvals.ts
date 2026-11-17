import { computed, ref } from 'vue'
import { defineStore } from 'pinia'

import { ppmpApiService } from '@/features/ppmp-app/services/ppmpApiService'

const PENDING_STATUS = 'For Unit Head Approval'

export const useUnitHeadApprovalsStore = defineStore('unit-head-approvals', () => {
  const pendingCount = ref(0)
  const lastFetchedAt = ref<number | null>(null)
  const isRefreshing = ref(false)

  const hasPending = computed(() => pendingCount.value > 0)

  function setPendingCount(count: number): void {
    pendingCount.value = Math.max(0, Math.floor(count))
    lastFetchedAt.value = Date.now()
  }

  async function refresh(): Promise<void> {
    if (isRefreshing.value) return

    isRefreshing.value = true

    try {
      const summary = await ppmpApiService.getSummary()
      setPendingCount(summary.byStatus[PENDING_STATUS] ?? 0)
    } catch {
      // Keep the previous count on failure.
    } finally {
      isRefreshing.value = false
    }
  }

  function reset(): void {
    pendingCount.value = 0
    lastFetchedAt.value = null
  }

  return {
    pendingCount,
    lastFetchedAt,
    isRefreshing,
    hasPending,
    setPendingCount,
    refresh,
    reset,
  }
})