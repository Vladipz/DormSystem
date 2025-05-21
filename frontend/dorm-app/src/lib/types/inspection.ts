export type InspectionStatus = "Scheduled" | "Active" | "Completed";

export type RoomInspectionStatus =
  | "Pending"
  | "Confirmed"
  | "NotConfirmed"
  | "NoAccess";

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

export interface ShortInspection {
  id: string;
  name: string;
  type: string;
  startDate: Date;
  status: InspectionStatus;
  roomsCount: number;
  pendingRoomsCount: number;
  confirmedRoomsCount: number;
  notConfirmedRoomsCount: number;
  noAccessRoomsCount: number;
}

