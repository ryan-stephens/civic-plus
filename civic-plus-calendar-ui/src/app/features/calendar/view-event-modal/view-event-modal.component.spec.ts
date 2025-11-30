import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import { ViewEventModalComponent } from './view-event-modal.component';
import { CalendarEvent } from '../../../api/generated/api-client';

describe('ViewEventModalComponent', () => {
  let component: ViewEventModalComponent;
  let fixture: ComponentFixture<ViewEventModalComponent>;

  const mockEvent: CalendarEvent = {
    id: '1',
    title: 'Test Event',
    description: 'Test Description',
    startDate: new Date(2025, 11, 15, 9, 0),
    endDate: new Date(2025, 11, 15, 10, 0),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ViewEventModalComponent, CommonModule],
    }).compileComponents();

    fixture = TestBed.createComponent(ViewEventModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display event data when event is provided', () => {
    fixture.componentRef.setInput('event', mockEvent);
    fixture.detectChanges();
    expect(component.event()).toEqual(mockEvent);
  });

  it('should emit isVisibleChange when handleClose is called', () => {
    spyOn(component.isVisibleChange, 'emit');
    component.handleClose();
    expect(component.isVisibleChange.emit).toHaveBeenCalledWith(false);
  });

  it('should format date correctly', () => {
    const testDate = new Date(2025, 11, 15, 14, 30, 0);
    const formatted = component.formatDate(testDate);
    expect(formatted).toContain('2025');
    expect(formatted).toContain('15');
  });

  it('should handle string date in formatDate', () => {
    const dateString = '2025-12-15T14:30:00Z';
    const formatted = component.formatDate(dateString);
    expect(formatted).toBeTruthy();
    expect(formatted).not.toBe('N/A');
  });

  it('should return N/A for undefined date', () => {
    const formatted = component.formatDate(undefined);
    expect(formatted).toBe('N/A');
  });

  it('should return N/A for null date', () => {
    const formatted = component.formatDate(null as any);
    expect(formatted).toBe('N/A');
  });

  it('should expose isVisible as getter', () => {
    fixture.componentRef.setInput('isVisible', true);
    fixture.detectChanges();
    expect(component.isVisibleValue).toBe(true);
  });

  it('should expose isVisible as false by default', () => {
    expect(component.isVisibleValue).toBe(false);
  });
});
