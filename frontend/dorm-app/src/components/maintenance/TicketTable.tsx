import {
  Badge,
  Button,
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui";
import type { MaintenanceTicketResponse } from "@/lib/types/maintenanceTicket";
import { prioColor, statusColor } from "@/lib/utils/maintenanceUtils";
import { ArrowUpDown, Filter } from "lucide-react";
import { useEffect, useState } from "react";
import { EditTicketDialog } from "../dialogs/EditTicketDialog";

type SortField = "date" | "room" | "priority" | "status";

interface TicketTableProps {
  tickets: MaintenanceTicketResponse[];
  sortBy: SortField;
  sortOrder: "asc" | "desc";
  toggleSort: (column: SortField) => void;
  viewRoom: (id: string) => void;
  isAdmin?: boolean;
}

export function TicketTable({
  tickets,
  sortBy,
  sortOrder,
  toggleSort,
  viewRoom,
  isAdmin = false,
}: TicketTableProps) {
  const [editingTicket, setEditingTicket] =
    useState<MaintenanceTicketResponse | null>(null);
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);

  // use effect for debugging ticket data
  useEffect(() => {
    console.log("Tickets data:", tickets);
  }, [tickets]);

  const handleEditClick = (ticket: MaintenanceTicketResponse) => {
    setEditingTicket(ticket);
    // Small delay to ensure dropdown closes first
    setTimeout(() => {
      setIsEditDialogOpen(true);
    }, 100);
  };

  const handleEditDialogClose = () => {
    setIsEditDialogOpen(false);
    // Clear the ticket after dialog animation completes
    setTimeout(() => {
      setEditingTicket(null);
    }, 200);
  };

  const getSortIcon = (column: SortField) =>
    sortBy === column && (
      <ArrowUpDown
        className={`ml-1 h-4 w-4 ${sortOrder === "desc" ? "rotate-180" : ""}`}
      />
    );

  return (
    <>
      <Card>
        <CardHeader>
          <CardTitle>Maintenance Tickets ({tickets.length})</CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                {[
                  { key: "date", label: "Date" },
                  { key: "room", label: "Room" },
                  { key: "priority", label: "Priority" },
                  { key: "status", label: "Status" },
                ].map(({ key, label }) => (
                  <TableHead key={key}>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="p-0 font-medium"
                      onClick={() => toggleSort(key as SortField)}
                    >
                      {label} {getSortIcon(key as SortField)}
                    </Button>
                  </TableHead>
                ))}
                <TableHead>Description</TableHead>
                <TableHead>Reported By</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>

            <TableBody>
              {tickets.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} className="py-6 text-center">
                    No tickets
                  </TableCell>
                </TableRow>
              ) : (
                tickets.map((ticket: MaintenanceTicketResponse) => (
                  <TableRow key={ticket.id}>
                    <TableCell>
                      {new Date(ticket.createdAt).toLocaleDateString()}
                    </TableCell>
                    <TableCell>Room {ticket.room.label}</TableCell>
                    <TableCell>
                      <Badge
                        variant="outline"
                        className={prioColor(ticket.priority)}
                      >
                        {ticket.priority}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Badge
                        variant="outline"
                        className={statusColor(ticket.status)}
                      >
                        {ticket.status === "InProgress"
                          ? "In Progress"
                          : ticket.status}
                      </Badge>
                    </TableCell>
                    <TableCell className="max-w-[240px] truncate">
                      {ticket.description}
                    </TableCell>
                    <TableCell>
                      {ticket.reporter?.firstName && ticket.reporter?.lastName
                        ? `${ticket.reporter.firstName} ${ticket.reporter.lastName}`
                        : "Unknown"}
                    </TableCell>
                    <TableCell className="text-right">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="sm">
                            <Filter className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem
                            onClick={() => viewRoom(ticket.room.id)}
                          >
                            View room
                          </DropdownMenuItem>
                          {isAdmin && (
                            <DropdownMenuItem
                              onClick={() => handleEditClick(ticket)}
                            >
                              Edit ticket
                            </DropdownMenuItem>
                          )}
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Edit Dialog outside of dropdown */}
      {editingTicket && (
        <EditTicketDialog
          key={editingTicket.id} // Force remount on ticket change
          ticket={editingTicket}
          open={isEditDialogOpen}
          onOpenChange={handleEditDialogClose}
        />
      )}
    </>
  );
}
