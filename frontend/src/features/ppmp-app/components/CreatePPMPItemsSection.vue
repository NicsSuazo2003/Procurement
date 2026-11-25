<script setup lang="ts">
import { ref, watch } from 'vue'
import { X } from '@lucide/vue'
import CreatePPMPCatalogueEntryForm from './CreatePPMPCatalogueEntryForm.vue'
import CreatePPMPManualEntryForm from './CreatePPMPManualEntryForm.vue'
import CreatePPMPItemList from './CreatePPMPItemList.vue'

import type { CreatePPMPViewModel } from '../viewmodels/useCreatePPMPViewModel'

type ItemsViewModel = Pick<
  CreatePPMPViewModel,
  | 'activeCategories'
  | 'activeCategory'
  | 'activeCategoryId'
  | 'addDraftSection'
  | 'addItemSection'
  | 'addManualItem'
  | 'allocationTotal'
  | 'cancelEditingItem'
  | 'categoryTotal'
  | 'addCatalogueDraftSection'
  | 'addCatalogueItem'
  | 'catalogueItems'
  | 'catalogueItemsCategoryId'
  | 'catalogueSearch'
  | 'draftTotalQuantity'
  | 'editingLineId'
  | 'expandedAllocationRows'
  | 'formatCurrency'
  | 'getAvailableSections'
  | 'getDescription'
  | 'getItemDraft'
  | 'getItemEntryLabel'
  | 'getItemsForCategory'
  | 'getCatalogueItemDraft'
  | 'getProjectType'
  | 'filteredCatalogueItems'
  | 'isLoadingCatalogueItems'
  | 'isAllocationBalanced'
  | 'isSaving'
  | 'removeDraftSection'
  | 'removeItem'
  | 'removeItemSection'
  | 'selectCategoryTab'
  | 'selectedItemsInCategory'
  | 'itemEntryMode'
  | 'removeCatalogueDraftSection'
  | 'selectCatalogueItem'
  | 'setItemEntryMode'
  | 'startEditingItem'
  | 'toggleAllocationRow'
  | 'updateItemField'
  | 'updateItemPlanningFields'
  | 'updateQuantityFromAllocations'
>

const props = defineProps<{ viewModel: ItemsViewModel }>()
const {
  activeCategories,
  activeCategory,
  activeCategoryId,
  categoryTotal,
  catalogueItemsCategoryId,
  itemEntryMode,
  formatCurrency,
  getItemsForCategory,
  selectCategoryTab,
  setItemEntryMode,
  selectedItemsInCategory,
} = props.viewModel

const isAddModalOpen = ref(false)

function openAddModal(mode: 'catalogue' | 'manual'): void {
  setItemEntryMode(mode)
  isAddModalOpen.value = true

  if (
    mode === 'catalogue' &&
    activeCategory.value &&
    !props.viewModel.isLoadingCatalogueItems &&
    catalogueItemsCategoryId.value !== activeCategory.value.categoryId
  ) {
    void selectCategoryTab(activeCategory.value.categoryId)
  }
}

function closeAddModal(): void {
  isAddModalOpen.value = false
}

watch(
  () =>
    activeCategory.value
      ? getItemsForCategory(activeCategory.value.categoryId).length
      : 0,
  (newCount, oldCount) => {
    if (isAddModalOpen.value && newCount > oldCount) {
      isAddModalOpen.value = false
    }
  },
)
</script>

