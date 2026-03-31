import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthService } from '@core/services/auth.service';
import { FinancialAnalyticsService, FinancialOverview } from '@core/services/financial-analytics.service';
import { Subject } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { forkJoin, of } from 'rxjs';

@Component({
  selector: 'app-financial-dashboard',
  templateUrl: './financial-dashboard.component.html',
  styleUrls: ['./financial-dashboard.component.scss']
})
export class FinancialDashboardComponent implements OnInit, OnDestroy {
  apiError = false;
  overview: FinancialOverview | null = null;
  revenueTrendItems: any[] = [];
  ordersVsRevenueItems: any[] = [];
  packageRevenueItems: any[] = [];
  designerRevenueItems: any[] = [];
  clientRevenueItems: any[] = [];
  invoiceStatusItems: any[] = [];
  weeklyRevenueItems: any[] = [];
  orderValueItems: any[] = [];
  activityItems: any[] = [];
  forecastItems: any[] = [];

  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private financialService: FinancialAnalyticsService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadData(): void {
    this.apiError = false;

    forkJoin({
      overview: this.financialService.getOverview().pipe(catchError(() => of(null))),
      revenueTrend: this.financialService.getRevenueTrend().pipe(catchError(() => of({ items: [] }))),
      ordersVsRevenue: this.financialService.getOrdersVsRevenue().pipe(catchError(() => of({ items: [] }))),
      packageRevenue: this.financialService.getPackageRevenue().pipe(catchError(() => of({ items: [] }))),
      designerRevenue: this.financialService.getDesignerRevenue().pipe(catchError(() => of({ items: [] }))),
      clientRevenue: this.financialService.getClientRevenue().pipe(catchError(() => of({ items: [] }))),
      invoiceStatus: this.financialService.getInvoiceStatus().pipe(catchError(() => of({ items: [] }))),
      weeklyRevenue: this.financialService.getWeeklyRevenue().pipe(catchError(() => of({ items: [] }))),
      orderValueTrend: this.financialService.getOrderValueTrend().pipe(catchError(() => of({ items: [] }))),
      activityFeed: this.financialService.getActivityFeed().pipe(catchError(() => of({ items: [] }))),
      forecast: this.financialService.getRevenueForecast().pipe(catchError(() => of({ items: [] })))
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.overview = data.overview ?? null;
          this.revenueTrendItems = data.revenueTrend.items ?? [];
          this.ordersVsRevenueItems = data.ordersVsRevenue.items ?? [];
          this.packageRevenueItems = data.packageRevenue.items ?? [];
          this.designerRevenueItems = data.designerRevenue.items ?? [];
          this.clientRevenueItems = data.clientRevenue.items ?? [];
          this.invoiceStatusItems = data.invoiceStatus.items ?? [];
          this.weeklyRevenueItems = data.weeklyRevenue.items ?? [];
          this.orderValueItems = data.orderValueTrend.items ?? [];
          this.activityItems = data.activityFeed.items ?? [];
          this.forecastItems = data.forecast.items ?? [];
          this.apiError = !data.overview;
        },
        error: () => {
          this.apiError = true;
        }
      });
  }
}
