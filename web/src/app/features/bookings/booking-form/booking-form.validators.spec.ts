import { FormControl, FormGroup } from '@angular/forms';
import { futureDate, startBeforeEnd } from './booking-form.validators';

const pad = (n: number) => String(n).padStart(2, '0');
const local = (date: Date) =>
  `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
const daysFromNow = (days: number) => local(new Date(Date.now() + days * 24 * 60 * 60 * 1000));

describe('booking form validators', () => {
  describe('futureDate', () => {
    it('accepts a date in the future', () => {
      expect(futureDate(new FormControl(daysFromNow(2)))).toBeNull();
    });

    it('rejects a date in the past', () => {
      expect(futureDate(new FormControl(daysFromNow(-2)))).toEqual({ pastDate: true });
    });

    it('ignores an empty value (required reports it)', () => {
      expect(futureDate(new FormControl(''))).toBeNull();
    });
  });

  describe('startBeforeEnd', () => {
    const group = (start: string, end: string) =>
      new FormGroup({ start: new FormControl(start), end: new FormControl(end) });

    it('accepts Start before End', () => {
      expect(startBeforeEnd(group(daysFromNow(2), daysFromNow(3)))).toBeNull();
    });

    it('rejects Start equal to End', () => {
      const same = daysFromNow(2);
      expect(startBeforeEnd(group(same, same))).toEqual({ rangeInvalid: true });
    });

    it('rejects Start after End', () => {
      expect(startBeforeEnd(group(daysFromNow(3), daysFromNow(2)))).toEqual({ rangeInvalid: true });
    });
  });
});
