import axios from "axios";
import { CreateEventRequest, Event, EventDetails, PagedEventsResponse } from "../types/event";

const API_URL = "http://localhost:5095/api/events";

export const EventService = {
  // Отримати всі події
  async getAllEvents(): Promise<Event[]> {
    const response = await axios.get<PagedEventsResponse>(API_URL);
    console.log("Response data:", response.data); // Додано для налагодження
    return response.data.items;
  },

  // Отримати подію за ID
  async getEventById(eventId: string): Promise<EventDetails> {
    const accessToken = localStorage.getItem('accessToken');
    const headers: Record<string, string> = {};
    
    if (accessToken) {
      headers['Authorization'] = `Bearer ${accessToken}`;
    }
    console.log("Makes request with headers:", headers); // Додано для налагодження
    const response = await axios.get<EventDetails>(`${API_URL}/${eventId}`, { headers });
    return response.data;
  },

  // Приєднатися до події
  async joinEvent(eventId: string, userId: string): Promise<void> {
    await axios.post(`${API_URL}/${eventId}/participants`, { userId });
  },
  
  // Створити нову подію
  async createEvent(eventData: CreateEventRequest): Promise<Event> {
    const accessToken = localStorage.getItem('accessToken');
    if (!accessToken) {
      throw new Error('Authentication required to create an event');
    }
    
    try {
      const response = await axios.post<Event>(API_URL, eventData, {
        headers: {
          'Authorization': `Bearer ${accessToken}`,
          'Content-Type': 'application/json',
        }
      });
      
      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error) && (error.response?.status === 401 || error.response?.status === 403)) {
        throw new Error('Authentication required to create an event');
      }
      throw error;
    }
  },
  
  // Оновити існуючу подію
  async updateEvent(eventId: string, eventData: CreateEventRequest): Promise<void> {
    const accessToken = localStorage.getItem('accessToken');
    if (!accessToken) {
      throw new Error('Authentication required to update an event');
    }
    
    try {
      await axios.put(`${API_URL}/${eventId}`, eventData, {
        headers: {
          'Authorization': `Bearer ${accessToken}`,
          'Content-Type': 'application/json',
        }
      });
    } catch (error) {
      if (axios.isAxiosError(error)) {
        if (error.response?.status === 401 || error.response?.status === 403) {
          throw new Error('Authentication required to update this event');
        } else if (error.response?.status === 404) {
          throw new Error('Event not found');
        }
      }
      throw error;
    }
  },
};
