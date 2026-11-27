import { DatePipe } from '@angular/common';
import { Component, computed, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { Observable, catchError, finalize, of, switchMap, tap } from 'rxjs';
import { ApiError } from '../../../core/errors/api-error';
import { BookingDetails } from '../../../core/models/booking.models';
import { BookingsApiService } from '../../../core/services/bookings-api.service';
import { ErrorAlertComponent } from '../../../shared/error-alert/error-alert.component';
import { StatusBadgeComponent } from '../../../shared/status-badge/status-badge.component';

@Component({
  selector: 'app-booking-details',
  imports: [DatePipe, RouterLink, StatusBadgeComponent, ErrorAlertComponent],
  templateUrl: './booking-details.component.html',
})
export class BookingDetailsComponent {
  private readonly api = inject(BookingsApiService);

  /** Route parameter ":id" (bound by withComponentInputBinding). */
  readonly id = input.required<string>();

  protected readonly booking = signal<BookingDetails | null>(null);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<ApiError | null>(null);

  protected readonly busy = signal(false);
  protected readonly actionError = signal<ApiError | null>(null);

  private readonly refresh = signal(0);

  protected readonly canConfirm = computed(() => this.booking()?.status === 'Pending');
  protected readonly canCancel = computed(() => {
    const status = this.booking()?.status;
    return status === 'Pending' || status === 'Confirmed';
  });

  constructor() {
    toObservable(computed(() => ({ id: this.id(), refresh: this.refresh() })))
      .pipe(
        tap(() => {
          this.loading.set(true);
          this.loadError.set(null);
        }),
        switchMap(({ id }) =>
          this.api.get(id).pipe(
            catchError((error: ApiError) => {
              this.loadError.set(error);
              return of(null);
            }),
          ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((booking) => {
        this.booking.set(booking);
        this.loading.set(false);
      });
  }

  protected confirm(): void {
    this.run(this.api.confirm(this.id()));
  }

  protected cancel(reason: string): void {
    this.run(this.api.cancel(this.id(), reason.trim() || undefined));
  }

  /** If the action fails the error is shown and the displayed booking is left untouched. */
  private run(action: Observable<unknown>): void {
    this.busy.set(true);
    this.actionError.set(null);

    action.pipe(finalize(() => this.busy.set(false))).subscribe({
      next: () => this.refresh.update((n) => n + 1),
      error: (error: ApiError) => this.actionError.set(error),
    });
  }
}
