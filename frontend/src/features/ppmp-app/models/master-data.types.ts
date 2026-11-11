    export interface DepartmentDto {
    departmentId: number
    departmentCode: string
    departmentName: string
    isActive: boolean
    }

    export interface DepartmentSectionDto {
    sectionId: number
    departmentId: number
    departmentName: string
    sectionCode: string
    sectionName: string
    isActive: boolean
    }

    export interface PlanningYearDto {
    planningYearId: number
    year: number
    status: string
    }

    export interface PlanningYearCategoryDto {
    planningYearCategoryId: number
    planningYearId: number
    year: number
    categoryId: number
    categoryCode: string
    categoryName: string
    isActive: boolean
    }

    export interface ItemDto {
    itemId: number
    categoryId: number
    categoryCode: string
    categoryName: string
    itemCode: string
    itemName: string
    itemType: string | null
    unit: string
    specifications: string | null
    isActive: boolean
    }

    export interface ProcurementCategoryDto {
    categoryId: number
    categoryCode: string
    categoryName: string
    description: string | null
    isActive: boolean
    }

    export interface PlanningYearItemDto {
    planningYearItemId: number
    planningYearCategoryId: number
    planningYearId: number
    planningYear: number

    categoryId: number
    categoryCode: string
    categoryName: string

    itemId: number
    itemCode: string
    itemName: string
    unit: string
    specifications: string | null

    abcPerUnit: number
    isActive: boolean
    }

    export interface PlanningYearCatalogueItemDto {
  planningYear: number
  itemId: number
  itemCode: string
  itemName: string
  itemType: string | null
  unit: string
  specifications: string | null
  categoryId: number
  categoryCode: string
  categoryName: string
  abcPerUnit: number
}

    export interface AssignPlanningYearItemRequest {
    planningYearCategoryId: number
    itemId: number
    abcPerUnit: number
    }

    export interface UpdatePlanningYearItemRequest {
    abcPerUnit: number
    }
    export interface CreateItemRequest {
    categoryId: number
    itemName: string
    itemType: string | null
    unit: string
    specifications: string | null
    }

    export interface UpdateItemRequest {
    categoryId: number
    itemName: string
    itemType: string | null
    unit: string
    specifications: string | null
    }