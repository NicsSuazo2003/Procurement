
import { apiRequest } from '@/services/api'

import type {
  LoginRequest,
  LoginResponse,
} from '../models/auth.types'

export const authService = {
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    return apiRequest<LoginResponse>('/Auth/login', {
      method: 'POST',
      authenticated: false,
      body: JSON.stringify(credentials),
    })
  },
}
