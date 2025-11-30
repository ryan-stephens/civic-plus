import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CalendarEvent } from '../../../api/generated/api-client';

@Component({
  selector: 'app-view-event-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './view-event-modal.component.html',
  styleUrl: './view-event-modal.component.scss',
})
export class ViewEventModalComponent {
  // Signal-based inputs and outputs
  isVisible = input<boolean>(false);
  event = input<CalendarEvent | null>(null);
  isVisibleChange = output<boolean>();

  // Expose isVisible as a getter for template binding
  get isVisibleValue(): boolean {
    return this.isVisible();
  }

  handleClose(): void {
    this.isVisibleChange.emit(false);
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    return dateObj.toLocaleString();
  }
}
