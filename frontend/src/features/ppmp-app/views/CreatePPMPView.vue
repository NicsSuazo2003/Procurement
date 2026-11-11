
<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'

import {
  ArrowLeft,
  Trash2,
  Save,
  Send,
  ChevronDown,
  AlertCircle,
  FileText,
  Plus,
  Pencil,
  X,
} from '@lucide/vue'

import type { PPMPItem } from '../models/ppmp.types'
import type {
  PlanningYearCategoryDto,
  DepartmentSectionDto,
} from '../models/master-data.types'
import type { CreatePPMPRequest } from '../models/ppmp-api.types'

import { useAuthStore } from '@/stores/auth'
import { ppmpApiService } from '../services/ppmpApiService'
import { userCategoryPreferenceApiService } from '../services/userCategoryPreferenceApiService'

import { usePPMPFormViewModel } from '../viewmodels/usePPMPFormViewModel'
import { usePPMPMasterData } from '../viewmodels/usePPMPMasterData'

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

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const isEditMode = computed(() => Boolean(route.params.id))

const {
  form,
  errors,
  isSaving,
  estimatedBudget,
  addItem: addFormItem,
} = usePPMPFormViewModel()

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
} = usePPMPMasterData()

const selectedPlanningYearId = ref<number | null>(null)
const preferredCategories = ref<PlanningYearCategoryDto[]>([])
const hasLoadedPreferences = ref(false)
const isLoadingCategories = ref(false)

const activeCategoryId = ref<number | null>(null)
const expandedAllocationRows = ref<number[]>([])
const pageError = ref('')

const itemDrafts = ref<Record<number, ManualItemDraft>>({})
const editingLineId = ref<number | null>(null)

let nextLineId = Date.now()

const currentDepartment = computed(() => {
  const departmentId = authStore.user?.departmentId

  return departments.value.find(
    department => department.departmentId === departmentId,
  )
})

const activeCategories = computed(() => preferredCategories.value)

const selectedItems = computed(() =>
  form.value.items.filter(item => Number(item.quantity) > 0),
)

const errorMessage = computed(() =>
  pageError.value || errors.value[0] || masterDataError.value || '',
)

const activeCategory = computed(
  () =>
    activeCategories.value.find(
      category => category.categoryId === activeCategoryId.value,
    ) ?? null,
)

const activeSections = computed(() =>
  sections.value.filter(section => section.isActive),
)

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

function getItemDraft(categoryId: number): ManualItemDraft {
  if (!itemDrafts.value[categoryId]) {
    itemDrafts.value[categoryId] = createEmptyDraft()
  }

  return itemDrafts.value[categoryId]!
}

function resetItemDraft(categoryId: number): void {
  itemDrafts.value[categoryId] = createEmptyDraft()
}

function selectCategoryTab(categoryId: number): void {
  activeCategoryId.value = categoryId
  editingLineId.value = null
  pageError.value = ''
}

function getItemsForCategory(categoryId: number): PPMPItem[] {
  return form.value.items.filter(
    item => item.categoryId === categoryId,
  )
}

function selectedItemsInCategory(categoryId: number): number {
  return getItemsForCategory(categoryId).filter(
    item => Number(item.quantity) > 0,
  ).length
}

function categoryTotal(categoryId: number): number {
  return getItemsForCategory(categoryId).reduce(
    (sum, item) =>
      sum + Number(item.quantity || 0) * Number(item.unitPrice || 0),
    0,
  )
}

function toggleAllocationRow(lineId: number): void {
  if (expandedAllocationRows.value.includes(lineId)) {
    expandedAllocationRows.value =
      expandedAllocationRows.value.filter(id => id !== lineId)
  } else {
    expandedAllocationRows.value.push(lineId)
  }
}

function getAvailableSections(
  allocations: SectionAllocationDraft[],
): DepartmentSectionDto[] {
  const selectedIds = new Set(
    allocations.map(allocation => Number(allocation.sectionId)),
  )

  return activeSections.value.filter(
    section => !selectedIds.has(section.sectionId),
  )
}

function addDraftSection(categoryId: number, event: Event): void {
  const select = event.target as HTMLSelectElement
  const sectionId = Number(select.value)

  if (!sectionId) return

  const draft = getItemDraft(categoryId)

  if (
    draft.sectionAllocations.some(
      allocation => Number(allocation.sectionId) === sectionId,
    )
  ) {
    select.value = ''
    return
  }

  const section = activeSections.value.find(
    entry => entry.sectionId === sectionId,
  )

  if (!section) return

  draft.sectionAllocations.push({
    sectionId: String(section.sectionId),
    sectionName: section.sectionName,
    quantity: 0,
  })

  select.value = ''
}

function removeDraftSection(
  categoryId: number,
  sectionId: string,
): void {
  const draft = getItemDraft(categoryId)

  draft.sectionAllocations =
    draft.sectionAllocations.filter(
      allocation => allocation.sectionId !== sectionId,
    )
}

