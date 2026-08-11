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
import { useNavigate } from "react-router";
import { useAuth } from "@/contexts/AuthContext";


const LoginFormDataSchema = z.object({
  username: z.string().min(6).max(50)
    .regex(/^[a-zA-Z0-9._-]+$/, "Username can only contain letters, numbers, underscores, dots and hyphens"),
  password: z.string().min(1).max(50)
    .regex(/^\S+$/, "Password must not contain whitespaces.") // simply check no whitespace
});
type LoginFormData = z.infer<typeof LoginFormDataSchema>;

export default function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuth();

  const [isLogging, setIsLogging] = useState<boolean>(false);
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors }
  } = useForm({ resolver: zodResolver(LoginFormDataSchema) });

  const onSubmit: SubmitHandler<LoginFormData> = async (data) => {
    setIsLogging(true);

    const success = await login(data.username, data.password);
    if (success) {
      navigate("/urls");
    }
    if (!success) {
      // set a general error message for login failure;
      setError("root.serverError", {
        type: "server",
        message: "The username or password is incorrect."
      });
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
        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4" >
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