import { PageHeader } from "@/components/PageHeader";
import { EventDiscussionSection } from "@/components/events/comments/EventDiscussionSection";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { useAuth } from "@/lib/hooks/useAuth";
import { useBuildings } from "@/lib/hooks/useBuildings";
import { useEvents } from "@/lib/hooks/useEvents";
import { useRooms } from "@/lib/hooks/useRooms";
import { useUser } from "@/lib/hooks/useUser";
import { authService } from "@/lib/services/authService";
import { EventService } from "@/lib/services/eventService";
import { BuildingsResponse } from "@/lib/types/building";
import { RoomsResponse } from "@/lib/types/room";
import { getPlaceholderAvatar, resolveApiUrl } from "@/lib/utils";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import {
  Calendar,
  Clock,
  Copy,
  Edit,
  MapPin,
  Sparkles,
  Users,
} from "lucide-react";
import { useEffect, useState } from "react";

export const Route = createFileRoute("/_mainLayout/events/$eventId/")({
  async beforeLoad() {
    const authStatus = await authService.checkAuthStatus();
    return { authStatus };
  },
  component: EventDetailsPage,
});

// Component to display user avatar with real data
function UserAvatar({
  userId,
  size = "h-8 w-8",
  showTooltip = true,
  fallbackIndex = 0,
}: {
  userId: string;
  size?: string;
  showTooltip?: boolean;
  fallbackIndex?: number;
}) {
  const { data: userDetails, isLoading } = useUser(userId);

  if (isLoading) {
    return <Skeleton className={`${size} rounded-full`} />;
  }

  const displayName = userDetails
    ? `${userDetails.firstName} ${userDetails.lastName}`
    : "Unknown User";

  const initials = userDetails
    ? `${userDetails.firstName[0] || ""}${userDetails.lastName[0] || ""}`.toUpperCase()
    : getPlaceholderAvatar(fallbackIndex, userId);

  const avatar = (
    <Avatar className={size}>
      <AvatarImage
        src={resolveApiUrl(userDetails?.avatarUrl)}
        alt={displayName}
      />
      <AvatarFallback className="text-xs">{initials}</AvatarFallback>
    </Avatar>
  );

  if (!showTooltip) {
    return avatar;
  }

  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <div className="cursor-default">{avatar}</div>
      </TooltipTrigger>
      <TooltipContent className="bg-popover text-popover-foreground border shadow-md">
        <p>{displayName}</p>
        {userDetails?.email && (
          <p className="text-muted-foreground text-xs">{userDetails.email}</p>
        )}
      </TooltipContent>
    </Tooltip>
  );
}

// Component to display organizer information
function OrganizerInfo({ organizerId }: { organizerId: string }) {
  const { data: organizer, isLoading } = useUser(organizerId);

  if (isLoading) {
    return (
      <div className="flex items-center gap-2">
        <Skeleton className="h-8 w-8 rounded-full" />
        <Skeleton className="h-4 w-24" />
      </div>
    );
  }

  if (!organizer) {
    return (
      <div className="flex items-center gap-2">
        <div className="h-8 w-8 rounded-full bg-gray-300"></div>
        <span className="text-muted-foreground">Unknown Organizer</span>
      </div>
    );
  }

  return (
    <div className="flex items-center gap-2">
      <UserAvatar userId={organizerId} size="h-8 w-8" showTooltip={true} />
      <div>
        <div className="font-medium">
          {organizer.firstName} {organizer.lastName}
        </div>
        <div className="text-muted-foreground text-sm">{organizer.email}</div>
      </div>
    </div>
  );
}

