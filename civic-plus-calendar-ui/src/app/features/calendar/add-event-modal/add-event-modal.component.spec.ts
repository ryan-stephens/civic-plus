import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AddEventModalComponent } from './add-event-modal.component';
import { CreateEventRequest } from '../../../api/generated/api-client';

describe('AddEventModalComponent', () => {
  let component: AddEventModalComponent;
  let fixture: ComponentFixture<AddEventModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddEventModalComponent, ReactiveFormsModule, CommonModule],
    }).compileComponents();

    fixture = TestBed.createComponent(AddEventModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with empty values', () => {
    expect(component.form()).toBeDefined();
    expect(component.form()?.get('title')?.value).toBe('');
    expect(component.form()?.get('description')?.value).toBe('');
    expect(component.form()?.get('startDate')?.value).toBeNull();
    expect(component.form()?.get('endDate')?.value).toBeNull();
  });

  it('should have invalid form when fields are empty', () => {
    expect(component.form()?.valid).toBeFalsy();
  });

  it('should have valid form when all fields are filled', () => {
    component.form()?.patchValue({
      title: 'Test Event',
      description: 'Test Description',
      startDate: '2025-12-01T09:00',
      endDate: '2025-12-01T10:00',
    });
    expect(component.form()?.valid).toBeTruthy();
  });

  it('should show title error when title is touched and empty', () => {
    const titleControl = component.form()?.get('title');
    titleControl?.markAsTouched();
    titleControl?.updateValueAndValidity();
    expect(titleControl?.invalid && titleControl?.touched).toBeTruthy();
  });

  it('should show description error when description is touched and empty', () => {
    const descControl = component.form()?.get('description');
    descControl?.markAsTouched();
    descControl?.updateValueAndValidity();
    expect(descControl?.invalid && descControl?.touched).toBeTruthy();
  });

  it('should show startDate error when startDate is touched and empty', () => {
    const startDateControl = component.form()?.get('startDate');
    startDateControl?.markAsTouched();
    startDateControl?.updateValueAndValidity();
    expect(startDateControl?.invalid && startDateControl?.touched).toBeTruthy();
  });

  it('should show endDate error when endDate is touched and empty', () => {
    const endDateControl = component.form()?.get('endDate');
    endDateControl?.markAsTouched();
    endDateControl?.updateValueAndValidity();
    expect(endDateControl?.invalid && endDateControl?.touched).toBeTruthy();
  });

  it('should emit eventCreated when handleOk is called with valid form', () => {
    spyOn(component.eventCreated, 'emit');
    component.form()?.patchValue({
      title: 'Test Event',
      description: 'Test Description',
      startDate: '2025-12-01T09:00',
      endDate: '2025-12-01T10:00',
    });

    component.handleOk();

    expect(component.eventCreated.emit).toHaveBeenCalled();
    expect(component.isLoading()).toBeTruthy();
  });

  it('should not emit eventCreated when handleOk is called with invalid form', () => {
    spyOn(component.eventCreated, 'emit');
    component.handleOk();
    expect(component.eventCreated.emit).not.toHaveBeenCalled();
  });

  it('should emit isVisibleChange when handleCancel is called', () => {
    spyOn(component.isVisibleChange, 'emit');
    component.handleCancel();
    expect(component.isVisibleChange.emit).toHaveBeenCalledWith(false);
  });

  it('should reset form when resetForm is called', () => {
    component.form()?.patchValue({
      title: 'Test Event',
      description: 'Test Description',
    });
    component.resetForm();
    expect(component.form()?.get('title')?.value).toBeNull();
    expect(component.form()?.get('description')?.value).toBeNull();
    expect(component.isLoading()).toBeFalsy();
  });

  it('should set default start date to selected date at 9:00 AM', () => {
    const testDate = new Date(2025, 11, 15); // December 15, 2025
    component['setDefaultStartDate'](testDate);
    const startDateValue = component.form()?.get('startDate')?.value;
    expect(startDateValue).toContain('2025-12-15T09:00');
  });

  it('should format date correctly for input', () => {
    const testDate = new Date(2025, 11, 15, 14, 30, 0);
    const formatted = component['formatDateForInput'](testDate);
    expect(formatted).toBe('2025-12-15T14:30');
  });
});
