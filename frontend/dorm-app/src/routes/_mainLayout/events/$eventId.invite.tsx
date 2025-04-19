import { PageHeader } from "@/components/PageHeader";
import { Button } from "@/components/ui/button";
import { useEvents } from "@/lib/hooks/useEvents";
import { authService } from "@/lib/services/authService";
import type { EventDetails } from "@/lib/types/event";
import { useQuery } from "@tanstack/react-query";
import { createFileRoute, redirect } from "@tanstack/react-router";
import type { AxiosError } from "axios";
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
    const user = authService.checkAuthStatus();
    if (token && (!user || !user.isAuthenticated)) {
      throw redirect({ to: "/login" });
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

  const [joinStatus, setJoinStatus] = useState<
    "idle" | "joining" | "success" | "error"
  >("idle");
  const [joinError, setJoinError] = useState<string | null>(null);

  const handleJoin = async () => {
    const user = authService.checkAuthStatus();
    if (!user || !user.isAuthenticated) {
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
          "Failed to join the event. You may already be a participant."
      );
    }
  };

  if (isLoading) {
    return <div className="p-6">Loading invitation...</div>;
  }

  if (isError) {
    return (
      <div className="p-6">
        <PageHeader
          title="Invitation Error"
          backTo="/events"
          backButtonLabel="Back to Events"
        />
        <div className="mt-8 text-center text-red-500">
          {error instanceof Error
            ? error.message
            : "Invalid or expired invitation link."}
        </div>
      </div>
    );
  }

  if (joinStatus === "success") {
    return (
      <div className="p-6">
        <PageHeader
          title="Joined Event!"
          backTo={`/events/${eventId}`}
          backButtonLabel="Go to Event"
        />
        <div className="mt-8 text-center text-green-600">
          You have successfully joined the event!
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-lg mx-auto">
      <PageHeader
        title="Event Invitation"
        backTo="/events"
        backButtonLabel="Back to Events"
      />
      <div className="mt-8 space-y-4">
        <div className="text-xl font-bold">{event?.name}</div>
        <div>
          Date: {event?.date ? new Date(event.date).toLocaleString() : "-"}
        </div>
        <div>Location: {event?.location}</div>
        <div>Description: {event?.description || "No description"}</div>
        <div>
          Current participants: {event?.currentParticipantsCount ?? "-"}
        </div>
        <div className="mt-6">
          <Button onClick={handleJoin} disabled={joinStatus === "joining"}>
            {joinStatus === "joining" ? "Joining..." : "Join Event"}
          </Button>
        </div>
        {joinStatus === "error" && (
          <div className="text-red-500 mt-2">{joinError}</div>
        )}
      </div>
    </div>
  );
}
