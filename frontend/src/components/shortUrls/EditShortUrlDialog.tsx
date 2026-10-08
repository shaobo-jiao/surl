import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Field, FieldDescription, FieldError, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import z from "zod";
import { useState } from "react";
import { Controller, useForm, type SubmitHandler } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Spinner } from "@/components/ui/spinner";
import DateTimePicker from "@/components/common/DateTimePicker";
import { toast } from "sonner";
import { SquarePen } from "lucide-react";
import { cn } from "@/lib/utils";
import type { ShortUrl } from "@/api/shortUrls/shortUrl.shared";
import { updateShortUrlApi, type UpdateShortUrlFields } from "@/api/shortUrls/updateShortUrl";


const EditShortUrlFormDataScheme = z.object({
  expiresAt: z.date()
    .min(new Date(), "The expiry date must be in the future.")
    .nullable(),
});
type EditShortUrlFormData = z.infer<typeof EditShortUrlFormDataScheme>

interface EditShortUrlDialogProps {
  shortUrl: ShortUrl;
  onUpdate: (shortUrl: ShortUrl) => void;
  // classes for the trigger button that are relevant to the parent, e.g. layout
  className?: string;
}

// edit icon button with its dialog; updates the short URL's details (currently only the expiry date)
export default function EditShortUrlDialog({ shortUrl, onUpdate, className }: EditShortUrlDialogProps) {
  const [isOpen, setIsOpen] = useState<boolean>(false);
  const {
    control,
    handleSubmit,
    reset,
    setError,
    clearErrors,
    formState: { errors, isSubmitting }
  } = useForm({ resolver: zodResolver(EditShortUrlFormDataScheme) });

  // seed the form with the current values whenever the dialog is opened
  const handleOpenChange = (open: boolean) => {
    if (open) reset({ expiresAt: shortUrl.expiresAt });
    setIsOpen(open);
  };

  const handleEditShortUrlSubmit: SubmitHandler<EditShortUrlFormData> = async (data) => {
    clearErrors();

    const updateResult = await updateShortUrlApi(shortUrl.id, {
      expiresAt: data.expiresAt
    });

    if (updateResult.success) {
      onUpdate(updateResult.data);
      reset();
      setIsOpen(false);
      toast.success("Short URL has been updated");
    } else {
      const updateError = updateResult.error;
      setError("root.serverError", { type: "server", message: updateError.detail })
      if (updateError.errors !== undefined) {
        for (const [field, messages] of Object.entries(updateError.errors)) {
          // set field error using the first error message of each field
          setError(field as UpdateShortUrlFields, { type: "server", message: messages[0] });
        }
      }
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={handleOpenChange}>
      <DialogTrigger render={<Button type="button"
        variant="ghost"
        size="icon"
        aria-label="Edit short URL"
        className={cn("text-muted-foreground", className)}>
        <SquarePen />
      </Button>} />
      <DialogContent className="sm:max-w-2xl">
        <form onSubmit={handleSubmit(handleEditShortUrlSubmit)} className="flex flex-col gap-4">
          <DialogHeader>
            <DialogTitle className="font-mono break-all mb-2">{shortUrl.absoluteUrl}</DialogTitle>
          </DialogHeader>

          {/* edit short url form inputs */}
          <FieldGroup>
            <Field>
              <FieldLabel htmlFor="originalUrl">Original URL</FieldLabel>
              <Input id="originalUrl" type="text" className="bg-muted" value={shortUrl.originalUrl} readOnly />
            </Field>
            <Field>
              <FieldLabel htmlFor="expiresAt">Expires at</FieldLabel>
              <Controller
                control={control}
                name="expiresAt"
                defaultValue={shortUrl.expiresAt}
                render={({ field: { value, onChange } }) => (
                  <DateTimePicker id="expiresAt" value={value} onChange={onChange} />
                )}
              />
              <FieldDescription>Optional. Leave empty for a link that never expires.</FieldDescription>
              {errors.expiresAt && <FieldError>{errors.expiresAt.message}</FieldError>}
            </Field>
          </FieldGroup>

          {errors.root?.serverError && <FieldError>{errors.root.serverError.message}</FieldError>}

          <DialogFooter>
            <DialogClose render={<Button type="button" variant="outline" />}>Cancel</DialogClose>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? <Spinner /> : null} Edit
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
};
