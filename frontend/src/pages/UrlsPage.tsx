import { queryShortUrlsApi } from "@/api/shortUrls/queryShortUrls";
import { deleteShortUrlApi } from "@/api/shortUrls/deleteShortUrl";
import { type ShortUrl } from "@/api/shortUrls/shortUrl.shared";
import CreateShortUrlDialog from "@/components/shortUrls/CreateShortUrlDialog";
import ShortUrlCard from "@/components/shortUrls/ShortUrlCard";
import UrlFiltersPopover, { DEFAULT_FILTERS, type ShortUrlFilters } from "@/components/shortUrls/ShortUrlFiltersPopover";
import { Link2 } from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";


export default function UrlsPage() {
  const [shortUrls, setShortUrls] = useState<ShortUrl[]>([]);
  const [filters, setFilters] = useState<ShortUrlFilters>(DEFAULT_FILTERS);

  useEffect(() => {
    // load the default short urls page with no query params on mount; 
    let ignore = false; // handles effect firing twice

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

  const handleDelete = async (toDelete: ShortUrl) => {
    const deleteResult = await deleteShortUrlApi(toDelete.id);
    if (!deleteResult.success) {
      toast.error("Failed deleting short URL", { description: deleteResult.error.detail });
      return false;
    }
    setShortUrls((previousShortUrls) =>
      previousShortUrls.filter((existing) => existing.id !== toDelete.id));
    toast.success("Short URL has been deleted");
    return true;
  };

  const handleApplyFilters = async () => {

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

      {/* toolbar: filter button on the left, result count on the right */}
      <div className="flex flex-row items-center w-full gap-2 mb-4">
        <UrlFiltersPopover filters={filters} onApply={setFilters} />

        <p className="ml-auto text-sm text-muted-foreground">
          {shortUrls.length} results
        </p>
      </div>

      {/* list of short url cards */}
      <div className="flex flex-col w-full gap-4">
        {shortUrls.map((shortUrl) => (
          <ShortUrlCard
            key={shortUrl.id}
            shortUrl={shortUrl}
            onDelete={handleDelete}
          />
        ))}
      </div>

      {/* pagination below the list */}
      
    </div>
  );
};
