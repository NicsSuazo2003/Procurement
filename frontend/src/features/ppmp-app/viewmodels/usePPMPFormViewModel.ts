import { computed, ref } from 'vue'

import type { PPMPItem } from '../models/ppmp.types'

import { ppmpCalculationService } from '../services/ppmpCalculationService'

export interface PPMPFormState {
  department: string
  planningYear: number
  remarks: string
  items: PPMPItem[]
}

function createEmptyForm(): PPMPFormState {
  return {
    department: '',
    planningYear: new Date().getFullYear() + 1,
    remarks: '',
    items: [],
  }
}

export function usePPMPFormViewModel() {
  const form = ref<PPMPFormState>(createEmptyForm())
  const errors = ref<string[]>([])
  const isSaving = ref(false)

  const totalItems = computed(() =>
    ppmpCalculationService.calculateTotalItems(form.value.items),
  )

  const estimatedBudget = computed(() =>
    ppmpCalculationService.calculateEstimatedBudget(form.value.items),
  )

  function clearErrors(): void {
    errors.value = []
  }

  function addItem(item: PPMPItem): void {
    form.value.items.push({
      ...item,
      lineId: item.lineId || Date.now(),
    })

    clearErrors()
  }

  function updateItem(lineId: number, updates: Partial<PPMPItem>): void {
    const index = form.value.items.findIndex(item => item.lineId === lineId)

    if (index === -1) return

    const existing = form.value.items[index]

    if (!existing) return

    form.value.items[index] = {
      ...existing,
      ...updates,
      lineId: existing.lineId,
    }

    clearErrors()
  }

  function removeItem(lineId: number): void {
    form.value.items = form.value.items.filter(item => item.lineId !== lineId)

    clearErrors()
  }

  return {
    form,
    errors,
    isSaving,
    totalItems,
    estimatedBudget,
    addItem,
    updateItem,
    removeItem,
    clearErrors,
  }
}