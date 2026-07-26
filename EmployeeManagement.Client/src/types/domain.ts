export type Role = 'Admin' | 'Manager' | 'Employee'

export type Session = {
  token: string
  email: string
  role: Role
  userId: number
}

export type Employee = {
  id: number
  fullName: string
  email?: string
  position?: string
  salary: number
  phone?: string
  departmentId: number
  departmentName: string
  userId: number
  userName: string
  role: Role
  createdAt: string
}

export type PaginatedResponse<T> = {
  total: number
  page: number
  pageSize: number
  data: T[]
}

export type Department = {
  id: number
  departmentName: string
  employee?: Employee[]
}

export type LeaveStatus = 0 | 1 | 2 | 3
export type LeaveType = 0 | 1 | 2

export type LeaveRequest = {
  id: number
  employeeId: number
  employeeName: string
  leaveType: LeaveType
  startDate: string
  endDate: string
  reason?: string
  status: LeaveStatus
  requestedAt: string
  reviewedByUserId?: number
  reviewedByUserName?: string
  reviewedAt?: string
  reviewNote?: string
}

export type LeaveBalance = {
  year: number
  annualAllowance: number
  annualUsed: number
  annualRemaining: number
  sickAllowance: number
  sickUsed: number
  sickRemaining: number
}
