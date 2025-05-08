import { BlockInfo } from "./block";
import { BuildingInfo } from "./building";

export type RoomStatus = "Available" | "Occupied" | "Maintenance";
export type RoomType = "Regular" | "Specialized";

export interface CreateRoomRequest {
  blockId?: string | null;
  label: string;
  capacity: number;
  status: RoomStatus;
  roomType: RoomType;
  purpose?: string | null;
  amenities: string[];
}

export interface CreateRoomResponse {
  id: string;
}

export interface DeletedRoomResponse {
  id: string;
}

export interface RoomDetailsResponse {
  id: string;
  block?: BlockInfo | null;
  floor: number;
  building: BuildingInfo;
  label: string;
  capacity: number;
  status: RoomStatus;
  roomType: RoomType;
  purpose?: string | null;
  amenities: string[];
}

export interface RoomsResponse {
  id: string;
  blockId?: string | null;
  label: string;
  capacity: number;
  status: RoomStatus;
  roomType: RoomType;
}

export interface UpdateRoomRequest {
  id: string;
  blockId?: string | null;
  label: string;
  capacity: number;
  status: RoomStatus;
  roomType: RoomType;
  purpose?: string | null;
  amenities: string[];
}

export interface UpdatedRoomResponse {
  id: string;
}
