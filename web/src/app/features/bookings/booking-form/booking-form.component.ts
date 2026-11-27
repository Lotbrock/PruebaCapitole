import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiError } from '../../../core/errors/api-error';
import { BookingsApiService } from '../../../core/services/bookings-api.service';
import { fromDatetimeLocal } from '../../../core/utils/date';
import { ErrorAlertComponent } from '../../../shared/error-alert/error-alert.component';
import { futureDate, startBeforeEnd } from './booking-form.validators';

type FieldName = 'resourceId' | 'customerName' | 'customerEmail' | 'start' | 'end';

/** Domain error codes that belong to a single input. */
const CODE_TO_FIELD: Record<string, FieldName> = {
  'resource.id_required': 'resourceId',
  'customer.name_required': 'customerName',
  'customer.name_too_long': 'customerName',
  'customer.email_invalid': 'customerEmail',
  'booking.start_in_past': 'start',
  'booking.invalid_range': 'end',
};

@Component({
  selector: 'app-booking-form',
  imports: [ReactiveFormsModule, RouterLink, ErrorAlertComponent],
  templateUrl: './booking-form.component.html',
})
export class BookingFormComponent {
  private readonly api = inject(BookingsApiService);
  private readonly router = inject(Router);
  private readonly fb = inject(NonNullableFormBuilder);

  // Same rules as the server (the server remains the source of truth).
  protected readonly form = this.fb.group(
    {
      resourceId: ['', [Validators.required, Validators.pattern(/\S/)]],
      customerName: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(100)]],
      customerEmail: ['', [Validators.required, Validators.pattern(/^[^@\s]+@[^@\s]+\.[^@\s]+$/)]],
      start: ['', [Validators.required, futureDate]],
      end: ['', Validators.required],
    },
    { validators: startBeforeEnd },
  );

  protected readonly submitting = signal(false);
  /** Errors that do not belong to any input. */
  protected readonly formError = signal<ApiError | null>(null);

  protected submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    const value = this.form.getRawValue();
    this.submitting.set(true);
    this.formError.set(null);

    this.api
      .create({
        resourceId: value.resourceId.trim(),
        customerName: value.customerName.trim(),
        customerEmail: value.customerEmail.trim(),
        start: fromDatetimeLocal(value.start),
        end: fromDatetimeLocal(value.end),
      })
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: ({ id }) => void this.router.navigate(['/bookings', id]),
        error: (error: ApiError) => this.showServerError(error),
      });
  }

  /** Message to show under an input, or null when it is valid. */
  protected errorText(name: FieldName): string | null {
    const control = this.form.controls[name];
    const errors = control.errors;
    if (!errors || !(control.touched || control.dirty)) {
      return name === 'end' && this.rangeInvalid() ? 'La fecha de fin debe ser posterior al inicio.' : null;
    }

    if (errors['server']) return String(errors['server']);
    if (errors['required']) return 'Campo obligatorio.';
    if (errors['pattern']) {
      return name === 'customerEmail' ? 'El email no es válido.' : 'Campo obligatorio.';
    }
    if (errors['maxlength']) return `Máximo ${errors['maxlength'].requiredLength} caracteres.`;
    if (errors['pastDate']) return 'La fecha de inicio debe ser futura.';
    return null;
  }

  private rangeInvalid(): boolean {
    const { start, end } = this.form.controls;
    return this.form.hasError('rangeInvalid') && (start.touched || end.touched);
  }

  /** Paints the server errors on the right input; anything else goes to the alert. */
  private showServerError(error: ApiError): void {
    let placed = false;

    for (const [key, messages] of Object.entries(error.fieldErrors)) {
      const name = (Object.keys(this.form.controls) as FieldName[]).find(
        (field) => field.toLowerCase() === key.toLowerCase(),
      );
      if (name) {
        this.setServerError(name, messages.join(' '));
        placed = true;
      }
    }

    const codeField = error.code ? CODE_TO_FIELD[error.code] : undefined;
    if (codeField) {
      this.setServerError(codeField, error.message);
      placed = true;
    }

    if (!placed) this.formError.set(error);
  }

  private setServerError(name: FieldName, message: string): void {
    const control = this.form.controls[name];
    control.setErrors({ server: message });
    control.markAsTouched();
  }
}
