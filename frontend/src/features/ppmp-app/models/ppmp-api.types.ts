
export interface CreatePPMPRequest {
  departmentId: number
  planningYearId: number
  items: CreatePPMPItemRequest[]
}

export interface CreatePPMPItemRequest {
  itemId: number | null
  categoryId: number

  // Legacy backend compatibility fields.
  itemCode: string
  itemDescription: string
  itemType: string | null
  unit: string
  specifications: string | null

  // Official PPMP fields.
  generalDescriptionAndObjective: string
  projectType: string
  preProcurementConference: boolean

  quantity: number
  unitPrice: number

  sectionAllocations: CreatePPMPSectionAllocationRequest[]
}

export interface CreatePPMPSectionAllocationRequest {
  sectionId: number
  quantity: number
}

export interface PPMPListApi {
  ppmpId: number
  ppmpReference: string
  departmentId: number
  departmentName: string
  planningYearId: number
  planningYear: number
  createdBy: number
  createdByName: string
  status: string
  estimatedBudget: number
  createdAt: string
  submittedAt: string | null
  updatedAt: string | null
  itemCount: number
}

export interface PPMPSectionAllocationApi {
  allocationId: number
  sectionId: number
  sectionCode: string
  sectionName: string
  quantity: number
}

export interface PPMPItemApi {
  ppmpItemId: number
  itemId: number | null
  categoryId: number
  categoryName: string

  // Legacy fields returned by the current backend.
  itemCode: string
  itemDescription: string
  itemType: string | null
  unit: string
  specifications: string | null

  // Official PPMP fields.
  generalDescriptionAndObjective: string
  projectType: string
  preProcurementConference: boolean

  quantity: number
  unitPrice: number
  estimatedBudget: number
  sectionAllocations: PPMPSectionAllocationApi[]
}

export interface PPMPDetailApi extends PPMPListApi {
  items: PPMPItemApi[]
}

export interface UpdatePPMPRequest {
  reason: string
  items: UpdatePPMPItemRequest[]
}

export interface UpdatePPMPItemRequest {
  ppmpItemId: number | null
  itemId: number | null
  categoryId: number

  // Legacy backend compatibility fields.
  itemCode: string
  itemDescription: string
  itemType: string | null
  unit: string
  specifications: string | null

  // Official PPMP fields.
  generalDescriptionAndObjective: string
  projectType: string
  preProcurementConference: boolean

  quantity: number
  unitPrice: number

  sectionAllocations: {
    sectionId: number
    quantity: number
  }[]
}
