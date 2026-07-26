import type { Role, Session } from '../types/domain'

export function isPrivilegedRole(role: Role) {
  return role === 'Admin' || role === 'Manager'
}

export function canManage(session: Session) {
  return isPrivilegedRole(session.role)
}
