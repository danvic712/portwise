import { RefreshCw } from "lucide-react"

import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/Card"
import { Skeleton } from "@/components/ui/Skeleton"
import { PageFrame } from "@/components/layout/PageFrame"

export type SettingsSkeletonGroup = {
  key: string
  fieldCount: number
}

export function SettingsPageSkeleton({ label, groups }: { label: string; groups: SettingsSkeletonGroup[] }) {
  return <div className="settings-skeleton" role="status" aria-live="polite" aria-busy="true">
    <div className="settings-skeleton-status">
      <span className="settings-skeleton-status-mark"><RefreshCw size={16} aria-hidden="true" /></span>
      <span>{label}</span>
    </div>

    <div className="settings-skeleton-page-title">
      <Skeleton className="settings-skeleton-eyebrow" />
      <Skeleton className="settings-skeleton-title" />
      <Skeleton className="settings-skeleton-description" />
    </div>

    <Card className="settings-skeleton-card settings-skeleton-workflow">
      <CardHeader className="settings-skeleton-card-header">
        <Skeleton className="settings-skeleton-workflow-title" />
        <Skeleton className="settings-skeleton-workflow-description" />
      </CardHeader>
      <CardContent className="settings-skeleton-workflow-steps">
        {groups.slice(0, 3).map((group, index) => <div className="settings-skeleton-step" key={group.key}>
          <Skeleton className="settings-skeleton-step-number" />
          <div><Skeleton className="settings-skeleton-step-title" /><Skeleton className="settings-skeleton-step-copy" /></div>
          <span className="sr-only">{index + 1}</span>
        </div>)}
      </CardContent>
    </Card>

    <div className="settings-skeleton-layout">
      <Card className="settings-skeleton-card settings-skeleton-stocks">
        <CardHeader className="settings-skeleton-card-header settings-skeleton-stocks-header">
          <div><Skeleton className="settings-skeleton-section-label" /><Skeleton className="settings-skeleton-section-title" /><Skeleton className="settings-skeleton-section-copy" /></div>
          <Skeleton className="settings-skeleton-badge" />
        </CardHeader>
        <CardContent className="settings-skeleton-stock-list">
          {["one", "two", "three"].map((key) => <div className="settings-skeleton-stock" key={key}>
            <Skeleton className="settings-skeleton-stock-avatar" />
            <div><Skeleton className="settings-skeleton-stock-name" /><Skeleton className="settings-skeleton-stock-code" /></div>
            <div className="settings-skeleton-stock-meta"><Skeleton className="settings-skeleton-stock-meta-line" /><Skeleton className="settings-skeleton-stock-meta-value" /></div>
          </div>)}
        </CardContent>
        <CardFooter className="settings-skeleton-footer"><Skeleton className="settings-skeleton-footer-icon" /><Skeleton className="settings-skeleton-footer-copy" /></CardFooter>
      </Card>

      <Card className="settings-skeleton-card settings-skeleton-editor">
        <CardHeader className="settings-skeleton-card-header settings-skeleton-editor-header">
          <div><Skeleton className="settings-skeleton-section-label" /><Skeleton className="settings-skeleton-editor-title" /><Skeleton className="settings-skeleton-section-copy" /></div>
          <Skeleton className="settings-skeleton-stamp" />
        </CardHeader>
        <CardContent className="settings-skeleton-editor-content">
          <div className="settings-skeleton-field-grid settings-skeleton-identity">
            {["model", "date"].map((key) => <SettingsSkeletonField key={key} />)}
          </div>
          {groups.map((group) => <section className="settings-skeleton-group" key={group.key}>
            <div className="settings-skeleton-group-heading"><Skeleton className="settings-skeleton-group-title" /><Skeleton className="settings-skeleton-group-icon" /></div>
            <div className="settings-skeleton-field-grid">{Array.from({ length: group.fieldCount }, (_, index) => <SettingsSkeletonField key={`${group.key}-${index}`} />)}</div>
          </section>)}
        </CardContent>
        <CardFooter className="settings-skeleton-editor-footer"><Skeleton className="settings-skeleton-hint" /><Skeleton className="settings-skeleton-save" /></CardFooter>
      </Card>
    </div>
  </div>
}

