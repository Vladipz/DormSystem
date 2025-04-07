import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label"; // Add this import
import { Switch } from "@/components/ui/switch"; // Add this import
import { useAuth } from "@/lib/hooks/useAuth";
import { useEvents } from "@/lib/hooks/useEvents";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useFormik } from "formik";
import { useEffect } from "react";
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
});

export const Route = createFileRoute("/_mainLayout/events/create")({
  component: RouteComponent,
});

function RouteComponent() {
  const navigate = useNavigate();
  const { createEventMutation } = useEvents();
  const { isAuthenticated, requireAuth, isLoading } = useAuth();

  // Check if user is authenticated and redirect if not
  useEffect(() => {
    if (!isLoading) {
      requireAuth("/events/create");
    }
  }, [isLoading, requireAuth]);

  const formik = useFormik({
    initialValues: {
      name: "",
      date: "",
      location: "",
      numberOfAttendees: null as number | null,
      isPublic: false,
    },
    validationSchema: EventSchema,
    onSubmit: (values) => {
      // Format the date to ISO string for API
      const formattedDate = new Date(values.date).toISOString();

      // Prepare request payload
      const payload = {
        name: values.name,
        date: formattedDate,
        location: values.location,
        numberOfAttendees: values.numberOfAttendees,
        isPublic: values.isPublic,
      };

      // Trigger the mutation using our centralized hook
      createEventMutation.mutate(payload, {
        onSuccess: () => {
          navigate({ to: "/events" });
        },
        onError: (error) => {
          // If it's an authentication error, redirect to login
          if (error.message?.includes("Authentication required")) {
            requireAuth("/events/create");
          }
        },
      });
    },
  });

  // Don't render the form if not authenticated or still loading
  if (isLoading || !isAuthenticated) {
    return null;
  }

  return (
    <div className="max-w-2xl mx-auto p-6">
      <h1 className="text-2xl font-bold mb-6">Create New Event</h1>

      {createEventMutation.isError && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          <p>
            {createEventMutation.error instanceof Error
              ? createEventMutation.error.message
              : "An error occurred while creating the event"}
          </p>
        </div>
      )}

      <form onSubmit={formik.handleSubmit} className="space-y-6">
        <div>
          <label htmlFor="name" className="block text-sm font-medium mb-1">
            Event Name
          </label>
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
          <label htmlFor="date" className="block text-sm font-medium mb-1">
            Date
          </label>
          <Input
            id="date"
            name="date"
            type="datetime-local"
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            value={formik.values.date}
            className={
              formik.errors.date && formik.touched.date ? "border-red-500" : ""
            }
          />
          {formik.errors.date && formik.touched.date && (
            <p className="text-red-500 text-xs mt-1">{formik.errors.date}</p>
          )}
        </div>

        <div>
          <label htmlFor="location" className="block text-sm font-medium mb-1">
            Location
          </label>
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
          <label
            htmlFor="numberOfAttendees"
            className="block text-sm font-medium mb-1"
          >
            Number of Attendees (Optional)
          </label>
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
            onCheckedChange={(checked) => formik.setFieldValue("isPublic", checked)}
          />
          <Label htmlFor="isPublic">
            Make this event public 
            <span className="text-sm text-gray-500 block">
              Public events can be joined by anyone without an invitation
            </span>
          </Label>
        </div>

        <div className="flex justify-end space-x-4 pt-2">
          <Button
            type="button"
            variant="outline"
            onClick={() => navigate({ to: "/events" })}
            disabled={createEventMutation.isPending}
          >
            Cancel
          </Button>
          <Button
            type="submit"
            disabled={createEventMutation.isPending || !formik.isValid}
          >
            {createEventMutation.isPending ? "Creating..." : "Create Event"}
          </Button>
        </div>
      </form>
    </div>
  );
}
