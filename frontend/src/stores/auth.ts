
import { computed, ref } from 'vue'
import { defineStore } from 'pinia'

import { authService } from '@/features/auth/services/auth.service'

import type {
  AuthSession,
  AuthUser,
  LoginRequest,
} from '@/features/auth/models/auth.types'

const STORAGE_KEY = 'drmc-auth-session'

export const useAuthStore = defineStore('auth', () => {
  const session = ref<AuthSession | null>(null)
  const isLoading = ref(false)

  const user = computed<AuthUser | null>(
    () => session.value?.user ?? null,
  )

  const accessToken = computed(
    () => session.value?.accessToken ?? null,
  )

  const isAuthenticated = computed(() => {
    if (!session.value) {
      return false
    }

    return (
      new Date(session.value.expiresAtUtc).getTime() >
      Date.now()
    )
  })

  const roles = computed(
    () => session.value?.user.roleNames ?? [],
  )

  function hasRole(role: string): boolean {
    return roles.value.includes(role)
  }

  function hasAnyRole(requiredRoles: string[]): boolean {
    return requiredRoles.some(role => hasRole(role))
  }

  function saveSession(newSession: AuthSession): void {
    session.value = newSession

    sessionStorage.setItem(
      STORAGE_KEY,
      JSON.stringify(newSession),
    )
  }

  function restoreSession(): void {
    try {
      const stored = sessionStorage.getItem(STORAGE_KEY)

      if (!stored) {
        session.value = null
        return
      }

      const parsed = JSON.parse(stored) as AuthSession

      const expiration = new Date(parsed.expiresAtUtc).getTime()

      if (
        !parsed.accessToken ||
        !parsed.user ||
        !parsed.expiresAtUtc ||
        !Number.isFinite(expiration) ||
        expiration <= Date.now()
      ) {
        logout()
        return
      }

      session.value = parsed
    } catch {
      logout()
    }
  }

  async function login(credentials: LoginRequest): Promise<void> {
    isLoading.value = true

    try {
      const response = await authService.login(credentials)

      if (!response.user.isActive) {
        throw new Error('Your account is inactive.')
      }

      if (!response.accessToken) {
        throw new Error('The server did not return an access token.')
      }

      saveSession({
        accessToken: response.accessToken,
        tokenType: response.tokenType,
        expiresAtUtc: response.expiresAtUtc,
        user: response.user,
      })
    } finally {
      isLoading.value = false
    }
  }

  function logout(): void {
    session.value = null
    sessionStorage.removeItem(STORAGE_KEY)
  }

  return {
    session,
    user,
    accessToken,
    roles,
    isLoading,
    isAuthenticated,
    hasRole,
    hasAnyRole,
    saveSession,
    restoreSession,
    login,
    logout,
  }
})
