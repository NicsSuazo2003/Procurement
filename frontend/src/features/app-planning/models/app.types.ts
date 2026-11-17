export interface APPListRecord {
  appId: number
  planningYearId: number
  appReference: string
  status: string
  totalEstimatedBudget: number
  createdAt: string
  categoryCount: number
  itemCount: number
}

export interface APPItemSource {
  appItemSourceId: number
  ppmpItemId: number
  ppmpId: number
  ppmpReference: string
  contributedQuantity: number
  contributedBudget: number
}

export interface APPItem {
  appItemId: number
  itemId: number | null
  itemCode: string
  itemDescription: string
  itemType: string | null
  unit: string
  specifications: string | null
  totalQuantity: number
  estimatedUnitPrice: number
  estimatedBudget: number
  sources: APPItemSource[]
}

export interface APPCategory {
  appCategoryId: number
  categoryId: number
  totalEstimatedBudget: number
  items: APPItem[]
}

export interface APPDetail {
  appId: number
  planningYearId: number
  createdBy: number
  appReference: string
  status: string
  totalEstimatedBudget: number
  createdAt: string
  approvedAt: string | null
  categories: APPCategory[]
}

export interface GenerateAPPResult {
  appId: number
  appReference: string
  status: string
  includedPPMPCount: number
  categoryCount: number
  itemCount: number
  totalEstimatedBudget: number
}
