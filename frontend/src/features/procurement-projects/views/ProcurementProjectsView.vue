<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { FolderKanban, RefreshCw, Search } from '@lucide/vue'
import { useProcurementProjectsViewModel } from '../viewmodels/useProcurementProjectsViewModel'
import { formatPHP, getPPMPStatusStyle } from '@/shared/utilities/ppmpUiHelpers'

const {
  projects,
  isLoading,
  errorMessage,
  loadProjects,
} = useProcurementProjectsViewModel()

const searchQuery = ref('')

const filteredProjects = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()
  if (!query) return projects.value

  return projects.value.filter(
    project =>
      project.projectCode.toLowerCase().includes(query) ||
      project.projectTitle.toLowerCase().includes(query) ||
      project.endUserDepartment.toLowerCase().includes(query),
  )
})

onMounted(() => {
  loadProjects()
})
</script>

<template>
  <div class="mx-auto max-w-7xl space-y-6">
    <!-- PAGE HEADER -->
    <div class="flex flex-wrap items-start justify-between gap-4">
      <div class="flex items-start gap-3">
        <div class="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/20">
          <FolderKanban :size="22" />
        </div>

        <div>
          <h1 class="text-2xl font-bold tracking-tight text-slate-900">
            Procurement Projects
          </h1>
          <p class="mt-1 text-sm text-slate-500">
            Manage and monitor active procurement projects across hospital units.
          </p>
        </div>
      </div>

      <button
        type="button"
        :disabled="isLoading"
        class="drmc-btn-secondary"
        @click="loadProjects"
      >
        <RefreshCw :size="16" :class="{ 'animate-spin': isLoading }" />
        Refresh
      </button>
    </div>

    <!-- ERROR -->
    <div
      v-if="errorMessage"
      role="alert"
      class="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-800"
    >
      <span>{{ errorMessage }}</span>
      <button
        type="button"
        class="rounded-lg bg-red-100 px-3 py-1 text-xs font-semibold text-red-800 hover:bg-red-200"
        @click="loadProjects"
      >
        Retry
      </button>
    </div>

    <!-- TABLE CARD -->
    <section class="drmc-card overflow-hidden">
      <div class="flex flex-col justify-between gap-3 border-b border-slate-200/80 p-5 sm:flex-row sm:items-center">
        <div>
          <h2 class="text-base font-semibold text-slate-900">Project Directory</h2>
          <p class="mt-0.5 text-xs text-slate-500 tabular-nums">
            Showing {{ filteredProjects.length }} of {{ projects.length }} project(s)
          </p>
        </div>

        <div class="relative w-full sm:max-w-xs">
          <Search
            :size="16"
            class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
          />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search code, title, or unit..."
            class="drmc-input pl-9"
          />
        </div>
      </div>

      <div v-if="isLoading" class="p-14 text-center">
        <RefreshCw :size="26" class="mx-auto mb-3 animate-spin text-brand-700" />
        <p class="text-sm font-medium text-slate-600">Loading procurement projects...</p>
      </div>

      <div v-else-if="filteredProjects.length === 0" class="p-14 text-center">
        <FolderKanban :size="34" class="mx-auto mb-3 text-slate-300" />
        <p class="font-semibold text-slate-800">No procurement projects found</p>
        <p class="mt-1 text-xs text-slate-500">
          {{ searchQuery ? 'Try adjusting your search query.' : 'No procurement projects have been recorded yet.' }}
        </p>
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-sm">
          <thead class="border-b border-slate-200/80 bg-slate-50/80 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
            <tr>
              <th class="whitespace-nowrap px-5 py-3.5">Project Code</th>
              <th class="px-5 py-3.5">Project Title</th>
              <th class="whitespace-nowrap px-5 py-3.5">End-User Unit</th>
              <th class="whitespace-nowrap px-5 py-3.5 text-right">Estimated Budget</th>
              <th class="whitespace-nowrap px-5 py-3.5">Status</th>
            </tr>
          </thead>

          <tbody class="divide-y divide-slate-100">
            <tr
              v-for="project in filteredProjects"
              :key="project.id"
              class="transition-colors hover:bg-slate-50/80"
            >
              <td class="whitespace-nowrap px-5 py-4 font-mono text-xs font-semibold text-brand-700">
                {{ project.projectCode }}
              </td>
              <td class="px-5 py-4 font-semibold text-slate-900">
                {{ project.projectTitle }}
              </td>
              <td class="whitespace-nowrap px-5 py-4 text-slate-600">
                {{ project.endUserDepartment }}
              </td>
              <td class="whitespace-nowrap px-5 py-4 text-right font-semibold text-slate-900 tabular-nums">
                {{ formatPHP(Number(project.estimatedBudget) || 0) }}
              </td>
              <td class="whitespace-nowrap px-5 py-4">
                <span
                  :class="[
                    'inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ring-1 ring-inset',
                    getPPMPStatusStyle(project.status).pill,
                  ]"
                >
                  <span :class="['h-1.5 w-1.5 rounded-full', getPPMPStatusStyle(project.status).dot]" />
                  {{ project.status }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </div>
</template>