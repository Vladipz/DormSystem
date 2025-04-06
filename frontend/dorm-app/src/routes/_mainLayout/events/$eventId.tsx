import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_mainLayout/events/$eventId")({
  component: RouteComponent,
});

function RouteComponent() {
  const { eventId } = Route.useParams();
  return <div>Hello "/_mainLayout/events/{eventId}"</div>;
}
