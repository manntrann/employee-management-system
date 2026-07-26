import { EmptyState } from '../components/common/EmptyState'
import { PageTitle } from '../components/common/PageTitle'

export function DashboardPage() {
  return (
    <main className="page">
      <PageTitle title="Dashboard" subtitle="Authentication and protected routing are ready." />
      <EmptyState text="Feature modules will appear in the next milestones." />
    </main>
  )
}
