import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Award } from "lucide-react";

interface Achievement {
  id: number;
  title: string;
  description: string;
  date: string;
}

interface AchievementsTabProps {
  achievements: Achievement[];
}

export function AchievementsTab({ achievements }: AchievementsTabProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Achievements</CardTitle>
        <CardDescription>
          Your dormitory achievements and rewards
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {achievements.map((achievement) => (
            <div
              key={achievement.id}
              className="flex items-start gap-4 rounded-lg border p-4"
            >
              <div className="bg-primary/10 rounded-full p-2">
                <Award className="text-primary h-6 w-6" />
              </div>
              <div className="flex-1">
                <h3 className="font-medium">{achievement.title}</h3>
                <p className="text-muted-foreground text-sm">
                  {achievement.description}
                </p>
                <p className="text-muted-foreground mt-1 text-xs">
                  Achieved on {achievement.date}
                </p>
              </div>
              <Badge variant="outline">+20 points</Badge>
            </div>
          ))}
        </div>
      </CardContent>
      <CardFooter>
        <Button variant="outline" className="w-full">
          View All Achievements
        </Button>
      </CardFooter>
    </Card>
  );
} 