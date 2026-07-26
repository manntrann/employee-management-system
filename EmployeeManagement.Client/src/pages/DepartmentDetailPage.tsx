import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { EmptyState } from '../components/common/EmptyState'
import { Forbidden } from '../components/common/Forbidden'
import { PageTitle } from '../components/common/PageTitle'
import { EmployeeTable } from '../components/employees/EmployeeTable'
import type { ApiClient } from '../types/api'
import type { Department, Session } from '../types/domain'
import { canManage } from '../utils/roles'
import './DepartmentDetailPage.css'

type DepartmentDetailPageProps = {
  api: ApiClient
  session: Session
}

export function DepartmentDetailPage({ api, session }: DepartmentDetailPageProps) {
  const { id } = useParams()
  const [department, setDepartment] = useState<Department | null>(null)
  const [loading, setLoading] = useState(true)
  const allowManage = canManage(session)

  useEffect(() => {
    if (!allowManage) return

    api<Department>(`/api/departments/${id}`)
      .then(setDepartment)
      .finally(() => setLoading(false))
  }, [allowManage, api, id])

  if (!allowManage) return <Forbidden />

  if (loading) {
    return (
      <main className="page">
        <EmptyState text="Loading department..." />
      </main>
    )
  }

  if (!department) {
    return (
      <main className="page">
        <EmptyState text="Department not found." />
      </main>
    )
  }

  const employeeCount = department.employee?.length ?? 0

  return (
    <main className="page">
      <div className="detail-back">
        <Link className="ghost-btn link-btn mini" to="/departments">
          Back
        </Link>
      </div>
      <PageTitle title={department.departmentName} subtitle={`${employeeCount} employees in this department.`} />
      <section className="panel department-detail-summary">
        <strong>{department.departmentName}</strong>
        <span>{employeeCount} employee profiles</span>
      </section>
      <section className="panel">
        <h3>Department Employees</h3>
        <EmployeeTable employees={department.employee ?? []} canViewSalary={allowManage} />
      </section>
    </main>
  )
}
