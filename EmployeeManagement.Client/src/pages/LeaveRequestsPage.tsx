import type { FormEvent } from 'react'
import { useCallback, useEffect, useState } from 'react'
import { Forbidden } from '../components/common/Forbidden'
import { PageTitle } from '../components/common/PageTitle'
import { LeaveList } from '../components/leave/LeaveList'
import type { ApiClient } from '../types/api'
import type { Employee, LeaveRequest, LeaveStatus, LeaveType, PaginatedResponse, Session } from '../types/domain'
import type { Toast } from '../types/toast'
import { daysBetween } from '../utils/date'
import { statusLabel } from '../utils/leave'
import { canManage } from '../utils/roles'
import './LeaveRequestsPage.css'

type LeaveRequestsPageProps = {
  api: ApiClient
  session: Session
  notify: (toast: Toast) => void
}

export function LeaveRequestsPage({ api, session, notify }: LeaveRequestsPageProps) {
  const today = new Date().toISOString().slice(0, 10)
  const [items, setItems] = useState<LeaveRequest[]>([])
  const [employees, setEmployees] = useState<Employee[]>([])
  const [filter, setFilter] = useState<'all' | LeaveStatus>(0)
  const [rejectingId, setRejectingId] = useState<number | null>(null)
  const [note, setNote] = useState('')
  const [form, setForm] = useState({
    employeeId: '',
    leaveType: 0 as LeaveType,
    startDate: '',
    endDate: '',
    reason: '',
  })
  const allowManage = canManage(session)

  const load = useCallback(() => api<LeaveRequest[]>('/api/leave-requests').then(setItems), [api])

  useEffect(() => {
    if (!allowManage) return

    Promise.all([
      load(),
      api<PaginatedResponse<Employee>>('/api/employees?page=1&pageSize=100').then((page) => setEmployees(page.data)),
    ]).catch((error) => notify({ tone: 'error', message: error.message }))
  }, [allowManage, api, load, notify])

  if (!allowManage) return <Forbidden />

  const updateStartDate = (startDate: string) => {
    setForm({
      ...form,
      startDate,
      endDate: form.endDate && form.endDate < startDate ? '' : form.endDate,
    })
  }

  const createForEmployee = async (event: FormEvent) => {
    event.preventDefault()

    try {
      await api<LeaveRequest>('/api/leave-requests/for-employee', {
        method: 'POST',
        body: JSON.stringify({
          ...form,
          employeeId: Number(form.employeeId),
        }),
      })

      setForm({ employeeId: '', leaveType: 0, startDate: '', endDate: '', reason: '' })
      notify({ tone: 'success', message: 'Employee leave request created.' })
      load()
    } catch (error) {
      notify({
        tone: 'error',
        message: error instanceof Error ? error.message : 'Unable to create leave request.',
      })
    }
  }

  const approve = async (id: number) => {
    try {
      await api<void>(`/api/leave-requests/${id}/approve`, {
        method: 'POST',
        body: JSON.stringify({ note: null }),
      })

      notify({ tone: 'success', message: 'Leave request approved.' })
      load()
    } catch (error) {
      notify({
        tone: 'error',
        message: error instanceof Error ? error.message : 'Unable to approve leave request.',
      })
    }
  }

  const openReject = (id: number) => {
    setRejectingId(id)
    setNote('')
  }

  const reject = async () => {
    if (!rejectingId) return

    if (!note.trim()) {
      notify({ tone: 'error', message: 'Please enter a rejection reason.' })
      return
    }

    try {
      await api<void>(`/api/leave-requests/${rejectingId}/reject`, {
        method: 'POST',
        body: JSON.stringify({ note: note.trim() }),
      })

      setRejectingId(null)
      setNote('')
      notify({ tone: 'success', message: 'Leave request rejected.' })
      load()
    } catch (error) {
      notify({
        tone: 'error',
        message: error instanceof Error ? error.message : 'Unable to reject leave request.',
      })
    }
  }

  const cancel = async (id: number) => {
    try {
      await api<void>(`/api/leave-requests/${id}/cancel`, {
        method: 'POST',
        body: JSON.stringify({}),
      })

      notify({ tone: 'success', message: 'Leave request cancelled.' })
      load()
    } catch (error) {
      notify({
        tone: 'error',
        message: error instanceof Error ? error.message : 'Unable to cancel leave request.',
      })
    }
  }

  const visible = filter === 'all' ? items : items.filter((item) => item.status === filter)
  const rejectingItem = items.find((item) => item.id === rejectingId)
  const requestedDays = daysBetween(form.startDate, form.endDate)

  return (
    <main className="page">
      <PageTitle title="Leave Review" subtitle="Create, cancel, approve, and reject employee leave requests." />
      <form className="inline-form leave-request-form" onSubmit={createForEmployee}>
        <label>
          Employee
          <select value={form.employeeId} onChange={(event) => setForm({ ...form, employeeId: event.target.value })} required>
            <option value="">Select employee</option>
            {employees.map((employee) => (
              <option key={employee.id} value={employee.id}>
                {employee.fullName} - {employee.departmentName}
              </option>
            ))}
          </select>
        </label>
        <label>
          Leave Type
          <select value={form.leaveType} onChange={(event) => setForm({ ...form, leaveType: Number(event.target.value) as LeaveType })}>
            <option value={0}>Annual Leave</option>
            <option value={1}>Sick Leave</option>
            <option value={2}>Unpaid Leave</option>
          </select>
        </label>
        <label>
          Start Date
          <input type="date" min={today} value={form.startDate} onChange={(event) => updateStartDate(event.target.value)} required />
        </label>
        <label>
          End Date
          <input
            type="date"
            min={form.startDate || today}
            value={form.endDate}
            onChange={(event) => setForm({ ...form, endDate: event.target.value })}
            required
          />
        </label>
        <label>
          Reason
          <input value={form.reason} onChange={(event) => setForm({ ...form, reason: event.target.value })} />
        </label>
        <p className="muted">{requestedDays} expected leave days</p>
        <button className="primary-btn compact" type="submit">
          Create Request
        </button>
      </form>
      <div className="tabs">
        {(['all', 0, 1, 2, 3] as const).map((item) => (
          <button
            className={filter === item ? 'active' : ''}
            onClick={() => {
              setFilter(item)
              setRejectingId(null)
              setNote('')
            }}
            type="button"
            key={String(item)}
          >
            {item === 'all' ? 'All' : statusLabel(item)}
          </button>
        ))}
      </div>
      {rejectingId && (
        <section className="reject-panel">
          <label className="note-box">
            Reject Reason {rejectingItem ? `- ${rejectingItem.employeeName || `Employee #${rejectingItem.employeeId}`}` : ''}
            <input value={note} onChange={(event) => setNote(event.target.value)} autoFocus />
          </label>
          <div className="row-actions">
            <button className="danger-btn" onClick={reject} type="button">
              Confirm Reject
            </button>
            <button className="ghost-btn" onClick={() => setRejectingId(null)} type="button">
              Cancel
            </button>
          </div>
        </section>
      )}
      <section className="panel">
        <LeaveList items={visible} onCancel={cancel} onApprove={approve} onReject={openReject} />
      </section>
    </main>
  )
}
