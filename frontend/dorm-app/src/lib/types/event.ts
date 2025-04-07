export interface Event {
  id: string;
  ownerId: string;
  name: string;
  date: string;
  location: string;
  lastParticipants: EventParticipant[];
  numberOfAttendees: number;
  isPublic: boolean;
}

export interface EventParticipant {
  userId: string;
  joinedAt: string;
}

export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export type PagedEventsResponse = PagedResponse<Event>;

export interface CreateEventDto {
  name: string;
  date: string;
  location: string;
  numberOfAttendees?: number;
  isPublic: boolean;
}

/**
 * Matches the C# CreateEventRequest model exactly
 * @see Events.API.Contracts.CreateEventRequest
 */
export interface CreateEventRequest {
  name: string;
  date: string; // ISO format string representation of DateTime
  location: string;
  numberOfAttendees: number | null;
  isPublic: boolean;
}
