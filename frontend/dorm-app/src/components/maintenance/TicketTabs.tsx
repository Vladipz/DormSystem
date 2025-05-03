import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui";
import type { MaintenanceTicketResponse } from "@/lib/types/maintenanceTicket";
import { TicketTable } from "./TicketTable";

type SortField = "date" | "room" | "priority" | "status";

interface TicketTabsProps {
  tickets: MaintenanceTicketResponse[];
  sortBy: SortField;
  sortOrder: "asc" | "desc";
  toggleSort: (column: SortField) => void;
  viewRoom: (id: string) => void;
}

export function TicketTabs({
  tickets,
  sortBy,
  sortOrder,
  toggleSort,
  viewRoom,
}: TicketTabsProps) {
  // Filter tickets by status for different tabs
  const openTickets = tickets.filter((t) => t.status === "Open");
  const inProgressTickets = tickets.filter((t) => t.status === "InProgress");
  const resolvedTickets = tickets.filter((t) => t.status === "Resolved");

  return (
    <Tabs defaultValue="all">
      <TabsList>
        <TabsTrigger value="all">All Tickets</TabsTrigger>
        <TabsTrigger value="open">Open Tickets</TabsTrigger>
        <TabsTrigger value="in-progress">In Progress Tickets</TabsTrigger>
        <TabsTrigger value="resolved">Resolved Tickets</TabsTrigger>
      </TabsList>

      <TabsContent value="all">
        <TicketTable
          tickets={tickets}
          sortBy={sortBy}
          sortOrder={sortOrder}
          toggleSort={toggleSort}
          viewRoom={viewRoom}
        />
      </TabsContent>

      <TabsContent value="open">
        <TicketTable
          tickets={openTickets}
          sortBy={sortBy}
          sortOrder={sortOrder}
          toggleSort={toggleSort}
          viewRoom={viewRoom}
        />
      </TabsContent>

      <TabsContent value="in-progress">
        <TicketTable
          tickets={inProgressTickets}
          sortBy={sortBy}
          sortOrder={sortOrder}
          toggleSort={toggleSort}
          viewRoom={viewRoom}
        />
      </TabsContent>

      <TabsContent value="resolved">
        <TicketTable
          tickets={resolvedTickets}
          sortBy={sortBy}
          sortOrder={sortOrder}
          toggleSort={toggleSort}
          viewRoom={viewRoom}
        />
      </TabsContent>
    </Tabs>
  );
}