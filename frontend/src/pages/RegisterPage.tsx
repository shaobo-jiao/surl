import * as authApi from "@/api/authApi";
import { Button } from "@/components/ui/button";
import { Card, CardAction, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Item, ItemActions, ItemContent, ItemMedia, ItemTitle } from "@/components/ui/item";
import { CircleCheck } from "lucide-react";
import { useState } from "react";
import { useForm, type SubmitHandler } from "react-hook-form";


interface RegisterFormData {
  username: string,
  password: string,
  confirmPassword: string,
  email: string
}

export default function RegisterPage() {
  const [success, setSuccess] = useState<boolean>(false);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors }
  } = useForm<RegisterFormData>(); // todo: add client validation with zod

  const onSubmit: SubmitHandler<RegisterFormData> = async (data) => {
    // handle submit after validation
    console.log(data);

    // api call
    const apiResponse = await authApi.register(data);
    if (apiResponse.success) {
      setSuccess(true);
      const result = apiResponse.data;
      console.log(result);
    }
    else {
      const result = apiResponse.error;
      // set server error title
      setError("root.serverError", { type: "server", message: result.title })
      if (result.errors !== undefined) {
        for (const [field, messages] of Object.entries(result.errors)) {
          // set field error using the first error message of each field
          setError(field as authApi.RegisterFields, { type: "server", message: messages[0] });
        }
      }
      console.log(result);
    }
  };


  return (
    <Card className="w-full max-w-2xl rounded-md">
      <CardHeader>
        <CardTitle>Sign Up</CardTitle>
        <CardDescription>Enter your username and password to sign up</CardDescription>
        <CardAction><Button variant="link">Sign In</Button></CardAction>
      </CardHeader>

      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)}>
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
            <Button type="submit" className="w-full">Submit</Button>
          </Field>
        </form>
      </CardContent>

      <CardFooter>
        {success &&
          <Item asChild>
            <a href="/login">
              <ItemMedia><CircleCheck /></ItemMedia>
              <ItemContent><ItemTitle>Your registration is successful.</ItemTitle></ItemContent>
              <ItemActions></ItemActions>
            </a>
          </Item>}
        {errors.root?.serverError && <FieldError>{errors.root.serverError.message}</FieldError>}
      </CardFooter>
    </Card>
  );
}