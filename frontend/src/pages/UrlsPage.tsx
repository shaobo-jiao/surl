import { type ShortUrl } from "@/api/shortUrls/shortUrl.shared";
import CreateShortUrlDialog from "@/components/shortUrls/CreateShortUrlDialog";
import ShortUrlCard from "@/components/shortUrls/ShortUrlCard";
import { Link2 } from "lucide-react";
import { useState } from "react";

// TEMPORARY sample data for checking the ShortUrlCard layout.
const sampleShortUrls: ShortUrl[] = [
  {
    id: "1",
    originalUrl: "https://www.example.com/a/very/long/original/url/path/that/should/be/truncated",
    code: "abc123",
    absoluteUrl: "http://surl.local/abc123",
    expiresAt: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000),
    clickCount: 1234,
  },
  {
    id: "2",
    originalUrl: "https://www.google.com",
    code: "xyz789",
    absoluteUrl: "http://surl.local/xyz789",
    expiresAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000),
    clickCount: 0,
  },
];

export default function UrlsPage() {
  const [urls, setUrls] = useState<ShortUrl[]>(sampleShortUrls);

  const handleCreate = (created: ShortUrl) => {
    setUrls((previousUrls) => [created, ...previousUrls]);
  };

  return (
    <div className="flex flex-col items-center w-full max-w-3xl">

      {/* body header with the title and create new button */}
      <div className="flex flex-row items-center w-full gap-4 my-8">
        <Link2 className="bg-black text-white size-12 p-3 rounded-xl" />
        <div className="flex flex-col">
          <h1 className="font-bold text-2xl">My URLs</h1>
          <p className="text-muted-foreground">Manage and track your shortened URLs</p>
        </div>
        <CreateShortUrlDialog onCreate={handleCreate} />
      </div>

      <div className="flex flex-col w-full gap-4">
        {urls.map((shortUrl) => (
          <ShortUrlCard
            key={shortUrl.id}
            shortUrl={shortUrl}
            onDelete={(deleted) => console.log("delete", deleted.id)}
          />
        ))}
      </div>
    </div>
  );
};

