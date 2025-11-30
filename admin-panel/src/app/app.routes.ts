import { Routes } from '@angular/router';
import { Dashboard } from './components/dashboard/dashboard';
import { Bookings } from './components/bookings/bookings';
import { Services } from './components/services/services';
import { Clients } from './components/clients/clients';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: Dashboard },
  { path: 'bookings', component: Bookings },
  { path: 'services', component: Services },
  { path: 'clients', component: Clients },
];
