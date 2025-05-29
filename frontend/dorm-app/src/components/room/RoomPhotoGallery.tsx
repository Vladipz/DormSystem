import {
  Button,
  Dialog,
  DialogContent,
  DialogTrigger,
} from "@/components/ui";
import { ChevronLeft, ChevronRight, X } from "lucide-react";
import { useState } from "react";

interface RoomPhotoGalleryProps {
  photos: string[];
  roomLabel: string;
  initialIndex?: number;
}

export function RoomPhotoGallery({ photos, roomLabel, initialIndex = 0 }: RoomPhotoGalleryProps) {
  const [currentIndex, setCurrentIndex] = useState(initialIndex);
  const [open, setOpen] = useState(false);

  if (photos.length === 0) {
    return null;
  }

  const goToPrevious = () => {
    setCurrentIndex((prevIndex) => 
      prevIndex === 0 ? photos.length - 1 : prevIndex - 1
    );
  };

  const goToNext = () => {
    setCurrentIndex((prevIndex) => 
      prevIndex === photos.length - 1 ? 0 : prevIndex + 1
    );
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <button className="cursor-pointer">
          <img
            src={photos[0]}
            alt={`Room ${roomLabel}`}
            className="h-full w-full object-cover hover:opacity-90 transition-opacity"
            onError={(e) => {
              const target = e.target as HTMLImageElement;
              target.src = "/placeholder.svg?height=300&width=500";
            }}
          />
        </button>
      </DialogTrigger>
      <DialogContent className="max-w-4xl p-0">
        <div className="relative">
          {/* Close button */}
          <Button
            variant="ghost"
            size="icon"
            className="absolute top-4 right-4 z-10 bg-black/50 text-white hover:bg-black/70"
            onClick={() => setOpen(false)}
          >
            <X className="h-4 w-4" />
          </Button>

          {/* Main image */}
          <div className="relative">
            <img
              src={photos[currentIndex]}
              alt={`Room ${roomLabel} - Photo ${currentIndex + 1}`}
              className="w-full h-auto max-h-[80vh] object-contain"
              onError={(e) => {
                const target = e.target as HTMLImageElement;
                target.src = "/placeholder.svg?height=600&width=800";
              }}
            />

            {/* Navigation arrows */}
            {photos.length > 1 && (
              <>
                <Button
                  variant="ghost"
                  size="icon"
                  className="absolute left-4 top-1/2 -translate-y-1/2 bg-black/50 text-white hover:bg-black/70"
                  onClick={goToPrevious}
                >
                  <ChevronLeft className="h-6 w-6" />
                </Button>
                <Button
                  variant="ghost"
                  size="icon"
                  className="absolute right-4 top-1/2 -translate-y-1/2 bg-black/50 text-white hover:bg-black/70"
                  onClick={goToNext}
                >
                  <ChevronRight className="h-6 w-6" />
                </Button>
              </>
            )}
          </div>

          {/* Thumbnail strip */}
          {photos.length > 1 && (
            <div className="p-4 bg-black/5">
              <div className="flex gap-2 justify-center overflow-x-auto">
                {photos.map((photo, index) => (
                  <button
                    key={index}
                    onClick={() => setCurrentIndex(index)}
                    className={`flex-shrink-0 ${
                      index === currentIndex
                        ? "ring-2 ring-primary"
                        : "opacity-70 hover:opacity-100"
                    }`}
                  >
                    <img
                      src={photo}
                      alt={`Thumbnail ${index + 1}`}
                      className="h-16 w-16 object-cover rounded"
                      onError={(e) => {
                        const target = e.target as HTMLImageElement;
                        target.src = "/placeholder.svg?height=64&width=64";
                      }}
                    />
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Photo counter */}
          <div className="absolute bottom-4 left-1/2 -translate-x-1/2 bg-black/50 text-white px-3 py-1 rounded-full text-sm">
            {currentIndex + 1} / {photos.length}
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
} 