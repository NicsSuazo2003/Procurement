<script setup lang="ts">
import CreatePPMPCatalogueEntryForm from './CreatePPMPCatalogueEntryForm.vue'
import CreatePPMPManualEntryForm from './CreatePPMPManualEntryForm.vue'
import CreatePPMPItemList from './CreatePPMPItemList.vue'

import type { CreatePPMPViewModel } from '../viewmodels/useCreatePPMPViewModel'

type ItemsViewModel = Pick<
  CreatePPMPViewModel,
  | 'activeCategories'
  | 'activeCategory'
  | 'activeCategoryId'
  | 'addCatalogueDraftSection'
  | 'addCatalogueItem'
  | 'addDraftSection'
  | 'addItemSection'
  | 'addManualItem'
  | 'allocationTotal'
  | 'cancelEditingItem'
  | 'catalogueItems'
  | 'catalogueSearch'
  | 'categoryTotal'
  | 'draftTotalQuantity'
  | 'editingLineId'
  | 'expandedAllocationRows'
  | 'filteredCatalogueItems'
  | 'formatCurrency'
  | 'getAvailableSections'
  | 'getCatalogueItemDraft'
  | 'getDescription'
  | 'getItemDraft'
  | 'getItemEntryLabel'
  | 'getItemsForCategory'
  | 'getProjectType'
  | 'isAllocationBalanced'
  | 'isLoadingCatalogueItems'
  | 'isSaving'
  | 'itemEntryMode'
  | 'removeCatalogueDraftSection'
  | 'removeDraftSection'
  | 'removeItem'
  | 'removeItemSection'
  | 'selectCatalogueItem'
  | 'selectCategoryTab'
  | 'selectedItemsInCategory'
  | 'startEditingItem'
  | 'toggleAllocationRow'
  | 'updateItemField'
  | 'updateQuantityFromAllocations'
>

const props = defineProps<{ viewModel: ItemsViewModel }>()
const {
  activeCategories,
  activeCategory,
  activeCategoryId,
  categoryTotal,
  formatCurrency,
  getItemsForCategory,
  isSaving,
  itemEntryMode,
  selectCategoryTab,
  selectedItemsInCategory,
} = props.viewModel
</script>

<template>
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
      <span class="ml-1 opacity-75"> ({{ getItemsForCategory(category.categoryId).length }}) </span>
    </button>
  </div>

  <!-- CATEGORY ITEM ENTRY -->
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
          {{ getItemsForCategory(activeCategory.categoryId).length }} item(s) ·
          <span class="font-semibold text-emerald-700">
            {{ selectedItemsInCategory(activeCategory.categoryId) }} with quantity
          </span>
        </p>
      </div>

      <div class="flex rounded-lg border border-slate-200 bg-white p-1">
        <button
          type="button"
          class="rounded-md px-3 py-2 text-xs font-semibold transition-colors"
          :class="
            itemEntryMode === 'catalogue'
              ? 'bg-emerald-600 text-white'
              : 'text-slate-600 hover:bg-slate-100'
          "
          :disabled="isSaving"
          @click="itemEntryMode = 'catalogue'"
        >
          Select from Catalogue
        </button>
        <button
          type="button"
          class="rounded-md px-3 py-2 text-xs font-semibold transition-colors"
          :class="
            itemEntryMode === 'manual'
              ? 'bg-emerald-600 text-white'
              : 'text-slate-600 hover:bg-slate-100'
          "
          :disabled="isSaving"
          @click="itemEntryMode = 'manual'"
        >
          Manual Entry
        </button>
      </div>
    </div>

    <CreatePPMPCatalogueEntryForm
      v-if="itemEntryMode === 'catalogue'"
      :view-model="props.viewModel"
      :active-category="activeCategory"
    />

    <CreatePPMPManualEntryForm
      v-else
      :view-model="props.viewModel"
      :active-category="activeCategory"
    />

    <CreatePPMPItemList :view-model="props.viewModel" :active-category="activeCategory" />

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
</template>
