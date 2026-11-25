import { beforeEach, describe, expect, it, vi } from 'vitest'

import { apiRequest } from './api'

describe('api request session refresh', () => {
  beforeEach(() => {
    sessionStorage.clear()
    vi.restoreAllMocks()
  })

  it('refreshes an expired access token and retries the original request', async () => {
    sessionStorage.setItem(
      'drmc-auth-session',
      JSON.stringify({
        accessToken: 'expired-access-token',
        tokenType: 'Bearer',
        expiresAtUtc: new Date(Date.now() - 60_000).toISOString(),
        refreshToken: 'current-refresh-token',
        sessionExpiresAtUtc: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
        user: { userId: 7 },
      }),
    )

    const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
      const url = String(input)
      const headers = new Headers(init?.headers)

      if (url.endsWith('/Auth/refresh')) {
        return new Response(
          JSON.stringify({
            accessToken: 'new-access-token',
            tokenType: 'Bearer',
            expiresAtUtc: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
            refreshToken: 'rotated-refresh-token',
            sessionExpiresAtUtc: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
            user: { userId: 7 },
          }),
          { status: 200, headers: { 'Content-Type': 'application/json' } },
        )
      }

      if (headers.get('Authorization') === 'Bearer expired-access-token') {
        return new Response(null, { status: 401 })
      }

      return new Response(JSON.stringify({ ok: true }), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      })
    })

    vi.stubGlobal('fetch', fetchMock)

    await expect(apiRequest<{ ok: boolean }>('/protected')).resolves.toEqual({ ok: true })
    expect(fetchMock).toHaveBeenCalledTimes(3)

    const session = JSON.parse(sessionStorage.getItem('drmc-auth-session') ?? '{}')
    expect(session.accessToken).toBe('new-access-token')
    expect(session.refreshToken).toBe('rotated-refresh-token')
  })
})
