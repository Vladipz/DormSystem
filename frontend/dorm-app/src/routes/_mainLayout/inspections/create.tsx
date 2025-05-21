import { CreateInspectionData, CreateInspectionForm } from "@/components/inspection/CreateInspectionForm";
import { useCreateInspection } from "@/lib/hooks/useInspections";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { toast } from "sonner";

export const Route = createFileRoute("/_mainLayout/inspections/create")({
  component: CreateInspectionRoute,
});

function CreateInspectionRoute() {
  const navigate = useNavigate();
  const createInspectionMutation = useCreateInspection();

  const handleCreateInspection = async (newInspection: CreateInspectionData) => {
    try {
      await createInspectionMutation.mutateAsync({
        name: newInspection.name,
        type: newInspection.type,
        startDate: newInspection.startDate,
        mode: newInspection.mode,
        dormitoryId: newInspection.dormitoryId,
        includeSpecialRooms: newInspection.includeSpecialRooms,
        rooms: newInspection.rooms || []
      });

      toast.success(`${newInspection.name} has been scheduled.`, {
        description: `Inspection has been created successfully.`,
      });

      navigate({ to: "/inspections" });
    } catch (error) {
      toast.error("Failed to create inspection", {
        description: error instanceof Error ? error.message : "An unknown error occurred",
      });
    }
  };

  return (
    <CreateInspectionForm
      onSubmit={handleCreateInspection}
      onCancel={() => navigate({ to: "/inspections" })}
    />
  );
}
