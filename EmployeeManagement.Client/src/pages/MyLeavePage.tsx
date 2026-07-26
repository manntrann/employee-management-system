import type { FormEvent } from 'react'
import { useCallback, useEffect, useState } from 'react'
import { PageTitle } from '../components/common/PageTitle'
import { LeaveList } from '../components/leave/LeaveList'
import { LeaveMeter } from '../components/leave/LeaveMeter'
import type { ApiClient } from '../types/api'
import type { LeaveBalance, LeaveRequest, LeaveType } from '../types/domain'
import type { Toast } from '../types/toast'
import { daysBetween } from '../utils/date'
import './MyLeavePage.css'

type MyLeavePageProps = {
  api: ApiClient
  notify: (toast: Toast) => void
}

export function MyLeavePage({ api, notify }: MyLeavePageProps) {
  const today = new Date().toISOString().slice(0, 10)
  const [balance, setBalance] = useState<LeaveBalance | null>(null)
  const [items, setItems] = useState<LeaveRequest[]>([])
  const [form, setForm] = useState({
    leaveType: 0 as LeaveType,
    startDate: '',
    endDate: '',
    reason: '',
  })

  const load = useCallback(
    () =>
      Promise.all([
        api<LeaveBalance>('/api/leave-requests/balance').catch(() => null),
        api<LeaveRequest[]>('/api/leave-requests/mine'),
      ]).then(([nextBalance, requests]) => {
        setBalance(nextBalance)
        setItems(requests)
      }),
    [api],
  )

  useEffect(() => {
    load().catch((error) => notify({ tone: 'error', message: error.message }))
  }, [load, notify])

  const requestedDays = daysBetween(form.startDate, form.endDate)
  const remaining =
    form.leaveType === 1
      ? balance?.sickRemaining
      : form.leaveType === 0
        ? balance?.annualRemaining
        : undefined

  const submit = async (event: FormEvent) => {
    event.preventDefault()

    try {
      await api<LeaveRequest>('/api/leave-requests', {
        method: 'POST',
        body: JSON.stringify(form),
      })

      setForm({ leaveType: 0, startDate: '', endDate: '', reason: '' })
      notify({ tone: 'success', message: 'Leave request submitted.' })
      load()
    } catch (error) {
      notify({
        tone: 'error',
        message: error instanceof Error ? error.message : 'Unable to submit leave request.',
      })
    }
  }

  const cancel = async (id: number) => {
    try {
      await api<void>(`/api/leave-requests/${id}/cancel`, {
        method: 'POST',
        body: JSON.stringify({}),
      })

      notify({ tone: 'success', message: 'Pending leave request cancelled.' })
      load()
    } catch (error) {
      notify({
        tone: 'error',
        message: error instanceof Error ? error.message : 'Unable to cancel leave request.',
      })
    }
  }

  const updateStartDate = (startDate: string) => {
    setForm({
      ...form,
      startDate,
      endDate: form.endDate && form.endDate < startDate ? '' : form.endDate,
    })
  }

  return (
    <main className="page">
      <PageTitle title="My Leave" subtitle="Track your balance and submit leave requests." />
      {balance && (
        <section className="metric-grid two">
          <LeaveMeter title="Annual Leave" used={balance.annualUsed} total={balance.annualAllowance} remaining={balance.annualRemaining} />
          <LeaveMeter title="Sick Leave" used={balance.sickUsed} total={balance.sickAllowance} remaining={balance.sickRemaining} />
        </section>
      )}
      <form className="inline-form my-leave-form" onSubmit={submit}>
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
        <p className={remaining !== undefined && requestedDays > remaining ? 'warning' : 'muted'}>
          {requestedDays} expected leave days
        </p>
        <button className="primary-btn compact" type="submit">
          Submit
        </button>
      </form>
      <section className="panel">
        <LeaveList items={items} onCancel={cancel} />
      </section>
    </main>
  )
}
