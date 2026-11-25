<script setup lang="ts">
import { Lock } from '@lucide/vue'
import type { PPMPPlanningFields } from '../models/ppmp.types'

const props = withDefaults(
  defineProps<{
    modelValue: PPMPPlanningFields
    disabled?: boolean
    /** Set to true for Admin / Procurement Staff configuration or review views */
    allowOverride?: boolean
  }>(),
  {
    disabled: false,
    allowOverride: false,
  },
)

const emit = defineEmits<{
  'update:modelValue': [value: PPMPPlanningFields]
}>()

function updateField<K extends keyof PPMPPlanningFields>(
  field: K,
  value: PPMPPlanningFields[K],
): void {
  if (!props.allowOverride) return
  emit('update:modelValue', { ...props.modelValue, [field]: value })
}
</script>

<template>
  <div class="rounded-2xl border border-slate-200/90 bg-slate-50/70 p-5">
    <div class="mb-4 flex flex-wrap items-center justify-between gap-2">
      <div>
        <h5 class="text-base font-bold text-slate-900">Procurement Planning Details</h5>
        <p v-if="!allowOverride" class="mt-0.5 text-xs text-slate-500">
          Pre-configured schedule and funding details per category by the Procurement Office.
        </p>
      </div>

      <span
        v-if="!allowOverride"
        class="inline-flex items-center gap-1.5 rounded-full bg-slate-200/70 px-3 py-1 text-xs font-semibold text-slate-700"
      >
        <Lock :size="13" />
        Fixed by Procurement
      </span>
    </div>

    <div class="grid grid-cols-1 gap-5 md:grid-cols-2">
      <div>
        <label class="mb-2 block text-sm font-semibold text-slate-700">
          Recommended Mode of Procurement <span class="text-red-500">*</span>
        </label>
        <input
          :value="modelValue.recommendedModeOfProcurement"
          type="text"
          :list="allowOverride ? 'ppmp-procurement-modes' : undefined"
          maxlength="150"
          class="drmc-input"
          :readonly="!allowOverride"
          :disabled="disabled || !allowOverride"
          required
          @input="
            updateField(
              'recommendedModeOfProcurement',
              ($event.target as HTMLInputElement).value,
            )
          "
        />
        <datalist v-if="allowOverride" id="ppmp-procurement-modes">
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
        <label class="mb-2 block text-sm font-semibold text-slate-700">
          Source of Funds <span class="text-red-500">*</span>
        </label>
        <input
          :value="modelValue.sourceOfFunds"
          type="text"
          maxlength="100"
          class="drmc-input"
          :readonly="!allowOverride"
          :disabled="disabled || !allowOverride"
          required
          @input="updateField('sourceOfFunds', ($event.target as HTMLInputElement).value)"
        />
      </div>

      <div>
        <label class="mb-2 block text-sm font-semibold text-slate-700">
          Start of Procurement Activity <span class="text-red-500">*</span>
        </label>
        <input
          :value="modelValue.startOfProcurementActivity"
          type="month"
          class="drmc-input"
          :readonly="!allowOverride"
          :disabled="disabled || !allowOverride"
          required
          @input="
            updateField('startOfProcurementActivity', ($event.target as HTMLInputElement).value)
          "
        />
      </div>

      <div>
        <label class="mb-2 block text-sm font-semibold text-slate-700">
          End of Procurement Activity <span class="text-red-500">*</span>
        </label>
        <input
          :value="modelValue.endOfProcurementActivity"
          type="month"
          class="drmc-input"
          :readonly="!allowOverride"
          :disabled="disabled || !allowOverride"
          required
          @input="
            updateField('endOfProcurementActivity', ($event.target as HTMLInputElement).value)
          "
        />
      </div>

      <div>
        <label class="mb-2 block text-sm font-semibold text-slate-700">
          Expected Delivery / Implementation Period <span class="text-red-500">*</span>
        </label>
        <input
          :value="modelValue.expectedDeliveryImplementationPeriod"
          type="text"
          maxlength="100"
          class="drmc-input"
          :readonly="!allowOverride"
          :disabled="disabled || !allowOverride"
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
        <label class="mb-2 block text-sm font-semibold text-slate-700">
          Attached Supporting Documents
        </label>
        <input
          :value="modelValue.attachedSupportingDocuments"
          type="text"
          maxlength="500"
          placeholder="None"
          class="drmc-input"
          :readonly="!allowOverride"
          :disabled="disabled || !allowOverride"
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