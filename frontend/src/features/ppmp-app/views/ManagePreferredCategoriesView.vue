<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { Save, RefreshCw, CheckCircle2, AlertCircle, SlidersHorizontal } from '@lucide/vue'

import type {
  PlanningYearDto,
  PlanningYearCategoryDto,
} from '../models/master-data.types'

import { masterDataApiService } from '../services/masterDataApiService'
import { userCategoryPreferenceApiService } from '../services/userCategoryPreferenceApiService'

const planningYears = ref<PlanningYearDto[]>([])
const categories = ref<PlanningYearCategoryDto[]>([])

const selectedPlanningYearId = ref<number | null>(null)
const selectedCategoryIds = ref<number[]>([])
const savedCategoryIds = ref<number[]>([])

const isLoadingYears = ref(false)
const isLoadingCategories = ref(false)
const isSaving = ref(false)

const pageError = ref('')
const successMessage = ref('')

const hasChanges = computed(() => {
  const selected = [...selectedCategoryIds.value].sort((a, b) => a - b)
  const saved = [...savedCategoryIds.value].sort((a, b) => a - b)

  return (
    selected.length !== saved.length ||
    selected.some((id, index) => id !== saved[index])
  )
})

const activeCategories = computed(() =>
  categories.value.filter(category => category.isActive),
)

const selectedCount = computed(() => selectedCategoryIds.value.length)

function selectAll() {
  selectedCategoryIds.value = activeCategories.value.map(
    category => category.planningYearCategoryId,
  )
}

function clearAll() {
  selectedCategoryIds.value = []
}

async function loadPlanningYears() {
  isLoadingYears.value = true
  pageError.value = ''

  try {
    planningYears.value = await masterDataApiService.getPlanningYears()

    const firstPlanningYear = planningYears.value.at(0)

    if (firstPlanningYear) {
      selectedPlanningYearId.value = firstPlanningYear.planningYearId

      await loadPreferencesForYear(
        firstPlanningYear.planningYearId,
      )
    }
  } catch (error) {
    pageError.value =
      error instanceof Error
        ? error.message
        : 'Unable to load planning years.'
  } finally {
    isLoadingYears.value = false
  }
}

async function loadPreferencesForYear(planningYearId: number) {
  isLoadingCategories.value = true
  pageError.value = ''
  successMessage.value = ''

  categories.value = []
  selectedCategoryIds.value = []
  savedCategoryIds.value = []

  try {
    const [yearCategories, preferences] = await Promise.all([
      masterDataApiService.getPlanningYearCategories(planningYearId),
      userCategoryPreferenceApiService.getPreferences(planningYearId),
    ])

    categories.value = yearCategories

    const activeIds = new Set(
      yearCategories
        .filter(category => category.isActive)
        .map(category => category.planningYearCategoryId),
    )

    const savedIds = preferences
      .map(preference => preference.planningYearCategoryId)
      .filter(id => activeIds.has(id))

    selectedCategoryIds.value =
      preferences.length === 0 ? [...activeIds] : [...savedIds]
    savedCategoryIds.value = [...savedIds]
  } catch (error) {
    pageError.value =
      error instanceof Error
        ? error.message
        : 'Unable to load category preferences.'
  } finally {
    isLoadingCategories.value = false
  }
}

async function handlePlanningYearChange(event: Event) {
  const value = (event.target as HTMLSelectElement).value
  const planningYearId = Number(value)

  if (!planningYearId) {
    selectedPlanningYearId.value = null
    categories.value = []
    selectedCategoryIds.value = []
    savedCategoryIds.value = []
    return
  }

  selectedPlanningYearId.value = planningYearId
  await loadPreferencesForYear(planningYearId)
}

async function savePreferences() {
  if (!selectedPlanningYearId.value) return

  if (selectedCategoryIds.value.length === 0) {
    pageError.value = 'Keep at least one category visible in your PPMP.'
    successMessage.value = ''
    return
  }

  isSaving.value = true
  pageError.value = ''
  successMessage.value = ''

  try {
    const savedPreferences =
      await userCategoryPreferenceApiService.savePreferences({
        planningYearId: selectedPlanningYearId.value,
        planningYearCategoryIds: selectedCategoryIds.value,
      })

    const savedIds = savedPreferences.map(
      preference => preference.planningYearCategoryId,
    )

    savedCategoryIds.value = [...savedIds]
    selectedCategoryIds.value = [...savedIds]

    successMessage.value = 'Your visible categories have been saved.'
  } catch (error) {
    pageError.value =
      error instanceof Error
        ? error.message
        : 'Unable to save category preferences.'
  } finally {
    isSaving.value = false
  }
}

onMounted(loadPlanningYears)
</script>

