
import type { AuthSession, LoginResponse } from '@/features/auth/models/auth.types'

const API_BASE_URL = '/api'
const SESSION_STORAGE_KEY = 'drmc-auth-session'

interface ApiRequestOptions extends RequestInit {
  authenticated?: boolean
}

function getStoredSession(): AuthSession | null {
  try {
    const storedSession = sessionStorage.getItem(SESSION_STORAGE_KEY)

    if (!storedSession) return null

    return JSON.parse(storedSession) as AuthSession
  } catch {
    return null
  }
}

function dispatchUnauthorized(): void {
  window.dispatchEvent(new CustomEvent('drmc:unauthorized'))
}

let refreshRequest: Promise<string | null> | null = null

async function refreshAccessToken(): Promise<string | null> {
  const session = getStoredSession()
  const sessionExpiry = Date.parse(session?.sessionExpiresAtUtc ?? '')

  if (
    !session?.refreshToken ||
    !Number.isFinite(sessionExpiry) ||
    sessionExpiry <= Date.now()
  ) {
    return null
  }

  if (!refreshRequest) {
    refreshRequest = (async () => {
      const response = await fetch(`${API_BASE_URL}/Auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: session.refreshToken }),
      })

      if (response.status === 401) return null
      if (!response.ok) {
        throw new Error(`Unable to refresh the session (status ${response.status}).`)
      }

      const refreshedSession = await response.json() as LoginResponse
      const currentSession = getStoredSession()
      if (!currentSession) return null

      sessionStorage.setItem(
        SESSION_STORAGE_KEY,
        JSON.stringify({
          ...currentSession,
          accessToken: refreshedSession.accessToken,
          tokenType: refreshedSession.tokenType,
          expiresAtUtc: refreshedSession.expiresAtUtc,
          refreshToken: refreshedSession.refreshToken,
          sessionExpiresAtUtc: refreshedSession.sessionExpiresAtUtc,
          user: refreshedSession.user,
        } satisfies AuthSession),
      )

      return refreshedSession.accessToken
    })().finally(() => {
      refreshRequest = null
    })
  }

  return refreshRequest
}

function createRequestHeaders(headers: HeadersInit | undefined, token: string | null): Headers {
  const requestHeaders = new Headers(headers)
  if (token) {
    requestHeaders.set('Authorization', `Bearer ${token}`)
  }
  return requestHeaders
}

export async function apiRequest<T>(
  endpoint: string,
  options: ApiRequestOptions = {},
): Promise<T> {
  const {
    authenticated = true,
    headers,
    ...requestOptions
  } = options

  const requestHeaders = new Headers(headers)

  if (requestOptions.body && !requestHeaders.has('Content-Type')) {
    requestHeaders.set('Content-Type', 'application/json')
  }

  const session = authenticated ? getStoredSession() : null
  let response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...requestOptions,
    headers: createRequestHeaders(requestHeaders, session?.accessToken ?? null),
  })

  if (response.status === 401 && authenticated) {
    const refreshedAccessToken = await refreshAccessToken()
    if (refreshedAccessToken) {
      response = await fetch(`${API_BASE_URL}${endpoint}`, {
        ...requestOptions,
        headers: createRequestHeaders(requestHeaders, refreshedAccessToken),
      })
    }

    if (response.status === 401) {
      dispatchUnauthorized()
    }
  }

  if (!response.ok) {
    let message = `Request failed with status ${response.status}.`

    try {
      const errorData = await response.json()

      message =
        errorData.message ??
        errorData.title ??
        errorData.detail ??
        message
    } catch {
      // Keep default error message.
    }

    throw new Error(message)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return await response.json() as T
}
