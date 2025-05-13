import { PagedResponse } from "./pagination";

export interface Event {
  id: string;
  ownerId: string;
  name: string;
  date: string;
  location: string;
  lastParticipants: Participant[];
  numberOfAttendees?: number;
  isPublic: boolean;
  description?: string;
  buildingId?: string;
  roomId?: string;
}

export interface EventDetails extends Event {
  participants: Participant[];
}

export interface Participant {
  userId: string;
  joinedAt: string;
  firstName: string;
  lastName: string;
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
  numberOfAttendees?: number;
  isPublic: boolean;
  description: string;
  buildingId?: string;
  roomId?: string;
}
