import { PageFrame } from "@/components/layout/PageFrame"
import { Skeleton } from "@/components/ui/Skeleton"
import "./onboarding.css"

type OnboardingSkeletonProps = {
  label: string
  onNavigate: (path: string) => void
}

export function OnboardingSkeleton({ label, onNavigate }: OnboardingSkeletonProps) {
  return (
    <PageFrame currentPath="/onboarding" onNavigate={onNavigate} dataState="pending" showBreadcrumb={false} showNavigation={false} showSettings={false} contentClassName="onboarding-page-wrap">
      <div className="onboarding-page onboarding-loading-page" role="status" aria-live="polite" aria-busy="true" aria-label={label}>
        <section className="onboarding-intro onboarding-skeleton-intro" aria-hidden="true">
          <Skeleton className="onboarding-skeleton-eyebrow" />
          <h1><Skeleton className="onboarding-skeleton-title" /><Skeleton className="onboarding-skeleton-title onboarding-skeleton-title-short" /></h1>
          <div className="onboarding-skeleton-description"><Skeleton /><Skeleton className="onboarding-skeleton-description-short" /></div>
          <div className="onboarding-callout onboarding-skeleton-callout"><Skeleton className="onboarding-skeleton-callout-icon" /><div><Skeleton className="onboarding-skeleton-callout-title" /><Skeleton className="onboarding-skeleton-callout-copy" /><Skeleton className="onboarding-skeleton-callout-copy onboarding-skeleton-callout-copy-short" /></div></div>
        </section>

        <section className="onboarding-workspace onboarding-skeleton-workspace" aria-hidden="true">
          <span className="onboarding-step-sticker onboarding-skeleton-sticker"><Skeleton /></span>
          <div className="onboarding-card onboarding-skeleton-card">
            <div className="onboarding-card-header">
              <Skeleton className="onboarding-skeleton-card-eyebrow" />
              <Skeleton className="onboarding-skeleton-card-title" />
              <div className="onboarding-progress onboarding-skeleton-progress">
                {Array.from({ length: 4 }, (_, index) => <div className="onboarding-progress-item" key={index}><Skeleton className="onboarding-progress-marker onboarding-skeleton-progress-marker" /><Skeleton className="onboarding-skeleton-progress-label" /><Skeleton className="onboarding-skeleton-progress-status" />{index < 3 && <span className="onboarding-progress-line" />}</div>)}
              </div>
            </div>
            <div className="onboarding-card-content">
              <div className="onboarding-active-step onboarding-skeleton-active-step">
                <div className="onboarding-active-heading"><Skeleton className="onboarding-skeleton-active-pill" /><Skeleton className="onboarding-skeleton-active-rule" /><Skeleton className="onboarding-skeleton-active-eyebrow" /></div>
                <Skeleton className="onboarding-skeleton-active-title" />
                <Skeleton className="onboarding-skeleton-active-description" />
                <div className="onboarding-form-divider" />
                <div className="onboarding-skeleton-fields"><div><Skeleton className="onboarding-skeleton-field-label" /><Skeleton className="onboarding-skeleton-field-input" /></div><div><Skeleton className="onboarding-skeleton-field-label" /><Skeleton className="onboarding-skeleton-field-input" /></div></div>
                <div className="onboarding-skeleton-actions"><Skeleton /><Skeleton className="onboarding-skeleton-action-primary" /></div>
              </div>
            </div>
          </div>
        </section>
      </div>
    </PageFrame>
  )
}
