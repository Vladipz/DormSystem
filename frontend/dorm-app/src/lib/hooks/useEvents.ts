import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { EventService } from "../services/eventService";

export const useEvents = () => {
  const queryClient = useQueryClient();

  const {
    data: events = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ["events"],
    queryFn: async () => {
      try {
        console.log("Fetching events...");
        const events = await EventService.getAllEvents();
        console.log("Events fetched successfully:", events);
        return events;
      } catch (error) {
        console.error("Error fetching events:", error);
        return []; // Always return a defined value (empty array) in case of error
      }
    },
  });

  const joinEventMutation = useMutation({
    mutationFn: ({ eventId, userId }: { eventId: string; userId: string }) =>
      EventService.joinEvent(eventId, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
  });

  return {
    events,
    loading: isLoading,
    error,
    refreshEvents: () => {
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
    joinEvent: (eventId: string, userId: string) =>
      joinEventMutation.mutate({ eventId, userId }),
  };
};
