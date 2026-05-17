import {
  Button,
  Input,
  Label,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui";
import type { BuildingsResponse } from "@/lib/types/building";
import type {
  MaintenancePriority,
  MaintenanceStatus,
} from "@/lib/types/maintenanceTicket";
import type { UserDetails } from "@/lib/hooks/useUser";
import {
  Building2,
  CalendarDays,
  Flag,
  ListFilter,
  RotateCcw,
  UserRound,
} from "lucide-react";
import type { ComponentType, ReactNode } from "react";

export type MaintenanceDatePreset =
  | "week"
  | "month"
  | "quarter"
  | "semester"
  | "year"
  | "custom";

interface MaintenanceHeatmapFiltersProps {
  buildings: BuildingsResponse[];
  users: UserDetails[];
  buildingId: string;
  onBuildingIdChange: (value: string) => void;
  datePreset: MaintenanceDatePreset;
  onDatePresetChange: (value: MaintenanceDatePreset) => void;
  customDateFrom: string;
  onCustomDateFromChange: (value: string) => void;
  customDateTo: string;
  onCustomDateToChange: (value: string) => void;
  status: "All" | MaintenanceStatus;
  onStatusChange: (value: "All" | MaintenanceStatus) => void;
  priority: "All" | MaintenancePriority;
  onPriorityChange: (value: "All" | MaintenancePriority) => void;
  assignedToId: string;
  onAssignedToIdChange: (value: string) => void;
  onReset: () => void;
  disabled?: boolean;
}

const STATUSES: MaintenanceStatus[] = ["Open", "InProgress", "Resolved"];
const PRIORITIES: MaintenancePriority[] = ["Low", "Medium", "High", "Critical"];

export function MaintenanceHeatmapFilters({
  buildings,
  users,
  buildingId,
  onBuildingIdChange,
  datePreset,
  onDatePresetChange,
  customDateFrom,
  onCustomDateFromChange,
  customDateTo,
  onCustomDateToChange,
  status,
  onStatusChange,
  priority,
  onPriorityChange,
  assignedToId,
  onAssignedToIdChange,
  onReset,
  disabled = false,
}: MaintenanceHeatmapFiltersProps) {
  return (
    <div className="bg-card rounded-xl border p-4">
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-[1.1fr_1.1fr_1fr_1fr_1.1fr_auto]">
        <FilterSelect label="Building" icon={Building2}>
          <Select
            value={buildingId}
            onValueChange={onBuildingIdChange}
            disabled={disabled}
          >
            <SelectTrigger className="h-10 pl-9">
              <SelectValue placeholder="Select building" />
            </SelectTrigger>
            <SelectContent>
              {buildings.map((building) => (
                <SelectItem key={building.id} value={building.id}>
                  {building.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </FilterSelect>

        <FilterSelect label="Period" icon={CalendarDays}>
          <Select
            value={datePreset}
            onValueChange={(value) =>
              onDatePresetChange(value as MaintenanceDatePreset)
            }
            disabled={disabled}
          >
            <SelectTrigger className="h-10 pl-9">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="week">Last week</SelectItem>
              <SelectItem value="month">Last month</SelectItem>
              <SelectItem value="quarter">Last quarter</SelectItem>
              <SelectItem value="semester">Last semester</SelectItem>
              <SelectItem value="year">Last year</SelectItem>
              <SelectItem value="custom">Custom</SelectItem>
            </SelectContent>
          </Select>
        </FilterSelect>

        <FilterSelect label="Status" icon={ListFilter}>
          <Select
            value={status}
            onValueChange={(value) =>
              onStatusChange(value as "All" | MaintenanceStatus)
            }
            disabled={disabled}
          >
            <SelectTrigger className="h-10 pl-9">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="All">All statuses</SelectItem>
              {STATUSES.map((item) => (
                <SelectItem key={item} value={item}>
                  {item === "InProgress" ? "In Progress" : item}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </FilterSelect>

        <FilterSelect label="Priority" icon={Flag}>
          <Select
            value={priority}
            onValueChange={(value) =>
              onPriorityChange(value as "All" | MaintenancePriority)
            }
            disabled={disabled}
          >
            <SelectTrigger className="h-10 pl-9">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="All">All priorities</SelectItem>
              {PRIORITIES.map((item) => (
                <SelectItem key={item} value={item}>
                  {item}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </FilterSelect>

        <FilterSelect label="Assignee" icon={UserRound}>
          <Select
            value={assignedToId}
            onValueChange={onAssignedToIdChange}
            disabled={disabled}
          >
            <SelectTrigger className="h-10 pl-9">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All assignees</SelectItem>
              {users.map((user) => (
                <SelectItem key={user.id} value={user.id}>
                  {user.firstName} {user.lastName}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </FilterSelect>

        <div className="flex items-end">
          <Button
            type="button"
            variant="outline"
            className="h-10 w-full gap-2 whitespace-nowrap"
            onClick={onReset}
            disabled={disabled}
          >
            <RotateCcw className="h-4 w-4" />
            Reset filters
          </Button>
        </div>
      </div>

      {datePreset === "custom" && (
        <div className="mt-4 grid gap-4 border-t pt-4 md:grid-cols-2 xl:w-1/2">
          <div className="space-y-2">
            <Label>From</Label>
            <Input
              type="date"
              value={customDateFrom}
              onChange={(event) => onCustomDateFromChange(event.target.value)}
              disabled={disabled}
            />
          </div>
          <div className="space-y-2">
            <Label>To</Label>
            <Input
              type="date"
              value={customDateTo}
              onChange={(event) => onCustomDateToChange(event.target.value)}
              disabled={disabled}
            />
          </div>
        </div>
      )}
    </div>
  );
}

function FilterSelect({
  label,
  icon: Icon,
  children,
}: {
  label: string;
  icon: ComponentType<{ className?: string }>;
  children: ReactNode;
}) {
  return (
    <div className="space-y-2">
      <Label className="text-foreground text-sm font-medium">{label}</Label>
      <div className="relative">
        <Icon className="text-muted-foreground pointer-events-none absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2" />
        {children}
      </div>
    </div>
  );
}
