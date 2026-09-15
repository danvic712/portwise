import { House, RotateCw, TriangleAlert } from "lucide-react"

import { StatusPageShell } from "@/components/feedback/StatusPageShell"
import { useLocale } from "@/shared/i18n/i18n"

type ApplicationErrorPageProps = {
  message: string
  onRetry: () => void
  onNavigate: (path: string) => void
}

export function ApplicationErrorPage({ message, onRetry, onNavigate }: ApplicationErrorPageProps) {
  const { messages } = useLocale()
  const copy = messages.common.ui.states.applicationError

  return (
    <StatusPageShell
      tone="attention"
      code="500"
      eyebrow={copy.eyebrow}
      title={copy.title}
      description={copy.description}
      noteLabel={copy.noteLabel}
      note={message}
      illustrationLabel={copy.illustrationLabel}
      illustrationHint={copy.illustrationHint}
      icon={TriangleAlert}
      onNavigate={onNavigate}
      primaryAction={{ label: copy.actions.retry, onClick: onRetry, icon: RotateCw }}
      secondaryAction={{ label: copy.actions.goOverview, onClick: () => onNavigate("/overview"), icon: House }}
    />
  )
}
