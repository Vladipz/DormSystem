import { Input, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui";
import type { BuildingsResponse } from "@/lib/types/building";
import type { MaintenanceStatus } from "@/lib/types/maintenanceTicket";
import { Search } from "lucide-react";

interface TicketFiltersProps {
  searchTerm: string;
  setSearchTerm: (value: string) => void;
  statusFilter: "All" | MaintenanceStatus;
  setStatusFilter: (value: "All" | MaintenanceStatus) => void;
  buildingFilter: string;
  setBuildingFilter: (value: string) => void;
  buildings: BuildingsResponse[];
}

export function TicketFilters({
  searchTerm,
  setSearchTerm,
  statusFilter,
  setStatusFilter,
  buildingFilter,
  setBuildingFilter,
  buildings,
}: TicketFiltersProps) {
  return (
    <div className="space-y-4">
      {/* Search */}
      <div className="relative flex-1">
        <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search tickets…"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="pl-8"
        />
      </div>

      {/* Filters */}
      <div className="flex flex-wrap gap-2">
        <Select
          value={statusFilter}
          onValueChange={(v) => setStatusFilter(v as "All" | MaintenanceStatus)}
        >
          <SelectTrigger className="w-[170px]">
            <SelectValue placeholder="Status" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="All">All</SelectItem>
            {["Open", "InProgress", "Resolved"].map((s) => (
              <SelectItem key={s} value={s}>
                {s === "InProgress" ? "In Progress" : s}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={buildingFilter} onValueChange={setBuildingFilter}>
          <SelectTrigger className="w-[170px]">
            <SelectValue placeholder="Building" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Buildings</SelectItem>
            {buildings.map((building) => (
              <SelectItem value={building.id} key={building.id}>
                {building.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
    </div>
  );
}