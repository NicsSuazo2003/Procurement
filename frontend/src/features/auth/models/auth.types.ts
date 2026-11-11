
export type UserRole =
  | 'End-user'
  | 'Unit Head'
  | 'Procurement Staff'
  | 'System Administrator'

export interface AuthUser {
  userId: number
  departmentId: number | null
  departmentName: string | null
  employeeNumber: string
  fullName: string
  email: string
  isActive: boolean
  roleNames: string[]
}

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  accessToken: string
  tokenType: string
  expiresAtUtc: string
  user: AuthUser
}

export interface AuthSession {
  accessToken: string
  tokenType: string
  expiresAtUtc: string
  user: AuthUser
}
