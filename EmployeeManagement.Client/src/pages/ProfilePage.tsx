import type { FormEvent } from 'react'
import { useCallback, useEffect, useState } from 'react'
import { PageTitle } from '../components/common/PageTitle'
import type { ApiClient } from '../types/api'
import type { Employee } from '../types/domain'
import type { Toast } from '../types/toast'
import './ProfilePage.css'

type ProfilePageProps = {
  api: ApiClient
  notify: (toast: Toast) => void
  refreshSession: (token: string) => void
}

type ProfileUpdateResponse = {
  employee: Employee
  token: string
}

export function ProfilePage({ api, notify, refreshSession }: ProfilePageProps) {
  const [employee, setEmployee] = useState<Employee | null>(null)
  const [form, setForm] = useState({
    fullName: '',
    email: '',
    position: '',
    phone: '',
    password: '',
  })

  const load = useCallback(
    () =>
      api<Employee>('/api/employees/myprofile').then((nextEmployee) => {
        setEmployee(nextEmployee)
        setForm({
          fullName: nextEmployee.fullName,
          email: nextEmployee.email ?? '',
          position: nextEmployee.position ?? '',
          phone: nextEmployee.phone ?? '',
          password: '',
        })
      }),
    [api],
  )

  useEffect(() => {
    load().catch((error) => notify({ tone: 'error', message: error.message }))
  }, [load, notify])

  const submit = async (event: FormEvent) => {
    event.preventDefault()

    try {
      const response = await api<ProfileUpdateResponse>('/api/employees/myprofile', {
        method: 'PUT',
        body: JSON.stringify({
          fullName: form.fullName.trim(),
          email: form.email.trim(),
          position: form.position.trim() || null,
          phone: form.phone.trim() || null,
          ...(form.password.trim() ? { password: form.password } : {}),
        }),
      })

      setEmployee(response.employee)
      setForm((current) => ({ ...current, password: '' }))
      refreshSession(response.token)
      notify({ tone: 'success', message: 'Profile updated.' })
    } catch (error) {
      notify({
        tone: 'error',
        message: error instanceof Error ? error.message : 'Unable to update profile.',
      })
    }
  }

  return (
    <main className="page profile-page">
      <PageTitle title="My Profile" subtitle="Update your personal information and password." />
      <section className="panel profile-summary">
        <div>
          <span>Department</span>
          <strong>{employee?.departmentName ?? '...'}</strong>
        </div>
        <div>
          <span>Role</span>
          <strong>{employee?.role ?? '...'}</strong>
        </div>
      </section>
      <form className="inline-form profile-form" onSubmit={submit}>
        <label>
          Full Name
          <input value={form.fullName} onChange={(event) => setForm({ ...form, fullName: event.target.value })} required />
        </label>
        <label>
          Email
          <input type="email" value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} required />
        </label>
        <label>
          Position
          <input value={form.position} onChange={(event) => setForm({ ...form, position: event.target.value })} />
        </label>
        <label>
          Phone
          <input value={form.phone} onChange={(event) => setForm({ ...form, phone: event.target.value })} />
        </label>
        <label>
          New Password
          <input
            type="password"
            value={form.password}
            minLength={form.password ? 6 : undefined}
            placeholder="Leave blank to keep current password"
            onChange={(event) => setForm({ ...form, password: event.target.value })}
          />
        </label>
        <button className="primary-btn compact" type="submit">
          Update Profile
        </button>
      </form>
    </main>
  )
}
