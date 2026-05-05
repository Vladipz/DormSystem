import { axiosClient } from "../utils/axios-client";
import {
  CreateEventRequest,
  Event,
  EventComment,
  EventDetails,
  PagedEventCommentsResponse,
  PagedEventsResponse,
} from "../types/event";

const API_URL = "/events";

export class EventService {
  public static async getAllEvents(
    pageNumber = 1,
    pageSize = 10,
    search?: string,
  ): Promise<PagedEventsResponse> {
    const response = await axiosClient.get<PagedEventsResponse>(API_URL, {
      params: { pageNumber, pageSize, search: search || undefined },
    });

    return response.data;
  }

  public static async getEventById(eventId: string): Promise<EventDetails> {
    const response = await axiosClient.get<EventDetails>(`${API_URL}/${eventId}`);
    return response.data;
  }

  public static async getEventComments(
    eventId: string,
    pageNumber = 1,
    pageSize = 3,
  ): Promise<PagedEventCommentsResponse> {
    const response = await axiosClient.get<PagedEventCommentsResponse>(
      `${API_URL}/${eventId}/comments`,
      {
        params: { pageNumber, pageSize },
      },
    );

    return response.data;
  }

  public static async createEventComment(
    eventId: string,
    content: string,
  ): Promise<EventComment> {
    const response = await axiosClient.post<EventComment>(
      `${API_URL}/${eventId}/comments`,
      { content },
    );

    return response.data;
  }

  public static async updateEventComment(
    eventId: string,
    commentId: string,
    content: string,
  ): Promise<EventComment> {
    const response = await axiosClient.put<EventComment>(
      `${API_URL}/${eventId}/comments/${commentId}`,
      { content },
    );

    return response.data;
  }

  public static async deleteEventComment(
    eventId: string,
    commentId: string,
  ): Promise<void> {
    await axiosClient.delete(`${API_URL}/${eventId}/comments/${commentId}`);
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
