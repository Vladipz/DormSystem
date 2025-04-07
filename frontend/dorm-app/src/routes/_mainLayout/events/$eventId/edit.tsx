import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { useAuth } from "@/lib/hooks/useAuth";
import { useEvents } from "@/lib/hooks/useEvents";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useFormik } from "formik";
import { useEffect } from "react";
import * as Yup from "yup";

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

export const Route = createFileRoute("/_mainLayout/events/$eventId/edit")({
  component: RouteComponent,
});

function RouteComponent() {
  const { event, editEventMutation } = useEvents();
  const navigate = useNavigate();
  const { isAuthenticated, requireAuth, isLoading } = useAuth();

  useEffect(() => {
    if (!isLoading) {
      requireAuth(`/events/${event?.id}/edit`);
    }
  }, [isLoading, requireAuth, event?.id]);

  const formik = useFormik({
    initialValues: {
      name: event?.name ?? "",
      date: event?.date ? new Date(event.date).toISOString().slice(0, 16) : "",
      location: event?.location ?? "",
      numberOfAttendees: event?.numberOfAttendees ?? null,
      isPublic: event?.isPublic ?? false,
    },
    validationSchema: EventSchema,
    enableReinitialize: true,
    onSubmit: (values) => {
      if (!event) return;

      const formattedDate = new Date(values.date).toISOString();

      const payload = {
        name: values.name,
        date: formattedDate,
        location: values.location,
        numberOfAttendees: values.numberOfAttendees,
        isPublic: values.isPublic,
      };

      editEventMutation.mutate(
        { id: event.id, ...payload },
        {
          onSuccess: () => {
            navigate({ to: "/events" });
          },
          onError: (error) => {
            if (error.message?.includes("Authentication required")) {
              requireAuth(`/events/${event.id}/edit`);
            }
          },
        }
      );
    },
  });

  if (isLoading || !isAuthenticated || !event) {
    return null;
  }

  return (
    <div className="max-w-2xl mx-auto p-6">
      <h1 className="text-2xl font-bold mb-6">Edit Event</h1>

      {editEventMutation.isError && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          <p>
            {editEventMutation.error instanceof Error
              ? editEventMutation.error.message
              : "An error occurred while updating the event"}
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
            disabled={editEventMutation.isPending}
          >
            Cancel
          </Button>
          <Button
            type="submit"
            disabled={editEventMutation.isPending || !formik.isValid}
          >
            {editEventMutation.isPending ? "Saving..." : "Save Changes"}
          </Button>
        </div>
      </form>
    </div>
  );
}
