import { axiosClient } from "../utils/axios-client";
import { CreateEventRequest, Event, EventDetails, PagedEventsResponse } from "../types/event";

const API_URL = "/events";

export class EventService {
  public static async getAllEvents(
    pageNumber = 1,
    pageSize = 10,
  ): Promise<PagedEventsResponse> {
    const response = await axiosClient.get<PagedEventsResponse>(API_URL, {
      params: { pageNumber, pageSize },
    });

    return response.data;
  }

  public static async getEventById(eventId: string): Promise<EventDetails> {
    const response = await axiosClient.get<EventDetails>(`${API_URL}/${eventId}`);
    return response.data;
  }

  public static async joinEvent(eventId: string): Promise<void> {
    await axiosClient.post(`${API_URL}/${eventId}/join`, {});
  }

  public static async leaveEvent(eventId: string, userId: string): Promise<void> {
    await axiosClient.delete(`${API_URL}/${eventId}/participants/${userId}`);
  }

  public static async createEvent(eventData: CreateEventRequest): Promise<Event> {
    const response = await axiosClient.post<Event>(API_URL, eventData);
    return response.data;
  }

  public static async updateEvent(eventId: string, eventData: CreateEventRequest): Promise<void> {
    await axiosClient.put(`${API_URL}/${eventId}`, eventData);
  }

  public static async validateInvitation(
    eventId: string,
    token: string,
  ): Promise<EventDetails> {
    const response = await axiosClient.get<EventDetails>(
      `${API_URL}/${eventId}/validate-invitation`,
      {
        params: { token },
      },
    );

    return response.data;
  }

  public static async joinEventWithToken(eventId: string, token: string): Promise<void> {
    const body = token ? { token } : {};
    await axiosClient.post(`${API_URL}/${eventId}/join`, body);
  }

  public static async getEventInviteLink(eventId: string): Promise<string> {
    const response = await axiosClient.get<{ invitationLink: string }>(
      `${API_URL}/${eventId}/generate-invitation`,
    );

    return response.data.invitationLink;
  }
}
