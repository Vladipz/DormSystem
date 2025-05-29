import { RoomPhotoManager } from "@/components/room/RoomPhotoManager";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { useRoomById, useUpdateRoom } from "@/lib/hooks/useRooms";
import { RoomStatus, RoomType, UpdateRoomRequest } from "@/lib/types/room";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useFormik } from "formik";
import { ArrowLeft, Loader2, Save } from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import * as Yup from "yup";

// Define validation schema using Yup
const RoomSchema = Yup.object().shape({
  label: Yup.string()
    .min(2, "Label must be at least 2 characters")
    .max(50, "Label must be less than 50 characters")
    .required("Room label is required"),
  capacity: Yup.number()
    .min(1, "Capacity must be at least 1")
    .integer("Capacity must be a whole number")
    .required("Capacity is required"),
  status: Yup.string()
    .oneOf(["Available", "Occupied", "Maintenance"], "Invalid room status")
    .required("Status is required"),
  roomType: Yup.string()
    .oneOf(["Regular", "Specialized"], "Invalid room type")
    .required("Room type is required"),
  purpose: Yup.string().nullable(),
  amenities: Yup.array().of(Yup.string()),
});

export const Route = createFileRoute("/_mainLayout/rooms/$roomId/edit")({
  component: RoomEditPage,
});

