export type BookingStatus = 'Pending' | 'Confirmed' | 'Cancelled';
export type BookingSortBy = 'Start' | 'End' | 'CreatedAt';
export type SortDirection = 'Asc' | 'Desc';

export interface BookingListItem {
  id: string;
  resourceId: string;
  customerName: string;
  customerEmail: string;
  start: string;
  end: string;
  status: BookingStatus;
  createdAt: string;
}

export interface BookingDetails extends BookingListItem {
  confirmedAt?: string;
  cancelledAt?: string;
  cancellationReason?: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface BookingFilters {
  resourceId?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
  sortBy?: BookingSortBy;
  sortDirection?: SortDirection;
}

export interface CreateBookingRequest {
  resourceId: string;
  customerName: string;
  customerEmail: string;
  start: string;
  end: string;
}
