import { ArrowLeft, House, MapPinOff } from "lucide-react"

import { StatusPageShell } from "@/components/status-page-shell"
import { useLocale } from "@/lib/i18n"

export function NotFoundPage({ onNavigate }: { onNavigate: (path: string) => void }) {
  const { messages } = useLocale()
  const copy = messages.common.ui.states.notFound

  return (
    <StatusPageShell
      tone="calm"
      code="404"
      eyebrow={copy.eyebrow}
      title={copy.title}
      description={copy.description}
      noteLabel={copy.noteLabel}
      note={copy.note}
      illustrationLabel={copy.illustrationLabel}
      illustrationHint={copy.illustrationHint}
      icon={MapPinOff}
      onNavigate={onNavigate}
      primaryAction={{ label: copy.actions.goOverview, onClick: () => onNavigate("/overview"), icon: House }}
      secondaryAction={{ label: copy.actions.goBack, onClick: () => window.history.back(), icon: ArrowLeft }}
    />
  )
}
