import { PageHeader } from "@/components/PageHeader";
import {
  Avatar,
  AvatarFallback,
  AvatarImage,
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
import { useAuth } from "@/lib/hooks/useAuth";
import {
  useCreateMaintenanceTicket,
  useMaintenanceTickets,
} from "@/lib/hooks/useMaintenanceTicket";
import { useRoomById } from "@/lib/hooks/useRooms";
import { createFileRoute, useRouter } from "@tanstack/react-router";
import {
  ArrowLeft,
  ArrowRight,
  Bed,
  Calendar,
  Clipboard,
  Home,
  Loader2,
  PenToolIcon as Tool,
  User,
} from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";

export const Route = createFileRoute("/_mainLayout/rooms/$roomId/")({
  component: RoomDetails,
});

interface RoomDetailsProps {
  roomId: string;
  onBack?: () => void;
}

export function RoomDetails({ onBack }: RoomDetailsProps) {
  const { roomId } = Route.useParams();
  const router = useRouter();
  const { user } = useAuth();
  const [isRequestMoveDialogOpen, setIsRequestMoveDialogOpen] = useState(false);
  const [isMaintenanceDialogOpen, setIsMaintenanceDialogOpen] = useState(false);
  const [maintenanceDescription, setMaintenanceDescription] = useState("");
  const [maintenanceTitle, setMaintenanceTitle] = useState("");

  // Fetch room data from backend
  const {
    data: room,
    isLoading: isRoomLoading,
    error: roomError,
  } = useRoomById(roomId);

  // Fetch maintenance tickets for this room
  const { data: maintenanceData, isLoading: isMaintenanceLoading } =
    useMaintenanceTickets({
      roomId: roomId,
      pageSize: 10,
      page: 1,
    });

  const { mutate: createTicket, isPending: isCreatingTicket } =
    useCreateMaintenanceTicket();

  const handleBack = () => {
    if (onBack) {
      onBack();
    } else {
      router.navigate({
        to: "/",
        search: { blockId: room?.blockId },
      });
    }
  };

  const handleRequestMove = () => {
    // In a real app, this would send the request to an API
    setIsRequestMoveDialogOpen(false);
    toast.info("Room move request functionality is not implemented yet");
  };

  const handleReportMaintenance = () => {
    if (!maintenanceTitle.trim() || !maintenanceDescription.trim()) {
      toast.error("Please provide both title and description");
      return;
    }

    if (!user?.id) {
      toast.error("You must be logged in to report maintenance issues");
      return;
    }

    createTicket(
      {
        roomId: roomId.toString(),
        title: maintenanceTitle,
        description: maintenanceDescription,
        reporterById: user.id,
        priority: "Medium", // Default priority
      },
      {
        onSuccess: () => {
          setMaintenanceTitle("");
          setMaintenanceDescription("");
          setIsMaintenanceDialogOpen(false);
        },
      }
    );
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

  const getGenderRuleColor = (genderRule: string) => {
    switch (genderRule) {
      case "male":
        return "bg-blue-100 text-blue-800";
      case "female":
        return "bg-pink-100 text-pink-800";
      case "mixed":
        return "bg-purple-100 text-purple-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  // Handle loading state
  if (isRoomLoading) {
    return (
      <div className="flex items-center justify-center h-48">
        <Loader2 className="w-8 h-8 animate-spin text-primary" />
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
          <ArrowLeft className="mr-2 h-4 w-4" />
          Go Back
        </Button>
      </div>
    );
  }

  // Use the actual room data but keep mocked data for parts not available from the API
  const roomPhotos = [
    {
      id: 1,
      url: "/placeholder.svg?height=300&width=400",
      caption: "Room overview",
    },
    {
      id: 2,
      url: "/placeholder.svg?height=300&width=400",
      caption: "Window view",
    },
    {
      id: 3,
      url: "/placeholder.svg?height=300&width=400",
      caption: "Bathroom",
    },
  ];

  // Mock data for places and occupants, as these seem to be missing from the API
  const places = Array.from({ length: room.capacity }, (_, i) => ({
    id: i + 1,
    roomId: room.id,
    index: i + 1,
    occupiedByUserId: i < 2 ? `${i}` : null,
    movedInAt: i < 2 ? "2023-09-01" : null,
    movedOutAt: null,
    occupant:
      i < 2
        ? {
            id: i.toString(),
            name: ["Alex Kovalenko", "Maria Shevchenko", "Ivan Petrenko"][i],
            avatar: "/placeholder.svg?height=40&width=40",
            faculty: ["Computer Science", "Economics", "Law"][i],
            year: (i % 4) + 1,
          }
        : null,
  }));

  return (
    <div className="space-y-6 pt-3">
      <PageHeader
        title={`${room.label} Room Details`}
        backTo="/"
        backButtonLabel="Back"
        actions={
          <div className="flex gap-2">
            <Dialog
              open={isMaintenanceDialogOpen}
              onOpenChange={setIsMaintenanceDialogOpen}
            >
              <DialogTrigger asChild>
                <Button variant="outline">
                  <Tool className="mr-2 h-4 w-4" /> Report Maintenance
                </Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>Report Maintenance Issue</DialogTitle>
                  <DialogDescription>
                    Describe the maintenance issue in detail. Our team will
                    address it as soon as possible.
                  </DialogDescription>
                </DialogHeader>
                <div className="space-y-4 py-4">
                  <div className="space-y-2">
                    <Label htmlFor="title">Title</Label>
                    <input
                      id="title"
                      className="w-full p-2 border rounded"
                      placeholder="Brief title of the issue..."
                      value={maintenanceTitle}
                      onChange={(e) => setMaintenanceTitle(e.target.value)}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="description">Description</Label>
                    <Textarea
                      id="description"
                      placeholder="Describe the issue..."
                      rows={4}
                      value={maintenanceDescription}
                      onChange={(e) =>
                        setMaintenanceDescription(e.target.value)
                      }
                    />
                  </div>
                </div>
                <DialogFooter>
                  <Button
                    variant="outline"
                    onClick={() => setIsMaintenanceDialogOpen(false)}
                    disabled={isCreatingTicket}
                  >
                    Cancel
                  </Button>
                  <Button
                    onClick={handleReportMaintenance}
                    disabled={isCreatingTicket}
                  >
                    {isCreatingTicket ? (
                      <>
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        Submitting...
                      </>
                    ) : (
                      "Submit Report"
                    )}
                  </Button>
                </DialogFooter>
              </DialogContent>
            </Dialog>

            <Dialog
              open={isRequestMoveDialogOpen}
              onOpenChange={setIsRequestMoveDialogOpen}
            >
              <DialogTrigger asChild>
                <Button>
                  <ArrowRight className="mr-2 h-4 w-4" /> Request Move
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
      {/* <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" onClick={handleBack}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back
          </Button>
          <h1 className="text-2xl font-bold">Room {room.label}</h1>
          <Badge className={getRoomStatusColor(room.status)} variant="outline">
            {room.status}
          </Badge>
        </div>
      </div> */}

      <div className="grid gap-6 md:grid-cols-3">
        <div className="md:col-span-2 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Room Information</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid gap-6 md:grid-cols-2">
                <div>
                  <div className="aspect-video rounded-md overflow-hidden mb-4">
                    <img
                      src="/placeholder.svg?height=300&width=500"
                      alt={`Room ${room.label}`}
                      className="w-full h-full object-cover"
                    />
                  </div>
                  <div className="grid grid-cols-3 gap-2">
                    {roomPhotos.map((photo) => (
                      <div
                        key={photo.id}
                        className="aspect-square rounded-md overflow-hidden"
                      >
                        <img
                          src={photo.url || "/placeholder.svg"}
                          alt={photo.caption}
                          className="w-full h-full object-cover"
                        />
                      </div>
                    ))}
                  </div>
                </div>
                <div className="space-y-4">
                  <div>
                    <h3 className="font-medium text-sm text-muted-foreground mb-1">
                      Location
                    </h3>
                    <p className="font-medium">
                      {/* Building information is not available in current API */}
                      Block {room.blockId}
                    </p>
                  </div>
                  <div>
                    <h3 className="font-medium text-sm text-muted-foreground mb-1">
                      Room Type
                    </h3>
                    <p className="font-medium capitalize">{room.roomType}</p>
                  </div>
                  <div>
                    <h3 className="font-medium text-sm text-muted-foreground mb-1">
                      Capacity
                    </h3>
                    <p className="font-medium">{room.capacity} places</p>
                  </div>
                  <div>
                    <h3 className="font-medium text-sm text-muted-foreground mb-1">
                      Gender Rule
                    </h3>
                    {/* Gender rule is not available in current API */}
                    <Badge
                      className={getGenderRuleColor("mixed")}
                      variant="outline"
                    >
                      mixed
                    </Badge>
                  </div>
                  <div>
                    <h3 className="font-medium text-sm text-muted-foreground mb-1">
                      Amenities
                    </h3>
                    <div className="flex flex-wrap gap-2">
                      {room.amenities &&
                        room.amenities.map((amenity) => (
                          <Badge key={amenity} variant="outline">
                            {amenity}
                          </Badge>
                        ))}
                    </div>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          <Tabs defaultValue="places">
            <TabsList>
              <TabsTrigger value="places">Places & Residents</TabsTrigger>
              <TabsTrigger value="maintenance">Maintenance History</TabsTrigger>
            </TabsList>
            <TabsContent value="places">
              <Card>
                <CardHeader>
                  <CardTitle>Places & Residents</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    {places.map((place) => (
                      <div key={place.id} className="border rounded-lg p-4">
                        <div className="flex items-center justify-between mb-2">
                          <div className="flex items-center">
                            <Bed className="mr-2 h-5 w-5 text-primary" />
                            <h3 className="font-medium">Place {place.index}</h3>
                          </div>
                          <Badge
                            variant={
                              place.occupiedByUserId ? "secondary" : "outline"
                            }
                          >
                            {place.occupiedByUserId ? "Occupied" : "Available"}
                          </Badge>
                        </div>
                        {place.occupant ? (
                          <div className="flex items-start gap-4">
                            <Avatar className="h-10 w-10">
                              <AvatarImage
                                src={
                                  place.occupant.avatar || "/placeholder.svg"
                                }
                                alt={place.occupant.name}
                              />
                              <AvatarFallback>
                                {place.occupant.name.charAt(0)}
                              </AvatarFallback>
                            </Avatar>
                            <div>
                              <p className="font-medium">
                                {place.occupant.name}
                              </p>
                              <p className="text-sm text-muted-foreground">
                                {place.occupant.faculty}, Year{" "}
                                {place.occupant.year}
                              </p>
                              <p className="text-xs text-muted-foreground mt-1">
                                Moved in:{" "}
                                {new Date(place.movedInAt).toLocaleDateString()}
                              </p>
                            </div>
                          </div>
                        ) : (
                          <div className="flex items-center text-muted-foreground">
                            <User className="mr-2 h-5 w-5" />
                            <span>No resident assigned</span>
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>
            </TabsContent>
            <TabsContent value="maintenance">
              <Card>
                <CardHeader>
                  <CardTitle>Maintenance History</CardTitle>
                </CardHeader>
                <CardContent>
                  {isMaintenanceLoading ? (
                    <div className="flex items-center justify-center h-24">
                      <Loader2 className="w-6 h-6 animate-spin text-primary" />
                      <span className="ml-2">Loading tickets...</span>
                    </div>
                  ) : (
                    <div className="space-y-4">
                      {maintenanceData?.items &&
                        maintenanceData.items.map((ticket) => (
                          <div
                            key={ticket.id}
                            className="border rounded-lg p-4"
                          >
                            <div className="flex items-center justify-between mb-2">
                              <div className="flex items-center">
                                <Tool className="mr-2 h-5 w-5 text-primary" />
                                <h3 className="font-medium">{ticket.title}</h3>
                              </div>
                              <Badge
                                variant={
                                  ticket.status === "Resolved"
                                    ? "success"
                                    : "secondary"
                                }
                              >
                                {ticket.status}
                              </Badge>
                            </div>
                            <div className="text-sm text-muted-foreground">
                              <p>{ticket.description}</p>
                              <p>
                                Reported by:{" "}
                                {ticket.reporter?.name || "Unknown"}
                              </p>
                              <p>
                                Reported on:{" "}
                                {new Date(
                                  ticket.createdAt
                                ).toLocaleDateString()}
                              </p>
                              {ticket.resolvedAt && (
                                <p>
                                  Completed on:{" "}
                                  {new Date(
                                    ticket.resolvedAt
                                  ).toLocaleDateString()}
                                </p>
                              )}
                            </div>
                          </div>
                        ))}
                      {(!maintenanceData?.items ||
                        maintenanceData.items.length === 0) && (
                        <div className="text-center py-6 text-muted-foreground">
                          <Tool className="mx-auto h-12 w-12 mb-2 opacity-20" />
                          <p>No maintenance history available</p>
                        </div>
                      )}
                    </div>
                  )}
                </CardContent>
              </Card>
            </TabsContent>
          </Tabs>
        </div>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Room Status</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
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
                    {places.filter((p) => p.occupiedByUserId).length}/
                    {room.capacity}
                  </span>
                </div>
                <Separator />
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
              <Button
                variant="outline"
                className="w-full justify-start"
                onClick={() => setIsMaintenanceDialogOpen(true)}
              >
                <Tool className="mr-2 h-4 w-4" />
                Report Maintenance Issue
              </Button>
              <Button
                variant="outline"
                className="w-full justify-start"
                onClick={() => setIsRequestMoveDialogOpen(true)}
              >
                <ArrowRight className="mr-2 h-4 w-4" />
                Request Room Change
              </Button>
              <Button variant="outline" className="w-full justify-start">
                <Calendar className="mr-2 h-4 w-4" />
                View Cleaning Schedule
              </Button>
              <Button variant="outline" className="w-full justify-start">
                <Home className="mr-2 h-4 w-4" />
                View Block Details
              </Button>
              <Button variant="outline" className="w-full justify-start">
                <Clipboard className="mr-2 h-4 w-4" />
                Room Inventory Checklist
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
