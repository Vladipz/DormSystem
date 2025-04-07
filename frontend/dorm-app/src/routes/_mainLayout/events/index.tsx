import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { useEvents } from "@/lib/hooks/useEvents";
import { createFileRoute, Link } from "@tanstack/react-router";
import { Calendar, Globe, Lock, MapPin, Plus, Search, Users } from "lucide-react";
import { useState } from "react";

export const Route = createFileRoute("/_mainLayout/events/")({
  component: RouteComponent,
});

function RouteComponent() {
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedEventId] = useState<string | null>(null);
  const { events, loading, error, joinEvent } = useEvents();
  const [joiningEventId, setJoiningEventId] = useState<string | null>(null);

  const handleJoinEvent = async (eventId: string) => {
    setJoiningEventId(eventId);
    try {
      // TODO: Get actual user ID from auth context
      const userId = "a92b0237-c938-4a68-9750-9aa3f7d834a7";
      await joinEvent(eventId, userId);
    } catch (error) {
      console.error("Failed to join event:", error);
    } finally {
      setJoiningEventId(null);
    }
  };

  // If an event is selected, show its details
  if (selectedEventId !== null) {
    return <div>event details</div>;
  }

  if (error) {
    return (
      <div className="text-center p-8">
        <h2 className="text-2xl font-bold text-destructive mb-4">
          Error loading events
        </h2>
        <p className="text-muted-foreground">Please try again later</p>
      </div>
    );
  }

  return (
    <div className="space-y-4 py-6">
      <div className="flex flex-col sm:flex-row justify-between gap-4">
        <div className="relative flex-1">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input
            type="search"
            placeholder="Search events..."
            className="pl-8"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
        <Button asChild>
          <Link to="/events/create">
            <Plus className="mr-2 h-4 w-4" /> Create Event
          </Link>
        </Button>
      </div>

      {loading ? (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {[1, 2, 3].map((i) => (
            <Card key={i} className="overflow-hidden animate-pulse">
              <div className="w-full h-40 bg-muted" />
              <CardHeader>
                <div className="h-6 w-2/3 bg-muted rounded" />
              </CardHeader>
              <CardContent>
                <div className="space-y-2">
                  <div className="h-4 bg-muted rounded w-full" />
                  <div className="h-4 bg-muted rounded w-2/3" />
                </div>
              </CardContent>
              <CardFooter className="flex justify-between">
                <div className="h-9 w-20 bg-muted rounded" />
                <div className="h-9 w-20 bg-muted rounded" />
              </CardFooter>
            </Card>
          ))}
        </div>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {events.map((event) => (
            <Card key={event.id} className="overflow-hidden h-full">
              <CardHeader>
                <CardTitle className="text-lg font-bold">
                  {event.name}
                </CardTitle>
              </CardHeader>
              <CardContent className="flex-1">
                <div className="space-y-2">
                  <div className="flex items-center text-sm">
                    <Calendar className="mr-2 h-4 w-4" />
                    <span>{new Date(event.date).toLocaleDateString()}</span>
                  </div>
                  <div className="flex items-center text-sm">
                    <MapPin className="mr-2 h-4 w-4" />
                    <span>{event.location}</span>
                  </div>
                  <div className="flex items-center text-sm">
                    <Users className="mr-2 h-4 w-4" />
                    <span>
                      {event.lastParticipants.length} participant
                      {event.lastParticipants.length !== 1 ? "s" : ""}
                      {event.numberOfAttendees &&
                        ` (max ${event.numberOfAttendees})`}
                    </span>
                  </div>
                  <div className="flex items-center text-sm">
                    {event.isPublic ? (
                      <div className="flex items-center text-green-600">
                        <Globe className="mr-2 h-4 w-4" />
                        <span>Public event</span>
                      </div>
                    ) : (
                      <div className="flex items-center text-gray-600">
                        <Lock className="mr-2 h-4 w-4" />
                        <span>Private event - Invitation required</span>
                      </div>
                    )}
                  </div>
                </div>
              </CardContent>
              <CardFooter className="flex justify-between ">
                <Button variant="outline" size="sm" asChild>
                  <Link to="/events/$eventId" params={{ eventId: event.id }}>
                    Details
                  </Link>
                </Button>
                <Button
                  size="sm"
                  onClick={() => handleJoinEvent(event.id)}
                  disabled={joiningEventId === event.id}
                >
                  {joiningEventId === event.id ? "Joining..." : "Join Event"}
                </Button>
              </CardFooter>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
