import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'bookings' },
  {
    path: 'bookings',
    title: 'Reservas',
    loadComponent: () =>
      import('./features/bookings/booking-list/booking-list.component').then((m) => m.BookingListComponent),
  },
  {
    // Must go before ":id", otherwise "new" would be read as an id.
    path: 'bookings/new',
    title: 'Nueva reserva',
    loadComponent: () =>
      import('./features/bookings/booking-form/booking-form.component').then((m) => m.BookingFormComponent),
  },
  {
    path: 'bookings/:id',
    title: 'Detalle de la reserva',
    loadComponent: () =>
      import('./features/bookings/booking-details/booking-details.component').then(
        (m) => m.BookingDetailsComponent,
      ),
  },
  { path: '**', redirectTo: 'bookings' },
];
