import DateTimePicker from "@/components/common/DateTimePicker";
import { Button } from "@/components/ui/button";
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Spinner } from "@/components/ui/spinner";
import {
  Popover,
  PopoverContent,
  PopoverDescription,
  PopoverHeader,
  PopoverTitle,
  PopoverTrigger,
} from "@/components/ui/popover";
import { type QueryShortUrlsOrderBy, type QueryShortUrlsParams } from "@/api/shortUrls/queryShortUrls";
import { useState } from "react";
import { Select, SelectContent, SelectGroup, SelectItem, SelectTrigger, SelectValue } from "../ui/select";
import { Switch } from "../ui/switch";
import { SlidersHorizontal } from "lucide-react";

export type ShortUrlFilters = Required<QueryShortUrlsParams>;

export const DEFAULT_FILTERS: ShortUrlFilters = {
  expiresFrom: null,
  expiresTo: null,
  orderBy: "expiresAt",
  isDescending: true,
  page: 1,
  pageSize: 10,
};

// Editable form of the filters: page and page size are kept as raw strings so the
// user can freely edit the inputs. They are converted back to positive integers 
// when apply the draft
type ShortUrlFiltersDraft = Omit<ShortUrlFilters, "page" | "pageSize"> & {
  page: string;
  pageSize: string;
};

const toDraft = (filters: ShortUrlFilters): ShortUrlFiltersDraft => ({
  ...filters,
  page: String(filters.page),
  pageSize: String(filters.pageSize),
});

const ORDER_BY_OPTIONS: { value: QueryShortUrlsOrderBy | ""; label: string }[] = [
  // shadcn/ui select component recommend empty string instead of null value
  { value: "", label: "Select order key" }, 
  { value: "expiresAt", label: "Expiration date" },
  { value: "clickCount", label: "Click count" },
];

interface UrlFiltersPopoverProps {
  filters: ShortUrlFilters; // current filter used in and passed from UrlsPage
  onApply: (filters: ShortUrlFilters) => Promise<boolean>;
}

export default function UrlFiltersPopover({ filters, onApply }: UrlFiltersPopoverProps) {
  const [isOpen, setIsOpen] = useState<boolean>(false);
  const [isApplying, setIsApplying] = useState<boolean>(false);
  const [draft, setDraft] = useState<ShortUrlFiltersDraft>(() => toDraft(filters));

  // whenever the popover opens, start from the filters that are currently applied
  const handleOpenChange = (nextOpen: boolean) => {
    if (nextOpen)
      setDraft(toDraft(filters));
    setIsOpen(nextOpen);
  };

  const updateDraft = (patch: Partial<ShortUrlFiltersDraft>) => {
    setDraft((previousDraft) => ({ ...previousDraft, ...patch }));
  };

  const handleReset = () => {
    setDraft(toDraft(DEFAULT_FILTERS));
  }

  const handleApply = async () => {
    const next: ShortUrlFilters = {
      ...draft,
      // parse the input or fallback to current page or page size;
      page: Math.max(1, Number.parseInt(draft.page, 10) || filters.page),
      pageSize: Math.max(1, Number.parseInt(draft.pageSize, 10) || filters.pageSize),
    };
    
    setIsApplying(true);
    try {
      const success = await onApply(next);
      if (success) setIsOpen(false);
    } finally {
      setIsApplying(false);
    }
  };

  return (
    <Popover open={isOpen} onOpenChange={handleOpenChange}>
      <PopoverTrigger render={<Button type="button" variant="outline" />}>
        <SlidersHorizontal /> Filters
      </PopoverTrigger>

      <PopoverContent align="start" className="w-80 gap-4 p-4">
        <PopoverHeader>
          <PopoverTitle>Filters</PopoverTitle>
          <PopoverDescription>Refine the list of your short URLs.</PopoverDescription>
        </PopoverHeader>

        <FieldGroup className="gap-4">
          <Field>
            <FieldLabel htmlFor="expiresFrom">Expires from</FieldLabel>
            <DateTimePicker
              id="expiresFrom"
              value={draft.expiresFrom}
              onChange={(value) => updateDraft({ expiresFrom: value })}
              placeholder="Any start date"
            />
          </Field>

          <Field>
            <FieldLabel htmlFor="expiresTo">Expires to</FieldLabel>
            <DateTimePicker
              id="expiresTo"
              value={draft.expiresTo}
              onChange={(value) => updateDraft({ expiresTo: value })}
              placeholder="Any end date"
            />
          </Field>

          <Field>
            <FieldLabel htmlFor="orderBy">Order by</FieldLabel>
            <Select id="orderBy"
              items={ORDER_BY_OPTIONS}
              value={draft.orderBy ?? ""}
              onValueChange={value => {
                const orderBy = (value || null) as QueryShortUrlsOrderBy;
                updateDraft({ orderBy: orderBy })
              }}>
              <SelectTrigger>
                <SelectValue placeholder="Select sort key" />
              </SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  {ORDER_BY_OPTIONS.map(item => (
                    <SelectItem key={item.value} value={item.value}>
                      {item.label}
                    </SelectItem>))}
                </SelectGroup>
              </SelectContent>
            </Select>
          </Field>

          <Field orientation="horizontal">
            <FieldLabel htmlFor="isDescending">Descending</FieldLabel>
            <Switch id="isDescending" checked={draft.isDescending} onCheckedChange={
              checked => updateDraft({ isDescending: checked })
            } />
          </Field>

          <div className="grid grid-cols-2 gap-3">
            <Field>
              <FieldLabel htmlFor="pageSize">Page size</FieldLabel>
              <Input
                id="pageSize"
                type="number"
                min={1}
                step={1}
                value={draft.pageSize}
                onChange={(event) => updateDraft({ pageSize: event.target.value })}
              />
            </Field>

            <Field>
              <FieldLabel htmlFor="page">Page</FieldLabel>
              <Input
                id="page"
                type="number"
                min={1}
                step={1}
                value={draft.page}
                onChange={(event) => updateDraft({ page: event.target.value })}
              />
            </Field>
          </div>
        </FieldGroup>

        <div className="flex flex-row justify-end gap-2">
          <Button type="button" variant="outline" onClick={handleReset} disabled={isApplying}>Reset</Button>
          <Button type="button" onClick={handleApply} disabled={isApplying}>
            {isApplying ? <Spinner /> : null} Apply
          </Button>
        </div>
      </PopoverContent>
    </Popover>
  );
}
