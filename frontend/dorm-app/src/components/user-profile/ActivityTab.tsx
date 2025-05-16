import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Calendar, ShoppingCart } from "lucide-react";

interface Activity {
  id: number;
  type: string;
  title: string;
  date: string;
}

interface ActivityTabProps {
  activities: Activity[];
}

export function ActivityTab({ activities }: ActivityTabProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Activity History</CardTitle>
        <CardDescription>
          Your recent activity in the dormitory
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {activities.map((activity) => (
            <div
              key={activity.id}
              className="flex items-start gap-4 rounded-lg border p-4"
            >
              <div className="bg-primary/10 rounded-full p-2">
                {activity.type === "Event" && (
                  <Calendar className="text-primary h-5 w-5" />
                )}
                {(activity.type === "Booking" || activity.type === "Laundry") && (
                  <Calendar className="text-primary h-5 w-5" />
                )}
                {(activity.type === "Purchase Request" || activity.type === "Marketplace") && (
                  <ShoppingCart className="text-primary h-5 w-5" />
                )}
              </div>
              <div className="flex-1">
                <h3 className="font-medium">{activity.title}</h3>
                <p className="text-muted-foreground text-xs">
                  {activity.date}
                </p>
              </div>
              <Badge variant="outline">{activity.type}</Badge>
            </div>
          ))}
        </div>
      </CardContent>
      <CardFooter>
        <Button variant="outline" className="w-full">
          View Full Activity History
        </Button>
      </CardFooter>
    </Card>
  );
} 