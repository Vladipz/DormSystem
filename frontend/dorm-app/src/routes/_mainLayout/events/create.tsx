import { EventForm, EventFormValues } from "@/components/EventForm";
import { LoginRequiredMessage } from "@/components/LoginRequiredMessage";
import { PageHeader } from "@/components/PageHeader";
import { useEvents } from "@/lib/hooks/useEvents";
import { authService } from "@/lib/services/authService";
import { CreateEventRequest } from "@/lib/types/event";
import { createFileRoute, useNavigate } from "@tanstack/react-router";

export const Route = createFileRoute("/_mainLayout/events/create")({
  beforeLoad: async () => {
    const user = await authService.checkAuthStatus();
    console.log("User auth status:", user);
    if (!user || !user.isAuthenticated) {
      return { loginRequired: true };
    }
    return { loginRequired: false };
  },
  component: RouteComponent,
});

function RouteComponent() {
  const navigate = useNavigate();
  const { createEventMutation } = useEvents();
  const { loginRequired } = Route.useRouteContext();
  if (loginRequired) {
    console.log(loginRequired)
    return <LoginRequiredMessage returnTo="/events/create" />;
  }

  // Initial form values
  const initialValues: EventFormValues = {
    name: "",
    date: "",
    location: "",
    numberOfAttendees: null,
    isPublic: false,
    description: "",
    buildingId: undefined,
    roomId: undefined,
  };

  // Handle form submission
  const handleSubmit = (eventData: CreateEventRequest) => {
    createEventMutation.mutate(eventData, {
      onSuccess: () => {
        navigate({ to: "/events" });
      },
    });
  };

  return (
    <div className="max-w-2xl mx-auto p-6">
      <PageHeader
        title="Create New Event"
        backTo="/events"
        backButtonLabel="Back to Events"
      />

      <EventForm
        initialValues={initialValues}
        onSubmit={handleSubmit}
        onCancel={() => navigate({ to: "/events" })}
        isSubmitting={createEventMutation.isPending}
        submitButtonText="Create Event"
        error={createEventMutation.error as Error | null}
      />
    </div>
  );
}
