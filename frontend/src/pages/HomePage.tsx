import React, { useState } from "react"
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Field } from "@/components/ui/field";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { CircleCheck, CircleX, Copy, CopyCheck } from "lucide-react";
import { Spinner } from "@/components/ui/spinner";
import { createAnonymousShortUrlApi } from "@/api/shortUrls/createShortUrl";
import type { ShortUrl } from "@/api/shortUrls/shortUrl.shared";

// HomePage: contains a hero section and URL Shortener form
const HomePage = () => {
  const [originalUrl, setOriginalUrl] = useState("");
  const [isCreating, setIsCreating] = useState(false);
  const [hasError, setHasError] = useState(false);
  const [shortUrl, setShortUrl] = useState(null as ShortUrl | null)

  const handleSubmit = async (e: React.SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();

    setShortUrl(null);
    setIsCreating(true);
    setHasError(false);
    const apiResponse = await createAnonymousShortUrlApi({ originalUrl });
    if (apiResponse.success) {
      setShortUrl(apiResponse.data);
    }
    else {
      setHasError(true);
    }
    setIsCreating(false);
  }

  const [copied, setCopied] = useState(false);
  const handleCopy = async () => {
    if (shortUrl === null) return;
    setCopied(true);
    await navigator.clipboard.writeText(shortUrl.absoluteUrl);
    setTimeout(() => setCopied(false), 3000);
  }

  return (
    <div className="flex flex-col items-center w-full max-w-5xl">
      <h1 className="font-bold text-5xl my-8">Shorten. Manage. Share</h1>
      <p className="font-medium text-md mb-8">
        An easy-to-use platform to shorten, organize, and manage your URLs</p>
      <Card className="w-full max-w-2xl">
        <CardHeader>
          <CardTitle>Try It Out</CardTitle>
          <CardDescription>Enter your URL to shorten.</CardDescription>
        </CardHeader>

        <CardContent>
          <form onSubmit={handleSubmit}>
            <Field orientation="horizontal">
              <Input id="original-url" type="url" placeholder="https://example.com"
                value={originalUrl} onChange={e => setOriginalUrl(e.target.value)} />
              {!isCreating 
                ? <Button type="submit" variant="outline" disabled={isCreating}>
                    Shorten
                  </Button>
                : <Button variant="outline" disabled>
                    <Spinner data-icon="inline-start" /> Shortening...
                  </Button>
              }
            </Field>
          </form>
        </CardContent>

        {shortUrl &&
          <CardFooter>
            <div className="flex flex-col w-full items-start gap-2">
              <h3 className="font-medium flex items-center gap-1">
                <CircleCheck className="size-4" /> Successful!
              </h3>
              <div className="flex flex-row w-full items-center gap-2">
                <a href={shortUrl.absoluteUrl} target="_blank" className="hover:underline flex-1 ">
                  {shortUrl.absoluteUrl}
                </a>
                <Button type="button" variant="outline" size="icon" className="ml-auto" 
                  onClick={handleCopy}>
                  {copied ? <CopyCheck /> : <Copy />}
                </Button>
              </div>
              <Separator />
              <p className="text-muted-foreground">
                The link will expire at {shortUrl.expiresAt.toLocaleString()}
              </p>
            </div>
          </CardFooter>
        }

        {hasError &&
          <CardFooter>
            <div className="flex flex-col w-full items-start gap-2">
              <h3 className="font-medium flex items-center gap-1">
                <CircleX className="size-4" /> Oops, something went wrong.
              </h3>
              <p className="text-muted-foreground">
                Please try again later. If the issue persists, please contact the website owner.
              </p>
            </div>
          </CardFooter>
        }
      </Card>
    </div>
  )
}

export default HomePage;
