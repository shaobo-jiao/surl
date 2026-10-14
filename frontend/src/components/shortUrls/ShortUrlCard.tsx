import { EllipsisVertical, MousePointerClick } from "lucide-react";
import { type ShortUrl } from "@/api/shortUrls/shortUrl.shared";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import CopyUrlButton from "@/components/shortUrls/CopyUrlButton";
import DeleteShortUrlButton from "@/components/shortUrls/DeleteShortUrlButton";
import EditShortUrlDialog from "@/components/shortUrls/EditShortUrlDialog";
import { Item, ItemContent, ItemDescription, ItemHeader } from "../ui/item";
import { format } from "date-fns";
import { Button } from "../ui/button";
import { Popover, PopoverContent, PopoverTrigger } from "../ui/popover";

interface ShortUrlCardProps {
  shortUrl: ShortUrl;
  onDelete: (shortUrl: ShortUrl) => void;
  onUpdate: (shortUrl: ShortUrl) => void;
}

export default function ShortUrlCard({ shortUrl, onDelete, onUpdate }: ShortUrlCardProps) {
  // removes https in the short url to save space
  const displayUrl = shortUrl.absoluteUrl.replace(/^https?:\/\//, "");

  return (
    <Card>
      {/* Header: the short URL and the action buttons */}
      <CardHeader className="flex flex-row w-full items-center gap-2">
        <a href={shortUrl.absoluteUrl}
          target="_blank"
          rel="noreferrer"
          className="block rounded-lg px-3 py-2 hover:underline font-mono truncate">
          {displayUrl}
        </a>

        {/* wide screen: three icons for the three actions */}
        <div className="hidden sm:flex flex-row ml-auto">
          <CopyUrlButton
            value={shortUrl.absoluteUrl}
            className="text-muted-foreground" />
          <EditShortUrlDialog
            shortUrl={shortUrl}
            onUpdate={onUpdate} />
          <DeleteShortUrlButton
            shortUrl={shortUrl}
            onDelete={onDelete} />
        </div>

        {/* small screen: single icon that expands to the three icons */}
        <Popover>
          <PopoverTrigger render={<Button type="button"
            variant="ghost"
            size="icon"
            aria-label="Short URL actions"
            className="text-muted-foreground sm:hidden ml-auto" />}>
            <EllipsisVertical />
          </PopoverTrigger>
          
          <PopoverContent align="end" className="sm:hidden gap-0.5 p-1 w-fit">
            <CopyUrlButton
              value={shortUrl.absoluteUrl}
              className="text-muted-foreground" />
            <EditShortUrlDialog
              shortUrl={shortUrl}
              onUpdate={onUpdate} />
            <DeleteShortUrlButton
              shortUrl={shortUrl}
              onDelete={onDelete} />
          </PopoverContent>
        </Popover>
      </CardHeader>

      {/* Body: original URL with caption, clicks and expiry datetime */}
      <CardContent>
        <Item className="py-0 px-3">
          <ItemContent>
            <ItemHeader>Original URL</ItemHeader>
            <a href={shortUrl.originalUrl}
              target="_blank"
              rel="noreferrer"
              className="block text-sm text-muted-foreground hover:underline break-all">
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
