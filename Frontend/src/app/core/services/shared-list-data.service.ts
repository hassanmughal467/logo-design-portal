import { Injectable } from '@angular/core';
import { Observable, forkJoin, of } from 'rxjs';
import { map, shareReplay, switchMap } from 'rxjs/operators';
import { ApiService } from './api.service';

/** Matches GET /api/users/search response (camelCase from API). */
export interface UserTypeaheadItem {
  id: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  roleName?: string;
  label: string;
}

/** Page size aligned with backend cap (100); keeps payloads bounded while full scans paginate. */
export const SHARED_LIST_PAGE_SIZE = 100;

/** Safety cap: malformed totals or huge pages must not spin forever or open unlimited parallel requests. */
const MAX_FETCH_ALL_PAGES = 500;

/**
 * Deduplicates heavy "fetch all pages" calls for users/orders across components via shareReplay.
 * Cleared on demand when UI needs fresh data after mutations.
 */
@Injectable({ providedIn: 'root' })
export class SharedListDataService {
  private allUsers$: Observable<unknown[]> | null = null;
  private allOrdersAdmin$: Observable<unknown[]> | null = null;

  constructor(private api: ApiService) {}

  /** Typeahead user search (debounce ~300ms in the component). Optional role: Client | Designer | … */
  searchUsers(query: string, role?: string | null, limit = 15): Observable<UserTypeaheadItem[]> {
    const q = encodeURIComponent((query ?? '').trim());
    const roleParam = role ? `&role=${encodeURIComponent(role)}` : '';
    const lim = Math.min(Math.max(limit, 1), 20);
    return this.api.get<UserTypeaheadItem[]>(`users/search?query=${q}&limit=${lim}${roleParam}`);
  }

  /** All users (every page), shared across subscribers. */
  getAllUsers(): Observable<unknown[]> {
    if (!this.allUsers$) {
      this.allUsers$ = this.fetchAllPages<unknown>('users').pipe(
        shareReplay({ bufferSize: 1, refCount: true })
      );
    }
    return this.allUsers$;
  }

  /** All orders for admin list endpoint (every page). */
  getAllOrdersAdmin(): Observable<unknown[]> {
    if (!this.allOrdersAdmin$) {
      this.allOrdersAdmin$ = this.fetchAllPages<unknown>('orders').pipe(
        shareReplay({ bufferSize: 1, refCount: true })
      );
    }
    return this.allOrdersAdmin$;
  }

  clearUsersCache(): void {
    this.allUsers$ = null;
  }

  clearOrdersAdminCache(): void {
    this.allOrdersAdmin$ = null;
  }

  clearAll(): void {
    this.clearUsersCache();
    this.clearOrdersAdminCache();
  }

  /** Full user list (all pages), no shareReplay — use when you need a fresh snapshot (e.g. order management grid). */
  fetchAllUsersUncached(): Observable<unknown[]> {
    return this.fetchAllPages<unknown>('users');
  }

  /** Full admin orders list (all pages), no shareReplay — fresh snapshot for grids after navigation/mutations. */
  fetchAllOrdersUncached(): Observable<unknown[]> {
    return this.fetchAllPages<unknown>('orders');
  }

  private fetchAllPages<T>(resource: string): Observable<T[]> {
    const pageSize = SHARED_LIST_PAGE_SIZE;
    return this.api.get<unknown>(`${resource}?page=1&pageSize=${pageSize}`).pipe(
      switchMap((first) => {
        const meta = ApiService.extractPagedMeta(first);
        const items = ApiService.extractItems<T>(first);
        let totalPages = Math.max(1, meta.totalPages || 1);
        if (!Number.isFinite(totalPages)) totalPages = 1;
        totalPages = Math.min(Math.floor(totalPages), MAX_FETCH_ALL_PAGES);
        if (totalPages <= 1) {
          return of(items);
        }
        const rest: Observable<unknown>[] = [];
        for (let p = 2; p <= totalPages; p++) {
          rest.push(this.api.get<unknown>(`${resource}?page=${p}&pageSize=${pageSize}`));
        }
        return forkJoin(rest).pipe(
          map((pages) => {
            const all = [...items];
            for (const pageRes of pages) {
              all.push(...ApiService.extractItems<T>(pageRes));
            }
            return all;
          })
        );
      })
    );
  }
}
