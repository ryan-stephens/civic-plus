import { Component, input, output, signal, effect, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CreateEventRequest } from '../../../api/generated/api-client';

@Component({
  selector: 'app-add-event-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-event-modal.component.html',
  styleUrl: './add-event-modal.component.scss',
})
export class AddEventModalComponent {
  // Signal-based inputs and outputs
  isVisible = input<boolean>(false);
  selectedDate = input<Date | null>(null);
  isVisibleChange = output<boolean>();
  eventCreated = output<CreateEventRequest>();

  // Signals for component state
  isLoading = signal(false);
  form = signal<FormGroup | null>(null);

  // Expose isVisible as a getter for template binding
  get isVisibleValue(): boolean {
    return this.isVisible();
  }

  // Computed error state signals
  titleError = computed(() => {
    const control = this.form()?.get('title');
    return !!(control && control.invalid && control.touched);
  });

  descriptionError = computed(() => {
    const control = this.form()?.get('description');
    return !!(control && control.invalid && control.touched);
  });

  startDateError = computed(() => {
    const control = this.form()?.get('startDate');
    return !!(control && control.invalid && control.touched);
  });

  endDateError = computed(() => {
    const control = this.form()?.get('endDate');
    return !!(control && control.invalid && control.touched);
  });

  constructor(private fb: FormBuilder) {
    this.initializeForm();

    // Effect to update form when selectedDate changes
    effect(() => {
      const date = this.selectedDate();
      const currentForm = this.form();
      if (date && currentForm) {
        this.setDefaultStartDate(date);
      }
    });
  }

  private initializeForm(): void {
    const newForm = this.fb.group({
      title: ['', [Validators.required]],
      description: ['', [Validators.required]],
      startDate: [null, [Validators.required]],
      endDate: [null, [Validators.required]],
    });
    this.form.set(newForm);
  }

  private setDefaultStartDate(date: Date): void {
    // Set start date to selected date at 9:00 AM
    const defaultDate = new Date(date);
    defaultDate.setHours(9, 0, 0, 0);
    this.form()?.patchValue({
      startDate: this.formatDateForInput(defaultDate),
    });
  }

  private formatDateForInput(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  handleOk(): void {
    const currentForm = this.form();
    if (currentForm?.valid) {
      this.isLoading.set(true);
      const formValue = currentForm.value;
      const request: CreateEventRequest = {
        title: formValue.title,
        description: formValue.description,
        startDate: new Date(formValue.startDate),
        endDate: new Date(formValue.endDate),
      };
      this.eventCreated.emit(request);
    }
  }

  handleCancel(): void {
    this.isVisibleChange.emit(false);
    this.form()?.reset();
  }

  resetForm(): void {
    this.form()?.reset();
    this.isLoading.set(false);
  }
}
