<script setup lang="ts">
import { Search, Plus, Trash2 } from '@lucide/vue'

import type { PlanningYearCategoryDto } from '../models/master-data.types'
import type { CreatePPMPViewModel } from '../viewmodels/useCreatePPMPViewModel'

type EntryViewModel = Pick<
  CreatePPMPViewModel,
  | 'addCatalogueDraftSection'
  | 'addCatalogueItem'
  | 'catalogueItems'
  | 'catalogueSearch'
  | 'draftTotalQuantity'
  | 'filteredCatalogueItems'
  | 'formatCurrency'
  | 'getAvailableSections'
  | 'getCatalogueItemDraft'
  | 'isLoadingCatalogueItems'
  | 'isSaving'
  | 'removeCatalogueDraftSection'
  | 'selectCatalogueItem'
>

const props = defineProps<{
  viewModel: EntryViewModel
  activeCategory: PlanningYearCategoryDto
}>()
const {
  addCatalogueDraftSection,
  addCatalogueItem,
  catalogueItems,
  catalogueSearch,
  draftTotalQuantity,
  filteredCatalogueItems,
  formatCurrency,
  getAvailableSections,
  getCatalogueItemDraft,
  isLoadingCatalogueItems,
  isSaving,
  removeCatalogueDraftSection,
  selectCatalogueItem,
} = props.viewModel
const { activeCategory } = props
</script>

