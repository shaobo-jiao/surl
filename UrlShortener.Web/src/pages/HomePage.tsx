import React, { useState } from "react"
import { createShortUrl, type ShortUrlResponse } from "@/ShortUrlApi";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Field } from "@/components/ui/field";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { CircleCheck, CircleX, Copy, CopyCheck } from "lucide-react";
import { Spinner } from "@/components/ui/spinner";

// HomePage: contains a hero section and URL Shortener form
const HomePage = () => {
  const [originalUrl, setOriginalUrl] = useState("");
  const [isCreating, setIsCreating] = useState(false);
  const [hasError, setHasError] = useState(false);
  const [response, setResponse] = useState(null as ShortUrlResponse | null)

  const handleSubmit = async (e: React.SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();

    setResponse(null);
    setIsCreating(true);
    setHasError(false);
    try {
      const response = await createShortUrl({ originalUrl });
      setResponse(response);
      setIsCreating(false);
    }
    catch (e) {
      setHasError(true);
      setIsCreating(false);
    }
  }

  const [copied, setCopied] = useState(false);
  const handleCopy = async () => {
    if (response === null) return;
    setCopied(true);
    await navigator.clipboard.writeText(response.shortUrl);
    setTimeout(() => setCopied(false), 3000);
  }

  return (
    <div className="flex flex-col items-center w-full max-w-5xl">
      <h1 className="font-bold text-5xl my-8">Shorten. Manage. Share</h1>
      <p className="font-medium text-md mb-8">
        An easy-to-use platform to shorten, organize, and manage your URLs</p>
      <Card className="w-full max-w-2xl rounded-md">
        <CardHeader>
          <CardTitle>Try It Out</CardTitle>
          <CardDescription>Enter your URL to shorten.</CardDescription>
        </CardHeader>

        <CardContent>
          <form onSubmit={handleSubmit}>
            <Field orientation="horizontal">
              <Input id="original-url" type="url" placeholder="https://example.com" className="rounded-md" 
                value={originalUrl} onChange={e => setOriginalUrl(e.target.value)} />
              {!isCreating 
                ? <Button type="submit" variant="outline" className="rounded-md" disabled={isCreating}>
                    Shorten
                  </Button>
                : <Button variant="outline" className="rounded-md" disabled>
                    <Spinner data-icon="inline-start" /> Shortening...
                  </Button>
              }
            </Field>
          </form>
        </CardContent>

        {response &&
          <CardFooter>
            <div className="flex flex-col w-full items-start gap-2">
              <h3 className="font-medium flex items-center gap-1">
                <CircleCheck className="size-4" /> Successful!
              </h3>
              <div className="flex flex-row w-full items-center gap-2">
                <a href={response.shortUrl} target="_blank" className="hover:underline flex-1 ">
                  {response.shortUrl}
                </a>
                <Button type="button" variant="outline" size="icon" className="ml-auto rounded-md" 
                  onClick={handleCopy}>
                  {copied ? <CopyCheck /> : <Copy />}
                </Button>
              </div>
              <Separator />
              <p className="text-muted-foreground">
                The link will expire at {response.expiresAt.toLocaleString()}
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
