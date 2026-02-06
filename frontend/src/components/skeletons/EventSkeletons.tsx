export function EventCardSkeleton() {
  return (
    <div className="animate-pulse rounded-lg border border-gray-200 bg-white shadow-sm">
      <div className="h-48 bg-gray-200 rounded-t-lg" />
      <div className="p-4 space-y-3">
        <div className="h-6 bg-gray-200 rounded w-3/4" />
        <div className="h-4 bg-gray-200 rounded w-1/2" />
        <div className="h-4 bg-gray-200 rounded w-2/3" />
        <div className="flex gap-2 mt-4">
          <div className="h-6 bg-gray-200 rounded w-16" />
          <div className="h-6 bg-gray-200 rounded w-16" />
        </div>
        <div className="mt-4 h-10 bg-gray-200 rounded" />
      </div>
    </div>
  );
}

export function EventDetailsSkeleton() {
  return (
    <div className="animate-pulse space-y-6">
      {/* Image skeleton */}
      <div className="h-64 bg-gray-200 rounded-lg" />

      {/* Title skeleton */}
      <div className="h-8 bg-gray-200 rounded w-3/4" />

      {/* Meta info skeleton */}
      <div className="flex gap-4">
        <div className="h-5 bg-gray-200 rounded w-32" />
        <div className="h-5 bg-gray-200 rounded w-32" />
        <div className="h-5 bg-gray-200 rounded w-32" />
      </div>

      {/* Description skeleton */}
      <div className="space-y-3">
        <div className="h-4 bg-gray-200 rounded w-full" />
        <div className="h-4 bg-gray-200 rounded w-full" />
        <div className="h-4 bg-gray-200 rounded w-2/3" />
      </div>

      {/* Skills skeleton */}
      <div className="flex gap-2">
        <div className="h-6 bg-gray-200 rounded w-20" />
        <div className="h-6 bg-gray-200 rounded w-20" />
        <div className="h-6 bg-gray-200 rounded w-20" />
      </div>

      {/* Button skeleton */}
      <div className="h-12 bg-gray-200 rounded w-full" />
    </div>
  );
}

export function RegistrationCardSkeleton() {
  return (
    <div className="animate-pulse rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
      <div className="flex items-start justify-between">
        <div className="flex-1 space-y-3">
          <div className="h-6 bg-gray-200 rounded w-3/4" />
          <div className="h-4 bg-gray-200 rounded w-1/2" />
          <div className="h-4 bg-gray-200 rounded w-2/3" />
        </div>
        <div className="h-6 bg-gray-200 rounded w-24" />
      </div>
      <div className="mt-4 flex gap-3">
        <div className="h-10 bg-gray-200 rounded flex-1" />
        <div className="h-10 bg-gray-200 rounded w-24" />
      </div>
    </div>
  );
}
