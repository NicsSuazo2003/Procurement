import { apiRequest } from '@/services/api'

import type {
  DepartmentDto,
  DepartmentSectionDto,
  PlanningYearDto,
  PlanningYearCategoryDto,
  ItemDto,
} from '../models/master-data.types'

export const ppmpMasterDataService = {
  async getDepartments(): Promise<DepartmentDto[]> {
    const departments = await apiRequest<DepartmentDto[]>(
      '/Departments',
    )

    return departments.filter(department => department.isActive)
  },

  async getDepartmentSections(
    departmentId: number,
  ): Promise<DepartmentSectionDto[]> {
    const sections = await apiRequest<DepartmentSectionDto[]>(
      `/DepartmentSections/department/${departmentId}`,
    )

    return sections.filter(section => section.isActive)
  },

  async getPlanningYears(): Promise<PlanningYearDto[]> {
    const planningYears = await apiRequest<PlanningYearDto[]>(
      '/PlanningYears',
    )

    return planningYears
      .filter(year => year.status.toLowerCase() === 'open')
      .sort((a, b) => a.year - b.year)
  },

  async getCategoriesForPlanningYear(
    planningYearId: number,
  ): Promise<PlanningYearCategoryDto[]> {
    const categories = await apiRequest<PlanningYearCategoryDto[]>(
      `/PlanningYearCategories/planning-year/${planningYearId}`,
    )

    return categories.filter(category => category.isActive)
  },

  async getItemsByCategory(
    categoryId: number,
  ): Promise<ItemDto[]> {
    const items = await apiRequest<ItemDto[]>(
      `/Items/category/${categoryId}`,
    )

    return items.filter(item => item.isActive)
  },
}