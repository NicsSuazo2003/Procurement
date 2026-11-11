
export type ProcurementMethod =
  | 'Public Bidding'
  | 'Small Value Procurement'
  | 'Direct Contracting'
  | 'Negotiated Procurement'
  | 'Other'

export type ProcurementStatus =
  | 'Planning'
  | 'Ongoing'
  | 'Completed'
  | 'Cancelled'

export interface ProcurementProject {
  id: string
  projectCode: string
  projectTitle: string
  endUserDepartment: string
  procurementMethod: ProcurementMethod
  estimatedBudget: number
  fundingSource: string
  plannedStartDate: string
  targetCompletionDate: string
  status: ProcurementStatus
  createdAt: string
  updatedAt: string
}
