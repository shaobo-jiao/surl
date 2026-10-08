import { queryShortUrlsApi, type PagedResult } from "@/api/shortUrls/queryShortUrls";
import { type ShortUrl } from "@/api/shortUrls/shortUrl.shared";
import CreateShortUrlDialog from "@/components/shortUrls/CreateShortUrlDialog";
import ShortUrlCard from "@/components/shortUrls/ShortUrlCard";
import UrlFiltersPopover, { DEFAULT_FILTERS, type ShortUrlFilters } from "@/components/shortUrls/ShortUrlFiltersPopover";
import EmptyShortUrlsCard from "@/components/shortUrls/EmptyShortUrlsCard";
import PageLoader from "@/components/common/PageLoader";
import { Link2 } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import { toast } from "sonner";
import PaginationBar from "@/components/common/PaginationBar";


const EMPTY_PAGED_SHORT_URLS: PagedResult<ShortUrl> = {
  items: [],
  page: DEFAULT_FILTERS.page, // returned from backend, aligned with the filters
  pageSize: DEFAULT_FILTERS.pageSize, // returned from backend, aligned with the filters
  totalCount: 0,
  totalPages: 0,
};

export default function UrlsPage() {
  const [pagedShortUrls, setPagedShortUrls] = useState<PagedResult<ShortUrl>>(EMPTY_PAGED_SHORT_URLS);
  const [filters, setFilters] = useState<ShortUrlFilters>(DEFAULT_FILTERS);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  // returns the fetched paged result, or null when the request failed (already toasted)
  const fetchShortUrls = useCallback(async (queryParams: ShortUrlFilters) => {
    const queryResult = await queryShortUrlsApi(queryParams);
    if (!queryResult.success) {
      toast.error("Failed loading short URLs", { description: queryResult.error.detail });
      return null;
    }
    return queryResult.data;
  }, []);

  useEffect(() => {
    // load the default short urls page on mount
    let ignore = false; // handles effect firing twice
    fetchShortUrls(DEFAULT_FILTERS)
      .then((paged) => { if (!ignore && paged) setPagedShortUrls(paged); })
      .finally(() => { if (!ignore) setIsLoading(false); });
    return () => { ignore = true; };
  }, [fetchShortUrls]);

  // later to REVIEW create and delete to trigger re-fetch instead
  const handleCreate = (created: ShortUrl) => {
    setPagedShortUrls((previous) => ({
      ...previous,
      items: [created, ...previous.items],
      totalCount: previous.totalCount + 1,
    }));
  };

  const handleDelete = (toDelete: ShortUrl) => {
    setPagedShortUrls((previous) => ({
      ...previous,
      items: previous.items.filter((existing) => existing.id !== toDelete.id),
      totalCount: Math.max(0, previous.totalCount - 1),
    }));
  };

  const handleUpdate = (updated: ShortUrl) => {
    setPagedShortUrls((previous) => ({
      ...previous,
      items: previous.items.map((existing) =>
        existing.id === updated.id ? updated : existing),
    }));
  };

  const handleApplyFilters = async (nextFilters: ShortUrlFilters) => {
    const paged = await fetchShortUrls(nextFilters);
    if (!paged) return false;

    setFilters(nextFilters);
    setPagedShortUrls(paged);
    return true;
  };

  const handlePageChange = async (nextPage: number) => {
    const nextFilters = { ...filters, page: nextPage };
    const paged = await fetchShortUrls(nextFilters);
    if (!paged) return;

    setFilters(nextFilters);
    setPagedShortUrls(paged);
  };

  return (
    <div className="flex flex-col items-center w-full max-w-3xl gap-4">

      {/* body header with the title and create new button */}
      <div className="flex flex-row items-center w-full gap-4 mt-4">
        <Link2 className="bg-black text-white size-12 p-3 rounded-xl" />
        <div className="flex flex-col">
          <h1 className="font-bold text-2xl">My URLs</h1>
          <p className="text-muted-foreground">Manage and track your shortened URLs</p>
        </div>
        <CreateShortUrlDialog onCreate={handleCreate} />
      </div>

      {/* toolbar: filter button on the left, result count on the right */}
      <div className="flex flex-row items-center w-full gap-2">
        <UrlFiltersPopover filters={filters} onApply={handleApplyFilters} />

        <p className="ml-auto text-sm text-muted-foreground">
          {pagedShortUrls.items.length} out of {pagedShortUrls.totalCount} results
        </p>
      </div>

      {/* list of short url cards, or a placeholder when there is nothing to show yet */}
      {isLoading
        ? (<PageLoader className="py-12" />)
        : pagedShortUrls.totalCount === 0
          ? (<EmptyShortUrlsCard />)
          : (<div className="flex flex-col w-full gap-4">
            {pagedShortUrls.items.map((shortUrl) => (
              <ShortUrlCard
                key={shortUrl.id}
                shortUrl={shortUrl}
                onDelete={handleDelete}
                onUpdate={handleUpdate}
              />
            ))}
          </div>)
      }

      {/* pagination below the list */}
      {pagedShortUrls.totalCount > 0 && (
        <PaginationBar
          page={filters.page}
          totalPages={pagedShortUrls.totalPages}
          onPageChange={handlePageChange} />
      )}
    </div>
  );
};
