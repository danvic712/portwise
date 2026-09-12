import { LocaleProvider } from "@/shared/i18n/i18n"
import { ApplicationShell } from "@/app/ApplicationShell"
import { ThemeProvider } from "@/app/providers/ThemeProvider"

export default function App() {
  return <LocaleProvider><ThemeProvider><ApplicationShell /></ThemeProvider></LocaleProvider>
}
