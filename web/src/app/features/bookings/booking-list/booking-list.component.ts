import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, ParamMap, Params, Router, RouterLink } from '@angular/router';
import { catchError, of, switchMap, tap } from 'rxjs';
import { ApiError } from '../../../core/errors/api-error';
import {
  BookingFilters,
  BookingListItem,
  BookingSortBy,
  PagedResult,
  SortDirection,
} from '../../../core/models/booking.models';
import { BookingsApiService } from '../../../core/services/bookings-api.service';
import { fromDatetimeLocal, toDatetimeLocal } from '../../../core/utils/date';
import { ErrorAlertComponent } from '../../../shared/error-alert/error-alert.component';
import { GenericTableComponent } from '../../../shared/generic-table/generic-table.component';
import { StatusBadgeComponent } from '../../../shared/status-badge/status-badge.component';

const DEFAULT_PAGE_SIZE = 20;

/** The URL is the source of truth for the filters: reloading the page keeps them. */
function filtersFromParams(params: ParamMap): BookingFilters {
  return {
    resourceId: params.get('resourceId') || undefined,
    from: params.get('from') || undefined,
    to: params.get('to') || undefined,
    page: Number(params.get('page')) || 1,
    pageSize: Number(params.get('pageSize')) || DEFAULT_PAGE_SIZE,
    sortBy: (params.get('sortBy') as BookingSortBy | null) || undefined,
    sortDirection: (params.get('sortDirection') as SortDirection | null) || undefined,
  };
}

@Component({
  selector: 'app-booking-list',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    RouterLink,
    GenericTableComponent,
    StatusBadgeComponent,
    ErrorAlertComponent,
  ],
  templateUrl: './booking-list.component.html',
})
export class BookingListComponent {
  private readonly api = inject(BookingsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly sortOptions: { value: BookingSortBy; label: string }[] = [
    { value: 'Start', label: 'Inicio' },
    { value: 'End', label: 'Fin' },
    { value: 'CreatedAt', label: 'Creación' },
  ];
  protected readonly pageSizes = [10, 20, 50];

  protected readonly filterForm = this.fb.group({
    resourceId: '',
    from: '',
    to: '',
    sortBy: 'Start' as BookingSortBy,
    sortDirection: 'Asc' as SortDirection,
    pageSize: DEFAULT_PAGE_SIZE,
  });

  protected readonly result = signal<PagedResult<BookingListItem> | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<ApiError | null>(null);

  protected readonly items = computed(() => this.result()?.items ?? []);
  protected readonly totalPages = computed(() => {
    const result = this.result();
    return result ? Math.max(1, Math.ceil(result.totalCount / result.pageSize)) : 1;
  });

  constructor() {
    this.route.queryParamMap
      .pipe(
        tap((params) => {
          this.filterForm.patchValue({
            resourceId: params.get('resourceId') ?? '',
            from: toDatetimeLocal(params.get('from')),
            to: toDatetimeLocal(params.get('to')),
            sortBy: (params.get('sortBy') as BookingSortBy | null) ?? 'Start',
            sortDirection: (params.get('sortDirection') as SortDirection | null) ?? 'Asc',
            pageSize: Number(params.get('pageSize')) || DEFAULT_PAGE_SIZE,
          });
          this.loading.set(true);
          this.error.set(null);
        }),
        switchMap((params) =>
          this.api.list(filtersFromParams(params)).pipe(
            catchError((error: ApiError) => {
              this.error.set(error);
              return of(null);
            }),
          ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((result) => {
        this.result.set(result);
        this.loading.set(false);
      });
  }

  protected applyFilters(): void {
    const value = this.filterForm.getRawValue();
    this.navigate({
      resourceId: value.resourceId.trim() || null,
      from: fromDatetimeLocal(value.from) || null,
      to: fromDatetimeLocal(value.to) || null,
      sortBy: value.sortBy,
      sortDirection: value.sortDirection,
      pageSize: value.pageSize,
      page: 1,
    });
  }

  protected clearFilters(): void {
    void this.router.navigate([], { queryParams: {} });
  }

  protected goToPage(page: number): void {
    this.navigate({ page });
  }

  protected open(booking: BookingListItem): void {
    void this.router.navigate(['/bookings', booking.id]);
  }

  private navigate(queryParams: Params): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      queryParamsHandling: 'merge',
    });
  }
}
