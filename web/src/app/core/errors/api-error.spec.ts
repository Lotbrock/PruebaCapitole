import { HttpErrorResponse } from '@angular/common/http';
import { toApiError } from './api-error';

describe('toApiError', () => {
  it('reads code, detail and field errors from a ProblemDetails', () => {
    const response = new HttpErrorResponse({
      status: 409,
      error: { title: 'Conflict', detail: 'Ya existe una reserva confirmada.', code: 'booking.overlap' },
    });

    const error = toApiError(response);

    expect(error.status).toBe(409);
    expect(error.code).toBe('booking.overlap');
    expect(error.message).toBe('Ya existe una reserva confirmada.');
  });

  it('keeps the validation errors by field', () => {
    const response = new HttpErrorResponse({
      status: 400,
      error: { title: 'Validation', errors: { customerEmail: ['Email inválido'] } },
    });

    expect(toApiError(response).fieldErrors['customerEmail']).toEqual(['Email inválido']);
  });

  it('gives a readable message when the server is unreachable', () => {
    const error = toApiError(new HttpErrorResponse({ status: 0 }));

    expect(error.status).toBe(0);
    expect(error.message).toContain('No se pudo conectar');
  });

  it('falls back to a default message when the body is not a ProblemDetails', () => {
    const error = toApiError(new HttpErrorResponse({ status: 404, error: null }));

    expect(error.title).toBe('No se encontró lo que buscas.');
  });
});
