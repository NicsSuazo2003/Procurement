
export interface PPMPChangeHistorySectionAllocationApi {
  changeHistorySectionAllocationId: number

  previousSectionId: number | null
  previousSectionCode: string | null
  previousSectionName: string | null
  previousQuantity: number | null

  updatedSectionId: number | null
  updatedSectionCode: string | null
  updatedSectionName: string | null
  updatedQuantity: number | null
}

export interface PPMPChangeHistoryItemApi {
  changeHistoryItemId: number

  previousItemId: number | null
  previousItemCode: string | null
  previousDescription: string | null
  previousUnit: string | null
  previousQuantity: number | null
  previousUnitPrice: number | null
  previousEstimatedBudget: number | null

  previousGeneralDescriptionAndObjective: string | null
  previousProjectType: string | null
  previousPreProcurementConference: boolean | null

  updatedItemId: number | null
  updatedItemCode: string | null
  updatedDescription: string | null
  updatedUnit: string | null
  updatedQuantity: number | null
  updatedUnitPrice: number | null
  updatedEstimatedBudget: number | null

  updatedGeneralDescriptionAndObjective: string | null
  updatedProjectType: string | null
  updatedPreProcurementConference: boolean | null

  sectionAllocations: PPMPChangeHistorySectionAllocationApi[]
}

export interface PPMPChangeHistoryApi {
  changeHistoryId: number
  ppmpId: number

  changedBy: number
  changedByName: string

  reason: string

  previousBudget: number
  updatedBudget: number

  changedAt: string

  items: PPMPChangeHistoryItemApi[]
}
