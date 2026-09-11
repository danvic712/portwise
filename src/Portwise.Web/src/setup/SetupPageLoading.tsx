import { useLocale } from "@/shared/i18n/i18n"
import { BrandMark } from "@/components/brand/BrandMark"
import { Skeleton } from "@/components/ui/Skeleton"

import "./setup.css"

export function SetupPageLoading({ label }: { label: string }) {
  const { messages } = useLocale()
  const copy = messages.setup.ui

  return (
    <div className="setup-frame setup-loading-frame">
      <header className="setup-topbar">
        <div className="setup-brand-lockup" aria-label={messages.common.ui.brandName}>
          <BrandMark className="setup-brand-mark" />
          <span className="setup-brand-copy">
            <strong>{messages.common.ui.brandName}</strong>
            <small>{messages.common.ui.brandCaption}</small>
          </span>
        </div>
        <div className="setup-topbar-state" role="status">
          <span className="setup-topbar-dot" aria-hidden="true" />
          <span><strong>{copy.eyebrow}</strong><small>{messages.common.ui.aShareOnly}</small></span>
        </div>
      </header>

      <main className="page-wrap setup-wrap" aria-label={label} aria-live="polite" aria-busy="true">
        <div className="setup-page setup-skeleton-page" aria-hidden="true">
          <section className="setup-intro setup-skeleton-intro">
            <Skeleton className="setup-skeleton-eyebrow" />
            <Skeleton className="setup-skeleton-title" />
            <Skeleton className="setup-skeleton-title-short" />
            <Skeleton className="setup-skeleton-description" />
            <Skeleton className="setup-skeleton-description-short" />
            <div className="setup-skeleton-callout">
              <Skeleton className="setup-skeleton-callout-mark" />
              <div>
                <Skeleton className="setup-skeleton-callout-title" />
                <Skeleton className="setup-skeleton-callout-copy" />
              </div>
            </div>
          </section>

          <section className="setup-card setup-skeleton-card">
            <div className="setup-skeleton-card-heading">
              <div>
                <Skeleton className="setup-skeleton-kicker" />
                <Skeleton className="setup-skeleton-card-eyebrow" />
                <Skeleton className="setup-skeleton-card-title" />
                <Skeleton className="setup-skeleton-card-description" />
              </div>
              <span className="setup-card-mark setup-skeleton-card-mark" aria-hidden="true"><BrandMark /></span>
            </div>

            <div className="setup-skeleton-field">
              <Skeleton className="setup-skeleton-label" />
              <Skeleton className="setup-skeleton-input" />
            </div>

            <div className="setup-skeleton-section-heading">
              <div><Skeleton className="setup-skeleton-section-number" /><div><Skeleton className="setup-skeleton-section-title" /><Skeleton className="setup-skeleton-section-description" /></div></div>
              <Skeleton className="setup-skeleton-count" />
            </div>

            <div className="setup-skeleton-stock-row">
              <Skeleton className="setup-skeleton-stock-title" />
              <div className="setup-skeleton-stock-fields">
                <div><Skeleton className="setup-skeleton-label" /><Skeleton className="setup-skeleton-input" /></div>
                <div><Skeleton className="setup-skeleton-label" /><Skeleton className="setup-skeleton-input" /></div>
                <Skeleton className="setup-skeleton-remove" />
              </div>
              <Skeleton className="setup-skeleton-holding" />
            </div>

            <Skeleton className="setup-skeleton-add" />
            <div className="setup-skeleton-submit-row"><Skeleton className="setup-skeleton-submit" /><Skeleton className="setup-skeleton-submit-hint" /></div>
            <Skeleton className="setup-skeleton-footnote" />
          </section>
        </div>
      </main>

      <footer className="setup-footer"><span>{messages.common.ui.disclaimer}</span><span><i />{messages.common.ui.aShareOnly}</span></footer>
    </div>
  )
}
