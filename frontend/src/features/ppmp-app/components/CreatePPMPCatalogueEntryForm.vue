<script setup lang="ts">
import { computed, ref } from 'vue'
import {
  Search,
  Plus,
  Trash2,
  CheckCircle2,
  ArrowLeft,
  ArrowRight,
  Check,
  Lock,
} from '@lucide/vue'

import PPMPPlanningFieldsForm from './PPMPPlanningFieldsForm.vue'
import PPMPProjectTypeField from './PPMPProjectTypeField.vue'
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

type CatalogueEntryItem = Parameters<EntryViewModel['selectCatalogueItem']>[1]

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

const currentStep = ref<1 | 2 | 3>(1)

const steps = [
  { number: 1 as const, title: 'Select Item', subtitle: 'Choose from catalogue' },
  { number: 2 as const, title: 'Project & Schedule', subtitle: 'Details & procurement plan' },
  { number: 3 as const, title: 'Sub-Unit Quantities', subtitle: 'Allocate & confirm' },
]

const currentDraft = computed(() => getCatalogueItemDraft(activeCategory.categoryId))

const selectedCatalogueItemMeta = computed(() => {
  const selectedId = currentDraft.value.catalogueItemId
  if (selectedId === null) return null
  return catalogueItems.value.find((item) => item.itemId === selectedId) ?? null
})

const isStep2Valid = computed(() => {
  const d = currentDraft.value
  return (
    d.generalDescriptionAndObjective.trim().length > 0 &&
    d.projectType.trim().length > 0 &&
    d.unit.trim().length > 0 &&
    d.unitPrice >= 0 &&
    d.preProcurementConference !== null &&
    d.recommendedModeOfProcurement.trim().length > 0 &&
    d.startOfProcurementActivity.trim().length > 0 &&
    d.endOfProcurementActivity.trim().length > 0 &&
    d.expectedDeliveryImplementationPeriod.trim().length > 0 &&
    d.sourceOfFunds.trim().length > 0
  )
})

function handleSelectItem(catalogueItem: CatalogueEntryItem): void {
  selectCatalogueItem(activeCategory, catalogueItem)
  currentStep.value = 2
}

function goToStep(step: 1 | 2 | 3): void {
  if (step > 1 && currentDraft.value.catalogueItemId === null) return
  if (step === 3 && !isStep2Valid.value) return
  currentStep.value = step
}

async function handleAddCatalogueItem(): Promise<void> {
  await addCatalogueItem(activeCategory)
  if (currentDraft.value.catalogueItemId === null) {
    currentStep.value = 1
  }
}
</script>

