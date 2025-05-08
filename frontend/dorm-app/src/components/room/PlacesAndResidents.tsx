import {
  Avatar,
  AvatarFallback,
  AvatarImage,
  Badge,
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui";
import { Bed, User } from "lucide-react";

interface Occupant {
  id: string;
  name: string;
  avatar?: string;
  faculty: string;
  year: number;
}

interface Place {
  id: number;
  index: number;
  occupiedByUserId: string | null;
  movedInAt: string | null;
  occupant: Occupant | null;
}

interface PlacesAndResidentsProps {
  places: Place[];
}

export function PlacesAndResidents({ places }: PlacesAndResidentsProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Places & Residents</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {places.map((place) => (
            <div key={place.id} className="border rounded-lg p-4">
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center">
                  <Bed className="mr-2 h-5 w-5 text-primary" />
                  <h3 className="font-medium">Place {place.index}</h3>
                </div>
                <Badge variant={place.occupiedByUserId ? "secondary" : "outline"}>
                  {place.occupiedByUserId ? "Occupied" : "Available"}
                </Badge>
              </div>
              {place.occupant ? (
                <div className="flex items-start gap-4">
                  <Avatar className="h-10 w-10">
                    <AvatarImage
                      src={place.occupant.avatar || "/placeholder.svg"}
                      alt={place.occupant.name}
                    />
                    <AvatarFallback>{place.occupant.name.charAt(0)}</AvatarFallback>
                  </Avatar>
                  <div>
                    <p className="font-medium">{place.occupant.name}</p>
                    <p className="text-sm text-muted-foreground">
                      {place.occupant.faculty}, Year {place.occupant.year}
                    </p>
                    <p className="text-xs text-muted-foreground mt-1">
                      Moved in: {place.movedInAt && new Date(place.movedInAt).toLocaleDateString()}
                    </p>
                  </div>
                </div>
              ) : (
                <div className="flex items-center text-muted-foreground">
                  <User className="mr-2 h-5 w-5" />
                  <span>No resident assigned</span>
                </div>
              )}
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
