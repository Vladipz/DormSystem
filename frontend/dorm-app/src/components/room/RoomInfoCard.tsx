import {
  Badge,
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  Skeleton,
} from "@/components/ui";
import { useBlockById } from "@/lib/hooks/useBlocks";
import { RoomDetailsResponse } from "@/lib/types/room";
import { getGenderRuleColor } from "@/lib/utils/genreRuleUtils";

interface RoomInfoCardProps {
  room: RoomDetailsResponse;
}

const roomPhotos = [
  {
    id: 1,
    url: "/placeholder.svg?height=300&width=400",
    caption: "Room overview",
  },
  {
    id: 2,
    url: "/placeholder.svg?height=300&width=400",
    caption: "Window view",
  },
  { id: 3, url: "/placeholder.svg?height=300&width=400", caption: "Bathroom" },
];

export function RoomInfoCard({ room }: RoomInfoCardProps) {
  const { data: block, isLoading: isBlockLoading } = useBlockById(
    room.block?.id || "",
  );

  return (
    <Card>
      <CardHeader>
        <CardTitle>Room Information</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid gap-6 md:grid-cols-2">
          <div>
            <div className="mb-4 aspect-video overflow-hidden rounded-md">
              <img
                src="/placeholder.svg?height=300&width=500"
                alt={`Room ${room.label}`}
                className="h-full w-full object-cover"
              />
            </div>
            <div className="grid grid-cols-3 gap-2">
              {roomPhotos.map((photo) => (
                <div
                  key={photo.id}
                  className="aspect-square overflow-hidden rounded-md"
                >
                  <img
                    src={photo.url || "/placeholder.svg"}
                    alt={photo.caption}
                    className="h-full w-full object-cover"
                  />
                </div>
              ))}
            </div>
          </div>
          <div className="space-y-4">
            <div>
              <h3 className="text-muted-foreground mb-1 text-sm font-medium">
                Location
              </h3>
              <div className="space-y-1">
                <div className="flex items-center gap-1">
                  <p className="font-medium">{room.building.label}</p>
                  {room.block?.label && (
                    <>
                      <span className="text-muted-foreground">&middot;</span>
                      <p className="font-medium">Block {room.block.label}</p>
                    </>
                  )}
                </div>
                <p className="text-muted-foreground text-sm">
                  Floor {room.floor}
                </p>
              </div>
            </div>
            <div>
              <h3 className="text-muted-foreground mb-1 text-sm font-medium">
                Room Type
              </h3>
              <p className="font-medium capitalize">{room.roomType}</p>
            </div>
            <div>
              <h3 className="text-muted-foreground mb-1 text-sm font-medium">
                Capacity
              </h3>
              <p className="font-medium">{room.capacity} places</p>
            </div>
            {isBlockLoading ? (
              <div>
                <h3 className="text-muted-foreground mb-1 text-sm font-medium">
                  Gender Rule
                </h3>
                <Skeleton className="h-6 w-24 rounded-md" />
              </div>
            ) : block?.genderRule ? (
              <div>
                <h3 className="text-muted-foreground mb-1 text-sm font-medium">
                  Gender Rule
                </h3>
                <Badge
                  className={getGenderRuleColor(block.genderRule)}
                  variant="outline"
                >
                  {block.genderRule}
                </Badge>
              </div>
            ) : null}
            <div>
              <h3 className="text-muted-foreground mb-1 text-sm font-medium">
                Amenities
              </h3>
              <div className="flex flex-wrap gap-2">
                {room.amenities?.map((amenity) => (
                  <Badge key={amenity} variant="outline">
                    {amenity}
                  </Badge>
                ))}
              </div>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
