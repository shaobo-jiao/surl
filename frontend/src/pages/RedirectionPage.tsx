import { Item, ItemContent, ItemDescription, ItemMedia, ItemTitle } from "@/components/ui/item";
import { Spinner } from "@/components/ui/spinner";
import { CircleX } from "lucide-react";
import { useEffect, useState } from "react";
import { Link, useParams } from "react-router";
import { getRedirectionUrlApi } from "@/api/shortUrls/getRedirectionUrl";
import ThemeTrigger from "@/components/common/ThemeTrigger";
import AppFooter from "@/components/AppFooter";

/* 
* standalone RedirectionPage that is separated from the rest of the app.
* It isn't wrapped with AuthProvider nor AppLayout to reduce unnecessary 
* auth state loading time
*/
const RedirectionPage = () => {
  const { shortUrlCode } = useParams();
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!shortUrlCode) {
      setError("Empty Short URL");
      return;
    }
    // immediately invoked the async function to get original url and redirect
    const redirect = async () => {
      try {
        const originalUrl = await getRedirectionUrlApi(shortUrlCode);
        window.location.href = originalUrl;
      }
      catch {
        setError("The entered shortened URL is either invalid or expired.");
      }
    }
    redirect();
  }, [shortUrlCode]);

  return (
    <div className="flex flex-col min-h-screen">
      <header className="w-full border-b">
        <div className="mx-auto flex flex-row items-center gap-2 p-4 w-full max-w-5xl">
          <Link to="/" className="text-3xl font-black flex-1">SURL</Link>
          <ThemeTrigger />
        </div>
      </header>

      <main className="flex flex-1 flex-col items-center w-full p-4">
        {
          error ? (
            <Item variant="outline" className="w-full max-w-2xl rounded-md">
              <ItemMedia><CircleX className="size-4" /></ItemMedia>
              <ItemContent>
                <ItemTitle>Redirection failed</ItemTitle>
                <ItemDescription>{error}</ItemDescription>
              </ItemContent>
            </Item>
          ) : (
            <Item variant="outline" className="w-full max-w-2xl rounded-md">
              <ItemMedia><Spinner /></ItemMedia>
              <ItemContent>
                <ItemTitle>Redirecting...</ItemTitle>
              </ItemContent>
            </Item>
          )
        }
      </main>

      <AppFooter />
    </div>
  );
};

export default RedirectionPage;