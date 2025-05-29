import {
  Avatar,
  AvatarFallback,
  AvatarImage,
  Badge,
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  Skeleton,
} from "@/components/ui";
import { useUser } from "@/lib/hooks/useUser";
import { getPlaceholderAvatar } from "@/lib/utils";
import { Bed, User } from "lucide-react";

interface Place {
  id: string;
  roomId: string;
  index: number;
  isOccupied: boolean;
  movedInAt?: string | null;
  roomLabel: string;
  occupiedByUserId?: string | null;
}

interface PlacesAndResidentsProps {
  places: Place[];
}

function PlaceOccupant({
  userId,
  movedInAt,
}: {
  userId: string | null | undefined;
  movedInAt: string | null | undefined;
}) {
  const { data: user, isLoading } = useUser(userId as string);

  if (!userId) {
    return (
      <div className="text-muted-foreground flex items-center">
        <User className="mr-2 h-5 w-5" />
        <span>No user ID provided</span>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="flex items-start gap-4">
        <Skeleton className="h-10 w-10 rounded-full" />
        <div className="space-y-2">
          <Skeleton className="h-4 w-[200px]" />
          <Skeleton className="h-4 w-[150px]" />
        </div>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="text-muted-foreground flex items-center">
        <User className="mr-2 h-5 w-5" />
        <span>User not found</span>
      </div>
    );
  }

  const handleClick = () => {
    alert(
      //detail user info
      `Resident: ${user.firstName} ${user.lastName}\nFaculty: ${user.faculty}\nYear: ${user.year}`,
    );
  };

  const placeholderAvatar = getPlaceholderAvatar(); // Use user ID to generate a consistent placeholder emoji

  return (
    <div className="flex items-start gap-4" onClick={handleClick}>
      <Avatar className="h-10 w-10">
        <AvatarImage src={user.avatarUrl} alt={`${user.firstName} ${user.lastName}'s avatar`} />
        <AvatarFallback>{placeholderAvatar}</AvatarFallback>
      </Avatar>
      <div>
        <p className="font-medium">{`${user.firstName} ${user.lastName}`}</p>
        <p className="text-muted-foreground mt-1 text-xs">
          Moved in: {movedInAt && new Date(movedInAt).toLocaleDateString()}
        </p>
      </div>
    </div>
  );
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
            <div key={place.id} className="rounded-lg border p-4">
              <div className="mb-2 flex items-center justify-between">
                <div className="flex items-center">
                  <Bed className="text-primary mr-2 h-5 w-5" />
                  <h3 className="font-medium">Place {place.index}</h3>
                </div>
                <Badge
                  className={
                    place.isOccupied
                      ? "bg-black text-white" 
                      : "bg-green-100 text-green-800"
                  }
                  variant={place.isOccupied ? "secondary" : "outline"}
                >
                  {place.isOccupied ? "Occupied" : "Available"}
                </Badge>
              </div>
              {place.isOccupied && place.occupiedByUserId ? (
                <PlaceOccupant
                  userId={place.occupiedByUserId}
                  movedInAt={place.movedInAt}
                />
              ) : (
                <div className="text-muted-foreground flex items-center">
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
