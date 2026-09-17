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
      <div className="settings-workspace-tabs settings-workspace-skeleton-tabs" aria-hidden="true">
        {["stock", "ai"].map((tab) => <div className="settings-workspace-tab settings-workspace-skeleton-tab" key={tab}>
          <Skeleton className="settings-workspace-skeleton-tab-icon" />
          <Skeleton className="settings-workspace-skeleton-tab-label" />
          <Skeleton className="settings-workspace-skeleton-tab-status" />
        </div>)}
      </div>
      <SettingsFormSkeleton kind={kind} label={label} includeSchedule={kind === "stock"} />
    </div>
  </PageFrame>
}

export function SettingsFormSkeleton({ kind, label, includeSchedule = false }: { kind: "stock" | "ai"; label: string; includeSchedule?: boolean }) {
  return <div className={`settings-workspace-form-skeleton settings-${kind}-form-skeleton`} role="status" aria-label={label} aria-busy="true">
    <span className="sr-only">{label}</span>
    {kind === "stock" ? <StockDataFormSkeleton includeSchedule={includeSchedule} /> : <InferenceFormSkeleton />}
  </div>
}

function StockDataFormSkeleton({ includeSchedule }: { includeSchedule: boolean }) {
  return <>
    <div className="settings-stock-tab-intro settings-skeleton-stock-intro" aria-hidden="true">
      <Skeleton className="settings-skeleton-stock-intro-mark" />
      <div className="settings-stock-tab-intro-copy">
        <Skeleton className="settings-skeleton-stock-kicker" />
        <Skeleton className="settings-skeleton-stock-title" />
        <Skeleton className="settings-skeleton-stock-description" />
      </div>
    </div>

    <div className="settings-stock-capability-rail settings-skeleton-capability-rail" aria-hidden="true">
      {[0, 1, 2, 3].map((index) => <div className="settings-stock-capability-item" key={index}>
        <Skeleton className="settings-skeleton-capability-mark" />
        <div><Skeleton className="settings-skeleton-capability-title" /><Skeleton className="settings-skeleton-capability-provider" /></div>
        <Skeleton className="settings-skeleton-capability-badge" />
      </div>)}
    </div>

    <div className="settings-stock-config-grid settings-skeleton-stock-config" aria-hidden="true">
      <Card className="settings-stock-source-card settings-skeleton-stock-card">
        <SkeletonStockCardHeader route={false} />
        <CardContent><div className="settings-stock-fields settings-skeleton-stock-fields">
          <SkeletonStockField />
          <SkeletonStockField />
        </div></CardContent>
        <CardFooter><Skeleton className="settings-skeleton-card-button" /><Skeleton className="settings-skeleton-card-button settings-skeleton-card-button-secondary" /></CardFooter>
      </Card>

      <Card className="settings-stock-routes-card settings-skeleton-stock-card">
        <SkeletonStockCardHeader route />
        <CardContent><div className="settings-stock-route-fields settings-skeleton-route-fields">
          {[0, 1, 2, 3].map((index) => <div className="settings-stock-route-field" data-slot="field" key={index}>
            <div className="settings-skeleton-route-label-row"><Skeleton className="settings-skeleton-route-label" /><Skeleton className="settings-skeleton-route-badge" /></div>
            <Skeleton className="settings-skeleton-route-select" />
          </div>)}
        </div></CardContent>
        <CardFooter><Skeleton className="settings-skeleton-card-button" /></CardFooter>
      </Card>
    </div>

    {includeSchedule && <StockDataSyncScheduleSkeleton />}
  </>
}

function SkeletonStockCardHeader({ route }: { route: boolean }) {
  return <CardHeader>
    <div className="settings-stock-card-heading">
      <Skeleton className={`settings-skeleton-stock-card-icon${route ? " settings-skeleton-stock-card-icon-route" : ""}`} />
      <div className="settings-skeleton-stock-card-copy"><Skeleton className="settings-skeleton-card-kicker" /><Skeleton className="settings-skeleton-card-title" /><Skeleton className="settings-skeleton-card-description" /></div>
    </div>
    <Skeleton className={route ? "settings-skeleton-route-count" : "settings-skeleton-card-badge"} />
  </CardHeader>
}

function SkeletonStockField() {
  return <div className="settings-skeleton-stock-field" data-slot="field"><Skeleton className="settings-skeleton-field-label" /><Skeleton className="settings-skeleton-field-input" /></div>
}

