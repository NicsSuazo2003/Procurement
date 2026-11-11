import { apiRequest } from '@/services/api'

import type {
  CreatePPMPRequest,
  PPMPDetailApi,
  PPMPListApi,
  UpdatePPMPRequest,
} from '../models/ppmp-api.types'

import type {
  PPMPChangeHistoryApi,
} from '../models/ppmp-history.types'

/**
 * Body sent for endpoints that semantically take no payload but are
 * declared as PATCH/POST by the backend. Sending an empty JSON object
 * forces `Content-Type: application/json`, which ASP.NET Core requires
 * to avoid 415 Unsupported Media Type.
 */
const EMPTY_JSON_BODY = JSON.stringify({})

// =========================================================
// PAGED + SUMMARY TYPES
// =========================================================

export interface PPMPPagedQuery {
  page?: number
  pageSize?: number
  /**
   * Single status, comma-separated statuses, or an array of statuses.
   * Arrays are sent as repeated query params (?status=A&status=B),
   * which binds to string[] on the backend.
   */
  status?: string | string[]
  planningYear?: number
}

export interface PPMPPagedResponse<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
}

export interface PPPMSummary {
  total: number
  awaitingAction: number
  completedReview: number
  byStatus: Record<string, number>
}

// =========================================================
// SERVICE
// =========================================================

export const ppmpApiService = {
  // =========================================================
  // GET ALL PPMPs (unpaged — kept for backward compatibility)
  // =========================================================
  // New callers should use getPaged() instead. This remains for
  // any view that hasn't been migrated to server-side paging yet.

  getAll(): Promise<PPMPListApi[]> {
    return apiRequest<PPMPListApi[]>('/PPMPs')
  },

  // =========================================================
  // GET PAGED PPMPs (server-side pagination + status filter)
  // =========================================================

  getPaged(
    query: PPMPPagedQuery = {},
  ): Promise<PPMPPagedResponse<PPMPListApi>> {
    const params = new URLSearchParams()

    if (query.page && query.page > 0) {
      params.set('page', String(query.page))
    }

    if (query.pageSize && query.pageSize > 0) {
      params.set('pageSize', String(query.pageSize))
    }

    if (query.status) {
      const statuses = Array.isArray(query.status)
        ? query.status
        : [query.status]

      for (const s of statuses) {
        if (s) params.append('status', s)
      }
    }

    if (query.planningYear) {
      params.set('planningYear', String(query.planningYear))
    }

    const qs = params.toString()

    return apiRequest<PPMPPagedResponse<PPMPListApi>>(
      `/PPMPs${qs ? `?${qs}` : ''}`,
    )
  },

  // =========================================================
  // GET SUMMARY (counts + per-status totals)
  // =========================================================

  getSummary(): Promise<PPPMSummary> {
    return apiRequest<PPPMSummary>('/PPMPs/summary')
  },

  // =========================================================
  // GET PPMP BY ID
  // =========================================================

  getById(id: number): Promise<PPMPDetailApi> {
    return apiRequest<PPMPDetailApi>(`/PPMPs/${id}`)
  },

  // =========================================================
  // GET PPMP CHANGE HISTORY
  // =========================================================

  getChangeHistory(id: number): Promise<PPMPChangeHistoryApi[]> {
    return apiRequest<PPMPChangeHistoryApi[]>(
      `/PPMPs/${id}/change-history`,
    )
  },

  // =========================================================
  // CREATE PPMP
  // =========================================================

  create(request: CreatePPMPRequest): Promise<PPMPDetailApi> {
    return apiRequest<PPMPDetailApi>('/PPMPs', {
      method: 'POST',
      body: JSON.stringify(request),
    })
  },

  // =========================================================
  // SUBMIT PPMP
  // =========================================================

  submit(id: number): Promise<PPMPDetailApi> {
    return apiRequest<PPMPDetailApi>(
      `/PPMPs/${id}/submit`,
      {
        method: 'PATCH',
        body: EMPTY_JSON_BODY,
      },
    )
  },

  // =========================================================
  // UNIT HEAD APPROVES PPMP
  // =========================================================

  approve(id: number): Promise<PPMPDetailApi> {
    return apiRequest<PPMPDetailApi>(
      `/PPMPs/${id}/approve`,
      {
        method: 'PATCH',
        body: EMPTY_JSON_BODY,
      },
    )
  },

  // =========================================================
  // START PROCUREMENT REVIEW
  // =========================================================

  startProcurementReview(id: number): Promise<PPMPDetailApi> {
    return apiRequest<PPMPDetailApi>(
      `/PPMPs/${id}/start-review`,
      {
        method: 'PATCH',
        body: EMPTY_JSON_BODY,
      },
    )
  },

  // =========================================================
  // UPDATE PPMP
  // =========================================================

  update(
    id: number,
    request: UpdatePPMPRequest,
  ): Promise<PPMPDetailApi> {
    return apiRequest<PPMPDetailApi>(
      `/PPMPs/${id}`,
      {
        method: 'PUT',
        body: JSON.stringify(request),
      },
    )
  },

  // =========================================================
  // COMPLETE PROCUREMENT REVIEW
  // =========================================================

  completeReview(id: number): Promise<PPMPDetailApi> {
    return apiRequest<PPMPDetailApi>(
      `/PPMPs/${id}/complete-review`,
      {
        method: 'PATCH',
        body: EMPTY_JSON_BODY,
      },
    )
  },
}