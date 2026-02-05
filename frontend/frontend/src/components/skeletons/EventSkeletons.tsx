// Skeleton components for loading states
export function EventCardSkeleton() {
  return (
    <div className="animate-pulse rounded-lg border border-gray-200 bg-white shadow-sm">
      <div className="flex flex-col h-full">
        {/* Image placeholder */}
        <div className="h-48 bg-gray-200 rounded-t-lg" />
        
        {/* Content placeholder */}
        <div className="p-4 space-y-3">
          {/* Title */}
          <div className="h-6 bg-gray-200 rounded w-3/4" />
          
          {/* Date & Time */}
          <div className="h-4 bg-gray-200 rounded w-1/2" />
          
          {/* Location */}
          <div className="h-4 bg-gray-200 rounded w-2/3" />
          
          {/* Skills badges */}
          <div className="flex gap-2">
            <div className="h-6 bg-gray-200 rounded-full w-16" />
            <div className="h-6 bg-gray-200 rounded-full w-20" />
            <div className="h-6 bg-gray-200 rounded-full w-16" />
          </div>
          
          {/* Capacity */}
          <div className="h-4 bg-gray-200 rounded w-1/3" />
        </div>
      </div>
    </div>
  );
}

export function EventDetailsSkeleton() {
  return (
    <div className="animate-pulse max-w-4xl mx-auto">
      {/* Image */}
      <div className="h-96 bg-gray-200 rounded-lg mb-8" />
      
      {/* Title */}
      <div className="h-10 bg-gray-200 rounded w-2/3 mb-4" />
      
      {/* Meta info */}
      <div className="space-y-3 mb-8">
        <div className="h-6 bg-gray-200 rounded w-1/2" />
        <div className="h-6 bg-gray-200 rounded w-1/3" />
        <div className="h-6 bg-gray-200 rounded w-2/5" />
      </div>
      
      {/* Description */}
      <div className="space-y-2 mb-8">
        <div className="h-4 bg-gray-200 rounded w-full" />
        <div className="h-4 bg-gray-200 rounded w-full" />
        <div className="h-4 bg-gray-200 rounded w-3/4" />
      </div>
      
      {/* Buttons */}
      <div className="flex gap-4">
        <div className="h-12 bg-gray-200 rounded w-40" />
        <div className="h-12 bg-gray-200 rounded w-32" />
      </div>
    </div>
  );
}

export function RegistrationCardSkeleton() {
  return (
    <div className="animate-pulse rounded-lg border border-gray-200 bg-white p-6">
      <div className="flex justify-between items-start mb-4">
        <div className="h-7 bg-gray-200 rounded w-1/2" />
        <div className="h-6 bg-gray-200 rounded-full w-24" />
      </div>
      
      <div className="space-y-3">
        <div className="h-5 bg-gray-200 rounded w-1/3" />
        <div className="h-5 bg-gray-200 rounded w-2/5" />
        <div className="h-5 bg-gray-200 rounded w-1/4" />
      </div>
    </div>
  );
}
