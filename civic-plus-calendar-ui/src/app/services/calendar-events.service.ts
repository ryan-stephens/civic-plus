import { Injectable } from '@angular/core';
import {
  Client,
  CalendarEvent,
  CreateEventRequest,
  CalendarEventResponse,
} from '../api/generated/api-client';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CalendarEventsService {
  constructor(private apiClient: Client) {}

  /**
   * Fetch all calendar events
   */
  getEvents(top: number = 100, skip: number = 0): Observable<CalendarEventResponse> {
    return this.apiClient.calendarEventsGET(top, skip);
  }

  /**
   * Fetch a specific event by ID
   */
  getEventById(id: string): Observable<CalendarEvent> {
    return this.apiClient.calendarEventsGET2(id);
  }

  /**
   * Create a new calendar event
   */
  createEvent(request: CreateEventRequest): Observable<CalendarEvent> {
    return this.apiClient.calendarEventsPOST(request);
  }
}
