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