import * as React from "react"

export type ActionFeedbackTone = "success" | "attention" | "error"

export type ActionFeedbackInput = {
  message: string
  tone?: ActionFeedbackTone
  durationMs?: number
}

export type ActionFeedbackContextValue = {
  showFeedback: (input: ActionFeedbackInput) => void
  dismissFeedback: () => void
}

export const ActionFeedbackContext = React.createContext<ActionFeedbackContextValue | undefined>(undefined)

export function useActionFeedback() {
  const context = React.useContext(ActionFeedbackContext)
  if (!context) throw new Error("useActionFeedback must be used within an ActionFeedbackProvider")
  return context
}
