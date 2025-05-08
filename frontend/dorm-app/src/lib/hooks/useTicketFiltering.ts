import { type MaintenanceStatus, type MaintenanceTicketResponse } from "@/lib/types/maintenanceTicket";
import { prioWeight, statusWeight } from "@/lib/utils/maintenanceUtils";
import { useMemo, useState } from "react";
import { useBuildings } from "./useBuildings";

type SortField = "date" | "room" | "priority" | "status";

export function useTicketFiltering(
  tickets: MaintenanceTicketResponse[]
) {
  // Filter state
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<"All" | MaintenanceStatus>("All");
  const [buildingFilter, setBuildingFilter] = useState("all");

  // Sort state
  const [sortBy, setSortBy] = useState<SortField>("date");
  const [sortOrder, setOrder] = useState<"asc" | "desc">("desc");

  // Fetch buildings
  const { data: buildingsResponse } = useBuildings();

  // Derived data
  const buildings = useMemo(
    () => buildingsResponse || [],
    [buildingsResponse]
  );

  // Filtered tickets (text search only - status and building are filtered on backend)
  const filteredTickets = useMemo(() => {
    return tickets.filter((ticket) => {
      // Text search match
      const searchText = `${ticket.room.id} ${ticket.title} ${ticket.description}`.toLowerCase();
      const matchesSearch = searchText.includes(searchTerm.toLowerCase());
      
      return matchesSearch;
    });
  }, [tickets, searchTerm]);

  // Sorted tickets
  const sortedTickets = useMemo(() => {
    const copy = [...filteredTickets];
    const sortDirection = sortOrder === "asc" ? 1 : -1;
    
    copy.sort((a, b) => {
      let comparison = 0;
      
      if (sortBy === "date") {
        comparison = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
      } else if (sortBy === "priority") {
        comparison = prioWeight(a.priority) - prioWeight(b.priority);
      } else if (sortBy === "status") {
        comparison = statusWeight(a.status) - statusWeight(b.status);
      } else if (sortBy === "room") {
        comparison = a.room.label.localeCompare(b.room.label);
      }
      
      return comparison * sortDirection;
    });
    
    return copy;
  }, [filteredTickets, sortBy, sortOrder]);

  // Toggle sort function
  const toggleSort = (column: SortField) => {
    setSortBy((prev) => {
      if (prev === column) {
        setOrder((o) => (o === "asc" ? "desc" : "asc"));
        return column;
      } else {
        setOrder("asc");
        return column;
      }
    });
  };

  return {
    // State
    searchTerm,
    setSearchTerm,
    statusFilter,
    setStatusFilter,
    buildingFilter,
    setBuildingFilter,
    sortBy,
    sortOrder,
    
    // Derived data
    buildings,
    filteredTickets,
    sortedTickets,
    
    // Actions
    toggleSort,
  };
}