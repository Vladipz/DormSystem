import { Button } from "@/components/ui/button";
import {
  Pagination,
  PaginationContent,
  PaginationItem,
} from "@/components/ui/pagination";
import type { PagedEventCommentsResponse } from "@/lib/types/event";

interface CommentPaginationProps {
  commentsPage: PagedEventCommentsResponse;
  onPageChange: (pageNumber: number) => void;
}

export function CommentPagination({ commentsPage, onPageChange }: CommentPaginationProps) {
  if (commentsPage.totalCount === 0) {
    return null;
  }

  return (
    <Pagination className="justify-end">
      <PaginationContent>
        <PaginationItem>
          <span className="rounded-full bg-muted px-3 py-1 text-xs font-medium text-muted-foreground">
            {commentsPage.totalCount} {commentsPage.totalCount === 1 ? "comment" : "comments"}
          </span>
        </PaginationItem>
        <PaginationItem>
          <Button
            type="button"
            variant="ghost"
            size="sm"
            disabled={!commentsPage.hasPreviousPage}
            onClick={() => onPageChange(commentsPage.pageNumber - 1)}
          >
            Previous
          </Button>
        </PaginationItem>
        <PaginationItem>
          <div className="flex h-9 min-w-9 items-center justify-center rounded-md border px-3 text-sm font-medium">
            {commentsPage.pageNumber}
          </div>
        </PaginationItem>
        <PaginationItem>
          <Button
            type="button"
            variant="ghost"
            size="sm"
            disabled={!commentsPage.hasNextPage}
            onClick={() => onPageChange(commentsPage.pageNumber + 1)}
          >
            Next
          </Button>
        </PaginationItem>
      </PaginationContent>
    </Pagination>
  );
}
