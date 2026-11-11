
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
} from '@lucide/vue'

import { apiRequest } from '@/services/api'
import { ppmpApiService } from '../services/ppmpApiService'

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

const selectedCatalogItem = ref('')
const procurementReason = ref('')

const errorMessage = ref('')
const successMessage = ref('')

const isLoading = ref(true)
const isSaving = ref(false)

const totalEstimatedBudget = computed(() =>
  items.value.reduce((total, item) => {
    const quantity = item.allocations.reduce(
      (sum, allocation) => sum + Number(allocation.quantity || 0),
      0,
    )

    return total + quantity * Number(item.unitPrice || 0)
  }, 0),
)

const itemCount = computed(() => items.value.length)

const canEdit = computed(() => ppmp.value?.status === 'Under Review')

const availableCatalogItems = computed(() => {
  const existingItemIds = new Set(
    items.value
      .map(item => item.itemId)
      .filter((id): id is number => id !== null),
  )

  return catalogItems.value.filter(
    item => !existingItemIds.has(item.itemId),
  )
})

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('en-PH', {
    style: 'currency',
    currency: 'PHP',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount)
}

function getItemQuantity(item: EditableItem) {
  return item.allocations.reduce(
    (total, allocation) => total + Number(allocation.quantity || 0),
    0,
  )
}

function getItemBudget(item: EditableItem) {
  return getItemQuantity(item) * Number(item.unitPrice || 0)
}

