import { maintenanceTicketService } from "@/lib/services/maintenanceTicketService";
import type {
  ChangeMaintenanceTicketStatusRequest,
  CreateMaintenanceTicketRequest,
  MaintenancePriority,
  MaintenanceStatus,
  UpdateMaintenanceTicketRequest,
} from "@/lib/types/maintenanceTicket";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

// Query keys for React Query
const MAINTENANCE_TICKETS_KEY = "maintenanceTickets";
const MAINTENANCE_TICKET_KEY = "maintenanceTicket";

export const useMaintenanceTickets = (params?: {
  roomId?: string;
  status?: MaintenanceStatus;
  reporterById?: string;
  assignedToId?: string;
  priority?: MaintenancePriority;
  buildingId?: string;
  page?: number;
  pageSize?: number;
}) => {
  return useQuery({
    queryKey: [MAINTENANCE_TICKETS_KEY, params],
    queryFn: () => maintenanceTicketService.getMaintenanceTickets(params),
  });
};

export const useMaintenanceTicket = (id: string) => {
  return useQuery({
    queryKey: [MAINTENANCE_TICKET_KEY, id],
    queryFn: () => maintenanceTicketService.getMaintenanceTicketById(id),
    enabled: !!id, // Only run query if ID is provided
  });
};

export const useCreateMaintenanceTicket = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateMaintenanceTicketRequest) =>
      maintenanceTicketService.createMaintenanceTicket(data),
    onSuccess: () => {
      toast.success("Ticket created successfully.");
      queryClient.invalidateQueries({
        queryKey: [MAINTENANCE_TICKETS_KEY],
      });
    },
    onError: () => {
      toast.error("Failed to create ticket.");
    },
  });
};

export const useUpdateMaintenanceTicket = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      ...data
    }: UpdateMaintenanceTicketRequest & { id: string }) =>
      maintenanceTicketService.updateMaintenanceTicket(id, data),
    onSuccess: (_, variables) => {
      toast.success("Ticket updated successfully.");
      queryClient.invalidateQueries({
        queryKey: [MAINTENANCE_TICKETS_KEY],
      });
      queryClient.invalidateQueries({
        queryKey: [MAINTENANCE_TICKET_KEY, variables.id],
      });
    },
    onError: () => {
      toast.error("Failed to update ticket.");
    },
  });
};

export const useChangeMaintenanceTicketStatus = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      ...data
    }: ChangeMaintenanceTicketStatusRequest & { id: string }) =>
      maintenanceTicketService.changeMaintenanceTicketStatus(id, data),
    onSuccess: (_, variables) => {
      toast.success("Ticket status changed successfully.");
      queryClient.invalidateQueries({
        queryKey: [MAINTENANCE_TICKETS_KEY],
      });
      queryClient.invalidateQueries({
        queryKey: [MAINTENANCE_TICKET_KEY, variables.id],
      });
    },
    onError: () => {
      toast.error("Failed to change ticket status.");
    },
  });
};

export const useDeleteMaintenanceTicket = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      maintenanceTicketService.deleteMaintenanceTicket(id),
    onSuccess: (_, id) => {
      toast.success("Ticket deleted successfully.");
      queryClient.invalidateQueries({
        queryKey: [MAINTENANCE_TICKETS_KEY],
      });
      queryClient.removeQueries({
        queryKey: [MAINTENANCE_TICKET_KEY, id],
      });
    },
    onError: () => {
      toast.error("Failed to delete ticket.");
    },
  });
};

// Helper hook for common maintenance ticket operations
export const useMaintenanceTicketOperations = (ticketId?: string) => {
  const {
    data: ticket,
    isLoading,
    error,
  } = useMaintenanceTicket(ticketId ?? "");

  const { mutate: updateTicket, isPending: isUpdating } =
    useUpdateMaintenanceTicket();

  const { mutate: changeStatus, isPending: isChangingStatus } =
    useChangeMaintenanceTicketStatus();

  const { mutate: deleteTicket, isPending: isDeleting } =
    useDeleteMaintenanceTicket();

  const isProcessing = isUpdating || isChangingStatus || isDeleting;

  return {
    ticket,
    isLoading,
    error,
    isProcessing,
    updateTicket,
    changeStatus,
    deleteTicket,
  };
};
