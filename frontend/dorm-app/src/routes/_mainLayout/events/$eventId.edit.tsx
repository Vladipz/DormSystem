import { PageHeader } from "@/components/PageHeader";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import { useAuth } from "@/lib/hooks/useAuth";
import { useEvents } from "@/lib/hooks/useEvents";
import { EventService } from "@/lib/services/eventService";
import { CreateEventRequest } from "@/lib/types/event";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createFileRoute,
  useNavigate,
  useParams,
} from "@tanstack/react-router";
import { useFormik } from "formik";
import { AlertCircle, Save, X } from "lucide-react";
import { useEffect, useState } from "react";
import * as Yup from "yup";

// Define validation schema using Yup
const EventSchema = Yup.object().shape({
  name: Yup.string()
    .min(3, "Name must be at least 3 characters")
    .max(100, "Name must be less than 100 characters")
    .required("Event name is required"),
  date: Yup.date()
    .min(new Date(), "Date cannot be in the past")
    .required("Date is required"),
  location: Yup.string()
    .min(3, "Location must be at least 3 characters")
    .required("Location is required"),
  numberOfAttendees: Yup.number()
    .nullable()
    .min(1, "Number of attendees must be at least 1")
    .integer("Number of attendees must be a whole number"),
  isPublic: Yup.boolean().required("Please specify if the event is public"),
  description: Yup.string().max(
    2000,
    "Description must be less than 2000 characters"
  ),
});

export const Route = createFileRoute("/_mainLayout/events/$eventId/edit")({
  component: RouteComponent,
});

