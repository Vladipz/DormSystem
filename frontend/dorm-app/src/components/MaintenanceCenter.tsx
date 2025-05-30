"use client";

import { useAuth } from "@/lib/hooks/useAuth";
import { useMaintenanceTickets } from "@/lib/hooks/useMaintenanceTicket";
import { useRooms } from "@/lib/hooks/useRooms";
import { useTicketFiltering } from "@/lib/hooks/useTicketFiltering";
import { useRouter } from "@tanstack/react-router";
import { CreateTicketDialog } from "./dialogs/CreateTicketDialog";
import { TicketFilters } from "./maintenance/TicketFilters";
import { TicketTabs } from "./maintenance/TicketTabs";
import { Skeleton } from "./ui";

export function MaintenanceCenter() {
  const { isAuthenticated, userRole } = useAuth();
  const router = useRouter();

  // Check if user is admin
  const isAdmin = userRole === "Admin";

  // Use the custom hook for filtering and sorting
  const {
    searchTerm,
    setSearchTerm,
    statusFilter,
    setStatusFilter,
    buildingFilter,
    setBuildingFilter,
    buildings,
    sortBy,
    sortOrder,
    toggleSort,
  } = useTicketFiltering([]);

  // Fetch data with filters
  const {
    data: ticketsResponse,
    isLoading,
    isError,
    isFetching,
  } = useMaintenanceTickets({
    page: 1,
    pageSize: 100,
    status: statusFilter === "All" ? undefined : statusFilter,
    buildingId: buildingFilter === "all" ? undefined : buildingFilter,
  });

  const { data: roomsResponse = [] } = useRooms(undefined, true);

  // Filter and sort tickets client-side (only text search)
  const {
    sortedTickets,
  } = useTicketFiltering(ticketsResponse?.items || []);

  // Room navigation handler
  const viewRoom = (id: string) =>
    router.navigate({
      to: `/rooms/${id}`,
    });

  // Initial loading state
  if (isLoading && !isFetching) return <p className="p-6">Loading…</p>;
  if (isError)
    return <p className="p-6 text-red-600">Failed to load tickets</p>;

  return (
    <div className="space-y-6">
      {/* Search and Create Button row */}
      <div className="flex flex-col sm:flex-row gap-4">
        <div className="flex-1">
          <TicketFilters
            searchTerm={searchTerm}
            setSearchTerm={setSearchTerm}
            statusFilter={statusFilter}
            setStatusFilter={setStatusFilter}
            buildingFilter={buildingFilter}
            setBuildingFilter={setBuildingFilter}
            buildings={buildings}
            isLoading={isFetching}
          />
        </div>

        {/* Create Ticket Button - only shown to authenticated users */}
        {isAuthenticated && (
          <div className="flex-shrink-0 self-start">
            <CreateTicketDialog rooms={roomsResponse} />
          </div>
        )}
      </div>

      {/* Ticket Tabs with loading state */}
      {isFetching ? (
        <div className="space-y-4">
          <Skeleton className="h-10 w-full max-w-md" />
          <Skeleton className="h-[400px] w-full" />
        </div>
      ) : (
        <TicketTabs
          tickets={sortedTickets}
          sortBy={sortBy}
          sortOrder={sortOrder}
          toggleSort={toggleSort}
          viewRoom={viewRoom}
          isAdmin={isAdmin}
        />
      )}
    </div>
  );
}
