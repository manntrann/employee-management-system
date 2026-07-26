import type { Session } from '../types/domain'

const roleClaim =
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

export function decodeJwt(token: string): Session | null {
  try {
    const payload = JSON.parse(
      atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')),
    )

    return {
      token,
      email: payload.email ?? payload.sub ?? '',
      role: payload[roleClaim] ?? payload.role ?? 'Employee',
      userId: Number(payload.sub ?? 0),
    }
  } catch {
    return null
  }
}
