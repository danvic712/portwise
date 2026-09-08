import { LocaleProvider } from "@/lib/i18n"
import { ApplicationShell } from "@/lib/application-shell"

export default function App() {
  return <LocaleProvider><ApplicationShell /></LocaleProvider>
}