export default function RoomEditPage() {
  const { roomId } = Route.useParams();
  const navigate = useNavigate();
  const [amenity, setAmenity] = useState("");

  // Get room data
  const { data: room, isLoading, isError, error } = useRoomById(roomId || "");

  // Update room mutation
  const updateRoomMutation = useUpdateRoom();

  const formik = useFormik({
    initialValues: {
      id: "",
      blockId: "",
      label: "",
      capacity: 1,
      status: "Available" as RoomStatus,
      roomType: "Regular" as RoomType,
      purpose: "",
      amenities: [] as string[],
    },
    validationSchema: RoomSchema,
    onSubmit: (values) => {
      const payload: UpdateRoomRequest = {
        id: roomId || "",
        blockId: values.blockId || null,
        label: values.label,
        capacity: values.capacity,
        status: values.status,
        roomType: values.roomType,
        purpose: values.purpose || null,
        amenities: values.amenities,
        photoIds: room?.photoUrls?.map(url => url.split('/').pop() || '') || [],
      };

      updateRoomMutation.mutate(payload, {
        onSuccess: () => {
          toast.success("Room updated successfully");
          navigate({ to: `/_mainLayout/rooms/${roomId}` });
        },
        onError: (err) => {
          toast.error("Failed to update room");
          console.error("Error updating room:", err);
        },
      });
    },
  });

  // Set form values when room data is loaded
  useEffect(() => {
    if (room) {
      formik.setValues({
        id: room.id,
        blockId: room.block?.id || "",
        label: room.label,
        capacity: room.capacity,
        status: room.status,
        roomType: room.roomType,
        purpose: room.purpose || "",
        amenities: room.amenities || [],
      });
    }
  }, [room]);

  // Handle adding amenities
  const handleAddAmenity = () => {
    if (amenity.trim() && !formik.values.amenities.includes(amenity.trim())) {
      formik.setFieldValue("amenities", [
        ...formik.values.amenities,
        amenity.trim(),
      ]);
      setAmenity("");
    }
  };

  // Handle removing amenities
  const handleRemoveAmenity = (amenityToRemove: string) => {
    formik.setFieldValue(
      "amenities",
      formik.values.amenities.filter((a) => a !== amenityToRemove),
    );
  };

  if (isLoading) {
    return (
      <div className="flex h-[60vh] items-center justify-center">
        <Loader2 className="text-primary h-8 w-8 animate-spin" />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="mb-4 rounded border border-red-400 bg-red-100 px-4 py-3 text-red-700">
        <p>
          Error loading room:{" "}
          {error instanceof Error ? error.message : "Unknown error"}
        </p>
        <Button
          variant="outline"
          onClick={() => navigate({ to: "/room-dashboard" })}
          className="mt-4"
        >
          <ArrowLeft className="mr-2 h-4 w-4" /> Back to Rooms
        </Button>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6">
      <div className="mb-6 flex items-center">
        <Button
          variant="outline"
          onClick={() => navigate({ to: `/rooms/${roomId}` })}
          className="mr-4"
        >
          <ArrowLeft className="mr-2 h-4 w-4" /> Back
        </Button>
        <h1 className="text-2xl font-bold">Edit Room</h1>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Room Details</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={formik.handleSubmit} className="space-y-6">
            {/* Room Label */}
            <div className="space-y-2">
              <Label htmlFor="label">Room Label</Label>
              <Input
                id="label"
                name="label"
                value={formik.values.label}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                className={
                  formik.errors.label && formik.touched.label
                    ? "border-red-500"
                    : ""
                }
              />
              {formik.errors.label && formik.touched.label && (
                <p className="mt-1 text-xs text-red-500">
                  {formik.errors.label}
                </p>
              )}
            </div>

            {/* Capacity */}
            <div className="space-y-2">
              <Label htmlFor="capacity">Capacity</Label>
              <Input
                id="capacity"
                name="capacity"
                type="number"
                min="1"
                value={formik.values.capacity}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                className={
                  formik.errors.capacity && formik.touched.capacity
                    ? "border-red-500"
                    : ""
                }
              />
              {formik.errors.capacity && formik.touched.capacity && (
                <p className="mt-1 text-xs text-red-500">
                  {formik.errors.capacity}
                </p>
              )}
            </div>

            {/* Status */}
            <div className="space-y-2">
              <Label htmlFor="status">Status</Label>
              <Select
                name="status"
                value={formik.values.status}
                onValueChange={(value) => formik.setFieldValue("status", value)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Available">Available</SelectItem>
                  <SelectItem value="Occupied">Occupied</SelectItem>
                  <SelectItem value="Maintenance">Maintenance</SelectItem>
                </SelectContent>
              </Select>
              {formik.errors.status && formik.touched.status && (
                <p className="mt-1 text-xs text-red-500">
                  {formik.errors.status}
                </p>
              )}
            </div>

            {/* Room Type */}
            <div className="space-y-2">
              <Label htmlFor="roomType">Room Type</Label>
              <Select
                name="roomType"
                value={formik.values.roomType}
                onValueChange={(value) =>
                  formik.setFieldValue("roomType", value)
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select room type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Regular">Regular</SelectItem>
                  <SelectItem value="Specialized">Specialized</SelectItem>
                </SelectContent>
              </Select>
              {formik.errors.roomType && formik.touched.roomType && (
                <p className="mt-1 text-xs text-red-500">
                  {formik.errors.roomType}
                </p>
              )}
            </div>

            {/* Purpose */}
            <div className="space-y-2">
              <Label htmlFor="purpose">Purpose (Optional)</Label>
              <Textarea
                id="purpose"
                name="purpose"
                value={formik.values.purpose || ""}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                className={
                  formik.errors.purpose && formik.touched.purpose
                    ? "border-red-500"
                    : ""
                }
              />
              {formik.errors.purpose && formik.touched.purpose && (
                <p className="mt-1 text-xs text-red-500">
                  {formik.errors.purpose}
                </p>
              )}
            </div>

            {/* Amenities */}
            <div className="space-y-2">
              <Label htmlFor="amenities">Amenities</Label>
              <div className="flex gap-2">
                <Input
                  id="amenity-input"
                  value={amenity}
                  onChange={(e) => setAmenity(e.target.value)}
                  placeholder="Add amenity"
                  className="flex-1"
                />
                <Button
                  type="button"
                  onClick={handleAddAmenity}
                  variant="secondary"
                >
                  Add
                </Button>
              </div>
              <div className="mt-2 flex flex-wrap gap-2">
                {formik.values.amenities.map((item, index) => (
                  <Badge key={index} variant="secondary" className="px-3 py-1">
                    {item}
                    <button
                      type="button"
                      onClick={() => handleRemoveAmenity(item)}
                      className="ml-2 text-xs font-bold"
                    >
                      ×
                    </button>
                  </Badge>
                ))}
              </div>
            </div>

            {/* Submit Button */}
            <div className="flex justify-end">
              <Button
                type="submit"
                disabled={updateRoomMutation.isPending || !formik.isValid}
                className="flex items-center"
              >
                {updateRoomMutation.isPending ? (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                ) : (
                  <Save className="mr-2 h-4 w-4" />
                )}
                {updateRoomMutation.isPending ? "Saving..." : "Save Changes"}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>

      {/* Photo Management Section */}
      {room && <RoomPhotoManager room={room} />}
    </div>
  );
}
