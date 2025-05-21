"use client";

import type React from "react";

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
import { Switch } from "@/components/ui/switch";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useBuildings } from "@/lib/hooks/useBuildings";
import { useRooms } from "@/lib/hooks/useRooms";
import { RoomInfoDto } from "@/lib/services/inspectionService";
import type { Inspection } from "@/lib/types/inspection";
import { ArrowLeft } from "lucide-react";
import { useMemo, useState } from "react";

// Define a type for the new inspection data that includes mode and other fields
export type CreateInspectionData = Omit<Inspection, "id" | "rooms"> & {
  mode: "manual" | "automatic";
  dormitoryId?: string;
  includeSpecialRooms?: boolean;
  rooms?: RoomInfoDto[];
};

interface CreateInspectionFormProps {
  onSubmit: (inspection: CreateInspectionData) => void;
  onCancel: () => void;
}

export function CreateInspectionForm({
  onSubmit,
  onCancel,
}: CreateInspectionFormProps) {
  const [name, setName] = useState("");
  const [type, setType] = useState("");
  const [date, setDate] = useState("");
  const [time, setTime] = useState("");
  const [mode, setMode] = useState<"manual" | "automatic">("automatic");
  const [dormitoryId, setDormitoryId] = useState<string>("");
  const [includeSpecialRooms, setIncludeSpecialRooms] = useState(false);
  const [selectedRoomIds, setSelectedRoomIds] = useState<Set<string>>(
    new Set(),
  );

  // Fetch dormitories
  const { data: dormitories } = useBuildings();

  // Fetch rooms if in manual mode
  const { data: rooms, isLoading: isRoomsLoading } = useRooms(
    undefined,
    mode === "manual",
  );

  const selectedRooms = useMemo(() => {
    if (!rooms) return [];
    return Array.from(selectedRoomIds)
      .map((roomId) => {
        const room = rooms.find((r) => r.id === roomId);
        if (!room) return null;
        return {
          roomId: room.id,
          roomNumber: room.label,
          // We'll have to work with what we have in the current data model
          floor: "1", // This would ideally come from the room data
          building: "Main Building", // This would ideally come from the room data
        } as RoomInfoDto;
        //TODO: change to real data
      })
      .filter(Boolean) as RoomInfoDto[];
  }, [rooms, selectedRoomIds]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    // Combine date and time into a Date object
    const [year, month, day] = date.split("-").map(Number);
    const [hours, minutes] = time.split(":").map(Number);

    const startDate = new Date(year, month - 1, day, hours, minutes);

    onSubmit({
      name,
      type,
      startDate,
      status: "Scheduled", // Fixed to match the InspectionStatus type
      mode,
      ...(mode === "automatic"
        ? {
            dormitoryId,
            includeSpecialRooms,
          }
        : {
            rooms: selectedRooms,
          }),
    });
  };

  const handleRoomToggle = (roomId: string, checked: boolean) => {
    const newSelectedRooms = new Set(selectedRoomIds);
    if (checked) {
      newSelectedRooms.add(roomId);
    } else {
      newSelectedRooms.delete(roomId);
    }
    setSelectedRoomIds(newSelectedRooms);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center space-x-2">
        <Button variant="ghost" size="sm" onClick={onCancel}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Inspections
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Create New Inspection</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Inspection Name</Label>
              <Input
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Enter inspection name"
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="type">Inspection Type</Label>
              <Select value={type} onValueChange={setType} required>
                <SelectTrigger id="type">
                  <SelectValue placeholder="Select inspection type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Safety">Safety</SelectItem>
                  <SelectItem value="Maintenance">Maintenance</SelectItem>
                  <SelectItem value="Fire Safety">Fire Safety</SelectItem>
                  <SelectItem value="Health">Health</SelectItem>
                  <SelectItem value="General">General</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="flex flex-col space-y-2">
              <div className="space-y-2">
                <Label htmlFor="date">Date</Label>
                <Input
                  id="date"
                  type="date"
                  value={date}
                  onChange={(e) => setDate(e.target.value)}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="time">Time</Label>
                <Input
                  id="time"
                  type="time"
                  value={time}
                  onChange={(e) => setTime(e.target.value)}
                  required
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label>Room Selection Mode</Label>
              <Tabs
                defaultValue="automatic"
                onValueChange={(value) =>
                  setMode(value as "manual" | "automatic")
                }
              >
                <TabsList className="w-full">
                  <TabsTrigger value="automatic" className="flex-1">
                    Automatic (by Dormitory)
                  </TabsTrigger>
                  <TabsTrigger value="manual" className="flex-1">
                    Manual (Select Rooms)
                  </TabsTrigger>
                </TabsList>

                <TabsContent value="automatic" className="mt-4 space-y-4">
                  <div className="space-y-2">
                    <Label htmlFor="dormitory">Dormitory</Label>
                    <Select
                      value={dormitoryId}
                      onValueChange={setDormitoryId}
                      required={mode === "automatic"}
                    >
                      <SelectTrigger id="dormitory">
                        <SelectValue placeholder="Select dormitory" />
                      </SelectTrigger>
                      <SelectContent>
                        {dormitories?.map((dorm) => (
                          <SelectItem key={dorm.id} value={dorm.id}>
                            {dorm.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="flex items-center space-x-2">
                    <Switch
                      id="includeSpecial"
                      checked={includeSpecialRooms}
                      onCheckedChange={setIncludeSpecialRooms}
                    />
                    <Label htmlFor="includeSpecial">
                      Include special rooms
                    </Label>
                  </div>
                </TabsContent>

                <TabsContent value="manual" className="mt-4 space-y-4">
                  <div className="max-h-60 space-y-2 overflow-y-auto rounded-md border p-2">
                    {isRoomsLoading ? (
                      <div className="p-4 text-center">Loading rooms...</div>
                    ) : rooms?.length ? (
                      <div className="grid grid-cols-1 gap-2 md:grid-cols-2">
                        {rooms.map((room) => (
                          <div
                            key={room.id}
                            className="flex items-center space-x-2 rounded border p-2 hover:bg-gray-50"
                          >
                            <Switch
                              checked={selectedRoomIds.has(room.id)}
                              onCheckedChange={(checked) =>
                                handleRoomToggle(room.id, checked)
                              }
                            />
                            <div>
                              <p className="font-medium">{room.label}</p>
                              <p className="text-xs text-gray-500">
                                {room.roomType} • Capacity: {room.capacity}
                              </p>
                            </div>
                          </div>
                        ))}
                      </div>
                    ) : (
                      <div className="p-4 text-center">No rooms available</div>
                    )}
                  </div>

                  <div className="text-sm text-gray-500">
                    {selectedRoomIds.size} rooms selected
                  </div>
                </TabsContent>
              </Tabs>
            </div>

            <div className="flex justify-end space-x-2 pt-4">
              <Button type="button" variant="outline" onClick={onCancel}>
                Cancel
              </Button>
              <Button type="submit">Create Inspection</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
