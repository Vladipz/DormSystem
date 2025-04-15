import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import { CreateEventRequest } from "@/lib/types/event";
import { useFormik } from "formik";
import { Save, X } from "lucide-react";
import * as Yup from "yup";

// Define validation schema using Yup
export const EventSchema = Yup.object().shape({
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

export interface EventFormValues {
  name: string;
  date: string;
  location: string;
  numberOfAttendees: number | null;
  isPublic: boolean;
  description: string;
}

interface EventFormProps {
  initialValues: EventFormValues;
  onSubmit: (values: CreateEventRequest) => void;
  onCancel: () => void;
  isSubmitting: boolean;
  submitButtonText: string;
  error?: Error | null;
}

export function EventForm({
  initialValues,
  onSubmit,
  onCancel,
  isSubmitting,
  submitButtonText,
  error,
}: EventFormProps) {
  const formik = useFormik({
    initialValues,
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

      onSubmit(payload);
    },
  });

  return (
    <>
      {error && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          <p className="text-sm sm:text-base">
            {error instanceof Error ? error.message : "An unexpected error occurred"}
          </p>
        </div>
      )}

      <form onSubmit={formik.handleSubmit} className="space-y-4 sm:space-y-6">
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
              formik.errors.date && formik.touched.date ? "border-red-500" : ""
            }
          />
          {formik.errors.date && formik.touched.date && (
            <p className="text-red-500 text-xs mt-1">{formik.errors.date}</p>
          )}
        </div>

        <div>
          <Label htmlFor="location" className="block text-sm font-medium mb-1">
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
            <p className="text-red-500 text-xs mt-1">{formik.errors.location}</p>
          )}
        </div>

        <div>
          <Label htmlFor="description" className="block text-sm font-medium mb-1">
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
          <Label htmlFor="numberOfAttendees" className="block text-sm font-medium mb-1">
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
              formik.errors.numberOfAttendees && formik.touched.numberOfAttendees
                ? "border-red-500"
                : ""
            }
          />
          {formik.errors.numberOfAttendees && formik.touched.numberOfAttendees && (
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

        <div className="flex justify-end gap-3 mt-6 pt-2">
          <Button
            type="button"
            variant="outline"
            onClick={onCancel}
            disabled={isSubmitting}
            className="flex items-center"
          >
            <X className="mr-2 h-4 w-4" />
            Cancel
          </Button>
          <Button
            type="submit"
            disabled={isSubmitting || !formik.isValid}
            className="flex items-center"
          >
            <Save className="mr-2 h-4 w-4" />
            {isSubmitting ? "Saving..." : submitButtonText}
          </Button>
        </div>
      </form>
    </>
  );
}