<template>
  <div class="flex flex-col bg-white">
    <!-- STEPPER BAR -->
    <div class="border-b border-slate-200/80 bg-slate-50/70 px-6 py-4 sm:px-7">
      <ol class="grid grid-cols-1 gap-3 sm:grid-cols-3">
        <li
          v-for="step in steps"
          :key="step.number"
          class="flex items-center gap-3 rounded-xl border px-3.5 py-2.5 transition-all"
          :class="
            currentStep === step.number
              ? 'border-brand-600 bg-white shadow-2xs ring-1 ring-brand-600/20'
              : currentStep > step.number
                ? 'cursor-pointer border-brand-200 bg-brand-50/40 hover:bg-brand-50/70'
                : 'border-slate-200/80 bg-slate-100/60 opacity-70'
          "
          @click="goToStep(step.number)"
        >
          <span
            class="flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-xs font-bold tabular-nums"
            :class="
              currentStep === step.number
                ? 'bg-brand-700 text-white'
                : currentStep > step.number
                  ? 'bg-brand-100 text-brand-800'
                  : 'bg-slate-200 text-slate-600'
            "
          >
            <Check v-if="currentStep > step.number" :size="15" />
            <span v-else>{{ step.number }}</span>
          </span>

          <div class="min-w-0">
            <p class="truncate text-xs font-bold text-slate-900 sm:text-sm">
              Step {{ step.number }}: {{ step.title }}
            </p>
            <p class="truncate text-xs text-slate-500">
              {{ step.subtitle }}
            </p>
          </div>
        </li>
      </ol>
    </div>

    <!-- STEP 1: SELECT ITEM -->
    <div v-if="currentStep === 1" class="p-6 sm:p-7">
      <div class="relative mb-4">
        <Search
          :size="18"
          class="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400"
        />
        <input
          v-model="catalogueSearch"
          type="search"
          placeholder="Search catalogue by item code, name, type, unit, or specifications..."
          class="drmc-input pl-11"
          :disabled="isSaving || isLoadingCatalogueItems"
        />
      </div>

      <div
        v-if="isLoadingCatalogueItems"
        class="rounded-xl border border-slate-200 bg-slate-50/70 p-8 text-center text-sm text-slate-600"
      >
        Loading catalogue items...
      </div>
      <div
        v-else-if="catalogueItems.length === 0"
        class="rounded-xl border border-amber-200 bg-amber-50/80 p-5 text-sm text-amber-800"
      >
        No catalogue items are available for this planning year and category. Switch to
        <strong>Additional Sheet</strong> above to encode an unlisted item.
      </div>
      <div
        v-else-if="filteredCatalogueItems.length === 0"
        class="rounded-xl border border-slate-200 bg-slate-50/70 p-8 text-center text-sm text-slate-600"
      >
        No catalogue items match your search.
      </div>
      <div v-else class="grid max-h-[48vh] gap-3 overflow-y-auto pr-1">
        <button
          v-for="catalogueItem in filteredCatalogueItems"
          :key="catalogueItem.itemId"
          type="button"
          class="group rounded-xl border p-4 text-left transition-all"
          :class="
            currentDraft.catalogueItemId === catalogueItem.itemId
              ? 'border-brand-500 bg-brand-50/50 ring-1 ring-brand-500/30'
              : 'border-slate-200 bg-white hover:border-brand-400 hover:bg-slate-50/80'
          "
          :disabled="isSaving"
          @click="handleSelectItem(catalogueItem)"
        >
          <div class="flex items-start justify-between gap-3">
            <div class="flex items-center gap-2">
              <CheckCircle2
                v-if="currentDraft.catalogueItemId === catalogueItem.itemId"
                :size="18"
                class="shrink-0 text-brand-700"
              />
              <span class="text-base font-semibold text-slate-900 group-hover:text-brand-800">
                {{ catalogueItem.itemName }}
              </span>
            </div>
            <span class="shrink-0 rounded-lg bg-slate-100 px-2.5 py-1 font-mono text-xs font-semibold text-slate-600">
              {{ catalogueItem.itemCode }}
            </span>
          </div>

          <p class="mt-1.5 text-sm text-slate-600 tabular-nums">
            {{ catalogueItem.itemType || 'Unspecified type' }} · {{ catalogueItem.unit }} ·
            <span class="font-semibold text-brand-700">{{ formatCurrency(catalogueItem.abcPerUnit) }}</span> / unit
          </p>
          <p v-if="catalogueItem.specifications" class="mt-1.5 line-clamp-2 text-xs text-slate-500 sm:text-sm">
            {{ catalogueItem.specifications }}
          </p>
        </button>
      </div>
    </div>

    <!-- STEP 2: PROJECT & PLANNING DETAILS -->
    <div v-else-if="currentStep === 2" class="space-y-6 p-6 sm:p-7">
      <!-- SELECTED ITEM BANNER -->
      <div
        v-if="selectedCatalogueItemMeta"
        class="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-brand-200 bg-brand-50/60 px-4 py-3"
      >
        <div class="min-w-0">
          <p class="text-xs font-semibold uppercase tracking-wider text-brand-700">
            Selected Default Item · {{ selectedCatalogueItemMeta.itemCode }}
          </p>
          <p class="truncate text-sm font-bold text-slate-900 sm:text-base">
            {{ selectedCatalogueItemMeta.itemName }}
          </p>
        </div>
        <button
          type="button"
          class="text-xs font-semibold text-brand-700 underline hover:text-brand-900"
          @click="currentStep = 1"
        >
          Change Item
        </button>
      </div>

      <div class="grid grid-cols-1 gap-5 md:grid-cols-2">
        <div class="md:col-span-2">
          <label class="mb-2 block text-sm font-semibold text-slate-700">
            General Description and Objective of the Project to be Procured
            <span class="text-red-500">*</span>
          </label>
          <textarea
            v-model="currentDraft.generalDescriptionAndObjective"
            rows="3"
            maxlength="2000"
            class="drmc-input resize-y text-base leading-relaxed"
            :disabled="isSaving"
          ></textarea>
        </div>

        <div>
          <div class="mb-2 flex items-center justify-between gap-2">
            <label class="text-sm font-semibold text-slate-700">
              Type of the Project to be Procured <span class="text-red-500">*</span>
            </label>
            <span class="inline-flex items-center gap-1 text-[11px] font-medium text-slate-500">
              <Lock :size="11" /> Fixed
            </span>
          </div>
          <PPMPProjectTypeField
            v-model="currentDraft.projectType"
            :disabled="true"
          />
        </div>

        <div>
          <label class="mb-2 block text-sm font-semibold text-slate-700">
            Pre-Procurement Conference <span class="text-red-500">*</span>
          </label>
          <select
            v-model="currentDraft.preProcurementConference"
            class="drmc-input"
            :disabled="isSaving"
          >
            <option :value="null" disabled>Select Yes or No</option>
            <option :value="true">Yes</option>
            <option :value="false">No</option>
          </select>
        </div>

        <div>
          <div class="mb-2 flex items-center justify-between gap-2">
            <label class="text-sm font-semibold text-slate-700">
              Unit of Issue <span class="text-red-500">*</span>
            </label>
            <span class="inline-flex items-center gap-1 text-[11px] font-medium text-slate-500">
              <Lock :size="11" /> Fixed
            </span>
          </div>
          <input
            :value="currentDraft.unit"
            type="text"
            class="drmc-input"
            readonly
            disabled
          />
        </div>

        <div>
          <div class="mb-2 flex items-center justify-between gap-2">
            <label class="text-sm font-semibold text-slate-700">
              ABC / Unit (₱) <span class="text-red-500">*</span>
            </label>
            <span class="inline-flex items-center gap-1 text-[11px] font-medium text-slate-500">
              <Lock :size="11" /> Fixed
            </span>
          </div>
          <input
            :value="currentDraft.unitPrice"
            type="number"
            step="0.01"
            class="drmc-input tabular-nums"
            readonly
            disabled
          />
        </div>
      </div>

      <PPMPPlanningFieldsForm
        :model-value="currentDraft"
        :disabled="isSaving"
        @update:model-value="Object.assign(currentDraft, $event)"
      />

      <!-- STEP 2 NAVIGATION -->
      <div class="flex items-center justify-between border-t border-slate-200/80 pt-5">
        <button
          type="button"
          class="drmc-btn-secondary px-4 py-2.5 text-sm"
          @click="currentStep = 1"
        >
          <ArrowLeft :size="16" />
          Back: Select Item
        </button>

        <button
          type="button"
          :disabled="!isStep2Valid"
          class="drmc-btn-primary px-5 py-2.5 text-sm"
          @click="currentStep = 3"
        >
          Next: Sub-Unit Quantities
          <ArrowRight :size="16" />
        </button>
      </div>
    </div>

    <!-- STEP 3: SUB-UNIT QUANTITIES & CONFIRM -->
    <div v-else class="space-y-6 p-6 sm:p-7">
      <!-- LIVE SUMMARY STRIP -->
      <div class="grid grid-cols-1 gap-4 rounded-2xl border border-brand-200 bg-brand-50/50 p-4 sm:grid-cols-3">
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">Item</p>
          <p class="mt-1 truncate text-sm font-bold text-slate-900">
            {{ selectedCatalogueItemMeta?.itemName }}
          </p>
        </div>
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">Total Quantity</p>
          <p class="mt-1 text-lg font-extrabold text-slate-900 tabular-nums">
            {{ draftTotalQuantity(currentDraft) }} {{ currentDraft.unit }}
          </p>
        </div>
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
            Estimated Budget
          </p>
          <p class="mt-1 text-lg font-extrabold text-brand-700 tabular-nums">
            {{ formatCurrency(draftTotalQuantity(currentDraft) * currentDraft.unitPrice) }}
          </p>
        </div>
      </div>

      <div>
        <label class="mb-2 block text-sm font-semibold text-slate-700">
          Add Sub-Unit Requiring This Item <span class="text-red-500">*</span>
        </label>
        <select
          class="drmc-input bg-white"
          :disabled="
            isSaving ||
            getAvailableSections(currentDraft.sectionAllocations).length === 0
          "
          @change="addCatalogueDraftSection(activeCategory.categoryId, $event)"
        >
          <option value="">
            {{
              getAvailableSections(currentDraft.sectionAllocations).length
                ? 'Select a sub-unit to allocate quantity...'
                : 'All available sub-units have been added'
            }}
          </option>
          <option
            v-for="section in getAvailableSections(currentDraft.sectionAllocations)"
            :key="section.sectionId"
            :value="section.sectionId"
          >
            {{ section.sectionName }}
          </option>
        </select>
      </div>

      <div
        v-if="currentDraft.sectionAllocations.length === 0"
        class="rounded-2xl border border-dashed border-slate-300 bg-slate-50/60 px-4 py-8 text-center"
      >
        <p class="text-sm font-semibold text-slate-700">No sub-units selected yet.</p>
        <p class="mt-1 text-xs text-slate-500">
          Choose a sub-unit from the dropdown above and enter its requested quantity.
        </p>
      </div>

      <div v-else class="space-y-3">
        <div
          v-for="allocation in currentDraft.sectionAllocations"
          :key="allocation.sectionId"
          class="grid grid-cols-1 items-end gap-3 rounded-xl border border-slate-200 bg-white p-4 shadow-2xs sm:grid-cols-[1fr_180px_auto]"
        >
          <p class="self-center text-sm font-semibold text-slate-800">
            {{ allocation.sectionName }}
          </p>
          <div>
            <label class="mb-1.5 block text-xs font-semibold text-slate-500">
              Requested Quantity ({{ currentDraft.unit }})
            </label>
            <input
              v-model.number="allocation.quantity"
              type="number"
              min="0"
              step="1"
              class="drmc-input text-center text-base font-bold tabular-nums"
              :disabled="isSaving"
            />
          </div>
          <button
            type="button"
            class="inline-flex min-h-[48px] items-center justify-center gap-1.5 rounded-xl border border-red-200 bg-white px-4 py-2.5 text-xs font-semibold text-red-600 transition hover:bg-red-50 sm:text-sm"
            :disabled="isSaving"
            @click="removeCatalogueDraftSection(activeCategory.categoryId, allocation.sectionId)"
          >
            <Trash2 :size="15" />
            Remove
          </button>
        </div>
      </div>

      <!-- STEP 3 NAVIGATION -->
      <div class="flex flex-wrap items-center justify-between gap-3 border-t border-slate-200/80 pt-5">
        <button
          type="button"
          :disabled="isSaving"
          class="drmc-btn-secondary px-4 py-2.5 text-sm"
          @click="currentStep = 2"
        >
          <ArrowLeft :size="16" />
          Back: Project & Schedule
        </button>

        <button
          type="button"
          :disabled="isSaving || draftTotalQuantity(currentDraft) <= 0"
          class="drmc-btn-primary px-6 py-2.5 text-sm"
          @click="handleAddCatalogueItem"
        >
          <Plus :size="17" />
          {{ isSaving ? 'Adding...' : 'Add Item to PPMP' }}
        </button>
      </div>
    </div>
  </div>
</template>