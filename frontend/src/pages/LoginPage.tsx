import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Item, ItemContent, ItemDescription, ItemMedia, ItemTitle } from "@/components/ui/item";
import { Spinner } from "@/components/ui/spinner";
import { zodResolver } from "@hookform/resolvers/zod";
import { CircleX } from "lucide-react";
import { useState } from "react";
import { useForm, type SubmitHandler } from "react-hook-form";
import z from "zod";
import { useLocation, useNavigate } from "react-router";
import { useAuth } from "@/hooks/useAuth";
import type { LoginFields } from "@/api/auth/login";
import { passwordSchema } from "@/lib/passwordForm";

const LoginFormDataSchema = z.object({
  username: z.string().min(6).max(50)
    .regex(/^[a-zA-Z0-9._-]+$/, "Username can only contain letters, numbers, underscores, dots and hyphens"),
  password: passwordSchema
});
type LoginFormData = z.infer<typeof LoginFormDataSchema>;

export default function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { login } = useAuth();
  const [isLogging, setIsLogging] = useState<boolean>(false);
  
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors }
  } = useForm({ resolver: zodResolver(LoginFormDataSchema) });

  const handleLoginSubmit: SubmitHandler<LoginFormData> = async (data) => {
    setIsLogging(true);

    const loginResult = await login(data);
    if (loginResult.success) {
      // login success, redirect to previous page;
      navigate(location.state?.from ?? "/urls");
    } else {
      // login failure, display errors
      const loginError = loginResult.error;
      setError("root.serverError", { type: "server", message: loginError.detail })
      if (loginError.errors !== undefined) {
        for (const [field, messages] of Object.entries(loginError.errors)) {
          // set field error using the first error message of each field
          setError(field as LoginFields, { type: "server", message: messages[0] });
        }
      }
    }

    setIsLogging(false);
  };

  return (
    <Card className="w-full max-w-2xl">
      <CardHeader>
        <CardTitle>Log In</CardTitle>
        <CardDescription>Enter your username and password</CardDescription>
      </CardHeader>

      <CardContent>
        <form onSubmit={handleSubmit(handleLoginSubmit)} className="flex flex-col gap-4" >
          <Field>
            <FieldLabel htmlFor="username">Username</FieldLabel>
            <Input id="username" type="text" {...register("username")}></Input>
            {errors.username && <FieldError>{errors.username.message}</FieldError>}
          </Field>
          <Field>
            <FieldLabel htmlFor="password">Password</FieldLabel>
            <Input id="password" type="password" {...register("password")}></Input>
            {errors.password && <FieldError>{errors.password.message}</FieldError>}
          </Field>
          <Field>
            {
              isLogging
                ? <Button disabled> <Spinner /> Logging In...</Button>
                : <Button type="submit">Log In</Button>
            }
          </Field>
        </form>
      </CardContent>

      {/* on successful login, will redirect to URLs page. so no successful footer here */}

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
  )
} 
