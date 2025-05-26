import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { notificationService } from "../services/notificationService";
import { UpdateNotificationPreferencesRequest } from "../types/notification";

/* Cache key helper */
const prefKey = (userId: string) => ["notificationPreferences", userId];

/* 1. read hook */
export function useNotificationPreferences(userId: string | undefined) {
  return useQuery({
    queryKey: prefKey(userId ?? ""),
    queryFn: () => {
      if (!userId) throw new Error("userId required");
      return notificationService.getPreferences(userId);
    },
    enabled: !!userId, // don't run until we have userId
  });
}

/* 2. update hook (optimistic update included) */
export function useUpdateNotificationPreferences(userId: string) {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: (payload: UpdateNotificationPreferencesRequest) =>
      notificationService.updatePreferences(payload),
    onSuccess: () => {
      // refetch so UI shows fresh data
      qc.invalidateQueries({ queryKey: prefKey(userId) });
    },
  });
}
