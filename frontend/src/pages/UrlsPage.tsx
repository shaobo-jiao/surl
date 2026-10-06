import { queryShortUrlsApi } from "@/api/shortUrls/queryShortUrls";
import { type ShortUrl } from "@/api/shortUrls/shortUrl.shared";
import CreateShortUrlDialog from "@/components/shortUrls/CreateShortUrlDialog";
import ShortUrlCard from "@/components/shortUrls/ShortUrlCard";
import { Link2 } from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";

export default function UrlsPage() {
  const [shortUrls, setShortUrls] = useState<ShortUrl[]>([]);

  useEffect(() => {
    let ignore = false; // handles effect firing twice

    // load the default short urls page on mount; with no query params,
    // the backend uses its default page and page size
    const loadShortUrls = async () => {
      const queryResult = await queryShortUrlsApi();
      if (!queryResult.success) {
        toast.error("Failed loading short URLs", { description: queryResult.error.detail });
        return;
      }
      if (!ignore)
        setShortUrls(queryResult.data.items);
    };

    loadShortUrls();

    return () => { ignore = true; };
  }, []);

  const handleCreate = (created: ShortUrl) => {
    setShortUrls((previousShortUrls) => [created, ...previousShortUrls]);
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
        {shortUrls.map((shortUrl) => (
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

