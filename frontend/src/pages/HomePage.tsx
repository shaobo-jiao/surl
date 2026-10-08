import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Item, ItemActions, ItemContent, ItemDescription, ItemMedia, ItemTitle } from "@/components/ui/item";
import { CalendarClock, CircleCheck, CircleX, Link2, MousePointerClick } from "lucide-react";
import { Spinner } from "@/components/ui/spinner";
import CopyUrlButton from "@/components/shortUrls/CopyUrlButton";
import { createAnonymousShortUrlApi } from "@/api/shortUrls/createShortUrl";
import type { ShortUrl } from "@/api/shortUrls/shortUrl.shared";
import { originalUrlSchema } from "@/lib/shortUrlForm";
import z from "zod";
import { useState } from "react";
import { useForm, type SubmitHandler } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { format } from "date-fns";

// HomePage's anonymous "Try It Out" form only needs the URL
// expiry date defaults to the backend setting
const ShortenUrlFormDataScheme = z.object({
  originalUrl: originalUrlSchema,
});
type ShortenUrlFormData = z.infer<typeof ShortenUrlFormDataScheme>;

// highlights to explain what the app offers
const HIGHLIGHTS = [
  {
    icon: Link2,
    title: "Short links",
    description: "Turn any long URL into a clean link you can share anywhere.",
  },
  {
    icon: MousePointerClick,
    title: "Click tracking",
    description: "See how many times each of your links gets opened.",
  },
  {
    icon: CalendarClock,
    title: "Optional expiry",
    description: "Set links to expire on their own when you need them to.",
  },
];

// HomePage: contains a hero section, feature highlights and an anonymous short URL form
const HomePage = () => {
  const [createdShortUrl, setCreatedShortUrl] = useState<ShortUrl | null>(null);
  const {
    register,
    handleSubmit,
    reset,
    clearErrors,
    setError,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(ShortenUrlFormDataScheme),
    defaultValues: { originalUrl: "" },
  });

  const handleShortenSubmit: SubmitHandler<ShortenUrlFormData> = async (data) => {
    setCreatedShortUrl(null);
    clearErrors();

    const apiResponse = await createAnonymousShortUrlApi({ originalUrl: data.originalUrl });
    if (apiResponse.success) {
      setCreatedShortUrl(apiResponse.data);
      reset();
    } else {
      const createError = apiResponse.error;
      setError("root.serverError", { type: "server", message: createError.detail });
      if (createError.errors !== undefined) {
        for (const [field, messages] of Object.entries(createError.errors)) {
          // set field error using the first error message of each field
          setError(field as keyof ShortenUrlFormData, { type: "server", message: messages[0] });
        }
      }
    }
  };

  return (
    <div className="flex flex-col items-center w-full max-w-5xl gap-10 py-10">
      {/* hero section */}
      <section className="flex flex-col items-center gap-5 text-center max-w-2xl">
        <h1 className="font-bold text-4xl tracking-tight sm:text-5xl">
          Shorten. Manage. Share.
        </h1>
        <p className="text-lg text-muted-foreground">
          Turn long, messy links into short ones you can share anywhere.
          Create an account to save, track and manage them all in one place.
        </p>
      </section>

      {/* feature highlights */}
      <div className="grid w-full max-w-2xl gap-4 sm:grid-cols-3">
        {HIGHLIGHTS.map(({ icon: Icon, title, description }) => (
          <Item key={title} variant="muted">
            <ItemMedia variant="icon"><Icon /></ItemMedia>
            <ItemContent>
              <ItemTitle>{title}</ItemTitle>
              <ItemDescription className="line-clamp-none">{description}</ItemDescription>
            </ItemContent>
          </Item>
        ))}
      </div>

      {/* anonymous "Try It Out" form */}
      <Card className="w-full max-w-2xl">
        <CardHeader>
          <CardTitle>Try It Out</CardTitle>
          <CardDescription>Paste a link below to shorten it instantly. No account needed.</CardDescription>
        </CardHeader>

        <CardContent>
          <form onSubmit={handleSubmit(handleShortenSubmit)}>
            <Field>
              <FieldLabel htmlFor="originalUrl" className="sr-only">URL to shorten</FieldLabel>
              <Field orientation="horizontal">
                <Input id="originalUrl" type="text" placeholder="https://example.com"
                  {...register("originalUrl")} />
                <Button type="submit" variant="outline" disabled={isSubmitting}>
                  {isSubmitting
                    ? <><Spinner data-icon="inline-start" /> Shortening...</>
                    : "Shorten"}
                </Button>
              </Field>
              {errors.originalUrl && <FieldError>{errors.originalUrl.message}</FieldError>}
            </Field>
          </form>
        </CardContent>

        {createdShortUrl &&
          <CardFooter>
            <Item className="p-0">
              <ItemMedia><CircleCheck className="size-4" /></ItemMedia>
              <ItemContent>
                <ItemTitle>Your short link is ready</ItemTitle>
                <ItemDescription>
                  <a href={createdShortUrl.absoluteUrl} target="_blank" rel="noreferrer"
                    className="break-all font-mono no-underline! hover:underline!">
                    {createdShortUrl.absoluteUrl}
                  </a>
                </ItemDescription>
                
                <ItemDescription>
                  {createdShortUrl.expiresAt
                    ? `Expires at ${format(createdShortUrl.expiresAt, "PP p")}`
                    : "This link never expires."}
                </ItemDescription>
              </ItemContent>
              <ItemActions>
                <CopyUrlButton value={createdShortUrl.absoluteUrl} />
              </ItemActions>
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
    </div>
  );
};

export default HomePage;
