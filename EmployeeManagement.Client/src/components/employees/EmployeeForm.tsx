import type { FormEvent } from 'react'
import { useState } from 'react'
import type { ApiClient } from '../../types/api'
import type { Department, Employee, Role } from '../../types/domain'
import type { Toast } from '../../types/toast'
import './EmployeeForm.css'

type EmployeeFormProps = {
  api: ApiClient
  departments: Department[]
  positionOptions: string[]
  notify: (toast: Toast) => void
  onSaved: (token?: string) => void
  editingEmployee?: Employee | null
}

type EmployeeUpdateResult = {
  token?: string
}

const emptyForm = {
  fullName: '',
  email: '',
  position: '',
  salary: '0',
  phone: '',
  departmentId: '',
  role: 'Employee' as Role,
  password: '',
}

function toForm(employee?: Employee | null) {
  if (!employee) return emptyForm

  return {
    fullName: employee.fullName,
    email: employee.email ?? '',
    position: employee.position ?? '',
    salary: String(employee.salary),
    phone: employee.phone ?? '',
    departmentId: String(employee.departmentId),
    role: employee.role,
    password: '',
  }
}

function optionalText(value: string) {
  const trimmed = value.trim()
  return trimmed === '' ? null : trimmed
}

export function EmployeeForm({
  api,
  departments,
  positionOptions,
  notify,
  onSaved,
  editingEmployee,
}: EmployeeFormProps) {
  const [form, setForm] = useState(() => toForm(editingEmployee))

  const submit = async (event: FormEvent) => {
    event.preventDefault()

    const payload = {
      fullName: form.fullName.trim(),
      email: optionalText(form.email),
      position: optionalText(form.position),
      salary: Number(form.salary),
      phone: optionalText(form.phone),
      departmentId: Number(form.departmentId),
      role: form.role,
      ...(form.password.trim() ? { password: form.password } : {}),
    }

    try {
      if (editingEmployee) {
        const result = await api<EmployeeUpdateResult | void>(`/api/employees/${editingEmployee.id}`, {
          method: 'PUT',
          body: JSON.stringify(payload),
        })
        const token = result && 'token' in result ? result.token : undefined

        onSaved(token)
      } else {
        await api<Employee>('/api/employees', {
          method: 'POST',
          body: JSON.stringify(payload),
        })

        onSaved()
      }

      notify({
        tone: 'success',
        message: editingEmployee ? 'Employee updated.' : 'Employee and login account created.',
      })
    } catch (error) {
      notify({
        tone: 'error',
        message: error instanceof Error ? error.message : 'Unable to save employee.',
      })
    }
  }

  return (
    <form className="inline-form employee-form" onSubmit={submit}>
      <p className="form-hint">
        A login account will be created automatically from the employee email. The role is assigned directly in this form.
      </p>
      <label>
        Full Name
        <input value={form.fullName} onChange={(event) => setForm({ ...form, fullName: event.target.value })} required />
      </label>
      <label>
        Login Email
        <input type="email" value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} required />
      </label>
      <label>
        Position
        <input
          list="position-options"
          value={form.position}
          onChange={(event) => setForm({ ...form, position: event.target.value })}
          placeholder="Select or type a new position"
        />
        <datalist id="position-options">
          {positionOptions.map((position) => (
            <option key={position} value={position} />
          ))}
        </datalist>
      </label>
      <label>
        Salary
        <input type="number" min="0" value={form.salary} onChange={(event) => setForm({ ...form, salary: event.target.value })} />
      </label>
      <label>
        Phone
        <input value={form.phone} onChange={(event) => setForm({ ...form, phone: event.target.value })} />
      </label>
      <label>
        Department
        <select value={form.departmentId} onChange={(event) => setForm({ ...form, departmentId: event.target.value })} required>
          <option value="">Select department</option>
          {departments.map((department) => (
            <option key={department.id} value={department.id}>
              {department.departmentName}
            </option>
          ))}
        </select>
      </label>
      <label>
        Role
        <select value={form.role} onChange={(event) => setForm({ ...form, role: event.target.value as Role })}>
          <option>Employee</option>
          <option>Manager</option>
          <option>Admin</option>
        </select>
      </label>
      <label>
        Password
        <input
          type="password"
          value={form.password}
          placeholder={editingEmployee ? 'Leave blank to keep current password' : 'Login password'}
          onChange={(event) => setForm({ ...form, password: event.target.value })}
          required={!editingEmployee}
          minLength={form.password ? 6 : undefined}
        />
      </label>
      <button className="primary-btn compact" type="submit">
        {editingEmployee ? 'Update' : 'Save'}
      </button>
    </form>
  )
}
