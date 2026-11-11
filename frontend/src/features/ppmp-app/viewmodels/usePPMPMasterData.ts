
import { ref } from 'vue'

import { masterDataApiService } from '../services/masterDataApiService'

import type {
  DepartmentDto,
  DepartmentSectionDto,
  PlanningYearDto,
  PlanningYearCategoryDto,
  PlanningYearCatalogueItemDto,
} from '../models/master-data.types'

export function usePPMPMasterData() {
  const departments = ref<DepartmentDto[]>([])
  const planningYears = ref<PlanningYearDto[]>([])
  const planningYearCategories = ref<PlanningYearCategoryDto[]>([])
  const sections = ref<DepartmentSectionDto[]>([])
const items = ref<PlanningYearCatalogueItemDto[]>([])

  const isLoading = ref(false)
  const error = ref('')

  async function loadInitialData(): Promise<void> {
    isLoading.value = true
    error.value = ''

    try {
      const [departmentData, planningYearData] = await Promise.all([
        masterDataApiService.getDepartments(),
        masterDataApiService.getPlanningYears(),
      ])

      departments.value = departmentData.filter(item => item.isActive)

      planningYears.value = planningYearData.filter(
        item => item.status.toLowerCase() === 'open',
      )
    } catch (err) {
      error.value =
        err instanceof Error
          ? err.message
          : 'Unable to load departments and planning years.'
    } finally {
      isLoading.value = false
    }
  }

  async function loadSections(departmentId: number): Promise<void> {
    sections.value = []
    error.value = ''

    try {
      const result =
        await masterDataApiService.getDepartmentSections(departmentId)

      sections.value = result.filter(section => section.isActive)
    } catch (err) {
      error.value =
        err instanceof Error
          ? err.message
          : 'Unable to load department sections.'
    }
  }

  async function loadCategories(
    planningYearId: number,
  ): Promise<void> {
    planningYearCategories.value = []
    items.value = []
    error.value = ''

    try {
      const result =
        await masterDataApiService.getPlanningYearCategories(
          planningYearId,
        )

      planningYearCategories.value = result.filter(
        category => category.isActive,
      )
    } catch (err) {
      error.value =
        err instanceof Error
          ? err.message
          : 'Unable to load planning year categories.'
    }
  }

  async function loadItems(
  planningYear: number,
  categoryId: number,
 ): Promise<void> {
  items.value = []
  error.value = ''

  try {
    const result = await masterDataApiService.getPlanningYearCatalogueItems(
      planningYear,
      categoryId,
    )

    items.value = result
   } catch (err) {
    error.value = err instanceof Error
      ? err.message
      : 'Unable to load planning year catalogue items.'
   }
 } 

 async function loadAllItems(
  planningYear: number,
): Promise<void> {
  items.value = []
  error.value = ''

  try {
    const results = await Promise.all(
      planningYearCategories.value.map(category =>
        masterDataApiService.getPlanningYearCatalogueItems(
          planningYear,
          category.categoryId,
        ),
      ),
    )

    items.value = results.flat()
  } catch (err) {
    error.value =
      err instanceof Error
        ? err.message
        : 'Unable to load all planning year catalogue items.'
  }
}

  return {
  departments,
  planningYears,
  planningYearCategories,
  sections,
  items,
  isLoading,
  error,
  loadInitialData,
  loadSections,
  loadCategories,
  loadItems,
  loadAllItems,
}
}
