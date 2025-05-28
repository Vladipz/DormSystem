import { InspectionDetails } from "@/components/inspection/InspectionDetails";
import {
  useCompleteInspection,
  useGenerateReport,
  useInspection,
  useStartInspection,
  useUpdateRoomStatus,
} from "@/lib/hooks/useInspections";
import { authService } from "@/lib/services/authService";
import { ReportStyle } from "@/lib/services/inspectionService";
import type { RoomInspectionStatus } from "@/lib/types/inspection";
import {
  createFileRoute,
  redirect,
  useNavigate,
  useParams,
} from "@tanstack/react-router";
import { toast } from "sonner";

export const Route = createFileRoute("/_mainLayout/inspections/$inspectionId/")(
  {
    beforeLoad: async () => {
      const authStatus = await authService.checkAuthStatus();
      if (!authStatus || !authStatus.isAuthenticated) {
        throw redirect({ to: "/login", search: { returnTo: "/inspections" } });
      }
      if (authStatus.role !== "Admin") {
        throw redirect({ to: "/inspections" });
      }
    },
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
  // Mutations
  const { mutate: updateRoomStatus } = useUpdateRoomStatus(inspectionId);
  const { mutate: startInspection } = useStartInspection();
  const { mutate: completeInspection } = useCompleteInspection();
  const { mutate: generateReport, isPending: isGeneratingReport } = useGenerateReport();

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

  const handleGenerateReport = (style: ReportStyle = "simple") => {
    generateReport(
      { id: inspectionId, style },
      {
        onSuccess: (blob) => {
          // Create a URL for the blob
          const url = URL.createObjectURL(blob);
          
          // Create a link element
          const link = document.createElement('a');
          link.href = url;
          link.download = `inspection-${inspectionId}-${style}.pdf`;
          
          // Append to the document, click it, and remove it
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
          
          // Release the blob URL
          URL.revokeObjectURL(url);
          
          toast.success("Report Generated", {
            description: `${style.charAt(0).toUpperCase() + style.slice(1)} report has been generated and downloaded.`,
          });
        },
        onError: (error) => {
          toast.error("Failed to generate report", {
            description: error.message,
          });
        },
      }
    );
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
      isGeneratingReport={isGeneratingReport}
    />
  );
}
