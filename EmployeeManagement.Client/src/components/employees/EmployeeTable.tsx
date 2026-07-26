import type { Employee } from '../../types/domain'
import { formatDate } from '../../utils/date'
import { money } from '../../utils/format'
import { EmptyState } from '../common/EmptyState'
import './EmployeeTable.css'

type EmployeeTableProps = {
  employees: Employee[]
  canViewSalary: boolean
  canViewRole?: boolean
  canManage?: boolean
  onEdit?: (employee: Employee) => void
  onDelete?: (employee: Employee) => void
}

export function EmployeeTable({
  employees,
  canViewSalary,
  canViewRole = false,
  canManage = false,
  onEdit,
  onDelete,
}: EmployeeTableProps) {
  if (employees.length === 0) {
    return <EmptyState text="No employees to display." />
  }

  return (
    <div className="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Employee</th>
            <th>Position</th>
            <th>Department</th>
            <th>Email</th>
            <th>Phone</th>
            {canViewRole && <th>Role</th>}
            <th>Start Date</th>
            {canViewSalary && <th>Salary</th>}
            {canManage && <th>Actions</th>}
          </tr>
        </thead>
        <tbody>
          {employees.map((employee) => (
            <tr key={employee.id}>
              <td>
                <strong>{employee.fullName}</strong>
              </td>
              <td>{employee.position ?? '...'}</td>
              <td>{employee.departmentName}</td>
              <td>{employee.email ?? '...'}</td>
              <td>{employee.phone ?? '...'}</td>
              {canViewRole && (
                <td>
                  <span className="role-pill">{employee.role}</span>
                </td>
              )}
              <td>{formatDate(employee.createdAt)}</td>
              {canViewSalary && <td>{money(employee.salary)}</td>}
              {canManage && (
                <td>
                  <div className="row-actions">
                    <button className="secondary-btn mini" onClick={() => onEdit?.(employee)} type="button">
                      Edit
                    </button>
                    <button className="danger-btn mini" onClick={() => onDelete?.(employee)} type="button">
                      Delete
                    </button>
                  </div>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
