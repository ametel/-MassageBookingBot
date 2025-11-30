import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService, Booking } from '../../services/api.service';

@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './bookings.html',
  styleUrl: './bookings.css',
})
export class Bookings implements OnInit {
  bookings: Booking[] = [];
  loading = true;

  constructor(private apiService: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadBookings();
  }

  loadBookings() {
    this.loading = true;
    console.log('Loading bookings...');
    this.apiService.getBookings().subscribe({
      next: (data) => {
        console.log('Bookings loaded:', data);
        this.bookings = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading bookings:', err);
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  cancelBooking(id: number) {
    if (confirm('Are you sure you want to cancel this booking?')) {
      this.apiService.cancelBooking(id).subscribe({
        next: () => {
          this.loadBookings();
        },
        error: (err) => console.error('Error canceling booking:', err),
      });
    }
  }

  getStatusText(status: number): string {
    const statuses = ['Pending', 'Confirmed', 'Cancelled', 'Completed'];
    return statuses[status] || 'Unknown';
  }

  isCancelled(status: number): boolean {
    return status === 2; // Cancelled = 2
  }
}
