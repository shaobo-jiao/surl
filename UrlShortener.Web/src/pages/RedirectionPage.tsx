import { Item, ItemContent, ItemDescription, ItemMedia, ItemTitle } from "@/components/ui/item";
import { Spinner } from "@/components/ui/spinner";
import { getOriginalUrl } from "@/ShortUrlApi";
import { CircleX } from "lucide-react";
import { useEffect, useState } from "react";
import { useParams } from "react-router";

const RedirectionPage = () => {
  const { shortUrlCode } = useParams();
  const [error, setError] = useState(null as string | null);

  useEffect(() => {
    if (!shortUrlCode)
    {
      setError("Empty Short URL"); 
      return;
    }
    // immediately invoked the async function to get original url and redirect
    (async () => {
      try {
        const originalUrl = await getOriginalUrl(shortUrlCode);
        window.location.href = originalUrl;
      }
      catch {
        setError("The entered shortened URL is either invalid or expired.");
      }
    })();
  }, [shortUrlCode]);

  if (error) {
    return (
      <Item variant="outline" className="w-full max-w-2xl rounded-md">
        <ItemMedia><CircleX className="size-4" /></ItemMedia>
        <ItemContent>
          <ItemTitle>Redirection Failed</ItemTitle>
          <ItemDescription>{error}</ItemDescription>
        </ItemContent>
      </Item>
    );
  }
  return (
    <Item variant="outline" className="w-full max-w-2xl rounded-md">
      <ItemMedia><Spinner /></ItemMedia>
      <ItemContent>
        <ItemTitle>Redirecting...</ItemTitle>
      </ItemContent>
    </Item>
  );
};

export default RedirectionPage;