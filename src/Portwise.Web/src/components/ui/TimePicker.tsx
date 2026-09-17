import { ChevronDown, Clock3 } from "lucide-react"
import { useMemo, useState } from "react"

import { Button } from "@/components/ui/Button"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/Popover"
import { Select, SelectContent, SelectGroup, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/Select"

type TimePickerProps = {
  id: string
  value: string
  label: string
  hourLabel: string
  minuteLabel: string
  onChange: (value: string) => void
  disabled?: boolean
}

const hours = Array.from({ length: 24 }, (_, value) => value.toString().padStart(2, "0"))
const minutes = Array.from({ length: 60 }, (_, value) => value.toString().padStart(2, "0"))

function parseTime(value: string) {
  const [rawHour, rawMinute] = value.split(":")
  const hour = hours.includes(rawHour) ? rawHour : "18"
  const minute = minutes.includes(rawMinute) ? rawMinute : "00"
  return { hour, minute }
}

export function TimePicker({ id, value, label, hourLabel, minuteLabel, onChange, disabled = false }: TimePickerProps) {
  const [open, setOpen] = useState(false)
  const { hour, minute } = useMemo(() => parseTime(value), [value])

  function updateTime(nextHour: string | null, nextMinute: string | null) {
    onChange(`${nextHour ?? hour}:${nextMinute ?? minute}`)
  }

  return <Popover open={open} onOpenChange={setOpen}>
    <PopoverTrigger render={<Button type="button" id={id} variant="outline" className="settings-time-picker-trigger" disabled={disabled} aria-label={label}>
      <Clock3 aria-hidden="true" />
      <span className="settings-time-picker-value">{value}</span>
      <ChevronDown aria-hidden="true" />
    </Button>} />
    <PopoverContent align="end" className="settings-time-picker-popover">
      <div className="settings-time-picker-heading">
        <span>{label}</span>
        <strong>{value}</strong>
      </div>
      <div className="settings-time-picker-fields">
        <div className="settings-time-picker-field">
          <label htmlFor={`${id}-hour`}>{hourLabel}</label>
          <Select value={hour} onValueChange={(nextHour) => updateTime(nextHour, null)}>
            <SelectTrigger id={`${id}-hour`} aria-label={hourLabel}><SelectValue /></SelectTrigger>
            <SelectContent><SelectGroup>{hours.map((option) => <SelectItem value={option} key={option}>{option}</SelectItem>)}</SelectGroup></SelectContent>
          </Select>
        </div>
        <div className="settings-time-picker-field">
          <label htmlFor={`${id}-minute`}>{minuteLabel}</label>
          <Select value={minute} onValueChange={(nextMinute) => updateTime(null, nextMinute)}>
            <SelectTrigger id={`${id}-minute`} aria-label={minuteLabel}><SelectValue /></SelectTrigger>
            <SelectContent><SelectGroup>{minutes.map((option) => <SelectItem value={option} key={option}>{option}</SelectItem>)}</SelectGroup></SelectContent>
          </Select>
        </div>
      </div>
    </PopoverContent>
  </Popover>
}
