"use client";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { ReportStyle } from "@/lib/services/inspectionService";
import type { Inspection, RoomInspection, RoomInspectionStatus } from "@/lib/types/inspection";
import { format } from "date-fns";
import {
    AlertCircle,
    ArrowLeft,
    CheckCircle2,
    ChevronDown,
    ClipboardCheck,
    DoorClosed,
    FileText,
    XCircle,
} from "lucide-react";
import { useMemo, useState } from "react";

interface InspectionDetailsProps {
  inspection: Inspection;
  onBack: () => void;
  onUpdateRoomStatus: (
    roomId: string,
    status: RoomInspectionStatus,
    comment?: string,
  ) => void;
  onCompleteInspection: () => void;
  onGenerateReport: (style?: ReportStyle) => void;
  onStartInspection: () => void;
  isGeneratingReport?: boolean;
}

export function InspectionDetails({
  inspection,
  onBack,
  onUpdateRoomStatus,
  onCompleteInspection,
  onGenerateReport,
  onStartInspection,
  isGeneratingReport,
}: InspectionDetailsProps) {
  const [selectedFloor, setSelectedFloor] = useState<string>("all");
  const [commentDialogOpen, setCommentDialogOpen] = useState(false);
  const [currentRoom, setCurrentRoom] = useState<RoomInspection | null>(null);
  const [comment, setComment] = useState("");  const [statusToSet, setStatusToSet] =
    useState<RoomInspectionStatus>("NotConfirmed");

  // Get unique floors from rooms
  //TODO: change to get floors from API
  const floors = useMemo(() => {
    const floorSet = new Set(inspection.rooms.map((room) => room.floor));
    return Array.from(floorSet).sort(
      (a, b) => Number.parseInt(a) - Number.parseInt(b),
    );
  }, [inspection.rooms]);

  // Filter rooms by selected floor
  const filteredRooms = useMemo(() => {
    if (selectedFloor === "all") {
      return inspection.rooms;
    }
    return inspection.rooms.filter((room) => room.floor === selectedFloor);
  }, [inspection.rooms, selectedFloor]);

  // Group rooms by floor for display
  const roomsByFloor = useMemo(() => {
    const grouped: Record<string, RoomInspection[]> = {};

    if (selectedFloor === "all") {
      // Group all rooms by floor
      inspection.rooms.forEach((room) => {
        if (!grouped[room.floor]) {
          grouped[room.floor] = [];
        }
        grouped[room.floor].push(room);
      });
    } else {
      // Only include the selected floor
      grouped[selectedFloor] = filteredRooms;
    }

    // Sort rooms within each floor by room number
    Object.keys(grouped).forEach((floor) => {
      grouped[floor].sort((a, b) =>
        a.roomNumber.localeCompare(b.roomNumber, undefined, { numeric: true }),
      );
    });

    return grouped;
  }, [inspection.rooms, filteredRooms, selectedFloor]);

  const allRoomsInspected = inspection.rooms.every(
    (room) =>
      room.status === "Confirmed" ||
      room.status === "NotConfirmed" ||
      room.status === "NoAccess",
  );

  const handleStatusUpdate = (
    room: RoomInspection,
    status: RoomInspectionStatus,
  ) => {
    if (status === "Confirmed" || status === "Pending") {
      onUpdateRoomStatus(room.id, status);
    } else {
      // For statuses that need a comment, open the dialog
      setCurrentRoom(room);
      setStatusToSet(status);
      setComment(room.comment || "");
      setCommentDialogOpen(true);
    }
  };
  const handleCommentSubmit = () => {
    if (currentRoom) {
      onUpdateRoomStatus(currentRoom.id, statusToSet, comment);
      setCommentDialogOpen(false);
      setCurrentRoom(null);
      setComment("");
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center space-x-2">
        <Button variant="ghost" size="sm" onClick={onBack}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Inspections
        </Button>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{inspection.name}</h1>
          <p className="text-muted-foreground">
            {inspection.type} • {format(inspection.startDate, "PPP p")}
          </p>
        </div>
        <div className="flex space-x-2">
          {inspection.status === "Scheduled" && (
            <Button onClick={onStartInspection}>
              <ClipboardCheck className="mr-2 h-4 w-4" />
              Start Inspection
            </Button>
          )}

          {inspection.status === "Active" && allRoomsInspected && (
            <Button onClick={onCompleteInspection}>
              <CheckCircle2 className="mr-2 h-4 w-4" />
              Complete Inspection
            </Button>
          )}

        {inspection.status === "Completed" && (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button disabled={isGeneratingReport}>
                  <FileText className="mr-2 h-4 w-4" />
                  Generate Report
                  <ChevronDown className="ml-2 h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent>
                <DropdownMenuItem
                  onClick={() => onGenerateReport("fancy")}
                >
                  Fancy Report
                </DropdownMenuItem>                <DropdownMenuItem
                  onClick={() => onGenerateReport("simple")}
                >
                  Simple Report
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          )}
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Inspection Summary</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Status:</span>
                <span>{getStatusBadge(inspection.status)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Total Rooms:</span>
                <span>{inspection.rooms.length}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Confirmed:</span>
                <span className="text-green-600">
                  {
                    inspection.rooms.filter((r) => r.status === "Confirmed")
                      .length
                  }
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Not Confirmed:</span>
                <span className="text-red-600">
                  {
                    inspection.rooms.filter((r) => r.status === "NotConfirmed")
                      .length
                  }
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">No Access:</span>
                <span className="text-amber-600">
                  {
                    inspection.rooms.filter((r) => r.status === "NoAccess")
                      .length
                  }
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Pending:</span>
                <span className="text-blue-600">
                  {
                    inspection.rooms.filter((r) => r.status === "Pending")
                      .length
                  }
                </span>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="mt-6 flex items-center justify-between">
        <h2 className="text-xl font-semibold">Rooms to Inspect</h2>
        <div className="flex items-center space-x-2">
          <span className="text-muted-foreground text-sm">
            Filter by Floor:
          </span>
          <Select value={selectedFloor} onValueChange={setSelectedFloor}>
            <SelectTrigger className="w-[120px]">
              <SelectValue placeholder="Select Floor" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Floors</SelectItem>
              {floors.map((floor) => (
                <SelectItem key={floor} value={floor}>
                  Floor {floor}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      <div className="space-y-6">
        {Object.entries(roomsByFloor)
          .sort(
            ([floorA], [floorB]) =>
              Number.parseInt(floorA) - Number.parseInt(floorB),
          )
          .map(([floor, rooms]) => (
            <div key={floor} className="space-y-2">
              <h3 className="text-lg font-medium">Floor {floor}</h3>
              <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {rooms.map((room) => (
                  <RoomInspectionCard
                    key={room.id}
                    room={room}
                    inspectionStatus={inspection.status}
                    onUpdateStatus={(status) =>
                      handleStatusUpdate(room, status)
                    }
                  />
                ))}
              </div>
            </div>
          ))}
      </div>

      {/* Comment Dialog for Not Confirmed or No Access */}
      <Dialog open={commentDialogOpen} onOpenChange={setCommentDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {statusToSet === "NotConfirmed" ? "Not Confirmed" : "No Access"}{" "}
              - Add Comment
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <p className="text-muted-foreground text-sm">
              {statusToSet === "NotConfirmed"
                ? "Please provide a reason why this room is not confirmed."
                : "Please provide details about why there was no access to this room."}
            </p>
            <Textarea
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              placeholder="Enter your comment here..."
              className="min-h-[100px]"
            />
            <div className="flex justify-end space-x-2">
              <Button
                variant="outline"
                onClick={() => setCommentDialogOpen(false)}
              >
                Cancel
              </Button>
              <Button onClick={handleCommentSubmit}>Submit</Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}

interface RoomInspectionCardProps {
  room: RoomInspection;
  inspectionStatus: "Scheduled" | "Active" | "Completed";
  onUpdateStatus: (status: RoomInspectionStatus) => void;
}

function RoomInspectionCard({
  room,
  inspectionStatus,
  onUpdateStatus,
}: RoomInspectionCardProps) {
  const isDisabled = inspectionStatus !== "Active";

  return (
    <Card>
      <CardContent className="p-4">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="font-medium">Room {room.roomNumber}</h3>
            <p className="text-muted-foreground text-sm">
              Floor {room.floor}, Building {room.building}
            </p>
          </div>
          <div className="flex items-center space-x-2">
            {room.status === "Pending" && (
              <div className="flex flex-col space-y-2">
                <Button
                  variant="outline"
                  size="sm"
                  className="text-green-600"
                  onClick={() => onUpdateStatus("Confirmed")}
                  disabled={isDisabled}
                >
                  <CheckCircle2 className="mr-1 h-4 w-4" />
                  Confirm
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  className="text-red-600"
                  onClick={() => onUpdateStatus("NotConfirmed")}
                  disabled={isDisabled}
                >
                  <XCircle className="mr-1 h-4 w-4" />
                  Not Confirm
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  className="text-amber-600"
                  onClick={() => onUpdateStatus("NoAccess")}
                  disabled={isDisabled}
                >
                  <DoorClosed className="mr-1 h-4 w-4" />
                  No Access
                </Button>
              </div>
            )}

            {room.status === "Confirmed" && (
              <Badge
                variant="outline"
                className="border-green-200 bg-green-50 text-green-700"
              >
                <CheckCircle2 className="mr-1 h-4 w-4" />
                Confirmed
              </Badge>
            )}

            {room.status === "NotConfirmed" && (
              <Badge
                variant="outline"
                className="border-red-200 bg-red-50 text-red-700"
              >
                <AlertCircle className="mr-1 h-4 w-4" />
                Not Confirmed
              </Badge>
            )}

            {room.status === "NoAccess" && (
              <Badge
                variant="outline"
                className="border-amber-200 bg-amber-50 text-amber-700"
              >
                <DoorClosed className="mr-1 h-4 w-4" />
                No Access
              </Badge>
            )}
          </div>
        </div>

        {(room.status === "NotConfirmed" || room.status === "NoAccess") &&
          room.comment && (
            <div
              className={`mt-2 rounded-md p-2 text-sm ${room.status === "NotConfirmed" ? "bg-red-50" : "bg-amber-50"}`}
            >
              <p
                className={`font-medium ${room.status === "NotConfirmed" ? "text-red-700" : "text-amber-700"}`}
              >
                Comment:
              </p>
              <p
                className={
                  room.status === "NotConfirmed"
                    ? "text-red-600"
                    : "text-amber-600"
                }
              >
                {room.comment}
              </p>
            </div>
          )}
      </CardContent>
    </Card>
  );
}

function getStatusBadge(status: "Scheduled" | "Active" | "Completed") {
  switch (status) {
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
}
