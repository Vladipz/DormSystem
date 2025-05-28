import { InspectionCard } from "@/components/inspection/InspectionCard";
import {
  Button,
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/components/ui";
import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/components/ui/pagination";
import { useListInspections } from "@/lib/hooks/useInspections";
import { authService } from "@/lib/services/authService";
import { InspectionStatus } from "@/lib/types/inspection";
import { createFileRoute, redirect, useNavigate } from "@tanstack/react-router";
import { Plus } from "lucide-react";
import { useState } from "react";

export const Route = createFileRoute("/_mainLayout/inspections/")({
  beforeLoad: async () => {
    const authStatus = await authService.checkAuthStatus();
    if (!authStatus || !authStatus.isAuthenticated) {
      throw redirect({ to: "/login", search: { returnTo: "/inspections" } });
    }
    return { userRole: authStatus.role };
  },
  component: InspectionsPage,
});

export function InspectionsPage() {
  const navigate = useNavigate();
  const { userRole } = Route.useRouteContext();
  const [currentPage, setCurrentPage] = useState(1);
  const [activeTab, setActiveTab] = useState<"all" | InspectionStatus>("all");
  const pageSize = 10; // Show 10 inspections per page

  const { data, isLoading } = useListInspections({
    status: activeTab === "all" ? undefined : activeTab,
    pageNumber: currentPage,
    pageSize,
  });

  const inspections = data?.items ?? [];
  const totalPages = data?.totalPages ?? 1;

  // Reset to first page when changing filters
  const handleTabChange = (value: string) => {
    setActiveTab(value as "all" | InspectionStatus);
    setCurrentPage(1);
  };

  // Check if user is admin to show create button and allow details access
  const isAdmin = userRole === "Admin";

  if (isLoading) {
    return <div>Loading...</div>;
  }

  const paginationItems = [];
  for (let i = 1; i <= totalPages; i++) {
    if (
      i === 1 || // Always show first page
      i === totalPages || // Always show last page
      (i >= currentPage - 1 && i <= currentPage + 1) // Show current page and neighbors
    ) {
      paginationItems.push(
        <PaginationItem key={i}>
          <PaginationLink
            onClick={() => setCurrentPage(i)}
            isActive={currentPage === i}
          >
            {i}
          </PaginationLink>
        </PaginationItem>,
      );
    } else if (
      (i === 2 && currentPage > 3) ||
      (i === totalPages - 1 && currentPage < totalPages - 2)
    ) {
      paginationItems.push(
        <PaginationItem key={i}>
          <PaginationEllipsis />
        </PaginationItem>,
      );
    }
  }

  // Handle inspection card click - only allow details for admin users
  const handleInspectionClick = (inspectionId: string) => {
    if (isAdmin) {
      navigate({ to: `/inspections/${inspectionId}` });
    }
    // For non-admin users, do nothing (no navigation to details)
  };

  // Return the list of inspections
  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Dorm Inspections</h1>
        {isAdmin && (
          <Button onClick={() => navigate({ to: "/inspections/create" })}>
            <Plus className="mr-2 h-4 w-4" /> Create Inspection
          </Button>
        )}
      </div>

      <Tabs
        defaultValue="all"
        value={activeTab}
        onValueChange={handleTabChange}
      >
        <TabsList>
          <TabsTrigger value="all">All</TabsTrigger>
          <TabsTrigger value="scheduled">Scheduled</TabsTrigger>
          <TabsTrigger value="active">Active</TabsTrigger>
          <TabsTrigger value="completed">Completed</TabsTrigger>
        </TabsList>

        <TabsContent value={activeTab} className="mt-4 space-y-4">
          {inspections.map((inspection) => (
            <InspectionCard
              key={inspection.id}
              inspection={inspection}
              onClick={() => handleInspectionClick(inspection.id)}
              isClickable={isAdmin}
            />
          ))}
        </TabsContent>
      </Tabs>

      {totalPages > 1 && (
        <div className="mt-8 flex justify-center">
          <Pagination>
            <PaginationContent>
              <PaginationItem>
                <PaginationPrevious
                  href="#"
                  onClick={(e) => {
                    e.preventDefault();
                    setCurrentPage((prev) => Math.max(1, prev - 1));
                  }}
                  aria-disabled={currentPage === 1}
                />
              </PaginationItem>

              {paginationItems}

              <PaginationItem>
                <PaginationNext
                  href="#"
                  onClick={(e) => {
                    e.preventDefault();
                    setCurrentPage((prev) => Math.min(totalPages, prev + 1));
                  }}
                  aria-disabled={currentPage === totalPages}
                />
              </PaginationItem>
            </PaginationContent>
          </Pagination>
        </div>
      )}
    </div>
  );
}
