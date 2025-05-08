import { Button } from "@/components/ui";
import { createFileRoute } from "@tanstack/react-router";
import { toast } from "sonner"; // Make sure both come from the same package

export const Route = createFileRoute("/_mainLayout/")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <div>
      <p>dadsa</p>
    </div>
  );
}
