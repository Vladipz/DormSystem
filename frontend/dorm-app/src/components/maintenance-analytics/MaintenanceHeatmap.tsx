import { MaintenanceDrilldownPanel } from "@/components/maintenance-analytics/MaintenanceDrilldownPanel";
import {
  MaintenanceDatePreset,
  MaintenanceHeatmapFilters,
} from "@/components/maintenance-analytics/MaintenanceHeatmapFilters";
import { MaintenanceHeatmapGrid } from "@/components/maintenance-analytics/MaintenanceHeatmapGrid";
import { Skeleton } from "@/components/ui";
import { useBuildings } from "@/lib/hooks/useBuildings";
import { useMaintenanceHeatmap } from "@/lib/hooks/useMaintenanceAnalytics";
import { useUsers } from "@/lib/hooks/useUser";
import { cn } from "@/lib/utils";
import type {
  MaintenanceAnalyticsFilters,
  MaintenanceHeatmapCellResponse,
  MaintenanceHeatmapResponse,
} from "@/lib/types/maintenanceAnalytics";
import type {
  MaintenancePriority,
  MaintenanceStatus,
} from "@/lib/types/maintenanceTicket";
import {
  CircleAlert,
  CircleCheck,
  ClipboardList,
  TrendingUp,
} from "lucide-react";
import type { ComponentType } from "react";
import { useEffect, useState } from "react";

interface MaintenanceHeatmapProps {
  isAdmin: boolean;
}

