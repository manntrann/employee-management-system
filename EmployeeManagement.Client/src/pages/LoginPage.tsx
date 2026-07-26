import type { FormEvent } from 'react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { API_BASE_URL } from '../config/apiConfig'
import type { Toast } from '../types/toast'
import './LoginPage.css'

type LoginPageProps = {
  onLogin: (token: string, remember: boolean) => void
  notify: (toast: Toast) => void
}

export function LoginPage({ onLogin, notify }: LoginPageProps) {
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [remember, setRemember] = useState(true)
  const [loading, setLoading] = useState(false)

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setLoading(true)

    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      })
      const body = await response.json().catch(() => null)

      if (!response.ok) {
        throw new Error(body?.message ?? 'Invalid email or password.')
      }

      onLogin(body.token, remember)
      navigate('/dashboard')
    } catch (error) {
      notify({
        tone: 'error',
        message: error instanceof Error ? error.message : 'Login failed.',
      })
    } finally {
      setLoading(false)
    }
  }

  return (
    <main className="login-page">
      <section className="login-panel">
        <div className="brand-mark">PH</div>
        <h1>PeopleHub</h1>
        <p>Employee, department, account, and leave management for internal teams.</p>
        <form onSubmit={submit} className="form-stack">
          <label>
            Email
            <input value={email} onChange={(event) => setEmail(event.target.value)} type="email" required />
          </label>
          <label>
            Password
            <input value={password} onChange={(event) => setPassword(event.target.value)} type="password" required />
          </label>
          <label className="check-row">
            <input checked={remember} onChange={(event) => setRemember(event.target.checked)} type="checkbox" />
            Remember me
          </label>
          <button className="primary-btn" disabled={loading} type="submit">
            {loading ? 'Signing in...' : 'Sign in'}
          </button>
        </form>
      </section>
    </main>
  )
}
