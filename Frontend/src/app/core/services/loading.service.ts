import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, defer } from 'rxjs';
import { distinctUntilChanged, finalize, map } from 'rxjs/operators';

/**
 * Lightweight loading state: tracks in-flight HTTP requests (non-blocking) and optional "critical" flows.
 * UI must opt in to any global indicator; normal API traffic does not block interaction.
 */
@Injectable({ providedIn: 'root' })
export class LoadingService {
  private activeHttpCount = 0;
  private readonly _activeHttpCount$ = new BehaviorSubject(0);

  private criticalCount = 0;
  private readonly _critical$ = new BehaviorSubject(false);

  /** Emits true while one or more HTTP requests are being tracked (for optional global UI). */
  readonly isGlobalLoading$: Observable<boolean> = this._activeHttpCount$.pipe(
    map((c) => c > 0),
    distinctUntilChanged()
  );

  /** Optional: true during explicit critical operations (e.g. login). Use for a slim top bar, not a full-screen block. */
  readonly criticalLoading$: Observable<boolean> = this._critical$.asObservable();

  beginHttp(): void {
    this.activeHttpCount++;
    this._activeHttpCount$.next(this.activeHttpCount);
  }

  endHttp(): void {
    this.activeHttpCount = Math.max(0, this.activeHttpCount - 1);
    this._activeHttpCount$.next(this.activeHttpCount);
  }

  beginCritical(): void {
    this.criticalCount++;
    this._critical$.next(this.criticalCount > 0);
  }

  endCritical(): void {
    this.criticalCount = Math.max(0, this.criticalCount - 1);
    this._critical$.next(this.criticalCount > 0);
  }

  /** Wrap an observable so critical loading is active until it completes, errors, or unsubscribes. */
  runCritical<T>(source: Observable<T>): Observable<T> {
    return defer(() => {
      this.beginCritical();
      return source.pipe(finalize(() => this.endCritical()));
    });
  }
}
