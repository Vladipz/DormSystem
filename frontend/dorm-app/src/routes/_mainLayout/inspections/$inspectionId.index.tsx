import { InspectionDetails } from "@/components/inspection/InspectionDetails";
import {
  useCompleteInspection,
  useInspection,
  useStartInspection,
  useUpdateRoomStatus,
} from "@/lib/hooks/useInspections";
import type { RoomInspectionStatus } from "@/lib/types/inspection";
import {
  createFileRoute,
  useNavigate,
  useParams,
} from "@tanstack/react-router";
import { toast } from "sonner";

export const Route = createFileRoute("/_mainLayout/inspections/$inspectionId/")(
  {
    component: InspectionDetailRoute,
  },
);

function InspectionDetailRoute() {
  const { inspectionId } = useParams({
    from: "/_mainLayout/inspections/$inspectionId/",
  });
  const navigate = useNavigate();

  // Fetch inspection data
  const { data: inspection, isLoading } = useInspection(inspectionId);
  console.log(inspection);
  // Mutations
  const { mutate: updateRoomStatus } = useUpdateRoomStatus(inspectionId);
  const { mutate: startInspection } = useStartInspection();
  const { mutate: completeInspection } = useCompleteInspection();

  const handleUpdateRoomStatus = (
    roomId: string,
    status: RoomInspectionStatus,
    comment?: string,
  ) => {
    updateRoomStatus(
      {
        roomId,
        data: { status, comment },
      },
      {
        onSuccess: () => {
          toast.success(
            `Room status has been updated to ${status.replace("_", " ")}.`,
          );
        },
        onError: (error) => {
          toast.error("Failed to update room status", {
            description: error.message,
          });
        },
      },
    );
  };

  const handleCompleteInspection = () => {
    completeInspection(inspectionId, {
      onSuccess: () => {
        toast.success("Inspection Completed", {
          description: "The inspection has been marked as completed.",
        });
      },
      onError: (error) => {
        toast.error("Failed to complete inspection", {
          description: error.message,
        });
      },
    });
  };

  const handleGenerateReport = () => {
    //TODO: generate report
    // In a real application, this would generate a PDF
    toast.success("Report Generated", {
      description: `Report has been generated and is ready for download.`,
    });
  };

  const handleStartInspection = () => {
    startInspection(inspectionId, {
      onSuccess: () => {
        toast.success("Inspection Started", {
          description: "The inspection has been marked as active.",
        });
      },
      onError: (error) => {
        toast.error("Failed to start inspection", {
          description: error.message,
        });
      },
    });
  };

  if (isLoading || !inspection) {
    return <div>Loading inspection...</div>;
  }

  return (
    <InspectionDetails
      inspection={inspection}
      onBack={() => navigate({ to: "/inspections" })}
      onUpdateRoomStatus={handleUpdateRoomStatus}
      onCompleteInspection={handleCompleteInspection}
      onGenerateReport={handleGenerateReport}
      onStartInspection={handleStartInspection}
    />
  );
}
