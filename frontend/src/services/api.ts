
const API_BASE_URL = '/api'

interface ApiRequestOptions extends RequestInit {
  authenticated?: boolean
}

function getAccessToken(): string | null {
  try {
    const storedSession = sessionStorage.getItem('drmc-auth-session')

    if (!storedSession) return null

    const session = JSON.parse(storedSession)

    return session.accessToken ?? null
  } catch {
    return null
  }
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

  if (authenticated) {
    const token = getAccessToken()

    if (token) {
      requestHeaders.set('Authorization', `Bearer ${token}`)
    }
  }

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...requestOptions,
    headers: requestHeaders,
  })

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

    if (response.status === 401) {
      window.dispatchEvent(new CustomEvent('drmc:unauthorized'))
    }

    throw new Error(message)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return await response.json() as T
}
