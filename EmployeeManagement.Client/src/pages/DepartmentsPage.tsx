import type { FormEvent } from 'react'
import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Forbidden } from '../components/common/Forbidden'
import { PageTitle } from '../components/common/PageTitle'
import type { ApiClient } from '../types/api'
import type { Department, Session } from '../types/domain'
import type { Toast } from '../types/toast'
import { canManage } from '../utils/roles'
import './DepartmentsPage.css'

type DepartmentsPageProps = {
  api: ApiClient
  session: Session
  notify: (toast: Toast) => void
}

export function DepartmentsPage({ api, session, notify }: DepartmentsPageProps) {
  const [items, setItems] = useState<Department[]>([])
  const [name, setName] = useState('')
  const [editingDepartment, setEditingDepartment] = useState<Department | null>(null)
  const allowManage = canManage(session)

  const load = useCallback(() => api<Department[]>('/api/departments').then(setItems), [api])

  useEffect(() => {
    if (allowManage) {
      load().catch((error) => notify({ tone: 'error', message: error.message }))
    }
  }, [allowManage, load, notify])

  if (!allowManage) return <Forbidden />

  const resetForm = () => {
    setName('')
    setEditingDepartment(null)
  }

  const submit = async (event: FormEvent) => {
    event.preventDefault()

    try {
      if (editingDepartment) {
        await api<void>(`/api/departments/${editingDepartment.id}`, {
          method: 'PUT',
          body: JSON.stringify({ name }),
        })
      } else {
        await api<Department>('/api/departments', {
          method: 'POST',
          body: JSON.stringify({ name }),
        })
      }

      notify({
        tone: 'success',
        message: editingDepartment ? 'Department updated.' : 'Department created.',
      })
      resetForm()
      load()
    } catch (error) {
      notify({
        tone: 'error',
        message: error instanceof Error ? error.message : 'Unable to save department.',
      })
    }
  }

  const startEdit = (department: Department) => {
    setEditingDepartment(department)
    setName(department.departmentName)
  }

  const deleteDepartment = async (department: Department) => {
    if (!window.confirm(`Delete department "${department.departmentName}"?`)) return

    try {
      await api<void>(`/api/departments/${department.id}`, { method: 'DELETE' })
      notify({ tone: 'success', message: 'Department deleted.' })
      load()
    } catch (error) {
      notify({
        tone: 'error',
        message: error instanceof Error ? error.message : 'Unable to delete department.',
      })
    }
  }

  return (
    <main className="page">
      <PageTitle title="Departments" subtitle="Manage departments and open each department detail page." />
      <form className="toolbar" onSubmit={submit}>
        <input placeholder="Department name" value={name} onChange={(event) => setName(event.target.value)} required />
        <button className="primary-btn compact" type="submit">
          {editingDepartment ? 'Update Department' : 'Add Department'}
        </button>
        {editingDepartment && (
          <button className="ghost-btn" onClick={resetForm} type="button">
            Cancel Edit
          </button>
        )}
      </form>
      <section className="panel">
        <div className="department-list">
          {items.map((department) => (
            <article className="department-row" key={department.id}>
              <div>
                <h3>{department.departmentName}</h3>
                <p>{department.employee?.length ?? 0} employees</p>
              </div>
              <div className="row-actions">
                <Link className="secondary-btn link-btn mini" to={`/departments/${department.id}`}>
                  View Details
                </Link>
                <button className="secondary-btn mini" onClick={() => startEdit(department)} type="button">
                  Edit
                </button>
                <button className="danger-btn mini" onClick={() => deleteDepartment(department)} type="button">
                  Delete
                </button>
              </div>
            </article>
          ))}
        </div>
      </section>
    </main>
  )
}
