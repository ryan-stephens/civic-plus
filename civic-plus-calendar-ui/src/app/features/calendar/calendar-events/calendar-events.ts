import { Component, OnInit, ViewChild, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NzCalendarModule } from 'ng-zorro-antd/calendar';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzListModule } from 'ng-zorro-antd/list';
import { NzEmptyModule } from 'ng-zorro-antd/empty';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzSpaceModule } from 'ng-zorro-antd/space';
import { NzGridModule } from 'ng-zorro-antd/grid';
import { NzBadgeModule } from 'ng-zorro-antd/badge';
import { CalendarEventsService } from '../../../services/calendar-events.service';
import { CalendarEvent, CreateEventRequest } from '../../../api/generated/api-client';
import { AddEventModalComponent } from '../add-event-modal/add-event-modal.component';
import { ViewEventModalComponent } from '../view-event-modal/view-event-modal.component';

@Component({
  selector: 'app-calendar-events',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NzCalendarModule,
    NzCardModule,
    NzListModule,
    NzEmptyModule,
    NzSpinModule,
    NzAlertModule,
    NzTagModule,
    NzButtonModule,
    NzSpaceModule,
    NzGridModule,
    NzBadgeModule,
    AddEventModalComponent,
    ViewEventModalComponent,
  ],
  templateUrl: './calendar-events.html',
  styleUrl: './calendar-events.scss',
})
export class CalendarEvents implements OnInit {
  @ViewChild(AddEventModalComponent) addEventModal!: AddEventModalComponent;
  @ViewChild(ViewEventModalComponent) viewEventModal!: ViewEventModalComponent;

  // Signal-based state management
  events = signal<CalendarEvent[]>([]);
  isLoading = signal(false);
  error = signal<string | null>(null);
  selectedDate = signal<Date | null>(null);
  addEventModalVisible = signal(false);
  selectedMonth = signal(new Date());
  viewEventModalVisible = signal(false);
  selectedEvent = signal<CalendarEvent | null>(null);

  // Computed signals
  eventsMap = computed(() => {
    const map = new Map<string, CalendarEvent[]>();
    this.events().forEach((event) => {
      if (event.startDate) {
        const dateKey = this.getDateKey(new Date(event.startDate));
        if (!map.has(dateKey)) {
          map.set(dateKey, []);
        }
        map.get(dateKey)!.push(event);
      }
    });
    return map;
  });

  filteredEvents = computed(() => {
    const year = this.selectedMonth().getFullYear();
    const month = this.selectedMonth().getMonth();
    return this.events().filter((event) => {
      if (!event.startDate) return false;
      const eventDate = new Date(event.startDate);
      return eventDate.getFullYear() === year && eventDate.getMonth() === month;
    });
  });

  constructor(private calendarEventsService: CalendarEventsService) {}

  ngOnInit(): void {
    this.loadEvents();
  }

  loadEvents(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.calendarEventsService.getEvents().subscribe({
      next: (response) => {
        const sortedEvents = (response.items || [])
          .map((event) => ({
            ...event,
            startDate: event.startDate ? new Date(event.startDate) : undefined,
            endDate: event.endDate ? new Date(event.endDate) : undefined,
          }))
          .sort((a, b) => {
            const dateA = a.startDate?.getTime() || 0;
            const dateB = b.startDate?.getTime() || 0;
            return dateA - dateB;
          });
        this.events.set(sortedEvents);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading events:', err);
        this.error.set('Failed to load events. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  private getDateKey(date: Date): string {
    return date.toISOString().split('T')[0];
  }

  onDateSelect(date: Date): void {
    const selectedYear = date.getFullYear();
    const selectedMonth = date.getMonth();
    const currentYear = this.selectedMonth().getFullYear();
    const currentMonth = this.selectedMonth().getMonth();

    if (selectedYear !== currentYear || selectedMonth !== currentMonth) {
      this.selectedMonth.set(date);
      return;
    }

    this.selectedDate.set(date);
    this.addEventModalVisible.set(true);
  }

  onMonthChange(event: { date: Date; mode: string }): void {
    this.selectedMonth.set(event.date);
  }

  onEventClick(event: CalendarEvent, e: Event): void {
    e.stopPropagation();
    this.selectedEvent.set(event);
    this.viewEventModalVisible.set(true);
  }

  onAddEventModalClose(visible: boolean): void {
    this.addEventModalVisible.set(visible);
    if (!visible) {
      this.selectedDate.set(null);
      this.addEventModal?.resetForm();
    }
  }

  onViewEventModalClose(visible: boolean): void {
    this.viewEventModalVisible.set(visible);
    if (!visible) {
      this.selectedEvent.set(null);
    }
  }

  onEventCreated(request: CreateEventRequest): void {
    this.calendarEventsService.createEvent(request).subscribe({
      next: (createdEvent) => {
        const eventWithDates = {
          ...createdEvent,
          startDate: createdEvent.startDate ? new Date(createdEvent.startDate) : undefined,
          endDate: createdEvent.endDate ? new Date(createdEvent.endDate) : undefined,
        };
        const updatedEvents = [...this.events(), eventWithDates].sort((a, b) => {
          const dateA = a.startDate?.getTime() || 0;
          const dateB = b.startDate?.getTime() || 0;
          return dateA - dateB;
        });
        this.events.set(updatedEvents);
        this.addEventModalVisible.set(false);
        this.addEventModal?.resetForm();
        this.selectedDate.set(null);
      },
      error: (err) => {
        console.error('Error creating event:', err);
        this.error.set('Failed to create event. Please try again.');
      },
    });
  }

  getEventsForDate(date: Date): CalendarEvent[] {
    const dateKey = this.getDateKey(date);
    return this.eventsMap().get(dateKey) || [];
  }
}
