
import { apiRequest } from '@/services/api'

import type {
  DepartmentDto,
  DepartmentSectionDto,
  PlanningYearDto,
  PlanningYearCategoryDto,
  ItemDto,
  ProcurementCategoryDto,
  PlanningYearItemDto,
  AssignPlanningYearItemRequest,
  UpdatePlanningYearItemRequest,
  CreateItemRequest,
UpdateItemRequest,
PlanningYearCatalogueItemDto,
} from '../models/master-data.types'

export const masterDataApiService = {
  getDepartments(): Promise<DepartmentDto[]> {
    return apiRequest<DepartmentDto[]>('/Departments')
  },

  getDepartmentSections(
    departmentId: number,
  ): Promise<DepartmentSectionDto[]> {
    return apiRequest<DepartmentSectionDto[]>(
      `/DepartmentSections/department/${departmentId}`,
    )
  },

  getPlanningYears(): Promise<PlanningYearDto[]> {
    return apiRequest<PlanningYearDto[]>('/PlanningYears')
  },

  getProcurementCategories(): Promise<ProcurementCategoryDto[]> {
    return apiRequest<ProcurementCategoryDto[]>('/ProcurementCategories')
  },

  getPlanningYearCategories(
    planningYearId: number,
  ): Promise<PlanningYearCategoryDto[]> {
    return apiRequest<PlanningYearCategoryDto[]>(
      `/PlanningYearCategories/planning-year/${planningYearId}`,
    )
  },

  // Master catalogue item management

  getAllItems(): Promise<ItemDto[]> {
    return apiRequest<ItemDto[]>('/Items')
  },

  getItemsByCategory(categoryId: number): Promise<ItemDto[]> {
    return apiRequest<ItemDto[]>(`/Items/category/${categoryId}`)
  },

  getItem(id: number): Promise<ItemDto> {
    return apiRequest<ItemDto>(`/Items/${id}`)
  },

  createItem(request: CreateItemRequest): Promise<ItemDto> {
    return apiRequest<ItemDto>('/Items', {
      method: 'POST',
      body: JSON.stringify(request),
    })
  },

  updateItem(
  id: number,
  request: UpdateItemRequest,
 ): Promise<ItemDto> {
  return apiRequest<ItemDto>(`/Items/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
  })
 },

  setItemActiveStatus(
    id: number,
    isActive: boolean,
  ): Promise<void> {
    return apiRequest<void>(`/Items/${id}/active-status`, {
      method: 'PATCH',
      body: JSON.stringify(isActive),
    })
  },

  // Planning-year catalogue assignments

  getPlanningYearItems(): Promise<PlanningYearItemDto[]> {
    return apiRequest<PlanningYearItemDto[]>('/PlanningYearItems')
  },

  getPlanningYearItemsByCategory(
    planningYearCategoryId: number,
  ): Promise<PlanningYearItemDto[]> {
    return apiRequest<PlanningYearItemDto[]>(
      `/PlanningYearItems/planning-year-category/${planningYearCategoryId}`,
    )
  },

  getPlanningYearItem(
    id: number,
  ): Promise<PlanningYearItemDto> {
    return apiRequest<PlanningYearItemDto>(
      `/PlanningYearItems/${id}`,
    )
  },

  assignPlanningYearItem(
    request: AssignPlanningYearItemRequest,
  ): Promise<PlanningYearItemDto> {
    return apiRequest<PlanningYearItemDto>(
      '/PlanningYearItems',
      {
        method: 'POST',
        body: JSON.stringify(request),
      },
    )
  },

  updatePlanningYearItem(
    id: number,
    request: UpdatePlanningYearItemRequest,
  ): Promise<PlanningYearItemDto> {
    return apiRequest<PlanningYearItemDto>(
      `/PlanningYearItems/${id}`,
      {
        method: 'PUT',
        body: JSON.stringify(request),
      },
    )
  },

  setPlanningYearItemActiveStatus(
    id: number,
    isActive: boolean,
  ): Promise<void> {
    return apiRequest<void>(
      `/PlanningYearItems/${id}/active-status?isActive=${isActive}`,
      {
        method: 'PATCH',
      },
    )
  },
  getPlanningYearCatalogueItems(
  planningYear: number,
  categoryId: number,
): Promise<PlanningYearCatalogueItemDto[]> {
  return apiRequest<PlanningYearCatalogueItemDto[]>(
    `/Items/planning-year/${planningYear}/category/${categoryId}`,
  )
},
}
