
import { ref } from 'vue'
import type { ProcurementProject } from '../models/ProcurementProject'
import { getProcurementProjects } from '../services/procurementProjectService'

export function useProcurementProjectsViewModel() {
  const projects = ref<ProcurementProject[]>([])
  const isLoading = ref(false)
  const errorMessage = ref('')

  async function loadProjects() {
    isLoading.value = true
    errorMessage.value = ''

    try {
      projects.value = await getProcurementProjects()
    } catch (error) {
      errorMessage.value =
        error instanceof Error
          ? error.message
          : 'An unexpected error occurred.'
    } finally {
      isLoading.value = false
    }
  }

  return {
    projects,
    isLoading,
    errorMessage,
    loadProjects,
  }
}
