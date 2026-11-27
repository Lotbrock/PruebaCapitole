import { HttpErrorResponse } from '@angular/common/http';

/** Shape of an RFC 7807 ProblemDetails with the "code" and "errors" extensions. */
interface ProblemDetails {
  title?: string;
  detail?: string;
  code?: string;
  errors?: Record<string, string[]>;
}

/** The only error type the UI knows: components never see HttpErrorResponse. */
export class ApiError {
  constructor(
    readonly status: number,
    readonly title: string,
    readonly detail?: string,
    /** Domain error code, for example "booking.overlap". */
    readonly code?: string,
    /** Validation messages by field name. */
    readonly fieldErrors: Record<string, string[]> = {},
  ) {}

  get message(): string {
    return this.detail ?? this.title;
  }
}

const DEFAULT_TITLES: Record<number, string> = {
  400: 'Los datos enviados no son válidos.',
  404: 'No se encontró lo que buscas.',
  409: 'La operación entra en conflicto con otra reserva.',
  422: 'Esa operación no está permitida.',
  500: 'Error del servidor. Inténtalo de nuevo más tarde.',
};

export function toApiError(error: HttpErrorResponse): ApiError {
  if (error.status === 0) {
    return new ApiError(0, 'No se pudo conectar con el servidor.');
  }

  const body: ProblemDetails =
    typeof error.error === 'object' && error.error !== null ? error.error : {};

  return new ApiError(
    error.status,
    body.title ?? DEFAULT_TITLES[error.status] ?? 'Ocurrió un error inesperado.',
    body.detail,
    body.code,
    body.errors ?? {},
  );
}
