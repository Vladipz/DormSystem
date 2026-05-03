import { Badge, Button, Card, CardContent, CardHeader, CardTitle, Skeleton } from "@/components/ui";
import { useAuth } from "@/lib/hooks/useAuth";
import { useEvents } from "@/lib/hooks/useEvents";
import { useListInspections } from "@/lib/hooks/useInspections";
import { createFileRoute, Link } from "@tanstack/react-router";
import { format, isPast, isToday, isTomorrow } from "date-fns";
import {
  Activity,
  ArrowRight,
  Building,
  Calendar,
  ClipboardCheck,
  Home,
  Plus,
  Settings,
  Users,
} from "lucide-react";

export const Route = createFileRoute("/_mainLayout/")({
  component: RouteComponent,
});

function RouteComponent() {
  const { user, userRole, isAuthenticated } = useAuth();
  const isAdmin = userRole === "Admin";

  // Fetch recent events (limit to 3 for home page)
  const { events, loading: eventsLoading } = useEvents({
    pageNumber: 1,
    pageSize: 3,
  });

  // Fetch recent inspections (limit to 3 for home page)
  const { data: inspectionsData, isLoading: inspectionsLoading } =
    useListInspections({
      pageNumber: 1,
      pageSize: 3,
    });

  const inspections = inspectionsData?.items ?? [];

  const getGreeting = () => {
    const hour = new Date().getHours();
    if (hour < 12) return "Good morning";
    if (hour < 18) return "Good afternoon";
    return "Good evening";
  };

  const getEventDateDisplay = (dateString: string) => {
    const date = new Date(dateString);
    if (isToday(date)) return "Today";
    if (isTomorrow(date)) return "Tomorrow";
    if (isPast(date)) return "Past";
    return format(date, "MMM d");
  };

  const getInspectionStatus = (status: string) => {
    switch (status) {
      case "Scheduled":
        return (
          <Badge
            variant="outline"
            className="border-blue-200 bg-blue-50 text-blue-700"
          >
            Scheduled
          </Badge>
        );
      case "Active":
        return (
          <Badge
            variant="outline"
            className="border-green-200 bg-green-50 text-green-700"
          >
            Active
          </Badge>
        );
      case "Completed":
        return (
          <Badge
            variant="outline"
            className="border-gray-200 bg-gray-50 text-gray-700"
          >
            Completed
          </Badge>
        );
      default:
        return <Badge variant="outline">{status}</Badge>;
    }
  };

  return (
    <div className="space-y-6 p-6">
      {/* Welcome Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            {getGreeting()}
            {isAuthenticated && user ? `, ${user.firstName}` : ""}!
          </h1>
          <p className="text-muted-foreground">
            Welcome to your dorm management dashboard
          </p>
        </div>
        <div className="flex items-center space-x-2">
          <Button variant="outline" size="icon" asChild>
            <Link to="/profile">
              <Settings className="h-4 w-4" />
            </Link>
          </Button>
        </div>
      </div>

      {/* Quick Stats Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active Events</CardTitle>
            <Calendar className="text-muted-foreground h-4 w-4" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {eventsLoading ? (
                <Skeleton className="h-8 w-12" />
              ) : (
                events.length
              )}
            </div>
            <p className="text-muted-foreground text-xs">Events this week</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Inspections</CardTitle>
            <ClipboardCheck className="text-muted-foreground h-4 w-4" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {inspectionsLoading ? (
                <Skeleton className="h-8 w-12" />
              ) : (
                inspections.length
              )}
            </div>
            <p className="text-muted-foreground text-xs">Recent inspections</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Room Service</CardTitle>
            <Building className="text-muted-foreground h-4 w-4" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">24/7</div>
            <p className="text-muted-foreground text-xs">Available services</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Your Role</CardTitle>
            <Users className="text-muted-foreground h-4 w-4" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{userRole || "Guest"}</div>
            <p className="text-muted-foreground text-xs">Access level</p>
          </CardContent>
        </Card>
      </div>

      {/* Main Content Grid */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        {/* Recent Events */}
        <Card className="col-span-1">
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <Calendar className="h-5 w-5" />
              Recent Events
            </CardTitle>
            <Button variant="ghost" size="sm" asChild>
              <Link to="/events">
                View all <ArrowRight className="ml-1 h-4 w-4" />
              </Link>
            </Button>
          </CardHeader>
          <CardContent className="space-y-3">
            {eventsLoading ? (
              <div className="space-y-3">
                <Skeleton className="h-16 w-full" />
                <Skeleton className="h-16 w-full" />
                <Skeleton className="h-16 w-full" />
              </div>
            ) : events.length > 0 ? (
              events.map((event) => (
                <div
                  key={event.id}
                  className="flex items-center justify-between rounded-lg border p-3"
                >
                  <div className="flex-1">
                    <h4 className="text-sm font-medium">{event.name}</h4>
                    <p className="text-muted-foreground text-xs">
                      {getEventDateDisplay(event.date)} •{" "}
                      {event.location || "Inhouse"}
                    </p>
                  </div>
                  <Badge variant="outline" className="ml-2">
                    {event.lastParticipants.length}
                  </Badge>
                </div>
              ))
            ) : (
              <div className="text-muted-foreground py-8 text-center">
                <Calendar className="mx-auto mb-2 h-8 w-8 opacity-50" />
                <p className="text-sm">No recent events</p>
              </div>
            )}

            {isAuthenticated && (
              <Button className="w-full" variant="outline" asChild>
                <Link to="/events/create">
                  <Plus className="mr-2 h-4 w-4" />
                  Create Event
                </Link>
              </Button>
            )}
          </CardContent>
        </Card>

        {/* Inspections Status */}
        <Card className="col-span-1">
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <ClipboardCheck className="h-5 w-5" />
              Inspections
            </CardTitle>
            <Button variant="ghost" size="sm" asChild>
              <Link to="/inspections">
                View all <ArrowRight className="ml-1 h-4 w-4" />
              </Link>
            </Button>
          </CardHeader>
          <CardContent className="space-y-3">
            {inspectionsLoading ? (
              <div className="space-y-3">
                <Skeleton className="h-16 w-full" />
                <Skeleton className="h-16 w-full" />
                <Skeleton className="h-16 w-full" />
              </div>
            ) : inspections.length > 0 ? (
              inspections.map((inspection) => (
                <div
                  key={inspection.id}
                  className="flex items-center justify-between rounded-lg border p-3"
                >
                  <div className="flex-1">
                    <h4 className="text-sm font-medium">{inspection.name}</h4>
                    <p className="text-muted-foreground text-xs">
                      {format(new Date(inspection.startDate), "MMM d, yyyy")} •{" "}
                      {inspection.type}
                    </p>
                  </div>
                  <div className="ml-2">
                    {getInspectionStatus(inspection.status)}
                  </div>
                </div>
              ))
            ) : (
              <div className="text-muted-foreground py-8 text-center">
                <ClipboardCheck className="mx-auto mb-2 h-8 w-8 opacity-50" />
                <p className="text-sm">No recent inspections</p>
              </div>
            )}

            {isAdmin && (
              <Button className="w-full" variant="outline" asChild>
                <Link to="/inspections/create">
                  <Plus className="mr-2 h-4 w-4" />
                  Create Inspection
                </Link>
              </Button>
            )}
          </CardContent>
        </Card>

        <Card className="col-span-1">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Activity className="h-5 w-5" />
              Quick Actions
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <Button className="w-full justify-start" variant="outline" asChild>
              <Link to="/room-dashboard">
                <Building className="mr-2 h-4 w-4" />
                Room Dashboard
              </Link>
            </Button>

            <Button className="w-full justify-start" variant="outline" asChild>
              <Link to="/events">
                <Calendar className="mr-2 h-4 w-4" />
                Browse Events
              </Link>
            </Button>

            <Button className="w-full justify-start" variant="outline" asChild>
              <Link to="/profile">
                <Users className="mr-2 h-4 w-4" />
                View Profile
              </Link>
            </Button>

            {isAdmin && (
              <Button
                className="w-full justify-start"
                variant="outline"
                asChild
              >
                <Link to="/inspections">
                  <ClipboardCheck className="mr-2 h-4 w-4" />
                  Manage Inspections
                </Link>
              </Button>
            )}

            {isAdmin && (
              <Button
                className="w-full justify-start"
                variant="outline"
                asChild
              >
                <Link to="/admin">
                  <Users className="mr-2 h-4 w-4" />
                  Admin Panel
                </Link>
              </Button>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Welcome Message for New Users */}
      {!isAuthenticated && (
        <Card className="border-blue-200 bg-gradient-to-r from-blue-50 to-indigo-50 dark:border-blue-400/30 dark:from-slate-900 dark:via-blue-950/40 dark:to-slate-900">
          <CardContent className="p-6">
            <div className="flex items-center space-x-4">
              <div className="rounded-full bg-blue-100 p-3 dark:bg-blue-500/20">
                <Home className="h-6 w-6 text-blue-600 dark:text-blue-300" />
              </div>
              <div className="flex-1">
                <h3 className="font-semibold text-blue-900 dark:text-blue-100">
                  Welcome to Dorm Management System
                </h3>
                <p className="mt-1 text-sm text-blue-700 dark:text-blue-200/90">
                  Manage your dormitory life with ease. Access events, room
                  services, inspections, and more.
                </p>
              </div>
              <div className="space-x-2">
                <Button asChild>
                  <Link to="/login" search={{ returnTo: "/" }}>
                    Sign In
                  </Link>
                </Button>
                <Button variant="outline" asChild>
                  <Link to="/register">Register</Link>
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