function draftTotalQuantity(draft: ManualItemDraft): number {
  return draft.sectionAllocations.reduce(
    (sum, allocation) =>
      sum + Number(allocation.quantity || 0),
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
    pageError.value =
      'Please indicate whether a pre-procurement conference is applicable.'
    return
  }

  if (!draft.unit.trim()) {
    pageError.value = 'Please enter the unit of issue.'
    return
  }

  if (
    !Number.isFinite(Number(draft.unitPrice)) ||
    Number(draft.unitPrice) < 0
  ) {
    pageError.value = 'ABC per unit must be a valid non-negative amount.'
    return
  }

  if (draft.sectionAllocations.length === 0) {
    pageError.value =
      'Please select at least one department section for this item.'
    return
  }

  const invalidAllocation = draft.sectionAllocations.some(
    allocation =>
      !Number.isFinite(Number(allocation.quantity)) ||
      Number(allocation.quantity) < 0,
  )

  if (invalidAllocation) {
    pageError.value =
      'Section quantities must be valid non-negative numbers.'
    return
  }

  const quantity = draftTotalQuantity(draft)

  if (quantity <= 0) {
    pageError.value =
      'The total requested quantity must be greater than zero.'
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
    generalDescriptionAndObjective:
      draft.generalDescriptionAndObjective.trim(),
    projectType: draft.projectType.trim(),
    preProcurementConference: draft.preProcurementConference,

    quantity,
    unitPrice: Number(draft.unitPrice),

    sectionAllocations: draft.sectionAllocations.map(allocation => ({
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
  field: 'generalDescriptionAndObjective' | 'projectType' | 'unit' | 'unitPrice' | 'preProcurementConference',
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

  if (
    allocations.some(
      allocation => Number(allocation.sectionId) === sectionId,
    )
  ) {
    select.value = ''
    return
  }

  const section = activeSections.value.find(
    entry => entry.sectionId === sectionId,
  )

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

function removeItemSection(
  item: PPMPItem,
  sectionId: string,
): void {
  item.sectionAllocations =
    (item.sectionAllocations ?? []).filter(
      allocation => allocation.sectionId !== sectionId,
    )

  updateQuantityFromAllocations(item)
}

function updateQuantityFromAllocations(item: PPMPItem): void {
  const total =
    item.sectionAllocations?.reduce(
      (sum, allocation) =>
        sum + Number(allocation.quantity || 0),
      0,
    ) ?? 0

  item.quantity = total
}

function allocationTotal(item: PPMPItem): number {
  return (item.sectionAllocations ?? []).reduce(
    (sum, allocation) =>
      sum + Number(allocation.quantity || 0),
    0,
  )
}

function isAllocationBalanced(item: PPMPItem): boolean {
  return (
    Math.abs(
      allocationTotal(item) - Number(item.quantity || 0),
    ) < 0.000001
  )
}

function removeItem(lineId: number): void {
  form.value.items = form.value.items.filter(
    item => item.lineId !== lineId,
  )

  expandedAllocationRows.value =
    expandedAllocationRows.value.filter(id => id !== lineId)

  if (editingLineId.value === lineId) {
    editingLineId.value = null
  }

  errors.value = []
  pageError.value = ''
}

function validateItems(): boolean {
  if (selectedItems.value.length === 0) {
    pageError.value =
      'Please add at least one item with a quantity greater than zero.'
    return false
  }

  for (let index = 0; index < selectedItems.value.length; index++) {
    const item = selectedItems.value[index]!

    const description =
      item.generalDescriptionAndObjective ?? item.name

    const projectType =
      item.projectType ?? item.itemType ?? ''

    if (!description.trim()) {
      pageError.value =
        `Item ${index + 1}: General description and objective are required.`
      return false
    }

    if (!projectType.trim()) {
      pageError.value =
        `Item ${index + 1}: Type of project is required.`
      return false
    }

    if (item.preProcurementConference === undefined) {
      pageError.value =
        `Item ${index + 1}: Please indicate Yes or No for the pre-procurement conference.`
      return false
    }

    if (!item.unit.trim()) {
      pageError.value =
        `Item ${index + 1}: Unit of issue is required.`
      return false
    }

    if (
      !Number.isFinite(Number(item.quantity)) ||
      Number(item.quantity) <= 0
    ) {
      pageError.value =
        `Item ${index + 1}: Total quantity must be greater than zero.`
      return false
    }

    if (
      !Number.isFinite(Number(item.unitPrice)) ||
      Number(item.unitPrice) < 0
    ) {
      pageError.value =
        `Item ${index + 1}: ABC per unit must be a valid non-negative amount.`
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
      pageError.value =
        `Item ${index + 1}: Select at least one department section.`
      return false
    }

    const sectionIds = allocations.map(
      allocation => Number(allocation.sectionId),
    )

    if (new Set(sectionIds).size !== sectionIds.length) {
      pageError.value =
        `Item ${index + 1}: A department section cannot be selected more than once.`
      return false
    }

    const invalidAllocation = allocations.some(
      allocation =>
        !Number.isFinite(Number(allocation.quantity)) ||
        Number(allocation.quantity) < 0,
    )

    if (invalidAllocation) {
      pageError.value =
        `Item ${index + 1}: Section quantities must be valid non-negative numbers.`
      return false
    }

    const hasPositiveAllocation = allocations.some(
      allocation => Number(allocation.quantity) > 0,
    )

    if (!hasPositiveAllocation) {
      pageError.value =
        `Item ${index + 1}: Allocate a positive quantity to at least one section.`
      return false
    }

    const allocated = allocationTotal(item)

    if (
      Math.abs(allocated - Number(item.quantity)) > 0.000001
    ) {
      pageError.value =
        `Item ${index + 1}: The total quantity must equal the sum of its section quantities.`
      return false
    }
  }

  return true
}

async function loadPreferredCategories(
  planningYearId: number,
): Promise<void> {
  preferredCategories.value = []
  hasLoadedPreferences.value = false
  activeCategoryId.value = null
  pageError.value = ''
  isLoadingCategories.value = true

  try {
    const preferences =
      await userCategoryPreferenceApiService.getPreferences(
        planningYearId,
      )

    const preferredCategoryIds = new Set(
      preferences.map(
        preference => preference.planningYearCategoryId,
      ),
    )

    preferredCategories.value =
      planningYearCategories.value.filter(category =>
        preferredCategoryIds.has(
          category.planningYearCategoryId,
        ),
      )

    hasLoadedPreferences.value = true

    const firstCategory = preferredCategories.value[0]

    if (firstCategory) {
      activeCategoryId.value = firstCategory.categoryId
    }
  } catch (error) {
    pageError.value =
      error instanceof Error
        ? error.message
        : 'Unable to load your preferred categories.'
  } finally {
    isLoadingCategories.value = false
  }
}

onMounted(async () => {
  if (isEditMode.value) {
    pageError.value =
      'Editing an existing Draft through the backend is not available yet.'
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

  const department = departments.value.find(
    entry => entry.departmentId === departmentId,
  )

  if (!department) {
    pageError.value =
      'Your assigned department could not be found or is inactive.'
    return
  }

  form.value.department = department.departmentName

  await loadSections(departmentId)

  if (masterDataError.value) return

  if (activeSections.value.length === 0) {
    pageError.value =
      'No active sections are configured for your department.'
    return
  }

  const firstPlanningYear = planningYears.value[0]

  if (!firstPlanningYear) {
    pageError.value =
      'There are no open planning years available.'
    return
  }

  selectedPlanningYearId.value =
    firstPlanningYear.planningYearId

  form.value.planningYear = firstPlanningYear.year

  await loadCategories(firstPlanningYear.planningYearId)

  if (masterDataError.value) return

  await loadPreferredCategories(
    firstPlanningYear.planningYearId,
  )
})

async function onPlanningYearChange(): Promise<void> {
  const planningYear = planningYears.value.find(
    year =>
      year.planningYearId === selectedPlanningYearId.value,
  )

  if (!planningYear) return

  pageError.value = ''
  errors.value = []
  form.value.planningYear = planningYear.year
  form.value.items = []
  itemDrafts.value = {}
  expandedAllocationRows.value = []
  editingLineId.value = null

  await loadCategories(planningYear.planningYearId)

  if (masterDataError.value) return

  await loadPreferredCategories(
    planningYear.planningYearId,
  )
}

function buildCreateRequest(): CreatePPMPRequest {
  const departmentId = authStore.user?.departmentId
  const planningYearId = selectedPlanningYearId.value

  if (!departmentId || !planningYearId) {
    throw new Error(
      'Your department or planning year is not available.',
    )
  }

  return {
    departmentId,
    planningYearId,
    items: selectedItems.value.map(item => {
      const quantity = allocationTotal(item)

      const description =
        item.generalDescriptionAndObjective ?? item.name

      const projectType =
        item.projectType ?? item.itemType ?? ''

      return {
        itemId: null,
        categoryId: item.categoryId,

        // Legacy backend compatibility fields.
        // The End-User form does not display these separately.
        itemCode: '',
        itemDescription: description.trim(),
        itemType: null,
        unit: item.unit.trim(),
        specifications: null,

        // Official PPMP fields.
        generalDescriptionAndObjective: description.trim(),
        projectType: projectType.trim(),
        preProcurementConference:
          item.preProcurementConference === true,

        quantity,
        unitPrice: Number(item.unitPrice),

        sectionAllocations:
          (item.sectionAllocations ?? [])
            .filter(
              allocation =>
                Number(allocation.quantity) > 0,
            )
            .map(allocation => ({
              sectionId: Number(allocation.sectionId),
              quantity: Number(allocation.quantity),
            })),
      }
    }),
  }
}

async function savePPMP(
  shouldSubmit = false,
): Promise<void> {
  errors.value = []
  pageError.value = ''

  if (isEditMode.value) {
    pageError.value =
      'Backend Draft editing is not implemented yet.'
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
    !window.confirm(
      'Are you sure you want to submit this PPMP for Unit Head approval?',
    )
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
      window.alert(
        `PPMP ${created.ppmpReference} was successfully saved as a Draft.`,
      )
    }

    await router.push('/ppmp')
  } catch (error) {
    if (createdPPMPId !== null && shouldSubmit) {
      pageError.value =
        `The PPMP was created successfully as a Draft (ID: ${createdPPMPId}), but submission failed.`
    } else {
      pageError.value =
        error instanceof Error
          ? error.message
          : 'Unable to save the PPMP.'
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
</script>

<template>
  <div class="ppmp-shell mx-auto max-w-[1600px] space-y-4 pb-24">
    <!-- APPLICATION TOOLBAR -->
    <div class="print:hidden">
      <button
        type="button"
        class="mb-3 inline-flex items-center gap-2 text-sm font-medium text-slate-500 transition-colors hover:text-emerald-700"
        @click="router.push('/ppmp')"
      >
        <ArrowLeft :size="17" />
        Back to PPMP Management
      </button>

      <div class="flex flex-col justify-between gap-3 sm:flex-row sm:items-center">
        <div>
          <h1 class="text-2xl font-bold tracking-tight text-slate-900">
            Create Project Procurement Management Plan
          </h1>
          <p class="mt-1 text-sm text-slate-500">
            Encode proposed procurement projects and allocate each item's requested
            quantity to your department sections.
          </p>
        </div>

        <span
          class="inline-flex items-center gap-1.5 self-start rounded-full border border-emerald-200 bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700 sm:self-auto"
        >
          <span class="h-2 w-2 rounded-full bg-emerald-500"></span>
          {{ selectedItems.length }} item(s) budgeted
        </span>
      </div>
    </div>

    <!-- ERROR BANNER -->
    <div
      v-if="errorMessage"
      class="print:hidden flex items-start gap-3 rounded-xl border border-red-200 bg-red-50 p-4 text-sm text-red-800"
    >
      <AlertCircle :size="18" class="mt-0.5 shrink-0 text-red-600" />
      <p class="flex-1">{{ errorMessage }}</p>
    </div>

    <!-- INITIAL LOADING -->
    <div
      v-if="isLoading"
      class="print:hidden rounded-xl border border-slate-200 bg-white p-12 text-center text-sm text-slate-500 shadow-sm"
    >
      <div
        class="mb-3 inline-block h-8 w-8 animate-spin rounded-full border-4 border-emerald-600 border-t-transparent"
      ></div>
      <p>Loading departments and planning years...</p>
    </div>

    <template v-else>
      <!-- PLANNING YEAR TOOLBAR -->
      <div
        class="print:hidden sticky top-4 z-20 rounded-xl border border-slate-200 bg-white/95 p-4 shadow-md backdrop-blur-md"
      >
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label class="mb-1 block text-xs font-semibold uppercase tracking-wider text-slate-600">
              Planning Year
            </label>
            <select
              v-model.number="selectedPlanningYearId"
              class="w-full rounded-lg border border-slate-300 bg-slate-50 px-3 py-2.5 text-sm font-medium text-slate-800 outline-none transition-all focus:border-emerald-600 focus:bg-white focus:ring-1 focus:ring-emerald-200"
              :disabled="isLoadingCategories || isSaving"
              @change="onPlanningYearChange"
            >
              <option :value="null" disabled>Select planning year</option>
              <option
                v-for="year in planningYears"
                :key="year.planningYearId"
                :value="year.planningYearId"
              >
                Fiscal Year {{ year.year }}
              </option>
            </select>
          </div>

          <div class="flex items-end">
            <div class="w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5">
              <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
                Department
              </p>
              <p class="mt-1 text-sm font-semibold text-slate-800">
                {{ currentDepartment?.departmentName ?? form.department ?? 'Not assigned' }}
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- CATEGORY LOADING -->
      <div
        v-if="isLoadingCategories"
        class="print:hidden rounded-xl border border-slate-200 bg-white p-12 text-center text-sm text-slate-500 shadow-sm"
      >
        <div
          class="mb-2 inline-block h-6 w-6 animate-spin rounded-full border-2 border-emerald-600 border-t-transparent"
        ></div>
        <p>Loading your preferred categories...</p>
      </div>

      <template v-else>
        <!-- EMPTY STATES -->
        <div
          v-if="planningYearCategories.length === 0"
          class="print:hidden rounded-lg border border-amber-200 bg-amber-50 p-6 text-center text-sm text-amber-800"
        >
          No active categories are assigned to this planning year.
          Please contact the Procurement Office.
        </div>

        <div
          v-else-if="hasLoadedPreferences && preferredCategories.length === 0"
          class="print:hidden rounded-lg border border-amber-200 bg-amber-50 p-6 text-center"
        >
          <AlertCircle class="mx-auto mb-2 h-8 w-8 text-amber-600" />
          <h3 class="text-sm font-bold text-amber-900">
            No Preferred Categories Selected
          </h3>
          <p class="mt-1 text-sm text-amber-800">
            Please configure your category preferences for this planning year
            before creating a PPMP.
          </p>
        </div>

        <!-- CATEGORY TABS -->
        <div
          v-if="activeCategories.length > 0"
          class="print:hidden flex flex-wrap items-center gap-2 rounded-xl border border-slate-200 bg-white p-2 shadow-sm"
        >
          <button
            v-for="category in activeCategories"
            :key="category.categoryId"
            type="button"
            class="rounded-lg px-3 py-2 text-sm font-semibold transition-colors"
            :class="
              activeCategoryId === category.categoryId
                ? 'bg-emerald-600 text-white shadow-sm'
                : 'bg-slate-100 text-slate-700 hover:bg-slate-200'
            "
            @click="selectCategoryTab(category.categoryId)"
          >
            {{ category.categoryName }}
            <span class="ml-1 opacity-75">
              ({{ getItemsForCategory(category.categoryId).length }})
            </span>
          </button>
        </div>

        <!-- MANUAL ITEM ENTRY -->
        <section
          v-if="activeCategory"
          class="print:hidden overflow-hidden rounded-xl border border-slate-300 bg-white shadow-sm"
        >
          <div
            class="flex flex-col gap-3 border-b border-slate-200 bg-slate-50 px-6 py-5 sm:flex-row sm:items-center sm:justify-between"
          >
            <div class="min-w-0">
              <h3 class="text-xl font-bold text-slate-900">
                {{ activeCategory.categoryName }}
              </h3>
              <p class="mt-1 text-sm text-slate-600">
                {{ getItemsForCategory(activeCategory.categoryId).length }} manually
                encoded item(s) ·
                <span class="font-semibold text-emerald-700">
                  {{ selectedItemsInCategory(activeCategory.categoryId) }} with quantity
                </span>
              </p>
            </div>

            <span class="inline-flex items-center rounded-lg border border-blue-200 bg-blue-50 px-3 py-2 text-xs font-semibold text-blue-700">
              Manual item entry
            </span>
          </div>

          <!-- ADD ITEM FORM -->
          <div class="border-b border-slate-200 bg-white p-6">
            <div class="mb-4 flex items-center gap-2">
              <Plus :size="18" class="text-emerald-600" />
              <h4 class="text-base font-bold text-slate-900">
                Add Procurement Project
              </h4>
            </div>

            <div class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
              <div class="md:col-span-2 xl:col-span-4">
                <label class="mb-1 block text-xs font-semibold text-slate-600">
                  General Description and Objective of the Project to be Procured
                  <span class="text-red-500">*</span>
                </label>
                <textarea
                  v-model="getItemDraft(activeCategory.categoryId).generalDescriptionAndObjective"
                  rows="3"
                  maxlength="2000"
                  placeholder="Describe the procurement project, its purpose, intended use, and objective."
                  class="w-full resize-y rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
                  :disabled="isSaving"
                ></textarea>
                <p class="mt-1 text-xs text-slate-400">
                  Maximum 2,000 characters.
                </p>
              </div>

              <div>
                <label class="mb-1 block text-xs font-semibold text-slate-600">
                  Type of the Project to be Procured
                  <span class="text-red-500">*</span>
                </label>
                <input
                  v-model="getItemDraft(activeCategory.categoryId).projectType"
                  type="text"
                  maxlength="100"
                  placeholder="e.g. Goods"
                  class="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
                  :disabled="isSaving"
                />
              </div>

              <div>
                <label class="mb-1 block text-xs font-semibold text-slate-600">
                  Unit of Issue <span class="text-red-500">*</span>
                </label>
                <input
                  v-model="getItemDraft(activeCategory.categoryId).unit"
                  type="text"
                  maxlength="50"
                  placeholder="e.g. piece, box, ream"
                  class="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
                  :disabled="isSaving"
                />
              </div>

              <div>
                <label class="mb-1 block text-xs font-semibold text-slate-600">
                  ABC / Unit (₱) <span class="text-red-500">*</span>
                </label>
                <input
                  v-model.number="getItemDraft(activeCategory.categoryId).unitPrice"
                  type="number"
                  min="0"
                  step="0.01"
                  class="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
                  :disabled="isSaving"
                />
              </div>

              <div>
                <label class="mb-1 block text-xs font-semibold text-slate-600">
                  Pre-Procurement Conference
                  <span class="text-red-500">*</span>
                </label>
                <select
                  v-model="getItemDraft(activeCategory.categoryId).preProcurementConference"
                  class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
                  :disabled="isSaving"
                >
                  <option :value="null" disabled>Select Yes or No</option>
                  <option :value="true">Yes</option>
                  <option :value="false">No</option>
                </select>
              </div>
            </div>

            <!-- DRAFT SECTION ALLOCATION -->
            <div class="mt-6 rounded-xl border border-slate-200 bg-slate-50 p-4">
              <div class="flex flex-col justify-between gap-2 sm:flex-row sm:items-center">
                <div>
                  <h5 class="text-sm font-bold text-slate-800">
                    Department Section Requirements
                  </h5>
                  <p class="mt-1 text-xs text-slate-500">
                    Select each section that needs this procurement project and enter its requested quantity.
                  </p>
                </div>

                <div class="text-left sm:text-right">
                  <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">
                    Total Quantity
                  </p>
                  <p class="text-xl font-black text-emerald-700">
                    {{ draftTotalQuantity(getItemDraft(activeCategory.categoryId)) }}
                    {{ getItemDraft(activeCategory.categoryId).unit }}
                  </p>
                </div>
              </div>

              <div class="mt-4">
                <label class="mb-1 block text-xs font-semibold text-slate-600">
                  Add Department Section
                </label>
                <select
                  class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
                  :disabled="isSaving || getAvailableSections(getItemDraft(activeCategory.categoryId).sectionAllocations).length === 0"
                  @change="addDraftSection(activeCategory.categoryId, $event)"
                >
                  <option value="">
                    {{
                      getAvailableSections(getItemDraft(activeCategory.categoryId).sectionAllocations).length
                        ? 'Select a department section'
                        : 'All available sections have been added'
                    }}
                  </option>
                  <option
                    v-for="section in getAvailableSections(getItemDraft(activeCategory.categoryId).sectionAllocations)"
                    :key="section.sectionId"
                    :value="section.sectionId"
                  >
                    {{ section.sectionCode }} - {{ section.sectionName }}
                  </option>
                </select>
              </div>

              <div
                v-if="getItemDraft(activeCategory.categoryId).sectionAllocations.length === 0"
                class="mt-4 rounded-lg border border-dashed border-slate-300 bg-white px-4 py-6 text-center"
              >
                <p class="text-sm font-medium text-slate-600">
                  No department sections selected yet.
                </p>
                <p class="mt-1 text-xs text-slate-500">
                  Select a section above to begin allocating the requested quantity.
                </p>
              </div>

              <div v-else class="mt-4 space-y-2">
                <div
                  v-for="allocation in getItemDraft(activeCategory.categoryId).sectionAllocations"
                  :key="allocation.sectionId"
                  class="grid grid-cols-1 items-center gap-3 rounded-lg border border-slate-200 bg-white p-3 sm:grid-cols-[1fr_150px_auto]"
                >
                  <div>
                    <p class="text-sm font-semibold text-slate-800">
                      {{ allocation.sectionName }}
                    </p>
                    <p class="text-xs text-slate-500">
                      Section ID: {{ allocation.sectionId }}
                    </p>
                  </div>

                  <div>
                    <label class="mb-1 block text-xs font-medium text-slate-500">
                      Requested Quantity
                    </label>
                    <input
                      v-model.number="allocation.quantity"
                      type="number"
                      min="0"
                      step="1"
                      class="w-full rounded-lg border border-slate-300 px-3 py-2 text-center text-sm font-semibold outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
                      :disabled="isSaving"
                    />
                  </div>

                  <button
                    type="button"
                    class="inline-flex items-center justify-center gap-1 rounded-lg border border-red-200 px-3 py-2 text-xs font-semibold text-red-600 hover:bg-red-50"
                    :disabled="isSaving"
                    @click="removeDraftSection(activeCategory.categoryId, allocation.sectionId)"
                  >
                    <Trash2 :size="14" />
                    Remove
                  </button>
                </div>
              </div>

              <div class="mt-4 flex items-center justify-between border-t border-slate-200 pt-3">
                <span class="text-sm font-semibold text-slate-700">
                  Total requested quantity
                </span>
                <span class="text-lg font-black text-emerald-700">
                  {{ draftTotalQuantity(getItemDraft(activeCategory.categoryId)) }}
                  {{ getItemDraft(activeCategory.categoryId).unit }}
                </span>
              </div>
            </div>

            <div class="mt-4 flex flex-col justify-between gap-3 border-t border-slate-100 pt-4 sm:flex-row sm:items-center">
              <p class="text-xs text-slate-500">
                Items entered here are PPMP line items. They are not automatically
                added to the official item catalogue.
              </p>

              <button
                type="button"
                :disabled="isSaving"
                class="inline-flex items-center justify-center gap-2 rounded-lg bg-emerald-600 px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-50"
                @click="addManualItem(activeCategory)"
              >
                <Plus :size="16" />
                Add Item
              </button>
            </div>
          </div>

          <!-- ITEM LIST -->
          <div class="divide-y divide-slate-200">
            <div
              v-if="getItemsForCategory(activeCategory.categoryId).length === 0"
              class="px-6 py-14 text-center"
            >
              <FileText :size="32" class="mx-auto mb-3 text-slate-300" />
              <p class="text-base font-semibold text-slate-700">
                No items added to this category yet.
              </p>
              <p class="mt-1 text-sm text-slate-500">
                Use the form above to encode your first procurement project.
              </p>
            </div>

            <div
              v-for="(item, index) in getItemsForCategory(activeCategory.categoryId)"
              :key="item.lineId"
              class="px-6 py-5 transition-colors hover:bg-slate-50"
            >
              <!-- ITEM SUMMARY -->
              <div class="flex flex-col gap-4 lg:flex-row lg:items-start">
                <div class="min-w-0 flex-1">
                  <div class="flex flex-wrap items-center gap-2">
                    <span class="rounded-full bg-blue-50 px-2.5 py-0.5 text-[11px] font-bold uppercase tracking-wider text-blue-700">
                      Manual entry
                    </span>
                    <span
                      v-if="item.preProcurementConference === true"
                      class="rounded-full bg-amber-50 px-2.5 py-0.5 text-[11px] font-bold text-amber-700"
                    >
                      Pre-Procurement Conference: Yes
                    </span>
                    <span
                      v-else
                      class="rounded-full bg-slate-100 px-2.5 py-0.5 text-[11px] font-bold text-slate-600"
                    >
                      Pre-Procurement Conference: No
                    </span>
                  </div>

                  <p class="mt-2 text-base font-semibold leading-snug text-slate-900">
                    {{ getDescription(item) }}
                  </p>

                  <p class="mt-1 text-sm text-slate-500">
                    {{ getProjectType(item) }} · {{ item.unit }}
                  </p>

                  <div class="mt-3 flex flex-wrap gap-2">
                    <span
                      v-for="allocation in item.sectionAllocations"
                      :key="allocation.sectionId"
                      class="rounded-full border border-slate-200 bg-slate-50 px-2.5 py-1 text-xs font-medium text-slate-600"
                    >
                      {{ allocation.sectionName }}:
                      <strong>{{ allocation.quantity }}</strong>
                    </span>
                  </div>
                </div>

                <div class="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:w-[420px] lg:shrink-0">
                  <div class="text-right">
                    <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
                      Total Quantity
                    </p>
                    <p class="mt-1 text-sm font-semibold text-slate-700">
                      {{ item.quantity }} {{ item.unit }}
                    </p>
                  </div>

                  <div class="text-right">
                    <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
                      ABC / Unit
                    </p>
                    <p class="mt-1 text-sm font-semibold text-slate-700">
                      {{ formatCurrency(item.unitPrice) }}
                    </p>
                  </div>

                  <div class="col-span-2 text-right sm:col-span-1">
                    <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
                      Estimated Budget
                    </p>
                    <p class="mt-1 text-base font-bold text-emerald-700">
                      {{ formatCurrency(item.quantity * item.unitPrice) }}
                    </p>
                  </div>
                </div>

                <div class="flex flex-wrap items-center gap-2 lg:shrink-0">
                  <button
                    type="button"
                    class="inline-flex items-center gap-1.5 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-700 shadow-2xs hover:bg-slate-50"
                    @click="startEditingItem(item)"
                  >
                    <Pencil :size="15" />
                    Edit
                  </button>

                  <button
                    type="button"
                    class="inline-flex items-center gap-1.5 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-700 shadow-2xs hover:bg-slate-50"
                    @click="toggleAllocationRow(item.lineId)"
                  >
                    <ChevronDown
                      :size="15"
                      class="transition-transform"
                      :class="{ '-rotate-90': !expandedAllocationRows.includes(item.lineId) }"
                    />
                    Sections
                  </button>

                  <button
                    type="button"
                    class="rounded-lg border border-red-200 bg-white p-2 text-red-500 shadow-2xs hover:bg-red-50"
                    title="Remove item"
                    @click="removeItem(item.lineId)"
                  >
                    <Trash2 :size="16" />
                  </button>
                </div>
              </div>

              <!-- INLINE ITEM EDITOR -->
              <div
                v-if="editingLineId === item.lineId"
                class="mt-5 rounded-xl border border-emerald-200 bg-emerald-50/40 p-4"
              >
                <div class="mb-3 flex items-center justify-between">
                  <h5 class="text-sm font-bold text-slate-800">
                    Edit Procurement Project
                  </h5>
                  <button
                    type="button"
                    class="rounded-md p-1 text-slate-500 hover:bg-white"
                    @click="cancelEditingItem"
                  >
                    <X :size="17" />
                  </button>
                </div>

                <div class="grid grid-cols-1 gap-3 md:grid-cols-2 xl:grid-cols-4">
                  <div class="md:col-span-2 xl:col-span-4">
                    <label class="mb-1 block text-xs font-semibold text-slate-600">
                      General Description and Objective of the Project to be Procured
                    </label>
                    <textarea
                      :value="getDescription(item)"
                      rows="3"
                      maxlength="2000"
                      class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
                      @input="updateItemField(item, 'generalDescriptionAndObjective', ($event.target as HTMLTextAreaElement).value)"
                    ></textarea>
                  </div>

                  <div>
                    <label class="mb-1 block text-xs font-semibold text-slate-600">
                      Type of the Project to be Procured
                    </label>
                    <input
                      :value="getProjectType(item)"
                      type="text"
                      maxlength="100"
                      class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
                      @input="updateItemField(item, 'projectType', ($event.target as HTMLInputElement).value)"
                    />
                  </div>

                  <div>
                    <label class="mb-1 block text-xs font-semibold text-slate-600">
                      Unit of Issue
                    </label>
                    <input
                      :value="item.unit"
                      type="text"
                      maxlength="50"
                      class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
                      @input="updateItemField(item, 'unit', ($event.target as HTMLInputElement).value)"
                    />
                  </div>

                  <div>
                    <label class="mb-1 block text-xs font-semibold text-slate-600">
                      ABC / Unit
                    </label>
                    <input
                      :value="item.unitPrice"
                      type="number"
                      min="0"
                      step="0.01"
                      class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
                      @input="updateItemField(item, 'unitPrice', Number(($event.target as HTMLInputElement).value))"
                    />
                  </div>

                  <div>
                    <label class="mb-1 block text-xs font-semibold text-slate-600">
                      Pre-Procurement Conference
                    </label>
                    <select
                      :value="item.preProcurementConference === true ? 'true' : item.preProcurementConference === false ? 'false' : ''"
                      class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
                      @change="updateItemField(item, 'preProcurementConference', ($event.target as HTMLSelectElement).value === 'true')"
                    >
                      <option value="" disabled>Select Yes or No</option>
                      <option value="true">Yes</option>
                      <option value="false">No</option>
                    </select>
                  </div>
                </div>

                <div class="mt-4 rounded-lg border border-slate-200 bg-white p-4">
                  <div class="mb-3 flex items-center justify-between">
                    <div>
                      <h6 class="text-sm font-bold text-slate-800">
                        Section Requirements
                      </h6>
                      <p class="text-xs text-slate-500">
                        The total quantity is calculated from these section quantities.
                      </p>
                    </div>
                    <div class="text-right">
                      <p class="text-xs text-slate-500">Total Quantity</p>
                      <p class="text-lg font-black text-emerald-700">
                        {{ item.quantity }} {{ item.unit }}
                      </p>
                    </div>
                  </div>

                  <select
                    class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
                    :disabled="isSaving || getAvailableSections(item.sectionAllocations ?? []).length === 0"
                    @change="addItemSection(item, $event)"
                  >
                    <option value="">
                      {{
                        getAvailableSections(item.sectionAllocations ?? []).length
                          ? 'Add another department section'
                          : 'All available sections have been added'
                      }}
                    </option>
                    <option
                      v-for="section in getAvailableSections(item.sectionAllocations ?? [])"
                      :key="section.sectionId"
                      :value="section.sectionId"
                    >
                      {{ section.sectionCode }} - {{ section.sectionName }}
                    </option>
                  </select>

                  <div
                    v-if="!item.sectionAllocations?.length"
                    class="mt-3 rounded-lg border border-dashed border-slate-300 p-4 text-center text-sm text-slate-500"
                  >
                    No sections allocated.
                  </div>

                  <div v-else class="mt-3 space-y-2">
                    <div
                      v-for="allocation in item.sectionAllocations"
                      :key="allocation.sectionId"
                      class="grid grid-cols-1 items-center gap-3 rounded-lg border border-slate-200 p-3 sm:grid-cols-[1fr_140px_auto]"
                    >
                      <p class="text-sm font-semibold text-slate-700">
                        {{ allocation.sectionName }}
                      </p>

                      <input
                        v-model.number="allocation.quantity"
                        type="number"
                        min="0"
                        step="1"
                        class="w-full rounded-lg border border-slate-300 px-3 py-2 text-center text-sm font-semibold"
                        @input="updateQuantityFromAllocations(item)"
                      />

                      <button
                        type="button"
                        class="inline-flex items-center justify-center gap-1 rounded-lg border border-red-200 px-3 py-2 text-xs font-semibold text-red-600 hover:bg-red-50"
                        @click="removeItemSection(item, allocation.sectionId)"
                      >
                        <Trash2 :size="14" />
                        Remove
                      </button>
                    </div>
                  </div>

                  <div class="mt-3 flex justify-between border-t border-slate-200 pt-3 text-sm">
                    <span class="font-semibold text-slate-700">
                      Calculated Total Quantity
                    </span>
                    <span class="font-black text-emerald-700">
                      {{ item.quantity }} {{ item.unit }}
                    </span>
                  </div>
                </div>

                <div class="mt-3 flex justify-end">
                  <button
                    type="button"
                    class="rounded-lg bg-emerald-600 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-700"
                    @click="cancelEditingItem"
                  >
                    Done
                  </button>
                </div>
              </div>

              <!-- SECTION ALLOCATION SUMMARY -->
              <div
                v-if="expandedAllocationRows.includes(item.lineId)"
                class="mt-5 rounded-xl bg-slate-50 px-4 py-5 sm:px-6"
              >
                <div class="mb-4 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                  <p class="text-base font-bold text-slate-800">
                    Section allocation — {{ getDescription(item) }}
                  </p>
                  <span
                    class="text-sm font-semibold"
                    :class="isAllocationBalanced(item) ? 'text-emerald-700' : 'text-red-600'"
                  >
                    Allocated {{ allocationTotal(item) }} / {{ item.quantity }}
                    {{ item.unit }}
                    <span v-if="!isAllocationBalanced(item)">⚠</span>
                  </span>
                </div>

                <div
                  v-if="!item.sectionAllocations?.length"
                  class="rounded-lg border border-dashed border-slate-300 bg-white p-4 text-center text-sm text-slate-500"
                >
                  No section allocations.
                </div>

                <div v-else class="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
                  <div
                    v-for="allocation in item.sectionAllocations"
                    :key="allocation.sectionId"
                    class="rounded-lg border border-slate-200 bg-white p-4"
                  >
                    <p class="text-sm font-semibold text-slate-700">
                      {{ allocation.sectionName }}
                    </p>
                    <p class="mt-1 text-lg font-bold text-slate-900">
                      {{ allocation.quantity }}
                      <span class="text-xs font-normal text-slate-500">
                        {{ item.unit }}
                      </span>
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- CATEGORY TOTAL -->
          <div class="flex items-center justify-between border-t border-slate-200 bg-slate-100 px-6 py-4">
            <span class="text-sm font-bold uppercase tracking-wider text-slate-700">
              Category Total
            </span>
            <span class="text-xl font-black text-emerald-700">
              {{ formatCurrency(categoryTotal(activeCategory.categoryId)) }}
            </span>
          </div>
        </section>

        <!-- PRINT DOCUMENTS -->
        <section
          v-for="category in activeCategories"
          :key="`doc-${category.categoryId}`"
          class="ppmp-document hidden bg-white print:block"
        >
          <div class="ppmp-header">
            <div class="ppmp-header__meta-right">
              <div>Reference Code:</div>
              <div class="ppmp-header__meta-value">HOP-QR-PRO-001</div>
              <div class="ppmp-header__meta-spacer"></div>
              <div>Revision No.:</div>
              <div class="ppmp-header__meta-value">0</div>
              <div class="ppmp-header__meta-spacer"></div>
              <div>Effective Date:</div>
              <div class="ppmp-header__meta-value">January 1, 2026</div>
            </div>

            <div class="ppmp-header__republic">Republic of the Philippines</div>
            <div class="ppmp-header__agency">DAVAO REGIONAL MEDICAL CENTER</div>
            <div class="ppmp-header__title">
              PROJECT PROCUREMENT MANAGEMENT PLAN (PPMP) NO.
              <span class="ppmp-header__placeholder">___</span>
            </div>

            <div class="ppmp-header__info-grid">
              <div class="ppmp-header__info-row">
                <span class="ppmp-header__info-label">Fiscal Year :</span>
                <span class="ppmp-header__info-value">{{ form.planningYear || '' }}</span>
              </div>
              <div class="ppmp-header__info-row">
                <span class="ppmp-header__info-label">End-User or Implementing Unit :</span>
                <span class="ppmp-header__info-value">
                  {{ currentDepartment?.departmentName ?? form.department ?? '' }}
                </span>
              </div>
              <div class="ppmp-header__info-row">
                <span class="ppmp-header__info-label">Category :</span>
                <span class="ppmp-header__info-value">{{ category.categoryName }}</span>
              </div>
            </div>
          </div>

          <div
            v-if="getItemsForCategory(category.categoryId).filter(item => Number(item.quantity) > 0).length === 0"
            class="ppmp-empty-note"
          >
            No manually encoded items with quantities in this category.
          </div>

          <div v-else class="ppmp-table-wrap">
            <table class="ppmp-table">
              <colgroup>
                <col style="width: 12%" />
                <col style="width: 7%" />
                <col style="width: 4%" />
                <col style="width: 5%" />
                <col style="width: 6%" />
                <col style="width: 7%" />
                <col style="width: 6%" />
                <col style="width: 7%" />
                <col style="width: 7%" />
                <col style="width: 8%" />
                <col style="width: 6%" />
                <col style="width: 9%" />
                <col style="width: 8%" />
                <col style="width: 8%" />
              </colgroup>

              <thead>
                <tr class="ppmp-head-group">
                  <th colspan="5">PROCUREMENT PROJECT DETAILS</th>
                  <th>RECOMMENDED MODE OF PROCUREMENT</th>
                  <th>PRE-PROCUREMENT CONFERENCE (YES / NO)</th>
                  <th colspan="3">PROJECTED TIMELINE (MM/YYYY)</th>
                  <th colspan="2">FUNDING DETAILS</th>
                  <th>ATTACHED SUPPORTING DOCUMENTS</th>
                  <th>REMARKS</th>
                </tr>
                <tr class="ppmp-head-col">
                  <th>General Description and Objective of the Project to be Procured</th>
                  <th>Type of the Project to be Procured</th>
                  <th>Quantity and Size</th>
                  <th>Unit of Issue</th>
                  <th>ABC / unit</th>
                  <th></th>
                  <th></th>
                  <th>Start of Procurement Activity</th>
                  <th>End of Procurement Activity</th>
                  <th>Expected Delivery / Implementation Period</th>
                  <th>Source of Funds</th>
                  <th>Estimated Budget / Authorized Budgetary Allocation (PhP)</th>
                  <th></th>
                  <th></th>
                </tr>
                <tr class="ppmp-head-cols-num">
                  <th>Column 1</th>
                  <th>Column 2</th>
                  <th>Column 3</th>
                  <th></th>
                  <th></th>
                  <th>Column 4</th>
                  <th>Column 5</th>
                  <th>Column 6</th>
                  <th>Column 7</th>
                  <th>Column 8</th>
                  <th>Column 9</th>
                  <th>Column 10</th>
                  <th>Column 11</th>
                  <th>Column 12</th>
                </tr>
                <tr class="ppmp-head-sub">
                  <th></th>
                  <th></th>
                  <th>Quantity</th>
                  <th>Unit of Issue</th>
                  <th>ABC/unit</th>
                  <th></th>
                  <th></th>
                  <th></th>
                  <th></th>
                  <th></th>
                  <th></th>
                  <th></th>
                  <th></th>
                  <th></th>
                </tr>
              </thead>

              <tbody>
                <tr
                  v-for="item in getItemsForCategory(category.categoryId).filter(entry => Number(entry.quantity) > 0)"
                  :key="item.lineId"
                  class="ppmp-row"
                >
                  <td class="ppmp-cell ppmp-cell--desc">
                    <div class="ppmp-item-name">
                      {{ getDescription(item) }}
                    </div>
                    <div class="ppmp-item-spec">
                      Section allocations:
                      <span
                        v-for="(allocation, allocationIndex) in item.sectionAllocations"
                        :key="allocation.sectionId"
                      >
                        {{ allocation.sectionName }} ({{ allocation.quantity }})<span v-if="allocationIndex < (item.sectionAllocations?.length ?? 0) - 1">, </span>
                      </span>
                    </div>
                  </td>

                  <td class="ppmp-cell ppmp-cell--center">
                    {{ getProjectType(item) || '—' }}
                  </td>

                  <td class="ppmp-cell ppmp-cell--center">
                    {{ item.quantity }}
                  </td>

                  <td class="ppmp-cell ppmp-cell--center">
                    {{ item.unit }}
                  </td>

                  <td class="ppmp-cell ppmp-cell--right">
                    {{ formatCurrency(item.unitPrice) }}
                  </td>

                  <td class="ppmp-cell ppmp-cell--center ppmp-cell--muted">—</td>

                  <td class="ppmp-cell ppmp-cell--center">
                    {{ item.preProcurementConference === true ? 'Yes' : 'No' }}
                  </td>

                  <td class="ppmp-cell ppmp-cell--center ppmp-cell--muted">—</td>
                  <td class="ppmp-cell ppmp-cell--center ppmp-cell--muted">—</td>
                  <td class="ppmp-cell ppmp-cell--center ppmp-cell--muted">—</td>
                  <td class="ppmp-cell ppmp-cell--center ppmp-cell--muted">—</td>

                  <td class="ppmp-cell ppmp-cell--right ppmp-cell--budget">
                    {{ formatCurrency(item.quantity * item.unitPrice) }}
                  </td>

                  <td class="ppmp-cell ppmp-cell--center ppmp-cell--muted">—</td>
                  <td class="ppmp-cell ppmp-cell--center">{{ item.category }}</td>
                </tr>

                <tr class="ppmp-total-row">
                  <td colspan="11" class="ppmp-total-label">TOTAL BUDGET:</td>
                  <td class="ppmp-total-value">
                    {{ formatCurrency(categoryTotal(category.categoryId)) }}
                  </td>
                  <td colspan="2"></td>
                </tr>
              </tbody>
            </table>
          </div>

          <div class="ppmp-signatures">
            <div class="ppmp-signature-col">
              <div class="ppmp-signature-label">Prepared by:</div>
              <div class="ppmp-signature-line"></div>
              <div class="ppmp-signature-name">
                {{ authStore.user?.fullName || '' }}
              </div>
              <div class="ppmp-signature-role">Signature over Printed Name</div>
              <div class="ppmp-signature-name"></div>
              <div class="ppmp-signature-role">Designation</div>
              <div class="ppmp-signature-role">
                [{{ currentDepartment?.departmentName ?? form.department ?? 'End-User or Implementing Unit' }}]
              </div>
              <div class="ppmp-signature-date">Date : _________________</div>
            </div>

            <div class="ppmp-signature-col">
              <div class="ppmp-signature-label">Submitted by:</div>
              <div class="ppmp-signature-line"></div>
              <div class="ppmp-signature-name"></div>
              <div class="ppmp-signature-role">Signature over Printed Name</div>
              <div class="ppmp-signature-name"></div>
              <div class="ppmp-signature-role">Designation</div>
              <div class="ppmp-signature-role">
                [Head of the End-User or Implementing Unit]
              </div>
              <div class="ppmp-signature-date">Date : ____________</div>
            </div>
          </div>
        </section>

        <!-- ACTION BAR -->
        <div
          class="print:hidden sticky bottom-4 z-20 mt-6 rounded-xl border border-slate-300 bg-white/95 p-4 shadow-lg backdrop-blur-md"
        >
          <div class="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
            <div class="flex items-center gap-3">
              <FileText :size="20" class="shrink-0 text-emerald-600" />
              <div>
                <p class="text-[10px] font-bold uppercase tracking-wider text-slate-500">
                  Estimated Total Budget
                </p>
                <p class="text-lg font-black text-emerald-700">
                  {{ formatCurrency(estimatedBudget) }}
                </p>
              </div>
            </div>

            <div class="flex items-center justify-end gap-2">
              <button
                type="button"
                :disabled="isSaving || selectedItems.length === 0"
                class="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 shadow-sm transition-colors hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
                @click="savePPMP(false)"
              >
                <Save :size="16" />
                Save as Draft
              </button>

              <button
                type="button"
                :disabled="isSaving || selectedItems.length === 0"
                class="inline-flex items-center gap-2 rounded-lg bg-emerald-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-50"
                @click="savePPMP(true)"
              >
                <Send :size="16" />
                {{ isSaving ? 'Saving...' : 'Submit for Approval' }}
              </button>
            </div>
          </div>
        </div>
      </template>
    </template>
  </div>
</template>

<style scoped>
.ppmp-document {
  border: 1px solid #94a3b8;
  overflow-x: auto;
}

.ppmp-header {
  position: relative;
  padding: 16px 24px 20px;
  border-bottom: 1px solid #94a3b8;
  text-align: center;
  font-family: Arial, Helvetica, sans-serif;
}

.ppmp-header__meta-right {
  position: absolute;
  top: 12px;
  right: 16px;
  width: 150px;
  text-align: left;
  font-size: 9px;
  line-height: 1.35;
  color: #334155;
}

.ppmp-header__meta-value {
  font-weight: 600;
  color: #0f172a;
}

.ppmp-header__meta-spacer {
  height: 8px;
}

.ppmp-header__republic {
  font-size: 11px;
  letter-spacing: 0.04em;
}

.ppmp-header__agency {
  margin-top: 2px;
  font-size: 15px;
  font-weight: 700;
  letter-spacing: 0.02em;
  color: #0f172a;
}

.ppmp-header__title {
  margin-top: 10px;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.02em;
  color: #0f172a;
}

.ppmp-header__placeholder {
  font-weight: 400;
}

.ppmp-header__info-grid {
  margin-top: 14px;
  text-align: left;
  font-size: 11px;
}

.ppmp-header__info-row {
  display: flex;
  gap: 6px;
  padding: 1px 0;
}

.ppmp-header__info-label {
  min-width: 200px;
  font-weight: 700;
  color: #0f172a;
}

.ppmp-header__info-value {
  border-bottom: 1px solid #94a3b8;
  min-width: 260px;
  padding: 0 6px;
  color: #0f172a;
}

.ppmp-table {
  width: 100%;
  table-layout: fixed;
  border-collapse: collapse;
  font-family: Arial, Helvetica, sans-serif;
  font-size: 9.5px;
  color: #0f172a;
}

.ppmp-table th,
.ppmp-table td {
  border: 1px solid #475569;
  padding: 3px 4px;
  vertical-align: middle;
}

.ppmp-head-group th {
  background: #e2e8f0;
  font-size: 8.5px;
  font-weight: 700;
  text-align: center;
  text-transform: uppercase;
  letter-spacing: 0.02em;
  line-height: 1.2;
}

.ppmp-head-col th,
.ppmp-head-cols-num th,
.ppmp-head-sub th {
  background: #f1f5f9;
  font-size: 8px;
  font-weight: 600;
  text-align: center;
  line-height: 1.15;
}

.ppmp-head-cols-num th {
  font-style: italic;
  font-weight: 500;
  color: #475569;
}

.ppmp-head-sub th {
  background: #f8fafc;
}

.ppmp-row td.ppmp-cell {
  vertical-align: top;
  font-size: 9.5px;
}

.ppmp-cell--center {
  text-align: center;
  vertical-align: middle;
}

.ppmp-cell--right {
  text-align: right;
  vertical-align: middle;
  white-space: nowrap;
}

.ppmp-cell--desc {
  padding: 4px 5px;
}

.ppmp-cell--muted {
  color: #94a3b8;
  vertical-align: middle;
}

.ppmp-cell--budget {
  font-weight: 700;
  color: #065f46;
}

.ppmp-item-name {
  font-weight: 700;
  color: #0f172a;
  line-height: 1.2;
}

.ppmp-item-spec {
  margin-top: 2px;
  font-size: 8.5px;
  color: #475569;
  line-height: 1.25;
}

.ppmp-total-row td {
  background: #f1f5f9;
  font-weight: 700;
  font-size: 10px;
  padding: 6px;
}

.ppmp-total-label {
  text-align: right;
  letter-spacing: 0.03em;
}

.ppmp-total-value {
  text-align: right;
  color: #065f46;
  font-size: 11px;
  white-space: nowrap;
}

.ppmp-empty-note {
  padding: 20px;
  text-align: center;
  font-size: 12px;
  color: #64748b;
  border-top: 1px solid #cbd5e1;
}

.ppmp-signatures {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  padding: 20px 24px 28px;
  border-top: 1px solid #94a3b8;
  font-family: Arial, Helvetica, sans-serif;
  font-size: 10px;
  color: #0f172a;
}

.ppmp-signature-col {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.ppmp-signature-label {
  font-weight: 700;
  margin-bottom: 12px;
}

.ppmp-signature-line {
  height: 0;
  border-bottom: 1px solid #475569;
  margin: 4px 0 2px;
}

.ppmp-signature-name {
  font-weight: 700;
  text-align: center;
  min-height: 12px;
}

.ppmp-signature-role {
  font-size: 9px;
  text-align: center;
  color: #334155;
}

.ppmp-signature-date {
  margin-top: 10px;
  font-size: 10px;
}

@media print {
  @page {
    size: A4 landscape;
    margin: 10mm;
  }

  .ppmp-shell {
    max-width: none !important;
    padding: 0 !important;
    margin: 0 !important;
  }

  .ppmp-document {
    border: none;
    overflow: visible;
    page-break-inside: auto;
  }

  .ppmp-header {
    page-break-after: avoid;
  }

  .ppmp-table {
    font-size: 8pt;
  }

  .ppmp-table thead {
    display: table-header-group;
  }

  .ppmp-row {
    page-break-inside: avoid;
  }

  .ppmp-signatures {
    page-break-inside: avoid;
    page-break-before: avoid;
  }
}
</style>
