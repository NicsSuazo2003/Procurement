<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { ArrowLeft, RotateCcw } from '@lucide/vue'
</script>

<template>
  <div class="mx-auto max-w-4xl space-y-6">
    <div>
      <RouterLink
        to="/ppmp"
        class="mb-3 inline-flex items-center gap-1.5 text-xs font-semibold text-slate-500 transition hover:text-brand-700"
      >
        <ArrowLeft :size="15" />
        Back to PPMP List
      </RouterLink>

      <div class="flex items-start gap-3">
        <div class="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/20">
          <RotateCcw :size="21" />
        </div>
        <div>
          <h1 class="text-2xl font-bold tracking-tight text-slate-900">
            PPMP Reconsideration
          </h1>
          <p class="mt-1 text-sm text-slate-500">
            Review and manage reconsideration requests for returned PPMP submissions.
          </p>
        </div>
      </div>
    </div>

    <div class="drmc-card p-12 text-center">
      <RotateCcw :size="32" class="mx-auto mb-3 text-slate-300" />
      <h2 class="text-base font-semibold text-slate-800">No Reconsideration Requests</h2>
      <p class="mt-1 text-sm text-slate-500">
        This module is scheduled for a future phase.
      </p>
    </div>
  </div>
</template>