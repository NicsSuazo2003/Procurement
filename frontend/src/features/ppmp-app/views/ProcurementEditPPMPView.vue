<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  ArrowLeft,
  Plus,
  Trash2,
  Save,
  Calculator,
  Pencil,
  ClipboardList,
  RefreshCw,
  Search,
  Check,
  Package,
  X,
} from '@lucide/vue'

import { apiRequest } from '@/services/api'
import { ppmpApiService } from '../services/ppmpApiService'
import PPMPProjectTypeField from '../components/PPMPProjectTypeField.vue'
import {
  getPPMPTemplateDefaults,
  isPPMPProjectType,
  normalizePPMPProjectType,
} from '../models/ppmp-template.defaults'
import { formatDatePH, formatPHP, getPPMPStatusStyle } from '@/shared/utilities/ppmpUiHelpers'

import type {
  PPMPDetailApi,
  UpdatePPMPRequest,
  UpdatePPMPItemRequest,
} from '../models/ppmp-api.types'

import type {
  DepartmentSectionDto,
  PlanningYearCategoryDto,
  ItemDto,
} from '../models/master-data.types'

import type {
  EditableAllocation,
  EditableItem,
} from '../models/ppmp-edit.types'

const router = useRouter()
const route = useRoute()

const ppmpId = computed(() => Number(route.params.id))

const ppmp = ref<PPMPDetailApi | null>(null)
const items = ref<EditableItem[]>([])

const sections = ref<DepartmentSectionDto[]>([])
const planningYearCategories = ref<PlanningYearCategoryDto[]>([])
const catalogItems = ref<ItemDto[]>([])

// Catalogue picker modal state
const showCatalogModal = ref(false)
const catalogSearch = ref('')
const selectedCategoryFilter = ref<number | 'ALL'>('ALL')

const procurementReason = ref('')

// Per-item modal state
const editingItem = ref<EditableItem | null>(null)
const isNewModalItem = ref(false)
const modalErrorMessage = ref('')

// Save confirmation modal state
const showSaveConfirmModal = ref(false)

const errorMessage = ref('')
const successMessage = ref('')

const isLoading = ref(true)
const isSaving = ref(false)

const procurementModes = [
  'Competitive Bidding',
  'Limited Source Bidding',
  'Direct Contracting',
  'Repeat Order',
  'Shopping (Sec. 52.1.a)',
  'Shopping (Sec. 52.1.b)',
  'NP - Two Failed Biddings (Sec. 53.1)',
  'NP - Emergency Cases (Sec. 53.2)',
  'NP - Take-Over of Contracts (Sec. 53.3)',
  'NP - Adjacent or Contiguous (Sec. 53.4)',
  'NP - Agency-to-Agency (Sec. 53.5)',
  'NP - Scientific, Scholarly or Artistic Work (Sec. 53.6)',
  'NP - Highly Technical Consultants (Sec. 53.7)',
  'NP - Defense Cooperation Agreement (Sec. 53.8)',
  'NP - Small Value Procurement (Sec. 53.9)',
  'NP - Lease of Real Property and Venue (Sec. 53.10)',
  'NP - NGO Participation (Sec. 53.11)',
  'NP - Community Participation (Sec. 53.12)',
  "NP - UN Agencies, Int'l Organizations (Sec. 53.13)",
  'Direct Retail Purchase of POL and Airline Tickets (Sec. 53.14)',
]

const fundSources = [
  'GAA',
  'Hospital Income / Retained Earnings',
  'Trust Fund',
  'Special Purpose Fund',
  'SAA / Sub-Allotment',
  'Others',
]

const totalEstimatedBudget = computed(() =>
  items.value.reduce((total, item) => {
    const quantity = item.allocations.reduce(
      (sum, allocation) => sum + Number(allocation.quantity || 0),
      0,
    )

    return total + quantity * Number(item.unitPrice || 0)
  }, 0),
)

const totalQuantityAllItems = computed(() =>
  items.value.reduce((total, item) => total + getItemQuantity(item), 0),
)

const budgetDifference = computed(() => {
  if (!ppmp.value) return 0
  return totalEstimatedBudget.value - Number(ppmp.value.estimatedBudget || 0)
})

const itemCount = computed(() => items.value.length)

const canEdit = computed(() => ppmp.value?.status === 'Under Review')

const addedItemIds = computed(
  () =>
    new Set(
      items.value
        .map((item) => item.itemId)
        .filter((id): id is number => id !== null),
    ),
)

const filteredCatalogItems = computed(() => {
  const query = catalogSearch.value.trim().toLowerCase()

  return catalogItems.value.filter((item) => {
    const matchesCategory =
      selectedCategoryFilter.value === 'ALL' ||
      item.categoryId === selectedCategoryFilter.value

    const matchesQuery =
      !query ||
      item.itemCode.toLowerCase().includes(query) ||
      item.itemName.toLowerCase().includes(query) ||
      item.categoryName.toLowerCase().includes(query) ||
      item.unit.toLowerCase().includes(query)

    return matchesCategory && matchesQuery
  })
})

function getItemQuantity(item: EditableItem): number {
  return item.allocations.reduce(
    (total, allocation) => total + Number(allocation.quantity || 0),
    0,
  )
}

function getItemBudget(item: EditableItem): number {
  return getItemQuantity(item) * Number(item.unitPrice || 0)
}

function getActiveSubUnitCount(item: EditableItem): number {
  return item.allocations.filter((allocation) => Number(allocation.quantity) > 0).length
}

function cloneEditableItem(item: EditableItem): EditableItem {
  return {
    ...item,
    allocations: item.allocations.map((allocation) => ({ ...allocation })),
  }
}

