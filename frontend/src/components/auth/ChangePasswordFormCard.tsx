import { changePasswordApi, type ChangePasswordFields } from "@/api/auth/changePassword";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Item, ItemContent, ItemDescription, ItemMedia, ItemTitle } from "@/components/ui/item";
import { Spinner } from "@/components/ui/spinner";
import { passwordSchema, strongPasswordSchema } from "@/lib/passwordForm";
import { zodResolver } from "@hookform/resolvers/zod";
import { CircleCheck, CircleX } from "lucide-react";
import { useState } from "react";
import { useForm, type SubmitHandler } from "react-hook-form";
import z from "zod";

const ChangePasswordFormDataSchema = z
  .object({
    oldPassword: passwordSchema,
    newPassword: strongPasswordSchema,
    confirmNewPassword: z.string()
  })
  .refine(data => data.confirmNewPassword === data.newPassword, {
    path: ["confirmNewPassword"],
    error: "Passwords don't match"
  });
type ChangePasswordFormData = z.infer<typeof ChangePasswordFormDataSchema>;

export default function ChangePasswordFormCard() {
  const [isPasswordChanged, setIsPasswordChanged] = useState<boolean>(false);

  const {
    register,
    handleSubmit,
    reset,
    setError,
    clearErrors,
    formState: { errors, isSubmitting }
  } = useForm({ resolver: zodResolver(ChangePasswordFormDataSchema) });

  const handleChangePasswordSubmit: SubmitHandler<ChangePasswordFormData> = async (data) => {
    setIsPasswordChanged(false);
    clearErrors();

    const changePasswordResult = await changePasswordApi({
      oldPassword: data.oldPassword,
      newPassword: data.newPassword
    });

    if (changePasswordResult.success) {
      reset();
      setIsPasswordChanged(true);
      return;
    }

    const changePasswordError = changePasswordResult.error;
    // set errors: server error and field errors
    setError("root.serverError", { type: "server", message: changePasswordError.detail });
    if (changePasswordError.errors !== undefined) {
      for (const [field, messages] of Object.entries(changePasswordError.errors)) {
        // set field error using the first error message of each field
        setError(field as ChangePasswordFields, { type: "server", message: messages[0] });
      }
    }
  };

  return (
    <Card className="w-full max-w-2xl">
      <CardHeader>
        <CardTitle>Change Password</CardTitle>
        <CardDescription>Enter your old password and change to a new one below</CardDescription>
      </CardHeader>

      <CardContent>
        <form onSubmit={handleSubmit(handleChangePasswordSubmit)} className="flex flex-col gap-2">
          <Field>
            <FieldLabel htmlFor="oldPassword">Old Password*</FieldLabel>
            <Input id="oldPassword" type="password" {...register("oldPassword")} />
            {errors.oldPassword && <FieldError>{errors.oldPassword.message}</FieldError>}
          </Field>
          <Field>
            <FieldLabel htmlFor="newPassword">New Password*</FieldLabel>
            <Input id="newPassword" type="password" {...register("newPassword")} />
            {errors.newPassword && <FieldError>{errors.newPassword.message}</FieldError>}
          </Field>
          <Field>
            <FieldLabel htmlFor="confirmNewPassword">Confirm New Password*</FieldLabel>
            <Input id="confirmNewPassword" type="password" {...register("confirmNewPassword")} />
            {errors.confirmNewPassword && <FieldError>{errors.confirmNewPassword.message}</FieldError>}
          </Field>
          <Field>
            {
              isSubmitting
                ? <Button disabled> <Spinner /> Changing Password...</Button>
                : <Button type="submit">Change Password</Button>
            }
          </Field>
        </form>
      </CardContent>

      {isPasswordChanged &&
        <CardFooter>
          <Item className="p-0">
            <ItemMedia><CircleCheck className="size-4" /></ItemMedia>
            <ItemContent><ItemTitle>Your password has been changed.</ItemTitle></ItemContent>
          </Item>
        </CardFooter>
      }

      {errors.root?.serverError &&
        <CardFooter>
          <Item className="p-0">
            <ItemMedia><CircleX className="size-4" /></ItemMedia>
            <ItemContent>
              <ItemTitle>Something went wrong.</ItemTitle>
              <ItemDescription>{errors.root.serverError.message}</ItemDescription>
            </ItemContent>
          </Item>
        </CardFooter>
      }
    </Card>
  );
}
