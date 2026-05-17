import {
  Badge,
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui";
import { EditTicketDialog } from "@/components/dialogs/EditTicketDialog";
import { useMaintenanceDrilldown } from "@/lib/hooks/useMaintenanceAnalytics";
import type {
  MaintenanceAnalyticsFilters,
  MaintenanceDrilldownTicketResponse,
  MaintenanceHeatmapCellResponse,
} from "@/lib/types/maintenanceAnalytics";
import type { MaintenanceTicketResponse } from "@/lib/types/maintenanceTicket";
import { cn } from "@/lib/utils";
import { prioColor, statusColor } from "@/lib/utils/maintenanceUtils";
import { useRouter } from "@tanstack/react-router";
import { Filter } from "lucide-react";
import { useState } from "react";

interface MaintenanceDrilldownPanelProps {
  buildingId: string;
  selectedCell?: MaintenanceHeatmapCellResponse | null;
  filters: Omit<MaintenanceAnalyticsFilters, "buildingId">;
  isAdmin: boolean;
}

export function MaintenanceDrilldownPanel({
  buildingId,
  selectedCell,
  filters,
  isAdmin,
}: MaintenanceDrilldownPanelProps) {
  const router = useRouter();
  const [editingTicket, setEditingTicket] = useState<MaintenanceTicketResponse | null>(null);

  const params = selectedCell
    ? {
        ...filters,
        buildingId,
        floorId: selectedCell.floorId,
        blockId: selectedCell.blockId,
      }
    : undefined;

  const { data, isLoading, isError } = useMaintenanceDrilldown(params);

  if (!selectedCell) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Block Details</CardTitle>
          <CardDescription>Select a heatmap cell to inspect tickets and trends.</CardDescription>
        </CardHeader>
      </Card>
    );
  }

  if (isLoading) {
    return <Skeleton className="h-[520px] w-full" />;
  }

  if (isError || !data) {
    return <div className="rounded-md border p-6 text-destructive">Failed to load block analytics.</div>;
  }

  const maxRoomTickets = Math.max(...data.roomBars.map((bar) => bar.ticketsCount), 0);
  const maxTimelineTickets = Math.max(...data.timeline.map((point) => point.ticketsCount), 0);

  const viewRoom = (roomId: string) => {
    router.navigate({ to: `/rooms/${roomId}` });
  };

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>
            Floor {data.floorNumber}, {data.blockLabel}
          </CardTitle>
          <CardDescription>{data.diagnosticMessage}</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid gap-3 md:grid-cols-4">
            <SummaryTile label="Tickets" value={data.summary.ticketsCount.toString()} />
            <SummaryTile label="Open" value={data.summary.openCount.toString()} />
            <SummaryTile label="Resolved" value={`${data.summary.resolvedPercentage}%`} />
            <SummaryTile label="Avg days" value={data.summary.averageDaysInWork.toString()} />
          </div>
        </CardContent>
      </Card>

      <div className="grid gap-6 xl:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Requests By Room</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {data.roomBars.length === 0 ? (
              <div className="text-sm text-muted-foreground">No room data for the selected filters.</div>
            ) : (
              data.roomBars.map((bar) => (
                <div key={bar.roomId} className="space-y-1">
                  <div className="flex items-center justify-between text-sm">
                    <span>Room {bar.roomLabel}</span>
                    <span className="font-medium">{bar.ticketsCount}</span>
                  </div>
                  <SegmentBar value={bar.ticketsCount} max={maxRoomTickets} />
                </div>
              ))
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Request Dynamics</CardTitle>
          </CardHeader>
          <CardContent>
            {data.timeline.length === 0 ? (
              <div className="text-sm text-muted-foreground">No timeline data for the selected filters.</div>
            ) : (
              <div className="flex h-44 items-end gap-2 overflow-x-auto">
                {data.timeline.map((point) => (
                  <div key={point.periodStart} className="flex min-w-14 flex-col items-center gap-2">
                    <div className="flex h-28 items-end">
                      <div className={cn("w-8 rounded-t bg-primary", getTimelineHeight(point.ticketsCount, maxTimelineTickets))} />
                    </div>
                    <span className="text-xs font-medium">{point.ticketsCount}</span>
                    <span className="text-center text-[10px] text-muted-foreground">{point.label}</span>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Tickets</CardTitle>
          <CardDescription>Room, description, assignee, status, and days in work.</CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Room</TableHead>
                <TableHead>Opened</TableHead>
                <TableHead>Priority</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Assignee</TableHead>
                <TableHead>Days</TableHead>
                <TableHead>Description</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.tickets.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={8} className="py-6 text-center text-muted-foreground">
                    No tickets match the selected filters.
                  </TableCell>
                </TableRow>
              ) : (
                data.tickets.map((ticket) => (
                  <TableRow key={ticket.id}>
                    <TableCell>Room {ticket.roomLabel}</TableCell>
                    <TableCell>{new Date(ticket.createdAt).toLocaleDateString()}</TableCell>
                    <TableCell>
                      <Badge variant="outline" className={prioColor(ticket.priority)}>
                        {ticket.priority}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Badge variant="outline" className={statusColor(ticket.status)}>
                        {ticket.status === "InProgress" ? "In Progress" : ticket.status}
                      </Badge>
                    </TableCell>
                    <TableCell>{formatAssignee(ticket)}</TableCell>
                    <TableCell>{ticket.daysInWork}</TableCell>
                    <TableCell className="max-w-[260px] truncate">{ticket.description}</TableCell>
                    <TableCell className="text-right">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="sm">
                            <Filter className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem onClick={() => viewRoom(ticket.roomId)}>View room</DropdownMenuItem>
                          {isAdmin && (
                            <DropdownMenuItem onClick={() => setEditingTicket(toMaintenanceTicketResponse(ticket))}>
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

      {editingTicket && (
        <EditTicketDialog
          key={editingTicket.id}
          ticket={editingTicket}
          open={!!editingTicket}
          onOpenChange={(open) => {
            if (!open) {
              setEditingTicket(null);
            }
          }}
        />
      )}
    </div>
  );
}

function SummaryTile({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border bg-muted/30 p-4">
      <div className="text-sm text-muted-foreground">{label}</div>
      <div className="text-2xl font-bold">{value}</div>
    </div>
  );
}

function SegmentBar({ value, max }: { value: number; max: number }) {
  const segmentCount = 12;
  const filledSegments = max === 0 ? 0 : Math.max(1, Math.ceil((value / max) * segmentCount));

  return (
    <div className="grid grid-cols-12 gap-1">
      {Array.from({ length: segmentCount }).map((_, index) => (
        <div
          key={index}
          className={cn("h-2 rounded-full", index < filledSegments ? "bg-primary" : "bg-muted")}
        />
      ))}
    </div>
  );
}

function getTimelineHeight(value: number, max: number) {
  if (max === 0 || value === 0) {
    return "h-2";
  }

  const ratio = value / max;
  if (ratio >= 0.8) return "h-28";
  if (ratio >= 0.6) return "h-20";
  if (ratio >= 0.4) return "h-14";
  if (ratio >= 0.2) return "h-8";
  return "h-4";
}

function formatAssignee(ticket: MaintenanceDrilldownTicketResponse) {
  if (!ticket.assignedTo) {
    return "Unassigned";
  }

  return `${ticket.assignedTo.firstName} ${ticket.assignedTo.lastName}`.trim() || ticket.assignedTo.email;
}

function toMaintenanceTicketResponse(ticket: MaintenanceDrilldownTicketResponse): MaintenanceTicketResponse {
  return {
    id: ticket.id,
    room: {
      id: ticket.roomId,
      label: ticket.roomLabel,
    },
    title: ticket.title,
    description: ticket.description,
    status: ticket.status,
    createdAt: ticket.createdAt,
    resolvedAt: ticket.resolvedAt,
    reporter: {
      id: "00000000-0000-0000-0000-000000000000",
      firstName: "",
      lastName: "",
      email: "",
      roles: [],
    },
    assignedTo: ticket.assignedTo,
    priority: ticket.priority,
  };
}
