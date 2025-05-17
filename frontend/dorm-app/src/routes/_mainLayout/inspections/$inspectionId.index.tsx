import { InspectionDetails } from '@/components/inspection/InspectionDetails';
import type { Inspection, RoomInspectionStatus } from '@/lib/types/inspection';
import { createFileRoute, useNavigate, useParams } from '@tanstack/react-router';
import { useEffect, useState } from 'react';
import { toast } from 'sonner';


export const Route = createFileRoute('/_mainLayout/inspections/$inspectionId/')({
  component: InspectionDetailRoute,
})

function InspectionDetailRoute() {
  const { inspectionId } = useParams({ from: '/_mainLayout/inspections/$inspectionId/' });
  const navigate = useNavigate();
  const [inspection, setInspection] = useState<Inspection | null>(null);
  
  // In a real app, you would fetch the inspection data from an API
  // For now, we'll use a mock data structure (similar to the one in index.tsx)
  useEffect(() => {
    // This would be an API call in a real application
    const mockInspections: Inspection[] = [
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
        ],
      },
      {
        id: "2",
        name: "Quarterly Maintenance Check",
        type: "Maintenance",
        startDate: new Date(2025, 4, 15, 10, 30),
        status: "active",
        rooms: [
          {
            id: "r9",
            roomNumber: "101",
            floor: "1",
            building: "B",
            status: "confirmed",
          },
          {
            id: "r10",
            roomNumber: "102",
            floor: "1",
            building: "B",
            status: "not_confirmed",
            comment: "Issues with plumbing",
          },
        ],
      }
    ];
    
    const foundInspection = mockInspections.find(i => i.id === inspectionId);
    setInspection(foundInspection || null);
  }, [inspectionId]);

  const handleUpdateRoomStatus = (
    roomId: string,
    status: RoomInspectionStatus,
    comment?: string,
  ) => {
    if (!inspection) return;
    
    setInspection({
      ...inspection,
      rooms: inspection.rooms.map((room) => {
        if (room.id === roomId) {
          return { ...room, status, comment };
        }
        return room;
      }),
    });

    toast.success(`Room status has been updated to ${status.replace("_", " ")}.`);
  };

  const handleCompleteInspection = () => {
    if (!inspection) return;
    
    setInspection({
      ...inspection,
      status: "completed",
    });

    toast.success("Inspection Completed", {
      description: "The inspection has been marked as completed.",
    });
  };

  const handleGenerateReport = () => {
    // In a real application, this would generate a PDF
    toast.success("Report Generated", {
      description: `Report has been generated and is ready for download.`,
    });
  };

  const handleStartInspection = () => {
    if (!inspection) return;
    
    setInspection({
      ...inspection,
      status: "active",
    });

    toast.success("Inspection Started", {
      description: "The inspection has been marked as active.",
    });
  };

  if (!inspection) {
    return <div>Loading inspection...</div>;
  }

  return (
    <InspectionDetails
      inspection={inspection}
      onBack={() => navigate({ to: '/inspections' })}
      onUpdateRoomStatus={handleUpdateRoomStatus}
      onCompleteInspection={handleCompleteInspection}
      onGenerateReport={handleGenerateReport}
      onStartInspection={handleStartInspection}
    />
  );
}
