import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { userService } from "@/lib/services/userService";
import { Camera, Trash2, Upload } from "lucide-react";
import { useRef, useState } from "react";
import { toast } from "sonner";

interface UserInfoCardProps {
  user: {
    name: string;
    role: string;
    room: string;
    floor: string;
    building: string;
    points: number;
    avatar: string;
  };
  bio: string;
  onAvatarUpdate?: (newAvatarUrl: string) => void;
}

export function UserInfoCard({ user, bio, onAvatarUpdate }: UserInfoCardProps) {
  const [isUploading, setIsUploading] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [optimisticAvatar, setOptimisticAvatar] = useState<string | null>(null);

  // Використовуємо оптимістичний аватар якщо він є, інакше - з props
  const displayAvatar =
    optimisticAvatar !== null ? optimisticAvatar : user.avatar;
  console.log("optimisticAvatar", optimisticAvatar);
  console.log("displayAvatar", displayAvatar);
  console.log("user.avatar", user.avatar);

  const validateFile = (file: File): boolean => {
    const allowedTypes = [
      "image/jpeg",
      "image/jpg",
      "image/png",
      "image/gif",
      "image/webp",
    ];
    if (!allowedTypes.includes(file.type)) {
      toast.error("Invalid file type", {
        description: "Please select a JPEG, PNG, GIF, or WebP image.",
      });
      return false;
    }

    const maxSize = 5 * 1024 * 1024; // 5MB
    if (file.size > maxSize) {
      toast.error("File too large", {
        description: "Please select an image smaller than 5MB.",
      });
      return false;
    }

    return true;
  };

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file && validateFile(file)) {
      setSelectedFile(file);
    } else {
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
      setSelectedFile(null);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) return;

    setIsUploading(true);
    try {
      const response = await userService.uploadAvatar(selectedFile);

      // Оптимістично оновлюємо UI
      setOptimisticAvatar(response.avatarUrl);

      toast.success("Success", {
        description: response.message || "Avatar uploaded successfully!",
      });

      // Оновлюємо батьківський компонент
      onAvatarUpdate?.(response.avatarUrl);

      // Очищаємо вибраний файл
      setSelectedFile(null);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }

      // Скидаємо оптимістичне оновлення через деякий час
      setTimeout(() => {
        setOptimisticAvatar(null);
      }, 1000);
    } catch (error) {
      console.error("Upload error:", error);
      setOptimisticAvatar(null); // Скидаємо при помилці
      toast.error("Upload failed", {
        description:
          error instanceof Error ? error.message : "Failed to upload avatar",
      });
    } finally {
      setIsUploading(false);
    }
  };

  const handleDelete = async () => {
    setIsDeleting(true);
    try {
      await userService.deleteAvatar();

      // Оптимістично видаляємо аватар
      setOptimisticAvatar("");

      toast.success("Success", {
        description: "Avatar deleted successfully!",
      });

      // Оновлюємо батьківський компонент
      onAvatarUpdate?.("");

      // Скидаємо оптимістичне оновлення через деякий час
      setTimeout(() => {
        setOptimisticAvatar(null);
      }, 1000);
    } catch (error) {
      console.log("error", error);
      setOptimisticAvatar(null); // Скидаємо при помилці
      toast.error("Delete failed", {
        description:
          error instanceof Error ? error.message : "Failed to delete avatar",
      });
    } finally {
      setIsDeleting(false);
    }
  };

  const handleAvatarClick = () => {
    fileInputRef.current?.click();
  };

  return (
    <Card>
      <CardContent className="p-6">
        <div className="flex flex-col items-center gap-6 md:flex-row md:items-start">
          <div className="group relative">
            <Avatar
              className="h-24 w-24 cursor-pointer"
              onClick={handleAvatarClick}
            >
              <AvatarImage
                src={displayAvatar}
                alt={user.name}
                key={displayAvatar} // Форсуємо перезавантаження при зміні URL
              />
              <AvatarFallback>
                {user.name
                  .split(" ")
                  .map((n) => n[0])
                  .join("")}
              </AvatarFallback>
            </Avatar>

            <div
              className="bg-opacity-50 absolute inset-0 flex cursor-pointer items-center justify-center rounded-full bg-black opacity-0 transition-opacity group-hover:opacity-100"
              onClick={handleAvatarClick}
            >
              <Camera className="h-6 w-6 text-white" />
            </div>

            <Input
              ref={fileInputRef}
              type="file"
              accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
              onChange={handleFileSelect}
              className="hidden"
            />
          </div>

          <div className="flex-1 space-y-4 text-center md:text-left">
            <div className="space-y-2">
              <h2 className="text-2xl font-bold">{user.name}</h2>
              <div className="flex flex-wrap justify-center gap-2 md:justify-start">
                <Badge variant="secondary">{user.role}</Badge>
                <Badge variant="outline">
                  Room {user.room}, {user.building}
                </Badge>
                <Badge variant="outline" className="bg-primary/10 text-primary">
                  {user.points} Points
                </Badge>
              </div>
              <p className="text-muted-foreground">{bio}</p>
            </div>

            {selectedFile && (
              <div className="flex flex-col gap-2 md:flex-row">
                <div className="text-muted-foreground flex items-center gap-2 text-sm">
                  <span>Selected: {selectedFile.name}</span>
                </div>
                <div className="flex gap-2">
                  <Button
                    onClick={handleUpload}
                    disabled={isUploading}
                    size="sm"
                    className="flex items-center gap-2"
                  >
                    <Upload className="h-4 w-4" />
                    {isUploading ? "Uploading..." : "Upload"}
                  </Button>
                  <Button
                    onClick={() => {
                      setSelectedFile(null);
                      if (fileInputRef.current) {
                        fileInputRef.current.value = "";
                      }
                    }}
                    variant="outline"
                    size="sm"
                  >
                    Cancel
                  </Button>
                </div>
              </div>
            )}

            {displayAvatar && displayAvatar.trim() !== "" && !selectedFile && (
              <div className="flex gap-2">
                <Button
                  onClick={handleDelete}
                  disabled={isDeleting}
                  variant="outline"
                  size="sm"
                  className="text-destructive hover:text-destructive flex items-center gap-2"
                >
                  <Trash2 className="h-4 w-4" />
                  {isDeleting ? "Deleting..." : "Remove Avatar"}
                </Button>
              </div>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