export function MaintenanceHeatmap({ isAdmin }: MaintenanceHeatmapProps) {
  const {
    data: buildings = [],
    isLoading: buildingsLoading,
    isError: buildingsError,
  } = useBuildings();
  const { data: usersData, isLoading: usersLoading } = useUsers({
    pageSize: 100,
  });
  const users = usersData?.items ?? [];

  const [buildingId, setBuildingId] = useState("");
  const [datePreset, setDatePreset] = useState<MaintenanceDatePreset>("year");
  const [customDateFrom, setCustomDateFrom] = useState(
    toDateInput(addMonths(new Date(), -1)),
  );
  const [customDateTo, setCustomDateTo] = useState(toDateInput(new Date()));
  const [status, setStatus] = useState<"All" | MaintenanceStatus>("All");
  const [priority, setPriority] = useState<"All" | MaintenancePriority>("All");
  const [assignedToId, setAssignedToId] = useState("all");
  const [selectedCell, setSelectedCell] =
    useState<MaintenanceHeatmapCellResponse | null>(null);

  useEffect(() => {
    if (!buildingId && buildings.length > 0) {
      setBuildingId(buildings[0].id);
    }
  }, [buildingId, buildings]);

  const dateRange = getDateRange(datePreset, customDateFrom, customDateTo);
  const filters: MaintenanceAnalyticsFilters = {
    buildingId,
    dateFrom: dateRange.dateFrom,
    dateTo: dateRange.dateTo,
    status: status === "All" ? undefined : status,
    priority: priority === "All" ? undefined : priority,
    assignedToId: assignedToId === "all" ? undefined : assignedToId,
  };

  const {
    data: heatmap,
    isLoading,
    isError,
    isFetching,
  } = useMaintenanceHeatmap(filters);

  useEffect(() => {
    setSelectedCell(null);
  }, [
    buildingId,
    datePreset,
    customDateFrom,
    customDateTo,
    status,
    priority,
    assignedToId,
  ]);

  const resetFilters = () => {
    setBuildingId(buildings[0]?.id ?? "");
    setDatePreset("year");
    setCustomDateFrom(toDateInput(addMonths(new Date(), -1)));
    setCustomDateTo(toDateInput(new Date()));
    setStatus("All");
    setPriority("All");
    setAssignedToId("all");
    setSelectedCell(null);
  };

  if (buildingsLoading) {
    return <Skeleton className="h-[640px] w-full" />;
  }

  if (buildingsError) {
    return (
      <div className="text-destructive rounded-md border p-6">
        Failed to load buildings.
      </div>
    );
  }

  if (buildings.length === 0) {
    return (
      <div className="text-muted-foreground rounded-md border p-6">
        No buildings available for analytics.
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <MaintenanceHeatmapFilters
        buildings={buildings}
        users={users}
        buildingId={buildingId}
        onBuildingIdChange={setBuildingId}
        datePreset={datePreset}
        onDatePresetChange={setDatePreset}
        customDateFrom={customDateFrom}
        onCustomDateFromChange={setCustomDateFrom}
        customDateTo={customDateTo}
        onCustomDateToChange={setCustomDateTo}
        status={status}
        onStatusChange={setStatus}
        priority={priority}
        onPriorityChange={setPriority}
        assignedToId={assignedToId}
        onAssignedToIdChange={setAssignedToId}
        onReset={resetFilters}
        disabled={isFetching || usersLoading}
      />

      {isLoading ? (
        <Skeleton className="h-[420px] w-full" />
      ) : isError || !heatmap ? (
        <div className="text-destructive rounded-md border p-6">
          Failed to load maintenance heatmap.
        </div>
      ) : (
        <>
          <MaintenanceHeatmapSummary heatmap={heatmap} />
          <MaintenanceHeatmapLegend />
          <MaintenanceHeatmapGrid
            heatmap={heatmap}
            selectedCell={selectedCell}
            onCellSelect={setSelectedCell}
          />
        </>
      )}

      <MaintenanceDrilldownPanel
        buildingId={buildingId}
        selectedCell={selectedCell}
        filters={{
          dateFrom: filters.dateFrom,
          dateTo: filters.dateTo,
          status: filters.status,
          priority: filters.priority,
          assignedToId: filters.assignedToId,
        }}
        isAdmin={isAdmin}
      />
    </div>
  );
}

function MaintenanceHeatmapSummary({
  heatmap,
}: {
  heatmap: MaintenanceHeatmapResponse;
}) {
  const summary = getHeatmapSummary(heatmap);

  return (
    <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
      <SummaryCard
        icon={ClipboardList}
        iconClassName="border-blue-200 bg-blue-50 text-blue-600"
        label="Total Tickets"
        value={summary.totalTickets.toString()}
        description="Selected period"
      />
      <SummaryCard
        icon={CircleAlert}
        iconClassName="border-orange-200 bg-orange-50 text-orange-600"
        label="Open Tickets"
        value={summary.openTickets.toString()}
        description={`${summary.openPercentage}% of total`}
      />
      <SummaryCard
        icon={CircleCheck}
        iconClassName="border-emerald-200 bg-emerald-50 text-emerald-600"
        label="Avg Resolution Rate"
        value={`${summary.resolutionRate}%`}
        description="Across all areas"
      />
      <SummaryCard
        icon={TrendingUp}
        iconClassName="border-purple-200 bg-purple-50 text-purple-600"
        label="Highest Activity Area"
        value={summary.highestActivityLabel}
        description={summary.highestActivityDescription}
        descriptionClassName="inline-flex rounded-md bg-red-50 px-2 py-0.5 font-medium text-red-600"
      />
    </div>
  );
}

function SummaryCard({
  icon: Icon,
  iconClassName,
  label,
  value,
  description,
  descriptionClassName,
}: {
  icon: ComponentType<{ className?: string }>;
  iconClassName: string;
  label: string;
  value: string;
  description: string;
  descriptionClassName?: string;
}) {
  return (
    <div className="bg-card flex min-h-24 items-center gap-4 rounded-xl border p-5">
      <div
        className={cn(
          "flex h-12 w-12 shrink-0 items-center justify-center rounded-xl border",
          iconClassName,
        )}
      >
        <Icon className="h-6 w-6" />
      </div>
      <div className="min-w-0">
        <div className="text-muted-foreground text-sm font-medium">{label}</div>
        <div className="text-foreground truncate text-2xl leading-tight font-bold">
          {value}
        </div>
        <div
          className={cn(
            "text-muted-foreground mt-1 text-sm",
            descriptionClassName,
          )}
        >
          {description}
        </div>
      </div>
    </div>
  );
}

function MaintenanceHeatmapLegend() {
  return (
    <div className="text-muted-foreground flex flex-wrap items-center gap-x-6 gap-y-2 px-1 text-sm">
      <LegendItem colorClassName="bg-emerald-500" label="Low (0-25%)" />
      <LegendItem colorClassName="bg-yellow-400" label="Medium (26-50%)" />
      <LegendItem colorClassName="bg-orange-500" label="High (51-75%)" />
      <LegendItem colorClassName="bg-red-500" label="Critical (76%+)" />
      <span className="text-foreground font-medium">• % resolved</span>
    </div>
  );
}

function LegendItem({
  colorClassName,
  label,
}: {
  colorClassName: string;
  label: string;
}) {
  return (
    <span className="inline-flex items-center gap-2">
      <span className={cn("h-2.5 w-2.5 rounded-full", colorClassName)} />
      {label}
    </span>
  );
}

function getHeatmapSummary(heatmap: MaintenanceHeatmapResponse) {
  const totalTickets = heatmap.cells.reduce(
    (sum, cell) => sum + cell.ticketsCount,
    0,
  );
  const openTickets = heatmap.cells.reduce(
    (sum, cell) => sum + cell.openCount,
    0,
  );
  const resolvedTickets = heatmap.cells.reduce(
    (sum, cell) => sum + cell.resolvedCount,
    0,
  );
  const highestActivityCell =
    heatmap.cells.reduce<MaintenanceHeatmapCellResponse | null>(
      (current, cell) => {
        if (!current || cell.ticketsCount > current.ticketsCount) {
          return cell;
        }

        return current;
      },
      null,
    );

  return {
    totalTickets,
    openTickets,
    openPercentage: formatPercent(
      totalTickets === 0 ? 0 : (openTickets / totalTickets) * 100,
    ),
    resolutionRate: formatPercent(
      totalTickets === 0 ? 0 : (resolvedTickets / totalTickets) * 100,
    ),
    highestActivityLabel:
      highestActivityCell && highestActivityCell.ticketsCount > 0
        ? `Floor ${highestActivityCell.floorNumber} - ${highestActivityCell.blockLabel}`
        : "No activity",
    highestActivityDescription:
      highestActivityCell && highestActivityCell.ticketsCount > 0
        ? `${highestActivityCell.openCount} open tickets`
        : "No tickets",
  };
}

function formatPercent(value: number) {
  return value.toFixed(1);
}

function getDateRange(
  datePreset: MaintenanceDatePreset,
  customDateFrom: string,
  customDateTo: string,
) {
  if (datePreset === "custom") {
    return {
      dateFrom: customDateFrom
        ? startOfDay(customDateFrom).toISOString()
        : undefined,
      dateTo: customDateTo ? endOfDay(customDateTo).toISOString() : undefined,
    };
  }

  const dateTo = endOfDay(toDateInput(new Date()));
  const dateFrom = startOfDay(toDateInput(getPresetStartDate(datePreset)));

  return {
    dateFrom: dateFrom.toISOString(),
    dateTo: dateTo.toISOString(),
  };
}

function getPresetStartDate(
  datePreset: Exclude<MaintenanceDatePreset, "custom">,
) {
  const today = new Date();

  switch (datePreset) {
    case "week":
      return addDays(today, -7);
    case "month":
      return addMonths(today, -1);
    case "quarter":
      return addMonths(today, -3);
    case "semester":
      return addMonths(today, -6);
    case "year":
      return addMonths(today, -12);
  }
}

function addDays(date: Date, days: number) {
  const result = new Date(date);
  result.setDate(result.getDate() + days);
  return result;
}

function addMonths(date: Date, months: number) {
  const result = new Date(date);
  result.setMonth(result.getMonth() + months);
  return result;
}

function toDateInput(date: Date) {
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, "0");
  const day = `${date.getDate()}`.padStart(2, "0");
  return `${year}-${month}-${day}`;
}

function startOfDay(dateValue: string) {
  return new Date(`${dateValue}T00:00:00`);
}

function endOfDay(dateValue: string) {
  return new Date(`${dateValue}T23:59:59.999`);
}
