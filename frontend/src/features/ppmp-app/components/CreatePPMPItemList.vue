<script setup lang="ts">
import { computed } from 'vue'
import { FileText, Pencil, ChevronDown, X, Trash2, Plus } from '@lucide/vue'

import PPMPPlanningFieldsForm from './PPMPPlanningFieldsForm.vue'
import PPMPProjectTypeField from './PPMPProjectTypeField.vue'
import type { PlanningYearCategoryDto } from '../models/master-data.types'
import type { CreatePPMPViewModel } from '../viewmodels/useCreatePPMPViewModel'

type ItemListViewModel = Pick<
  CreatePPMPViewModel,
  | 'addItemSection'
  | 'allocationTotal'
  | 'cancelEditingItem'
  | 'editingLineId'
  | 'expandedAllocationRows'
  | 'formatCurrency'
  | 'getAvailableSections'
  | 'getDescription'
  | 'getItemEntryLabel'
  | 'getItemsForCategory'
  | 'getProjectType'
  | 'isAllocationBalanced'
  | 'isSaving'
  | 'removeItem'
  | 'removeItemSection'
  | 'startEditingItem'
  | 'toggleAllocationRow'
  | 'updateItemField'
  | 'updateItemPlanningFields'
  | 'updateQuantityFromAllocations'
>

const props = defineProps<{
  viewModel: ItemListViewModel
  activeCategory: PlanningYearCategoryDto
}>()

const emit = defineEmits<{
  (e: 'open-add-modal'): void
}>()

const {
  addItemSection,
  allocationTotal,
  cancelEditingItem,
  editingLineId,
  expandedAllocationRows,
  formatCurrency,
  getAvailableSections,
  getDescription,
  getItemEntryLabel,
  getItemsForCategory,
  getProjectType,
  isAllocationBalanced,
  isSaving,
  removeItem,
  removeItemSection,
  startEditingItem,
  toggleAllocationRow,
  updateItemField,
  updateItemPlanningFields,
  updateQuantityFromAllocations,
} = props.viewModel

const editingItem = computed(() => {
  if (editingLineId.value === null) return null
  return (
    getItemsForCategory(props.activeCategory.categoryId).find(
      (entry) => entry.lineId === editingLineId.value,
    ) ?? null
  )
})
</script>

