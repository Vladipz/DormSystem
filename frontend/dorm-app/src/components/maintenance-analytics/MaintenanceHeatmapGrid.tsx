import {
  Badge,
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui";
import type {
  MaintenanceHeatmapCellResponse,
  MaintenanceHeatmapResponse,
} from "@/lib/types/maintenanceAnalytics";
import { cn } from "@/lib/utils";
import { prioColor } from "@/lib/utils/maintenanceUtils";

interface MaintenanceHeatmapGridProps {
  heatmap: MaintenanceHeatmapResponse;
  selectedCell?: MaintenanceHeatmapCellResponse | null;
  onCellSelect: (cell: MaintenanceHeatmapCellResponse) => void;
}

export function MaintenanceHeatmapGrid({
  heatmap,
  selectedCell,
  onCellSelect,
}: MaintenanceHeatmapGridProps) {
  const floorNumbers = Array.from(
    new Set(heatmap.cells.map((cell) => cell.floorNumber)),
  ).sort((a, b) => a - b);
  const blockLabels = Array.from(
    new Set(heatmap.cells.map((cell) => cell.blockLabel)),
  ).sort((a, b) => a.localeCompare(b, undefined, { numeric: true }));

  return (
    <div className="overflow-hidden rounded-xl border bg-card">
        {heatmap.cells.length === 0 ? (
          <div className="text-muted-foreground rounded-md border border-dashed p-8 text-center">
            No blocks found for this building.
          </div>
        ) : (
          <TooltipProvider>
            <div className="overflow-x-auto">
              <table className="bg-card w-full min-w-[760px] border-collapse">
                <thead>
                  <tr>
                    <th className="bg-muted/30 text-foreground w-32 border px-4 py-4 text-center text-sm font-semibold">
                      Floor
                    </th>
                    {blockLabels.map((label) => (
                      <th
                        key={label}
                        className="bg-muted/30 text-foreground min-w-36 border px-4 py-4 text-center text-sm font-semibold"
                      >
                        {label}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {floorNumbers.map((floorNumber) => (
                    <tr key={floorNumber}>
                      <td className="bg-muted/10 text-foreground border px-4 py-3 text-center text-sm font-semibold">
                        Floor {floorNumber}
                      </td>
                      {blockLabels.map((blockLabel) => {
                        const cell = heatmap.cells.find(
                          (item) =>
                            item.floorNumber === floorNumber &&
                            item.blockLabel === blockLabel,
                        );

                        if (!cell) {
                          return (
                            <td key={blockLabel} className="border p-2">
                              <div className="bg-muted/20 h-16 rounded-md border border-dashed" />
                            </td>
                          );
                        }

                        const isSelected =
                          selectedCell?.blockId === cell.blockId;

                        return (
                          <td key={cell.blockId} className="border p-2">
                            <Tooltip>
                              <TooltipTrigger asChild>
                                <button
                                  type="button"
                                  className={cn(
                                    "h-16 w-full rounded-md border p-2 text-center transition hover:scale-[1.01] focus-visible:ring-ring/50 focus-visible:ring-[3px]",
                                    getHeatClass(
                                      cell.ticketsCount,
                                      heatmap.maxTicketsCount,
                                    ),
                                    isSelected &&
                                      "ring-primary ring-2 ring-offset-2",
                                  )}
                                  onClick={() => onCellSelect(cell)}
                                >
                                  <span className="block text-xl leading-none font-bold">
                                    {cell.ticketsCount}
                                  </span>
                                  <span className="block text-xs opacity-80">
                                    {cell.resolvedPercentage}%
                                  </span>
                                </button>
                              </TooltipTrigger>
                              <TooltipContent className="bg-popover text-popover-foreground w-64 rounded-lg border p-3">
                                <div className="space-y-2">
                                  <div className="font-medium">
                                    Floor {cell.floorNumber}, {cell.blockLabel}
                                  </div>
                                  <div className="grid grid-cols-2 gap-1 text-xs">
                                    <span>Requests</span>
                                    <span className="text-right font-medium">
                                      {cell.ticketsCount}
                                    </span>
                                    <span>Open</span>
                                    <span className="text-right font-medium">
                                      {cell.openCount}
                                    </span>
                                    <span>In progress</span>
                                    <span className="text-right font-medium">
                                      {cell.inProgressCount}
                                    </span>
                                    <span>Resolved</span>
                                    <span className="text-right font-medium">
                                      {cell.resolvedCount}
                                    </span>
                                    <span>Resolved rate</span>
                                    <span className="text-right font-medium">
                                      {cell.resolvedPercentage}%
                                    </span>
                                  </div>
                                  <div className="flex items-center justify-between gap-2 text-xs">
                                    <span>Top priority</span>
                                    {cell.mostFrequentPriority ? (
                                      <Badge
                                        variant="outline"
                                        className={prioColor(
                                          cell.mostFrequentPriority,
                                        )}
                                      >
                                        {cell.mostFrequentPriority}
                                      </Badge>
                                    ) : (
                                      <span className="text-muted-foreground">
                                        None
                                      </span>
                                    )}
                                  </div>
                                </div>
                              </TooltipContent>
                            </Tooltip>
                          </td>
                        );
                      })}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </TooltipProvider>
        )}
    </div>
  );
}

function getHeatClass(ticketsCount: number, maxTicketsCount: number) {
  const ratio = maxTicketsCount === 0 ? 0 : ticketsCount / maxTicketsCount;

  if (ratio > 0.75) {
    return "border-red-400 bg-red-100 text-red-950 hover:bg-red-200 dark:border-red-700 dark:bg-red-800/45 dark:text-red-100 dark:hover:bg-red-800/60";
  }

  if (ratio > 0.5) {
    return "border-orange-400 bg-orange-100 text-orange-950 hover:bg-orange-200 dark:border-orange-700 dark:bg-orange-800/45 dark:text-orange-100 dark:hover:bg-orange-800/60";
  }

  if (ratio > 0.25) {
    return "border-yellow-400 bg-yellow-100 text-yellow-950 hover:bg-yellow-200 dark:border-yellow-700 dark:bg-yellow-800/45 dark:text-yellow-100 dark:hover:bg-yellow-800/60";
  }

  return "border-emerald-400 bg-emerald-100 text-emerald-950 hover:bg-emerald-200 dark:border-emerald-700 dark:bg-emerald-800/45 dark:text-emerald-100 dark:hover:bg-emerald-800/60";
}
