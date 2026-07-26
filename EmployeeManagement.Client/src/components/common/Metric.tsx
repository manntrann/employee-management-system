import './Metric.css'

type MetricProps = {
  label: string
  value: number
}

export function Metric({ label, value }: MetricProps) {
  return (
    <article className="metric">
      <span>{label}</span>
      <strong>{value}</strong>
    </article>
  )
}
