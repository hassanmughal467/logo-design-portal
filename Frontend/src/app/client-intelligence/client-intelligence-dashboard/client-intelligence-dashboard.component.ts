import { Component, OnInit, OnDestroy } from '@angular/core';
import { ClientAnalyticsService, ClientAnalyticsOverview } from '@core/services/client-analytics.service';
import { Subject } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { forkJoin, of } from 'rxjs';

@Component({
  selector: 'app-client-intelligence-dashboard',
  templateUrl: './client-intelligence-dashboard.component.html',
  styleUrls: ['./client-intelligence-dashboard.component.scss']
})
export class ClientIntelligenceDashboardComponent implements OnInit, OnDestroy {
  apiError = false;
  overview: ClientAnalyticsOverview | null = null;
  topClients: any[] = [];
  revenueTrendItems: any[] = [];
  monthlyRevenueItems: any[] = [];
  inactiveClients: any[] = [];
  lifetimeValueItems: any[] = [];
  growthItems: any[] = [];
  activityItems: any[] = [];
  alerts: any[] = [];
  clientsDropdown: any[] = [];
  selectedClientId: string | null = null;
  monthlyRevenueLoading = false;

  private destroy$ = new Subject<void>();

  constructor(private clientAnalytics: ClientAnalyticsService) {}

  ngOnInit(): void {
    this.loadData();
    this.loadClientsDropdown();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadData(): void {
    this.apiError = false;

    forkJoin({
      overview: this.clientAnalytics.getOverview().pipe(catchError(() => of(null))),
      topClients: this.clientAnalytics.getTopClients(10).pipe(catchError(() => of({ items: [] }))),
      revenueTrend: this.clientAnalytics.getRevenueTrend(6).pipe(catchError(() => of({ items: [] }))),
      inactiveClients: this.clientAnalytics.getInactiveClients(30).pipe(catchError(() => of({ items: [] }))),
      lifetimeValue: this.clientAnalytics.getLifetimeValue().pipe(catchError(() => of({ items: [] }))),
      growth: this.clientAnalytics.getClientGrowth().pipe(catchError(() => of({ items: [] }))),
      activity: this.clientAnalytics.getClientActivity().pipe(catchError(() => of({ items: [] }))),
      alerts: this.clientAnalytics.getAlerts().pipe(catchError(() => of({ items: [] })))
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.overview = data.overview ?? null;
          this.topClients = data.topClients.items ?? [];
          this.revenueTrendItems = data.revenueTrend.items ?? [];
          this.inactiveClients = data.inactiveClients.items ?? [];
          this.lifetimeValueItems = data.lifetimeValue.items ?? [];
          this.growthItems = data.growth.items ?? [];
          this.activityItems = data.activity.items ?? [];
          this.alerts = data.alerts.items ?? [];
          this.apiError = !data.overview;
        },
        error: () => {
          this.apiError = true;
        },
        complete: () => {}
      });
  }

  loadClientsDropdown(): void {
    this.clientAnalytics.getClientsForDropdown()
      .pipe(takeUntil(this.destroy$), catchError(() => of([])))
      .subscribe(items => {
        this.clientsDropdown = items;
        if (items.length > 0 && !this.selectedClientId) {
          this.selectedClientId = items[0].clientId;
          this.onClientSelected();
        }
      });
  }

  onClientSelected(): void {
    if (!this.selectedClientId) return;
    this.monthlyRevenueLoading = true;
    this.clientAnalytics.getMonthlyRevenue(this.selectedClientId, 12)
      .pipe(takeUntil(this.destroy$), catchError(() => of({ clientId: '', clientName: '', items: [] })))
      .subscribe({
        next: (data) => {
          this.monthlyRevenueItems = data.items ?? [];
        },
        complete: () => {
          this.monthlyRevenueLoading = false;
        }
      });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(value);
  }

  getActivityStatusLabel(status: string): string {
    const map: Record<string, string> = {
      Active: 'Active',
      LowActivity: 'Low Activity',
      Inactive: 'Inactive'
    };
    return map[status] || status;
  }

  getActivitySeverity(status: string): string {
    const map: Record<string, string> = {
      Active: 'success',
      LowActivity: 'warn',
      Inactive: 'danger'
    };
    return map[status] || 'info';
  }

  getGrowthSeverity(status: string): string {
    const map: Record<string, string> = {
      Increase: 'success',
      Decrease: 'danger',
      Stable: 'info'
    };
    return map[status] || 'info';
  }

  getAlertSeverity(alertType: string): string {
    const map: Record<string, string> = {
      Inactive: 'danger',
      RevenueIncrease: 'success',
      Milestone: 'info'
    };
    return map[alertType] || 'info';
  }
}
