import { apiRequest } from '@/services/api'

import type {
  UserCategoryPreferenceDto,
  SaveUserCategoryPreferencesRequest,
} from '../models/user-category-preference.types'

export const userCategoryPreferenceApiService = {
  getPreferences(
    planningYearId: number,
  ): Promise<UserCategoryPreferenceDto[]> {
    return apiRequest<UserCategoryPreferenceDto[]>(
      `/UserCategoryPreferences/${planningYearId}`,
    )
  },

  savePreferences(
    request: SaveUserCategoryPreferencesRequest,
  ): Promise<UserCategoryPreferenceDto[]> {
    return apiRequest<UserCategoryPreferenceDto[]>(
      '/UserCategoryPreferences',
      {
        method: 'PUT',
        body: JSON.stringify(request),
      },
    )
  },
}