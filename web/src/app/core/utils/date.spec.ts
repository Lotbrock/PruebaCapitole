import { fromDatetimeLocal, toDatetimeLocal } from './date';

describe('date utils', () => {
  it('round-trips a local date-time through ISO', () => {
    const local = '2030-05-20T10:30';

    expect(toDatetimeLocal(fromDatetimeLocal(local))).toBe(local);
  });

  it('returns an empty string for empty or invalid values', () => {
    expect(fromDatetimeLocal('')).toBe('');
    expect(fromDatetimeLocal('not a date')).toBe('');
    expect(toDatetimeLocal(undefined)).toBe('');
  });
});