<template>
  <!-- ITEM LIST -->
  <div class="divide-y divide-slate-200/80">
    <div
      v-if="getItemsForCategory(activeCategory.categoryId).length === 0"
      class="px-6 py-14 text-center"
    >
      <FileText :size="32" class="mx-auto mb-3 text-slate-300" />
      <p class="text-base font-semibold text-slate-700">No items added to this category yet.</p>
      <p class="mt-1 text-sm text-slate-500">
        Click the button below to select from Procurement's default items or add an unlisted item.
      </p>
      <button
        type="button"
        class="drmc-btn-primary mt-4 px-4 py-2 text-xs sm:text-sm"
        @click="emit('open-add-modal')"
      >
        <Plus :size="16" />
        Add Item to {{ activeCategory.categoryName }}
      </button>
    </div>

    <div
      v-for="item in getItemsForCategory(activeCategory.categoryId)"
      :key="item.lineId"
      class="px-6 py-5 transition-colors hover:bg-slate-50/70"
    >
      <!-- ITEM SUMMARY -->
      <div class="flex flex-col gap-4 lg:flex-row lg:items-start">
        <div class="min-w-0 flex-1">
          <div class="flex flex-wrap items-center gap-2">
            <span
              class="rounded-full px-2.5 py-0.5 text-[11px] font-bold uppercase tracking-wider ring-1 ring-inset"
              :class="
                item.catalogItemId === null
                  ? 'bg-blue-50 text-blue-700 ring-blue-600/20'
                  : 'bg-brand-50 text-brand-700 ring-brand-600/20'
              "
            >
              {{ getItemEntryLabel(item) }}
            </span>
            <span
              v-if="item.preProcurementConference === true"
              class="rounded-full bg-amber-50 px-2.5 py-0.5 text-[11px] font-bold text-amber-700 ring-1 ring-inset ring-amber-600/20"
            >
              Pre-Procurement Conference: Yes
            </span>
            <span
              v-else
              class="rounded-full bg-slate-100 px-2.5 py-0.5 text-[11px] font-bold text-slate-600 ring-1 ring-inset ring-slate-300/60"
            >
              Pre-Procurement Conference: No
            </span>
          </div>

          <p class="mt-2 text-base font-semibold leading-snug text-slate-900">
            {{ getDescription(item) }}
          </p>

          <p class="mt-1 text-sm text-slate-500">{{ getProjectType(item) }} · {{ item.unit }}</p>

          <div class="mt-3 flex flex-wrap gap-2">
            <span
              v-for="allocation in item.sectionAllocations"
              :key="allocation.sectionId"
              class="rounded-full border border-slate-200 bg-slate-50 px-2.5 py-1 text-xs font-medium text-slate-600 tabular-nums"
            >
              {{ allocation.sectionName }}:
              <strong class="text-slate-900">{{ allocation.quantity }}</strong>
            </span>
          </div>
        </div>

        <div class="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:w-[420px] lg:shrink-0">
          <div class="text-right">
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Total Quantity
            </p>
            <p class="mt-1 text-sm font-semibold text-slate-700 tabular-nums">
              {{ item.quantity }} {{ item.unit }}
            </p>
          </div>

          <div class="text-right">
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              ABC / Unit
            </p>
            <p class="mt-1 text-sm font-semibold text-slate-700 tabular-nums">
              {{ formatCurrency(item.unitPrice) }}
            </p>
          </div>

          <div class="col-span-2 text-right sm:col-span-1">
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Estimated Budget
            </p>
            <p class="mt-1 text-base font-bold text-brand-700 tabular-nums">
              {{ formatCurrency(item.quantity * item.unitPrice) }}
            </p>
          </div>
        </div>

        <div class="flex flex-wrap items-center gap-2 lg:shrink-0">
          <button
            type="button"
            class="drmc-btn-secondary px-3 py-2 text-xs"
            @click="startEditingItem(item)"
          >
            <Pencil :size="14" />
            Edit
          </button>

          <button
            type="button"
            class="drmc-btn-secondary px-3 py-2 text-xs"
            @click="toggleAllocationRow(item.lineId)"
          >
            <ChevronDown
              :size="14"
              class="transition-transform"
              :class="{ '-rotate-90': !expandedAllocationRows.includes(item.lineId) }"
            />
            Sub-Units
          </button>

          <button
            type="button"
            class="rounded-lg border border-red-200 bg-white p-2 text-red-500 transition hover:bg-red-50"
            title="Remove item"
            @click="removeItem(item.lineId)"
          >
            <Trash2 :size="15" />
          </button>
        </div>
      </div>

      <!-- SECTION ALLOCATION SUMMARY -->
      <div
        v-if="expandedAllocationRows.includes(item.lineId)"
        class="mt-5 rounded-xl border border-slate-200/80 bg-slate-50/70 px-4 py-5 sm:px-6"
      >
        <div class="mb-4 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
          <p class="text-sm font-bold text-slate-900">
            Sub-unit allocation — {{ getDescription(item) }}
          </p>
          <span
            class="text-xs font-semibold tabular-nums"
            :class="isAllocationBalanced(item) ? 'text-brand-700' : 'text-red-600'"
          >
            Allocated {{ allocationTotal(item) }} / {{ item.quantity }}
            {{ item.unit }}
            <span v-if="!isAllocationBalanced(item)">⚠</span>
          </span>
        </div>

        <div
          v-if="!item.sectionAllocations?.length"
          class="rounded-xl border border-dashed border-slate-300 bg-white p-4 text-center text-sm text-slate-500"
        >
          No sub-unit allocations.
        </div>

        <div v-else class="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <div
            v-for="allocation in item.sectionAllocations"
            :key="allocation.sectionId"
            class="rounded-xl border border-slate-200 bg-white p-4"
          >
            <p class="text-xs font-semibold text-slate-600">
              {{ allocation.sectionName }}
            </p>
            <p class="mt-1 text-lg font-bold text-slate-900 tabular-nums">
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

  <!-- EDIT ITEM DETAILS MODAL -->
  <Teleport to="body">
    <div
      v-if="editingItem"
      class="print:hidden fixed inset-0 z-50 flex items-center justify-center overflow-y-auto bg-slate-900/55 p-4 backdrop-blur-2xs"
      @click.self="cancelEditingItem"
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-label="Edit Procurement Project"
        class="my-auto flex max-h-[90vh] w-full max-w-4xl flex-col overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-2xl"
      >
        <!-- MODAL HEADER -->
        <div class="flex items-start justify-between gap-4 border-b border-slate-200/80 bg-slate-50/80 px-6 py-4">
          <div>
            <span class="inline-flex items-center rounded-full bg-brand-50 px-2.5 py-0.5 text-[11px] font-bold uppercase tracking-wider text-brand-700 ring-1 ring-inset ring-brand-600/20">
              {{ activeCategory.categoryName }}
            </span>
            <h4 class="mt-1.5 text-lg font-bold tracking-tight text-slate-900">
              Edit Procurement Project
            </h4>
            <p class="mt-0.5 text-xs text-slate-500">
              Update item details, procurement schedule, and sub-unit quantity allocations.
            </p>
          </div>

          <button
            type="button"
            class="rounded-lg p-2 text-slate-400 transition hover:bg-slate-200/70 hover:text-slate-700"
            title="Close modal"
            @click="cancelEditingItem"
          >
            <X :size="20" />
          </button>
        </div>

        <!-- SCROLLABLE MODAL BODY -->
        <div class="space-y-5 overflow-y-auto p-6">
          <div class="grid grid-cols-1 gap-3.5 md:grid-cols-2 xl:grid-cols-4">
            <div class="md:col-span-2 xl:col-span-4">
              <label class="mb-1.5 block text-xs font-semibold text-slate-700">
                General Description and Objective of the Project to be Procured
              </label>
              <textarea
                :value="getDescription(editingItem)"
                rows="3"
                maxlength="2000"
                class="drmc-input resize-y"
                @input="
                  updateItemField(
                    editingItem,
                    'generalDescriptionAndObjective',
                    ($event.target as HTMLTextAreaElement).value,
                  )
                "
              ></textarea>
            </div>

            <div>
              <label class="mb-1.5 block text-xs font-semibold text-slate-700">
                Type of the Project to be Procured
              </label>
              <PPMPProjectTypeField
                :model-value="getProjectType(editingItem)"
                @update:model-value="updateItemField(editingItem, 'projectType', $event)"
              />
            </div>

            <div>
              <label class="mb-1.5 block text-xs font-semibold text-slate-700">Unit of Issue</label>
              <input
                :value="editingItem.unit"
                type="text"
                maxlength="50"
                class="drmc-input"
                @input="
                  updateItemField(editingItem, 'unit', ($event.target as HTMLInputElement).value)
                "
              />
            </div>

            <div>
              <label class="mb-1.5 block text-xs font-semibold text-slate-700">ABC / Unit</label>
              <input
                :value="editingItem.unitPrice"
                type="number"
                min="0"
                step="0.01"
                class="drmc-input tabular-nums"
                @input="
                  updateItemField(
                    editingItem,
                    'unitPrice',
                    Number(($event.target as HTMLInputElement).value),
                  )
                "
              />
            </div>

            <div>
              <label class="mb-1.5 block text-xs font-semibold text-slate-700">
                Pre-Procurement Conference
              </label>
              <select
                :value="
                  editingItem.preProcurementConference === true
                    ? 'true'
                    : editingItem.preProcurementConference === false
                      ? 'false'
                      : ''
                "
                class="drmc-input"
                @change="
                  updateItemField(
                    editingItem,
                    'preProcurementConference',
                    ($event.target as HTMLSelectElement).value === 'true',
                  )
                "
              >
                <option value="" disabled>Select Yes or No</option>
                <option value="true">Yes</option>
                <option value="false">No</option>
              </select>
            </div>
          </div>

          <PPMPPlanningFieldsForm
            :model-value="editingItem"
            @update:model-value="updateItemPlanningFields(editingItem, $event)"
          />

          <div class="rounded-xl border border-slate-200 bg-slate-50/60 p-4">
            <div class="mb-3 flex items-center justify-between">
              <div>
                <h6 class="text-sm font-bold text-slate-900">Sub-Unit Requirements</h6>
                <p class="text-xs text-slate-500">
                  The total quantity is calculated from these section quantities.
                </p>
              </div>
              <div class="text-right">
                <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
                  Total Quantity
                </p>
                <p class="text-lg font-extrabold text-brand-700 tabular-nums">
                  {{ editingItem.quantity }} {{ editingItem.unit }}
                </p>
              </div>
            </div>

            <select
              class="drmc-input bg-white"
              :disabled="isSaving || getAvailableSections(editingItem.sectionAllocations ?? []).length === 0"
              @change="addItemSection(editingItem, $event)"
            >
              <option value="">
                {{
                  getAvailableSections(editingItem.sectionAllocations ?? []).length
                    ? 'Add another sub-unit'
                    : 'All available sub-units have been added'
                }}
              </option>
              <option
                v-for="section in getAvailableSections(editingItem.sectionAllocations ?? [])"
                :key="section.sectionId"
                :value="section.sectionId"
              >
                {{ section.sectionCode }} — {{ section.sectionName }}
              </option>
            </select>

            <div
              v-if="!editingItem.sectionAllocations?.length"
              class="mt-3 rounded-xl border border-dashed border-slate-300 bg-white p-4 text-center text-sm text-slate-500"
            >
              No sub-units allocated.
            </div>

            <div v-else class="mt-3 space-y-2.5">
              <div
                v-for="allocation in editingItem.sectionAllocations"
                :key="allocation.sectionId"
                class="grid grid-cols-1 items-center gap-3 rounded-xl border border-slate-200 bg-white p-3 sm:grid-cols-[1fr_140px_auto]"
              >
                <p class="text-sm font-semibold text-slate-700">
                  {{ allocation.sectionName }}
                </p>

                <input
                  v-model.number="allocation.quantity"
                  type="number"
                  min="0"
                  step="1"
                  class="drmc-input text-center font-semibold tabular-nums"
                  @input="updateQuantityFromAllocations(editingItem)"
                />

                <button
                  type="button"
                  class="inline-flex items-center justify-center gap-1.5 rounded-lg border border-red-200 px-3 py-2 text-xs font-semibold text-red-600 transition hover:bg-red-50"
                  @click="removeItemSection(editingItem, allocation.sectionId)"
                >
                  <Trash2 :size="14" />
                  Remove
                </button>
              </div>
            </div>

            <div class="mt-3 flex justify-between border-t border-slate-200/80 pt-3 text-sm">
              <span class="font-semibold text-slate-700">Calculated Total Quantity</span>
              <span class="font-extrabold text-brand-700 tabular-nums">
                {{ editingItem.quantity }} {{ editingItem.unit }}
              </span>
            </div>
          </div>
        </div>

        <!-- MODAL FOOTER -->
        <div class="flex items-center justify-between border-t border-slate-200/80 bg-slate-50/80 px-6 py-4">
          <div class="text-xs text-slate-500 tabular-nums">
            Estimated Budget:
            <strong class="ml-1 text-sm font-bold text-brand-700">
              {{ formatCurrency(editingItem.quantity * editingItem.unitPrice) }}
            </strong>
          </div>

          <button
            type="button"
            class="drmc-btn-primary px-5 py-2 text-xs sm:text-sm"
            @click="cancelEditingItem"
          >
            Done
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>