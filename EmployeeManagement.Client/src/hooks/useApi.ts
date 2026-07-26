import { useCallback } from 'react'
import { API_BASE_URL } from '../config/apiConfig'
import type { ApiClient } from '../types/api'
import type { Session } from '../types/domain'

export function useApi(session: Session | null, signOut: () => void): ApiClient {
  return useCallback(
    async <T,>(path: string, options: RequestInit = {}): Promise<T> => {
      const response = await fetch(`${API_BASE_URL}${path}`, {
        ...options,
        headers: {
          'Content-Type': 'application/json',
          ...(session?.token ? { Authorization: `Bearer ${session.token}` } : {}),
          ...options.headers,
        },
      })

      if (response.status === 401) {
        signOut()
        throw new Error('Your session has expired.')
      }

      if (!response.ok) {
        const body = await response.json().catch(() => null)
        throw new Error(body?.message ?? 'Unable to process the request.')
      }

      if (response.status === 204) return undefined as T

      return response.json()
    },
    [session, signOut],
  )
}
