import { useCallback, useState } from 'react'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ToastMessage } from './components/common/ToastMessage'
import { useSession } from './hooks/useSession'
import { DashboardLayout } from './layouts/DashboardLayout'
import { DashboardPage } from './pages/DashboardPage'
import { LoginPage } from './pages/LoginPage'
import type { Toast } from './types/toast'

function App() {
  const { session, saveSession, signOut } = useSession()
  const [toast, setToast] = useState<Toast | null>(null)

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
