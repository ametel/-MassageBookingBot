import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  totalBookings = 0;
  activeServices = 0;
  totalClients = 0;
  todaysAppointments = 0;
  loading = true;

  constructor(private apiService: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.loading = true;
    console.log('Loading dashboard data...');

    // Load bookings
    this.apiService.getBookings().subscribe({
      next: (bookings) => {
        this.totalBookings = bookings.length;

        // Count unique clients
        const uniqueClients = new Set(bookings.map((b) => b.userId));
        this.totalClients = uniqueClients.size;

        // Count today's appointments
        const today = new Date().toDateString();
        this.todaysAppointments = bookings.filter(
          (b) => new Date(b.bookingDateTime).toDateString() === today
        ).length;

        console.log('Dashboard bookings loaded:', this.totalBookings);
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error loading bookings:', err),
    });

    // Load services
    this.apiService.getServices(true).subscribe({
      next: (services) => {
        this.activeServices = services.length;
        this.loading = false;
        console.log('Dashboard services loaded:', this.activeServices);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading services:', err);
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }
}
