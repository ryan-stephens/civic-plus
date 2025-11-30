import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { CalendarEvents } from './calendar-events';
import { CalendarEventsService } from '../../../services/calendar-events.service';
import { CalendarEvent, CreateEventRequest } from '../../../api/generated/api-client';

describe('CalendarEvents', () => {
  let component: CalendarEvents;
  let fixture: ComponentFixture<CalendarEvents>;
  let calendarEventsService: jasmine.SpyObj<CalendarEventsService>;

  const mockEvents: CalendarEvent[] = [
    {
      id: '1',
      title: 'Event 1',
      description: 'Description 1',
      startDate: new Date(2025, 11, 15, 9, 0),
      endDate: new Date(2025, 11, 15, 10, 0),
    },
    {
      id: '2',
      title: 'Event 2',
      description: 'Description 2',
      startDate: new Date(2025, 11, 20, 14, 0),
      endDate: new Date(2025, 11, 20, 15, 0),
    },
  ];

  beforeEach(async () => {
    const calendarEventsServiceSpy = jasmine.createSpyObj('CalendarEventsService', [
      'getEvents',
      'createEvent',
    ]);

    await TestBed.configureTestingModule({
      imports: [CalendarEvents],
      providers: [{ provide: CalendarEventsService, useValue: calendarEventsServiceSpy }],
    }).compileComponents();

    calendarEventsService = TestBed.inject(
      CalendarEventsService,
    ) as jasmine.SpyObj<CalendarEventsService>;
    fixture = TestBed.createComponent(CalendarEvents);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Initialization', () => {
    it('should initialize with empty events', () => {
      expect(component.events()).toEqual([]);
    });

    it('should initialize with loading false', () => {
      expect(component.isLoading()).toBeFalsy();
    });

    it('should initialize with no error', () => {
      expect(component.error()).toBeNull();
    });

    it('should initialize with add modal hidden', () => {
      expect(component.addEventModalVisible()).toBeFalsy();
    });

    it('should initialize with view modal hidden', () => {
      expect(component.viewEventModalVisible()).toBeFalsy();
    });

    it('should call loadEvents on ngOnInit', () => {
      calendarEventsService.getEvents.and.returnValue(of({ items: mockEvents }));
      component.ngOnInit();
      expect(calendarEventsService.getEvents).toHaveBeenCalled();
    });
  });

  describe('loadEvents', () => {
    it('should load events successfully', (done) => {
      calendarEventsService.getEvents.and.returnValue(of({ items: mockEvents }));
      component.loadEvents();

      setTimeout(() => {
        expect(component.events().length).toBe(2);
        expect(component.isLoading()).toBeFalsy();
        expect(component.error()).toBeNull();
        done();
      }, 100);
    });

    it('should sort events by start date', (done) => {
      const unsortedEvents = [mockEvents[1], mockEvents[0]];
      calendarEventsService.getEvents.and.returnValue(of({ items: unsortedEvents }));
      component.loadEvents();

      setTimeout(() => {
        const events = component.events();
        expect(events[0].id).toBe('1');
        expect(events[1].id).toBe('2');
        done();
      }, 100);
    });

    it('should handle empty events list', (done) => {
      calendarEventsService.getEvents.and.returnValue(of({ items: [] }));
      component.loadEvents();

      setTimeout(() => {
        expect(component.events().length).toBe(0);
        expect(component.isLoading()).toBeFalsy();
        done();
      }, 100);
    });

    it('should handle error when loading events', (done) => {
      calendarEventsService.getEvents.and.returnValue(throwError(() => new Error('API Error')));
      component.loadEvents();

      setTimeout(() => {
        expect(component.error()).toBe('Failed to load events. Please try again.');
        expect(component.isLoading()).toBeFalsy();
        done();
      }, 100);
    });
  });

  describe('Date Selection', () => {
    it('should open add modal when date in current month is selected', () => {
      const testDate = new Date(2025, 11, 15);
      component.selectedMonth.set(new Date(2025, 11, 1));
      component.onDateSelect(testDate);

      expect(component.selectedDate()).toEqual(testDate);
      expect(component.addEventModalVisible()).toBeTruthy();
    });

    it('should update selected month when date in different month is selected', () => {
      const testDate = new Date(2025, 10, 15);
      component.selectedMonth.set(new Date(2025, 11, 1));
      component.onDateSelect(testDate);

      expect(component.selectedMonth()).toEqual(testDate);
      expect(component.addEventModalVisible()).toBeFalsy();
    });

    it('should update selected month on month change', () => {
      const newMonth = new Date(2025, 10, 1);
      component.onMonthChange({ date: newMonth, mode: 'month' });

      expect(component.selectedMonth()).toEqual(newMonth);
    });
  });

  describe('Event Click', () => {
    it('should open view modal when event is clicked', () => {
      const testEvent = mockEvents[0];
      const mockEvent = new Event('click');
      spyOn(mockEvent, 'stopPropagation');

      component.onEventClick(testEvent, mockEvent);

      expect(mockEvent.stopPropagation).toHaveBeenCalled();
      expect(component.selectedEvent()).toEqual(testEvent);
      expect(component.viewEventModalVisible()).toBeTruthy();
    });
  });

  describe('Modal Management', () => {
    it('should close add modal and reset form', () => {
      component.addEventModalVisible.set(true);
      component.selectedDate.set(new Date());
      component.onAddEventModalClose(false);

      expect(component.addEventModalVisible()).toBeFalsy();
      expect(component.selectedDate()).toBeNull();
    });

    it('should close view modal and clear selected event', () => {
      component.viewEventModalVisible.set(true);
      component.selectedEvent.set(mockEvents[0]);
      component.onViewEventModalClose(false);

      expect(component.viewEventModalVisible()).toBeFalsy();
      expect(component.selectedEvent()).toBeNull();
    });
  });

  describe('Event Creation', () => {
    it('should create event and add to list', (done) => {
      calendarEventsService.getEvents.and.returnValue(of({ items: [] }));
      component.loadEvents();

      const newEvent: CalendarEvent = {
        id: '3',
        title: 'New Event',
        description: 'New Description',
        startDate: new Date(2025, 11, 25, 11, 0),
        endDate: new Date(2025, 11, 25, 12, 0),
      };

      calendarEventsService.createEvent.and.returnValue(of(newEvent));

      const createRequest: CreateEventRequest = {
        title: 'New Event',
        description: 'New Description',
        startDate: new Date(2025, 11, 25, 11, 0),
        endDate: new Date(2025, 11, 25, 12, 0),
      };

      component.onEventCreated(createRequest);

      setTimeout(() => {
        expect(component.events().length).toBe(1);
        expect(component.events()[0].id).toBe('3');
        expect(component.addEventModalVisible()).toBeFalsy();
        done();
      }, 100);
    });

    it('should handle error when creating event', (done) => {
      calendarEventsService.createEvent.and.returnValue(throwError(() => new Error('API Error')));

      const createRequest: CreateEventRequest = {
        title: 'New Event',
        description: 'New Description',
        startDate: new Date(2025, 11, 25, 11, 0),
        endDate: new Date(2025, 11, 25, 12, 0),
      };

      component.onEventCreated(createRequest);

      setTimeout(() => {
        expect(component.error()).toBe('Failed to create event. Please try again.');
        done();
      }, 100);
    });
  });

  describe('Computed Signals', () => {
    it('should compute events map correctly', () => {
      component.events.set(mockEvents);
      const eventsMap = component.eventsMap();

      expect(eventsMap.size).toBeGreaterThan(0);
    });

    it('should compute filtered events for current month', () => {
      component.events.set(mockEvents);
      component.selectedMonth.set(new Date(2025, 11, 1));
      const filtered = component.filteredEvents();

      expect(filtered.length).toBe(2);
    });

    it('should return empty filtered events for month with no events', () => {
      component.events.set(mockEvents);
      component.selectedMonth.set(new Date(2025, 0, 1));
      const filtered = component.filteredEvents();

      expect(filtered.length).toBe(0);
    });
  });

  describe('Helper Methods', () => {
    it('should get events for specific date', () => {
      component.events.set(mockEvents);
      const testDate = new Date(2025, 11, 15);
      const eventsForDate = component.getEventsForDate(testDate);

      expect(eventsForDate.length).toBe(1);
      expect(eventsForDate[0].id).toBe('1');
    });

    it('should return empty array for date with no events', () => {
      component.events.set(mockEvents);
      const testDate = new Date(2025, 11, 10);
      const eventsForDate = component.getEventsForDate(testDate);

      expect(eventsForDate.length).toBe(0);
    });
  });
});
