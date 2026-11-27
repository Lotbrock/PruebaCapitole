import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api/api-config';
import {
  BookingDetails,
  BookingFilters,
  BookingListItem,
  CreateBookingRequest,
  PagedResult,
} from '../models/booking.models';

/** Data layer: the only place that talks to the API. Components never use HttpClient. */
@Injectable({ providedIn: 'root' })
export class BookingsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${inject(API_BASE_URL)}/bookings`;

  list(filters: BookingFilters): Observable<PagedResult<BookingListItem>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<PagedResult<BookingListItem>>(this.baseUrl, { params });
  }

  get(id: string): Observable<BookingDetails> {
    return this.http.get<BookingDetails>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateBookingRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.baseUrl, request);
  }

  confirm(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/confirm`, null);
  }

  cancel(id: string, reason?: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/cancel`, { reason: reason ?? null });
  }
}
