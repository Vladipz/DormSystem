import { UserProfile } from "@/components/UserProfile";
import { useAuth } from "@/lib/hooks/useAuth";
import { useUserAddress } from "@/lib/hooks/usePlaces";
import { useUser } from "@/lib/hooks/useUser";
import { mapUserDetailsToUserProfileProps } from "@/lib/mappers/userMappers";
import { authService } from "@/lib/services/authService";
import { createFileRoute, redirect, useNavigate } from "@tanstack/react-router";
import { useEffect } from "react";

export const Route = createFileRoute("/_mainLayout/profile/")({
  beforeLoad: async () => {
    const authStatus = await authService.checkAuthStatus();
    if (!authStatus || !authStatus.isAuthenticated) {
      throw redirect({ to: "/login", search: { returnTo: "/profile" } });
    }
  },
  component: RouteComponent,
});

function RouteComponent() {
  const { userId, isAuthenticated, isLoading: authLoading } = useAuth();
  const { data: user, isLoading: userLoading, error } = useUser(userId);
  const { data: userAddress, isLoading: addressLoading } = useUserAddress(userId);
  const navigate = useNavigate();
  console.log("userAddress", userAddress);

  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      navigate({ to: "/login", search: { returnTo: "/profile" } });
    }
  }, [authLoading, isAuthenticated, navigate]);

  if (error) {
    console.log(error);
    authService.logout();
    navigate({ to: "/login", search: { returnTo: "/profile" } });
    return null;
  }

  if (authLoading || userLoading || addressLoading || !user || !userId) {
    return <div>Loading...</div>;
  }

  console.log("user", user);
  console.log("userAddress", userAddress);
  
  // Enhanced user object with address information
  const enhancedUser = {
    ...user,
    dormInfo: userAddress ? {
      room: userAddress.roomLabel ?? "—",
      floor: userAddress.floorLabel ?? "—",
      building: userAddress.buildingName ?? "—",
      address: userAddress.buildingAddress ?? "—"
    } : undefined
  };
  
  const userProfileProps = mapUserDetailsToUserProfileProps(enhancedUser);
  return <UserProfile user={userProfileProps.user} userId={userId} />;
}
