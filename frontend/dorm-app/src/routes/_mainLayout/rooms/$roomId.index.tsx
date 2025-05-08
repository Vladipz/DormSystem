import { CreateTicketDialog } from "@/components/dialogs/CreateTicketDialog";
import { PageHeader } from "@/components/PageHeader";
import { MaintenanceHistory } from "@/components/room/MaintenanceHistory";
import { PlacesAndResidents } from "@/components/room/PlacesAndResidents";
import { RoomInfoCard } from "@/components/room/RoomInfoCard";
import {
  Badge,
  Button,
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  Label,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  Separator,
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
  Textarea,
} from "@/components/ui";
import { useMaintenanceTickets } from "@/lib/hooks/useMaintenanceTicket";
import { usePlaces } from "@/lib/hooks/usePlaces";
import { useRoomById } from "@/lib/hooks/useRooms";
import {
  createFileRoute,
  useNavigate,
  useRouter,
} from "@tanstack/react-router";
import {
  ArrowLeft,
  ArrowRight,
  Loader2,
  PenToolIcon as Tool,
} from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";

export const Route = createFileRoute("/_mainLayout/rooms/$roomId/")({
  component: RoomDetails,
});

// interface RoomDetailsProps {
//   roomId: string;
//   onBack?: () => void;
// }

