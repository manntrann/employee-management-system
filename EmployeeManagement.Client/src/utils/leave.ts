import type { LeaveStatus, LeaveType } from '../types/domain'

export function statusLabel(status: LeaveStatus) {
  return ['Pending', 'Approved', 'Rejected', 'Cancelled'][status] ?? 'Pending'
}

export function leaveTypeLabel(type: LeaveType) {
  return ['Annual', 'Sick', 'Unpaid'][type] ?? 'Annual'
}
