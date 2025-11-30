import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Booking {
  id: number;
  userId: number;
  userName: string;
  serviceId: number;
  serviceName: string;
  bookingDateTime: string;
  status: string;
  notes?: string;
}

export interface Service {
  id: number;
  name: string;
  description: string;
  price: number;
  durationMinutes: number;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) { }

  // Bookings
  getBookings(userId?: number, fromDate?: string, toDate?: string): Observable<Booking[]> {
    let params: any = {};
    if (userId) params.userId = userId;
    if (fromDate) params.fromDate = fromDate;
    if (toDate) params.toDate = toDate;
    
    return this.http.get<Booking[]>(`${this.apiUrl}/bookings`, { params });
  }

  cancelBooking(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/bookings/${id}`);
  }

  // Services
  getServices(activeOnly: boolean = true): Observable<Service[]> {
    return this.http.get<Service[]>(`${this.apiUrl}/services`, { params: { activeOnly } });
  }
}