function createAllocations(
  existingAllocations: {
    sectionId: number
    sectionName: string
    quantity: number
  }[] = [],
): EditableAllocation[] {
  return sections.value.map((section) => {
    const existing = existingAllocations.find(
      (allocation) => allocation.sectionId === section.sectionId,
    )

    return {
      sectionId: section.sectionId,
      sectionName: section.sectionName,
      quantity: existing?.quantity ?? 0,
    }
  })
}

async function loadPPMP() {
  isLoading.value = true
  errorMessage.value = ''
  ppmp.value = null
  items.value = []
  catalogItems.value = []
  editingItem.value = null
  showCatalogModal.value = false

  try {
    if (!Number.isInteger(ppmpId.value) || ppmpId.value <= 0) {
      throw new Error('Invalid PPMP record ID.')
    }

    const record = await ppmpApiService.getById(ppmpId.value)

    if (record.status !== 'Under Review') {
      throw new Error(
        'Only PPMP records with Under Review status can be edited by Procurement.',
      )
    }

    const loadedSections = await apiRequest<DepartmentSectionDto[]>(
      `/DepartmentSections/department/${record.departmentId}`,
    )

    sections.value = loadedSections.filter((section) => section.isActive)

    if (sections.value.length === 0) {
      throw new Error(
        'No active sub-units were found. Please configure the units and sub-units first.',
      )
    }

    const loadedCategories = await apiRequest<PlanningYearCategoryDto[]>(
      `/PlanningYearCategories/planning-year/${record.planningYearId}`,
    )

    planningYearCategories.value = loadedCategories.filter(
      (category) => category.isActive,
    )

    const itemResults = await Promise.all(
      planningYearCategories.value.map((category) =>
        apiRequest<ItemDto[]>(
          `/Items/category/${category.categoryId}`,
        ),
      ),
    )

    catalogItems.value = itemResults
      .flat()
      .filter((item) => item.isActive)

    ppmp.value = record

    items.value = record.items.map((item, index) => {
      const defaults = getPPMPTemplateDefaults(item.categoryName)
      const generalDescription =
        item.generalDescriptionAndObjective?.trim() ||
        item.itemDescription ||
        ''

      const projectType = item.projectType?.trim() || ''

      return {
        lineId: item.ppmpItemId || -(index + 1),
        ppmpItemId: item.ppmpItemId,
        itemId: item.itemId,
        categoryId: item.categoryId,
        categoryName: item.categoryName,

        code: item.itemCode ?? '',

        name: generalDescription,
        itemType: item.itemType ?? null,
        specifications: item.specifications ?? '',

        generalDescriptionAndObjective: generalDescription,
        projectType: normalizePPMPProjectType(projectType),
        preProcurementConference: item.preProcurementConference ?? null,
        recommendedModeOfProcurement:
          item.recommendedModeOfProcurement || defaults.recommendedModeOfProcurement,
        startOfProcurementActivity:
          item.startOfProcurementActivity || defaults.startOfProcurementActivity,
        endOfProcurementActivity:
          item.endOfProcurementActivity || defaults.endOfProcurementActivity,
        expectedDeliveryImplementationPeriod:
          item.expectedDeliveryImplementationPeriod ||
          defaults.expectedDeliveryImplementationPeriod,
        sourceOfFunds: item.sourceOfFunds || defaults.sourceOfFunds,
        attachedSupportingDocuments:
          item.attachedSupportingDocuments || defaults.attachedSupportingDocuments,

        unit: item.unit,
        unitPrice: item.unitPrice,
        allocations: createAllocations(item.sectionAllocations),
      }
    })
  } catch (error) {
    console.error('Unable to load PPMP:', error)

    errorMessage.value =
      error instanceof Error
        ? error.message
        : 'Unable to load this PPMP record.'
  } finally {
    isLoading.value = false
  }
}

onMounted(loadPPMP)

// --------------------------------------------------
// CATALOGUE PICKER & PER-ITEM MODAL ACTIONS
// --------------------------------------------------

function openCatalogModal() {
  if (!canEdit.value) return
  catalogSearch.value = ''
  selectedCategoryFilter.value = 'ALL'
  showCatalogModal.value = true
}

function closeCatalogModal() {
  showCatalogModal.value = false
}

function openAddCatalogItemModal(selectedItem: ItemDto) {
  errorMessage.value = ''
  successMessage.value = ''
  modalErrorMessage.value = ''

  if (!canEdit.value) {
    errorMessage.value = 'This PPMP is not available for editing.'
    return
  }

  if (addedItemIds.value.has(selectedItem.itemId)) {
    return
  }

  const assignedCategory = planningYearCategories.value.find(
    (category) => category.categoryId === selectedItem.categoryId,
  )

  if (!assignedCategory) {
    errorMessage.value =
      'This item does not belong to a category assigned to the planning year.'
    return
  }

  const defaults = getPPMPTemplateDefaults(assignedCategory.categoryName)

  showCatalogModal.value = false
  isNewModalItem.value = true
  editingItem.value = {
    lineId: Date.now() + Math.floor(Math.random() * 1000),
    ppmpItemId: null,
    itemId: selectedItem.itemId,
    categoryId: selectedItem.categoryId,
    categoryName: selectedItem.categoryName,

    code: selectedItem.itemCode,

    name: selectedItem.itemName,
    itemType: null,
    specifications: '',

    generalDescriptionAndObjective: selectedItem.itemName,
    projectType: 'Goods',
    preProcurementConference: null,
    ...defaults,

    unit: selectedItem.unit,
    unitPrice: 0,
    allocations: createAllocations(),
  }
}

function openEditItemModal(item: EditableItem) {
  if (!canEdit.value) return
  errorMessage.value = ''
  successMessage.value = ''
  modalErrorMessage.value = ''
  isNewModalItem.value = false
  editingItem.value = cloneEditableItem(item)
}

function closeItemModal() {
  editingItem.value = null
  isNewModalItem.value = false
  modalErrorMessage.value = ''
}

