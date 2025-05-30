import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useAuth } from "@/lib/hooks/useAuth";
import { usePageTitle } from "@/lib/hooks/usePageTitle";
import { userService } from "@/lib/services/userService";
import { UserProfile } from "@/lib/types/auth";
import { useQuery } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import { LogOut, User } from "lucide-react";
import { useState } from "react";

export function Navbar() {
  const { pageTitle } = usePageTitle();
  const { user, isAuthenticated, isLoading: authLoading, logout } = useAuth();
  const [error, setError] = useState<string | null>(null);

  // Fetch full user profile when authenticated
  const { data: userProfile, isLoading: profileLoading } = useQuery<UserProfile>({
    queryKey: ["userProfile"],
    queryFn: () => userService.getCurrentUserProfile(),
    enabled: isAuthenticated && !!user,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  const handleLogout = async () => {
    try {
      await logout();
    } catch {
      setError("Failed to logout. Please try again.");
      // Auto-clear error after 3 seconds
      setTimeout(() => setError(null), 3000);
    }
  };

  // Create user display name from auth user or profile
  const displayName = userProfile 
    ? `${userProfile.firstName || ""} ${userProfile.lastName || ""}`.trim() || "User"
    : user 
    ? `${user.firstName || ""} ${user.lastName || ""}`.trim() || "User"
    : "User";

  // Create initials for avatar fallback
  const initials = userProfile
    ? `${userProfile.firstName?.[0] || ""}${userProfile.lastName?.[0] || ""}`.toUpperCase() || "U"
    : user
    ? `${user.firstName?.[0] || ""}${user.lastName?.[0] || ""}`.toUpperCase() || "U"
    : "U";

  // Get avatar URL from profile
  const avatarUrl = userProfile?.avatarUrl;

  // Get user role from auth user or profile
  const userRole = userProfile?.roles?.[0] || user?.role || "User";

  // Get user email from profile
  const userEmail = userProfile?.email;

  const isLoading = authLoading || profileLoading;

  return (
    <header className="bg-background z-50 w-full border-b">
      <div className="container mx-auto flex items-center justify-between px-4 py-3">
        <h2 className="text-2xl font-bold">{pageTitle}</h2>
        
        <div className="flex items-center gap-4">
          {error && (
            <span className="self-center text-sm text-red-500">{error}</span>
          )}

          {isLoading ? (
            <div className="flex items-center gap-2">
              <div className="h-8 w-8 animate-pulse rounded-full bg-muted"></div>
              <span className="text-sm text-muted-foreground">Loading...</span>
            </div>
          ) : isAuthenticated && user ? (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="relative h-auto p-2">
                  <div className="flex items-center gap-3">
                    <div className="text-right">
                      <div className="text-sm font-medium">{displayName}</div>
                      <div className="flex items-center gap-1">
                        <Badge variant="secondary" className="text-xs">
                          {userRole}
                        </Badge>
                      </div>
                    </div>
                    <Avatar className="h-8 w-8">
                      <AvatarImage 
                        src={avatarUrl} 
                        alt={displayName}
                      />
                      <AvatarFallback className="text-xs">
                        {initials}
                      </AvatarFallback>
                    </Avatar>
                  </div>
                </Button>
              </DropdownMenuTrigger>
              
              <DropdownMenuContent align="end" className="w-56">
                <DropdownMenuLabel>
                  <div className="flex flex-col space-y-1">
                    <p className="text-sm font-medium leading-none">{displayName}</p>
                    {userEmail && (
                      <p className="text-xs leading-none text-muted-foreground">
                        {userEmail}
                      </p>
                    )}
                  </div>
                </DropdownMenuLabel>
                
                <DropdownMenuSeparator />
                
                <DropdownMenuItem asChild>
                  <Link to="/profile" className="flex items-center">
                    <User className="mr-2 h-4 w-4" />
                    Profile
                  </Link>
                </DropdownMenuItem>
                
                <DropdownMenuSeparator />
                
                <DropdownMenuItem 
                  onClick={handleLogout} 
                  disabled={isLoading}
                  className="text-red-600 focus:text-red-600"
                >
                  <LogOut className="mr-2 h-4 w-4" />
                  {isLoading ? "Logging out..." : "Logout"}
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          ) : (
            <div className="flex gap-2">
              <Button variant="ghost" asChild>
                <Link to="/login">Login</Link>
              </Button>
              <Button asChild>
                <Link to="/register">Register</Link>
              </Button>
            </div>
          )}
        </div>
      </div>
    </header>
  );
}
