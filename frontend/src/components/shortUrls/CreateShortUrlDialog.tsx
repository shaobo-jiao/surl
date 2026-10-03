import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogFooter,
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
import DateTimePicker from "@/components/DateTimePicker";
import { useAuth } from "@/contexts/AuthContext";
import { createShortUrl, type CreateShortUrlFields, type ShortUrl } from "@/api/shortUrls/shortUrlApi";
import { toast } from "sonner";


const CreateShortUrlFormDataScheme = z.object({
  originalUrl: z.string().trim()
    .nonempty()
    .refine(value => {
      try {
        // validate URL is valid and is http or https
        const url = new URL(value);
        return url.protocol === "http:" || url.protocol === "https:";
      } catch {
        return false
      }
    }, "The URL is invalid"),
  expiresAt: z.date()
    .min(new Date(), "The expiry date must be in the future.")
    .nullable(),
});
type CreateShortUrlFormData = z.infer<typeof CreateShortUrlFormDataScheme>

interface CreateShortUrlDialogProps {
  onCreate: (shortUrl: ShortUrl) => void;
}

export default function CreateShortUrlDialog({ onCreate }: CreateShortUrlDialogProps) {
  const { userLogin } = useAuth();
  const [isOpen, setIsOpen] = useState<boolean>(false);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const {
    register,
    control,
    handleSubmit,
    reset,
    setError,
    formState: { errors }
  } = useForm({ resolver: zodResolver(CreateShortUrlFormDataScheme) });

  const handleCreateShortUrlSubmit: SubmitHandler<CreateShortUrlFormData> = async (data) => {
    setIsSubmitting(true);

    const apiResponse = await createShortUrl({
      originalUrl: data.originalUrl,
      expiresAt: data.expiresAt ?? undefined
    }, userLogin.accessToken!
    );

    if (apiResponse.success) {
      onCreate(apiResponse.data);
      reset();
      setIsOpen(false);
      toast.success("Short URL has been created");
    }
    else {
      const result = apiResponse.error;
      setError("root.serverError", { type: "server", message: result.detail })
      if (result.errors !== undefined) {
        for (const [field, messages] of Object.entries(result.errors)) {
          // set field error using the first error message of each field
          setError(field as CreateShortUrlFields, { type: "server", message: messages[0] });
        }
      }
    }

    setIsSubmitting(false);
  };

  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <DialogTrigger render={<Button size="lg" className="ml-auto" />}>Create New</DialogTrigger>
      <DialogContent className="sm:max-w-2xl">
        <form onSubmit={handleSubmit(handleCreateShortUrlSubmit)} className="flex flex-col gap-4">
          {/* create short url form inputs */}
          <FieldGroup>
            <Field>
              <FieldLabel htmlFor="originalUrl">Shorten your URL</FieldLabel>
              <Input id="originalUrl" type="text" placeholder="https://www.example.com" {...register("originalUrl")} />
              <FieldDescription>Paste the full link, including http:// or https://</FieldDescription>
              {errors.originalUrl && <FieldError>{errors.originalUrl.message}</FieldError>}
            </Field>
            <Field>
              <FieldLabel htmlFor="expiresAt">Expires at</FieldLabel>
              <Controller
                control={control}
                name="expiresAt"
                defaultValue={null}
                render={({ field: { value, onChange } }) => (
                  <DateTimePicker value={value} onChange={onChange} />
                )}
              />
              <FieldDescription>Optional. Leave empty for a link that never expires.</FieldDescription>
              {errors.expiresAt && <FieldError>{errors.expiresAt.message}</FieldError>}
            </Field>
          </FieldGroup>

          {errors.root?.serverError && <FieldError>{errors.root.serverError.message}</FieldError>}

          <DialogFooter>
            <DialogClose render={<Button variant="outline" />}>Cancel</DialogClose>
            {isSubmitting
              ? <Button disabled><Spinner /> Creating...</Button>
              : <Button type="submit">Create</Button>
            }
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
};
