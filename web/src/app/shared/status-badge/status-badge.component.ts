import { Component, computed, input } from '@angular/core';
import { BookingStatus } from '../../core/models/booking.models';

const LABELS: Record<BookingStatus, string> = {
  Pending: 'Pendiente',
  Confirmed: 'Confirmada',
  Cancelled: 'Cancelada',
};

const STYLES: Record<BookingStatus, string> = {
  Pending: 'text-bg-warning',
  Confirmed: 'text-bg-success',
  Cancelled: 'text-bg-secondary',
};

@Component({
  selector: 'app-status-badge',
  template: `<span class="badge" [class]="style()">{{ label() }}</span>`,
})
export class StatusBadgeComponent {
  readonly status = input.required<BookingStatus>();

  protected readonly label = computed(() => LABELS[this.status()] ?? this.status());
  protected readonly style = computed(() => STYLES[this.status()] ?? 'text-bg-light');
}
