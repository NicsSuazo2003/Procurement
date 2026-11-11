export type PPMPStatus =
  | 'Draft'
  | 'For Unit Head Approval'
  | 'Approved by Unit Head'
  | 'Under Review'
  | 'Updated'
  | 'Reviewed'
  | 'Included in APP'

export interface PPMPSectionAllocation {
  sectionId: string
  sectionName: string
  quantity: number
}

export interface PPMPItem {
  lineId: number

  // Null for manually encoded items.
  catalogItemId: number | null

  // Actual procurement category ID from the planning year.
  categoryId: number

  // Legacy/display properties retained for compatibility.
  code: string
  name: string
  category: string
  itemType: string | null
  unit: string
  specifications: string

  // Official PPMP fields.
  generalDescriptionAndObjective?: string
  projectType?: string
  preProcurementConference?: boolean

  quantity: number
  unitPrice: number

  sectionAllocations?: PPMPSectionAllocation[]
}