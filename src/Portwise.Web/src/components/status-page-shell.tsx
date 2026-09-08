import type { LucideIcon } from "lucide-react"

import { PageFrame } from "@/components/page-frame"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card"
import { Skeleton } from "@/components/ui/skeleton"
import { cn } from "@/lib/utils"
import { useLocale } from "@/lib/i18n"

import "./status-page.css"

type StatusPageAction = {
  label: string
  onClick: () => void
  icon: LucideIcon
}

type StatusPageShellProps = {
  tone: "calm" | "attention"
  code: string
  eyebrow: string
  title: string
  description: string
  noteLabel: string
  note: string
  illustrationLabel: string
  illustrationHint: string
  icon: LucideIcon
  primaryAction: StatusPageAction
  secondaryAction?: StatusPageAction
  onNavigate: (path: string) => void
}

export function StatusPageShell({
  tone,
  code,
  eyebrow,
  title,
  description,
  noteLabel,
  note,
  illustrationLabel,
  illustrationHint,
  icon: StatusIcon,
  primaryAction,
  secondaryAction,
  onNavigate,
}: StatusPageShellProps) {
  const { messages } = useLocale()
  const PrimaryIcon = primaryAction.icon
  const SecondaryIcon = secondaryAction?.icon

  return (
    <PageFrame currentPath="/status" onNavigate={onNavigate} dataState="unknown" contentClassName="status-page-wrap" showBreadcrumb={false}>
      <section className={cn("status-page", `status-page-${tone}`)} aria-labelledby="status-page-title">
        <div className="status-page-copy">
          <p className="status-page-eyebrow"><span aria-hidden="true">✦</span>{eyebrow}</p>
          <h1 id="status-page-title">{title}</h1>
          <p className="status-page-description">{description}</p>

          <Card className="status-page-note">
            <CardHeader className="status-page-note-header">
              <span className="status-page-note-icon" aria-hidden="true"><StatusIcon size={17} /></span>
              <span>{noteLabel}</span>
            </CardHeader>
            <CardContent>{note}</CardContent>
          </Card>

          <div className="status-page-actions">
            <Button size="lg" onClick={primaryAction.onClick}>
              {primaryAction.label}
              <PrimaryIcon data-icon="inline-end" aria-hidden="true" />
            </Button>
            {secondaryAction && SecondaryIcon && (
              <Button size="lg" variant="outline" onClick={secondaryAction.onClick}>
                <SecondaryIcon data-icon="inline-start" aria-hidden="true" />
                {secondaryAction.label}
              </Button>
            )}
          </div>
        </div>

        <Card className="status-page-illustration" aria-label={`${code} · ${illustrationLabel}`}>
          <CardHeader className="status-page-illustration-header">
            <Badge variant="outline">{illustrationLabel}</Badge>
            <StatusIcon size={19} aria-hidden="true" />
          </CardHeader>
          <CardContent className="status-page-art">
            <span className="status-page-art-ring status-page-art-ring-one" aria-hidden="true" />
            <span className="status-page-art-ring status-page-art-ring-two" aria-hidden="true" />
            <span className="status-page-art-tape" aria-hidden="true" />
            <span className="status-page-code" aria-hidden="true">{code}</span>
            <span className="status-page-art-sticker" aria-hidden="true">{tone === "calm" ? messages.common.ui.statusSticker.calm : messages.common.ui.statusSticker.attention}</span>
          </CardContent>
          <CardFooter className="status-page-illustration-footer">
            <span className="status-page-footer-dot" aria-hidden="true" />
            <span>{illustrationHint}</span>
          </CardFooter>
        </Card>
      </section>
    </PageFrame>
  )
}

export function StatusPageSkeleton({ label, onNavigate }: { label: string; onNavigate: (path: string) => void }) {
  return (
    <PageFrame currentPath="/status" onNavigate={onNavigate} dataState="pending" contentClassName="status-page-wrap" showBreadcrumb={false}>
      <section className="status-page status-page-skeleton" aria-label={label} aria-live="polite" aria-busy="true">
        <div className="status-page-skeleton-copy">
          <Skeleton className="status-page-skeleton-eyebrow" />
          <Skeleton className="status-page-skeleton-title" />
          <Skeleton className="status-page-skeleton-title-short" />
          <Skeleton className="status-page-skeleton-description" />
          <Skeleton className="status-page-skeleton-description-short" />
          <Card className="status-page-skeleton-note">
            <Skeleton className="status-page-skeleton-note-icon" />
            <div>
              <Skeleton className="status-page-skeleton-note-title" />
              <Skeleton className="status-page-skeleton-note-copy" />
            </div>
          </Card>
          <div className="status-page-skeleton-actions">
            <Skeleton />
            <Skeleton />
          </div>
        </div>
        <Card className="status-page-illustration status-page-skeleton-illustration">
          <CardHeader className="status-page-illustration-header">
            <Skeleton className="status-page-skeleton-badge" />
            <Skeleton className="status-page-skeleton-icon" />
          </CardHeader>
          <CardContent className="status-page-art">
            <Skeleton className="status-page-skeleton-code" />
          </CardContent>
          <CardFooter className="status-page-illustration-footer">
            <Skeleton className="status-page-skeleton-footer-dot" />
            <Skeleton className="status-page-skeleton-footer-copy" />
          </CardFooter>
        </Card>
      </section>
    </PageFrame>
  )
}
