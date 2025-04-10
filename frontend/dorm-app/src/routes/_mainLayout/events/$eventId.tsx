import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { EventService } from "@/lib/services/eventService";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { ArrowLeft, Calendar, Clock, MapPin, Users } from "lucide-react";
import { useState } from "react";

export const Route = createFileRoute("/_mainLayout/events/$eventId")({
  component: EventDetailsPage,
});

function EventDetailsPage() {
  const { eventId } = Route.useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [comment, setComment] = useState("");

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

  // Mutation for joining/leaving an event
  const attendanceMutation = useMutation({
    mutationFn: async (isJoining: boolean) => {
      if (isJoining) {
        // TODO: Get actual user ID from auth context
        const userId = "currentUserId";
        await EventService.joinEvent(eventId, userId);
      } else {
        // Leave event logic - to be implemented
        // await EventService.leaveEvent(eventId);
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

  // Mutation for adding comments
  const commentMutation = useMutation({
    mutationFn: async (text: string) => {
      // In a real app, we would send this to the API
      console.log("Adding comment:", text);
      // Placeholder for API call: await EventService.addComment(eventId, text);
      return true;
    },
    onSuccess: () => {
      setComment("");
      // Invalidate and refetch event data to show the new comment
      queryClient.invalidateQueries({ queryKey: ["events", eventId] });
    },
  });

  const goBack = () => {
    navigate({ to: "/events" });
  };

  const toggleAttendance = () => {
    if (!event) return;

    // Check if user is currently attending and toggle the state
    const isCurrentlyAttending = event.isUserAttending || false;
    attendanceMutation.mutate(!isCurrentlyAttending);
  };

  const handleAddComment = () => {
    if (comment.trim() === "") return;
    commentMutation.mutate(comment);
  };

  if (isLoading) {
    return (
      <div className="p-6">
        <div className="animate-pulse space-y-4">
          <div className="h-8 bg-gray-200 rounded w-1/4"></div>
          <div className="h-64 bg-gray-200 rounded"></div>
          <div className="h-32 bg-gray-200 rounded"></div>
        </div>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="p-6">
        <Button variant="ghost" onClick={goBack}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Events
        </Button>
        <div className="mt-8 text-center">
          <h2 className="text-2xl font-bold">Error loading event</h2>
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
        <Button variant="ghost" onClick={goBack}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Events
        </Button>
        <div className="mt-8 text-center">
          <h2 className="text-2xl font-bold">Event not found</h2>
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

  const isAttending = event.isUserAttending || false;

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" onClick={goBack}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Events
        </Button>
        <h1 className="text-2xl font-bold">{event.name}</h1>
      </div>

      {/* Event Header Card */}
      <Card>
        <div className="relative">
          <div className="w-full h-64 bg-gray-200 rounded-t-lg"></div>
          <div className="absolute bottom-4 right-4">
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
        <CardContent className="p-6">
          <div className="grid gap-6 md:grid-cols-2">
            <div>
              <h2 className="text-xl font-semibold mb-2">About this Event</h2>
              <p className="text-muted-foreground mb-4 break-words whitespace-normal">
                {event.description}
              </p>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="flex items-center gap-2">
                  <Calendar className="h-5 w-5 text-primary shrink-0" />
                  <div className="min-w-0">
                    <p className="font-medium">Date</p>
                    <p className="text-sm text-muted-foreground truncate">
                      {formatDate(event.date)}
                    </p>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <Clock className="h-5 w-5 text-primary shrink-0" />
                  <div className="min-w-0">
                    <p className="font-medium">Time</p>
                    <p className="text-sm text-muted-foreground truncate">
                      {formatTime(event.date) || "TBD"}
                    </p>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <MapPin className="h-5 w-5 text-primary shrink-0" />
                  <div className="min-w-0">
                    <p className="font-medium">Location</p>
                    <p className="text-sm text-muted-foreground truncate">
                      {event.location}
                    </p>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <Users className="h-5 w-5 text-primary shrink-0" />
                  <div className="min-w-0">
                    <p className="font-medium">Participants</p>
                    <p className="text-sm text-muted-foreground truncate">
                      {event.lastParticipants?.length || 0} attending
                      {event.numberOfAttendees
                        ? ` (max ${event.numberOfAttendees})`
                        : ""}
                    </p>
                  </div>
                </div>
              </div>
            </div>

            <div>
              <h3 className="font-medium mb-2">Organized by</h3>
              <div className="flex items-center gap-2 mb-4">
                <div className="h-8 w-8 rounded-full bg-gray-300"></div>
                <span>{event.ownerName || "Event Organizer"}</span>
              </div>

              <h3 className="font-medium mb-2">Participants</h3>
              <div className="flex flex-wrap gap-1">
                {event.lastParticipants &&
                  event.lastParticipants
                    .slice(0, 5)
                    .map((participant, index) => (
                      <div
                        key={index}
                        className="h-8 w-8 rounded-full bg-gray-300 border-2 border-background"
                      ></div>
                    ))}
                {event.lastParticipants &&
                  event.lastParticipants.length > 5 && (
                    <div className="flex items-center justify-center h-8 w-8 rounded-full bg-muted text-xs font-medium">
                      +{event.lastParticipants.length - 5}
                    </div>
                  )}
                {(!event.lastParticipants ||
                  event.lastParticipants.length === 0) && (
                  <p className="text-sm text-muted-foreground">
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
          {/* Discussion Section */}
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-xl">Discussion</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="flex gap-4">
                  <div className="h-10 w-10 rounded-full bg-gray-300"></div>
                  <div className="flex-1 space-y-2">
                    <textarea
                      className="w-full p-2 border rounded-md"
                      placeholder="Add a comment..."
                      value={comment}
                      onChange={(e) => setComment(e.target.value)}
                      rows={3}
                      disabled={commentMutation.isPending}
                    />
                    <div className="flex justify-end">
                      <Button
                        onClick={handleAddComment}
                        disabled={
                          commentMutation.isPending || comment.trim() === ""
                        }
                      >
                        {commentMutation.isPending
                          ? "Posting..."
                          : "Post Comment"}
                      </Button>
                    </div>
                  </div>
                </div>

                <div className="h-px bg-gray-200 my-4"></div>

                <div className="space-y-4">
                  <p className="text-center text-sm text-muted-foreground">
                    No comments yet. Be the first to start a conversation!
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
