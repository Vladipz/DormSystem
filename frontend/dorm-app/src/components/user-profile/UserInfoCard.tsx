import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";

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
}

export function UserInfoCard({ user, bio }: UserInfoCardProps) {
  return (
    <Card>
      <CardContent className="p-6">
        <div className="flex flex-col items-center gap-6 md:flex-row md:items-start">
          <Avatar className="h-24 w-24">
            <AvatarImage src={user.avatar} alt={user.name} />
            <AvatarFallback>
              {user.name
                .split(" ")
                .map((n) => n[0])
                .join("")}
            </AvatarFallback>
          </Avatar>
          <div className="flex-1 space-y-2 text-center md:text-left">
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
        </div>
      </CardContent>
    </Card>
  );
}
