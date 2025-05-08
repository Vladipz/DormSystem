import {
  Badge,
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui";
import { HardHat, Loader2, PenTool } from "lucide-react";

interface MaintenanceTicket {
  id: string;
  title: string;
  description: string;
  status: string;
  createdAt: string;
  resolvedAt?: string | null;
  reporter?: {
    firstName: string;
    lastName: string;
  };
}

interface MaintenanceHistoryProps {
  tickets: MaintenanceTicket[];
  isLoading: boolean;
}

export function MaintenanceHistory({
  tickets,
  isLoading,
}: MaintenanceHistoryProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Maintenance History</CardTitle>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="flex h-24 items-center justify-center">
            <Loader2 className="text-primary h-6 w-6 animate-spin" />
            <span className="ml-2">Loading tickets...</span>
          </div>
        ) : (
          <div className="space-y-4">
            {tickets.map((ticket) => (
              <div key={ticket.id} className="rounded-lg border p-4">
                <div className="mb-2 flex items-start justify-between gap-2">
                  <div className="flex min-w-0 items-center">
                    <HardHat className="text-primary mr-2 h-5 w-5 shrink-0" />
                    <h3 className="line-clamp-1 text-sm font-medium break-words">
                      {ticket.title}
                    </h3>
                  </div>
                  <Badge
                    className="shrink-0"
                    variant={
                      ticket.status === "Resolved" ? "default" : "secondary"
                    }
                  >
                    {ticket.status}
                  </Badge>
                </div>
                <div className="text-muted-foreground space-y-1 text-sm">
                  <p className="line-clamp-2 break-words">
                    {ticket.description}
                  </p>
                  <p>Reported by: {ticket.reporter?.firstName || "Unknown"}</p>
                  <p>
                    Reported on:{" "}
                    {new Date(ticket.createdAt).toLocaleDateString()}
                  </p>
                  {ticket.resolvedAt && (
                    <p>
                      Completed on:{" "}
                      {new Date(ticket.resolvedAt).toLocaleDateString()}
                    </p>
                  )}
                </div>
              </div>
            ))}
            {tickets.length === 0 && (
              <div className="text-muted-foreground py-6 text-center">
                <PenTool className="mx-auto mb-2 h-12 w-12 opacity-20" />
                <p>No maintenance history available</p>
              </div>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