export function RoomDetails() {
  const { roomId } = Route.useParams();
  const router = useRouter();
  const navigate = useNavigate();
  const [isRequestMoveDialogOpen, setIsRequestMoveDialogOpen] = useState(false);

  // Fetch room data from backend
  const {
    data: room,
    isLoading: isRoomLoading,
    error: roomError,
  } = useRoomById(roomId);

  const { data: places, isLoading } = usePlaces({
    roomId: roomId,
  });

  // Fetch maintenance tickets for this room
  const { data: maintenanceData, isLoading: isMaintenanceLoading } =
    useMaintenanceTickets({
      roomId: roomId,
      pageSize: 10,
      page: 1,
    });

  const handleBack = () => {
    if (window.history.length > 1) {
      router.history.back();
    } else {
      navigate({ to: "/" });
    }
  };

  const handleRequestMove = () => {
    // In a real app, this would send the request to an API
    setIsRequestMoveDialogOpen(false);
    toast.info("Room move request functionality is not implemented yet");
  };

  const getRoomStatusColor = (status: string) => {
    switch (status) {
      case "Available":
        return "bg-green-100 text-green-800";
      case "Occupied":
        return "bg-blue-100 text-blue-800";
      case "Maintenance":
        return "bg-red-100 text-red-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  // Handle loading state
  if (isRoomLoading) {
    return (
      <div className="flex h-48 items-center justify-center">
        <Loader2 className="text-primary h-8 w-8 animate-spin" />
        <span className="ml-2">Loading room data...</span>
      </div>
    );
  }

  // Handle error state
  if (roomError || !room) {
    return (
      <div className="p-6 text-center">
        <h2 className="text-xl font-bold text-red-500">
          Error loading room data
        </h2>
        <p className="mt-2">
          Unable to load the room details. Please try again later.
        </p>
        <Button className="mt-4" onClick={handleBack}>
          <ArrowLeft className="h-4 w-4" />
          Go Back
        </Button>
      </div>
    );
  }

  // Mock data for places and occupants, as these seem to be missing from the API
  // const places = Array.from({ length: room.capacity }, (_, i) => ({
  //   id: i + 1,
  //   roomId: room.id,
  //   index: i + 1,
  //   occupiedByUserId: i < 2 ? `${i}` : null,
  //   movedInAt: i < 2 ? "2023-09-01" : null,
  //   movedOutAt: null,
  //   occupant:
  //     i < 2
  //       ? {
  //           id: i.toString(),
  //           name: ["Alex Kovalenko", "Maria Shevchenko", "Ivan Petrenko"][i],
  //           avatar: "/placeholder.svg?height=40&width=40",
  //           faculty: ["Computer Science", "Economics", "Law"][i],
  //           year: (i % 4) + 1,
  //         }
  //       : null,
  // }));

  return (
    <div className="space-y-6 pt-3">
      <PageHeader
        title={`${room.label} Room Details`}
        backTo="/room-dashboard"
        backButtonLabel="Back"
        actions={
          <div className="flex gap-2">
            <CreateTicketDialog
              preselectedRoomId={room.id}
              preselectedRoomLabel={room.label}
            />
            <Dialog
              open={isRequestMoveDialogOpen}
              onOpenChange={setIsRequestMoveDialogOpen}
            >
              <DialogTrigger asChild>
                <Button>
                  <ArrowRight className="h-4 w-4" /> Request Move
                </Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>Request Room Change</DialogTitle>
                  <DialogDescription>
                    Select your preferred room type and location. Your request
                    will be reviewed by the dormitory administration.
                  </DialogDescription>
                </DialogHeader>
                <div className="space-y-4 py-4">
                  <div className="space-y-2">
                    <Label htmlFor="room-type">Preferred Room Type</Label>
                    <Select defaultValue="double">
                      <SelectTrigger id="room-type">
                        <SelectValue placeholder="Select room type" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="single">Single</SelectItem>
                        <SelectItem value="double">Double</SelectItem>
                        <SelectItem value="triple">Triple</SelectItem>
                        <SelectItem value="suite">Suite</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="building">Preferred Building</Label>
                    <Select defaultValue="any">
                      <SelectTrigger id="building">
                        <SelectValue placeholder="Select building" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="any">Any Building</SelectItem>
                        <SelectItem value="a">Building A</SelectItem>
                        <SelectItem value="b">Building B</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="reason">Reason for Move</Label>
                    <Textarea
                      id="reason"
                      placeholder="Explain why you want to change rooms..."
                      rows={3}
                    />
                  </div>
                </div>
                <DialogFooter>
                  <Button
                    variant="outline"
                    onClick={() => setIsRequestMoveDialogOpen(false)}
                  >
                    Cancel
                  </Button>
                  <Button onClick={handleRequestMove}>Submit Request</Button>
                </DialogFooter>
              </DialogContent>
            </Dialog>
          </div>
        }
      ></PageHeader>
      <div className="grid gap-6 md:grid-cols-3">
        <div className="space-y-6 md:col-span-2">
          <RoomInfoCard room={room} />
          <Tabs defaultValue="places">
            <TabsList>
              <TabsTrigger value="places">Places & Residents</TabsTrigger>
              <TabsTrigger value="maintenance">Maintenance History</TabsTrigger>
            </TabsList>
            <TabsContent value="places">
              <PlacesAndResidents places={places?.items || []} />
            </TabsContent>
            <TabsContent value="maintenance">
              <MaintenanceHistory
                tickets={maintenanceData?.items || []}
                isLoading={isMaintenanceLoading}
              />
            </TabsContent>
          </Tabs>
        </div>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Room Status</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-5">
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium">Current Status</span>
                  <Badge
                    className={getRoomStatusColor(room.status)}
                    variant="outline"
                  >
                    {room.status}
                  </Badge>
                </div>
                <Separator />
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium">Occupancy</span>
                  <span className="text-sm">
                    {/* This is mocked as the current API doesn't provide occupancy */}
                    {places?.items.filter((p) => p.isOccupied).length}/
                    {room.capacity}
                  </span>
                </div>
                <Separator />
                {/* TODO: Add actual data for this */}
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium">Last Inspection</span>
                  <span className="text-sm">2023-06-15</span>
                </div>
                <Separator />
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium">Next Inspection</span>
                  <span className="text-sm">2023-12-15</span>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Quick Actions</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              <CreateTicketDialog
                preselectedRoomId={room.id}
                preselectedRoomLabel={room.label}
                trigger={
                  <Button
                    variant="outline"
                    className="w-full justify-start gap-2 px-4 py-2 text-sm font-medium"
                  >
                    <Tool className="h-4 w-4" />
                    <span className="truncate">Report Maintenance Issue</span>
                  </Button>
                }
              />
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
