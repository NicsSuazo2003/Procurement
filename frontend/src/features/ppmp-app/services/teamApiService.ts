import { apiRequest } from '@/services/api'
import type { TeamDto } from '../models/team.types'

export interface TeamUserDto {
  userId: number
  fullName: string
  email: string
}

export const teamApiService = {
  getMine(): Promise<TeamDto[]> {
    return apiRequest<TeamDto[]>('/Teams/mine')
  },

  getByDepartment(departmentId: number): Promise<TeamDto[]> {
    return apiRequest<TeamDto[]>(`/Teams/department/${departmentId}`)
  },

  getDepartmentUsers(departmentId: number): Promise<TeamUserDto[]> {
    return apiRequest<TeamUserDto[]>(`/Teams/department/${departmentId}/users`)
  },

  create(departmentId: number, teamCode: string, teamName: string): Promise<TeamDto> {
    return apiRequest<TeamDto>('/Teams', {
      method: 'POST',
      body: JSON.stringify({ departmentId, teamCode, teamName }),
    })
  },

  addMember(teamId: number, userId: number): Promise<void> {
    return apiRequest<void>(`/Teams/${teamId}/members/${userId}`, { method: 'POST' })
  },
}