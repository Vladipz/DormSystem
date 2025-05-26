import { UserProfile } from "@/components/UserProfile";
import { useAuth } from "@/lib/hooks/useAuth";
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
  const navigate = useNavigate();

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

  if (authLoading || userLoading || !user) return <div>Завантаження…</div>;

  const userProfileProps = mapUserDetailsToUserProfileProps(user);
  return <UserProfile user={userProfileProps.user} userId={userId} />;
}