function validateSingleItem(item: EditableItem): string | null {
  const monthPattern = /^\d{4}-(0[1-9]|1[0-2])$/

  if (
    !item.generalDescriptionAndObjective.trim() ||
    !isPPMPProjectType(item.projectType) ||
    !item.unit.trim()
  ) {
    return 'Please complete the general description, choose a valid project type, and enter the unit of issue.'
  }

  if (
    !item.recommendedModeOfProcurement.trim() ||
    !monthPattern.test(item.startOfProcurementActivity) ||
    !monthPattern.test(item.endOfProcurementActivity) ||
    item.endOfProcurementActivity < item.startOfProcurementActivity ||
    !item.expectedDeliveryImplementationPeriod.trim() ||
    !item.sourceOfFunds.trim()
  ) {
    return 'Please complete the procurement mode, valid activity dates (End date cannot be earlier than Start date), delivery period, and source of funds.'
  }

  if (item.preProcurementConference === null) {
    return 'Please select Yes or No for Pre-Procurement Conference.'
  }

  if (
    String(item.unitPrice).trim().length === 0 ||
    !Number.isFinite(Number(item.unitPrice)) ||
    Number(item.unitPrice) < 0
  ) {
    return 'Please enter a valid non-negative ABC / Unit price.'
  }

  const invalidAllocation = item.allocations.find(
    (allocation) =>
      !Number.isFinite(Number(allocation.quantity)) ||
      Number(allocation.quantity) < 0,
  )

  if (invalidAllocation) {
    return 'Sub-unit quantities cannot be negative.'
  }

  const positiveAllocations = item.allocations.filter(
    (allocation) => Number(allocation.quantity) > 0,
  )

  if (positiveAllocations.length === 0) {
    return 'Please allocate a quantity greater than 0 to at least one sub-unit.'
  }

  const hasInvalidPrecision =
    Math.abs(
      Number(item.unitPrice) * 100 -
        Math.round(Number(item.unitPrice) * 100),
    ) > 0.000001 ||
    item.allocations.some(
      (allocation) =>
        Math.abs(
          Number(allocation.quantity) * 100 -
            Math.round(Number(allocation.quantity) * 100),
        ) > 0.000001,
    )

  if (hasInvalidPrecision) {
    return 'Quantity and unit price must have no more than two decimal places.'
  }

  return null
}

function saveModalItem() {
  if (!editingItem.value) return

  modalErrorMessage.value = ''
  const validationError = validateSingleItem(editingItem.value)

  if (validationError) {
    modalErrorMessage.value = validationError
    return
  }

  const savedDraft = cloneEditableItem(editingItem.value)
  savedDraft.name = savedDraft.generalDescriptionAndObjective.trim()

  if (isNewModalItem.value) {
    items.value.push(savedDraft)
  } else {
    const idx = items.value.findIndex((item) => item.lineId === savedDraft.lineId)
    if (idx >= 0) {
      items.value[idx] = savedDraft
    }
  }

  closeItemModal()
}

function removeItem(lineId: number) {
  errorMessage.value = ''
  successMessage.value = ''

  if (!canEdit.value) return

  items.value = items.value.filter((item) => item.lineId !== lineId)
}

// --------------------------------------------------
// FULL FORM VALIDATION & SUBMISSION
// --------------------------------------------------

function validateForm(): boolean {
  if (!ppmp.value || !canEdit.value) {
    errorMessage.value = 'This PPMP is not available for editing.'
    return false
  }

  if (items.value.length === 0) {
    errorMessage.value = 'A PPMP must contain at least one item.'
    return false
  }

  if (!procurementReason.value.trim()) {
    errorMessage.value =
      'Please provide a reason for the Procurement changes.'
    return false
  }

  for (const item of items.value) {
    const itemError = validateSingleItem(item)
    if (itemError) {
      errorMessage.value = `${item.generalDescriptionAndObjective || 'An item'}: ${itemError}`
      return false
    }
  }

  return true
}

function buildUpdateRequest(): UpdatePPMPRequest {
  const updateItems: UpdatePPMPItemRequest[] = items.value.map((item) => {
    const generalDescription = item.generalDescriptionAndObjective.trim()
    const projectType = item.projectType.trim()

    const positiveAllocations = item.allocations
      .filter((allocation) => Number(allocation.quantity) > 0)
      .map((allocation) => ({
        sectionId: allocation.sectionId,
        quantity: Number(allocation.quantity),
      }))

    const quantity = positiveAllocations.reduce(
      (total, allocation) => total + allocation.quantity,
      0,
    )

    return {
      ppmpItemId: item.ppmpItemId,
      itemId: item.itemId,
      categoryId: item.categoryId,

      itemCode: item.code,
      itemDescription: generalDescription,
      itemType: projectType,
      unit: item.unit.trim(),
      specifications: null,

      generalDescriptionAndObjective: generalDescription,
      projectType,
      preProcurementConference: item.preProcurementConference === true,
      recommendedModeOfProcurement: item.recommendedModeOfProcurement.trim(),
      startOfProcurementActivity: item.startOfProcurementActivity,
      endOfProcurementActivity: item.endOfProcurementActivity,
      expectedDeliveryImplementationPeriod:
        item.expectedDeliveryImplementationPeriod.trim(),
      sourceOfFunds: item.sourceOfFunds.trim(),
      attachedSupportingDocuments: item.attachedSupportingDocuments.trim(),

      quantity,
      unitPrice: Number(item.unitPrice),
      sectionAllocations: positiveAllocations,
    }
  })

  return {
    reason: procurementReason.value.trim(),
    items: updateItems,
  }
}

function promptSaveChanges() {
  errorMessage.value = ''
  successMessage.value = ''

  if (!validateForm() || !ppmp.value) return
  showSaveConfirmModal.value = true
}

