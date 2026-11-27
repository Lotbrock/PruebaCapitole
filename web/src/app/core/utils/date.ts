const pad = (n: number): string => String(n).padStart(2, '0');

/** ISO string -> value for <input type="datetime-local"> (local time). */
export function toDatetimeLocal(iso?: string | null): string {
  if (!iso) return '';
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return '';
  return (
    `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}` +
    `T${pad(date.getHours())}:${pad(date.getMinutes())}`
  );
}

/** Value of <input type="datetime-local"> (local time) -> ISO string in UTC. Empty if invalid. */
export function fromDatetimeLocal(value?: string | null): string {
  if (!value) return '';
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? '' : date.toISOString();
}
