import { PageHeader } from "@/components/PageHeader";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { useBuildings } from "@/lib/hooks/useBuildings";
import { useEvents } from "@/lib/hooks/useEvents";
import { useRooms } from "@/lib/hooks/useRooms";
import { authService } from "@/lib/services/authService";
import { BuildingsResponse } from "@/lib/types/building";
import type { EventDetails } from "@/lib/types/event";
import { RoomsResponse } from "@/lib/types/room";
import { useQuery } from "@tanstack/react-query";
import { createFileRoute, redirect } from "@tanstack/react-router";
import type { AxiosError } from "axios";
import {
  AlertCircle,
  Calendar,
  CheckCircle,
  Clock,
  Loader2,
  MapPin,
  Users,
} from "lucide-react";
import { useState } from "react";

export const Route = createFileRoute("/_mainLayout/events/$eventId/invite")({
  async beforeLoad({ search }) {
    let token: string | undefined = undefined;
    if (
      search &&
      typeof search === "object" &&
      "token" in search &&
      typeof search["token"] === "string"
    ) {
      token = search["token"] as string;
    }
    const user = await authService.checkAuthStatus();
    if (token && (!user || !user)) {
      throw redirect({
        to: "/login",
        search: { returnTo: window.location.pathname + window.location.search },
      });
    }
  },
  component: RouteComponent,
});

