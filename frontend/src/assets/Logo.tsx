export function Logo({ className = "text-blue-600", size = 64 }: { className?: string; size?: number }) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 64 64"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className={className}
      aria-label="Volunteer Event Portal Logo"
    >
      {/* Heart shape representing volunteering/care */}
      <path
        d="M32 54C32 54 8 38 8 22C8 16.4 12.4 12 18 12C22 12 25.5 14.5 27 18C27 18 27 18 27 18C28.5 14.5 32 12 36 12C41.6 12 46 16.4 46 22C46 38 32 54 32 54Z"
        fill="currentColor"
        className="text-blue-600"
      />
      {/* Hands joining/helping symbol */}
      <path
        d="M20 28L24 32L28 28M36 28L40 32L44 28"
        stroke="white"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      {/* Circle border */}
      <circle
        cx="32"
        cy="32"
        r="30"
        stroke="currentColor"
        strokeWidth="2"
        className="text-blue-600"
      />
    </svg>
  )
}
