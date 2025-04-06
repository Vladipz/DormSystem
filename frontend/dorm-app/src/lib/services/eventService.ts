import axios from "axios";
import { Event, PagedEventsResponse } from "../types/event";

const API_URL = "http://localhost:5095/api/events";

export const EventService = {
  // Отримати всі події
  async getAllEvents(): Promise<Event[]> {
    const response = await axios.get<PagedEventsResponse>(API_URL);
    console.log("Response data:", response.data); // Додано для налагодження
    return response.data.items;
  },

  // Приєднатися до події
  async joinEvent(eventId: string, userId: string): Promise<void> {
    await axios.post(`${API_URL}/${eventId}/participants`, { userId });
  },
};