<template>
  <!-- SELECT CATEGORY -->
  <div
    v-if="activeCategories.length > 0"
    class="print:hidden drmc-card space-y-3 p-4 sm:p-5"
  >
    <div class="flex flex-wrap items-center justify-between gap-2">
      <label class="text-xs font-semibold uppercase tracking-wider text-slate-500">
        Select Category
      </label>
      <span class="text-xs text-slate-400 tabular-nums">
        {{ activeCategories.length }} available categor{{ activeCategories.length === 1 ? 'y' : 'ies' }}
      </span>
    </div>

    <div class="flex flex-wrap items-center gap-2.5">
      <button
        v-for="category in activeCategories"
        :key="category.categoryId"
        type="button"
        class="inline-flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-semibold transition-all"
        :class="
          activeCategoryId === category.categoryId
            ? 'bg-brand-700 text-white shadow-xs'
            : 'bg-slate-100/80 text-slate-700 hover:bg-slate-200/70'
        "
        @click="selectCategoryTab(category.categoryId)"
      >
        <span>{{ category.categoryName }}</span>
        <span
          class="rounded-full px-2 py-0.5 text-xs font-bold tabular-nums"
          :class="
            activeCategoryId === category.categoryId
              ? 'bg-white/20 text-white'
              : 'bg-slate-200/80 text-slate-600'
          "
        >
          {{ selectedItemsInCategory(category.categoryId) }}
        </span>
      </button>
    </div>
  </div>

  <!-- CATEGORY ADDED ITEMS LIST -->
  <section
    v-if="activeCategory"
    class="print:hidden drmc-card overflow-hidden"
  >
    <div
      class="flex flex-col gap-3 border-b border-slate-200/80 bg-slate-50/70 px-6 py-5 sm:flex-row sm:items-center sm:justify-between"
    >
      <div class="min-w-0">
        <h3 class="text-lg font-bold tracking-tight text-slate-900 sm:text-xl">
          {{ activeCategory.categoryName }}
        </h3>
        <p class="mt-1 text-sm text-slate-500 tabular-nums">
          {{ getItemsForCategory(activeCategory.categoryId).length }} item(s) added ·
          <span class="font-semibold text-brand-700">
            {{ selectedItemsInCategory(activeCategory.categoryId) }} with quantity
          </span>
        </p>
      </div>
    </div>

    <CreatePPMPItemList
      :view-model="props.viewModel"
      :active-category="activeCategory"
      @open-add-modal="openAddModal('catalogue')"
    />

    <!-- CATEGORY TOTAL -->
    <div class="flex flex-wrap items-center justify-between gap-2 border-t border-slate-200/80 bg-slate-50/90 px-6 py-4">
      <span class="text-sm font-bold uppercase tracking-wider text-slate-600">
        Category Total
      </span>
      <span class="text-xl font-extrabold text-brand-700 tabular-nums">
        {{ formatCurrency(categoryTotal(activeCategory.categoryId)) }}
      </span>
    </div>
  </section>

  <!-- ADD ITEM MODAL -->
  <Teleport to="body">
    <div
      v-if="isAddModalOpen && activeCategory"
      class="print:hidden fixed inset-0 z-50 flex items-center justify-center overflow-y-auto bg-slate-900/55 p-4 backdrop-blur-2xs"
      @click.self="closeAddModal"
    >
      <div
        role="dialog"
        aria-modal="true"
        :aria-label="`Add item to ${activeCategory.categoryName}`"
        class="my-auto flex max-h-[92vh] w-full max-w-5xl flex-col overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-2xl"
      >
        <!-- MODAL HEADER -->
        <div class="flex items-start justify-between gap-4 border-b border-slate-200/80 bg-slate-50/80 px-6 py-5 sm:px-7">
          <div>
            <span class="inline-flex items-center rounded-full bg-brand-50 px-3 py-1 text-xs font-bold uppercase tracking-wider text-brand-700 ring-1 ring-inset ring-brand-600/20">
              {{ activeCategory.categoryName }}
            </span>
            <h4 class="mt-2 text-xl font-bold tracking-tight text-slate-900">
              Add Procurement Item
            </h4>
            <p class="mt-1 text-sm text-slate-500">
              Select from Procurement's default catalogue items or encode an unlisted item on the Additional Sheet.
            </p>
          </div>

          <button
            type="button"
            class="rounded-xl p-2 text-slate-400 transition hover:bg-slate-200/70 hover:text-slate-700"
            title="Close modal"
            @click="closeAddModal"
          >
            <X :size="22" />
          </button>
        </div>

        <!-- MODE SWITCHER -->
        <div class="flex flex-wrap gap-2.5 border-b border-slate-200/80 bg-white px-6 py-3.5 sm:px-7">
          <button
            type="button"
            class="rounded-xl px-4 py-2.5 text-sm font-semibold transition"
            :class="
              itemEntryMode === 'catalogue'
                ? 'bg-brand-700 text-white shadow-2xs'
                : 'bg-slate-100 text-slate-700 hover:bg-slate-200/80'
            "
            @click="setItemEntryMode('catalogue')"
          >
            Procurement default items
          </button>
          <button
            type="button"
            class="rounded-xl px-4 py-2.5 text-sm font-semibold transition"
            :class="
              itemEntryMode === 'manual'
                ? 'bg-brand-700 text-white shadow-2xs'
                : 'bg-slate-100 text-slate-700 hover:bg-slate-200/80'
            "
            @click="setItemEntryMode('manual')"
          >
            Additional Sheet
          </button>
        </div>

        <!-- SCROLLABLE FORM BODY -->
        <div class="overflow-y-auto p-1">
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
        </div>

        <!-- MODAL FOOTER -->
        <div class="flex items-center justify-end border-t border-slate-200/80 bg-slate-50/80 px-6 py-4 sm:px-7">
          <button
            type="button"
            class="drmc-btn-secondary px-5 py-2.5 text-sm"
            @click="closeAddModal"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>