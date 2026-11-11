
<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { Save, RefreshCw, CheckCircle2, AlertCircle } from '@lucide/vue'

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

    selectedCategoryIds.value = [...savedIds]
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

    successMessage.value = 'Your preferred categories have been saved.'
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
  <div class="space-y-6">
    <!-- Page heading -->
    <div>
      <h1 class="text-2xl font-bold text-gray-900">
        Manage Preferred Categories
      </h1>

      <p class="mt-1 text-sm text-gray-500">
        Select the procurement categories you want to work with
        when preparing your PPMP.
      </p>
    </div>

    <!-- Planning year selector -->
    <div class="rounded-xl border border-gray-200 bg-white p-5">
      <label
        for="planningYear"
        class="mb-2 block text-sm font-semibold text-gray-700"
      >
        Planning Year
      </label>

      <select
        id="planningYear"
        :value="selectedPlanningYearId ?? ''"
        :disabled="isLoadingYears || isLoadingCategories"
        @change="handlePlanningYearChange"
        class="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-100 md:max-w-sm"
      >
        <option value="" disabled>
          Select a planning year
        </option>

        <option
          v-for="year in planningYears"
          :key="year.planningYearId"
          :value="year.planningYearId"
        >
          {{ year.year }} — {{ year.status }}
        </option>
      </select>
    </div>

    <!-- Error message -->
    <div
      v-if="pageError"
      class="flex items-start gap-2 rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-700"
    >
      <AlertCircle class="mt-0.5 h-5 w-5 shrink-0" />
      <p>{{ pageError }}</p>
    </div>

    <!-- Success message -->
    <div
      v-if="successMessage"
      class="flex items-center gap-2 rounded-lg border border-green-200 bg-green-50 p-4 text-sm text-green-700"
    >
      <CheckCircle2 class="h-5 w-5 shrink-0" />
      <p>{{ successMessage }}</p>
    </div>

    <!-- Category selection -->
    <div class="rounded-xl border border-gray-200 bg-white">
      <div
        class="flex flex-col justify-between gap-2 border-b border-gray-200 p-5 sm:flex-row sm:items-center"
      >
        <div>
          <h2 class="font-semibold text-gray-900">
            Available Categories
          </h2>

          <p class="mt-1 text-sm text-gray-500">
            Choose the categories that should appear in your PPMP.
          </p>
        </div>

        <span class="text-sm font-medium text-gray-600">
          {{ selectedCount }} selected
        </span>
      </div>

      <div class="p-5">
        <div
          v-if="isLoadingYears || isLoadingCategories"
          class="py-10 text-center text-sm text-gray-500"
        >
          <RefreshCw class="mx-auto mb-3 h-6 w-6 animate-spin" />
          Loading categories...
        </div>

        <div
          v-else-if="!selectedPlanningYearId"
          class="py-10 text-center text-sm text-gray-500"
        >
          No planning year available.
        </div>

        <div
          v-else-if="activeCategories.length === 0"
          class="py-10 text-center text-sm text-gray-500"
        >
          No active categories are assigned to this planning year.
        </div>

        <div v-else class="space-y-3">
          <label
            v-for="category in activeCategories"
            :key="category.planningYearCategoryId"
            class="flex cursor-pointer items-center gap-3 rounded-lg border border-gray-200 p-4 transition hover:border-blue-300 hover:bg-blue-50/40"
          >
            <input
              v-model="selectedCategoryIds"
              type="checkbox"
              :value="category.planningYearCategoryId"
              :disabled="isSaving"
              class="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
            />

            <div class="min-w-0 flex-1">
              <p class="text-sm font-medium text-gray-900">
                {{ category.categoryName }}
              </p>

              <p class="mt-0.5 text-xs text-gray-500">
                {{ category.categoryCode }}
              </p>
            </div>

            <span
              v-if="selectedCategoryIds.includes(category.planningYearCategoryId)"
              class="text-xs font-medium text-blue-600"
            >
              Selected
            </span>
          </label>
        </div>
      </div>

      <!-- Save button -->
      <div
        class="flex flex-col justify-between gap-3 border-t border-gray-200 bg-gray-50 p-5 sm:flex-row sm:items-center"
      >
        <p class="text-xs text-gray-500">
          Your preferences are saved to your account for this planning year.
        </p>

        <button
          type="button"
          @click="savePreferences"
          :disabled="
            !selectedPlanningYearId ||
            isLoadingCategories ||
            isSaving ||
            !hasChanges
          "
          class="inline-flex items-center justify-center gap-2 rounded-lg bg-blue-600 px-5 py-2.5 text-sm font-semibold text-white transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
        >
          <Save class="h-4 w-4" />
          {{ isSaving ? 'Saving...' : 'Save Preferences' }}
        </button>
      </div>
    </div>
  </div>
</template>
