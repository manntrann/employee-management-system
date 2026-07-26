import { EmptyState } from './EmptyState'

export function Forbidden() {
  return (
    <main className="page">
      <EmptyState text="You do not have access to this page." />
    </main>
  )
}
