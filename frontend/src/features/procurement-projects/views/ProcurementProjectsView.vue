
<script setup lang="ts">
import { onMounted } from 'vue'
import { useProcurementProjectsViewModel } from '../viewmodels/useProcurementProjectsViewModel'

const {
  projects,
  isLoading,
  errorMessage,
  loadProjects,
} = useProcurementProjectsViewModel()

onMounted(() => {
  loadProjects()
})
</script>

<template>
  <main class="p-6">
    <div class="mb-6">
      <h1 class="text-2xl font-bold">Procurement Projects</h1>
      <p class="text-gray-500">
        Manage and monitor procurement projects.
      </p>
    </div>

    <div v-if="isLoading">
      Loading procurement projects...
    </div>

    <div v-else-if="errorMessage" class="text-red-600">
      {{ errorMessage }}
      <button @click="loadProjects" class="ml-2 underline">
        Retry
      </button>
    </div>

    <div v-else-if="projects.length === 0" class="text-gray-500">
      No procurement projects found.
    </div>

    <div v-else class="overflow-x-auto">
      <table class="w-full border-collapse border">
        <thead>
          <tr class="bg-gray-100">
            <th class="border p-3 text-left">Project Code</th>
            <th class="border p-3 text-left">Project Title</th>
            <th class="border p-3 text-left">End User Department</th>
            <th class="border p-3 text-left">Estimated Budget</th>
            <th class="border p-3 text-left">Status</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="project in projects" :key="project.id">
            <td class="border p-3">{{ project.projectCode }}</td>
            <td class="border p-3">{{ project.projectTitle }}</td>
            <td class="border p-3">{{ project.endUserDepartment }}</td>
            <td class="border p-3">
              {{ project.estimatedBudget.toLocaleString() }}
            </td>
            <td class="border p-3">{{ project.status }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </main>
</template>
