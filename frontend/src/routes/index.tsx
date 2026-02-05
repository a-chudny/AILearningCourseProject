import { Routes, Route } from 'react-router-dom'
import { Suspense, lazy } from 'react'
import { MainLayout } from '@/layouts/MainLayout'
import { AdminLayout } from '@/layouts/AdminLayout'
import { RoleGuard } from '@/components/RoleGuard'
import { UserRole } from '@/types/enums'

// Lazy load pages for code splitting
const HomePage = lazy(() => import('@/pages/HomePage'))
const NotFoundPage = lazy(() => import('@/pages/NotFoundPage'))
const LoginPage = lazy(() => import('@/pages/auth/LoginPage'))
const RegisterPage = lazy(() => import('@/pages/auth/RegisterPage'))
const EventListPage = lazy(() => import('@/pages/public/EventListPage'))
const EventDetailsPage = lazy(() => import('@/pages/public/EventDetailsPage'))
const CreateEventPage = lazy(() => import('@/pages/user/CreateEventPage'))
const EditEventPage = lazy(() => import('@/pages/user/EditEventPage'))
const MyEventsPage = lazy(() => import('@/pages/user/MyEventsPage'))
const ProfilePage = lazy(() => import('@/pages/user/ProfilePage'))
const AdminDashboardPage = lazy(() => import('@/pages/admin/AdminDashboardPage'))
const AdminUsersPage = lazy(() => import('@/pages/admin/AdminUsersPage'))
const AdminEventsPage = lazy(() => import('@/pages/admin/AdminEventsPage'))
const AdminReportsPage = lazy(() => import('@/pages/admin/AdminReportsPage'))

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
        {/* Main Layout Routes - pages with header and footer */}
        <Route
          path="/"
          element={
            <MainLayout>
              <HomePage />
            </MainLayout>
          }
        />
        <Route
          path="/events"
          element={
            <MainLayout>
              <EventListPage />
            </MainLayout>
          }
        />
        <Route
          path="/events/:id"
          element={
            <MainLayout>
              <EventDetailsPage />
            </MainLayout>
          }
        />

        {/* Protected User Routes */}
        <Route
          path="/events/create"
          element={
            <RoleGuard allowedRoles={[UserRole.Organizer, UserRole.Admin]}>
              <MainLayout>
                <CreateEventPage />
              </MainLayout>
            </RoleGuard>
          }
        />
        <Route
          path="/events/:id/edit"
          element={
            <RoleGuard allowedRoles={[UserRole.Organizer, UserRole.Admin]}>
              <MainLayout>
                <EditEventPage />
              </MainLayout>
            </RoleGuard>
          }
        />
        <Route
          path="/my-events"
          element={
            <RoleGuard allowedRoles={[UserRole.Volunteer, UserRole.Organizer, UserRole.Admin]}>
              <MainLayout>
                <MyEventsPage />
              </MainLayout>
            </RoleGuard>
          }
        />
        <Route
          path="/profile"
          element={
            <RoleGuard allowedRoles={[UserRole.Volunteer, UserRole.Organizer, UserRole.Admin]}>
              <MainLayout>
                <ProfilePage />
              </MainLayout>
            </RoleGuard>
          }
        />

        {/* Admin Routes - only accessible by Admin role */}
        <Route
          path="/admin"
          element={
            <RoleGuard allowedRoles={[UserRole.Admin]}>
              <AdminLayout>
                <AdminDashboardPage />
              </AdminLayout>
            </RoleGuard>
          }
        />
        <Route
          path="/admin/users"
          element={
            <RoleGuard allowedRoles={[UserRole.Admin]}>
              <AdminLayout>
                <AdminUsersPage />
              </AdminLayout>
            </RoleGuard>
          }
        />
        <Route
          path="/admin/events"
          element={
            <RoleGuard allowedRoles={[UserRole.Admin]}>
              <AdminLayout>
                <AdminEventsPage />
              </AdminLayout>
            </RoleGuard>
          }
        />
        <Route
          path="/admin/reports"
          element={
            <RoleGuard allowedRoles={[UserRole.Admin]}>
              <AdminLayout>
                <AdminReportsPage />
              </AdminLayout>
            </RoleGuard>
          }
        />

        {/* Auth pages use AuthLayout (no MainLayout) */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        {/* 404 page with MainLayout */}
        <Route
          path="*"
          element={
            <MainLayout>
              <NotFoundPage />
            </MainLayout>
          }
        />
      </Routes>
    </Suspense>
  )
}
