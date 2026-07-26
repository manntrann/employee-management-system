import './LeaveMeter.css'
import '../common/Metric.css'

type LeaveMeterProps = {
  title: string
  used: number
  total: number
  remaining: number
}

export function LeaveMeter({ title, used, total, remaining }: LeaveMeterProps) {
  return (
    <article className="metric">
      <span>{title}</span>
      <strong>{remaining}</strong>
      <p>
        {used}/{total} days used
      </p>
      <div className="meter">
        <i style={{ width: `${Math.min(100, (used / Math.max(1, total)) * 100)}%` }} />
      </div>
    </article>
  )
}
