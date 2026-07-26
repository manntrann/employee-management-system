export function daysBetween(start: string, end: string) {
  if (!start || !end) return 0

  const startDate = new Date(`${start}T00:00:00`)
  const endDate = new Date(`${end}T00:00:00`)
  const diff = endDate.getTime() - startDate.getTime()

  return diff < 0 ? 0 : Math.floor(diff / 86400000) + 1
}

export function formatDate(value?: string) {
  if (!value) return '...'

  return new Intl.DateTimeFormat('vi-VN').format(new Date(value))
}
