import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

import { ppmpApiService } from '@/features/ppmp-app/services/ppmpApiService'
import { useProcurementReviewCountStore } from './procurement-review-count'
import { useUnitHeadApprovalsStore } from './unit-head-approvals'

describe('PPMP sidebar status counts', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('uses the full submitted-for-Unit-Head count from the summary', async () => {
    vi.spyOn(ppmpApiService, 'getSummary').mockResolvedValue({
      total: 40,
      awaitingAction: 14,
      completedReview: 0,
      byStatus: {
        'For Unit Head Approval': 31,
        'Approved by Unit Head': 9,
      },
    })

    const store = useUnitHeadApprovalsStore()
    await store.refresh()

    expect(ppmpApiService.getSummary).toHaveBeenCalledOnce()
    expect(store.pendingCount).toBe(31)
  })

  it('uses the full Unit-Head-approved count for Procurement', async () => {
    vi.spyOn(ppmpApiService, 'getSummary').mockResolvedValue({
      total: 40,
      awaitingAction: 14,
      completedReview: 0,
      byStatus: {
        'For Unit Head Approval': 31,
        'Approved by Unit Head': 9,
      },
    })

    const store = useProcurementReviewCountStore()
    await store.refresh()

    expect(ppmpApiService.getSummary).toHaveBeenCalledOnce()
    expect(store.pendingCount).toBe(9)
  })
})