export function SettingsParameterSkeleton({ label, groups }: { label: string; groups: SettingsSkeletonGroup[] }) {
  return <CardContent className="settings-parameters-loading" aria-live="polite" aria-busy="true">
    <div className="settings-loading-status">
      <span className="settings-loading-mark"><RefreshCw size={17} aria-hidden="true" /></span>
      <div><strong>{label}</strong><Skeleton className="settings-loading-status-line" /></div>
    </div>
    <div className="settings-loading-identity">
      {["model", "date"].map((key) => <SettingsSkeletonField key={key} />)}
    </div>
    {groups.map((group) => <section className="settings-loading-group" key={group.key}>
      <div className="settings-loading-group-heading"><Skeleton className="settings-loading-group-label" /><Skeleton className="settings-loading-group-icon" /></div>
      <div className="settings-loading-grid">{Array.from({ length: group.fieldCount }, (_, index) => <SettingsSkeletonField key={`${group.key}-${index}`} />)}</div>
    </section>)}
  </CardContent>
}

function SettingsSkeletonField() {
  return <div className="settings-loading-field"><Skeleton className="settings-loading-label" /><Skeleton className="settings-loading-input" /><Skeleton className="settings-loading-help" /></div>
}

export function SettingsWorkspaceSkeleton({ label, onNavigate, kind = "stock" }: { label: string; onNavigate: (path: string) => void; kind?: "stock" | "ai" }) {
  return <PageFrame currentPath="/settings" onNavigate={onNavigate} dataState="pending" contentClassName="settings-workspace-wrap">
    <div className="settings-workspace-hero settings-workspace-hero-loading" aria-hidden="true">
      <Skeleton className="settings-workspace-skeleton-sticker" />
      <div className="settings-workspace-hero-copy"><Skeleton className="settings-workspace-skeleton-kicker" /><Skeleton className="settings-workspace-skeleton-title" /><Skeleton className="settings-workspace-skeleton-copy" /></div>
      <Skeleton className="settings-workspace-skeleton-status" />
    </div>
    <div className="settings-workspace-panel">
      <div className="settings-workspace-tabs settings-workspace-skeleton-tabs" aria-hidden="true"><Skeleton /><Skeleton /></div>
      <SettingsFormSkeleton kind={kind} label={label} />
    </div>
  </PageFrame>
}

export function SettingsFormSkeleton({ kind, label }: { kind: "stock" | "ai"; label: string }) {
  return <div className="settings-workspace-form-skeleton" role="status" aria-label={label} aria-busy="true">
    <span className="sr-only">{label}</span>
    <section>
      <Skeleton className="settings-workspace-skeleton-kicker" />
      <Skeleton className="settings-workspace-skeleton-section-title" />
      <Skeleton className="settings-workspace-skeleton-copy" />
      {kind === "ai" && <div className="settings-workspace-skeleton-providers">{[0, 1, 2, 3].map((index) => <Skeleton key={index} />)}</div>}
      <div className="settings-workspace-skeleton-fields">{[0, 1].map((index) => <div key={index}><Skeleton /><Skeleton /></div>)}</div>
      <Skeleton className="settings-workspace-skeleton-action" />
    </section>
    <section>
      <Skeleton className="settings-workspace-skeleton-kicker" />
      <Skeleton className="settings-workspace-skeleton-section-title" />
      <Skeleton className="settings-workspace-skeleton-copy" />
      <div className="settings-workspace-skeleton-routes">{Array.from({ length: kind === "ai" ? 2 : 4 }, (_, index) => <div key={index}><Skeleton /><Skeleton /></div>)}</div>
    </section>
  </div>
}
