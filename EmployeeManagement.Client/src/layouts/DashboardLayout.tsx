import type { ReactNode } from 'react'
import { Link, NavLink } from 'react-router-dom'
import type { Role, Session } from '../types/domain'
import './DashboardLayout.css'

type DashboardLayoutProps = {
  children: ReactNode
  session: Session
  signOut: () => void
}

const navItems: Array<[string, string, Role[]]> = [
  ['Dashboard', '/dashboard', ['Admin', 'Manager', 'Employee']],
  ['My Profile', '/profile', ['Admin', 'Manager', 'Employee']],
  ['Employees', '/employees', ['Admin', 'Manager', 'Employee']],
  ['Departments', '/departments', ['Admin', 'Manager']],
  ['My Leave', '/my-leave', ['Admin', 'Manager', 'Employee']],
  ['Leave Review', '/leave-requests', ['Admin', 'Manager']],
]

export function DashboardLayout({ children, session, signOut }: DashboardLayoutProps) {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <Link className="brand" to="/dashboard">
          <span className="brand-mark small">PH</span>
          <span>PeopleHub</span>
        </Link>
        <nav>
          {navItems
            .filter((item) => item[2].includes(session.role))
            .map(([label, href]) => (
              <NavLink key={href} to={href}>
                {label}
              </NavLink>
            ))}
        </nav>
        <button className="ghost-btn" onClick={signOut} type="button">
          Sign out
        </button>
      </aside>
      <div className="workspace">
        <header className="topbar">
          <div>
            <p className="eyebrow">Welcome</p>
            <h2>{session.email}</h2>
          </div>
          <span className="role-pill">{session.role}</span>
        </header>
        {children}
      </div>
    </div>
  )
}
