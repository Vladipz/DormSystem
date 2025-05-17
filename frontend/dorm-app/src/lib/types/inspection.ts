export type InspectionStatus = "scheduled" | "active" | "completed";

export type RoomInspectionStatus =
  | "pending"
  | "confirmed"
  | "not_confirmed"
  | "no_access";

export interface RoomInspection {
  id: string;
  roomNumber: string;
  floor: string;
  building: string;
  status: RoomInspectionStatus;
  comment?: string;
}

export interface Inspection {
  id: string;
  name: string;
  type: string;
  startDate: Date;
  status: InspectionStatus;
  rooms: RoomInspection[];
} 