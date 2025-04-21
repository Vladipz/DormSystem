import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { EventService } from "../services/eventService";
import { CreateEventRequest } from "../types/event";

interface UseEventsParams {
  pageNumber?: number;
  pageSize?: number;
}

export const useEvents = ({ pageNumber = 1, pageSize = 10 }: UseEventsParams = {}) => {
  const queryClient = useQueryClient();

  const {
    data,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: ["events", { pageNumber, pageSize }],
    queryFn: () => EventService.getAllEvents(pageNumber, pageSize),
    retry: (failureCount, error) => {
      if (error instanceof Error && error.message === 'Authentication required to fetch events') {
        return false;
      }
      return failureCount < 1;
    },
    retryDelay: 1000,
  });

  const joinEventMutation = useMutation({
    mutationFn: ({ eventId, userId }: { eventId: string; userId: string }) =>
      EventService.joinEvent(eventId, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
  });

  const leaveEventMutation = useMutation({
    mutationFn: ({ eventId, userId }: { eventId: string; userId: string }) =>
      EventService.leaveEvent(eventId, userId),
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
    mutationFn: ({
      eventId,
      eventData,
    }: {
      eventId: string;
      eventData: CreateEventRequest;
    }) => EventService.updateEvent(eventId, eventData),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
  });

  // Додаю методи для інвайт-логіки
  const validateInvitation = (eventId: string, token: string) =>
    EventService.validateInvitation(eventId, token);

  const joinEventWithToken = (eventId: string, token: string) =>
    EventService.joinEventWithToken(eventId, token);

  const getEventInviteLink = (eventId: string) =>
    EventService.getEventInviteLink(eventId);

  return {
    events: data?.items ?? [],
    totalPages: data?.totalPages ?? 0,
    currentPage: data?.pageNumber ?? pageNumber,
    totalCount: data?.totalCount ?? 0,
    loading: isLoading,
    error,
    refreshEvents: () => refetch(),
    joinEvent: (eventId: string, userId: string) =>
      joinEventMutation.mutate({ eventId, userId }),
    leaveEvent: (eventId: string, userId: string) =>
      leaveEventMutation.mutate({ eventId, userId }),
    createEvent: (eventData: CreateEventRequest) =>
      createEventMutation.mutate(eventData),
    updateEvent: (eventId: string, eventData: CreateEventRequest) =>
      updateEventMutation.mutate({ eventId, eventData }),
    createEventMutation,
    updateEventMutation,
    getEventInviteLink,
    validateInvitation,
    joinEventWithToken,
  };
};
