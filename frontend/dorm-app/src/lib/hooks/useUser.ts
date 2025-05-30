import { axiosClient } from "@/lib/utils/axios-client";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

export interface UserDetails {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  roles: string[];
  avatarUrl: string;
  points?: number;
  faculty?: string;
  year?: number;
}

export interface PagedUsersResponse {
  items: UserDetails[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface UsersQueryParams {
  pageNumber?: number;
  pageSize?: number;
}

export interface UpdateUserRoleRequest {
  userId: string;
  newRole: string;
  currentRole?: string;
}

const API_URL = `${import.meta.env.VITE_USERS_API_URL ?? "http://localhost:5137/api/user"}`;

async function getUserDetails(userId: string): Promise<UserDetails> {
  const { data } = await axiosClient.get<UserDetails>(`${API_URL}/${userId}`);
  return data;
}

async function getUsers(params: UsersQueryParams = {}): Promise<PagedUsersResponse> {
  const { pageNumber = 1, pageSize = 10 } = params;
  const searchParams = new URLSearchParams({
    pageNumber: pageNumber.toString(),
    pageSize: pageSize.toString(),
  });
  
  const { data } = await axiosClient.get<PagedUsersResponse>(`${API_URL}?${searchParams}`);
  return data;
}

async function updateUserRole(request: UpdateUserRoleRequest): Promise<void> {
  const { userId, newRole, currentRole } = request;
  
  // Only remove current role if it exists and is different from the new role
  if (currentRole && currentRole.trim() !== '' && currentRole !== newRole) {
    await axiosClient.delete(`${API_URL}/${userId}/roles/${currentRole}`);
  }
  
  // Add the new role
  await axiosClient.post(`${API_URL}/${userId}/roles/${newRole}`);
}

export function useUser(userId: string | undefined) {
  return useQuery({
    queryKey: ["user", userId],
    queryFn: () => getUserDetails(userId!),
    enabled: !!userId,
  });
}

export function useUsers(params: UsersQueryParams = {}) {
  return useQuery({
    queryKey: ["users", params],
    queryFn: () => getUsers(params),
  });
}

export function useUpdateUserRole() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: updateUserRole,
    onSuccess: () => {
      // Invalidate users query to refetch the updated data
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
  });
} 