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
import { useFloors } from "@/lib/hooks/useFloors";
import { useRoomsOnFloor } from "@/lib/hooks/useRooms";
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

  // Manual mode filters
  const [selectedBuildingId, setSelectedBuildingId] = useState<string>("");
  const [selectedFloor, setSelectedFloor] = useState<string>("");

  // Fetch dormitories
  const { data: dormitories } = useBuildings();

  // Fetch floors for selected building
  const { data: floors } = useFloors(
    selectedBuildingId,
    mode === "manual" && !!selectedBuildingId,
  );

  // Fetch rooms with server-side filtering by building and floor
  const { data: rooms, isLoading: isRoomsLoading } = useRoomsOnFloor(
    selectedFloor || "", // floorId - pass empty string if no floor selected
  );
  //console the params and the rooms
  console.log("selectedBuildingId", selectedBuildingId);
  console.log("selectedFloor", selectedFloor);
  console.log("rooms", rooms);

  // Since we're using server-side filtering, we can use rooms directly
  const filteredRooms = rooms || [];

  const selectedRooms = useMemo(() => {
    if (!filteredRooms) return [];
    return Array.from(selectedRoomIds)
      .map((roomId) => {
        const room = filteredRooms.find((r) => r.id === roomId);
        if (!room) return null;
        
        // Get building name from dormitories data
        const selectedBuilding = selectedBuildingId ? 
          dormitories?.find(building => building.id === selectedBuildingId) : null;
        const buildingName = selectedBuilding?.name || "Multiple Buildings";
        
        // Get floor number from floors data
        const selectedFloorData = selectedFloor ? 
          floors?.find(floor => floor.id === selectedFloor) : null;
        const floorNumber = selectedFloorData?.number?.toString() || "Multiple Floors";
        
        return {
          roomId: room.id,
          roomNumber: room.label,
          floor: floorNumber,
          building: buildingName,
        } as RoomInfoDto;
      })
      .filter(Boolean) as RoomInfoDto[];
  }, [filteredRooms, selectedRoomIds, dormitories, selectedBuildingId, floors, selectedFloor]);

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

  const handleBuildingChange = (buildingId: string) => {
    const newBuildingId = buildingId === "allBuildings" ? "" : buildingId;
    setSelectedBuildingId(newBuildingId);
    // Reset floor selection when building changes
    setSelectedFloor("");
    // Reset selected rooms when building changes
    setSelectedRoomIds(new Set());
  };

  const handleFloorChange = (floorId: string) => {
    const newFloorId = floorId === "allFloors" ? "" : floorId;
    setSelectedFloor(newFloorId);
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
                  {/* Building Filter */}
                  <div className="grid gap-4 md:grid-cols-2">
                    <div className="space-y-2">
                      <Label htmlFor="building-filter">
                        Filter by Building
                      </Label>
                      <Select
                        value={selectedBuildingId}
                        onValueChange={handleBuildingChange}
                      >
                        <SelectTrigger id="building-filter" className="w-full">
                          <SelectValue
                            placeholder="All buildings"
                            className="truncate"
                          />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="allBuildings">
                            All buildings
                          </SelectItem>
                          {dormitories?.map((building) => (
                            <SelectItem key={building.id} value={building.id}>
                              <div className="max-w-[300px] truncate">
                                {building.name}
                              </div>
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="floor-filter">Filter by Floor</Label>
                      <Select
                        value={selectedFloor}
                        onValueChange={handleFloorChange}
                        disabled={!selectedBuildingId}
                      >
                        <SelectTrigger id="floor-filter" className="w-full">
                          <SelectValue
                            placeholder="All floors"
                            className="truncate"
                          />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="allFloors">All floors</SelectItem>
                          {floors?.map((floor) => (
                            <SelectItem key={floor.id} value={floor.id}>
                              Floor {floor.number}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                  </div>

                  <div className="max-h-60 space-y-2 overflow-y-auto rounded-md border p-2">
                    {isRoomsLoading ? (
                      <div className="p-4 text-center">Loading rooms...</div>
                    ) : filteredRooms?.length ? (
                      <div className="grid grid-cols-1 gap-2 md:grid-cols-2">
                        {filteredRooms.map((room) => (
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
