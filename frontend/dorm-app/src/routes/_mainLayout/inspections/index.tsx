import { InspectionCard } from "@/components/inspection/InspectionCard";
import {
  Button,
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger
} from "@/components/ui";
import type { Inspection } from "@/lib/types/inspection";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import {
  Plus
} from "lucide-react";
import { useState } from "react";

export const Route = createFileRoute("/_mainLayout/inspections/")({
  component: InspectionsPage,
});

// Re-export types for backward compatibility (can be removed later when all files are updated)
export type { Inspection, InspectionStatus, RoomInspection, RoomInspectionStatus } from "@/lib/types/inspection";

export function InspectionsPage() {
  const navigate = useNavigate();

  const [inspections] = useState<Inspection[]>([
    {
      id: "1",
      name: "Monthly Safety Inspection",
      type: "Safety",
      startDate: new Date(2025, 4, 10, 9, 0),
      status: "scheduled",
      rooms: [
        {
          id: "r1",
          roomNumber: "101",
          floor: "1",
          building: "A",
          status: "pending",
        },
        {
          id: "r2",
          roomNumber: "102",
          floor: "1",
          building: "A",
          status: "pending",
        },
        {
          id: "r3",
          roomNumber: "103",
          floor: "1",
          building: "A",
          status: "pending",
        },
        {
          id: "r4",
          roomNumber: "201",
          floor: "2",
          building: "A",
          status: "pending",
        },
        {
          id: "r5",
          roomNumber: "202",
          floor: "2",
          building: "A",
          status: "pending",
        },
        {
          id: "r6",
          roomNumber: "203",
          floor: "2",
          building: "A",
          status: "pending",
        },
        {
          id: "r7",
          roomNumber: "301",
          floor: "3",
          building: "A",
          status: "pending",
        },
        {
          id: "r8",
          roomNumber: "302",
          floor: "3",
          building: "A",
          status: "pending",
        },
      ],
    },
    {
      id: "2",
      name: "Quarterly Maintenance Check",
      type: "Maintenance",
      startDate: new Date(2025, 4, 15, 10, 30),
      status: "scheduled",
      rooms: [
        {
          id: "r9",
          roomNumber: "101",
          floor: "1",
          building: "B",
          status: "pending",
        },
        {
          id: "r10",
          roomNumber: "102",
          floor: "1",
          building: "B",
          status: "pending",
        },
        {
          id: "r11",
          roomNumber: "201",
          floor: "2",
          building: "B",
          status: "pending",
        },
        {
          id: "r12",
          roomNumber: "202",
          floor: "2",
          building: "B",
          status: "pending",
        },
        {
          id: "r13",
          roomNumber: "301",
          floor: "3",
          building: "B",
          status: "pending",
        },
        {
          id: "r14",
          roomNumber: "302",
          floor: "3",
          building: "B",
          status: "pending",
        },
      ],
    },
    {
      id: "3",
      name: "Annual Fire Safety Inspection",
      type: "Fire Safety",
      startDate: new Date(2025, 3, 20, 14, 0),
      status: "active",
      rooms: [
        {
          id: "r15",
          roomNumber: "101",
          floor: "1",
          building: "C",
          status: "confirmed",
        },
        {
          id: "r16",
          roomNumber: "102",
          floor: "1",
          building: "C",
          status: "not_confirmed",
          comment: "Smoke detector needs battery replacement",
        },
        {
          id: "r17",
          roomNumber: "103",
          floor: "1",
          building: "C",
          status: "pending",
        },
        {
          id: "r18",
          roomNumber: "201",
          floor: "2",
          building: "C",
          status: "no_access",
          comment: "No answer after 3 attempts",
        },
        {
          id: "r19",
          roomNumber: "202",
          floor: "2",
          building: "C",
          status: "pending",
        },
        {
          id: "r20",
          roomNumber: "301",
          floor: "3",
          building: "C",
          status: "pending",
        },
      ],
    },
    {
      id: "4",
      name: "End of Semester Inspection",
      type: "General",
      startDate: new Date(2025, 3, 5, 9, 0),
      status: "completed",
      rooms: [
        {
          id: "r21",
          roomNumber: "401",
          floor: "4",
          building: "A",
          status: "confirmed",
        },
        {
          id: "r22",
          roomNumber: "402",
          floor: "4",
          building: "A",
          status: "confirmed",
        },
        {
          id: "r23",
          roomNumber: "403",
          floor: "4",
          building: "A",
          status: "not_confirmed",
          comment: "Damage to wall needs repair",
        },
        {
          id: "r24",
          roomNumber: "404",
          floor: "4",
          building: "A",
          status: "no_access",
          comment: "Resident on vacation",
        },
      ],
    },
  ]);

  // Return the list of inspections
  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Dorm Inspections</h1>
        <Button onClick={() => navigate({ to: '/inspections/create' })}>
          <Plus className="mr-2 h-4 w-4" /> Create Inspection
        </Button>
      </div>

      <Tabs defaultValue="all">
        <TabsList>
          <TabsTrigger value="all">All</TabsTrigger>
          <TabsTrigger value="scheduled">Scheduled</TabsTrigger>
          <TabsTrigger value="active">Active</TabsTrigger>
          <TabsTrigger value="completed">Completed</TabsTrigger>
        </TabsList>

        <TabsContent value="all" className="mt-4 space-y-4">
          {inspections.map((inspection) => (
            <InspectionCard
              key={inspection.id}
              inspection={inspection}
              onClick={() => navigate({ to: `/inspections/${inspection.id}` })}
            />
          ))}
        </TabsContent>

        <TabsContent value="scheduled" className="mt-4 space-y-4">
          {inspections
            .filter((i) => i.status === "scheduled")
            .map((inspection) => (
              <InspectionCard
                key={inspection.id}
                inspection={inspection}
                onClick={() => navigate({ to: `/inspections/${inspection.id}` })}
              />
            ))}
        </TabsContent>

        <TabsContent value="active" className="mt-4 space-y-4">
          {inspections
            .filter((i) => i.status === "active")
            .map((inspection) => (
              <InspectionCard
                key={inspection.id}
                inspection={inspection}
                onClick={() => navigate({ to: `/inspections/${inspection.id}` })}
              />
            ))}
        </TabsContent>

        <TabsContent value="completed" className="mt-4 space-y-4">
          {inspections
            .filter((i) => i.status === "completed")
            .map((inspection) => (
              <InspectionCard
                key={inspection.id}
                inspection={inspection}
                onClick={() => navigate({ to: `/inspections/${inspection.id}` })}
              />
            ))}
        </TabsContent>
      </Tabs>
    </div>
  );
}