<template>
  <div class="mx-auto max-w-4xl space-y-6">
    <!-- Page heading -->
    <div class="flex items-start gap-3">
      <div class="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/20">
        <SlidersHorizontal :size="21" />
      </div>

      <div>
        <h1 class="text-2xl font-bold tracking-tight text-slate-900">
          Show or Hide PPMP Categories
        </h1>
        <p class="mt-1 text-sm text-slate-500">
          All active Procurement categories are shown by default. Uncheck categories your unit does not need to hide them from PPMP preparation.
        </p>
      </div>
    </div>

    <!-- Planning year selector -->
    <div class="drmc-card p-5">
      <label
        for="planningYear"
        class="mb-2 block text-xs font-semibold uppercase tracking-wider text-slate-500"
      >
        Planning Year
      </label>

      <select
        id="planningYear"
        :value="selectedPlanningYearId ?? ''"
        :disabled="isLoadingYears || isLoadingCategories"
        class="drmc-input md:max-w-sm"
        @change="handlePlanningYearChange"
      >
        <option value="" disabled>
          Select a planning year
        </option>

        <option
          v-for="year in planningYears"
          :key="year.planningYearId"
          :value="year.planningYearId"
        >
          FY {{ year.year }} — {{ year.status }}
        </option>
      </select>
    </div>

    <!-- Error message -->
    <div
      v-if="pageError"
      role="alert"
      class="flex items-start gap-2.5 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-800"
    >
      <AlertCircle class="mt-0.5 h-5 w-5 shrink-0 text-red-600" />
      <p>{{ pageError }}</p>
    </div>

    <!-- Success message -->
    <div
      v-if="successMessage"
      role="status"
      class="flex items-center gap-2.5 rounded-xl border border-brand-200 bg-brand-50 px-4 py-3 text-sm font-medium text-brand-800"
    >
      <CheckCircle2 class="h-5 w-5 shrink-0 text-brand-600" />
      <p>{{ successMessage }}</p>
    </div>

    <!-- Category selection -->
    <div class="drmc-card overflow-hidden">
      <div class="flex flex-wrap items-center justify-between gap-3 border-b border-slate-200/80 p-5">
        <div>
          <h2 class="text-base font-semibold text-slate-900">
            Available Categories
          </h2>
          <p class="mt-0.5 text-xs text-slate-500">
            Categories checked here will appear when you prepare a PPMP.
          </p>
        </div>

        <div class="flex items-center gap-3">
          <div v-if="activeCategories.length > 0 && !isLoadingCategories" class="flex items-center gap-2 text-xs">
            <button
              type="button"
              :disabled="isSaving"
              class="font-semibold text-brand-700 hover:underline disabled:opacity-50"
              @click="selectAll"
            >
              Select all
            </button>
            <span class="text-slate-300">|</span>
            <button
              type="button"
              :disabled="isSaving"
              class="font-semibold text-slate-500 hover:text-slate-700 hover:underline disabled:opacity-50"
              @click="clearAll"
            >
              Clear
            </button>
          </div>

          <span class="rounded-full bg-brand-50 px-3 py-1 text-xs font-semibold text-brand-700 ring-1 ring-inset ring-brand-600/20 tabular-nums">
            {{ selectedCount }} selected
          </span>
        </div>
      </div>

      <div class="p-5">
        <div
          v-if="isLoadingYears || isLoadingCategories"
          class="py-12 text-center text-sm text-slate-500"
        >
          <RefreshCw class="mx-auto mb-3 h-6 w-6 animate-spin text-brand-700" />
          Loading categories...
        </div>

        <div
          v-else-if="!selectedPlanningYearId"
          class="py-12 text-center text-sm text-slate-500"
        >
          No planning year available.
        </div>

        <div
          v-else-if="activeCategories.length === 0"
          class="py-12 text-center text-sm text-slate-500"
        >
          No active categories are assigned to this planning year.
        </div>

        <div v-else class="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label
            v-for="category in activeCategories"
            :key="category.planningYearCategoryId"
            :class="[
              'flex cursor-pointer items-center gap-3.5 rounded-xl border p-4 transition-all',
              selectedCategoryIds.includes(category.planningYearCategoryId)
                ? 'border-brand-500 bg-brand-50/40 ring-1 ring-brand-500/30'
                : 'border-slate-200 bg-white hover:border-slate-300 hover:bg-slate-50/70',
            ]"
          >
            <input
              v-model="selectedCategoryIds"
              type="checkbox"
              :value="category.planningYearCategoryId"
              :disabled="isSaving"
              class="h-4 w-4 rounded border-slate-300 text-brand-700 focus:ring-brand-500"
            />

            <div class="min-w-0 flex-1">
              <p class="text-sm font-semibold text-slate-900">
                {{ category.categoryName }}
              </p>
              <p class="mt-0.5 font-mono text-xs text-slate-500">
                {{ category.categoryCode }}
              </p>
            </div>
          </label>
        </div>
      </div>

      <!-- Save button -->
      <div class="flex flex-col justify-between gap-3 border-t border-slate-200/80 bg-slate-50/70 px-5 py-4 sm:flex-row sm:items-center">
        <p class="text-xs text-slate-500">
          Your visible categories are saved to your account for this planning year.
        </p>

        <button
          type="button"
          :disabled="
            !selectedPlanningYearId ||
            isLoadingCategories ||
            isSaving ||
            !hasChanges
          "
          class="drmc-btn-primary"
          @click="savePreferences"
        >
          <Save class="h-4 w-4" />
          {{ isSaving ? 'Saving...' : 'Save Preferences' }}
        </button>
      </div>
    </div>
  </div>
</template>