function EventDetailsPage() {
  const { eventId } = Route.useParams();
  const queryClient = useQueryClient();
  const { user, isAuthenticated } = useAuth();
  const [canEdit, setCanEdit] = useState(false);
  const { authStatus } = Route.useRouteContext();
  const [copied, setCopied] = useState(false);
  const [inviteDialogOpen, setInviteDialogOpen] = useState(false);
  const { getEventInviteLink, joinEvent, leaveEvent } = useEvents();

  // Fetch event details using React Query
  const {
    data: event,
    isLoading,
    isError,
    error,
  } = useQuery({
    queryKey: ["events", eventId],
    queryFn: () => EventService.getEventById(eventId),
    staleTime: 1000 * 60 * 5, // 5 minutes
  });

  const canInvite = !!(
    event &&
    (event.isPublic ||
      (authStatus && event.ownerId === authStatus.id) ||
      (authStatus && authStatus.role === "Admin"))
  );

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

  // Check if the user can edit this event (is owner or admin)
  useEffect(() => {
    if (event && user && isAuthenticated) {
      const isOwner = event.ownerId === user.id;
      const isAdmin = user.role === "Admin";
      setCanEdit(isOwner || isAdmin);
    } else {
      setCanEdit(false);
    }
  }, [event, user, isAuthenticated]);

  // Mutation for joining/leaving an event
  const attendanceMutation = useMutation({
    mutationFn: async (isJoining: boolean) => {
      if (!user || !user.id) {
        throw new Error("You must be logged in to attend events");
      }

      if (isJoining) {
        await joinEvent(eventId, user.id);
      } else {
        await leaveEvent(eventId, user.id);
      }
    },
    onSuccess: () => {
      // Invalidate and refetch events data
      queryClient.invalidateQueries({ queryKey: ["events"] });
      queryClient.invalidateQueries({ queryKey: ["events", eventId] });
    },
    onError: (error) => {
      console.error("Failed to update attendance:", error);
    },
  });

  // Use Query for invite link
  const {
    data: inviteLink,
    isLoading: inviteLoading,
    isError: inviteError,
    error: inviteLinkError,
  } = useQuery({
    queryKey: ["eventInviteLink", eventId],
    queryFn: () => getEventInviteLink(eventId),
    enabled: inviteDialogOpen,
    retry: 1,
  });

  const navigate = useNavigate();

  const toggleAttendance = () => {
    if (!event) return;

    // Check if the user is logged in
    if (!user || !isAuthenticated) {
      // Redirect to login page with the current URL as the return destination
      const returnTo = `/events/${eventId}`;
      navigate({ to: "/login", search: { returnTo } });
      return;
    }

    attendanceMutation.mutate(!isAttending);
  };

  // Визначаю isAttending через user.id у списку учасників
  const isAttending = !!(
    event &&
    user &&
    event.participants &&
    event.participants.some((p) => p.userId === user.id)
  );

  const canComment = !!(
    user &&
    isAuthenticated &&
    event &&
    (isAttending || event.ownerId === user.id || user.role === "Admin")
  );

  const commentDisabledReason = !isAuthenticated
    ? "Sign in to add a comment..."
    : "Attend this event to add a comment...";

  if (isLoading) {
    return (
      <div className="p-6">
        <div className="animate-pulse space-y-4">
          <div className="h-8 w-1/4 rounded bg-gray-200"></div>
          <div className="h-64 rounded bg-gray-200"></div>
          <div className="h-32 rounded bg-gray-200"></div>
        </div>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="p-6">
        <PageHeader
          title="Error loading event"
          backTo="/events"
          backButtonLabel="Back to Events"
        />
        <div className="mt-8 text-center">
          <p className="text-muted-foreground mt-2">
            {error instanceof Error
              ? error.message
              : "Failed to load event details"}
          </p>
        </div>
      </div>
    );
  }

  if (!event) {
    return (
      <div className="p-6">
        <PageHeader
          title="Event not found"
          backTo="/events"
          backButtonLabel="Back to Events"
        />
        <div className="mt-8 text-center">
          <p className="text-muted-foreground mt-2">
            The event you're looking for doesn't exist or has been removed.
          </p>
        </div>
      </div>
    );
  }

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

  // Get the location display text
  const getLocationDisplay = () => {
    // If there's a building selected
    if (event.buildingId && eventBuilding) {
      // If there's also a room selected
      if (event.roomId && eventRoom) {
        return `${eventBuilding.name}, ${eventRoom.label}`;
      }
      // Just building, no room
      return eventBuilding.name;
    }
    // Custom location
    return event.location;
  };

  const motivationUnavailable = event.motivationalPhrase.trim() === "";

  return (
    <div className="space-y-6 p-6">
      <PageHeader
        title={event.name}
        backTo="/events"
        backButtonLabel="Back to Events"
        actions={
          <div className="flex gap-2">
            {canInvite && (
              <Dialog
                open={inviteDialogOpen}
                onOpenChange={setInviteDialogOpen}
              >
                <DialogTrigger asChild>
                  <Button variant="outline">Invite Link</Button>
                </DialogTrigger>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>Invitation Link</DialogTitle>
                  </DialogHeader>
                  <div className="flex items-center gap-2">
                    <Input
                      readOnly
                      value={
                        inviteLoading
                          ? "Loading..."
                          : inviteError
                            ? "Failed to load link"
                            : inviteLink
                              ? window.location.origin + inviteLink
                              : ""
                      }
                      className="flex-1"
                    />
                    <Button
                      variant="secondary"
                      onClick={() => {
                        if (inviteLink) {
                          const absLink = window.location.origin + inviteLink;
                          navigator.clipboard.writeText(absLink);
                          setCopied(true);
                          setTimeout(() => setCopied(false), 1200);
                        }
                      }}
                      disabled={inviteLoading || !inviteLink}
                    >
                      <Copy className="mr-1 h-4 w-4" />{" "}
                      {copied ? "Copied!" : "Copy"}
                    </Button>
                  </div>
                  {inviteError && (
                    <div className="mt-2 text-sm text-red-500">
                      {inviteLinkError instanceof Error
                        ? inviteLinkError.message
                        : "Failed to load invitation link."}
                    </div>
                  )}
                  <DialogFooter>
                    <DialogClose asChild>
                      <Button variant="outline">Close</Button>
                    </DialogClose>
                  </DialogFooter>
                </DialogContent>
              </Dialog>
            )}
            {canEdit && (
              <Button asChild>
                <Link to="/events/$eventId/edit" params={{ eventId }}>
                  <Edit className="mr-2 h-4 w-4" />
                  Edit Event
                </Link>
              </Button>
            )}
          </div>
        }
      />

      {/* Event Header Card */}
      <Card className="overflow-hidden py-0">
        <div className="relative">
          <img
            src="/movinight.png"
            alt="Event Cover"
            className="h-64 w-full object-cover"
          />
          <div className="absolute right-4 bottom-4">
            <Button
              variant={isAttending ? "outline" : "default"}
              className={isAttending ? "bg-background" : ""}
              onClick={toggleAttendance}
              disabled={attendanceMutation.isPending}
            >
              {attendanceMutation.isPending
                ? "Processing..."
                : isAttending
                  ? "Cancel Attendance"
                  : "I'll Attend"}
            </Button>
          </div>
        </div>
        <div className="mx-6 mt-5 mb-1 rounded-xl border border-amber-300/40 bg-gradient-to-r from-amber-100/80 via-orange-100/70 to-amber-100/80 px-4 py-4 md:px-5 dark:border-amber-400/25 dark:from-amber-950/45 dark:via-orange-950/35 dark:to-amber-950/45">
          <div className="mb-2 flex items-center gap-2 text-orange-700 dark:text-amber-300">
            <Sparkles className="h-4 w-4" />
            <p className="text-xs font-semibold tracking-wide uppercase">
              Motivation
            </p>
          </div>
          {motivationUnavailable ? (
            <p className="rounded-md border border-amber-300/60 bg-amber-100/70 px-3 py-2 text-sm text-amber-900 dark:border-amber-300/35 dark:bg-amber-950/45 dark:text-amber-200">
              Motivation service is not available right now.
            </p>
          ) : (
            <p className="text-sm leading-7 text-orange-900 md:text-base dark:text-amber-100">
              {event.motivationalPhrase}
            </p>
          )}
        </div>
        <CardContent className="p-6">
          <div className="grid gap-6 md:grid-cols-2">
            <div>
              <h2 className="mb-2 text-xl font-semibold">About this Event</h2>
              <p className="text-muted-foreground mb-4 break-words whitespace-normal">
                {event.description}
              </p>

              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <div className="flex items-center gap-2">
                  <Calendar className="text-primary h-5 w-5 shrink-0" />
                  <div className="min-w-0">
                    <p className="font-medium">Date</p>
                    <p className="text-muted-foreground truncate text-sm">
                      {formatDate(event.date)}
                    </p>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <Clock className="text-primary h-5 w-5 shrink-0" />
                  <div className="min-w-0">
                    <p className="font-medium">Time</p>
                    <p className="text-muted-foreground truncate text-sm">
                      {formatTime(event.date) || "TBD"}
                    </p>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <MapPin className="text-primary h-5 w-5 shrink-0" />
                  <div className="min-w-0">
                    <p className="font-medium">Location</p>
                    <p className="text-muted-foreground truncate text-sm">
                      {buildingLoading || roomsLoading
                        ? "Loading location..."
                        : getLocationDisplay()}
                    </p>
                    {event.buildingId &&
                      eventBuilding &&
                      event.roomId &&
                      eventRoom && (
                        <p className="text-muted-foreground text-xs">
                          Room capacity: {eventRoom.capacity} people
                        </p>
                      )}
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <Users className="text-primary h-5 w-5 shrink-0" />
                  <div className="min-w-0">
                    <p className="font-medium">Participants</p>
                    <p className="text-muted-foreground truncate text-sm">
                      {event.participants?.length || 0} attending
                      {event.numberOfAttendees
                        ? ` (max ${event.numberOfAttendees})`
                        : ""}
                    </p>
                  </div>
                </div>
              </div>
            </div>

            <div>
              <h3 className="mb-2 font-medium">Organized by</h3>
              <div className="mb-4">
                <OrganizerInfo organizerId={event.ownerId} />
              </div>

              <h3 className="mb-2 font-medium">Participants</h3>
              <div className="flex flex-wrap gap-1">
                {event.participants &&
                  event.participants
                    .slice(0, 5)
                    .map((participant, index) => (
                      <UserAvatar
                        key={participant.userId}
                        userId={participant.userId}
                        size="h-8 w-8"
                        showTooltip={true}
                        fallbackIndex={index}
                      />
                    ))}
                {event.participants && event.participants.length > 5 && (
                  <div className="bg-muted flex h-8 w-8 items-center justify-center rounded-full text-xs font-medium">
                    +{event.participants.length - 5}
                  </div>
                )}
                {(!event.participants || event.participants.length === 0) && (
                  <p className="text-muted-foreground text-sm">
                    No participants yet
                  </p>
                )}
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Content Sections */}
      <div className="grid gap-6">
        <div className="space-y-6">
          <EventDiscussionSection
            eventId={eventId}
            eventOwnerId={event.ownerId}
            canComment={canComment}
            disabledReason={commentDisabledReason}
          />
        </div>
      </div>
    </div>
  );
}
