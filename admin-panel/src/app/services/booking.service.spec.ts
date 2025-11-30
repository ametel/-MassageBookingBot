import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { BookingService } from './booking.service';

describe('BookingService', () => {
  let service: BookingService;
  let httpMock: HttpTestingController;
  const apiUrl = 'http://localhost:5000/api';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [BookingService],
    });

    service = TestBed.inject(BookingService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch bookings', () => {
    const mockBookings = [
      { id: 1, userName: 'John', serviceName: 'Massage' },
      { id: 2, userName: 'Jane', serviceName: 'Therapy' },
    ];

    service.getBookings().subscribe((bookings) => {
      expect(bookings.length).toBe(2);
      expect(bookings).toEqual(mockBookings);
    });

    const req = httpMock.expectOne(`${apiUrl}/bookings`);
    expect(req.request.method).toBe('GET');
    req.flush(mockBookings);
  });

  it('should handle error when fetching bookings fails', () => {
    service.getBookings().subscribe({
      next: () => fail('should have failed'),
      error: (error) => {
        expect(error).toBeTruthy();
      },
    });

    const req = httpMock.expectOne(`${apiUrl}/bookings`);
    req.flush('Error', { status: 500, statusText: 'Server Error' });
  });

  it('should cancel booking', () => {
    const bookingId = 1;

    service.cancelBooking(bookingId).subscribe((response) => {
      expect(response).toBeNull();
    });

    const req = httpMock.expectOne(`${apiUrl}/bookings/${bookingId}`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
