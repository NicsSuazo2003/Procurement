<script setup lang="ts">
import { computed, ref } from 'vue'
import { Plus, Trash2, ArrowLeft, ArrowRight, Check } from '@lucide/vue'

import PPMPPlanningFieldsForm from './PPMPPlanningFieldsForm.vue'
import PPMPProjectTypeField from './PPMPProjectTypeField.vue'
import type { PlanningYearCategoryDto } from '../models/master-data.types'
import type { CreatePPMPViewModel } from '../viewmodels/useCreatePPMPViewModel'
import { isPPMPProjectType } from '../models/ppmp-template.defaults'

type EntryViewModel = Pick<
  CreatePPMPViewModel,
  | 'addDraftSection'
  | 'addManualItem'
  | 'draftTotalQuantity'
  | 'formatCurrency'
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
  formatCurrency,
  getAvailableSections,
  getItemDraft,
  isSaving,
  removeDraftSection,
} = props.viewModel

const { activeCategory } = props

const currentStep = ref<1 | 2>(1)

const steps = [
  { number: 1 as const, title: 'Project & Schedule', subtitle: 'Description, price & procurement plan' },
  { number: 2 as const, title: 'Sub-Unit Quantities', subtitle: 'Allocate & confirm' },
]

const currentDraft = computed(() => getItemDraft(activeCategory.categoryId))

const isStep1Valid = computed(() => {
  const d = currentDraft.value
  const monthPattern = /^\d{4}-(0[1-9]|1[0-2])$/
  const hasValidUnitPrice =
    Number.isFinite(Number(d.unitPrice)) &&
    Number(d.unitPrice) >= 0 &&
    Math.abs(Number(d.unitPrice) * 100 - Math.round(Number(d.unitPrice) * 100)) <= 0.000001

  return (
    d.generalDescriptionAndObjective.trim().length > 0 &&
    isPPMPProjectType(d.projectType) &&
    d.unit.trim().length > 0 &&
    hasValidUnitPrice &&
    d.preProcurementConference !== null &&
    d.recommendedModeOfProcurement.trim().length > 0 &&
    monthPattern.test(d.startOfProcurementActivity) &&
    monthPattern.test(d.endOfProcurementActivity) &&
    d.endOfProcurementActivity >= d.startOfProcurementActivity &&
    d.expectedDeliveryImplementationPeriod.trim().length > 0 &&
    d.sourceOfFunds.trim().length > 0
  )
})

function goToStep(step: 1 | 2): void {
  if (step === 2 && !isStep1Valid.value) return
  currentStep.value = step
}

async function handleAddManualItem(): Promise<void> {
  await addManualItem(activeCategory)
  if (currentDraft.value.generalDescriptionAndObjective.trim().length === 0) {
    currentStep.value = 1
  }
}
</script>

