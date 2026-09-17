import { CalendarClock, Plus, Save, Trash2 } from "lucide-react"
import { useCallback, useEffect, useRef, useState } from "react"

import { useActionFeedback } from "@/app/providers/ActionFeedbackContext"
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/Alert"
import { Badge } from "@/components/ui/Badge"
import { Button } from "@/components/ui/Button"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/Card"
import { Field, FieldContent, FieldDescription, FieldGroup, FieldLabel } from "@/components/ui/Field"
import { Select, SelectContent, SelectGroup, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/Select"
import { Switch } from "@/components/ui/switch"
import { TimePicker } from "@/components/ui/TimePicker"
import { getApiErrorMessage } from "@/shared/http/api-errors"
import { isRequestAborted, useLatestRequest } from "@/shared/hooks/useLatestRequest"
import { useLocale } from "@/shared/i18n/i18n"
import { formatDateTime } from "@/shared/utils/utils"
import type { StockDataSyncSettingsResponse } from "@/shared/http/api-types"
import { getStockDataSyncSettings, updateStockDataSyncSettings } from "@/settings/stock-data-sync.api"
import { StockDataSyncScheduleSkeleton } from "@/settings/SettingsSkeleton"

type ScheduleDraft = {
  enabled: boolean
  timeZoneId: string
  runTimes: string[]
}

const timeZoneIds = ["Asia/Shanghai", "Asia/Tokyo", "UTC", "Europe/London", "America/New_York"] as const

type ZonedDateTimeParts = {
  year: number
  month: number
  day: number
  hour: number
  minute: number
  second: number
}

function getZonedDateTimeParts(value: Date, timeZoneId: string): ZonedDateTimeParts | null {
  try {
    const parts = new Intl.DateTimeFormat("en-US", {
      calendar: "iso8601",
      day: "2-digit",
      hour: "2-digit",
      hourCycle: "h23",
      minute: "2-digit",
      month: "2-digit",
      numberingSystem: "latn",
      second: "2-digit",
      timeZone: timeZoneId,
      year: "numeric",
    }).formatToParts(value)
    const values = Object.fromEntries(parts.filter((part) => part.type !== "literal").map((part) => [part.type, Number(part.value)]))
    if (["year", "month", "day", "hour", "minute", "second"].some((part) => !Number.isFinite(values[part]))) return null
    return values as ZonedDateTimeParts
  } catch {
    return null
  }
}

function addDays(value: ZonedDateTimeParts, days: number): ZonedDateTimeParts {
  const date = new Date(Date.UTC(value.year, value.month - 1, value.day + days))
  return { year: date.getUTCFullYear(), month: date.getUTCMonth() + 1, day: date.getUTCDate(), hour: 0, minute: 0, second: 0 }
}

function isWeekend(value: ZonedDateTimeParts) {
  const day = new Date(Date.UTC(value.year, value.month - 1, value.day)).getUTCDay()
  return day === 0 || day === 6
}

function toUtcDate(value: ZonedDateTimeParts, timeZoneId: string): Date | null {
  const utcGuess = Date.UTC(value.year, value.month - 1, value.day, value.hour, value.minute, value.second)
  const guessParts = getZonedDateTimeParts(new Date(utcGuess), timeZoneId)
  if (!guessParts) return null
  const localGuess = Date.UTC(guessParts.year, guessParts.month - 1, guessParts.day, guessParts.hour, guessParts.minute, guessParts.second)
  const result = new Date(utcGuess - (localGuess - utcGuess))
  const resultParts = getZonedDateTimeParts(result, timeZoneId)
  if (!resultParts || ["year", "month", "day", "hour", "minute", "second"].some((part) => resultParts[part as keyof ZonedDateTimeParts] !== value[part as keyof ZonedDateTimeParts])) return null
  return result
}

function getNextRunPreview(now: Date, runTimes: readonly string[], timeZoneId: string): Date | null {
  const localNow = getZonedDateTimeParts(now, timeZoneId)
  const candidates = runTimes
    .map((value) => /^(?<hour>[01]\d|2[0-3]):(?<minute>[0-5]\d)$/.exec(value.trim()))
    .filter((match): match is RegExpExecArray & { groups: { hour: string; minute: string } } => Boolean(match?.groups))
    .map((match) => ({ hour: Number(match.groups.hour), minute: Number(match.groups.minute) }))
    .sort((left, right) => left.hour * 60 + left.minute - (right.hour * 60 + right.minute))
  if (!localNow || candidates.length === 0) return null

  for (let daysAhead = 0; daysAhead < 370; daysAhead += 1) {
    const date = addDays(localNow, daysAhead)
    if (isWeekend(date)) continue
    for (const candidate of candidates) {
      if (daysAhead === 0 && candidate.hour * 60 + candidate.minute <= localNow.hour * 60 + localNow.minute) continue
      const result = toUtcDate({ ...date, ...candidate }, timeZoneId)
      if (result && result.getTime() > now.getTime()) return result
    }
  }
  return null
}

export function StockDataSyncSchedule({ onUpdated }: { onUpdated?: () => void }) {
  const { messages } = useLocale()
  const copy = messages.settings.ui.syncSchedule
  const [settings, setSettings] = useState<StockDataSyncSettingsResponse | null>(null)
  const [draft, setDraft] = useState<ScheduleDraft | null>(null)
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [now, setNow] = useState(() => new Date())
  const feedbackRef = useRef<HTMLDivElement>(null)
  const { begin } = useLatestRequest()
  const { showFeedback } = useActionFeedback()

  useEffect(() => {
    if (error) feedbackRef.current?.scrollIntoView({ behavior: "smooth", block: "start" })
  }, [error])

  useEffect(() => {
    const intervalId = window.setInterval(() => setNow(new Date()), 30_000)
    return () => window.clearInterval(intervalId)
  }, [])

  const load = useCallback(() => {
    const request = begin()
    setLoading(true)
    setError(null)
    void getStockDataSyncSettings(request.signal)
      .then((result) => {
        if (!request.isCurrent()) return
        setSettings(result)
        setDraft({ enabled: result.enabled, timeZoneId: result.timeZoneId, runTimes: [...result.runTimes] })
        setLoading(false)
      })
      .catch((loadError) => {
        if (request.isCurrent() && !isRequestAborted(loadError, request.signal)) {
          setError(getApiErrorMessage(loadError, copy.error, messages.common.ui.errors))
          setLoading(false)
        }
      })
  }, [begin, copy.error, messages.common.ui.errors])

  useEffect(() => {
    const timeoutId = window.setTimeout(load, 0)
    return () => window.clearTimeout(timeoutId)
  }, [load])

  function updateRunTime(index: number, value: string) {
    setDraft((current) => current ? { ...current, runTimes: current.runTimes.map((time, itemIndex) => itemIndex === index ? value : time) } : current)
  }

  function addRunTime() {
    setDraft((current) => current && current.runTimes.length < 8 ? { ...current, runTimes: [...current.runTimes, "18:00"] } : current)
  }

  function removeRunTime(index: number) {
    setDraft((current) => current && current.runTimes.length > 1 ? { ...current, runTimes: current.runTimes.filter((_, itemIndex) => itemIndex !== index) } : current)
  }

  const nextRunAtUtc = draft?.enabled ? getNextRunPreview(now, draft.runTimes, draft.timeZoneId) : null
  const nextRunLabel = !draft?.enabled
    ? copy.disabledHint
    : nextRunAtUtc
      ? formatDateTime(nextRunAtUtc.toISOString())
      : copy.invalidTime

  async function save() {
    if (!settings || !draft) return
    const normalizedTimes = draft.runTimes.map((time) => time.trim()).filter(Boolean)
    if (normalizedTimes.length === 0 || normalizedTimes.some((time) => !/^([01]\d|2[0-3]):[0-5]\d$/.test(time))) {
      setError(copy.invalidTime)
      return
    }
    if (new Set(normalizedTimes).size !== normalizedTimes.length) {
      setError(copy.duplicateTime)
      return
    }

    setBusy(true)
    setError(null)
    try {
      const result = await updateStockDataSyncSettings({
        enabled: draft.enabled,
        timeZoneId: draft.timeZoneId,
        runTimes: normalizedTimes,
        expectedRevision: settings.revision,
      })
      setSettings(result)
      setDraft({ enabled: result.enabled, timeZoneId: result.timeZoneId, runTimes: [...result.runTimes] })
      showFeedback({ message: copy.saved })
      onUpdated?.()
    } catch (saveError) {
      setError(getApiErrorMessage(saveError, copy.error, messages.common.ui.errors))
    } finally {
      setBusy(false)
    }
  }

  return <div className="settings-sync-schedule-wrap">
    {error && <Alert ref={feedbackRef} variant="destructive" className="settings-subpage-feedback settings-subpage-feedback-error"><AlertTitle>{copy.errorTitle}</AlertTitle><AlertDescription>{error}</AlertDescription></Alert>}
    <Card className="settings-subpage-card settings-sync-schedule-card">
      <CardHeader>
        <div>
          <p className="settings-form-kicker">{copy.eyebrow}</p>
          <CardTitle>{copy.title}</CardTitle>
          <CardDescription>{copy.description}</CardDescription>
        </div>
        {draft && <Badge variant={draft.enabled ? "accent" : "outline"}>{draft.enabled ? copy.enabled : copy.disabled}</Badge>}
      </CardHeader>
      {loading && !draft ? <StockDataSyncScheduleSkeleton wrapped={false} includeHeader={false} /> : !draft ? <CardContent><div className="settings-load-error"><p>{copy.loadFailed}</p><Button type="button" variant="outline" onClick={load}>{copy.retry}</Button></div></CardContent> : <>
        <CardContent>
          <div className="settings-sync-schedule-layout">
            <div className="settings-sync-schedule-controls">
              <div className="settings-sync-schedule-grid">
                <Field orientation="horizontal" className="settings-sync-schedule-toggle">
                  <Switch id="stock-sync-enabled" checked={draft.enabled} onCheckedChange={(checked) => setDraft((current) => current ? { ...current, enabled: checked } : current)} aria-label={copy.enabledLabel} />
                  <FieldContent>
                    <FieldLabel htmlFor="stock-sync-enabled">{copy.enabledLabel}</FieldLabel>
                    <FieldDescription>{copy.enabledHint}</FieldDescription>
                  </FieldContent>
                </Field>
                <Field className="settings-sync-timezone-field">
                  <FieldLabel htmlFor="stock-sync-timezone">{copy.timeZoneLabel}</FieldLabel>
                  <Select value={draft.timeZoneId} onValueChange={(value) => value && setDraft((current) => current ? { ...current, timeZoneId: value } : current)}>
                    <SelectTrigger id="stock-sync-timezone" aria-label={copy.timeZoneLabel}><SelectValue>{copy.timeZones[draft.timeZoneId as keyof typeof copy.timeZones] ?? draft.timeZoneId}</SelectValue></SelectTrigger>
                    <SelectContent><SelectGroup>{timeZoneIds.map((timeZoneId) => <SelectItem value={timeZoneId} key={timeZoneId}>{copy.timeZones[timeZoneId]}</SelectItem>)}</SelectGroup></SelectContent>
                  </Select>
                  <FieldDescription>{copy.timeZoneHint}</FieldDescription>
                </Field>
              </div>
            </div>
            <div className="settings-sync-times">
              <div className="settings-sync-times-heading"><div><strong>{copy.runTimesLabel}</strong><p>{copy.runTimesHint}</p></div><span>{draft.runTimes.length}/8</span></div>
              <FieldGroup className={`settings-sync-time-list${draft.runTimes.length === 1 ? " settings-sync-time-list-single" : ""}`}>
                {draft.runTimes.map((runTime, index) => <Field orientation="horizontal" className="settings-sync-time-row" key={index}>
                  <FieldLabel htmlFor={`stock-sync-time-${index}`}>{copy.timeLabel} {index + 1}</FieldLabel>
                  <TimePicker id={`stock-sync-time-${index}`} value={runTime} label={`${copy.timeLabel} ${index + 1}`} hourLabel={copy.hourLabel} minuteLabel={copy.minuteLabel} onChange={(value) => updateRunTime(index, value)} disabled={busy} />
                  <Button type="button" size="icon" variant="ghost" aria-label={copy.removeTime} onClick={() => removeRunTime(index)} disabled={draft.runTimes.length <= 1 || busy}><Trash2 size={15} aria-hidden="true" /></Button>
                </Field>)}
              </FieldGroup>
              <Button type="button" variant="outline" size="sm" onClick={addRunTime} disabled={draft.runTimes.length >= 8 || busy}><Plus data-icon="inline-start" />{copy.addTime}</Button>
            </div>
          </div>
          <div className="settings-sync-next-run" role="status" aria-live="polite">
            <span className="settings-sync-next-run-icon"><CalendarClock size={16} aria-hidden="true" /></span>
            <div className="settings-sync-next-run-copy">
              <div className="settings-sync-next-run-primary">
                <span>{copy.nextRun}</span>
                <strong>{nextRunLabel}</strong>
              </div>
              <div className="settings-sync-configured-times" aria-label={copy.runTimesLabel}>
                <span>{copy.runTimesLabel}</span>
                <div className="settings-sync-configured-time-list">
                  {[...draft.runTimes].sort().map((runTime, index) => <span className="settings-sync-configured-time" key={`${runTime}-${index}`}>{runTime}</span>)}
                </div>
              </div>
            </div>
          </div>
        </CardContent>
        <CardFooter><Button onClick={() => void save()} disabled={busy}><Save data-icon="inline-start" />{busy ? copy.saving : copy.save}</Button></CardFooter>
      </>}
    </Card>
  </div>
}
