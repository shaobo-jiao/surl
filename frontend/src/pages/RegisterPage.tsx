import { registerApi, type RegisterFields } from "@/api/auth/register";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Item, ItemActions, ItemContent, ItemDescription, ItemMedia, ItemTitle } from "@/components/ui/item";
import { Spinner } from "@/components/ui/spinner";
import { strongPasswordSchema } from "@/lib/passwordForm";
import { zodResolver } from "@hookform/resolvers/zod";
import { ChevronRight, CircleCheck, CircleX } from "lucide-react";
import { useState } from "react";
import { useForm, type SubmitHandler } from "react-hook-form";
import { Link } from "react-router";
import z from "zod";

const RegisterFormDataSchema = z
  .object({
    username: z.string().min(6).max(50)
      .regex(/^[a-zA-Z0-9._-]+$/, "Username can only contain letters, numbers, underscores, dots and hyphens"),
    password: strongPasswordSchema,
    confirmPassword: z.string(),
    email: z.email().optional().or(z.literal(''))
  })
  .refine(data => data.confirmPassword === data.password, {
    path: ["confirmPassword"],
    error: "Passwords don't match"
  });
type RegisterFormData = z.infer<typeof RegisterFormDataSchema>;

export default function RegisterPage() {
  const [isRegistered, setIsRegistered] = useState<boolean>(false);

  const {
    register,
    handleSubmit,
    setError,
    clearErrors,
    formState: { errors, isSubmitting }
  } = useForm({ resolver: zodResolver(RegisterFormDataSchema) });

  const handleRegisterSubmit: SubmitHandler<RegisterFormData> = async (data) => {
    setIsRegistered(false);
    clearErrors();

    const registerResult = await registerApi(data);
    if (registerResult.success) {
      setIsRegistered(true);
    }
    else {
      const registerError = registerResult.error;
      // set errors: server error and field errors
      setError("root.serverError", { type: "server", message: registerError.detail })
      if (registerError.errors !== undefined) {
        for (const [field, messages] of Object.entries(registerError.errors)) {
          // set field error using the first error message of each field
          setError(field as RegisterFields, { type: "server", message: messages[0] });
        }
      }
    }
  };

  return (
    <Card className="w-full max-w-2xl">
      <CardHeader>
        <CardTitle>Sign Up</CardTitle>
        <CardDescription>Enter your username and password below</CardDescription>
      </CardHeader>

      <CardContent>
        <form onSubmit={handleSubmit(handleRegisterSubmit)} className="flex flex-col gap-2">
          <Field>
            <FieldLabel htmlFor="username">Username*</FieldLabel>
            <Input id="username" type="text" {...register("username")} />
            {errors.username && <FieldError>{errors.username.message}</FieldError>}
          </Field>
          {/* Email field is commented until the backend supports mailing */}
          {/* <Field>
            <FieldLabel htmlFor="email">Email</FieldLabel>
            <Input id="email" type="text" {...register("email")} />
            {errors.email && <FieldError>{errors.email.message}</FieldError>}
          </Field> */}
          <Field>
            <FieldLabel htmlFor="password">Password*</FieldLabel>
            <Input id="password" type="password" {...register("password")} />
            {errors.password && <FieldError>{errors.password.message}</FieldError>}
          </Field>
          <Field>
            <FieldLabel htmlFor="confirmPassword">Confirm Password*</FieldLabel>
            <Input id="confirmPassword" type="password" {...register("confirmPassword")} />
            {errors.confirmPassword && <FieldError>{errors.confirmPassword.message}</FieldError>}
          </Field>
          <Field>
            {
              isSubmitting
                ? <Button disabled> <Spinner /> Signing Up...</Button>
                : <Button type="submit">Sign Up</Button>
            }
          </Field>
        </form>
      </CardContent>

      {isRegistered &&
        <CardFooter>
          <Item render={<Link to="/login" />} className="p-0">
            <ItemMedia><CircleCheck className="size-4" /></ItemMedia>
            <ItemContent><ItemTitle>Your registration is successful.</ItemTitle></ItemContent>
            <ItemActions><ChevronRight className="size-4" /></ItemActions>
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