<template>
  <div class="flex flex-col bg-white">
    <!-- STEPPER BAR -->
    <div class="border-b border-slate-200/80 bg-slate-50/70 px-6 py-4 sm:px-7">
      <ol class="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <li
          v-for="step in steps"
          :key="step.number"
          class="flex items-center gap-3 rounded-xl border px-3.5 py-2.5 transition-all"
          :class="
            currentStep === step.number
              ? 'border-brand-600 bg-white shadow-2xs ring-1 ring-brand-600/20'
              : currentStep > step.number || isStep1Valid
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

    <!-- STEP 1: PROJECT & PLANNING DETAILS -->
    <div v-if="currentStep === 1" class="space-y-6 p-6 sm:p-7">
      <div class="flex items-center gap-3">
        <div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/20">
          <Plus :size="18" />
        </div>
        <div>
          <h4 class="text-base font-bold text-slate-900">Additional Sheet Requirement</h4>
          <p class="text-xs text-slate-500 sm:text-sm">
            Encode requirements that are not listed in Procurement's default item catalogue.
          </p>
        </div>
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
            placeholder="Describe the procurement project, its purpose, intended use, and objective."
            class="drmc-input resize-y text-base leading-relaxed"
            :disabled="isSaving"
          ></textarea>
          <p class="mt-1.5 text-xs text-slate-400">Maximum 2,000 characters.</p>
        </div>

        <div>
          <label class="mb-2 block text-sm font-semibold text-slate-700">
            Type of the Project to be Procured
            <span class="text-red-500">*</span>
          </label>
          <PPMPProjectTypeField
            v-model="currentDraft.projectType"
            :disabled="isSaving"
          />
        </div>

        <div>
          <label class="mb-2 block text-sm font-semibold text-slate-700">
            Pre-Procurement Conference
            <span class="text-red-500">*</span>
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
          <label class="mb-2 block text-sm font-semibold text-slate-700">
            Unit of Issue <span class="text-red-500">*</span>
          </label>
          <input
            v-model="currentDraft.unit"
            type="text"
            maxlength="50"
            placeholder="e.g. piece, box, ream"
            class="drmc-input"
            :disabled="isSaving"
          />
        </div>

        <div>
          <label class="mb-2 block text-sm font-semibold text-slate-700">
            ABC / Unit (₱) <span class="text-red-500">*</span>
          </label>
          <input
            v-model.number="currentDraft.unitPrice"
            type="number"
            min="0"
            step="0.01"
            class="drmc-input tabular-nums"
            :disabled="isSaving"
          />
        </div>
      </div>

      <PPMPPlanningFieldsForm
        :model-value="currentDraft"
        :disabled="isSaving"
        @update:model-value="Object.assign(currentDraft, $event)"
      />

      <!-- STEP 1 NAVIGATION -->
      <div class="flex flex-wrap items-center justify-between gap-3 border-t border-slate-200/80 pt-5">
        <p class="text-xs text-slate-500">
          Items entered here are PPMP line items and are not added to the official item catalogue.
        </p>

        <button
          type="button"
          :disabled="!isStep1Valid"
          class="drmc-btn-primary px-5 py-2.5 text-sm"
          @click="currentStep = 2"
        >
          Next: Sub-Unit Quantities
          <ArrowRight :size="16" />
        </button>
      </div>
    </div>

    <!-- STEP 2: SUB-UNIT QUANTITIES & CONFIRM -->
    <div v-else class="space-y-6 p-6 sm:p-7">
      <!-- LIVE SUMMARY STRIP -->
      <div class="grid grid-cols-1 gap-4 rounded-2xl border border-brand-200 bg-brand-50/50 p-4 sm:grid-cols-3">
        <div class="min-w-0">
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">Project</p>
          <p class="mt-1 truncate text-sm font-bold text-slate-900">
            {{ currentDraft.generalDescriptionAndObjective }}
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
          @change="addDraftSection(activeCategory.categoryId, $event)"
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
            {{ section.sectionCode }} — {{ section.sectionName }}
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
          <div class="self-center">
            <p class="text-sm font-semibold text-slate-800">
              {{ allocation.sectionName }}
            </p>
            <p class="text-xs text-slate-400 tabular-nums">Sub-Unit ID: {{ allocation.sectionId }}</p>
          </div>

          <div>
            <label class="mb-1.5 block text-xs font-semibold text-slate-500">
              Requested Quantity ({{ currentDraft.unit }})
            </label>
            <input
              v-model.number="allocation.quantity"
              type="number"
              min="0.01"
              step="0.01"
              class="drmc-input text-center text-base font-bold tabular-nums"
              :disabled="isSaving"
            />
          </div>

          <button
            type="button"
            class="inline-flex min-h-[48px] items-center justify-center gap-1.5 rounded-xl border border-red-200 bg-white px-4 py-2.5 text-xs font-semibold text-red-600 transition hover:bg-red-50 sm:text-sm"
            :disabled="isSaving"
            @click="removeDraftSection(activeCategory.categoryId, allocation.sectionId)"
          >
            <Trash2 :size="15" />
            Remove
          </button>
        </div>
      </div>

      <!-- STEP 2 NAVIGATION -->
      <div class="flex flex-wrap items-center justify-between gap-3 border-t border-slate-200/80 pt-5">
        <button
          type="button"
          :disabled="isSaving"
          class="drmc-btn-secondary px-4 py-2.5 text-sm"
          @click="currentStep = 1"
        >
          <ArrowLeft :size="16" />
          Back: Project & Schedule
        </button>

        <button
          type="button"
          :disabled="isSaving || draftTotalQuantity(currentDraft) <= 0"
          class="drmc-btn-primary px-6 py-2.5 text-sm"
          @click="handleAddManualItem"
        >
          <Plus :size="17" />
          {{ isSaving ? 'Adding...' : 'Add Item to PPMP' }}
        </button>
      </div>
    </div>
  </div>
</template>