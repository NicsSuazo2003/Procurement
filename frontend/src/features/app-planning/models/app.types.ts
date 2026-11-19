export interface APPListRecord {
  appId: number
  planningYearId: number
  appReference: string
  status: string
  revisionNo: number
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
  papCode: string | null
  objectCode: string | null
  procurementProjectName: string | null
  generalDescription: string | null
  endUser: string | null
  classification: string | null
  modeOfProcurement: string | null
  bidEvaluationCriteria: string | null
  marginalizedSectorConsideration: boolean | null
  greenTechnicalSpecifications: boolean | null
  targetPRSubmissionDate: string | null
  startOfProcurementActivity: string | null
  endOfProcurementActivity: string | null
  sourceOfFunds: string | null
  mooe: number | null
  capitalOutlay: number | null
  multiYearContractCost: number | null
  lifeCycleCost: number | null
  procurementStrategyTools: string | null
  prospectiveBidders: string | null
  potentialObservers: string | null
  procurementAgent: string | null
  collaboratingEntities: string | null
  remarks: string | null
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
  revisionNo: number
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
