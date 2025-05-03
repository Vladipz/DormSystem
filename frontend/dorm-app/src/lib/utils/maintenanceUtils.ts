import type { MaintenancePriority, MaintenanceStatus } from "../types/maintenanceTicket";

/**
 * Capitalizes the first letter of a string
 */
export const capitalize = (s: string): string => 
  s.charAt(0).toUpperCase() + s.slice(1);

/**
 * Returns the weight of a priority for sorting
 */
export const prioWeight = (p: MaintenancePriority): number =>
  ({ High: 0, Medium: 1, Low: 2 })[p] ?? 99;

/**
 * Returns the weight of a status for sorting
 */
export const statusWeight = (s: MaintenanceStatus): number =>
  ({ Open: 0, InProgress: 1, Resolved: 2 })[s] ?? 99;

/**
 * Returns the CSS class for a status badge
 */
export const statusColor = (s: MaintenanceStatus): string =>
  ({
    Open: "bg-yellow-100 text-yellow-800",
    InProgress: "bg-blue-100 text-blue-800",
    Resolved: "bg-green-100 text-green-800",
  })[s] ?? "bg-gray-100 text-gray-800";

/**
 * Returns the CSS class for a priority badge
 */
export const prioColor = (p: MaintenancePriority): string =>
  ({
    High: "bg-orange-100 text-orange-800",
    Medium: "bg-blue-100 text-blue-800",
    Low: "bg-green-100 text-green-800",
  })[p] ?? "bg-gray-100 text-gray-800";