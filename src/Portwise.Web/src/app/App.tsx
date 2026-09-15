import { LocaleProvider } from "@/shared/i18n/i18n"
import { ApplicationShell } from "@/app/ApplicationShell"
import { ActionFeedbackProvider } from "@/app/providers/ActionFeedbackProvider"
import { PreferencesSyncProvider } from "@/app/providers/PreferencesSyncProvider"
import { ThemeProvider } from "@/app/providers/ThemeProvider"

export default function App() {
  return <LocaleProvider><ThemeProvider><ActionFeedbackProvider><PreferencesSyncProvider><ApplicationShell /></PreferencesSyncProvider></ActionFeedbackProvider></ThemeProvider></LocaleProvider>
}
