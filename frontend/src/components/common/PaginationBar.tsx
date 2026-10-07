import { Pagination, PaginationContent, PaginationEllipsis, PaginationItem, PaginationLink, PaginationNext, PaginationPrevious } from "../ui/pagination";


interface PaginationBarProps {
  page: number, // current page
  totalPages: number, // total number of pages
  onPageChange: (page: number) => Promise<void>;
}

export default function PaginationBar({
  page = 1,
  totalPages = 1,
  onPageChange }: PaginationBarProps) {
  // the pagination bar displays three numbers with ellipses
  // the three numbers to display depend on the current page
  // when there are 3 or less than 3 pages in total, simply display the numbers
  page = Math.max(1, page);
  totalPages = Math.max(1, totalPages);
  console.log(`page ${page} out of total ${totalPages} pages`);
  let displayPageNumbers: number[] = [];

  if (totalPages <= 3)
    displayPageNumbers = Array.from(Array(totalPages), (_, index) => index + 1)
  else if (page == 1)
    displayPageNumbers = [1, 2, 3];
  else if (page == totalPages)
    displayPageNumbers = [totalPages - 2, totalPages - 1, totalPages];
  else
    displayPageNumbers = [page - 1, page, page + 1];

  const displayLeftEllipsis = displayPageNumbers[0] > 1;
  const displayRightEllipsis = displayPageNumbers[displayPageNumbers.length - 1] < totalPages;

  const handlePrevious = async () => {
    console.log("clicked previous, current page is ", page);
    // already the 1st page, do nothing
    if (page == 1) return;
    await onPageChange(page - 1);
  }

  const handleNext = async () => {
    console.log("clicked next, current page is ", page);
    // already the last page, do nothing
    if (page == totalPages) return;
    await onPageChange(page + 1);
  }

  const handlePageChange = async (newPage: number) => {
    console.log("clicked page ", newPage);
    if (newPage == page) return;
    await onPageChange(newPage);
  }

  return (
    <Pagination>
      <PaginationContent>
        <PaginationItem>
          <PaginationPrevious onClick={handlePrevious} />
        </PaginationItem>
        {displayLeftEllipsis && <PaginationItem><PaginationEllipsis /></PaginationItem>}

        {
          displayPageNumbers.map(x =>
            <PaginationItem key={x.toString()}>
              <PaginationLink isActive={x == page} onClick={() => handlePageChange(x)}>
                {x}
              </PaginationLink>
            </PaginationItem>)
        }

        {displayRightEllipsis && <PaginationItem><PaginationEllipsis /></PaginationItem>}
        <PaginationItem>
          <PaginationNext onClick={handleNext} />
        </PaginationItem>
      </PaginationContent>
    </Pagination>
  )
}