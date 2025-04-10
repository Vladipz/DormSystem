import { Button } from "@/components/ui/button";
import {
    Card,
    CardContent,
    CardFooter,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Event } from "@/lib/types/event";
import { Link } from "@tanstack/react-router";
import { Calendar, Globe, Lock, MapPin, Users } from "lucide-react";

interface EventCardProps {
  event: Event;
  onJoin: (eventId: string) => void;
  isJoining: boolean;
}

export function EventCard({ event, onJoin, isJoining }: EventCardProps) {
  // Format date and time for display
  const formatDateTime = (dateString: string) => {
    const date = new Date(dateString);
    const formattedDate = date.toLocaleDateString();
    const formattedTime = date.toLocaleTimeString(undefined, {
      hour: '2-digit',
      minute: '2-digit'
    });
    return `${formattedDate}, ${formattedTime}`;
  };

  return (
    <Card key={event.id} className="overflow-hidden h-full">
      <CardHeader>
        <CardTitle className="text-lg font-bold">{event.name}</CardTitle>
      </CardHeader>
      <CardContent className="flex-1">
        <div className="space-y-2">
          <div className="flex items-center text-sm">
            <Calendar className="mr-2 h-4 w-4" />
            <span>{formatDateTime(event.date)}</span>
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
              {event.numberOfAttendees && ` (max ${event.numberOfAttendees})`}
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
      <CardFooter className="flex justify-between">
        <Button variant="outline" size="sm" asChild>
          <Link to="/events/$eventId" params={{ eventId: event.id }}>
            Details
          </Link>
        </Button>
        <Button size="sm" onClick={() => onJoin(event.id)} disabled={isJoining}>
          {isJoining ? "Joining..." : "Join Event"}
        </Button>
      </CardFooter>
    </Card>
  );
}
