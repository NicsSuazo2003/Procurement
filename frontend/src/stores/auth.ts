
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

    return new Date(session.value.sessionExpiresAtUtc).getTime() > Date.now()
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

      const sessionExpiration = new Date(parsed.sessionExpiresAtUtc).getTime()

      if (
        !parsed.accessToken ||
        !parsed.refreshToken ||
        !parsed.user ||
        !parsed.sessionExpiresAtUtc ||
        !Number.isFinite(sessionExpiration) ||
        sessionExpiration <= Date.now()
      ) {
        logout(false)
        return
      }

      session.value = parsed
    } catch {
      logout(false)
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

      const sessionExpiration = Date.parse(response.sessionExpiresAtUtc)
      if (
        !response.refreshToken ||
        !Number.isFinite(sessionExpiration) ||
        sessionExpiration <= Date.now()
      ) {
        throw new Error('The server did not return a valid refresh session.')
      }

      saveSession({
        accessToken: response.accessToken,
        tokenType: response.tokenType,
        expiresAtUtc: response.expiresAtUtc,
        refreshToken: response.refreshToken,
        sessionExpiresAtUtc: response.sessionExpiresAtUtc,
        user: response.user,
      })
    } finally {
      isLoading.value = false
    }
  }

  function logout(revokeRefreshToken = true): void {
    const refreshToken = session.value?.refreshToken
    session.value = null
    sessionStorage.removeItem(STORAGE_KEY)

    if (revokeRefreshToken && refreshToken) {
      void authService.logout(refreshToken).catch(error => {
        console.error('Unable to revoke the refresh token during logout.', error)
      })
    }
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
