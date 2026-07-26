import { useCallback, useEffect, useState } from 'react'
import { PageTitle } from '../components/common/PageTitle'
import { EmployeeForm } from '../components/employees/EmployeeForm'
import { EmployeeTable } from '../components/employees/EmployeeTable'
import type { ApiClient } from '../types/api'
import type { Department, Employee, PaginatedResponse, Session } from '../types/domain'
import type { Toast } from '../types/toast'
import { canManage } from '../utils/roles'
import './EmployeesPage.css'

type EmployeesPageProps = {
  api: ApiClient
  session: Session
  notify: (toast: Toast) => void
}

export function EmployeesPage({ api, session, notify }: EmployeesPageProps) {
  const [employees, setEmployees] = useState<Employee[]>([])
  const [departments, setDepartments] = useState<Department[]>([])
  const [search, setSearch] = useState('')
  const [showForm, setShowForm] = useState(false)
  const [editingEmployee, setEditingEmployee] = useState<Employee | null>(null)
  const allowManage = canManage(session)

  const load = useCallback(
    () =>
      api<PaginatedResponse<Employee>>(
        `/api/employees?page=1&pageSize=100${search ? `&search=${encodeURIComponent(search)}` : ''}`,
      ).then((employeePage) => setEmployees(employeePage.data)),
    [api, search],
  )

  useEffect(() => {
    load().catch((error) => notify({ tone: 'error', message: error.message }))
  }, [load, notify])

  useEffect(() => {
    if (!allowManage) return

    api<Department[]>('/api/departments')
      .then(setDepartments)
      .catch((error) => notify({ tone: 'error', message: error.message }))
  }, [allowManage, api, notify])

  const positionOptions = Array.from(
    new Set(employees.map((employee) => employee.position?.trim()).filter(Boolean) as string[]),
  )

  const openCreateForm = () => {
    setEditingEmployee(null)
    setShowForm(true)
  }

  const openEditForm = (employee: Employee) => {
    setEditingEmployee(employee)
    setShowForm(true)
  }

  const deleteEmployee = async (employee: Employee) => {
    if (!window.confirm(`Delete employee "${employee.fullName}"?`)) return

    try {
      await api<void>(`/api/employees/${employee.id}`, { method: 'DELETE' })
      notify({ tone: 'success', message: 'Employee deleted.' })
      load()
    } catch (error) {
      notify({
        tone: 'error',
        message: error instanceof Error ? error.message : 'Unable to delete employee.',
      })
    }
  }

  return (
    <main className="page employees-page">
      <PageTitle title="Employees" subtitle="Search profiles, contact details, departments, and roles." />
      <div className="toolbar">
        <input placeholder="Search by name..." value={search} onChange={(event) => setSearch(event.target.value)} />
        <button className="secondary-btn" onClick={load} type="button">
          Search
        </button>
        {allowManage && (
          <button className="primary-btn compact" onClick={showForm ? () => setShowForm(false) : openCreateForm} type="button">
            {showForm ? 'Close Form' : 'Add Employee'}
          </button>
        )}
      </div>
      {showForm && (
        <EmployeeForm
          key={editingEmployee?.id ?? 'new'}
          api={api}
          departments={departments}
          positionOptions={positionOptions}
          notify={notify}
          editingEmployee={editingEmployee}
          onSaved={() => {
            setEditingEmployee(null)
            setShowForm(false)
            load()
          }}
        />
      )}
      <section className="panel">
        <EmployeeTable
          employees={employees}
          canViewSalary={allowManage}
          canViewRole={allowManage}
          canManage={allowManage}
          onEdit={openEditForm}
          onDelete={deleteEmployee}
        />
      </section>
    </main>
  )
}
