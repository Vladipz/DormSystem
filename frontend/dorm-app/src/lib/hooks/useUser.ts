import { axiosClient } from "@/lib/utils/axios-client";
import { useQuery } from "@tanstack/react-query";

export interface UserDetails {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  roles: string[];
  avatarUrl: string;
}

const API_URL = `${import.meta.env.VITE_USERS_API_URL ?? "http://localhost:5137/api/users"}`;

async function getUserDetails(userId: string): Promise<UserDetails> {
  const { data } = await axiosClient.get<UserDetails>(`${API_URL}/${userId}`);
  return data;
}

export function useUser(userId: string | undefined) {
  return useQuery({
    queryKey: ["user", userId],
    queryFn: () => getUserDetails(userId!),
    enabled: !!userId,
  });
} 