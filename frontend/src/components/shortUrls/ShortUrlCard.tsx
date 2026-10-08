import { MousePointerClick } from "lucide-react";
import { type ShortUrl } from "@/api/shortUrls/shortUrl.shared";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import CopyUrlButton from "@/components/shortUrls/CopyUrlButton";
import DeleteShortUrlButton from "@/components/shortUrls/DeleteShortUrlButton";
import EditShortUrlDialog from "@/components/shortUrls/EditShortUrlDialog";
import { Item, ItemContent, ItemDescription, ItemHeader } from "../ui/item";
import { format } from "date-fns";

interface ShortUrlCardProps {
  shortUrl: ShortUrl;
  onDelete: (shortUrl: ShortUrl) => void;
  onUpdate: (shortUrl: ShortUrl) => void;
}

export default function ShortUrlCard({ shortUrl, onDelete, onUpdate }: ShortUrlCardProps) {
  return (
    <Card>
      {/* Header: the short URL, copy button, delete icon */}
      <CardHeader className="flex w-full items-center gap-2">
        <a href={shortUrl.absoluteUrl}
          target="_blank"
          rel="noreferrer"
          className="bg-muted rounded-lg px-3 py-2 hover:underline font-mono ">
          {shortUrl.absoluteUrl}
        </a>
        <CopyUrlButton value={shortUrl.absoluteUrl} />
        <EditShortUrlDialog
          shortUrl={shortUrl}
          onUpdate={onUpdate}
          className="ml-auto" />
        <DeleteShortUrlButton
          shortUrl={shortUrl}
          onDelete={onDelete} />
      </CardHeader>

      {/* Body: original URL with caption, clicks and expiry datetime */}
      <CardContent>
        <Item>
          <ItemContent>
            <ItemHeader>Original URL</ItemHeader>
            <a href={shortUrl.originalUrl}
              target="_blank"
              rel="noreferrer"
              className="text-sm text-muted-foreground hover:underline">
              {shortUrl.originalUrl}
            </a>
          </ItemContent>
        </Item>
        <Item>
          <ItemContent className="flex flex-row gap-6">
            <ItemDescription>
              <MousePointerClick className="inline size-4 mr-1" />
              {shortUrl.clickCount.toLocaleString()} clicks
            </ItemDescription>
            <ItemDescription>
              {shortUrl.expiresAt
                ? `Expires at ${format(shortUrl.expiresAt, "PP p")}`
                : "Never Expire"}
            </ItemDescription>
          </ItemContent>
        </Item>
      </CardContent>
    </Card>
  );
};
