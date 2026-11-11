export interface UserCategoryPreferenceDto {
  planningYearCategoryId: number
  planningYearId: number
  categoryId: number
  categoryCode: string
  categoryName: string
}

export interface SaveUserCategoryPreferencesRequest {
  planningYearId: number
  planningYearCategoryIds: number[]
}