function RouteComponent() {
  const { eventId } = Route.useParams();
  const search = Route.useSearch() as Record<string, unknown>;
  const token =
    typeof search["token"] === "string"
      ? (search["token"] as string)
      : undefined;
  const { validateInvitation, joinEventWithToken } = useEvents();

  // React Query for invitation validation
  const {
    data: event,
    isLoading,
    isError,
    error,
  } = useQuery<EventDetails, AxiosError>({
    queryKey: ["event-invite", eventId, token],
    queryFn: () => validateInvitation(eventId, token || ""),
    retry: 1,
  });

  // Fetch building information if buildingId is available
  const { data: buildings, isLoading: buildingLoading } = useBuildings(
    1,
    100,
    true,
  );

  // Fetch room information if roomId and buildingId are available
  const { data: rooms, isLoading: roomsLoading } = useRooms(
    event?.buildingId,
    !!event?.buildingId,
  );

  // Find the specific building and room
  const eventBuilding = buildings?.find(
    (b: BuildingsResponse) => b.id === event?.buildingId,
  );
  const eventRoom = rooms?.find((r: RoomsResponse) => r.id === event?.roomId);

  const [joinStatus, setJoinStatus] = useState<
    "idle" | "joining" | "success" | "error"
  >("idle");
  const [joinError, setJoinError] = useState<string | null>(null);

  // Get the location display text
  const getLocationDisplay = () => {
    // If there's a building selected
    if (event?.buildingId && eventBuilding) {
      // If there's also a room selected
      if (event.roomId && eventRoom) {
        return `${eventBuilding.name}, ${eventRoom.label}`;
      }
      // Just building, no room
      return eventBuilding.name;
    }
    // Custom location
    return event?.location;
  };

  // Format date for display
  const formatDate = (dateString: string) => {
    const options: Intl.DateTimeFormatOptions = {
      weekday: "long",
      year: "numeric",
      month: "long",
      day: "numeric",
    };
    return new Date(dateString).toLocaleDateString(undefined, options);
  };

  // Format time from date for display
  const formatTime = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleTimeString(undefined, {
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const handleJoin = async () => {
    const user = await authService.checkAuthStatus();
    if (!user) {
      setJoinStatus("error");
      setJoinError("You must be logged in to join the event.");
      return;
    }
    setJoinStatus("joining");
    setJoinError(null);
    try {
      if (token) {
        await joinEventWithToken(eventId, token);
      } else {
        // Для публічного івенту — join без токена
        await joinEventWithToken(eventId, "");
      }
      setJoinStatus("success");
    } catch (err: unknown) {
      const axiosErr = err as AxiosError<{ message?: string }>;
      setJoinStatus("error");
      setJoinError(
        axiosErr?.response?.data?.message ||
          "Failed to join the event. You may already be a participant.",
      );
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-6">
        <div className="mx-auto max-w-2xl">
          <Card className="shadow-lg">
            <CardContent className="flex items-center justify-center p-12">
              <div className="flex items-center gap-3 text-lg">
                <Loader2 className="h-6 w-6 animate-spin text-blue-600" />
                Loading invitation...
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-red-50 to-pink-100 p-6">
        <div className="mx-auto max-w-2xl">
          <PageHeader
            title="Invitation Error"
            backTo="/events"
            backButtonLabel="Back to Events"
          />
          <Card className="mt-8 border-red-200 shadow-lg">
            <CardContent className="p-12 text-center">
              <AlertCircle className="mx-auto mb-4 h-16 w-16 text-red-500" />
              <h2 className="mb-2 text-xl font-semibold text-red-700">
                Invalid Invitation
              </h2>
              <p className="text-red-600">
                {error instanceof Error
                  ? error.message
                  : "Invalid or expired invitation link."}
              </p>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  if (joinStatus === "success") {
    return (
      <div className="min-h-screen bg-gradient-to-br from-green-50 to-emerald-100 p-6">
        <div className="mx-auto max-w-2xl">
          <PageHeader
            title="Joined Event!"
            backTo={`/events/${eventId}`}
            backButtonLabel="Go to Event"
          />
          <Card className="mt-8 border-green-200 shadow-lg">
            <CardContent className="p-12 text-center">
              <CheckCircle className="mx-auto mb-4 h-16 w-16 text-green-500" />
              <h2 className="mb-2 text-xl font-semibold text-green-700">
                Welcome Aboard!
              </h2>
              <p className="text-green-600">
                You have successfully joined the event!
              </p>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screenp-6">
      <div className="mx-auto max-w-2xl">
        <PageHeader
          title="Event Invitation"
          backTo="/events"
          backButtonLabel="Back to Events"
        />

        {/* Main Event Card */}
        <Card className="mt-8 overflow-hidden border-0 shadow-xl">
          {/* Event Cover Image */}
          <div className="relative h-48 bg-gradient-to-r from-blue-600 to-purple-600">
            <img
              src="/movinight.png"
              alt="Event Cover"
              className="h-full w-full object-cover opacity-90"
            />
            <div className="absolute inset-0 bg-gradient-to-t from-black/20 to-transparent" />
            <div className="absolute bottom-4 left-6 text-white">
              <Badge variant="secondary" className="mb-2">
                {token ? "Private Invitation" : "Public Event"}
              </Badge>
              <h1 className="text-2xl font-bold drop-shadow-lg">
                {event?.name}
              </h1>
            </div>
          </div>

          <CardContent className="p-6">
            {/* Event Details Grid */}
            <div className="mb-6 grid gap-4 md:grid-cols-2">
              <div className="flex items-center gap-3 rounded-lg bg-blue-50 p-3">
                <Calendar className="h-5 w-5 text-blue-600" />
                <div>
                  <p className="font-medium text-gray-900">Date</p>
                  <p className="text-sm text-gray-600">
                    {event?.date ? formatDate(event.date) : "-"}
                  </p>
                </div>
              </div>

              <div className="flex items-center gap-3 rounded-lg bg-green-50 p-3">
                <Clock className="h-5 w-5 text-green-600" />
                <div>
                  <p className="font-medium text-gray-900">Time</p>
                  <p className="text-sm text-gray-600">
                    {event?.date ? formatTime(event.date) : "-"}
                  </p>
                </div>
              </div>

              <div className="flex items-center gap-3 rounded-lg bg-purple-50 p-3">
                <MapPin className="h-5 w-5 text-purple-600" />
                <div>
                  <p className="font-medium text-gray-900">Location</p>
                  <p className="text-sm text-gray-600">
                    {buildingLoading || roomsLoading
                      ? "Loading..."
                      : getLocationDisplay()}
                  </p>
                </div>
              </div>

              <div className="flex items-center gap-3 rounded-lg bg-orange-50 p-3">
                <Users className="h-5 w-5 text-orange-600" />
                <div>
                  <p className="font-medium text-gray-900">Participants</p>
                  <p className="text-sm text-gray-600">
                    {event?.participants?.length ?? "-"} attending
                  </p>
                </div>
              </div>
            </div>

            {/* Description */}
            {event?.description && (
              <div className="mb-6 rounded-lg bg-gray-50 p-4">
                <h3 className="mb-2 font-medium text-gray-900">Description</h3>
                <p className="text-gray-700">{event.description}</p>
              </div>
            )}

            {/* Join Button */}
            <div className="flex flex-col items-center gap-3">
              <Button
                onClick={handleJoin}
                disabled={joinStatus === "joining"}
                size="lg"
                className="w-full px-8 py-3 text-lg font-semibold md:w-auto"
              >
                {joinStatus === "joining" ? (
                  <>
                    <Loader2 className="mr-2 h-5 w-5 animate-spin" />
                    Joining...
                  </>
                ) : (
                  "Join Event"
                )}
              </Button>

              {joinStatus === "error" && (
                <div className="flex w-full items-center gap-2 rounded-lg bg-red-50 p-3 text-red-600">
                  <AlertCircle className="h-5 w-5" />
                  <span className="text-sm">{joinError}</span>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
