import { describe, it, expect, beforeEach, vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Bookings } from './bookings';
import { ApiService } from '../../services/api.service';
import { of, throwError } from 'rxjs';
import { ChangeDetectorRef } from '@angular/core';

describe('Bookings Component', () => {
  let component: Bookings;
  let fixture: ComponentFixture<Bookings>;
  let mockApiService: any;

  beforeEach(async () => {
    mockApiService = {
      getBookings: vi.fn().mockReturnValue(of([])),
      cancelBooking: vi.fn().mockReturnValue(of(null)),
    };

    await TestBed.configureTestingModule({
      imports: [Bookings],
      providers: [{ provide: ApiService, useValue: mockApiService }, ChangeDetectorRef],
    }).compileComponents();

    fixture = TestBed.createComponent(Bookings);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load bookings on init', () => {
    const mockBookings = [
      {
        id: 1,
        userName: 'John Doe',
        serviceName: 'Massage',
        bookingDateTime: '2025-12-01T10:00:00',
        status: 1,
      },
      {
        id: 2,
        userName: 'Jane Smith',
        serviceName: 'Therapy',
        bookingDateTime: '2025-12-02T14:00:00',
        status: 1,
      },
    ];

    mockApiService.getBookings.mockReturnValue(of(mockBookings));

    component.ngOnInit();

    expect(mockApiService.getBookings).toHaveBeenCalled();
  });

  it('should handle error when loading bookings fails', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockApiService.getBookings.mockReturnValue(throwError(() => new Error('API Error')));

    component.loadBookings();

    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should get correct status text', () => {
    expect(component.getStatusText(0)).toBe('Pending');
    expect(component.getStatusText(1)).toBe('Confirmed');
    expect(component.getStatusText(2)).toBe('Cancelled');
    expect(component.getStatusText(3)).toBe('Completed');
  });

  it('should identify cancelled bookings', () => {
    expect(component.isCancelled(2)).toBe(true);
    expect(component.isCancelled(1)).toBe(false);
  });

  it('should cancel booking with confirmation', () => {
    const mockBookingId = 1;
    vi.spyOn(window, 'confirm').mockReturnValue(true);

    component.cancelBooking(mockBookingId);

    expect(mockApiService.cancelBooking).toHaveBeenCalledWith(mockBookingId);
  });
});
