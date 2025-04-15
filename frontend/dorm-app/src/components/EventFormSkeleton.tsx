import { Skeleton } from "@/components/ui/skeleton";

export function EventFormSkeleton() {
  return (
    <div className="space-y-6 mt-4">
      {/* Form field skeletons */}
      {[1, 2, 3, 4, 5].map((i) => (
        <div key={i} className="space-y-2">
          <Skeleton className="h-4 w-24" />
          <Skeleton className="h-10 w-full" />
        </div>
      ))}

      {/* Switch field skeleton */}
      <div className="flex items-center gap-2">
        <Skeleton className="h-5 w-10" />
        <div className="space-y-1">
          <Skeleton className="h-4 w-36" />
          <Skeleton className="h-3 w-48" />
        </div>
      </div>

      {/* Buttons skeleton */}
      <div className="flex justify-end gap-3 pt-4">
        <Skeleton className="h-9 w-24" />
        <Skeleton className="h-9 w-32" />
      </div>
    </div>
  );
}

