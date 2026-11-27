import { AbstractControl, ValidationErrors } from '@angular/forms';
import { fromDatetimeLocal } from '../../../core/utils/date';

/** Same rule as the server: Start must be in the future. */
export function futureDate(control: AbstractControl): ValidationErrors | null {
  const iso = fromDatetimeLocal(control.value);
  if (!iso) return null; // "required" reports the empty case
  return new Date(iso).getTime() > Date.now() ? null : { pastDate: true };
}

/** Same rule as the server: Start must be before End. */
export function startBeforeEnd(group: AbstractControl): ValidationErrors | null {
  const start = fromDatetimeLocal(group.get('start')?.value);
  const end = fromDatetimeLocal(group.get('end')?.value);
  if (!start || !end) return null;
  return new Date(start).getTime() < new Date(end).getTime() ? null : { rangeInvalid: true };
}
