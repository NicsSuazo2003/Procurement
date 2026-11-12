<script setup lang="ts">
import { ArrowLeft, Save, Send, AlertCircle, FileText } from '@lucide/vue'

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
</script>

<template>
  <div class="ppmp-shell mx-auto max-w-[1600px] space-y-4 pb-24">
    <!-- APPLICATION TOOLBAR -->
    <div class="print:hidden">
      <button
        type="button"
        class="mb-3 inline-flex items-center gap-2 text-sm font-medium text-slate-500 transition-colors hover:text-emerald-700"
        @click="navigateToPPMPList"
      >
        <ArrowLeft :size="17" />
        Back to PPMP Management
      </button>

      <div class="flex flex-col justify-between gap-3 sm:flex-row sm:items-center">
        <div>
          <h1 class="text-2xl font-bold tracking-tight text-slate-900">
            Create Project Procurement Management Plan
          </h1>
          <p class="mt-1 text-sm text-slate-500">
            Encode proposed procurement projects and allocate each item's requested quantity to your
            department sections.
          </p>
        </div>

        <span
          class="inline-flex items-center gap-1.5 self-start rounded-full border border-emerald-200 bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700 sm:self-auto"
        >
          <span class="h-2 w-2 rounded-full bg-emerald-500"></span>
          {{ selectedItems.length }} item(s) budgeted
        </span>
      </div>
    </div>

    <!-- ERROR BANNER -->
    <div
      v-if="errorMessage"
      class="print:hidden flex items-start gap-3 rounded-xl border border-red-200 bg-red-50 p-4 text-sm text-red-800"
    >
      <AlertCircle :size="18" class="mt-0.5 shrink-0 text-red-600" />
      <p class="flex-1">{{ errorMessage }}</p>
    </div>

    <!-- INITIAL LOADING -->
    <div
      v-if="isLoading"
      class="print:hidden rounded-xl border border-slate-200 bg-white p-12 text-center text-sm text-slate-500 shadow-sm"
    >
      <div
        class="mb-3 inline-block h-8 w-8 animate-spin rounded-full border-4 border-emerald-600 border-t-transparent"
      ></div>
      <p>Loading departments and planning years...</p>
    </div>

    <template v-else>
      <!-- PLANNING YEAR TOOLBAR -->
      <div
        class="print:hidden sticky top-4 z-20 rounded-xl border border-slate-200 bg-white/95 p-4 shadow-md backdrop-blur-md"
      >
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label class="mb-1 block text-xs font-semibold uppercase tracking-wider text-slate-600">
              Planning Year
            </label>
            <select
              v-model.number="selectedPlanningYearId"
              class="w-full rounded-lg border border-slate-300 bg-slate-50 px-3 py-2.5 text-sm font-medium text-slate-800 outline-none transition-all focus:border-emerald-600 focus:bg-white focus:ring-1 focus:ring-emerald-200"
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
            <div class="w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5">
              <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
                Department
              </p>
              <p class="mt-1 text-sm font-semibold text-slate-800">
                {{ currentDepartment?.departmentName ?? form.department ?? 'Not assigned' }}
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- CATEGORY LOADING -->
      <div
        v-if="isLoadingCategories"
        class="print:hidden rounded-xl border border-slate-200 bg-white p-12 text-center text-sm text-slate-500 shadow-sm"
      >
        <div
          class="mb-2 inline-block h-6 w-6 animate-spin rounded-full border-2 border-emerald-600 border-t-transparent"
        ></div>
        <p>Loading your preferred categories...</p>
      </div>

      <template v-else>
        <!-- EMPTY STATES -->
        <div
          v-if="planningYearCategories.length === 0"
          class="print:hidden rounded-lg border border-amber-200 bg-amber-50 p-6 text-center text-sm text-amber-800"
        >
          No active categories are assigned to this planning year. Please contact the Procurement
          Office.
        </div>

        <div
          v-else-if="hasLoadedPreferences && preferredCategories.length === 0"
          class="print:hidden rounded-lg border border-amber-200 bg-amber-50 p-6 text-center"
        >
          <AlertCircle class="mx-auto mb-2 h-8 w-8 text-amber-600" />
          <h3 class="text-sm font-bold text-amber-900">No Preferred Categories Selected</h3>
          <p class="mt-1 text-sm text-amber-800">
            Please configure your category preferences for this planning year before creating a
            PPMP.
          </p>
        </div>

        <CreatePPMPItemsSection :view-model="createPPMPViewModel" />

        <CreatePPMPPrintPreview :view-model="createPPMPViewModel" />

        <!-- ACTION BAR -->
        <div
          class="print:hidden sticky bottom-4 z-20 mt-6 rounded-xl border border-slate-300 bg-white/95 p-4 shadow-lg backdrop-blur-md"
        >
          <div class="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
            <div class="flex items-center gap-3">
              <FileText :size="20" class="shrink-0 text-emerald-600" />
              <div>
                <p class="text-[10px] font-bold uppercase tracking-wider text-slate-500">
                  Estimated Total Budget
                </p>
                <p class="text-lg font-black text-emerald-700">
                  {{ formatCurrency(estimatedBudget) }}
                </p>
              </div>
            </div>

            <div class="flex items-center justify-end gap-2">
              <button
                type="button"
                :disabled="isSaving || selectedItems.length === 0"
                class="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 shadow-sm transition-colors hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
                @click="savePPMP(false)"
              >
                <Save :size="16" />
                Save as Draft
              </button>

              <button
                type="button"
                :disabled="isSaving || selectedItems.length === 0"
                class="inline-flex items-center gap-2 rounded-lg bg-emerald-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-50"
                @click="savePPMP(true)"
              >
                <Send :size="16" />
                {{ isSaving ? 'Saving...' : 'Submit for Approval' }}
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