function createAllocations(
  existingAllocations: {
    sectionId: number
    sectionName: string
    quantity: number
  }[] = [],
): EditableAllocation[] {
  return sections.value.map(section => {
    const existing = existingAllocations.find(
      allocation => allocation.sectionId === section.sectionId,
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

    sections.value = loadedSections.filter(section => section.isActive)

    if (sections.value.length === 0) {
      throw new Error(
        'No active department sections were found. Please configure the department sections first.',
      )
    }

    const loadedCategories = await apiRequest<PlanningYearCategoryDto[]>(
      `/PlanningYearCategories/planning-year/${record.planningYearId}`,
    )

    planningYearCategories.value = loadedCategories.filter(
      category => category.isActive,
    )

    const itemResults = await Promise.all(
      planningYearCategories.value.map(category =>
        apiRequest<ItemDto[]>(
          `/Items/category/${category.categoryId}`,
        ),
      ),
    )

    catalogItems.value = itemResults
      .flat()
      .filter(item => item.isActive)

    ppmp.value = record

    items.value = record.items.map((item, index) => {
      const generalDescription =
        item.generalDescriptionAndObjective?.trim() ||
        item.itemDescription ||
        ''

      const projectType =
        item.projectType?.trim() ||
        ''

      return {
        lineId: item.ppmpItemId || -(index + 1),
        ppmpItemId: item.ppmpItemId,
        itemId: item.itemId,
        categoryId: item.categoryId,
        categoryName: item.categoryName,

        code: item.itemCode ?? '',

        // Legacy compatibility fields.
        name: generalDescription,
        itemType: item.itemType ?? null,
        specifications: item.specifications ?? '',

        // Official PPMP fields.
        generalDescriptionAndObjective: generalDescription,
        projectType,
        preProcurementConference:
          item.preProcurementConference ?? null,

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

function addItem() {
  errorMessage.value = ''
  successMessage.value = ''

  if (!canEdit.value) {
    errorMessage.value = 'This PPMP is not available for editing.'
    return
  }

  const selectedItem = catalogItems.value.find(
    item => item.itemId === Number(selectedCatalogItem.value),
  )

  if (!selectedItem) {
    errorMessage.value = 'Please select an item from the catalogue.'
    return
  }

  const assignedCategory = planningYearCategories.value.find(
    category => category.categoryId === selectedItem.categoryId,
  )

  if (!assignedCategory) {
    errorMessage.value =
      'This item does not belong to a category assigned to the planning year.'
    return
  }

  items.value.push({
    lineId: Date.now(),
    ppmpItemId: null,
    itemId: selectedItem.itemId,
    categoryId: selectedItem.categoryId,
    categoryName: selectedItem.categoryName,

    code: selectedItem.itemCode,

    // Initial description comes from the catalogue but remains editable.
    name: selectedItem.itemName,
    itemType: null,
    specifications: '',

    generalDescriptionAndObjective: selectedItem.itemName,
    projectType: '',
    preProcurementConference: null,

    unit: selectedItem.unit,
    unitPrice: 0,
    allocations: createAllocations(),
  })

  selectedCatalogItem.value = ''
}

function removeItem(lineId: number) {
  errorMessage.value = ''
  successMessage.value = ''

  if (!canEdit.value) return

  items.value = items.value.filter(item => item.lineId !== lineId)
}

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
    if (
      !item.generalDescriptionAndObjective.trim() ||
      !item.projectType.trim() ||
      !item.unit.trim()
    ) {
      errorMessage.value =
        'Please complete the general description, project type, and unit of issue for every item.'
      return false
    }

    if (item.preProcurementConference === null) {
      errorMessage.value =
        `Please select Yes or No for Pre-Procurement Conference for ${item.generalDescriptionAndObjective}.`
      return false
    }

    if (
      !Number.isFinite(Number(item.unitPrice)) ||
      Number(item.unitPrice) < 0
    ) {
      errorMessage.value =
        `Please enter a valid unit price for ${item.generalDescriptionAndObjective}.`
      return false
    }

    const invalidAllocation = item.allocations.find(
      allocation =>
        !Number.isFinite(Number(allocation.quantity)) ||
        Number(allocation.quantity) < 0,
    )

    if (invalidAllocation) {
      errorMessage.value =
        `Please check the section allocation quantities for ${item.generalDescriptionAndObjective}.`
      return false
    }

    const positiveAllocations = item.allocations.filter(
      allocation => Number(allocation.quantity) > 0,
    )

    if (positiveAllocations.length === 0) {
      errorMessage.value =
        `Please allocate a quantity to at least one section for ${item.generalDescriptionAndObjective}.`
      return false
    }

    const hasInvalidPrecision =
      Math.abs(Number(item.unitPrice) * 100 -
        Math.round(Number(item.unitPrice) * 100)) > 0.000001 ||
      item.allocations.some(allocation =>
        Math.abs(Number(allocation.quantity) * 100 -
          Math.round(Number(allocation.quantity) * 100)) > 0.000001,
      )

    if (hasInvalidPrecision) {
      errorMessage.value =
        `Quantity and unit price for ${item.generalDescriptionAndObjective} must have no more than two decimal places.`
      return false
    }
  }

  return true
}

function buildUpdateRequest(): UpdatePPMPRequest {
  const updateItems: UpdatePPMPItemRequest[] = items.value.map(item => {
    const generalDescription =
      item.generalDescriptionAndObjective.trim()

    const projectType = item.projectType.trim()

    const positiveAllocations = item.allocations
      .filter(allocation => Number(allocation.quantity) > 0)
      .map(allocation => ({
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

      // Legacy fields retained for current backend compatibility.
      itemCode: item.code,
      itemDescription: generalDescription,
      itemType: projectType,
      unit: item.unit.trim(),
      specifications: null,

      // Official PPMP fields.
      generalDescriptionAndObjective: generalDescription,
      projectType,
      preProcurementConference: item.preProcurementConference === true,

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

async function saveChanges() {
  errorMessage.value = ''
  successMessage.value = ''

  if (!validateForm() || !ppmp.value) return

  const confirmed = window.confirm(
    `Save Procurement changes to ${ppmp.value.ppmpReference}?`,
  )

  if (!confirmed) return

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
  <div class="mx-auto max-w-7xl space-y-6">

    <!-- PAGE HEADING -->
    <div>
      <button
        type="button"
        class="mb-3 inline-flex items-center gap-2 text-sm font-medium text-slate-500 hover:text-emerald-700"
        @click="cancelEditing"
      >
        <ArrowLeft :size="17" />
        Back to Procurement Review
      </button>

      <div class="flex items-center gap-3">
        <div class="rounded-lg bg-emerald-50 p-2.5">
          <Pencil :size="23" class="text-emerald-700" />
        </div>

        <div>
          <h1 class="text-2xl font-bold text-slate-900">
            Procurement PPMP Editor
          </h1>

          <p class="mt-1 text-sm text-slate-500">
            Review and update PPMPs approved by the Unit Head.
          </p>
        </div>
      </div>
    </div>

    <!-- LOADING -->
    <div
      v-if="isLoading"
      class="rounded-xl border border-slate-200 bg-white p-10 text-center"
    >
      <p class="text-sm text-slate-500">
        Loading PPMP record and master data...
      </p>
    </div>

    <!-- ERROR WHEN RECORD CANNOT BE LOADED -->
    <div
      v-else-if="!ppmp"
      class="rounded-xl border border-red-200 bg-white p-8 text-center"
    >
      <ClipboardList
        :size="36"
        class="mx-auto mb-3 text-red-400"
      />

      <h2 class="font-semibold text-slate-900">
        Unable to Open PPMP
      </h2>

      <p class="mx-auto mt-2 max-w-lg text-sm text-red-600">
        {{ errorMessage }}
      </p>

      <button
        type="button"
        class="mt-5 rounded-lg bg-emerald-700 px-5 py-2.5 text-sm font-medium text-white hover:bg-emerald-800"
        @click="cancelEditing"
      >
        Back to Procurement Review
      </button>
    </div>

    <template v-else>

      <!-- PPMP INFORMATION -->
      <section class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <div class="mb-5 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 class="font-semibold text-slate-900">
              PPMP Information
            </h2>

            <p class="mt-1 text-xs text-slate-500">
              Submitted department information.
            </p>
          </div>

          <span class="w-fit rounded-full bg-amber-50 px-3 py-1 text-xs font-medium text-amber-700">
            {{ ppmp.status }}
          </span>
        </div>

        <div class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
          <div>
            <p class="text-xs text-slate-500">Reference Number</p>
            <p class="mt-1 font-semibold text-slate-900">
              {{ ppmp.ppmpReference }}
            </p>
          </div>

          <div>
            <p class="text-xs text-slate-500">Department</p>
            <p class="mt-1 font-semibold text-slate-900">
              {{ ppmp.departmentName }}
            </p>
          </div>

          <div>
            <p class="text-xs text-slate-500">Planning Year</p>
            <p class="mt-1 font-semibold text-slate-900">
              {{ ppmp.planningYear }}
            </p>
          </div>

          <div>
            <p class="text-xs text-slate-500">Date Submitted</p>
            <p class="mt-1 font-semibold text-slate-900">
              {{ ppmp.submittedAt ? new Date(ppmp.submittedAt).toLocaleDateString('en-PH') : '—' }}
            </p>
          </div>
        </div>
      </section>

      <!-- ITEM CATALOGUE -->
      <section class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <div class="mb-5">
          <h2 class="font-semibold text-slate-900">
            Item Catalogue
          </h2>

          <p class="mt-1 text-xs text-slate-500">
            Add items from the active catalogue categories assigned to this planning year.
          </p>
        </div>

        <div class="flex flex-col gap-3 sm:flex-row">
          <select
            v-model="selectedCatalogItem"
            class="min-w-0 flex-1 rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
          >
            <option value="" disabled>
              Select an item from the catalogue
            </option>

            <option
              v-for="catalogItem in availableCatalogItems"
              :key="catalogItem.itemId"
              :value="String(catalogItem.itemId)"
            >
              {{ catalogItem.itemCode }} — {{ catalogItem.itemName }}
              ({{ catalogItem.categoryName }})
            </option>
          </select>

          <button
            type="button"
            class="inline-flex items-center justify-center gap-2 rounded-lg bg-emerald-700 px-5 py-2.5 text-sm font-medium text-white hover:bg-emerald-800"
            @click="addItem"
          >
            <Plus :size="18" />
            Add Item
          </button>
        </div>

        <p
          v-if="availableCatalogItems.length === 0"
          class="mt-3 text-xs text-slate-500"
        >
          No additional active catalogue items are available for this planning year.
        </p>
      </section>

      <!-- PPMP ITEMS -->
      <section class="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
        <div class="flex flex-col gap-2 border-b border-slate-200 px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 class="font-semibold text-slate-900">
              PPMP Items and Section Allocations
            </h2>

            <p class="mt-1 text-xs text-slate-500">
              Update the official PPMP item details and distribute quantities across department sections.
            </p>
          </div>

          <span class="text-sm font-medium text-slate-600">
            {{ itemCount }} item line(s)
          </span>
        </div>

        <div
          v-if="items.length === 0"
          class="px-5 py-12 text-center"
        >
          <Calculator
            :size="32"
            class="mx-auto text-slate-300"
          />

          <p class="mt-3 font-medium text-slate-700">
            No items in this PPMP
          </p>

          <p class="mt-1 text-sm text-slate-500">
            Add an item from the catalogue above.
          </p>
        </div>

        <div v-else class="space-y-5 p-5">
          <article
            v-for="item in items"
            :key="item.lineId"
            class="rounded-lg border border-slate-200 p-4"
          >
            <!-- ITEM HEADING -->
            <div class="flex flex-wrap items-start justify-between gap-3">
              <div>
                <p class="font-semibold text-slate-900">
                  {{ item.generalDescriptionAndObjective || 'New PPMP Item' }}
                </p>

                <p class="mt-1 text-xs text-slate-500">
                  Catalogue Code: {{ item.code || '—' }}
                  · {{ item.categoryName }}
                  · {{ item.unit }}
                </p>
              </div>

              <button
                type="button"
                title="Remove item"
                class="rounded-lg p-2 text-red-600 hover:bg-red-50"
                @click="removeItem(item.lineId)"
              >
                <Trash2 :size="17" />
              </button>
            </div>

            <!-- OFFICIAL PPMP FIELDS -->
            <div class="mt-5 space-y-4">

              <div>
                <label class="mb-1 block text-xs font-medium text-slate-600">
                  General Description and Objective of the Project to be Procured
                  <span class="text-red-600">*</span>
                </label>

                <textarea
                  v-model="item.generalDescriptionAndObjective"
                  rows="3"
                  maxlength="2000"
                  placeholder="Describe the project/item to be procured and its objective..."
                  class="w-full rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-emerald-600"
                />
              </div>

              <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <div>
                  <label class="mb-1 block text-xs font-medium text-slate-600">
                    Type of the Project to be Procured
                    <span class="text-red-600">*</span>
                  </label>

                  <input
                    v-model="item.projectType"
                    type="text"
                    maxlength="100"
                    placeholder="Enter project type (e.g. Goods)"
                    class="w-full rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-emerald-600"
                  />
                </div>

                <div>
                  <label class="mb-1 block text-xs font-medium text-slate-600">
                    Pre-Procurement Conference, if applicable
                    <span class="text-red-600">*</span>
                  </label>

                  <select
                    v-model="item.preProcurementConference"
                    class="w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm outline-none focus:border-emerald-600"
                  >
                    <option :value="null" disabled>
                      Select Yes or No
                    </option>

                    <option :value="true">
                      Yes
                    </option>

                    <option :value="false">
                      No
                    </option>
                  </select>
                </div>
              </div>

              <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <div>
                  <label class="mb-1 block text-xs font-medium text-slate-600">
                    Unit of Issue
                    <span class="text-red-600">*</span>
                  </label>

                  <input
                    v-model="item.unit"
                    type="text"
                    maxlength="50"
                    placeholder="e.g. Piece, Box, Unit"
                    class="w-full rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-emerald-600"
                  />
                </div>

                <div>
                  <label class="mb-1 block text-xs font-medium text-slate-600">
                    ABC / Unit (₱)
                    <span class="text-red-600">*</span>
                  </label>

                  <input
                    v-model.number="item.unitPrice"
                    type="number"
                    min="0"
                    step="0.01"
                    class="w-full rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-emerald-600"
                  />
                </div>
              </div>
            </div>

            <!-- SECTION ALLOCATIONS -->
            <div class="mt-5">
              <h3 class="text-sm font-semibold text-slate-800">
                Section Quantity Allocation
              </h3>

              <p class="mt-1 text-xs text-slate-500">
                Enter the quantity required by each section. The total quantity is calculated from these allocations.
              </p>

              <div class="mt-3 grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
                <div
                  v-for="allocation in item.allocations"
                  :key="allocation.sectionId"
                  class="rounded-lg bg-slate-50 p-3"
                >
                  <label class="mb-1 block text-xs font-medium text-slate-600">
                    {{ allocation.sectionName }}
                  </label>

                  <input
                    v-model.number="allocation.quantity"
                    type="number"
                    min="0"
                    step="0.01"
                    class="w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm outline-none focus:border-emerald-600"
                  />
                </div>
              </div>
            </div>

            <!-- ITEM TOTALS -->
            <div class="mt-4 flex flex-wrap justify-between gap-2 border-t border-slate-100 pt-3 text-sm">
              <div>
                <span class="text-slate-500">Total Quantity:</span>
                <strong class="ml-2 text-slate-800">
                  {{ getItemQuantity(item) }}
                </strong>
              </div>

              <div>
                <span class="text-slate-500">Item Budget:</span>
                <strong class="ml-2 text-emerald-800">
                  {{ formatCurrency(getItemBudget(item)) }}
                </strong>
              </div>
            </div>
          </article>
        </div>

        <!-- BUDGET SUMMARY -->
        <div class="border-t border-slate-200 bg-slate-50 px-5 py-5">
          <div class="ml-auto flex max-w-sm items-center justify-between gap-4">
            <span class="text-sm font-medium text-slate-600">
              Updated Estimated Budget
            </span>

            <span class="text-xl font-bold text-emerald-800">
              {{ formatCurrency(totalEstimatedBudget) }}
            </span>
          </div>
        </div>
      </section>

      <!-- PROCUREMENT CHANGE REASON -->
      <section class="rounded-xl border border-blue-200 bg-white p-5 shadow-sm">
        <div class="mb-4">
          <h2 class="font-semibold text-slate-900">
            Procurement Change Reason
          </h2>

          <p class="mt-1 text-xs text-slate-500">
            Explain why Procurement modified the submitted PPMP. The backend will save this reason with the change history.
          </p>
        </div>

        <textarea
          v-model="procurementReason"
          rows="4"
          maxlength="1000"
          placeholder="Enter the reason for the changes made to this PPMP..."
          class="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-blue-600 focus:ring-2 focus:ring-blue-100"
        />
      </section>

      <!-- FEEDBACK -->
      <div
        v-if="errorMessage"
        class="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"
        role="alert"
      >
        {{ errorMessage }}
      </div>

      <div
        v-if="successMessage"
        class="rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700"
        role="status"
      >
        {{ successMessage }}
      </div>

      <!-- ACTION BUTTONS -->
      <div class="flex flex-col-reverse justify-end gap-3 sm:flex-row">
        <button
          type="button"
          class="rounded-lg border border-slate-300 bg-white px-5 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50"
          @click="cancelEditing"
        >
          Cancel
        </button>

        <button
          type="button"
          :disabled="isSaving || !canEdit"
          class="inline-flex items-center justify-center gap-2 rounded-lg bg-emerald-700 px-5 py-2.5 text-sm font-medium text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-60"
          @click="saveChanges"
        >
          <Save :size="17" />
          {{ isSaving ? 'Saving...' : 'Save Procurement Changes' }}
        </button>
      </div>

    </template>
  </div>
</template>
