import * as React from "react"
import { ThemeProviderContext, type Theme } from "@/app/providers/ThemeContext"

type ThemeProviderProps = {
  children: React.ReactNode
  defaultTheme?: Theme
  storageKey?: string
}

const mediaQuery = "(prefers-color-scheme: dark)"

function isTheme(value: string | null): value is Theme {
  return value === "light" || value === "dark" || value === "system"
}

function resolveTheme(theme: Theme) {
  return theme === "system"
    ? window.matchMedia(mediaQuery).matches
      ? "dark"
      : "light"
    : theme
}

export function ThemeProvider({ children, defaultTheme = "system", storageKey = "portwise-theme" }: ThemeProviderProps) {
  const [theme, setTheme] = React.useState<Theme>(() => {
    const stored = localStorage.getItem(storageKey)
    return isTheme(stored) ? stored : defaultTheme
  })

  React.useEffect(() => {
    const applyTheme = () => {
      document.documentElement.classList.remove("light", "dark")
      document.documentElement.classList.add(resolveTheme(theme))
    }

    applyTheme()
    if (theme !== "system") {
      return undefined
    }

    const query = window.matchMedia(mediaQuery)
    query.addEventListener("change", applyTheme)
    return () => query.removeEventListener("change", applyTheme)
  }, [theme])

  const updateTheme = React.useCallback((nextTheme: Theme) => {
    localStorage.setItem(storageKey, nextTheme)
    setTheme(nextTheme)
  }, [storageKey])

  return (
    <ThemeProviderContext.Provider value={{ theme, setTheme: updateTheme }}>
      {children}
    </ThemeProviderContext.Provider>
  )
}
