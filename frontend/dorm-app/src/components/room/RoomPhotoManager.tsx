import {
  Button,
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  Input,
  Label,
} from "@/components/ui";
import { useDeleteRoomPhoto, useUploadRoomPhoto } from "@/lib/hooks/useRooms";
import { RoomDetailsResponse } from "@/lib/types/room";
import { Image as ImageIcon, Upload, X } from "lucide-react";
import { useState } from "react";

interface RoomPhotoManagerProps {
  room: RoomDetailsResponse;
}

export function RoomPhotoManager({ room }: RoomPhotoManagerProps) {
  const [dragOver, setDragOver] = useState(false);
  const uploadPhoto = useUploadRoomPhoto();
  const deletePhoto = useDeleteRoomPhoto();

  const handleFileSelect = (files: FileList | null) => {
    if (!files || files.length === 0) return;

    const file = files[0];
    
    // Validate file type
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      alert('Please select a valid image file (JPEG, PNG, GIF, WebP)');
      return;
    }

    // Validate file size (10MB)
    const maxSize = 10 * 1024 * 1024;
    if (file.size > maxSize) {
      alert('File size must be less than 10MB');
      return;
    }

    uploadPhoto.mutate({ roomId: room.id, photo: file });
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
    handleFileSelect(e.dataTransfer.files);
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(true);
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
  };

  const handleDeletePhoto = (photoUrl: string) => {
    // Extract photo ID from URL
    const photoId = photoUrl.split('/').pop() || '';
    if (photoId) {
      deletePhoto.mutate({ roomId: room.id, photoId });
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <ImageIcon className="h-5 w-5" />
          Room Photos
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Photo Grid */}
        {room.photoUrls && room.photoUrls.length > 0 && (
          <div className="grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-4">
            {room.photoUrls.map((photoUrl, index) => (
              <div key={index} className="relative group">
                <div className="aspect-square overflow-hidden rounded-lg border">
                  <img
                    src={photoUrl}
                    alt={`Room ${room.label} - Photo ${index + 1}`}
                    className="h-full w-full object-cover"
                    onError={(e) => {
                      const target = e.target as HTMLImageElement;
                      target.src = "/placeholder.svg?height=200&width=200";
                    }}
                  />
                </div>
                <Button
                  variant="destructive"
                  size="icon"
                  className="absolute -top-2 -right-2 h-6 w-6 rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
                  onClick={() => handleDeletePhoto(photoUrl)}
                  disabled={deletePhoto.isPending}
                >
                  <X className="h-3 w-3" />
                </Button>
              </div>
            ))}
          </div>
        )}

        {/* Upload Area */}
        <div
          className={`border-2 border-dashed rounded-lg p-6 text-center transition-colors ${
            dragOver
              ? "border-primary bg-primary/5"
              : "border-muted-foreground/25 hover:border-primary/50"
          }`}
          onDrop={handleDrop}
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
        >
          <div className="flex flex-col items-center gap-3">
            <Upload className="h-8 w-8 text-muted-foreground" />
            <div>
              <Label htmlFor="photo-upload" className="cursor-pointer">
                <span className="text-primary hover:underline">
                  Click to upload
                </span>{" "}
                or drag and drop
              </Label>
              <p className="text-sm text-muted-foreground mt-1">
                PNG, JPG, GIF, WebP up to 10MB
              </p>
            </div>
          </div>
          <Input
            id="photo-upload"
            type="file"
            accept="image/*"
            className="hidden"
            onChange={(e) => handleFileSelect(e.target.files)}
            disabled={uploadPhoto.isPending}
          />
        </div>

        {/* Upload Status */}
        {uploadPhoto.isPending && (
          <div className="flex items-center justify-center p-4 bg-blue-50 dark:bg-blue-950 rounded-lg">
            <div className="text-sm text-blue-700 dark:text-blue-300">
              Uploading photo...
            </div>
          </div>
        )}

        {(room.photoUrls?.length || 0) === 0 && !uploadPhoto.isPending && (
          <div className="text-center py-8 text-muted-foreground">
            <ImageIcon className="h-12 w-12 mx-auto mb-3 opacity-50" />
            <p>No photos uploaded yet</p>
            <p className="text-sm">Upload the first photo to get started</p>
          </div>
        )}
      </CardContent>
    </Card>
  );
} 