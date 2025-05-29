import { RoomPhotoGallery } from "@/components/room/RoomPhotoGallery";
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

export function RoomInfoCard({ room }: RoomInfoCardProps) {
  const { data: block, isLoading: isBlockLoading } = useBlockById(
    room.block?.id || "",
  );

  const hasPhotos = room.photoUrls && room.photoUrls.length > 0;
  const thumbnailPhotos = hasPhotos ? room.photoUrls.slice(1, 4) : [];
  console.log("thumbnailPhotos", thumbnailPhotos);
  console.log("room.photoUrls", room.photoUrls);
  return (
    <Card>
      <CardHeader>
        <CardTitle>Room Information</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid gap-6 md:grid-cols-2">
          <div>
            <div className="mb-4 aspect-video overflow-hidden rounded-md">
              {hasPhotos ? (
                <RoomPhotoGallery
                  photos={room.photoUrls}
                  roomLabel={room.label}
                />
              ) : (
                <img
                  src="/placeholder.svg?height=300&width=500"
                  alt={`Room ${room.label}`}
                  className="h-full w-full object-cover"
                />
              )}
            </div>
            {hasPhotos && thumbnailPhotos.length > 0 && (
              <div className="grid grid-cols-3 gap-2">
                {thumbnailPhotos.map((photo, index) => (
                  <div
                    key={index}
                    className="aspect-square cursor-pointer overflow-hidden rounded-md"
                  >
                    <img
                      src={photo} // Використовуємо саме фото з масиву, а не photos[0]
                      alt={`Room ${room.label} - Thumbnail ${index + 2}`}
                      className="h-full w-full object-cover transition-opacity hover:opacity-90"
                      onClick={() => {
                        // Відкриваємо галерею з відповідним індексом
                        // Це потребує додаткової логіки
                      }}
                      onError={(e) => {
                        const target = e.target as HTMLImageElement;
                        target.src = "/placeholder.svg?height=100&width=100";
                      }}
                    />
                  </div>
                ))}
              </div>
            )}
            {!hasPhotos && (
              <div className="text-muted-foreground py-8 text-center">
                <p>No photos available for this room</p>
              </div>
            )}
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
