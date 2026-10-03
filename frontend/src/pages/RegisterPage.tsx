import * as registerApi from "@/api/auth/register";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Item, ItemActions, ItemContent, ItemDescription, ItemMedia, ItemTitle } from "@/components/ui/item";
import { Spinner } from "@/components/ui/spinner";
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
    password: z.string().min(12).max(50)
      .regex(/[A-Z]/, "Password must contain an uppercase letter.")
      .regex(/[a-z]/, "Password must contain a lowercase letter.")
      .regex(/[0-9]/, "Password must contain a number.")
      .regex(/[^a-zA-Z0-9]/, "Password must contain a special character.")
      .regex(/^\S+$/, "Password must not contain whitespaces."),
    confirmPassword: z.string(),
    email: z.email().optional().or(z.literal(''))
  })
  .refine(data => data.confirmPassword === data.password, {
    path: ["confirmPassword"],
    error: "Passwords don't match"
  });
type RegisterFormData = z.infer<typeof RegisterFormDataSchema>;

type Status = "editing" | "submitting" | "success";

export default function RegisterPage() {
  const [status, setStatus] = useState<Status>("editing");

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors }
  } = useForm({ resolver: zodResolver(RegisterFormDataSchema) });


  const handleRegisterSubmit: SubmitHandler<RegisterFormData> = async (data) => {
    setStatus("submitting");

    // api call
    const apiResponse = await registerApi.register(data);
    if (apiResponse.success) {
      setStatus("success");
    }
    else {
      setStatus("editing");
      const result = apiResponse.error;
      // set errors: server error and field errors
      setError("root.serverError", { type: "server", message: result.detail })
      if (result.errors !== undefined) {
        for (const [field, messages] of Object.entries(result.errors)) {
          // set field error using the first error message of each field
          setError(field as registerApi.RegisterFields, { type: "server", message: messages[0] });
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
          <Field>
            <FieldLabel htmlFor="email">Email</FieldLabel>
            <Input id="email" type="text" {...register("email")} />
            {errors.email && <FieldError>{errors.email.message}</FieldError>}
          </Field>
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
              status === "submitting"
                ? <Button disabled> <Spinner /> Signing Up...</Button>
                : <Button type="submit">Sign Up</Button>
            }
          </Field>
        </form>
      </CardContent>

      {status === "success" &&
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