<template>
  <div class="border-b border-slate-200 bg-white p-6">
    <div class="mb-4 flex items-center gap-2">
      <Search :size="18" class="text-emerald-600" />
      <h4 class="text-base font-bold text-slate-900">Select a Standardized Catalogue Item</h4>
    </div>

    <input
      v-model="catalogueSearch"
      type="search"
      placeholder="Search catalogue by item code, name, type, unit, or specifications"
      class="mb-4 w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
      :disabled="isSaving || isLoadingCatalogueItems"
    />

    <div
      v-if="isLoadingCatalogueItems"
      class="rounded-lg border border-slate-200 bg-slate-50 p-5 text-center text-sm text-slate-600"
    >
      Loading catalogue items...
    </div>
    <div
      v-else-if="catalogueItems.length === 0"
      class="rounded-lg border border-amber-200 bg-amber-50 p-5 text-sm text-amber-800"
    >
      No catalogue items are available for this planning year and category. You can switch to Manual
      Entry for a specialized requirement.
    </div>
    <div
      v-else-if="filteredCatalogueItems.length === 0"
      class="rounded-lg border border-slate-200 bg-slate-50 p-5 text-center text-sm text-slate-600"
    >
      No catalogue items match your search.
    </div>
    <div v-else class="grid max-h-72 gap-2 overflow-y-auto">
      <button
        v-for="catalogueItem in filteredCatalogueItems"
        :key="catalogueItem.itemId"
        type="button"
        class="rounded-lg border p-3 text-left transition-colors"
        :class="
          getCatalogueItemDraft(activeCategory.categoryId).catalogueItemId === catalogueItem.itemId
            ? 'border-emerald-500 bg-emerald-50 ring-1 ring-emerald-200'
            : 'border-slate-200 hover:border-emerald-300 hover:bg-slate-50'
        "
        :disabled="isSaving"
        @click="selectCatalogueItem(activeCategory, catalogueItem)"
      >
        <span class="font-semibold text-slate-900">
          {{ catalogueItem.itemName }}
        </span>
        <span class="float-right text-xs font-semibold text-slate-500">
          {{ catalogueItem.itemCode }}
        </span>
        <p class="mt-1 text-xs text-slate-600">
          {{ catalogueItem.itemType || 'Unspecified type' }} · {{ catalogueItem.unit }} ·
          {{ formatCurrency(catalogueItem.abcPerUnit) }} / unit
        </p>
        <p v-if="catalogueItem.specifications" class="mt-1 text-xs text-slate-500">
          {{ catalogueItem.specifications }}
        </p>
      </button>
    </div>

    <template v-if="getCatalogueItemDraft(activeCategory.categoryId).catalogueItemId !== null">
      <div class="mt-6 grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
        <div class="md:col-span-2 xl:col-span-4">
          <label class="mb-1 block text-xs font-semibold text-slate-600">
            General Description and Objective of the Project to be Procured
            <span class="text-red-500">*</span>
          </label>
          <textarea
            v-model="
              getCatalogueItemDraft(activeCategory.categoryId).generalDescriptionAndObjective
            "
            rows="3"
            maxlength="2000"
            class="w-full resize-y rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
            :disabled="isSaving"
          ></textarea>
        </div>
        <div>
          <label class="mb-1 block text-xs font-semibold text-slate-600">
            Type of the Project to be Procured <span class="text-red-500">*</span>
          </label>
          <input
            v-model="getCatalogueItemDraft(activeCategory.categoryId).projectType"
            type="text"
            maxlength="100"
            class="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
            :disabled="isSaving"
          />
        </div>
        <div>
          <label class="mb-1 block text-xs font-semibold text-slate-600">
            Unit of Issue <span class="text-red-500">*</span>
          </label>
          <input
            v-model="getCatalogueItemDraft(activeCategory.categoryId).unit"
            type="text"
            maxlength="50"
            class="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
            :disabled="isSaving"
          />
        </div>
        <div>
          <label class="mb-1 block text-xs font-semibold text-slate-600">
            ABC / Unit (₱) <span class="text-red-500">*</span>
          </label>
          <input
            v-model.number="getCatalogueItemDraft(activeCategory.categoryId).unitPrice"
            type="number"
            min="0"
            step="0.01"
            class="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
            :disabled="isSaving"
          />
        </div>
        <div>
          <label class="mb-1 block text-xs font-semibold text-slate-600">
            Pre-Procurement Conference <span class="text-red-500">*</span>
          </label>
          <select
            v-model="getCatalogueItemDraft(activeCategory.categoryId).preProcurementConference"
            class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
            :disabled="isSaving"
          >
            <option :value="null" disabled>Select Yes or No</option>
            <option :value="true">Yes</option>
            <option :value="false">No</option>
          </select>
        </div>
      </div>

      <div class="mt-6 rounded-xl border border-slate-200 bg-slate-50 p-4">
        <div class="flex flex-col justify-between gap-2 sm:flex-row sm:items-center">
          <div>
            <h5 class="text-sm font-bold text-slate-800">Department Section Requirements</h5>
            <p class="mt-1 text-xs text-slate-500">
              Select sections that need this item and enter requested quantities.
            </p>
          </div>
          <div>
            <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">
              Total Quantity
            </p>
            <p class="text-lg font-black text-emerald-700">
              {{ draftTotalQuantity(getCatalogueItemDraft(activeCategory.categoryId)) }}
              {{ getCatalogueItemDraft(activeCategory.categoryId).unit }}
            </p>
          </div>
        </div>
        <div class="mt-4">
          <label class="mb-1 block text-xs font-medium text-slate-500">
            Add department section
          </label>
          <select
            class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm"
            :disabled="
              isSaving ||
              getAvailableSections(
                getCatalogueItemDraft(activeCategory.categoryId).sectionAllocations,
              ).length === 0
            "
            @change="addCatalogueDraftSection(activeCategory.categoryId, $event)"
          >
            <option value="">Select a section...</option>
            <option
              v-for="section in getAvailableSections(
                getCatalogueItemDraft(activeCategory.categoryId).sectionAllocations,
              )"
              :key="section.sectionId"
              :value="section.sectionId"
            >
              {{ section.sectionName }}
            </option>
          </select>
        </div>
        <div
          v-for="allocation in getCatalogueItemDraft(activeCategory.categoryId).sectionAllocations"
          :key="allocation.sectionId"
          class="mt-3 grid grid-cols-1 items-end gap-3 rounded-lg border border-slate-200 bg-white p-3 sm:grid-cols-[1fr_180px_auto]"
        >
          <p class="text-sm font-semibold text-slate-800">
            {{ allocation.sectionName }}
          </p>
          <div>
            <label class="mb-1 block text-xs font-medium text-slate-500">
              Requested Quantity
            </label>
            <input
              v-model.number="allocation.quantity"
              type="number"
              min="0"
              step="1"
              class="w-full rounded-lg border border-slate-300 px-3 py-2 text-center text-sm font-semibold"
              :disabled="isSaving"
            />
          </div>
          <button
            type="button"
            class="inline-flex items-center justify-center gap-1 rounded-lg border border-red-200 px-3 py-2 text-xs font-semibold text-red-600 hover:bg-red-50"
            :disabled="isSaving"
            @click="removeCatalogueDraftSection(activeCategory.categoryId, allocation.sectionId)"
          >
            <Trash2 :size="14" />
            Remove
          </button>
        </div>
      </div>

      <div class="mt-4 flex justify-end border-t border-slate-100 pt-4">
        <button
          type="button"
          :disabled="isSaving"
          class="inline-flex items-center justify-center gap-2 rounded-lg bg-emerald-600 px-5 py-2.5 text-sm font-semibold text-white shadow-sm hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-50"
          @click="addCatalogueItem(activeCategory)"
        >
          <Plus :size="16" />
          Add Catalogue Item
        </button>
      </div>
    </template>
  </div>
</template>
