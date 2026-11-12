import { computed, onMounted, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import type { PPMPItem } from '../models/ppmp.types'
import type {
  PlanningYearCategoryDto,
  DepartmentSectionDto,
  PlanningYearCatalogueItemDto,
} from '../models/master-data.types'
import type { CreatePPMPRequest } from '../models/ppmp-api.types'

import { useAuthStore } from '@/stores/auth'
import { ppmpApiService } from '../services/ppmpApiService'
import { userCategoryPreferenceApiService } from '../services/userCategoryPreferenceApiService'

import { usePPMPFormViewModel } from './usePPMPFormViewModel'
import { usePPMPMasterData } from './usePPMPMasterData'

interface SectionAllocationDraft {
  sectionId: string
  sectionName: string
  quantity: number
}

interface ManualItemDraft {
  generalDescriptionAndObjective: string
  projectType: string
  preProcurementConference: boolean | null
  unit: string
  unitPrice: number
  sectionAllocations: SectionAllocationDraft[]
}

interface CatalogueItemDraft extends ManualItemDraft {
  catalogueItemId: number | null
}

export type CreatePPMPViewModel = ReturnType<typeof useCreatePPMPViewModel>

export function useCreatePPMPViewModel() {
  const router = useRouter()
  const route = useRoute()
  const authStore = useAuthStore()

  const isEditMode = computed(() => Boolean(route.params.id))
  const currentUserName = computed(() => authStore.user?.fullName ?? '')

  async function navigateToPPMPList(): Promise<void> {
    await router.push('/ppmp')
  }

  const { form, errors, isSaving, estimatedBudget, addItem: addFormItem } = usePPMPFormViewModel()

  const {
    departments,
    planningYears,
    planningYearCategories,
    sections,
    isLoading,
    error: masterDataError,
    loadInitialData,
    loadSections,
    loadCategories,
    loadItems,
  } = usePPMPMasterData()

  const selectedPlanningYearId = ref<number | null>(null)
  const preferredCategories = ref<PlanningYearCategoryDto[]>([])
  const hasLoadedPreferences = ref(false)
  const isLoadingCategories = ref(false)

  const activeCategoryId = ref<number | null>(null)
  const itemEntryMode = ref<'catalogue' | 'manual'>('catalogue')
  const catalogueItems = ref<PlanningYearCatalogueItemDto[]>([])
  const isLoadingCatalogueItems = ref(false)
  const catalogueSearch = ref('')
  const catalogueItemDrafts = ref<Record<number, CatalogueItemDraft>>({})
  const expandedAllocationRows = ref<number[]>([])
  const pageError = ref('')

  const itemDrafts = ref<Record<number, ManualItemDraft>>({})
  const editingLineId = ref<number | null>(null)

  let nextLineId = Date.now()
  let catalogueRequestId = 0

  const currentDepartment = computed(() => {
    const departmentId = authStore.user?.departmentId

    return departments.value.find((department) => department.departmentId === departmentId)
  })

  const activeCategories = computed(() => preferredCategories.value)

  const selectedItems = computed(() => form.value.items.filter((item) => Number(item.quantity) > 0))

  const errorMessage = computed(
    () => pageError.value || errors.value[0] || masterDataError.value || '',
  )

  const activeCategory = computed(
    () =>
      activeCategories.value.find((category) => category.categoryId === activeCategoryId.value) ??
      null,
  )

  const activeSections = computed(() => sections.value.filter((section) => section.isActive))

  function createEmptyDraft(): ManualItemDraft {
    return {
      generalDescriptionAndObjective: '',
      projectType: '',
      preProcurementConference: null,
      unit: '',
      unitPrice: 0,
      sectionAllocations: [],
    }
  }

  function createEmptyCatalogueDraft(): CatalogueItemDraft {
    return {
      catalogueItemId: null,
      generalDescriptionAndObjective: '',
      projectType: '',
      preProcurementConference: null,
      unit: '',
      unitPrice: 0,
      sectionAllocations: [],
    }
  }

  function getCatalogueItemDraft(categoryId: number): CatalogueItemDraft {
    if (!catalogueItemDrafts.value[categoryId]) {
      catalogueItemDrafts.value[categoryId] = createEmptyCatalogueDraft()
    }

    return catalogueItemDrafts.value[categoryId]!
  }

  function getItemDraft(categoryId: number): ManualItemDraft {
    if (!itemDrafts.value[categoryId]) {
      itemDrafts.value[categoryId] = createEmptyDraft()
    }

    return itemDrafts.value[categoryId]!
  }

  function resetItemDraft(categoryId: number): void {
    itemDrafts.value[categoryId] = createEmptyDraft()
  }

  async function selectCategoryTab(categoryId: number): Promise<void> {
    activeCategoryId.value = categoryId
    editingLineId.value = null
    pageError.value = ''

    if (selectedPlanningYearId.value) {
      await loadCatalogueItems(selectedPlanningYearId.value, categoryId)
    }
  }

  async function loadCatalogueItems(planningYearId: number, categoryId: number): Promise<void> {
    const requestId = ++catalogueRequestId
    isLoadingCatalogueItems.value = true
    catalogueItems.value = []

    try {
      const planningYear = planningYears.value.find(
        (year) => year.planningYearId === planningYearId,
      )
      if (!planningYear) {
        throw new Error('The selected planning year could not be found.')
      }

      const result = await loadItems(planningYear.year, categoryId)
      if (requestId !== catalogueRequestId) return

      catalogueItems.value = result
      if (masterDataError.value) {
        pageError.value = masterDataError.value
      }
    } catch (error) {
      if (requestId !== catalogueRequestId) return

      pageError.value =
        error instanceof Error ? error.message : 'Unable to load planning year catalogue items.'
    } finally {
      if (requestId === catalogueRequestId) {
        isLoadingCatalogueItems.value = false
      }
    }
  }

  const filteredCatalogueItems = computed(() => {
    const search = catalogueSearch.value.trim().toLowerCase()
    if (!search) return catalogueItems.value

    return catalogueItems.value.filter((item) =>
      [item.itemCode, item.itemName, item.itemType, item.unit, item.specifications].some((value) =>
        value?.toLowerCase().includes(search),
      ),
    )
  })

  function selectCatalogueItem(
    category: PlanningYearCategoryDto,
    catalogueItem: PlanningYearCatalogueItemDto,
  ): void {
    const draft = getCatalogueItemDraft(category.categoryId)
    draft.catalogueItemId = catalogueItem.itemId
    draft.generalDescriptionAndObjective = catalogueItem.itemName
    draft.unit = catalogueItem.unit
    draft.unitPrice = catalogueItem.abcPerUnit
    draft.projectType = catalogueItem.itemType ?? ''
    pageError.value = ''
  }

  function addCatalogueItem(category: PlanningYearCategoryDto): void {
    pageError.value = ''
    const draft = getCatalogueItemDraft(category.categoryId)
    const catalogueItem = catalogueItems.value.find((item) => item.itemId === draft.catalogueItemId)

    if (!catalogueItem) {
      pageError.value = 'Please select an item from the catalogue.'
      return
    }

    if (!draft.generalDescriptionAndObjective.trim()) {
      pageError.value =
        'Please enter the general description and objective of the project to be procured.'
      return
    }

    if (!draft.projectType.trim()) {
      pageError.value = 'Please enter the type of project to be procured.'
      return
    }

    if (draft.preProcurementConference === null) {
      pageError.value = 'Please indicate whether a pre-procurement conference is applicable.'
      return
    }

    if (!draft.unit.trim()) {
      pageError.value = 'Please enter the unit of issue.'
      return
    }

    if (!Number.isFinite(Number(draft.unitPrice)) || Number(draft.unitPrice) < 0) {
      pageError.value = 'ABC per unit must be a valid non-negative amount.'
      return
    }

    if (draft.sectionAllocations.length === 0) {
      pageError.value = 'Please select at least one department section for this item.'
      return
    }

    const invalidAllocation = draft.sectionAllocations.some(
      (allocation) =>
        !Number.isFinite(Number(allocation.quantity)) || Number(allocation.quantity) < 0,
    )

    if (invalidAllocation) {
      pageError.value = 'Section quantities must be valid non-negative numbers.'
      return
    }

    const quantity = draftTotalQuantity(draft)
    if (quantity <= 0) {
      pageError.value = 'The total requested quantity must be greater than zero.'
      return
    }

    addFormItem({
      lineId: nextLineId++,
      catalogItemId: catalogueItem.itemId,
      categoryId: category.categoryId,
      code: catalogueItem.itemCode,
      name: catalogueItem.itemName,
      category: category.categoryName,
      itemType: catalogueItem.itemType,
      unit: draft.unit.trim(),
      specifications: catalogueItem.specifications ?? '',
      generalDescriptionAndObjective: draft.generalDescriptionAndObjective.trim(),
      projectType: draft.projectType.trim(),
      preProcurementConference: draft.preProcurementConference,
      quantity,
      unitPrice: Number(draft.unitPrice),
      sectionAllocations: draft.sectionAllocations.map((allocation) => ({
        ...allocation,
        quantity: Number(allocation.quantity),
      })),
    })

    catalogueItemDrafts.value[category.categoryId] = createEmptyCatalogueDraft()
    catalogueSearch.value = ''
    errors.value = []
  }

  function getItemEntryLabel(item: PPMPItem): string {
    return item.catalogItemId === null ? 'Manual entry' : 'Catalogue item'
  }

  function getItemsForCategory(categoryId: number): PPMPItem[] {
    return form.value.items.filter((item) => item.categoryId === categoryId)
  }

  function selectedItemsInCategory(categoryId: number): number {
    return getItemsForCategory(categoryId).filter((item) => Number(item.quantity) > 0).length
  }

  function categoryTotal(categoryId: number): number {
    return getItemsForCategory(categoryId).reduce(
      (sum, item) => sum + Number(item.quantity || 0) * Number(item.unitPrice || 0),
      0,
    )
  }

  function toggleAllocationRow(lineId: number): void {
    if (expandedAllocationRows.value.includes(lineId)) {
      expandedAllocationRows.value = expandedAllocationRows.value.filter((id) => id !== lineId)
    } else {
      expandedAllocationRows.value.push(lineId)
    }
  }

  function getAvailableSections(allocations: SectionAllocationDraft[]): DepartmentSectionDto[] {
    const selectedIds = new Set(allocations.map((allocation) => Number(allocation.sectionId)))

    return activeSections.value.filter((section) => !selectedIds.has(section.sectionId))
  }

  function addSectionToDraft(draft: ManualItemDraft, event: Event): void {
    const select = event.target as HTMLSelectElement
    const sectionId = Number(select.value)

    if (!sectionId) return

    if (draft.sectionAllocations.some((allocation) => Number(allocation.sectionId) === sectionId)) {
      select.value = ''
      return
    }

    const section = activeSections.value.find((entry) => entry.sectionId === sectionId)

    if (!section) return

    draft.sectionAllocations.push({
      sectionId: String(section.sectionId),
      sectionName: section.sectionName,
      quantity: 0,
    })

    select.value = ''
  }

  function addDraftSection(categoryId: number, event: Event): void {
    addSectionToDraft(getItemDraft(categoryId), event)
  }

  function addCatalogueDraftSection(categoryId: number, event: Event): void {
    addSectionToDraft(getCatalogueItemDraft(categoryId), event)
  }

  function removeDraftSection(categoryId: number, sectionId: string): void {
    const draft = getItemDraft(categoryId)

    draft.sectionAllocations = draft.sectionAllocations.filter(
      (allocation) => allocation.sectionId !== sectionId,
    )
  }

  function removeCatalogueDraftSection(categoryId: number, sectionId: string): void {
    const draft = getCatalogueItemDraft(categoryId)

    draft.sectionAllocations = draft.sectionAllocations.filter(
      (allocation) => allocation.sectionId !== sectionId,
    )
  }

  function draftTotalQuantity(draft: ManualItemDraft): number {
    return draft.sectionAllocations.reduce(
      (sum, allocation) => sum + Number(allocation.quantity || 0),
      0,
    )
  }

  function addManualItem(category: PlanningYearCategoryDto): void {
    pageError.value = ''

    const draft = getItemDraft(category.categoryId)

    if (!draft.generalDescriptionAndObjective.trim()) {
      pageError.value =
        'Please enter the general description and objective of the project to be procured.'
      return
    }

    if (!draft.projectType.trim()) {
      pageError.value = 'Please enter the type of project to be procured.'
      return
    }

    if (draft.preProcurementConference === null) {
      pageError.value = 'Please indicate whether a pre-procurement conference is applicable.'
      return
    }

    if (!draft.unit.trim()) {
      pageError.value = 'Please enter the unit of issue.'
      return
    }

    if (!Number.isFinite(Number(draft.unitPrice)) || Number(draft.unitPrice) < 0) {
      pageError.value = 'ABC per unit must be a valid non-negative amount.'
      return
    }

    if (draft.sectionAllocations.length === 0) {
      pageError.value = 'Please select at least one department section for this item.'
      return
    }

    const invalidAllocation = draft.sectionAllocations.some(
      (allocation) =>
        !Number.isFinite(Number(allocation.quantity)) || Number(allocation.quantity) < 0,
    )

    if (invalidAllocation) {
      pageError.value = 'Section quantities must be valid non-negative numbers.'
      return
    }

    const quantity = draftTotalQuantity(draft)

    if (quantity <= 0) {
      pageError.value = 'The total requested quantity must be greater than zero.'
      return
    }

    const item: PPMPItem = {
      lineId: nextLineId++,
      catalogItemId: null,
      categoryId: category.categoryId,

      // Legacy compatibility properties.
      code: '',
      name: draft.generalDescriptionAndObjective.trim(),
      category: category.categoryName,
      itemType: draft.projectType.trim(),
      unit: draft.unit.trim(),
      specifications: '',

      // Official PPMP fields.
      generalDescriptionAndObjective: draft.generalDescriptionAndObjective.trim(),
      projectType: draft.projectType.trim(),
      preProcurementConference: draft.preProcurementConference,

      quantity,
      unitPrice: Number(draft.unitPrice),

      sectionAllocations: draft.sectionAllocations.map((allocation) => ({
        ...allocation,
        quantity: Number(allocation.quantity),
      })),
    }

    addFormItem(item)

    resetItemDraft(category.categoryId)
    errors.value = []
  }

  function startEditingItem(item: PPMPItem): void {
    editingLineId.value = item.lineId
    pageError.value = ''
  }

  function cancelEditingItem(): void {
    editingLineId.value = null
    pageError.value = ''
  }

  function updateItemField(
    item: PPMPItem,
    field:
      | 'generalDescriptionAndObjective'
      | 'projectType'
      | 'unit'
      | 'unitPrice'
      | 'preProcurementConference',
    value: string | number | boolean | null,
  ): void {
    if (field === 'unitPrice') {
      item.unitPrice = Math.max(0, Number(value) || 0)
      return
    }

    if (field === 'preProcurementConference') {
      item.preProcurementConference = value === true
      return
    }

    if (field === 'generalDescriptionAndObjective') {
      const description = String(value ?? '')
      item.generalDescriptionAndObjective = description

      // Keep the legacy display property synchronized.
      item.name = description
      return
    }

    if (field === 'projectType') {
      const projectType = String(value ?? '')
      item.projectType = projectType

      // Keep the legacy display property synchronized.
      item.itemType = projectType || null
      return
    }

    if (field === 'unit') {
      item.unit = String(value ?? '')
    }
  }

  function addItemSection(item: PPMPItem, event: Event): void {
    const select = event.target as HTMLSelectElement
    const sectionId = Number(select.value)

    if (!sectionId) return

    const allocations = item.sectionAllocations ?? []

    if (allocations.some((allocation) => Number(allocation.sectionId) === sectionId)) {
      select.value = ''
      return
    }

    const section = activeSections.value.find((entry) => entry.sectionId === sectionId)

    if (!section) return

    allocations.push({
      sectionId: String(section.sectionId),
      sectionName: section.sectionName,
      quantity: 0,
    })

    item.sectionAllocations = allocations
    updateQuantityFromAllocations(item)

    select.value = ''
  }

  function removeItemSection(item: PPMPItem, sectionId: string): void {
    item.sectionAllocations = (item.sectionAllocations ?? []).filter(
      (allocation) => allocation.sectionId !== sectionId,
    )

    updateQuantityFromAllocations(item)
  }

  function updateQuantityFromAllocations(item: PPMPItem): void {
    const total =
      item.sectionAllocations?.reduce(
        (sum, allocation) => sum + Number(allocation.quantity || 0),
        0,
      ) ?? 0

    item.quantity = total
  }

  function allocationTotal(item: PPMPItem): number {
    return (item.sectionAllocations ?? []).reduce(
      (sum, allocation) => sum + Number(allocation.quantity || 0),
      0,
    )
  }

  function isAllocationBalanced(item: PPMPItem): boolean {
    return Math.abs(allocationTotal(item) - Number(item.quantity || 0)) < 0.000001
  }

  function removeItem(lineId: number): void {
    form.value.items = form.value.items.filter((item) => item.lineId !== lineId)

    expandedAllocationRows.value = expandedAllocationRows.value.filter((id) => id !== lineId)

    if (editingLineId.value === lineId) {
      editingLineId.value = null
    }

    errors.value = []
    pageError.value = ''
  }

  function validateItems(): boolean {
    if (selectedItems.value.length === 0) {
      pageError.value = 'Please add at least one item with a quantity greater than zero.'
      return false
    }

    for (let index = 0; index < selectedItems.value.length; index++) {
      const item = selectedItems.value[index]!

      const description = item.generalDescriptionAndObjective ?? item.name

      const projectType = item.projectType ?? item.itemType ?? ''

      if (!description.trim()) {
        pageError.value = `Item ${index + 1}: General description and objective are required.`
        return false
      }

      if (!projectType.trim()) {
        pageError.value = `Item ${index + 1}: Type of project is required.`
        return false
      }

      if (item.preProcurementConference === undefined) {
        pageError.value = `Item ${index + 1}: Please indicate Yes or No for the pre-procurement conference.`
        return false
      }

      if (!item.unit.trim()) {
        pageError.value = `Item ${index + 1}: Unit of issue is required.`
        return false
      }

      if (!Number.isFinite(Number(item.quantity)) || Number(item.quantity) <= 0) {
        pageError.value = `Item ${index + 1}: Total quantity must be greater than zero.`
        return false
      }

      if (!Number.isFinite(Number(item.unitPrice)) || Number(item.unitPrice) < 0) {
        pageError.value = `Item ${index + 1}: ABC per unit must be a valid non-negative amount.`
        return false
      }
    }

    return true
  }

  function validateAllocations(): boolean {
    for (let index = 0; index < selectedItems.value.length; index++) {
      const item = selectedItems.value[index]!
      const allocations = item.sectionAllocations ?? []

      if (allocations.length === 0) {
        pageError.value = `Item ${index + 1}: Select at least one department section.`
        return false
      }

      const sectionIds = allocations.map((allocation) => Number(allocation.sectionId))

      if (new Set(sectionIds).size !== sectionIds.length) {
        pageError.value = `Item ${index + 1}: A department section cannot be selected more than once.`
        return false
      }

      const invalidAllocation = allocations.some(
        (allocation) =>
          !Number.isFinite(Number(allocation.quantity)) || Number(allocation.quantity) < 0,
      )

      if (invalidAllocation) {
        pageError.value = `Item ${index + 1}: Section quantities must be valid non-negative numbers.`
        return false
      }

      const hasPositiveAllocation = allocations.some(
        (allocation) => Number(allocation.quantity) > 0,
      )

      if (!hasPositiveAllocation) {
        pageError.value = `Item ${index + 1}: Allocate a positive quantity to at least one section.`
        return false
      }

      const allocated = allocationTotal(item)

      if (Math.abs(allocated - Number(item.quantity)) > 0.000001) {
        pageError.value = `Item ${index + 1}: The total quantity must equal the sum of its section quantities.`
        return false
      }
    }

    return true
  }

  async function loadPreferredCategories(planningYearId: number): Promise<void> {
    preferredCategories.value = []
    hasLoadedPreferences.value = false
    activeCategoryId.value = null
    pageError.value = ''
    isLoadingCategories.value = true

    try {
      const preferences = await userCategoryPreferenceApiService.getPreferences(planningYearId)

      const preferredCategoryIds = new Set(
        preferences.map((preference) => preference.planningYearCategoryId),
      )

      preferredCategories.value = planningYearCategories.value.filter((category) =>
        preferredCategoryIds.has(category.planningYearCategoryId),
      )

      hasLoadedPreferences.value = true

      const firstCategory = preferredCategories.value[0]

      if (firstCategory) {
        activeCategoryId.value = firstCategory.categoryId
        await loadCatalogueItems(planningYearId, firstCategory.categoryId)
      }
    } catch (error) {
      pageError.value =
        error instanceof Error ? error.message : 'Unable to load your preferred categories.'
    } finally {
      isLoadingCategories.value = false
    }
  }

  onMounted(async () => {
    if (isEditMode.value) {
      pageError.value = 'Editing an existing Draft through the backend is not available yet.'
      return
    }

    await loadInitialData()

    if (masterDataError.value) return

    const departmentId = authStore.user?.departmentId

    if (!departmentId) {
      pageError.value =
        'Your account does not have an assigned department. Please contact the System Administrator.'
      return
    }

    const department = departments.value.find((entry) => entry.departmentId === departmentId)

    if (!department) {
      pageError.value = 'Your assigned department could not be found or is inactive.'
      return
    }

    form.value.department = department.departmentName

    await loadSections(departmentId)

    if (masterDataError.value) return

    if (activeSections.value.length === 0) {
      pageError.value = 'No active sections are configured for your department.'
      return
    }

    const firstPlanningYear = planningYears.value[0]

    if (!firstPlanningYear) {
      pageError.value = 'There are no open planning years available.'
      return
    }

    selectedPlanningYearId.value = firstPlanningYear.planningYearId

    form.value.planningYear = firstPlanningYear.year

    await loadCategories(firstPlanningYear.planningYearId)

    if (masterDataError.value) return

    await loadPreferredCategories(firstPlanningYear.planningYearId)
  })

  async function onPlanningYearChange(): Promise<void> {
    const planningYear = planningYears.value.find(
      (year) => year.planningYearId === selectedPlanningYearId.value,
    )

    if (!planningYear) return

    pageError.value = ''
    errors.value = []
    form.value.planningYear = planningYear.year
    form.value.items = []
    itemDrafts.value = {}
    catalogueItems.value = []
    catalogueItemDrafts.value = {}
    catalogueSearch.value = ''
    expandedAllocationRows.value = []
    editingLineId.value = null

    await loadCategories(planningYear.planningYearId)

    if (masterDataError.value) return

    await loadPreferredCategories(planningYear.planningYearId)
  }

  function buildCreateRequest(): CreatePPMPRequest {
    const departmentId = authStore.user?.departmentId
    const planningYearId = selectedPlanningYearId.value

    if (!departmentId || !planningYearId) {
      throw new Error('Your department or planning year is not available.')
    }

    return {
      departmentId,
      planningYearId,
      items: selectedItems.value.map((item) => {
        const quantity = allocationTotal(item)

        const description = item.generalDescriptionAndObjective ?? item.name

        const projectType = item.projectType ?? item.itemType ?? ''

        return {
          itemId: item.catalogItemId,
          categoryId: item.categoryId,

          // Legacy backend compatibility fields.
          // The End-User form does not display these separately.
          itemCode: item.code,
          itemDescription: description.trim(),
          itemType: item.itemType,
          unit: item.unit.trim(),
          specifications: item.specifications || null,

          // Official PPMP fields.
          generalDescriptionAndObjective: description.trim(),
          projectType: projectType.trim(),
          preProcurementConference: item.preProcurementConference === true,

          quantity,
          unitPrice: Number(item.unitPrice),

          sectionAllocations: (item.sectionAllocations ?? [])
            .filter((allocation) => Number(allocation.quantity) > 0)
            .map((allocation) => ({
              sectionId: Number(allocation.sectionId),
              quantity: Number(allocation.quantity),
            })),
        }
      }),
    }
  }

  async function savePPMP(shouldSubmit = false): Promise<void> {
    errors.value = []
    pageError.value = ''

    if (isEditMode.value) {
      pageError.value = 'Backend Draft editing is not implemented yet.'
      return
    }

    if (!selectedPlanningYearId.value) {
      pageError.value = 'Please select a planning year.'
      return
    }

    if (!validateItems()) return
    if (!validateAllocations()) return

    if (
      shouldSubmit &&
      !window.confirm('Are you sure you want to submit this PPMP for Unit Head approval?')
    ) {
      return
    }

    isSaving.value = true
    let createdPPMPId: number | null = null

    try {
      const request = buildCreateRequest()
      const created = await ppmpApiService.create(request)

      createdPPMPId = created.ppmpId

      if (shouldSubmit) {
        await ppmpApiService.submit(created.ppmpId)

        window.alert(
          `PPMP ${created.ppmpReference} was successfully submitted for Unit Head approval.`,
        )
      } else {
        window.alert(`PPMP ${created.ppmpReference} was successfully saved as a Draft.`)
      }

      await navigateToPPMPList()
    } catch (error) {
      if (createdPPMPId !== null && shouldSubmit) {
        pageError.value = `The PPMP was created successfully as a Draft (ID: ${createdPPMPId}), but submission failed.`
      } else {
        pageError.value = error instanceof Error ? error.message : 'Unable to save the PPMP.'
      }
    } finally {
      isSaving.value = false
    }
  }

  function formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-PH', {
      style: 'currency',
      currency: 'PHP',
      minimumFractionDigits: 2,
    }).format(Number(amount) || 0)
  }

  function getDescription(item: PPMPItem): string {
    return item.generalDescriptionAndObjective ?? item.name
  }

  function getProjectType(item: PPMPItem): string {
    return item.projectType ?? item.itemType ?? ''
  }

  return {
    activeCategories,
    activeCategory,
    activeCategoryId,
    addCatalogueDraftSection,
    addCatalogueItem,
    addDraftSection,
    addItemSection,
    addManualItem,
    allocationTotal,
    cancelEditingItem,
    catalogueItems,
    catalogueSearch,
    categoryTotal,
    currentDepartment,
    departments,
    draftTotalQuantity,
    editingLineId,
    errorMessage,
    estimatedBudget,
    expandedAllocationRows,
    filteredCatalogueItems,
    form,
    formatCurrency,
    getAvailableSections,
    getCatalogueItemDraft,
    getDescription,
    getItemDraft,
    getItemEntryLabel,
    getItemsForCategory,
    getProjectType,
    hasLoadedPreferences,
    isAllocationBalanced,
    isLoading,
    isLoadingCatalogueItems,
    isLoadingCategories,
    isSaving,
    itemEntryMode,
    onPlanningYearChange,
    planningYearCategories,
    planningYears,
    preferredCategories,
    removeCatalogueDraftSection,
    removeDraftSection,
    removeItem,
    removeItemSection,
    currentUserName,
    navigateToPPMPList,
    savePPMP,
    sections,
    selectCatalogueItem,
    selectCategoryTab,
    selectedItems,
    selectedItemsInCategory,
    selectedPlanningYearId,
    startEditingItem,
    toggleAllocationRow,
    updateItemField,
    updateQuantityFromAllocations,
  }
}
