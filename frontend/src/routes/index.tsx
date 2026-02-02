import { Routes, Route } from 'react-router-dom'
import { Suspense, lazy } from 'react'

// Lazy load pages for code splitting
const HomePage = lazy(() => import('@/pages/HomePage'))
const NotFoundPage = lazy(() => import('@/pages/NotFoundPage'))

// Loading fallback component
function PageLoader() {
  return (
    <div className="flex min-h-screen items-center justify-center">
      <div className="flex flex-col items-center gap-4">
        <div className="h-12 w-12 animate-spin rounded-full border-4 border-blue-500 border-t-transparent" />
        <p className="text-gray-600">Loading...</p>
      </div>
    </div>
  )
}

export function AppRoutes() {
  return (
    <Suspense fallback={<PageLoader />}>
      <Routes>
        <Route path="/" element={<HomePage />} />
        {/* Add more routes here as features are implemented */}
        {/* <Route path="/events" element={<EventsPage />} /> */}
        {/* <Route path="/events/:id" element={<EventDetailPage />} /> */}
        {/* <Route path="/login" element={<LoginPage />} /> */}
        {/* <Route path="/register" element={<RegisterPage />} /> */}
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </Suspense>
  )
}
