import { EventForm, EventFormValues } from "@/components/EventForm";
import { EventFormSkeleton } from "@/components/EventFormSkeleton";
import { LoginRequiredMessage } from "@/components/LoginRequiredMessage";
import { PageHeader } from "@/components/PageHeader";
import { useEvents } from "@/lib/hooks/useEvents";
import { authService } from "@/lib/services/authService";
import { EventService } from "@/lib/services/eventService";
import { CreateEventRequest } from "@/lib/types/event";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createFileRoute,
  redirect,
  useNavigate,
  useParams,
} from "@tanstack/react-router";

export const Route = createFileRoute("/_mainLayout/events/$eventId/edit")({
  async beforeLoad({ params }) {
    const { eventId } = params;
    const user = authService.checkAuthStatus();
    if (!user || !user.isAuthenticated) {
      // Не редіректимо, просто даємо компоненту відрендерити LoginRequiredMessage
      return;
    }
    const event = await EventService.getEventById(eventId);
    const isOwner = event.ownerId === user.id;
    const isAdmin = user.role === "Admin";
    if (!isOwner && !isAdmin) {
      throw redirect({
        to: "/events/$eventId",
        params: { eventId },
        search: { error: "permission" },
      });
    }
  },
  component: RouteComponent,
});

function RouteComponent() {
  const { eventId } = useParams({ from: "/_mainLayout/events/$eventId/edit" });
  const navigate = useNavigate();
  const { updateEventMutation } = useEvents();
  const queryClient = useQueryClient();
  // Always call hooks first
  const user = authService.checkAuthStatus();
  const {
    data: event,
    isLoading: isEventLoading,
    isError,
    error,
  } = useQuery({
    queryKey: ["events", eventId],
    queryFn: () => EventService.getEventById(eventId),
    staleTime: 1000 * 60 * 5, // 5 minutes
  });

  // Render login message if not authenticated
  if (!user || !user.isAuthenticated) {
    return <LoginRequiredMessage returnTo={`/events/${eventId}/edit`} />;
  }

  // Format form initial values from event data
  const getInitialValues = (): EventFormValues => {
    if (event) {
      const dateObj = new Date(event.date);
      const formattedLocalDate = dateObj.toISOString().slice(0, 16); // Format as YYYY-MM-DDThh:mm for datetime-local input

      return {
        name: event.name || "",
        date: formattedLocalDate,
        location: event.location || "",
        numberOfAttendees: event.numberOfAttendees,
        isPublic: event.isPublic,
        description: event.description || "",
      };
    }

    return {
      name: "",
      date: "",
      location: "",
      numberOfAttendees: null,
      isPublic: false,
      description: "",
    };
  };

  // Handle form submission
  const handleSubmit = (eventData: CreateEventRequest) => {
    updateEventMutation.mutate(
      { eventId, eventData },
      {
        onSuccess: () => {
          // Invalidate events query data to refetch
          queryClient.invalidateQueries({ queryKey: ["events"] });
          queryClient.invalidateQueries({ queryKey: ["events", eventId] });
          navigate({ to: `/events/${eventId}` });
        },
      },
    );
  };

  // Show loading state
  if (isEventLoading) {
    return (
      <div className="mx-auto max-w-full p-4 sm:max-w-2xl sm:p-6">
        <PageHeader
          title="Edit Event"
          backTo={`/events/${eventId}`}
          backButtonLabel="Cancel"
        />
        <EventFormSkeleton />
      </div>
    );
  }

  // Show error state
  if (isError) {
    return (
      <div className="mx-auto max-w-full p-4 sm:max-w-2xl sm:p-6">
        <PageHeader
          title="Error"
          backTo="/events"
          backButtonLabel="Back to Events"
        />
        <div className="mb-4 rounded border border-red-400 bg-red-100 px-4 py-3 text-red-700">
          <p className="text-sm sm:text-base">
            {error instanceof Error
              ? error.message
              : "An error occurred while loading the event"}
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-full p-4 sm:max-w-2xl sm:p-6">
      <PageHeader
        title="Edit Event"
        backTo={`/events/${eventId}`}
        backButtonLabel="Cancel"
      />

      <EventForm
        initialValues={getInitialValues()}
        onSubmit={handleSubmit}
        onCancel={() => navigate({ to: `/events/${eventId}` })}
        isSubmitting={updateEventMutation.isPending}
        submitButtonText="Save Changes"
        error={updateEventMutation.error as Error | null}
      />
    </div>
  );
}
