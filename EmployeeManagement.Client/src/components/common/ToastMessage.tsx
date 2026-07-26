import type { Toast } from '../../types/toast'
import './ToastMessage.css'

type ToastMessageProps = {
  toast: Toast
}

export function ToastMessage({ toast }: ToastMessageProps) {
  return <div className={`toast ${toast.tone}`}>{toast.message}</div>
}
