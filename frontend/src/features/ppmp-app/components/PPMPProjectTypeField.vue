<script setup lang="ts">
import { PPMP_PROJECT_TYPES } from '../models/ppmp-template.defaults'

withDefaults(
  defineProps<{
    modelValue: string
    disabled?: boolean
  }>(),
  { disabled: false },
)

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()
</script>

<template>
  <select
    :value="modelValue"
    class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
    :disabled="disabled"
    required
    @change="emit('update:modelValue', ($event.target as HTMLSelectElement).value)"
  >
    <option v-for="projectType in PPMP_PROJECT_TYPES" :key="projectType" :value="projectType">
      {{ projectType }}
    </option>
  </select>
</template>
