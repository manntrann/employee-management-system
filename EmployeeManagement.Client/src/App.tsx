import { useCallback, useState } from 'react'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ToastMessage } from './components/common/ToastMessage'
import { useApi } from './hooks/useApi'
import { useSession } from './hooks/useSession'
import { DashboardLayout } from './layouts/DashboardLayout'
import { DashboardPage } from './pages/DashboardPage'
import { DepartmentDetailPage } from './pages/DepartmentDetailPage'
import { DepartmentsPage } from './pages/DepartmentsPage'
import { EmployeesPage } from './pages/EmployeesPage'
import { LoginPage } from './pages/LoginPage'
import type { Toast } from './types/toast'

function App() {
  const { session, saveSession, signOut } = useSession()
  const [toast, setToast] = useState<Toast | null>(null)
  const api = useApi(session, signOut)

  const notify = useCallback((nextToast: Toast) => {
    setToast(nextToast)
    window.setTimeout(() => setToast(null), 3600)
  }, [])

  return (
    <BrowserRouter>
      {toast && <ToastMessage toast={toast} />}
      <Routes>
        <Route
          path="/login"
          element={
            session ? (
              <Navigate to="/dashboard" replace />
            ) : (
              <LoginPage onLogin={saveSession} notify={notify} />
            )
          }
        />
        <Route
          path="/*"
          element={
            session ? (
              <DashboardLayout session={session} signOut={signOut}>
                <Routes>
                  <Route index element={<Navigate to="/dashboard" replace />} />
                  <Route path="dashboard" element={<DashboardPage />} />
                  <Route path="employees" element={<EmployeesPage api={api} session={session} notify={notify} />} />
                  <Route path="departments" element={<DepartmentsPage api={api} session={session} notify={notify} />} />
                  <Route path="departments/:id" element={<DepartmentDetailPage api={api} session={session} />} />
                  <Route path="*" element={<Navigate to="/dashboard" replace />} />
                </Routes>
              </DashboardLayout>
            ) : (
              <Navigate to="/login" replace />
            )
          }
        />
      </Routes>
    </BrowserRouter>
  )
}

export default App
