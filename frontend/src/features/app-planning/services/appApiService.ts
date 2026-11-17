import { apiRequest } from '@/services/api'

import type {
  APPDetail,
  APPListRecord,
  GenerateAPPResult,
} from '../models/app.types'

export const appApiService = {
  getAll(): Promise<APPListRecord[]> {
    return apiRequest<APPListRecord[]>('/APPs')
  },

  getById(id: number): Promise<APPDetail> {
    return apiRequest<APPDetail>(`/APPs/${id}`)
  },

  generate(planningYearId: number): Promise<GenerateAPPResult> {
    return apiRequest<GenerateAPPResult>(`/APPs/generate/${planningYearId}`, {
      method: 'POST',
    })
  },

  addReviewedPPMPs(id: number): Promise<GenerateAPPResult> {
    return apiRequest<GenerateAPPResult>(`/APPs/${id}/add-reviewed-ppmp`, {
      method: 'POST',
    })
  },
}
