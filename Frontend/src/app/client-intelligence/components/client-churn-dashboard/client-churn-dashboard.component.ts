import { Component, OnInit, OnDestroy } from '@angular/core';
import { ClientChurnAnalyticsService, ClientRiskScoreItem, ClientChurnAlertItem, ClientRetentionStats } from '@core/services/client-churn-analytics.service';
import { formatCurrencyAmount } from '@core/utils/currency-format';
import { Subject } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { forkJoin, of } from 'rxjs';

@Component({
  selector: 'app-client-churn-dashboard',
  templateUrl: './client-churn-dashboard.component.html',
  styleUrls: ['./client-churn-dashboard.component.scss']
})
export class ClientChurnDashboardComponent implements OnInit, OnDestroy {
  loading = true;
  apiError = false;
  riskScores: ClientRiskScoreItem[] = [];
  churnAlerts: ClientChurnAlertItem[] = [];
  retentionStats: ClientRetentionStats | null = null;

  private destroy$ = new Subject<void>();

  constructor(private churnService: ClientChurnAnalyticsService) {}

  ngOnInit(): void {
    this.loadData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadData(): void {
    this.loading = true;
    this.apiError = false;

    forkJoin({
      riskScores: this.churnService.getClientRiskScores().pipe(catchError(() => of({ items: [] }))),
      churnAlerts: this.churnService.getClientChurnAlerts(30).pipe(catchError(() => of({ items: [] }))),
      retentionStats: this.churnService.getClientRetentionStats(12).pipe(catchError(() => of(null)))
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.riskScores = data.riskScores.items ?? [];
          this.churnAlerts = data.churnAlerts.items ?? [];
          this.retentionStats = data.retentionStats;
          this.apiError = data.retentionStats === null && (data.riskScores.items?.length ?? 0) === 0;
        },
        error: () => {
          this.apiError = true;
        },
        complete: () => {
          this.loading = false;
        }
      });
  }

  formatCurrency(value: number, currencyCode?: string): string {
    return formatCurrencyAmount(value, currencyCode);
  }
}
