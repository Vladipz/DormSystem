import { CardSkeleton } from "@/components/CardSkeleton";
import { EventCard } from "@/components/EventCard";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Pagination, PaginationContent, PaginationEllipsis, PaginationItem, PaginationLink, PaginationNext, PaginationPrevious } from "@/components/ui/pagination";
import { useEvents } from "@/lib/hooks/useEvents";
import { createFileRoute, Link } from "@tanstack/react-router";
import { AlertCircle, Plus, RefreshCw, Search } from "lucide-react";
import { useState } from "react";

export const Route = createFileRoute("/_mainLayout/events/")({
  component: RouteComponent,
});

function RouteComponent() {
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedEventId] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 9; // Show 9 events per page (3x3 grid)

  const { events, loading, error, refreshEvents, joinEvent, totalPages } = useEvents({
    pageNumber: currentPage,
    pageSize,
  });
  const [joiningEventId, setJoiningEventId] = useState<string | null>(null);

  const handleJoinEvent = async (eventId: string) => {
    setJoiningEventId(eventId);
    try {
      // TODO: Get actual user ID from auth context
      const userId = "a92b0237-c938-4a68-9750-9aa3f7d834a7";
      await joinEvent(eventId, userId);
    } catch (error) {
      console.error("Failed to join event:", error);
    } finally {
      setJoiningEventId(null);
    }
  };

  if (selectedEventId !== null) {
    return <div>event details</div>;
  }

  if (loading) {
    return (
      <div className="space-y-4 py-6">
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {[1, 2, 3].map((i) => (
            <CardSkeleton key={i} />
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-4 py-6">
        <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-6 text-center">
          <div className="flex flex-col items-center justify-center space-y-3">
            <AlertCircle className="w-8 h-8 text-destructive" />
            <h3 className="text-lg font-medium text-destructive">
              Failed to load events
            </h3>
            <p className="text-sm text-muted-foreground max-w-md">
              There was a problem loading the events. Please try again.
            </p>
            <Button
              variant="outline"
              onClick={() => refreshEvents()}
              className="mt-2"
              size="sm"
            >
              <RefreshCw className="mr-2 h-4 w-4" />
              Try again
            </Button>
          </div>
        </div>
      </div>
    );
  }

  const paginationItems = [];
  for (let i = 1; i <= totalPages; i++) {
    if (
      i === 1 || // Always show first page
      i === totalPages || // Always show last page
      (i >= currentPage - 1 && i <= currentPage + 1) // Show current page and neighbors
    ) {
      paginationItems.push(
        <PaginationItem key={i}>
          <PaginationLink
            onClick={() => setCurrentPage(i)}
            isActive={currentPage === i}
          >
            {i}
          </PaginationLink>
        </PaginationItem>
      );
    } else if (
      (i === 2 && currentPage > 3) ||
      (i === totalPages - 1 && currentPage < totalPages - 2)
    ) {
      paginationItems.push(
        <PaginationItem key={i}>
          <PaginationEllipsis />
        </PaginationItem>
      );
    }
  }

  return (
    <div className="space-y-4 py-6">
      <div className="flex flex-col sm:flex-row justify-between gap-4">
        <div className="relative flex-1">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input
            type="search"
            placeholder="Search events..."
            className="pl-8"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
        <Button asChild>
          <Link to="/events/create">
            <Plus className="mr-2 h-4 w-4" /> Create Event
          </Link>
        </Button>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {events.map((event) => (
          <EventCard
            key={event.id}
            event={event}
            onJoin={handleJoinEvent}
            isJoining={joiningEventId === event.id}
          />
        ))}
      </div>

      {totalPages > 1 && (
        <div className="mt-8 flex justify-center">
          <Pagination>
            <PaginationContent>
              <PaginationItem>
                <PaginationPrevious
                  onClick={() => setCurrentPage((prev) => Math.max(1, prev - 1))}
                  isDisabled={currentPage === 1}
                />
              </PaginationItem>

              {paginationItems}

              <PaginationItem>
                <PaginationNext
                  onClick={() => setCurrentPage((prev) => Math.min(totalPages, prev + 1))}
                  isDisabled={currentPage === totalPages}
                />
              </PaginationItem>
            </PaginationContent>
          </Pagination>
        </div>
      )}
    </div>
  );
}
