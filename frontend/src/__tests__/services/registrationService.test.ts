import { describe, it, expect, vi, beforeEach } from 'vitest';
import { 
  registerForEvent, 
  cancelRegistration, 
  checkUserRegistration,
  getMyRegistrations 
} from '@/services/registrationService';
import { api } from '@/services/api';

// Mock the api module
vi.mock('@/services/api', () => ({
  api: {
    get: vi.fn(),
    post: vi.fn(),
    delete: vi.fn(),
  },
  getErrorMessage: vi.fn((error: any) => error.message || 'An error occurred'),
}));

describe('registrationService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getMyRegistrations', () => {
    it('should fetch user registrations successfully', async () => {
      const mockRegistrations = [
        {
          id: 1,
          eventId: 10,
          userId: 5,
          status: 'Confirmed',
          registeredAt: '2026-02-01T10:00:00Z',
        },
        {
          id: 2,
          eventId: 15,
          userId: 5,
          status: 'Confirmed',
          registeredAt: '2026-02-02T14:00:00Z',
        },
      ];

      vi.mocked(api.get).mockResolvedValue({ data: mockRegistrations });

      const result = await getMyRegistrations();

      expect(api.get).toHaveBeenCalledWith('/users/me/registrations');
      expect(result).toEqual(mockRegistrations);
    });
  });

  describe('checkUserRegistration', () => {
    it('should return isRegistered true when user is registered for event', async () => {
      const mockRegistrations = [
        {
          id: 1,
          eventId: 10,
          userId: 5,
          status: 'Confirmed',
          registeredAt: '2026-02-01T10:00:00Z',
        },
      ];

      vi.mocked(api.get).mockResolvedValue({ data: mockRegistrations });

      const result = await checkUserRegistration(10);

      expect(result.isRegistered).toBe(true);
      expect(result.registration).toEqual(mockRegistrations[0]);
    });

    it('should return isRegistered false when user is not registered for event', async () => {
      const mockRegistrations = [
        {
          id: 1,
          eventId: 10,
          userId: 5,
          status: 'Confirmed',
          registeredAt: '2026-02-01T10:00:00Z',
        },
      ];

      vi.mocked(api.get).mockResolvedValue({ data: mockRegistrations });

      const result = await checkUserRegistration(20);

      expect(result.isRegistered).toBe(false);
      expect(result.registration).toBeUndefined();
    });

    it('should return isRegistered false on API error', async () => {
      vi.mocked(api.get).mockRejectedValue(new Error('Unauthorized'));

      const result = await checkUserRegistration(10);

      expect(result.isRegistered).toBe(false);
      expect(result.registration).toBeUndefined();
    });
  });

  describe('registerForEvent', () => {
    it('should register user for event successfully', async () => {
      const mockResponse = {
        id: 1,
        eventId: 10,
        userId: 5,
        status: 'Confirmed',
        registeredAt: '2026-02-04T10:00:00Z',
      };

      vi.mocked(api.post).mockResolvedValue({ data: mockResponse });

      const result = await registerForEvent({ eventId: 10 });

      expect(api.post).toHaveBeenCalledWith('/events/10/register', { notes: undefined });
      expect(result).toEqual(mockResponse);
    });

    it('should register user with notes', async () => {
      const mockResponse = {
        id: 1,
        eventId: 10,
        userId: 5,
        status: 'Confirmed',
        registeredAt: '2026-02-04T10:00:00Z',
        notes: 'I can bring supplies',
      };

      vi.mocked(api.post).mockResolvedValue({ data: mockResponse });

      const result = await registerForEvent({ eventId: 10, notes: 'I can bring supplies' });

      expect(api.post).toHaveBeenCalledWith('/events/10/register', { notes: 'I can bring supplies' });
      expect(result).toEqual(mockResponse);
    });

    it('should throw error on registration failure', async () => {
      const errorMessage = 'Event is full';
      vi.mocked(api.post).mockRejectedValue({
        response: {
          data: { message: errorMessage },
        },
      });

      await expect(registerForEvent({ eventId: 10 })).rejects.toThrow();
    });
  });

  describe('cancelRegistration', () => {
    it('should cancel registration successfully', async () => {
      vi.mocked(api.delete).mockResolvedValue({});

      await cancelRegistration(10);

      expect(api.delete).toHaveBeenCalledWith('/events/10/register');
    });

    it('should throw error on cancellation failure', async () => {
      const errorMessage = 'Registration not found';
      vi.mocked(api.delete).mockRejectedValue({
        response: {
          data: { message: errorMessage },
        },
      });

      await expect(cancelRegistration(10)).rejects.toThrow();
    });
  });
});
