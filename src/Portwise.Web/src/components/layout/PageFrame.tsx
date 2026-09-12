import { ChevronRight } from "lucide-react"

import { Footer } from "@/components/layout/Footer"
import { Header } from "@/components/layout/Header"
import { findSiteNavigationItem } from "@/app/routing/site-navigation"
import { useLocale } from "@/shared/i18n/i18n"
import { cn } from "@/shared/utils/utils"

type PageFrameProps = {
  children: React.ReactNode
  currentPath: string
  onNavigate: (path: string) => void
  lastUpdated?: string | null
  dataState?: "synced" | "pending" | "unknown"
  contentClassName?: string
  showBreadcrumb?: boolean
}

export function PageFrame({ children, currentPath, onNavigate, lastUpdated, dataState = "unknown", contentClassName, showBreadcrumb = true }: PageFrameProps) {
  const { messages } = useLocale()
  const pageLabel = findSiteNavigationItem(currentPath)
  const usesWideFrame = currentPath === "/overview" || currentPath === "/status"

  return (
    <div className={cn("app-frame", usesWideFrame && "app-frame-wide", currentPath === "/overview" && "app-frame-overview", currentPath === "/status" && "app-frame-status", currentPath === "/onboarding" && "app-frame-onboarding")}>
      <Header currentPath={currentPath} onNavigate={onNavigate} />
      <main className={cn("page-wrap", contentClassName)}>
        {showBreadcrumb && currentPath !== "/overview" && <div className="page-crumb"><span>{messages.common.ui.appName}</span><ChevronRight size={13} /><span className="page-crumb-current">{pageLabel ? messages.common.ui.nav[pageLabel.key] : messages.common.ui.nav.settings}</span></div>}
        {children}
      </main>
      <Footer lastUpdated={lastUpdated} dataState={dataState} />
    </div>
  )
}
