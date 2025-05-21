import { PlaceService } from "@/lib/services/placeService";
import type {
    CreatePlaceRequest,
    GetAvailablePlacesParams,
    GetPlacesParams,
    MoveInRequest,
    MoveOutRequest,
    UpdatePlaceRequest
} from "@/lib/types/place";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

// Query keys for React Query
const PLACES_KEY = "places";
const PLACE_KEY = "place";
const AVAILABLE_PLACES_KEY = "availablePlaces";

/**
 * Hook to fetch a paginated list of places with optional filters
 */
export const usePlaces = (params?: GetPlacesParams) => {
  return useQuery({
    queryKey: [PLACES_KEY, params],
    queryFn: () => PlaceService.getPlaces(params),
  });
};

/**
 * Hook to fetch a single place by ID
 */
export const usePlace = (id: string) => {
  return useQuery({
    queryKey: [PLACE_KEY, id],
    queryFn: () => PlaceService.getPlaceById(id),
    enabled: !!id, // Only run query if ID is provided
  });
};

/**
 * Hook to fetch available places
 */
export const useAvailablePlaces = (params?: GetAvailablePlacesParams) => {
  return useQuery({
    queryKey: [AVAILABLE_PLACES_KEY, params],
    queryFn: () => PlaceService.getAvailablePlaces(params),
  });
};

/**
 * Hook to create a new place
 */
export const useCreatePlace = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreatePlaceRequest) => PlaceService.createPlace(data),
    onSuccess: () => {
      toast.success("Place created successfully.");
      queryClient.invalidateQueries({
        queryKey: [PLACES_KEY],
      });
      queryClient.invalidateQueries({
        queryKey: [AVAILABLE_PLACES_KEY],
      });
    },
    onError: () => {
      toast.error("Failed to create place.");
    },
  });
};

/**
 * Hook to update an existing place
 */
export const useUpdatePlace = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, ...data }: UpdatePlaceRequest & { id: string }) =>
      PlaceService.updatePlace(id, data),
    onSuccess: (_, variables) => {
      toast.success("Place updated successfully.");
      queryClient.invalidateQueries({
        queryKey: [PLACES_KEY],
      });
      queryClient.invalidateQueries({
        queryKey: [PLACE_KEY, variables.id],
      });
    },
    onError: () => {
      toast.error("Failed to update place.");
    },
  });
};

/**
 * Hook to move a user into a place
 */
export const useMoveIn = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, ...data }: MoveInRequest & { id: string }) =>
      PlaceService.moveIn(id, data),
    onSuccess: (_, variables) => {
      toast.success("User moved in successfully.");
      queryClient.invalidateQueries({
        queryKey: [PLACES_KEY],
      });
      queryClient.invalidateQueries({
        queryKey: [PLACE_KEY, variables.id],
      });
      queryClient.invalidateQueries({
        queryKey: [AVAILABLE_PLACES_KEY],
      });
    },
    onError: () => {
      toast.error("Failed to move user in.");
    },
  });
};

/**
 * Hook to move a user out of a place
 */
export const useMoveOut = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, ...data }: MoveOutRequest & { id: string }) =>
      PlaceService.moveOut(id, data),
    onSuccess: (_, variables) => {
      toast.success("User moved out successfully.");
      queryClient.invalidateQueries({
        queryKey: [PLACES_KEY],
      });
      queryClient.invalidateQueries({
        queryKey: [PLACE_KEY, variables.id],
      });
      queryClient.invalidateQueries({
        queryKey: [AVAILABLE_PLACES_KEY],
      });
    },
    onError: () => {
      toast.error("Failed to move user out.");
    },
  });
};

/**
 * Hook to delete a place
 */
export const useDeletePlace = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => PlaceService.deletePlace(id),
    onSuccess: (_, id) => {
      toast.success("Place deleted successfully.");
      queryClient.invalidateQueries({
        queryKey: [PLACES_KEY],
      });
      queryClient.invalidateQueries({
        queryKey: [AVAILABLE_PLACES_KEY],
      });
      queryClient.removeQueries({
        queryKey: [PLACE_KEY, id],
      });
    },
    onError: () => {
      toast.error("Failed to delete place.");
    },
  });
};

/**
 * Helper hook combining common place operations
 */
export const usePlaceOperations = (placeId?: string) => {
  const {
    data: place,
    isLoading,
    error,
  } = usePlace(placeId ?? "");

  const { mutate: updatePlace, isPending: isUpdating } = useUpdatePlace();
  const { mutate: moveInUser, isPending: isMovingIn } = useMoveIn();
  const { mutate: moveOutUser, isPending: isMovingOut } = useMoveOut();
  const { mutate: deletePlace, isPending: isDeleting } = useDeletePlace();

  const isProcessing = isUpdating || isMovingIn || isMovingOut || isDeleting;

  return {
    place,
    isLoading,
    error,
    isProcessing,
    updatePlace,
    moveInUser,
    moveOutUser,
    deletePlace,
  };
};