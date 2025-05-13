import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import { useBuildings } from "@/lib/hooks/useBuildings";
import { useRooms } from "@/lib/hooks/useRooms";
import { BuildingsResponse } from "@/lib/types/building";
import { CreateEventRequest } from "@/lib/types/event";
import { RoomsResponse } from "@/lib/types/room";
import { useFormik } from "formik";
import { Save, X } from "lucide-react";
import { useEffect, useState } from "react";
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
    .test(
      'location-or-building',
      'Either location or building must be specified',
      function(value) {
        const { buildingId } = this.parent;
        return Boolean(value) || Boolean(buildingId);
      }
    )
    .when('buildingId', {
      is: (buildingId: string) => Boolean(buildingId),
      then: (schema) => schema.optional(),
      otherwise: (schema) => schema
        .required('Location is required when not using a building')
        .min(3, 'Location must be at least 3 characters'),
    }),
  numberOfAttendees: Yup.number()
    .nullable()
    .min(1, "Number of attendees must be at least 1")
    .integer("Number of attendees must be a whole number"),
  isPublic: Yup.boolean().required("Please specify if the event is public"),
  description: Yup.string().max(
    2000,
    "Description must be less than 2000 characters"
  ),
  buildingId: Yup.string()
    .test(
      'building-or-location', 
      'Either location or building must be specified',
      function(value) {
        const { location } = this.parent;
        return Boolean(value) || Boolean(location);
      }
    ),
  roomId: Yup.string().optional(),
});

export interface EventFormValues {
  name: string;
  date: string;
  location: string;
  numberOfAttendees: number | null;
  isPublic: boolean;
  description: string;
  buildingId?: string;
  roomId?: string;
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
  const [useCustomLocation, setUseCustomLocation] = useState<boolean>(!initialValues.buildingId);
  const { data: buildings, isLoading: buildingsLoading } = useBuildings(1, 100, true);
  
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
        numberOfAttendees: values.numberOfAttendees ?? undefined,
        isPublic: values.isPublic,
        description: values.description || "",
      };

      // Only add buildingId and roomId if they are valid non-empty values
      if (!useCustomLocation && values.buildingId) {
        payload.buildingId = values.buildingId;
        
        // Only add roomId if it's a valid non-empty value
        if (values.roomId) {
          payload.roomId = values.roomId;
        }
      }

      onSubmit(payload);
    },
  });
  
  const { data: rooms, isLoading: roomsLoading } = useRooms(
    formik.values.buildingId,
    !!formik.values.buildingId
  );
  
  // Toggle between custom location and building selection
  useEffect(() => {
    if (useCustomLocation) {
      formik.setFieldValue('buildingId', undefined);
      formik.setFieldValue('roomId', undefined);
    } else {
      formik.setFieldValue('location', '');
    }
  }, [useCustomLocation]);

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
        
        <div className="flex items-center space-x-2 mb-2">
          <Switch
            id="locationToggle"
            checked={useCustomLocation}
            onCheckedChange={setUseCustomLocation}
          />
          <Label htmlFor="locationToggle" className="text-sm">
            Use custom location instead of building/room
          </Label>
        </div>

        {useCustomLocation ? (
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
        ) : (
          <>
            <div>
              <Label htmlFor="buildingId" className="block text-sm font-medium mb-1">
                Building
              </Label>
              <Select
                value={formik.values.buildingId}
                onValueChange={(value) => {
                  formik.setFieldValue("buildingId", value);
                  formik.setFieldValue("roomId", undefined);
                }}
              >
                <SelectTrigger
                  id="buildingId"
                  className={
                    formik.errors.buildingId && formik.touched.buildingId ? "border-red-500" : ""
                  }
                >
                  <SelectValue placeholder="Select a building" />
                </SelectTrigger>
                <SelectContent>
                  {buildingsLoading ? (
                    <SelectItem value="loading" disabled>
                      Loading buildings...
                    </SelectItem>
                  ) : !buildings ? (
                    <SelectItem value="none" disabled>
                      No buildings available
                    </SelectItem>
                  ) : (
                    buildings.map((building: BuildingsResponse) => (
                      <SelectItem key={building.id} value={building.id}>
                        {building.name}
                      </SelectItem>
                    ))
                  )}
                </SelectContent>
              </Select>
              {formik.errors.buildingId && formik.touched.buildingId && (
                <p className="text-red-500 text-xs mt-1">{formik.errors.buildingId}</p>
              )}
            </div>

            {formik.values.buildingId && (
              <div>
                <Label htmlFor="roomId" className="block text-sm font-medium mb-1">
                  Room (Optional)
                </Label>
                <Select
                  value={formik.values.roomId}
                  onValueChange={(value) => formik.setFieldValue("roomId", value)}
                >
                  <SelectTrigger
                    id="roomId"
                    className={
                      formik.errors.roomId && formik.touched.roomId ? "border-red-500" : ""
                    }
                  >
                    <SelectValue placeholder="Select a room (optional)" />
                  </SelectTrigger>
                  <SelectContent>
                    {roomsLoading ? (
                      <SelectItem value="loading" disabled>
                        Loading rooms...
                      </SelectItem>
                    ) : !rooms || rooms.length === 0 ? (
                      <SelectItem value="none" disabled>
                        No rooms available
                      </SelectItem>
                    ) : (
                      rooms.map((room: RoomsResponse) => (
                        <SelectItem key={room.id} value={room.id}>
                          {room.label}
                        </SelectItem>
                      ))
                    )}
                  </SelectContent>
                </Select>
                {formik.errors.roomId && formik.touched.roomId && (
                  <p className="text-red-500 text-xs mt-1">{formik.errors.roomId}</p>
                )}
              </div>
            )}
          </>
        )}

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

