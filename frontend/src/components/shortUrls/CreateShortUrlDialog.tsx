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
import { Spinner } from "../ui/spinner";
import DateTimePicker from "../DateTimePicker";


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


export default function CreateShortUrlDialog() {
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const {
    register,
    control,
    handleSubmit,
    setError,
    formState: { errors }
  } = useForm({ resolver: zodResolver(CreateShortUrlFormDataScheme) });

  const onSubmit: SubmitHandler<CreateShortUrlFormData> = async (data) => {
    setIsSubmitting(true);

    // call API to create and get response; parse error or result and then handle callback

    setTimeout(() => {
      setIsSubmitting(false);
    }, 2000);
    // setIsSubmitting(false);
  };

  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button size="lg" className="ml-auto">Create New</Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-2xl">
        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
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

          <DialogFooter>
            <DialogClose asChild>
              <Button variant="outline">Cancel</Button>
            </DialogClose>
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
