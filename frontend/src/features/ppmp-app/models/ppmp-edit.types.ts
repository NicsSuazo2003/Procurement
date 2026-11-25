
export interface EditableAllocation {
  sectionId: number
  sectionName: string
  quantity: number
}

export interface EditableItem {
  lineId: number
  ppmpItemId: number | null
  itemId: number | null
  categoryId: number
  categoryName: string

  // Catalogue reference only.
  code: string

  // Legacy backend compatibility fields.
  name: string
  itemType: string | null
  specifications: string

  // Official PPMP fields.
  generalDescriptionAndObjective: string
  projectType: string
  preProcurementConference: boolean | null

  recommendedModeOfProcurement: string
  startOfProcurementActivity: string
  endOfProcurementActivity: string
  expectedDeliveryImplementationPeriod: string
  sourceOfFunds: string
  attachedSupportingDocuments: string

  unit: string
  unitPrice: number
  allocations: EditableAllocation[]
}
