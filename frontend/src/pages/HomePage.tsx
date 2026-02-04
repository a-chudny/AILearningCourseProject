import { Link } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { useEvents } from '@/hooks/useEvents';
import { EventCard } from '@/components/events/EventCard';
import { useQuery } from '@tanstack/react-query';
import { api } from '@/services/api';

interface StatisticsResponse {
  totalEvents: number;
  totalVolunteers: number;
  totalRegistrations: number;
}

async function fetchStatistics(): Promise<StatisticsResponse> {
  const response = await api.get<StatisticsResponse>('/statistics');
  return response.data;
}

export default function HomePage() {
  const { isAuthenticated } = useAuth();
  const { data: eventsData, isLoading: eventsLoading } = useEvents({ page: 1, pageSize: 4 });
  const { data: stats } = useQuery({
    queryKey: ['statistics'],
    queryFn: fetchStatistics,
    retry: false,
  });

  const features = [
    {
      icon: (
        <svg className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
          />
        </svg>
      ),
      title: 'Browse Events',
      description: 'Discover volunteer opportunities that match your interests and skills.',
    },
    {
      icon: (
        <svg className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z"
          />
        </svg>
      ),
      title: 'Register',
      description: 'Sign up quickly and easily to participate in events that matter to you.',
    },
    {
      icon: (
        <svg className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
          />
        </svg>
      ),
      title: 'Join Volunteers',
      description: 'Connect with like-minded people making a difference in your community.',
    },
  ];

  return (
    <div className="min-h-screen bg-gradient-to-br from-green-50 to-emerald-100">
      {/* Hero Section */}
      <section className="bg-gradient-to-r from-green-600 to-emerald-600 text-white">
        <div className="container mx-auto px-4 py-20 text-center">
          <h1 className="mb-6 text-5xl font-bold md:text-6xl">
            Make a Difference in Your Community
          </h1>
          <p className="mb-8 text-xl md:text-2xl opacity-90">
            Connect with meaningful volunteer opportunities and create lasting impact
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link
              to={isAuthenticated ? '/events' : '/register'}
              className="rounded-lg bg-white px-8 py-3 font-semibold text-green-600 transition-colors hover:bg-gray-100"
            >
              Get Started
            </Link>
            <Link
              to="/events"
              className="rounded-lg border-2 border-white bg-transparent px-8 py-3 font-semibold text-white transition-colors hover:bg-white hover:text-green-600"
            >
              Browse Events
            </Link>
          </div>
        </div>
      </section>

      {/* Statistics Section */}
      {stats && (
        <section className="py-12 bg-white border-b border-gray-200">
          <div className="container mx-auto px-4">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
              <div className="text-center">
                <div className="text-4xl font-bold text-green-600">{stats.totalEvents}</div>
                <div className="mt-2 text-gray-600">Active Events</div>
              </div>
              <div className="text-center">
                <div className="text-4xl font-bold text-green-600">{stats.totalVolunteers}</div>
                <div className="mt-2 text-gray-600">Registered Volunteers</div>
              </div>
              <div className="text-center">
                <div className="text-4xl font-bold text-green-600">{stats.totalRegistrations}</div>
                <div className="mt-2 text-gray-600">Total Registrations</div>
              </div>
            </div>
          </div>
        </section>
      )}

      {/* How It Works Section */}
      <section className="py-16">
        <div className="container mx-auto px-4">
          <h2 className="mb-12 text-center text-4xl font-bold text-gray-900">How It Works</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {features.map((feature, index) => (
              <div
                key={index}
                className="rounded-lg bg-white p-6 shadow-md transition-shadow hover:shadow-lg"
              >
                <div className="mb-4 inline-flex rounded-full bg-green-100 p-3 text-green-600">
                  {feature.icon}
                </div>
                <h3 className="mb-3 text-xl font-semibold text-gray-900">{feature.title}</h3>
                <p className="text-gray-600">{feature.description}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Featured Events Section */}
      <section className="py-16 bg-white">
        <div className="container mx-auto px-4">
          <div className="mb-8 flex items-center justify-between">
            <h2 className="text-4xl font-bold text-gray-900">Upcoming Events</h2>
            <Link
              to="/events"
              className="text-green-600 font-semibold hover:text-green-700 transition-colors"
            >
              View All 
            </Link>
          </div>

          {eventsLoading && (
            <div className="flex items-center justify-center py-12">
              <div className="h-8 w-8 animate-spin rounded-full border-4 border-green-600 border-t-transparent" />
            </div>
          )}

          {eventsData && eventsData.events.length > 0 ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
              {eventsData.events.slice(0, 4).map((event) => (
                <EventCard key={event.id} event={event} />
              ))}
            </div>
          ) : (
            !eventsLoading && (
              <div className="rounded-lg bg-gray-50 p-12 text-center">
                <p className="text-gray-600">No upcoming events available at the moment.</p>
                {isAuthenticated && (
                  <Link
                    to="/events/create"
                    className="mt-4 inline-block rounded-lg bg-green-600 px-6 py-2 font-semibold text-white transition-colors hover:bg-green-700"
                  >
                    Create First Event
                  </Link>
                )}
              </div>
            )
          )}
        </div>
      </section>

      {/* Call to Action Section */}
      {!isAuthenticated && (
        <section className="bg-gradient-to-r from-green-600 to-emerald-600 text-white py-16">
          <div className="container mx-auto px-4 text-center">
            <h2 className="mb-4 text-3xl font-bold">Ready to Make an Impact?</h2>
            <p className="mb-8 text-xl opacity-90">
              Join our community of volunteers and start making a difference today
            </p>
            <Link
              to="/register"
              className="inline-block rounded-lg bg-white px-8 py-3 font-semibold text-green-600 transition-colors hover:bg-gray-100"
            >
              Sign Up Now
            </Link>
          </div>
        </section>
      )}
    </div>
  );
}
