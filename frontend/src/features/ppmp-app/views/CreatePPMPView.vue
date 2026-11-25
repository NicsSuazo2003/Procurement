<script setup lang="ts">
import { ArrowLeft, Save, Send, AlertCircle, FileText, Printer } from '@lucide/vue'

import { useCreatePPMPViewModel } from '../viewmodels/useCreatePPMPViewModel'
import CreatePPMPItemsSection from '../components/CreatePPMPItemsSection.vue'
import CreatePPMPPrintPreview from '../components/CreatePPMPPrintPreview.vue'

const createPPMPViewModel = useCreatePPMPViewModel()

const {
  currentDepartment,
  errorMessage,
  estimatedBudget,
  form,
  formatCurrency,
  hasLoadedPreferences,
  isLoading,
  isLoadingCategories,
  isSaving,
  onPlanningYearChange,
  planningYearCategories,
  planningYears,
  preferredCategories,
  navigateToPPMPList,
  savePPMP,
  selectedItems,
  selectedPlanningYearId,
} = createPPMPViewModel

function printPPMP(): void {
  window.print()
}
</script>

<template>
  <div class="ppmp-shell mx-auto max-w-[1600px] space-y-5 pb-28">
    <!-- APPLICATION TOOLBAR -->
    <div class="print:hidden">
      <button
        type="button"
        class="mb-3 inline-flex items-center gap-1.5 text-xs font-semibold text-slate-500 transition-colors hover:text-brand-700"
        @click="navigateToPPMPList"
      >
        <ArrowLeft :size="15" />
        Back to Procurement Requests
      </button>

      <div class="flex flex-col justify-between gap-3 sm:flex-row sm:items-center">
        <div>
          <h1 class="text-2xl font-bold tracking-tight text-slate-900">
            Create Project Procurement Management Plan
          </h1>
          <p class="mt-1 text-sm text-slate-500">
            Review Procurement's default items by category, include the items your unit needs, and add unlisted requirements on the Additional Sheet.
          </p>
        </div>

        <span
          class="inline-flex items-center gap-2 self-start rounded-full bg-brand-50 px-3.5 py-1.5 text-xs font-semibold text-brand-700 ring-1 ring-inset ring-brand-600/20 tabular-nums sm:self-auto"
        >
          <span class="h-2 w-2 rounded-full bg-brand-600"></span>
          {{ selectedItems.length }} item(s) budgeted
        </span>
      </div>
    </div>

    <!-- ERROR BANNER -->
    <div
      v-if="errorMessage"
      role="alert"
      class="print:hidden flex items-start gap-3 rounded-xl border border-red-200 bg-red-50 p-4 text-sm font-medium text-red-800"
    >
      <AlertCircle :size="18" class="mt-0.5 shrink-0 text-red-600" />
      <p class="flex-1">{{ errorMessage }}</p>
    </div>

    <!-- INITIAL LOADING -->
    <div
      v-if="isLoading"
      class="print:hidden drmc-card p-14 text-center text-sm text-slate-500"
    >
      <div
        class="mb-3 inline-block h-8 w-8 animate-spin rounded-full border-4 border-brand-700 border-t-transparent"
      ></div>
      <p class="font-medium text-slate-600">Loading departments and planning years...</p>
    </div>

    <template v-else>
      <!-- PLANNING YEAR TOOLBAR -->
      <div
        class="print:hidden drmc-card sticky top-4 z-20 bg-white/95 p-4 shadow-md backdrop-blur-md"
      >
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label class="mb-1.5 block text-xs font-semibold uppercase tracking-wider text-slate-500">
              Planning Year
            </label>
            <select
              v-model.number="selectedPlanningYearId"
              class="drmc-input font-medium"
              :disabled="isLoadingCategories || isSaving"
              @change="onPlanningYearChange"
            >
              <option :value="null" disabled>Select planning year</option>
              <option
                v-for="year in planningYears"
                :key="year.planningYearId"
                :value="year.planningYearId"
              >
                Fiscal Year {{ year.year }}
              </option>
            </select>
          </div>

          <div class="flex items-end">
            <div class="w-full rounded-xl border border-slate-200/90 bg-slate-50/70 px-3.5 py-2.5">
              <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
                End-User / Implementing Unit
              </p>
              <p class="mt-0.5 text-sm font-semibold text-slate-800">
                {{ currentDepartment?.departmentName ?? form.department ?? 'Not assigned' }}
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- CATEGORY LOADING -->
      <div
        v-if="isLoadingCategories"
        class="print:hidden drmc-card p-14 text-center text-sm text-slate-500"
      >
        <div
          class="mb-2.5 inline-block h-6 w-6 animate-spin rounded-full border-2 border-brand-700 border-t-transparent"
        ></div>
        <p class="font-medium text-slate-600">Loading available categories...</p>
      </div>

      <template v-else>
        <!-- EMPTY STATES -->
        <div
          v-if="planningYearCategories.length === 0"
          class="print:hidden rounded-xl border border-amber-200 bg-amber-50/80 p-6 text-center text-sm font-medium text-amber-800"
        >
          No active categories are assigned to this planning year. Please contact the Procurement
          Office.
        </div>

        <div
          v-else-if="hasLoadedPreferences && preferredCategories.length === 0"
          class="print:hidden rounded-xl border border-amber-200 bg-amber-50/80 p-6 text-center"
        >
          <AlertCircle class="mx-auto mb-2 h-8 w-8 text-amber-600" />
          <h3 class="text-sm font-bold text-amber-900">No Categories Visible</h3>
          <p class="mt-1 text-sm text-amber-800">
            Please configure your category preferences for this planning year before creating a
            PPMP.
          </p>
        </div>

        <CreatePPMPItemsSection :view-model="createPPMPViewModel" />

        <!-- HIDDEN ON SCREEN, VISIBLE ONLY IN PRINT PREVIEW -->
        <CreatePPMPPrintPreview :view-model="createPPMPViewModel" />

        <!-- STICKY ACTION BAR -->
        <div
          class="print:hidden drmc-card sticky bottom-4 z-20 mt-6 border-slate-300/90 bg-white/95 p-4 shadow-lg backdrop-blur-md"
        >
          <div class="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div class="flex items-center gap-3">
              <div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/20">
                <FileText :size="19" />
              </div>
              <div>
                <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
                  Estimated Total Budget
                </p>
                <p class="text-lg font-extrabold text-brand-700 tabular-nums">
                  {{ formatCurrency(estimatedBudget) }}
                </p>
              </div>
            </div>

            <div class="flex flex-wrap items-center justify-end gap-2.5">
              <button
                type="button"
                :disabled="selectedItems.length === 0"
                class="drmc-btn-secondary w-full sm:w-auto"
                @click="printPPMP"
              >
                <Printer :size="16" />
                Print PPMP
              </button>

              <button
                type="button"
                :disabled="isSaving || selectedItems.length === 0"
                class="drmc-btn-secondary w-full sm:w-auto"
                @click="savePPMP(false)"
              >
                <Save :size="16" />
                Save as Draft
              </button>

              <button
                type="button"
                :disabled="isSaving || selectedItems.length === 0"
                class="drmc-btn-primary w-full sm:w-auto"
                @click="savePPMP(true)"
              >
                <Send :size="16" />
                {{ isSaving ? 'Saving...' : 'Submit for Unit Head Approval' }}
              </button>
            </div>
          </div>
        </div>
      </template>
    </template>
  </div>
</template>

<style scoped>
@media print {
  .ppmp-shell {
    max-width: none !important;
    padding: 0 !important;
    margin: 0 !important;
  }
}
</style>