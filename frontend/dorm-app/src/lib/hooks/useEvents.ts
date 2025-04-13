import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { EventService } from "../services/eventService";
import { CreateEventRequest } from "../types/event";

export const useEvents = () => {
  const queryClient = useQueryClient();

  const {
    data: events = [],
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: ["events"],
    queryFn: async () => {
      console.log("Fetching events...");
      const events = await EventService.getAllEvents();
      console.log("Events fetched successfully:", events);
      return events;
    },
    retry: 1,
    retryDelay: 1000,
  });

  const joinEventMutation = useMutation({
    mutationFn: ({ eventId, userId }: { eventId: string; userId: string }) =>
      EventService.joinEvent(eventId, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
  });

  const createEventMutation = useMutation({
    mutationFn: (eventData: CreateEventRequest) => 
      EventService.createEvent(eventData),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
  });

  const updateEventMutation = useMutation({
    mutationFn: ({ eventId, eventData }: { eventId: string; eventData: CreateEventRequest }) => 
      EventService.updateEvent(eventId, eventData),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
  });

  return {
    events,
    loading: isLoading,
    error,
    refreshEvents: () => refetch(),
    joinEvent: (eventId: string, userId: string) =>
      joinEventMutation.mutate({ eventId, userId }),
    createEvent: (eventData: CreateEventRequest) => 
      createEventMutation.mutate(eventData),
    updateEvent: (eventId: string, eventData: CreateEventRequest) =>
      updateEventMutation.mutate({ eventId, eventData }),
    createEventMutation, // Export the mutation for more granular access to status
    updateEventMutation, // Export the update mutation for access to status
  };
};
