import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: '/calendar-events' },
  {
    path: 'calendar-events',
    loadChildren: () =>
      import('./features/calendar/calendar.routes').then((m) => m.CALENDAR_EVENT_ROUTES),
  },
];
