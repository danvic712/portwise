import { LocaleProvider } from "@/shared/i18n/i18n"
import { ApplicationShell } from "@/app/ApplicationShell"

export default function App() {
  return <LocaleProvider><ApplicationShell /></LocaleProvider>
}