export function StockDataSyncScheduleSkeleton({ wrapped = true, includeHeader = true }: { wrapped?: boolean; includeHeader?: boolean } = {}) {
  const header = <CardHeader>
    <div className="settings-skeleton-schedule-heading"><Skeleton className="settings-skeleton-card-kicker" /><Skeleton className="settings-skeleton-schedule-title" /><Skeleton className="settings-skeleton-schedule-description" /></div>
    <Skeleton className="settings-skeleton-card-badge" />
  </CardHeader>
  const content = <>
      <CardContent>
        <div className="settings-sync-schedule-layout">
          <div className="settings-sync-schedule-controls">
            <div className="settings-sync-schedule-grid settings-skeleton-schedule-controls">
              <div className="settings-skeleton-schedule-toggle"><Skeleton className="settings-skeleton-switch" /><div><Skeleton className="settings-skeleton-field-label" /><Skeleton className="settings-skeleton-field-help" /></div></div>
              <div className="settings-skeleton-schedule-field"><Skeleton className="settings-skeleton-field-label" /><Skeleton className="settings-skeleton-field-input" /><Skeleton className="settings-skeleton-field-help" /></div>
            </div>
          </div>
          <div className="settings-sync-times settings-skeleton-sync-times">
            <div className="settings-sync-times-heading"><div><Skeleton className="settings-skeleton-run-title" /><Skeleton className="settings-skeleton-run-copy" /></div><Skeleton className="settings-skeleton-run-count" /></div>
            <div className="settings-sync-time-list settings-skeleton-time-list">
              {[0, 1, 2, 3].map((index) => <div className="settings-sync-time-row" key={index}><Skeleton className="settings-skeleton-time-label" /><Skeleton className="settings-skeleton-time-picker" /><Skeleton className="settings-skeleton-time-remove" /></div>)}
            </div>
            <Skeleton className="settings-skeleton-add-time" />
          </div>
        </div>
        <div className="settings-sync-next-run settings-skeleton-next-run"><Skeleton className="settings-skeleton-next-run-icon" /><div className="settings-sync-next-run-copy"><div className="settings-sync-next-run-primary"><Skeleton className="settings-skeleton-next-run-label" /><Skeleton className="settings-skeleton-next-run-value" /></div><div className="settings-sync-configured-times"><Skeleton className="settings-skeleton-configured-label" /><div className="settings-sync-configured-time-list">{[0, 1, 2, 3].map((index) => <Skeleton className="settings-skeleton-configured-time" key={index} />)}</div></div></div></div>
      </CardContent>
      <CardFooter><Skeleton className="settings-skeleton-card-button" /></CardFooter>
  </>
  const card = <Card className="settings-sync-schedule-card settings-skeleton-schedule-card" aria-hidden="true">{includeHeader && header}{content}</Card>
  return wrapped ? <div className="settings-sync-schedule-wrap settings-sync-schedule-wrap-skeleton">{card}</div> : includeHeader ? card : content
}

function InferenceFormSkeleton() {
  return <div className="settings-skeleton-inference-sections" aria-hidden="true">
    <Card className="settings-subpage-card settings-inference-provider-card settings-skeleton-inference-card">
      <CardHeader><div className="settings-skeleton-inference-heading"><Skeleton className="settings-skeleton-card-kicker" /><Skeleton className="settings-skeleton-inference-title" /><Skeleton className="settings-skeleton-card-description" /></div></CardHeader>
      <CardContent>
        <div className="settings-inference-provider-list settings-skeleton-inference-providers">{[0, 1, 2, 3].map((index) => <Skeleton key={index} />)}</div>
        <div className="settings-provider-fields settings-skeleton-inference-fields"><SkeletonInferenceField /><SkeletonInferenceField /><SkeletonInferenceField /></div>
      </CardContent>
      <CardFooter><Skeleton className="settings-skeleton-card-button" /><Skeleton className="settings-skeleton-card-button settings-skeleton-card-button-secondary" /></CardFooter>
    </Card>
    <Card className="settings-subpage-card settings-inference-routes-card settings-skeleton-inference-card">
      <CardHeader><div className="settings-skeleton-inference-heading"><Skeleton className="settings-skeleton-card-kicker" /><Skeleton className="settings-skeleton-inference-title settings-skeleton-inference-title-short" /><Skeleton className="settings-skeleton-card-description" /></div></CardHeader>
      <CardContent className="settings-route-list settings-skeleton-inference-routes">{[0, 1].map((index) => <div className="settings-route-row" key={index}><div><Skeleton className="settings-skeleton-route-label" /><Skeleton className="settings-skeleton-route-badge" /></div><Skeleton className="settings-skeleton-route-select" /><Skeleton className="settings-skeleton-route-select" /></div>)}</CardContent>
      <CardFooter><Skeleton className="settings-skeleton-card-button" /></CardFooter>
    </Card>
  </div>
}

function SkeletonInferenceField() {
  return <div className="settings-skeleton-inference-field"><Skeleton className="settings-skeleton-field-label" /><Skeleton className="settings-skeleton-field-input" /><Skeleton className="settings-skeleton-field-help" /></div>
}
