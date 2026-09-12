import { SettingsPage } from "@/settings/SettingsPage"
import type { QueryPatch } from "@/app/routing/navigation"

export function StrategyPage({ onNavigate, onReplaceQuery, initialStockKey }: { onNavigate: (path: string) => void; onReplaceQuery: (patch: QueryPatch) => void; initialStockKey: string }) {
  return <SettingsPage currentPath="/strategy" onNavigate={onNavigate} onReplaceQuery={onReplaceQuery} initialStockKey={initialStockKey} />
}
