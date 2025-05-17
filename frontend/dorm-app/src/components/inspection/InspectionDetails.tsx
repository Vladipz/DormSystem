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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import type { Inspection, RoomInspection, RoomInspectionStatus } from "@/lib/types/inspection";
import { format } from "date-fns";
import {
  AlertCircle,
  ArrowLeft,
  CheckCircle2,
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
  onGenerateReport: () => void;
  onStartInspection: () => void;
}

export function InspectionDetails({
  inspection,
  onBack,
  onUpdateRoomStatus,
  onCompleteInspection,
  onGenerateReport,
  onStartInspection,
}: InspectionDetailsProps) {
  const [selectedFloor, setSelectedFloor] = useState<string>("all");
  const [commentDialogOpen, setCommentDialogOpen] = useState(false);
  const [currentRoom, setCurrentRoom] = useState<RoomInspection | null>(null);
  const [comment, setComment] = useState("");
  const [statusToSet, setStatusToSet] =
    useState<RoomInspectionStatus>("not_confirmed");

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
      room.status === "confirmed" ||
      room.status === "not_confirmed" ||
      room.status === "no_access",
  );

  const handleStatusUpdate = (
    room: RoomInspection,
    status: RoomInspectionStatus,
  ) => {
    if (status === "confirmed" || status === "pending") {
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
          {inspection.status === "scheduled" && (
            <Button onClick={onStartInspection}>
              <ClipboardCheck className="mr-2 h-4 w-4" />
              Start Inspection
            </Button>
          )}

          {inspection.status === "active" && allRoomsInspected && (
            <Button onClick={onCompleteInspection}>
              <CheckCircle2 className="mr-2 h-4 w-4" />
              Complete Inspection
            </Button>
          )}

          {inspection.status === "completed" && (
            <Button onClick={onGenerateReport}>
              <FileText className="mr-2 h-4 w-4" />
              Generate Report
            </Button>
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
                    inspection.rooms.filter((r) => r.status === "confirmed")
                      .length
                  }
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Not Confirmed:</span>
                <span className="text-red-600">
                  {
                    inspection.rooms.filter((r) => r.status === "not_confirmed")
                      .length
                  }
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">No Access:</span>
                <span className="text-amber-600">
                  {
                    inspection.rooms.filter((r) => r.status === "no_access")
                      .length
                  }
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Pending:</span>
                <span className="text-blue-600">
                  {
                    inspection.rooms.filter((r) => r.status === "pending")
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
              {statusToSet === "not_confirmed" ? "Not Confirmed" : "No Access"}{" "}
              - Add Comment
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <p className="text-muted-foreground text-sm">
              {statusToSet === "not_confirmed"
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
  inspectionStatus: "scheduled" | "active" | "completed";
  onUpdateStatus: (status: RoomInspectionStatus) => void;
}

function RoomInspectionCard({
  room,
  inspectionStatus,
  onUpdateStatus,
}: RoomInspectionCardProps) {
  const isDisabled = inspectionStatus !== "active";

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
            {room.status === "pending" && (
              <div className="flex flex-col space-y-2">
                <Button
                  variant="outline"
                  size="sm"
                  className="text-green-600"
                  onClick={() => onUpdateStatus("confirmed")}
                  disabled={isDisabled}
                >
                  <CheckCircle2 className="mr-1 h-4 w-4" />
                  Confirm
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  className="text-red-600"
                  onClick={() => onUpdateStatus("not_confirmed")}
                  disabled={isDisabled}
                >
                  <XCircle className="mr-1 h-4 w-4" />
                  Not Confirm
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  className="text-amber-600"
                  onClick={() => onUpdateStatus("no_access")}
                  disabled={isDisabled}
                >
                  <DoorClosed className="mr-1 h-4 w-4" />
                  No Access
                </Button>
              </div>
            )}

            {room.status === "confirmed" && (
              <Badge
                variant="outline"
                className="border-green-200 bg-green-50 text-green-700"
              >
                <CheckCircle2 className="mr-1 h-4 w-4" />
                Confirmed
              </Badge>
            )}

            {room.status === "not_confirmed" && (
              <Badge
                variant="outline"
                className="border-red-200 bg-red-50 text-red-700"
              >
                <AlertCircle className="mr-1 h-4 w-4" />
                Not Confirmed
              </Badge>
            )}

            {room.status === "no_access" && (
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

        {(room.status === "not_confirmed" || room.status === "no_access") &&
          room.comment && (
            <div
              className={`mt-2 rounded-md p-2 text-sm ${room.status === "not_confirmed" ? "bg-red-50" : "bg-amber-50"}`}
            >
              <p
                className={`font-medium ${room.status === "not_confirmed" ? "text-red-700" : "text-amber-700"}`}
              >
                Comment:
              </p>
              <p
                className={
                  room.status === "not_confirmed"
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

function getStatusBadge(status: "scheduled" | "active" | "completed") {
  switch (status) {
    case "scheduled":
      return (
        <Badge
          variant="outline"
          className="border-blue-200 bg-blue-50 text-blue-700"
        >
          Scheduled
        </Badge>
      );
    case "active":
      return (
        <Badge
          variant="outline"
          className="border-green-200 bg-green-50 text-green-700"
        >
          Active
        </Badge>
      );
    case "completed":
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
