<script setup lang="ts">
import type { PPMPPlanningFields } from '../models/ppmp.types'

const props = withDefaults(
  defineProps<{
    modelValue: PPMPPlanningFields
    disabled?: boolean
  }>(),
  { disabled: false },
)

const emit = defineEmits<{
  'update:modelValue': [value: PPMPPlanningFields]
}>()

function updateField<K extends keyof PPMPPlanningFields>(
  field: K,
  value: PPMPPlanningFields[K],
): void {
  emit('update:modelValue', { ...props.modelValue, [field]: value })
}
</script>

<template>
  <div class="mt-4 rounded-lg border border-slate-200 bg-slate-50 p-4">
    <h5 class="mb-3 text-sm font-bold text-slate-800">Procurement Planning Details</h5>

    <div class="grid grid-cols-1 gap-3 md:grid-cols-2 xl:grid-cols-3">
      <div>
        <label class="mb-1 block text-xs font-semibold text-slate-600">
          Recommended Mode of Procurement <span class="text-red-500">*</span>
        </label>
        <input
          :value="modelValue.recommendedModeOfProcurement"
          type="text"
          list="ppmp-procurement-modes"
          maxlength="150"
          placeholder="Choose or enter a procurement mode"
          class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
          :disabled="disabled"
          required
          @input="
            updateField(
              'recommendedModeOfProcurement',
              ($event.target as HTMLInputElement).value,
            )
          "
        />
        <datalist id="ppmp-procurement-modes">
          <option value="Public Bidding" />
          <option value="Small Value Procurement" />
          <option value="Direct Acquisition" />
          <option value="Direct Contracting" />
          <option value="Agency-to-Agency" />
          <option value="Negotiated Procurement - Sagip Saka" />
          <option value="Negotiated Procurement - Direct Retail Purchase of POL" />
          <option value="Negotiated Procurement - Lease of Real Property and Venue" />
        </datalist>
      </div>

      <div>
        <label class="mb-1 block text-xs font-semibold text-slate-600">
          Start of Procurement Activity <span class="text-red-500">*</span>
        </label>
        <input
          :value="modelValue.startOfProcurementActivity"
          type="month"
          class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
          :disabled="disabled"
          required
          @input="
            updateField('startOfProcurementActivity', ($event.target as HTMLInputElement).value)
          "
        />
      </div>

      <div>
        <label class="mb-1 block text-xs font-semibold text-slate-600">
          End of Procurement Activity <span class="text-red-500">*</span>
        </label>
        <input
          :value="modelValue.endOfProcurementActivity"
          type="month"
          class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
          :disabled="disabled"
          required
          @input="
            updateField('endOfProcurementActivity', ($event.target as HTMLInputElement).value)
          "
        />
      </div>

      <div>
        <label class="mb-1 block text-xs font-semibold text-slate-600">
          Expected Delivery / Implementation Period <span class="text-red-500">*</span>
        </label>
        <input
          :value="modelValue.expectedDeliveryImplementationPeriod"
          type="text"
          maxlength="100"
          placeholder="e.g. April 2027 or within 30 days"
          class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
          :disabled="disabled"
          required
          @input="
            updateField(
              'expectedDeliveryImplementationPeriod',
              ($event.target as HTMLInputElement).value,
            )
          "
        />
      </div>

      <div>
        <label class="mb-1 block text-xs font-semibold text-slate-600">
          Source of Funds <span class="text-red-500">*</span>
        </label>
        <input
          :value="modelValue.sourceOfFunds"
          type="text"
          maxlength="100"
          placeholder="e.g. GAA"
          class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
          :disabled="disabled"
          required
          @input="updateField('sourceOfFunds', ($event.target as HTMLInputElement).value)"
        />
      </div>

      <div>
        <label class="mb-1 block text-xs font-semibold text-slate-600">
          Attached Supporting Documents
        </label>
        <input
          :value="modelValue.attachedSupportingDocuments"
          type="text"
          maxlength="500"
          placeholder="e.g. Purchase Request, Market Scoping"
          class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
          :disabled="disabled"
          @input="
            updateField(
              'attachedSupportingDocuments',
              ($event.target as HTMLInputElement).value,
            )
          "
        />
      </div>
    </div>
  </div>
</template>
