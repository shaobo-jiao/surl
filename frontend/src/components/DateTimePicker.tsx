"use client"

import * as React from "react"
import { format } from "date-fns"
import { CalendarIcon, Clock } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Calendar } from "@/components/ui/calendar"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"
import { Card, CardContent, CardFooter } from "@/components/ui/card"
import { Field } from "@/components/ui/field"
import { InputGroup, InputGroupAddon, InputGroupInput } from "@/components/ui/input-group"

export interface DateTimePickerProps {
  value: Date | null,
  onChange: (value: Date | null) => void,
  placeholder?: string,
}

/**
 * Popover calendar + time input, driven by a value/onChange pair so it can be
 */
export default function DateTimePicker({
  value,
  onChange,
  placeholder = "Pick a date and time",
}: DateTimePickerProps) {
  const [open, setOpen] = React.useState(false)

  const handleDateSelect = (date?: Date) => {
    // handles date select: changing the date part and keeps the time, or clear the date
    if (!date) {
      onChange(null);
      return;
    }

    // Keep the time already chosen, otherwise default to 0
    const next = new Date(date);
    if (value) {
      next.setHours(value.getHours(), value.getMinutes());
    }
    onChange(next);
  }

  const handleTimeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const time = event.target.value;
    if (!time || !value) return;

    const [hours, minutes] = time.split(":").map(Number);
    const next = new Date(value);
    next.setHours(hours, minutes);
    onChange(next);
  }

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger
        render={
          <Button
            type="button"
            variant="outline"
            className="w-full justify-start font-normal"
          />
        }
      >
        <CalendarIcon />
        {value ? format(value, "PP p") : placeholder}
      </PopoverTrigger>

      <PopoverContent align="start" className="w-auto gap-0 p-0">
        <Card>
          <CardContent className="flex flex-col gap-2">
            <Calendar
              mode="single"
              selected={value ?? undefined}
              onSelect={handleDateSelect}
              defaultMonth={value ?? undefined}
              autoFocus
              className="p-0"
            />
            <Field>
              <InputGroup>
                <InputGroupAddon><Clock /></InputGroupAddon>
                <InputGroupInput
                  type="time"
                  step="60"
                  defaultValue={value?.toTimeString().slice(0, 5) ?? "00:00"}
                  className="appearance-none text-sm [&::-webkit-calendar-picker-indicator]:hidden [&::-webkit-calendar-picker-indicator]:appearance-none"
                  onChange={handleTimeChange}
                />
              </InputGroup>
            </Field>
          </CardContent>

          <CardFooter className="flex flex-row justify-end bg-card gap-1">
            <Button type="button" variant="outline" onClick={() => { onChange(null); setOpen(false) }}>Clear</Button>
            <Button type="button" onClick={() => setOpen(false)}>Done</Button>
          </CardFooter>
        </Card>
      </PopoverContent>
    </Popover>
  )
}