function RouteComponent() {
  const { eventId } = useParams({ from: "/_mainLayout/events/$eventId/edit" });
  const navigate = useNavigate();
  const { updateEventMutation } = useEvents();
  const { user, isAuthenticated, isLoading } = useAuth();
  const [isOwnerOrAdmin, setIsOwnerOrAdmin] = useState(false);
  const queryClient = useQueryClient();

  // Fetch event details
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

  // Check permissions once event data is loaded
  useEffect(() => {
    if (event && user) {
      const isOwner = event.ownerId === user.id;
      const isAdmin = user.role === "Admin";
      setIsOwnerOrAdmin(isOwner || isAdmin);
    }
  }, [event, user]);

  const formik = useFormik({
    initialValues: {
      name: "",
      date: "",
      location: "",
      numberOfAttendees: null as number | null,
      isPublic: false,
      description: "",
    },
    validationSchema: EventSchema,
    onSubmit: (values) => {
      // Format the date to ISO string for API
      const formattedDate = new Date(values.date).toISOString();

      // Prepare request payload
      const payload: CreateEventRequest = {
        name: values.name,
        date: formattedDate,
        location: values.location,
        numberOfAttendees: values.numberOfAttendees,
        isPublic: values.isPublic,
        description: values.description || "",
      };

      // Trigger the update mutation
      updateEventMutation.mutate(
        {
          eventId,
          eventData: payload,
        },
        {
          onSuccess: () => {
            // Invalidate events query data to refetch
            queryClient.invalidateQueries({ queryKey: ["events"] });
            queryClient.invalidateQueries({ queryKey: ["events", eventId] });
            navigate({ to: `/events/${eventId}` });
          },
        }
      );
    },
  });

  // Populate the form with event data once it's loaded
  useEffect(() => {
    if (event) {
      const dateObj = new Date(event.date);
      const formattedLocalDate = dateObj.toISOString().slice(0, 16); // Format as YYYY-MM-DDThh:mm for datetime-local input

      formik.setValues({
        name: event.name || "",
        date: formattedLocalDate,
        location: event.location || "",
        numberOfAttendees: event.numberOfAttendees,
        isPublic: event.isPublic,
        description: event.description || "",
      });
    }
  }, [event]);

  // Handle unauthorized access
  if (!isLoading && !isAuthenticated) {
    return (
      <div className="mx-auto p-4 sm:p-6 max-w-full sm:max-w-2xl">
        <div className="bg-orange-100 border border-orange-400 text-orange-700 px-4 py-5 sm:py-8 rounded mb-4 flex flex-col items-center text-center">
          <AlertCircle className="h-8 w-8 sm:h-10 sm:w-10 mb-3 sm:mb-4" />
          <h3 className="text-base sm:text-lg font-medium mb-1 sm:mb-2">
            Authentication Required
          </h3>
          <p className="text-center text-sm sm:text-base mb-4 sm:mb-6">
            You need to sign in to edit events.
          </p>
          <Button
            onClick={() =>
              navigate({
                to: "/login",
                search: { returnTo: `/events/${eventId}/edit` },
              })
            }
            className="w-full sm:w-auto"
          >
            Sign In
          </Button>
        </div>
      </div>
    );
  }

  // Handle permission check
  if (!isLoading && !isEventLoading && !isOwnerOrAdmin) {
    return (
      <div className="mx-auto p-4 sm:p-6 max-w-full sm:max-w-2xl">
        <PageHeader
          title="Access Denied"
          backTo={`/events/${eventId}`}
          backButtonLabel="Back"
        />
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-5 sm:py-8 rounded mb-4 flex flex-col items-center text-center">
          <AlertCircle className="h-8 w-8 sm:h-10 sm:w-10 mb-3 sm:mb-4" />
          <h3 className="text-base sm:text-lg font-medium mb-1 sm:mb-2">
            Permission Denied
          </h3>
          <p className="text-center text-sm sm:text-base mb-4 sm:mb-6">
            You don't have permission to edit this event. Only the event owner
            or administrators can make changes.
          </p>
          <Button
            onClick={() => navigate({ to: `/events/${eventId}` })}
            className="w-full sm:w-auto"
          >
            Return to Event
          </Button>
        </div>
      </div>
    );
  }

  // Show loading state
  if (isLoading || isEventLoading) {
    return (
      <div className="mx-auto p-4 sm:p-6 max-w-full sm:max-w-2xl">
        <div className="animate-pulse space-y-4">
          <div className="h-8 bg-gray-200 rounded w-1/3 sm:w-1/4"></div>
          <div className="h-32 sm:h-64 bg-gray-200 rounded"></div>
          <div className="h-20 bg-gray-200 rounded"></div>
        </div>
      </div>
    );
  }

  // Show error state
  if (isError) {
    return (
      <div className="mx-auto p-4 sm:p-6 max-w-full sm:max-w-2xl">
        <PageHeader
          title="Error"
          backTo="/events"
          backButtonLabel="Back to Events"
        />
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
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
    <div className="mx-auto p-4 sm:p-6 max-w-full sm:max-w-2xl">
      <PageHeader
        title="Edit Event"
        backTo={`/events/${eventId}`}
        backButtonLabel="Cancel"
        actions={
          <div className="flex flex-col sm:flex-row gap-2 mt-2 sm:mt-0">
            <Button
              variant="outline"
              onClick={() => navigate({ to: `/events/${eventId}` })}
              size="sm"
              className="w-full sm:w-auto order-2 sm:order-1"
            >
              <X className="mr-2 h-4 w-4" />
              <span className="hidden sm:inline">Cancel</span>
            </Button>
            <Button
              onClick={() => formik.handleSubmit()}
              disabled={updateEventMutation.isPending || !formik.isValid}
              size="sm"
              className="w-full sm:w-auto order-1 sm:order-2"
            >
              <Save className="mr-2 h-4 w-4" />
              {updateEventMutation.isPending ? "Saving..." : "Save Changes"}
            </Button>
          </div>
        }
      />

      {updateEventMutation.isError && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          <p className="text-sm sm:text-base">
            {updateEventMutation.error instanceof Error
              ? updateEventMutation.error.message
              : "An error occurred while updating the event"}
          </p>
        </div>
      )}

      <form
        onSubmit={formik.handleSubmit}
        className="space-y-4 sm:space-y-6 mt-4"
      >
        <div>
          <Label htmlFor="name" className="block text-sm font-medium mb-1">
            Event Name
          </Label>
          <Input
            id="name"
            name="name"
            type="text"
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            value={formik.values.name}
            className={
              formik.errors.name && formik.touched.name ? "border-red-500" : ""
            }
          />
          {formik.errors.name && formik.touched.name && (
            <p className="text-red-500 text-xs mt-1">{formik.errors.name}</p>
          )}
        </div>

        <div>
          <Label htmlFor="date" className="block text-sm font-medium mb-1">
            Date
          </Label>
          <Input
            id="date"
            name="date"
            type="datetime-local"
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            value={formik.values.date}
            className={
              formik.errors.date && formik.touched.date
                ? "border-red-500"
                : ""
            }
          />
          {formik.errors.date && formik.touched.date && (
            <p className="text-red-500 text-xs mt-1">{formik.errors.date}</p>
          )}
        </div>

        <div>
          <Label
            htmlFor="location"
            className="block text-sm font-medium mb-1"
          >
            Location
          </Label>
          <Input
            id="location"
            name="location"
            type="text"
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            value={formik.values.location}
            className={
              formik.errors.location && formik.touched.location
                ? "border-red-500"
                : ""
            }
          />
          {formik.errors.location && formik.touched.location && (
            <p className="text-red-500 text-xs mt-1">
              {formik.errors.location}
            </p>
          )}
        </div>

        <div>
          <Label
            htmlFor="description"
            className="block text-sm font-medium mb-1"
          >
            Description
          </Label>
          <Textarea
            id="description"
            name="description"
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            value={formik.values.description}
            className={
              formik.errors.description && formik.touched.description
                ? "border-red-500"
                : ""
            }
            rows={4}
          />
          {formik.errors.description && formik.touched.description && (
            <p className="text-red-500 text-xs mt-1">
              {formik.errors.description}
            </p>
          )}
        </div>

        <div>
          <Label
            htmlFor="numberOfAttendees"
            className="block text-sm font-medium mb-1"
          >
            Number of Attendees (Optional)
          </Label>
          <Input
            id="numberOfAttendees"
            name="numberOfAttendees"
            type="number"
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            value={formik.values.numberOfAttendees ?? ""}
            className={
              formik.errors.numberOfAttendees &&
              formik.touched.numberOfAttendees
                ? "border-red-500"
                : ""
            }
          />
          {formik.errors.numberOfAttendees &&
            formik.touched.numberOfAttendees && (
              <p className="text-red-500 text-xs mt-1">
                {formik.errors.numberOfAttendees as string}
              </p>
            )}
        </div>

        <div className="flex items-center space-x-2">
          <Switch
            id="isPublic"
            name="isPublic"
            checked={formik.values.isPublic}
            onCheckedChange={(checked) =>
              formik.setFieldValue("isPublic", checked)
            }
          />
          <Label htmlFor="isPublic" className="text-sm">
            Make this event public
            <span className="text-xs text-gray-500 block mt-1">
              Public events can be joined by anyone
            </span>
          </Label>
        </div>
      </form>
    </div>
  );
}
