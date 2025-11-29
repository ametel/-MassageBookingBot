import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard';
import { BookingsComponent } from './components/bookings/bookings';
import { ServicesComponent } from './components/services/services';
import { ClientsComponent } from './components/clients/clients';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'bookings', component: BookingsComponent },
  { path: 'services', component: ServicesComponent },
  { path: 'clients', component: ClientsComponent }
];
