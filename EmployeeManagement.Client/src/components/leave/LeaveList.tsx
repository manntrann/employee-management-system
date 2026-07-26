import type { LeaveRequest } from '../../types/domain'
import { daysBetween, formatDate } from '../../utils/date'
import { leaveTypeLabel, statusLabel } from '../../utils/leave'
import { EmptyState } from '../common/EmptyState'
import './LeaveList.css'

type LeaveListProps = {
  items: LeaveRequest[]
  onCancel?: (id: number) => void
  onApprove?: (id: number) => void
  onReject?: (id: number) => void
}

export function LeaveList({ items, onCancel, onApprove, onReject }: LeaveListProps) {
  if (items.length === 0) {
    return <EmptyState text="No leave requests yet." />
  }

  return (
    <div className="leave-list">
      {items.map((item) => (
        <article className="leave-item" key={item.id}>
          <div>
            <strong>{item.employeeName || `Employee #${item.employeeId}`}</strong>
            <p>
              {leaveTypeLabel(item.leaveType)} | {formatDate(item.startDate)} -{' '}
              {formatDate(item.endDate)} | {daysBetween(item.startDate, item.endDate)} days
            </p>
            {item.reason && <p className="muted">{item.reason}</p>}
            {item.status !== 0 && item.reviewedByUserName && (
              <p className="muted">
                {statusLabel(item.status)} by {item.reviewedByUserName}
                {item.reviewedAt ? ` on ${formatDate(item.reviewedAt)}` : ''}
              </p>
            )}
            {item.status === 2 && item.reviewNote && (
              <p className="reject-note">Reject reason: {item.reviewNote}</p>
            )}
          </div>
          <div className="leave-actions">
            <span className={`status s-${item.status}`}>{statusLabel(item.status)}</span>
            {onCancel && item.status === 0 && (
              <button className="ghost-btn" onClick={() => onCancel(item.id)} type="button">
                Cancel
              </button>
            )}
            {onApprove && item.status === 0 && (
              <button className="secondary-btn" onClick={() => onApprove(item.id)} type="button">
                Approve
              </button>
            )}
            {onReject && item.status === 0 && (
              <button className="danger-btn" onClick={() => onReject(item.id)} type="button">
                Reject
              </button>
            )}
          </div>
        </article>
      ))}
    </div>
  )
}
