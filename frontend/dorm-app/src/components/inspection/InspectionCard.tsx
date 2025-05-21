import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import type { ShortInspection } from "@/lib/types/inspection";
import { format, isPast, isToday } from "date-fns";
import {
  CalendarClock,
  CheckCircle2,
  ClipboardCheck,
  FileText,
} from "lucide-react";

interface InspectionCardProps {
  inspection: ShortInspection;
  onClick: () => void;
}

export function InspectionCard({ inspection, onClick }: InspectionCardProps) {
  const totalRooms = inspection.roomsCount;
  const pendingRooms = inspection.pendingRoomsCount;
  const confirmedRooms = inspection.confirmedRoomsCount;
  const notConfirmedRooms = inspection.notConfirmedRoomsCount;
  const noAccessRooms = inspection.noAccessRoomsCount;

  const getStatusIcon = () => {
    switch (inspection.status) {
      case "Scheduled":
        return <CalendarClock className="h-5 w-5 text-blue-500" />;
      case "Active":
        return <ClipboardCheck className="h-5 w-5 text-green-500" />;
      case "Completed":
        return <FileText className="h-5 w-5 text-gray-500" />;
    }
  };

  const getStatusBadge = () => {
    switch (inspection.status) {
      case "Scheduled":
        return (
          <Badge
            variant="outline"
            className="border-blue-200 bg-blue-50 text-blue-700"
          >
            Scheduled
          </Badge>
        );
      case "Active":
        return (
          <Badge
            variant="outline"
            className="border-green-200 bg-green-50 text-green-700"
          >
            Active
          </Badge>
        );
      case "Completed":
        return (
          <Badge
            variant="outline"
            className="border-gray-200 bg-gray-50 text-gray-700"
          >
            Completed
          </Badge>
        );
    }
  };

  const getDateDisplay = () => {
    const date = inspection.startDate;

    if (isToday(date)) {
      return "Today";
    } else if (isPast(date)) {
      return `${format(date, "MMM d, yyyy")} (Past)`;
    } else {
      return format(date, "MMM d, yyyy");
    }
  };

  const getCompletionStatus = () => {
    if (inspection.status === "Completed") {
      return (
        <div className="flex items-center text-gray-600">
          <CheckCircle2 className="mr-1 h-4 w-4" />
          Completed
        </div>
      );
    }

    if (totalRooms === 0) return null;

    const percentage = Math.round(
      ((confirmedRooms + notConfirmedRooms + noAccessRooms) / totalRooms) * 100,
    );

    return (
      <div className="text-sm text-gray-500">
        {pendingRooms === 0 ? "All rooms inspected" : `${percentage}% complete`}
      </div>
    );
  };

  return (
    <Card
      className="cursor-pointer transition-shadow hover:shadow-md"
      onClick={onClick}
    >
      <CardContent className="p-5">
        <div className="flex items-start justify-between">
          <div className="space-y-1">
            <div className="flex items-center">
              {getStatusIcon()}
              <h3 className="ml-2 font-semibold">{inspection.name}</h3>
            </div>
            <p className="text-sm text-gray-500">{inspection.type}</p>
          </div>
          {getStatusBadge()}
        </div>

        <div className="mt-4 flex justify-between border-t pt-4">
          <div className="text-sm">
            <div className="font-medium">{getDateDisplay()}</div>
            <div className="text-gray-500">
              {format(inspection.startDate, "h:mm a")}
            </div>
          </div>

          <div className="text-right">
            <div className="font-medium text-gray-700">
              {confirmedRooms + notConfirmedRooms + noAccessRooms} of{" "}
              {totalRooms} rooms
            </div>
            {getCompletionStatus()}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