async function executeSaveChanges() {
  if (!ppmp.value || isSaving.value) return

  showSaveConfirmModal.value = false
  isSaving.value = true

  try {
    const request = buildUpdateRequest()

    await ppmpApiService.update(ppmpId.value, request)

    successMessage.value =
      'Procurement changes and change history saved successfully.'

    await router.push('/procurement-review')
  } catch (error) {
    console.error('Unable to save Procurement changes:', error)

    errorMessage.value =
      error instanceof Error
        ? error.message
        : 'Unable to save Procurement changes. Please try again.'
  } finally {
    isSaving.value = false
  }
}

function cancelEditing() {
  router.push('/procurement-review')
}
</script>

<template>
  <div class="mx-auto max-w-7xl space-y-6 pb-8">
    <!-- PAGE HEADING -->
    <div class="flex flex-wrap items-start justify-between gap-4">
      <div>
        <button
          type="button"
          class="mb-3 inline-flex items-center gap-1.5 text-xs font-semibold text-slate-500 transition hover:text-brand-700"
          @click="cancelEditing"
        >
          <ArrowLeft :size="15" />
          Back to Procurement Review
        </button>

        <div class="flex items-start gap-3">
          <div class="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-blue-50 text-blue-700 ring-1 ring-inset ring-blue-600/20">
            <Pencil :size="21" />
          </div>

          <div>
            <h1 class="text-2xl font-bold tracking-tight text-slate-900">
              Procurement PPMP Editor
            </h1>
            <p class="mt-1 text-sm text-slate-500">
              Review, add requested catalogue items, and edit each PPMP line item individually before completing the review.
            </p>
          </div>
        </div>
      </div>

      <!-- LIVE BUDGET SUMMARY PILL -->
      <div
        v-if="ppmp"
        class="flex items-center gap-4 rounded-2xl border border-slate-200 bg-white px-4 py-3 shadow-2xs"
      >
        <div>
          <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
            Original Budget
          </p>
          <p class="mt-0.5 text-sm font-semibold text-slate-700 tabular-nums">
            {{ formatPHP(ppmp.estimatedBudget) }}
          </p>
        </div>

        <div class="h-8 w-px bg-slate-200" />

        <div>
          <p class="text-[11px] font-semibold uppercase tracking-wider text-brand-700">
            Updated Budget
          </p>
          <p class="mt-0.5 text-base font-extrabold text-brand-700 tabular-nums">
            {{ formatPHP(totalEstimatedBudget) }}
          </p>
        </div>
      </div>
    </div>

    <!-- LOADING -->
    <div
      v-if="isLoading"
      class="drmc-card p-12 text-center"
    >
      <RefreshCw :size="26" class="mx-auto mb-3 animate-spin text-brand-700" />
      <p class="text-sm font-medium text-slate-600">
        Loading PPMP record and master data...
      </p>
    </div>

    <!-- ERROR WHEN RECORD CANNOT BE LOADED -->
    <div
      v-else-if="!ppmp"
      class="drmc-card border-red-200 p-10 text-center"
    >
      <ClipboardList :size="36" class="mx-auto mb-3 text-red-400" />

      <h2 class="text-base font-semibold text-slate-900">
        Unable to Open PPMP
      </h2>

      <p class="mx-auto mt-2 max-w-lg text-sm text-red-600">
        {{ errorMessage }}
      </p>

      <button
        type="button"
        class="drmc-btn-primary mt-5"
        @click="cancelEditing"
      >
        Back to Procurement Review
      </button>
    </div>

    <template v-else>
      <!-- PPMP INFORMATION -->
      <section class="drmc-card p-5">
        <div class="mb-4 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 class="text-base font-semibold text-slate-900">
              PPMP Information
            </h2>
            <p class="mt-0.5 text-xs text-slate-500">
              Submitted unit metadata.
            </p>
          </div>

          <span
            :class="[
              'inline-flex w-fit items-center gap-1.5 rounded-full px-3 py-1 text-xs font-semibold ring-1 ring-inset',
              getPPMPStatusStyle(ppmp.status).pill,
            ]"
          >
            <span :class="['h-1.5 w-1.5 rounded-full', getPPMPStatusStyle(ppmp.status).dot]" />
            {{ ppmp.status }}
          </span>
        </div>

        <div class="grid grid-cols-1 gap-4 rounded-xl bg-slate-50/80 p-4 sm:grid-cols-2 lg:grid-cols-4">
          <div>
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Reference Number
            </p>
            <p class="mt-1 font-semibold text-brand-700">
              {{ ppmp.ppmpReference }}
            </p>
          </div>

          <div>
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Unit
            </p>
            <p class="mt-1 font-semibold text-slate-900">
              {{ ppmp.departmentName }}
            </p>
          </div>

          <div>
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Planning Year
            </p>
            <p class="mt-1 font-semibold text-slate-900 tabular-nums">
              FY {{ ppmp.planningYear }}
            </p>
          </div>

          <div>
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Date Submitted
            </p>
            <p class="mt-1 font-semibold text-slate-900">
              {{ formatDatePH(ppmp.submittedAt) }}
            </p>
          </div>
        </div>
      </section>

      <!-- PPMP ITEMS TABLE -->
      <section class="drmc-card overflow-hidden">
        <div class="flex flex-col gap-3 border-b border-slate-200/80 px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
          <div class="flex items-center gap-2.5">
            <Package :size="19" class="text-brand-700" />
            <div>
              <h2 class="text-base font-semibold text-slate-900">
                PPMP Line Items
              </h2>
              <p class="mt-0.5 text-xs text-slate-500">
                Click <strong>Edit</strong> on any item to update its details, schedule, funding, and sub-unit allocations.
              </p>
            </div>
          </div>

          <div class="flex flex-wrap items-center gap-2.5">
            <span class="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-700 tabular-nums">
              {{ itemCount }} item line(s) · {{ totalQuantityAllItems }} total unit(s)
            </span>

            <button
              type="button"
              :disabled="!canEdit"
              class="drmc-btn-primary px-3.5 py-2 text-xs"
              @click="openCatalogModal"
            >
              <Plus :size="15" />
              Add More Additional or Item
            </button>
          </div>
        </div>

        <div
          v-if="items.length === 0"
          class="px-5 py-14 text-center"
        >
          <Calculator :size="32" class="mx-auto text-slate-300" />
          <p class="mt-3 font-semibold text-slate-800">
            No items in this PPMP
          </p>
          <p class="mt-1 text-xs text-slate-500">
            Click the button below to add an item to this PPMP.
          </p>
          <button
            type="button"
            :disabled="!canEdit"
            class="drmc-btn-primary mt-4 px-4 py-2 text-xs"
            @click="openCatalogModal"
          >
            <Plus :size="15" />
            Add More Additional or Item
          </button>
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full min-w-[900px] border-collapse text-left text-sm">
            <thead class="border-b border-slate-200/80 bg-slate-50/80 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
              <tr>
                <th class="w-12 px-4 py-3.5 text-center">#</th>
                <th class="w-[34%] px-4 py-3.5">Item Description & Category</th>
                <th class="px-4 py-3.5">Mode & Timeline</th>
                <th class="px-4 py-3.5 text-center">Sub-Units</th>
                <th class="px-4 py-3.5 text-right">Total Qty</th>
                <th class="px-4 py-3.5 text-right">ABC / Unit</th>
                <th class="px-4 py-3.5 text-right">Estimated Budget</th>
                <th class="w-36 px-4 py-3.5 text-center">Actions</th>
              </tr>
            </thead>

            <tbody class="divide-y divide-slate-100">
              <tr
                v-for="(item, idx) in items"
                :key="item.lineId"
                class="transition-colors hover:bg-slate-50/80"
              >
                <td class="px-4 py-4 text-center">
                  <span class="inline-flex h-6 w-6 items-center justify-center rounded-md bg-brand-50 text-xs font-bold text-brand-700 ring-1 ring-inset ring-brand-600/20 tabular-nums">
                    {{ idx + 1 }}
                  </span>
                </td>

                <td class="px-4 py-4">
                  <div class="flex flex-wrap items-center gap-2">
                    <p class="font-semibold text-slate-900">
                      {{ item.generalDescriptionAndObjective || 'New PPMP Item' }}
                    </p>
                    <span
                      v-if="item.ppmpItemId === null"
                      class="rounded-full bg-blue-50 px-2 py-0.5 text-[11px] font-semibold text-blue-700 ring-1 ring-inset ring-blue-600/20"
                    >
                      Added by Procurement
                    </span>
                  </div>
                  <p class="mt-1 text-xs text-slate-500">
                    <span class="font-mono font-medium text-slate-700">{{ item.code || '—' }}</span>
                    · {{ item.categoryName }}
                    · {{ item.projectType }}
                  </p>
                </td>

                <td class="px-4 py-4 text-xs text-slate-600">
                  <p class="font-medium text-slate-800">
                    {{ item.recommendedModeOfProcurement || '—' }}
                  </p>
                  <p class="mt-0.5 tabular-nums text-slate-500">
                    {{ item.startOfProcurementActivity }} → {{ item.endOfProcurementActivity }}
                  </p>
                </td>

                <td class="px-4 py-4 text-center">
                  <span class="inline-flex items-center rounded-full bg-slate-100 px-2.5 py-0.5 text-xs font-semibold text-slate-700 tabular-nums">
                    {{ getActiveSubUnitCount(item) }} / {{ item.allocations.length }}
                  </span>
                </td>

                <td class="whitespace-nowrap px-4 py-4 text-right font-semibold text-slate-900 tabular-nums">
                  {{ getItemQuantity(item) }} <span class="font-normal text-slate-500">{{ item.unit }}</span>
                </td>

                <td class="whitespace-nowrap px-4 py-4 text-right text-slate-700 tabular-nums">
                  {{ formatPHP(item.unitPrice) }}
                </td>

                <td class="whitespace-nowrap px-4 py-4 text-right font-bold text-brand-700 tabular-nums">
                  {{ formatPHP(getItemBudget(item)) }}
                </td>

                <td class="whitespace-nowrap px-4 py-4">
                  <div class="flex items-center justify-center gap-1.5">
                    <button
                      type="button"
                      class="inline-flex items-center gap-1.5 rounded-xl border border-blue-200 bg-blue-50/70 px-3 py-1.5 text-xs font-semibold text-blue-700 transition hover:bg-blue-100"
                      @click="openEditItemModal(item)"
                    >
                      <Pencil :size="13" />
                      Edit
                    </button>

                    <button
                      type="button"
                      title="Remove item"
                      class="rounded-lg p-1.5 text-slate-400 transition hover:bg-red-50 hover:text-red-600"
                      @click="removeItem(item.lineId)"
                    >
                      <Trash2 :size="16" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- BUDGET SUMMARY FOOTER -->
        <div class="border-t border-slate-200/80 bg-brand-950 px-5 py-4 text-white">
          <div class="flex flex-wrap items-center justify-between gap-4">
            <div class="text-xs text-brand-200">
              <span>Original: <strong class="text-white tabular-nums">{{ formatPHP(ppmp.estimatedBudget) }}</strong></span>
              <span class="mx-2">·</span>
              <span>
                Variance:
                <strong
                  :class="[
                    'tabular-nums',
                    budgetDifference > 0
                      ? 'text-amber-300'
                      : budgetDifference < 0
                        ? 'text-emerald-300'
                        : 'text-white',
                  ]"
                >
                  {{ budgetDifference >= 0 ? '+' : '' }}{{ formatPHP(budgetDifference) }}
                </strong>
              </span>
            </div>

            <div class="flex items-center gap-3">
              <span class="text-xs font-semibold uppercase tracking-wider text-brand-200">
                Updated Estimated Budget
              </span>
              <span class="text-xl font-bold text-white tabular-nums">
                {{ formatPHP(totalEstimatedBudget) }}
              </span>
            </div>
          </div>
        </div>
      </section>

      <!-- PROCUREMENT CHANGE REASON -->
      <section class="drmc-card border-blue-200/80 p-5">
        <div class="mb-3">
          <h2 class="text-base font-semibold text-slate-900">
            Procurement Change Reason <span class="text-red-600">*</span>
          </h2>
          <p class="mt-0.5 text-xs text-slate-500">
            Explain why Procurement modified the submitted PPMP. This reason is recorded in the official change history.
          </p>
        </div>

        <textarea
          v-model="procurementReason"
          rows="3"
          maxlength="1000"
          placeholder="Enter the reason for the changes made to this PPMP..."
          class="drmc-input"
        />
      </section>

      <!-- FEEDBACK -->
      <div
        v-if="errorMessage"
        role="alert"
        class="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-800"
      >
        {{ errorMessage }}
      </div>

      <div
        v-if="successMessage"
        role="status"
        class="rounded-xl border border-brand-200 bg-brand-50 px-4 py-3 text-sm font-medium text-brand-800"
      >
        {{ successMessage }}
      </div>

      <!-- ACTION BUTTONS -->
      <div class="flex flex-col-reverse justify-end gap-3 sm:flex-row">
        <button
          type="button"
          class="drmc-btn-secondary"
          @click="cancelEditing"
        >
          Cancel
        </button>

        <button
          type="button"
          :disabled="isSaving || !canEdit"
          class="drmc-btn-primary"
          @click="promptSaveChanges"
        >
          <Save :size="16" />
          {{ isSaving ? 'Saving...' : 'Save Procurement Changes' }}
        </button>
      </div>
    </template>
  </div>

  <!-- ================================================================ -->
  <!-- CATALOGUE SELECTION MODAL                                        -->
  <!-- ================================================================ -->
  <Teleport to="body">
    <div
      v-if="showCatalogModal && ppmp"
      class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 p-4 backdrop-blur-2xs"
      @click.self="closeCatalogModal"
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-label="Add Additional or Catalogue Item"
        class="flex max-h-[85vh] w-full max-w-3xl flex-col overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-2xl"
      >
        <!-- MODAL HEADER -->
        <div class="flex items-start justify-between gap-4 border-b border-slate-200 bg-slate-50/80 px-6 py-4">
          <div>
            <h3 class="text-lg font-bold text-slate-900">
              Add More Additional or Item
            </h3>
            <p class="mt-0.5 text-xs text-slate-500">
              Select an active item assigned to FY {{ ppmp.planningYear }} to configure and add to this PPMP.
            </p>
          </div>

          <button
            type="button"
            title="Close modal"
            class="rounded-lg p-1.5 text-slate-400 transition hover:bg-slate-200/70 hover:text-slate-700"
            @click="closeCatalogModal"
          >
            <X :size="20" />
          </button>
        </div>

        <!-- SEARCH & FILTER CONTROLS -->
        <div class="border-b border-slate-100 bg-white px-6 py-4">
          <div class="grid grid-cols-1 gap-3 sm:grid-cols-3">
            <div class="relative sm:col-span-2">
              <Search
                :size="16"
                class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
              />
              <input
                v-model="catalogSearch"
                type="text"
                placeholder="Search by item code, name, category, or unit..."
                class="drmc-input pl-9"
              />
            </div>

            <select
              v-model="selectedCategoryFilter"
              class="drmc-input"
            >
              <option value="ALL">All Categories</option>
              <option
                v-for="category in planningYearCategories"
                :key="category.categoryId"
                :value="category.categoryId"
              >
                {{ category.categoryName }}
              </option>
            </select>
          </div>
        </div>

        <!-- CATALOGUE LIST -->
        <div class="flex-1 divide-y divide-slate-100 overflow-y-auto px-6 py-2">
          <template v-if="filteredCatalogItems.length > 0">
            <div
              v-for="catalogItem in filteredCatalogItems"
              :key="catalogItem.itemId"
              class="flex items-center justify-between gap-3 py-3 transition hover:bg-slate-50/80"
            >
              <div class="min-w-0">
                <p class="truncate text-sm font-semibold text-slate-900">
                  {{ catalogItem.itemName }}
                </p>
                <p class="mt-0.5 text-xs text-slate-500">
                  <span class="font-mono font-medium text-slate-700">{{ catalogItem.itemCode }}</span>
                  · {{ catalogItem.categoryName }}
                  · Unit: {{ catalogItem.unit }}
                </p>
              </div>

              <span
                v-if="addedItemIds.has(catalogItem.itemId)"
                class="inline-flex shrink-0 items-center gap-1 rounded-lg bg-brand-50 px-2.5 py-1.5 text-xs font-semibold text-brand-700 ring-1 ring-inset ring-brand-600/20"
              >
                <Check :size="13" />
                In PPMP
              </span>

              <button
                v-else
                type="button"
                class="drmc-btn-primary shrink-0 px-3 py-1.5 text-xs"
                @click="openAddCatalogItemModal(catalogItem)"
              >
                <Plus :size="14" />
                Select
              </button>
            </div>
          </template>

          <div
            v-else
            class="py-12 text-center text-sm text-slate-500"
          >
            No items match your current search or category filter.
          </div>
        </div>

        <!-- MODAL FOOTER -->
        <div class="flex justify-end border-t border-slate-200 bg-slate-50 px-6 py-3.5">
          <button
            type="button"
            class="drmc-btn-secondary px-4 py-2 text-sm"
            @click="closeCatalogModal"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  </Teleport>

  <!-- ================================================================ -->
  <!-- PER-ITEM EDIT MODAL                                              -->
  <!-- ================================================================ -->
  <Teleport to="body">
    <div
      v-if="editingItem"
      class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 p-4 backdrop-blur-2xs"
      @click.self="closeItemModal"
    >
      <div
        role="dialog"
        aria-modal="true"
        :aria-label="isNewModalItem ? 'Add PPMP Item' : 'Edit PPMP Item'"
        class="flex max-h-[90vh] w-full max-w-4xl flex-col overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-2xl"
      >
        <!-- MODAL HEADER -->
        <div class="flex items-start justify-between gap-4 border-b border-slate-200 bg-slate-50/80 px-6 py-4">
          <div>
            <div class="flex items-center gap-2">
              <span class="rounded-lg bg-blue-100 px-2.5 py-0.5 text-xs font-bold text-blue-800">
                {{ isNewModalItem ? 'New Catalogue Item' : 'Edit PPMP Item' }}
              </span>
              <span class="font-mono text-xs font-semibold text-slate-600">
                {{ editingItem.code || '—' }}
              </span>
              <span class="text-xs text-slate-400">·</span>
              <span class="text-xs font-medium text-slate-600">
                {{ editingItem.categoryName }}
              </span>
            </div>

            <h3 class="mt-1.5 text-lg font-bold text-slate-900">
              {{ editingItem.generalDescriptionAndObjective || 'Configure PPMP Item' }}
            </h3>
          </div>

          <button
            type="button"
            title="Close modal"
            class="rounded-lg p-1.5 text-slate-400 transition hover:bg-slate-200/70 hover:text-slate-700"
            @click="closeItemModal"
          >
            <X :size="20" />
          </button>
        </div>

        <!-- MODAL BODY (SCROLLABLE) -->
        <div class="flex-1 space-y-5 overflow-y-auto p-6">
          <!-- MODAL ERROR -->
          <div
            v-if="modalErrorMessage"
            role="alert"
            class="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-xs font-medium text-red-800"
          >
            {{ modalErrorMessage }}
          </div>

          <!-- OFFICIAL PPMP FIELDS -->
          <div class="space-y-4">
            <div>
              <label class="mb-1.5 block text-xs font-semibold text-slate-700">
                General Description and Objective of the Project to be Procured
                <span class="text-red-600">*</span>
              </label>

              <textarea
                v-model="editingItem.generalDescriptionAndObjective"
                rows="3"
                maxlength="2000"
                placeholder="Describe the project/item to be procured and its objective..."
                class="drmc-input"
                required
              />
            </div>

            <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
              <div>
                <label class="mb-1.5 block text-xs font-semibold text-slate-700">
                  Project Type
                  <span class="text-red-600">*</span>
                </label>

                <PPMPProjectTypeField
                  v-model="editingItem.projectType"
                  :disabled="!canEdit || isSaving"
                />
              </div>

              <div>
                <label class="mb-1.5 block text-xs font-semibold text-slate-700">
                  Pre-Procurement Conf.
                  <span class="text-red-600">*</span>
                </label>

                <select
                  v-model="editingItem.preProcurementConference"
                  class="drmc-input"
                >
                  <option :value="null" disabled>
                    Select Yes or No
                  </option>
                  <option :value="true">Yes</option>
                  <option :value="false">No</option>
                </select>
              </div>

              <div>
                <label class="mb-1.5 block text-xs font-semibold text-slate-700">
                  Unit of Issue
                  <span class="text-red-600">*</span>
                </label>

                <input
                  v-model="editingItem.unit"
                  type="text"
                  maxlength="50"
                  placeholder="e.g. Piece, Box, Unit"
                  class="drmc-input"
                  required
                />
              </div>

              <div>
                <label class="mb-1.5 block text-xs font-semibold text-slate-700">
                  ABC / Unit (₱)
                  <span class="text-red-600">*</span>
                </label>

                <input
                  v-model.number="editingItem.unitPrice"
                  type="number"
                  min="0"
                  step="0.01"
                  class="drmc-input tabular-nums"
                  required
                />
              </div>
            </div>
          </div>

          <!-- EDITABLE PROCUREMENT PLANNING DETAILS (PROCUREMENT STAFF OVERRIDE) -->
          <div class="rounded-xl border border-blue-200/80 bg-blue-50/30 p-4">
            <div class="mb-3 flex flex-wrap items-center justify-between gap-2">
              <div>
                <h4 class="text-xs font-bold uppercase tracking-wider text-blue-900">
                  Procurement Planning Details
                </h4>
                <p class="mt-0.5 text-xs text-slate-600">
                  Schedule, mode of procurement, and funding details (editable by Procurement Staff).
                </p>
              </div>

              <span class="rounded-full bg-blue-100 px-2.5 py-0.5 text-[11px] font-semibold text-blue-800">
                Procurement Editable
              </span>
            </div>

            <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <div>
                <label class="mb-1.5 block text-xs font-semibold text-slate-700">
                  Recommended Mode of Procurement
                  <span class="text-red-600">*</span>
                </label>

                <input
                  v-model="editingItem.recommendedModeOfProcurement"
                  list="procurement-modes-list"
                  type="text"
                  maxlength="150"
                  placeholder="Select or enter mode of procurement"
                  class="drmc-input bg-white"
                  required
                />
                <datalist id="procurement-modes-list">
                  <option
                    v-for="mode in procurementModes"
                    :key="mode"
                    :value="mode"
                  />
                </datalist>
              </div>

              <div>
                <label class="mb-1.5 block text-xs font-semibold text-slate-700">
                  Source of Funds
                  <span class="text-red-600">*</span>
                </label>

                <input
                  v-model="editingItem.sourceOfFunds"
                  list="fund-sources-list"
                  type="text"
                  maxlength="100"
                  placeholder="e.g. GAA, Hospital Income"
                  class="drmc-input bg-white"
                  required
                />
                <datalist id="fund-sources-list">
                  <option
                    v-for="source in fundSources"
                    :key="source"
                    :value="source"
                  />
                </datalist>
              </div>

              <div>
                <label class="mb-1.5 block text-xs font-semibold text-slate-700">
                  Start of Procurement Activity (MM/YYYY)
                  <span class="text-red-600">*</span>
                </label>

                <input
                  v-model="editingItem.startOfProcurementActivity"
                  type="month"
                  class="drmc-input bg-white tabular-nums"
                  required
                />
              </div>

              <div>
                <label class="mb-1.5 block text-xs font-semibold text-slate-700">
                  End of Procurement Activity (MM/YYYY)
                  <span class="text-red-600">*</span>
                </label>

                <input
                  v-model="editingItem.endOfProcurementActivity"
                  type="month"
                  :min="editingItem.startOfProcurementActivity"
                  class="drmc-input bg-white tabular-nums"
                  required
                />
              </div>

              <div>
                <label class="mb-1.5 block text-xs font-semibold text-slate-700">
                  Expected Delivery / Implementation Period
                  <span class="text-red-600">*</span>
                </label>

                <input
                  v-model="editingItem.expectedDeliveryImplementationPeriod"
                  type="text"
                  maxlength="150"
                  placeholder="e.g. Q2 2026 or 30 Calendar Days"
                  class="drmc-input bg-white"
                  required
                />
              </div>

              <div>
                <label class="mb-1.5 block text-xs font-semibold text-slate-700">
                  Attached Supporting Documents
                </label>

                <input
                  v-model="editingItem.attachedSupportingDocuments"
                  type="text"
                  maxlength="255"
                  placeholder="e.g. Market Study, Technical Specifications"
                  class="drmc-input bg-white"
                />
              </div>
            </div>
          </div>

          <!-- SUB-UNIT ALLOCATIONS -->
          <div class="border-t border-slate-100 pt-4">
            <div class="flex flex-wrap items-center justify-between gap-2">
              <div>
                <h4 class="text-xs font-bold uppercase tracking-wider text-slate-700">
                  Sub-Unit Quantity Allocation <span class="text-red-600">*</span>
                </h4>
                <p class="mt-0.5 text-xs text-slate-500">
                  Specify the quantity per sub-unit (leave 0 for sub-units not requesting this item).
                </p>
              </div>

              <span class="rounded-lg bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-700 tabular-nums">
                Total Allocated: {{ getItemQuantity(editingItem) }} {{ editingItem.unit }}
              </span>
            </div>

            <div class="mt-3 grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
              <div
                v-for="allocation in editingItem.allocations"
                :key="allocation.sectionId"
                :class="[
                  'rounded-xl border p-3 transition',
                  Number(allocation.quantity) > 0
                    ? 'border-brand-300 bg-brand-50/35'
                    : 'border-slate-200/70 bg-slate-50/70',
                ]"
              >
                <label class="mb-1.5 block text-xs font-medium text-slate-700">
                  {{ allocation.sectionName }}
                </label>

                <input
                  v-model.number="allocation.quantity"
                  type="number"
                  min="0"
                  step="0.01"
                  class="drmc-input bg-white tabular-nums"
                />
              </div>
            </div>
          </div>
        </div>

        <!-- MODAL FOOTER -->
        <div class="flex flex-wrap items-center justify-between gap-4 border-t border-slate-200 bg-slate-50 px-6 py-4">
          <div class="flex items-center gap-5 text-sm">
            <div>
              <span class="text-xs font-medium text-slate-500">Total Qty:</span>
              <strong class="ml-1.5 font-bold text-slate-900 tabular-nums">
                {{ getItemQuantity(editingItem) }} {{ editingItem.unit }}
              </strong>
            </div>

            <div>
              <span class="text-xs font-medium text-slate-500">Item Budget:</span>
              <strong class="ml-1.5 font-bold text-brand-700 tabular-nums">
                {{ formatPHP(getItemBudget(editingItem)) }}
              </strong>
            </div>
          </div>

          <div class="flex items-center gap-2.5">
            <button
              type="button"
              class="drmc-btn-secondary px-4 py-2 text-sm"
              @click="closeItemModal"
            >
              Cancel
            </button>
            <button
              type="button"
              class="drmc-btn-primary px-4 py-2 text-sm"
              @click="saveModalItem"
            >
              <Check :size="16" />
              {{ isNewModalItem ? 'Add Item to PPMP' : 'Apply Item Changes' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>

  <!-- ================================================================ -->
  <!-- SAVE CONFIRMATION MODAL                                          -->
  <!-- ================================================================ -->
  <Teleport to="body">
    <div
      v-if="showSaveConfirmModal && ppmp"
      class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/55 p-4 backdrop-blur-2xs"
      @click.self="showSaveConfirmModal = false"
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-label="Confirm Procurement Changes"
        class="w-full max-w-md rounded-2xl border border-slate-200 bg-white p-6 shadow-2xl"
      >
        <h3 class="text-lg font-bold text-slate-900">
          Save Procurement Changes
        </h3>
        <p class="mt-2 text-sm leading-relaxed text-slate-600">
          Save updates to <strong class="text-slate-900">{{ ppmp.ppmpReference }}</strong>
          with an updated budget of <strong class="text-brand-700">{{ formatPHP(totalEstimatedBudget) }}</strong>?
          All changes will be logged in the Procurement Change History.
        </p>

        <div class="mt-6 flex justify-end gap-2.5">
          <button
            type="button"
            class="drmc-btn-secondary px-4 py-2 text-sm"
            @click="showSaveConfirmModal = false"
          >
            Cancel
          </button>
          <button
            type="button"
            class="drmc-btn-primary px-4 py-2 text-sm"
            @click="executeSaveChanges"
          >
            Yes, Save Changes
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>