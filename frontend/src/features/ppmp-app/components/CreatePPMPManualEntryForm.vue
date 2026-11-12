<script setup lang="ts">
import { Plus, Trash2 } from '@lucide/vue'

import type { PlanningYearCategoryDto } from '../models/master-data.types'
import type { CreatePPMPViewModel } from '../viewmodels/useCreatePPMPViewModel'

type EntryViewModel = Pick<
  CreatePPMPViewModel,
  | 'addDraftSection'
  | 'addManualItem'
  | 'draftTotalQuantity'
  | 'getAvailableSections'
  | 'getItemDraft'
  | 'isSaving'
  | 'removeDraftSection'
>

const props = defineProps<{
  viewModel: EntryViewModel
  activeCategory: PlanningYearCategoryDto
}>()
const {
  addDraftSection,
  addManualItem,
  draftTotalQuantity,
  getAvailableSections,
  getItemDraft,
  isSaving,
  removeDraftSection,
} = props.viewModel
const { activeCategory } = props
</script>

<template>
  <div class="border-b border-slate-200 bg-white p-6">
    <div class="mb-4 flex items-center gap-2">
      <Plus :size="18" class="text-emerald-600" />
      <h4 class="text-base font-bold text-slate-900">Add Procurement Project</h4>
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
        <p class="mt-1 text-xs text-slate-400">Maximum 2,000 characters.</p>
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
          <h5 class="text-sm font-bold text-slate-800">Department Section Requirements</h5>
          <p class="mt-1 text-xs text-slate-500">
            Select each section that needs this procurement project and enter its requested
            quantity.
          </p>
        </div>

        <div class="text-left sm:text-right">
          <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Total Quantity</p>
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
          :disabled="
            isSaving ||
            getAvailableSections(getItemDraft(activeCategory.categoryId).sectionAllocations)
              .length === 0
          "
          @change="addDraftSection(activeCategory.categoryId, $event)"
        >
          <option value="">
            {{
              getAvailableSections(getItemDraft(activeCategory.categoryId).sectionAllocations)
                .length
                ? 'Select a department section'
                : 'All available sections have been added'
            }}
          </option>
          <option
            v-for="section in getAvailableSections(
              getItemDraft(activeCategory.categoryId).sectionAllocations,
            )"
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
        <p class="text-sm font-medium text-slate-600">No department sections selected yet.</p>
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
            <p class="text-xs text-slate-500">Section ID: {{ allocation.sectionId }}</p>
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
        <span class="text-sm font-semibold text-slate-700"> Total requested quantity </span>
        <span class="text-lg font-black text-emerald-700">
          {{ draftTotalQuantity(getItemDraft(activeCategory.categoryId)) }}
          {{ getItemDraft(activeCategory.categoryId).unit }}
        </span>
      </div>
    </div>

    <div
      class="mt-4 flex flex-col justify-between gap-3 border-t border-slate-100 pt-4 sm:flex-row sm:items-center"
    >
      <p class="text-xs text-slate-500">
        Items entered here are PPMP line items. They are not automatically added to the official
        item catalogue.
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
</template>
