<script setup lang="ts">
import { FileText, Pencil, ChevronDown, X, Trash2 } from '@lucide/vue'

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
  | 'updateQuantityFromAllocations'
>

const props = defineProps<{
  viewModel: ItemListViewModel
  activeCategory: PlanningYearCategoryDto
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
  updateQuantityFromAllocations,
} = props.viewModel
const { activeCategory } = props
</script>

<template>
  <!-- ITEM LIST -->
  <div class="divide-y divide-slate-200">
    <div
      v-if="getItemsForCategory(activeCategory.categoryId).length === 0"
      class="px-6 py-14 text-center"
    >
      <FileText :size="32" class="mx-auto mb-3 text-slate-300" />
      <p class="text-base font-semibold text-slate-700">No items added to this category yet.</p>
      <p class="mt-1 text-sm text-slate-500">
        Use the form above to encode your first procurement project.
      </p>
    </div>

    <div
      v-for="item in getItemsForCategory(activeCategory.categoryId)"
      :key="item.lineId"
      class="px-6 py-5 transition-colors hover:bg-slate-50"
    >
      <!-- ITEM SUMMARY -->
      <div class="flex flex-col gap-4 lg:flex-row lg:items-start">
        <div class="min-w-0 flex-1">
          <div class="flex flex-wrap items-center gap-2">
            <span
              class="rounded-full px-2.5 py-0.5 text-[11px] font-bold uppercase tracking-wider"
              :class="
                item.catalogItemId === null
                  ? 'bg-blue-50 text-blue-700'
                  : 'bg-emerald-50 text-emerald-700'
              "
            >
              {{ getItemEntryLabel(item) }}
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

          <p class="mt-1 text-sm text-slate-500">{{ getProjectType(item) }} · {{ item.unit }}</p>

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
          <h5 class="text-sm font-bold text-slate-800">Edit Procurement Project</h5>
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
              @input="
                updateItemField(
                  item,
                  'generalDescriptionAndObjective',
                  ($event.target as HTMLTextAreaElement).value,
                )
              "
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
              @input="
                updateItemField(item, 'projectType', ($event.target as HTMLInputElement).value)
              "
            />
          </div>

          <div>
            <label class="mb-1 block text-xs font-semibold text-slate-600"> Unit of Issue </label>
            <input
              :value="item.unit"
              type="text"
              maxlength="50"
              class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
              @input="updateItemField(item, 'unit', ($event.target as HTMLInputElement).value)"
            />
          </div>

          <div>
            <label class="mb-1 block text-xs font-semibold text-slate-600"> ABC / Unit </label>
            <input
              :value="item.unitPrice"
              type="number"
              min="0"
              step="0.01"
              class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
              @input="
                updateItemField(
                  item,
                  'unitPrice',
                  Number(($event.target as HTMLInputElement).value),
                )
              "
            />
          </div>

          <div>
            <label class="mb-1 block text-xs font-semibold text-slate-600">
              Pre-Procurement Conference
            </label>
            <select
              :value="
                item.preProcurementConference === true
                  ? 'true'
                  : item.preProcurementConference === false
                    ? 'false'
                    : ''
              "
              class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
              @change="
                updateItemField(
                  item,
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

        <div class="mt-4 rounded-lg border border-slate-200 bg-white p-4">
          <div class="mb-3 flex items-center justify-between">
            <div>
              <h6 class="text-sm font-bold text-slate-800">Section Requirements</h6>
              <p class="text-xs text-slate-500">
                The total quantity is calculated from these section quantities.
              </p>
            </div>
            <div class="text-right">
              <p class="text-xs text-slate-500">Total Quantity</p>
              <p class="text-lg font-black text-emerald-700">{{ item.quantity }} {{ item.unit }}</p>
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
            <span class="font-semibold text-slate-700"> Calculated Total Quantity </span>
            <span class="font-black text-emerald-700"> {{ item.quantity }} {{ item.unit }} </span>
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
</template>
