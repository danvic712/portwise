import { CheckCircle2, CircleAlert, X, XCircle } from "lucide-react"
import * as React from "react"

import { ActionFeedbackContext, type ActionFeedbackInput, type ActionFeedbackTone } from "@/app/providers/ActionFeedbackContext"
import { Alert, AlertDescription } from "@/components/ui/Alert"
import { useLocale } from "@/shared/i18n/i18n"

type VisibleFeedback = Required<Pick<ActionFeedbackInput, "message" | "tone" | "durationMs">> & { id: number }

const exitDurationMs = 180

function defaultDuration(tone: ActionFeedbackTone) {
  if (tone === "error") return 8000
  if (tone === "attention") return 6500
  return 4500
}

export function ActionFeedbackProvider({ children }: { children: React.ReactNode }) {
  const { messages } = useLocale()
  const [feedback, setFeedback] = React.useState<VisibleFeedback | null>(null)
  const [leaving, setLeaving] = React.useState(false)
  const sequence = React.useRef(0)
  const dismissTimer = React.useRef<number | null>(null)
  const exitTimer = React.useRef<number | null>(null)

  const clearTimers = React.useCallback(() => {
    if (dismissTimer.current !== null) window.clearTimeout(dismissTimer.current)
    if (exitTimer.current !== null) window.clearTimeout(exitTimer.current)
    dismissTimer.current = null
    exitTimer.current = null
  }, [])

  const dismissFeedback = React.useCallback(() => {
    if (!feedback || leaving) return
    if (dismissTimer.current !== null) window.clearTimeout(dismissTimer.current)
    dismissTimer.current = null
    setLeaving(true)
    exitTimer.current = window.setTimeout(() => {
      setFeedback(null)
      setLeaving(false)
      exitTimer.current = null
    }, exitDurationMs)
  }, [feedback, leaving])

  const showFeedback = React.useCallback((input: ActionFeedbackInput) => {
    clearTimers()
    const tone = input.tone ?? "success"
    const durationMs = input.durationMs ?? defaultDuration(tone)
    sequence.current += 1
    setLeaving(false)
    setFeedback({ id: sequence.current, message: input.message, tone, durationMs })
  }, [clearTimers])

  React.useEffect(() => {
    if (!feedback || leaving) return undefined
    dismissTimer.current = window.setTimeout(dismissFeedback, feedback.durationMs)
    return () => {
      if (dismissTimer.current !== null) window.clearTimeout(dismissTimer.current)
      dismissTimer.current = null
    }
  }, [dismissFeedback, feedback, leaving])

  React.useEffect(() => clearTimers, [clearTimers])

  const value = React.useMemo(() => ({ showFeedback, dismissFeedback }), [dismissFeedback, showFeedback])
  const alertVariant = feedback?.tone === "error" ? "destructive" : feedback?.tone === "attention" ? "attention" : "default"

  return <ActionFeedbackContext.Provider value={value}>
    {children}
    {feedback && <Alert
      key={feedback.id}
      variant={alertVariant}
      className={`action-feedback action-feedback-${feedback.tone}${leaving ? " is-leaving" : ""}`}
      aria-atomic="true"
      style={{ "--action-feedback-duration": `${feedback.durationMs}ms` } as React.CSSProperties}
    >
      {feedback.tone === "success" ? <CheckCircle2 size={18} aria-hidden="true" /> : feedback.tone === "error" ? <XCircle size={18} aria-hidden="true" /> : <CircleAlert size={18} aria-hidden="true" />}
      <AlertDescription>{feedback.message}</AlertDescription>
      <button type="button" aria-label={messages.common.ui.actions.dismiss} onClick={dismissFeedback}><X size={16} aria-hidden="true" /></button>
      <span className="action-feedback-progress" aria-hidden="true" />
    </Alert>}
  </ActionFeedbackContext.Provider>
}
