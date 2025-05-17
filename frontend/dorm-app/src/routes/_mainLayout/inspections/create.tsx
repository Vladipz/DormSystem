import { CreateInspectionForm } from "@/components/inspection/CreateInspectionForm";
import type { Inspection } from "@/lib/types/inspection";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { toast } from "sonner";

export const Route = createFileRoute("/_mainLayout/inspections/create")({
  component: CreateInspectionRoute,
});

function CreateInspectionRoute() {
  const navigate = useNavigate();

  const handleCreateInspection = (
    newInspection: Omit<Inspection, "id" | "rooms">,
  ) => {
    // Here you would typically make an API call to save the inspection
    // For now, we're just showing a toast and navigating back
    toast.success(`${newInspection.name} has been scheduled.`, {
      description: `${newInspection.name} has been scheduled.`,
    });

    navigate({ to: "/inspections" });
  };

  return (
    <CreateInspectionForm
      onSubmit={handleCreateInspection}
      onCancel={() => navigate({ to: "/inspections" })}
    />
  );
}
