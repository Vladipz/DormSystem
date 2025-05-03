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
  Textarea
} from "@/components/ui";
import { useAuth } from "@/lib/hooks/useAuth";
import { useCreateMaintenanceTicket } from "@/lib/hooks/useMaintenanceTicket";
import { MaintenancePriority } from "@/lib/types/maintenanceTicket";
import { RoomsResponse } from "@/lib/types/room";
import { zodResolver } from "@hookform/resolvers/zod";
import { Plus } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";
import { DialogClose } from "../ui/dialog";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "../ui/form";

// Type-safe priority values
const PRIORITIES: MaintenancePriority[] = ["Low", "Medium", "High"];

// Schema for the form validation
const formSchema = z.object({
  roomId: z.string().min(1, "Please select a room"),
  description: z.string().min(5, "Description is too short"),
  priority: z.enum(["Low", "Medium", "High"]),
});

// Type for the form values
type FormValues = z.infer<typeof formSchema>;

interface CreateTicketDialogProps {
  rooms: RoomsResponse[];
}

export function CreateTicketDialog({ rooms }: CreateTicketDialogProps) {
  const { userId } = useAuth();
  const { mutate: createTicket, isPending } = useCreateMaintenanceTicket();
  const [open, setOpen] = useState(false);

  // Initialize form with default values
  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      roomId: "",
      description: "",
      priority: "Medium",
    },
  });

  // Form submission handler
  const onSubmit = form.handleSubmit((data) => {
    createTicket(
      {
        roomId: data.roomId,
        title: data.description.slice(0, 40) || "Maintenance issue",
        description: data.description,
        reporterById: userId || "",
        priority: data.priority,
      },
      {
        onSuccess: () => {
          toast.success("Ticket created successfully");
          setOpen(false);
          form.reset();
        },
        onError: () => toast.error("Failed to create ticket"),
      }
    );
  });

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>
          <Plus className="mr-2 h-4 w-4" /> Report issue
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Report maintenance issue</DialogTitle>
          <DialogDescription>Fill the form below to report a maintenance issue.</DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={onSubmit} className="space-y-4 py-4">
            <FormField
              control={form.control}
              name="roomId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Room</FormLabel>
                  <Select onValueChange={field.onChange} value={field.value}>
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Select a room" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent className="z-[1001]">
                      {rooms.map((room) => (
                        <SelectItem key={room.id} value={room.id}>
                          Room {room.label}
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
                    <SelectContent className="z-[1001]">
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
            <DialogFooter>
              <DialogClose asChild>
                <Button type="button" variant="outline">
                  Cancel
                </Button>
              </DialogClose>
              <Button type="submit" disabled={isPending}>
                {isPending ? "Submitting..." : "Submit"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
