
import type { ProcurementProject } from '../models/ProcurementProject'

const API_BASE_URL = 'https://localhost:7001/api'

export async function getProcurementProjects(): Promise<ProcurementProject[]> {
  const response = await fetch(`${API_BASE_URL}/procurement-projects`)

  if (!response.ok) {
    throw new Error('Failed to retrieve procurement projects.')
  }

  return response.json() as Promise<ProcurementProject[]>
}
