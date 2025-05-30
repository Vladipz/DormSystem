import {
    Button,
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
    Textarea,
} from "@/components/ui";
import {
    useChangeMaintenanceTicketStatus,
    useUpdateMaintenanceTicket,
} from "@/lib/hooks/useMaintenanceTicket";
import {
    MaintenancePriority,
    MaintenanceStatus,
    MaintenanceTicketResponse,
} from "@/lib/types/maintenanceTicket";
import { zodResolver } from "@hookform/resolvers/zod";
import { Edit } from "lucide-react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "../ui/form";

// Type-safe status and priority values
const STATUSES: MaintenanceStatus[] = ["Open", "InProgress", "Resolved"];
const PRIORITIES: MaintenancePriority[] = ["Low", "Medium", "High"];

// Schema for the form validation
const formSchema = z.object({
  title: z.string().min(1, "Title is required"),
  description: z.string().min(5, "Description is too short"),
  priority: z.enum(["Low", "Medium", "High"]),
  status: z.enum(["Open", "InProgress", "Resolved"]),
});

// Type for the form values
type FormValues = z.infer<typeof formSchema>;

interface EditTicketDialogProps {
  ticket: MaintenanceTicketResponse;
  trigger?: React.ReactNode;
  open?: boolean;
  onOpenChange?: (open: boolean) => void;
}

export function EditTicketDialog({
  ticket,
  trigger,
  open: controlledOpen,
  onOpenChange,
}: EditTicketDialogProps) {
  const { mutate: updateTicket, isPending: isUpdating } =
    useUpdateMaintenanceTicket();
  const { mutate: changeStatus, isPending: isChangingStatus } =
    useChangeMaintenanceTicketStatus();
  const [uncontrolledOpen, setUncontrolledOpen] = useState(false);

  const isPending = isUpdating || isChangingStatus;

  // Use controlled or uncontrolled state
  const isControlled = controlledOpen !== undefined;
  const open = isControlled ? controlledOpen : uncontrolledOpen;
  const setOpen = isControlled ? onOpenChange! : setUncontrolledOpen;

  // Initialize form with ticket data
  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      title: ticket.title,
      description: ticket.description,
      priority: ticket.priority,
      status: ticket.status,
    },
  });

  // Reset form when dialog opens or ticket changes
  useEffect(() => {
    if (open) {
      form.reset({
        title: ticket.title,
        description: ticket.description,
        priority: ticket.priority,
        status: ticket.status,
      });
    }
  }, [ticket, open, form]);

  // Form submission handler
  const onSubmit = form.handleSubmit(async (data) => {
    const hasStatusChanged = data.status !== ticket.status;
    const hasOtherFieldsChanged =
      data.title !== ticket.title ||
      data.description !== ticket.description ||
      data.priority !== ticket.priority;

    try {
      // Handle status change separately if it changed
      if (hasStatusChanged) {
        await new Promise((resolve, reject) => {
          changeStatus(
            {
              id: ticket.id,
              ticketId: ticket.id,
              newStatus: data.status,
            },
            {
              onSuccess: resolve,
              onError: reject,
            },
          );
        });
      }

      // Handle other field updates
      if (hasOtherFieldsChanged) {
        await new Promise((resolve, reject) => {
          updateTicket(
            {
              id: ticket.id,
              ticketId: ticket.id,
              title: data.title,
              description: data.description,
              priority: data.priority,
              assignedToId: ticket.assignedTo?.id || null,
            },
            {
              onSuccess: resolve,
              onError: reject,
            },
          );
        });
      }

      toast.success("Ticket updated successfully");
      setOpen(false);
    } catch (error) {
      toast.error("Failed to update ticket");
    }
  });

  const handleCancel = () => {
    form.reset();
    setOpen(false);
  };

  const dialogContent = (
    <>
      <DialogHeader>
        <DialogTitle>Edit Maintenance Ticket</DialogTitle>
        <DialogDescription>
          Update the ticket details and status. Room: {ticket.room.label}
        </DialogDescription>
      </DialogHeader>
      <Form {...form}>
        <form onSubmit={onSubmit} className="space-y-4 py-4">
          <FormField
            control={form.control}
            name="title"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Title</FormLabel>
                <FormControl>
                  <Textarea
                    rows={2}
                    placeholder="Enter ticket title"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="description"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Description</FormLabel>
                <FormControl>
                  <Textarea
                    rows={3}
                    placeholder="Describe the issue"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <div className="grid grid-cols-2 gap-4">
            <FormField
              control={form.control}
              name="priority"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Priority</FormLabel>
                  <Select onValueChange={field.onChange} value={field.value}>
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {PRIORITIES.map((priority) => (
                        <SelectItem key={priority} value={priority}>
                          {priority}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="status"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Status</FormLabel>
                  <Select onValueChange={field.onChange} value={field.value}>
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {STATUSES.map((status) => (
                        <SelectItem key={status} value={status}>
                          {status === "InProgress" ? "In Progress" : status}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={handleCancel}
              disabled={isPending}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={isPending}>
              {isPending ? "Updating..." : "Update Ticket"}
            </Button>
          </DialogFooter>
        </form>
      </Form>
    </>
  );

  // If controlled, render without DialogTrigger
  if (isControlled) {
    return (
      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent className="sm:max-w-[425px]">
          {dialogContent}
        </DialogContent>
      </Dialog>
    );
  }

  // If uncontrolled, render with DialogTrigger
  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        {trigger ? (
          trigger
        ) : (
          <Button size="sm" variant="outline">
            <Edit className="mr-1 h-3 w-3" /> Edit
          </Button>
        )}
      </DialogTrigger>
      <DialogContent className="sm:max-w-[425px]">
        {dialogContent}
      </DialogContent>
    </Dialog>
  );
}
