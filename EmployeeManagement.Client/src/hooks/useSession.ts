import { useCallback, useState } from 'react'
import { TOKEN_KEY } from '../config/apiConfig'
import type { Session } from '../types/domain'
import { decodeJwt } from '../utils/auth'

export function useSession() {
  const [session, setSession] = useState<Session | null>(() => {
    const token = localStorage.getItem(TOKEN_KEY) ?? sessionStorage.getItem(TOKEN_KEY)

    return token ? decodeJwt(token) : null
  })

  const saveSession = useCallback((token: string, remember: boolean) => {
    const decoded = decodeJwt(token)

    if (!decoded) throw new Error('Token is invalid.')

    if (remember) localStorage.setItem(TOKEN_KEY, token)
    else sessionStorage.setItem(TOKEN_KEY, token)

    setSession(decoded)
  }, [])

  const refreshSession = useCallback((token: string) => {
    const remember = localStorage.getItem(TOKEN_KEY) !== null
    saveSession(token, remember)
  }, [saveSession])

  const signOut = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY)
    sessionStorage.removeItem(TOKEN_KEY)
    setSession(null)
  }, [])

  return { session, saveSession, refreshSession, signOut }
}
