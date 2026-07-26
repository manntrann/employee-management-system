import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { EmptyState } from '../components/common/EmptyState'
import { Metric } from '../components/common/Metric'
import { PageTitle } from '../components/common/PageTitle'
import { EmployeeTable } from '../components/employees/EmployeeTable'
import { LeaveList } from '../components/leave/LeaveList'
import type { ApiClient } from '../types/api'
import type { Department, Employee, LeaveRequest, PaginatedResponse, Session } from '../types/domain'
import { canManage } from '../utils/roles'
import './DashboardPage.css'

type DashboardPageProps = {
  api: ApiClient
  session: Session
}

export function DashboardPage({ api, session }: DashboardPageProps) {
  const [employees, setEmployees] = useState<Employee[]>([])
  const [departments, setDepartments] = useState<Department[]>([])
  const [leaveRequests, setLeaveRequests] = useState<LeaveRequest[]>([])
  const [loading, setLoading] = useState(true)
  const allowManage = canManage(session)

  useEffect(() => {
    Promise.all([
      api<PaginatedResponse<Employee>>('/api/employees?page=1&pageSize=100'),
      allowManage ? api<Department[]>('/api/departments') : Promise.resolve([]),
      session.role === 'Employee'
        ? api<LeaveRequest[]>('/api/leave-requests/mine')
        : api<LeaveRequest[]>('/api/leave-requests'),
    ])
      .then(([employeePage, departmentItems, leaveItems]) => {
        setEmployees(employeePage.data)
        setDepartments(departmentItems)
        setLeaveRequests(leaveItems)
      })
      .finally(() => setLoading(false))
  }, [allowManage, api, session.role])

  const departmentStats = Object.entries(
    employees.reduce<Record<string, number>>((acc, employee) => {
      const name = employee.departmentName?.trim() || 'Unassigned'
      acc[name] = (acc[name] ?? 0) + 1
      return acc
    }, {}),
  ).sort((a, b) => b[1] - a[1])

  const topDepartment = departmentStats[0]
  const averagePerDepartment = departmentStats.length
    ? Math.round((employees.length / departmentStats.length) * 10) / 10
    : 0
  const pendingCount = leaveRequests.filter((item) => item.status === 0).length
  const approvedCount = leaveRequests.filter((item) => item.status === 1).length
  const rejectedCount = leaveRequests.filter((item) => item.status === 2).length
  const cancelledCount = leaveRequests.filter((item) => item.status === 3).length

  return (
    <main className="page">
      <PageTitle title="Dashboard" subtitle="A quick view of employees, departments, and leave activity." />
      {loading ? (
        <EmptyState text="Loading dashboard data..." />
      ) : (
        <>
          <section className="metric-grid">
            <Metric label="Employees" value={employees.length} />
            <Metric label="Departments" value={departments.length || departmentStats.length} />
            <Metric label="Pending Requests" value={pendingCount} />
            <Metric label="On Leave" value={approvedCount} />
          </section>
          <section className="dashboard-grid">
            <div className="panel department-insights">
              <div className="panel-heading">
                <h3>Department Structure</h3>
                {topDepartment && <span>{topDepartment[0]} is largest</span>}
              </div>
              <div className="insight-grid">
                <article>
                  <span>Largest Department</span>
                  <strong>{topDepartment?.[0] ?? 'No data yet'}</strong>
                  <p>{topDepartment ? `${topDepartment[1]} employees` : 'Create employees to start reporting.'}</p>
                </article>
                <article>
                  <span>Average</span>
                  <strong>{averagePerDepartment}</strong>
                  <p>employees / department</p>
                </article>
              </div>
              <div className="department-distribution">
                {departmentStats.length === 0 ? (
                  <EmptyState text="No department data yet." />
                ) : (
                  departmentStats.map(([name, count]) => (
                    <div className="distribution-row" key={name}>
                      <div>
                        <strong>{name}</strong>
                        <span>{count} employees</span>
                      </div>
                      <div className="distribution-track">
                        <i style={{ width: `${Math.max(12, (count / Math.max(1, employees.length)) * 100)}%` }} />
                      </div>
                    </div>
                  ))
                )}
              </div>
            </div>
            <div className="panel">
              <div className="panel-heading">
                <h3>Latest Leave Requests</h3>
                <Link className="secondary-btn mini link-btn" to={allowManage ? '/leave-requests' : '/my-leave'}>
                  View All
                </Link>
              </div>
              <LeaveList items={leaveRequests.slice(0, 2)} />
              <div className="summary-strip leave-status-strip">
                <span>Pending {pendingCount}</span>
                <span>Approved {approvedCount}</span>
                <span>Rejected {rejectedCount}</span>
                <span>Cancelled {cancelledCount}</span>
              </div>
            </div>
          </section>
          <section className="panel">
            <h3>New Employees</h3>
            <EmployeeTable employees={employees.slice(0, 6)} canViewSalary={allowManage} />
          </section>
        </>
      )}
    </main>
  )
}
