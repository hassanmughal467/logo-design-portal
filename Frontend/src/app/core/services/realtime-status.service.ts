import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

/**
 * Tracks whether SignalR real-time connection is active.
 * Used by NotificationService to decide whether to poll (fallback when disconnected).
 */
@Injectable({
  providedIn: 'root'
})
export class RealtimeStatusService {
  private readonly connectedSubject = new BehaviorSubject<boolean>(false);
  readonly isConnected$: Observable<boolean> = this.connectedSubject.asObservable();

  setConnected(connected: boolean): void {
    this.connectedSubject.next(connected);
  }

  isConnected(): boolean {
    return this.connectedSubject.value;
  }
}
