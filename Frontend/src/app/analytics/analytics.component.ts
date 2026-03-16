import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthService } from '@core/services/auth.service';
import { AdminAnalyticsService } from '@core/services/admin-analytics.service';
import { DashboardService, DashboardData } from '@core/services/dashboard.service';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil, catchError, forkJoin, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { OrderStatus } from '@shared/models/order.model';

@Component({
  selector: 'app-analytics',
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.scss']
})
export class AnalyticsComponent implements OnInit, OnDestroy {
  loading = true;
  private destroy$ = new Subject<void>();

  overview: any = null;
  orderAnalytics: any = null;
  revenueAnalytics: any = null;
  designerAnalytics: any = null;
  clientAnalytics: any = null;
  workflowAnalytics: any = null;
  systemAnalytics: any = null;
  forecastAnalytics: any = null;
  insightsAnalytics: any = null;
  recentOrders: any[] = [];

  apiError = false;

  // Client analytics (usage insights)
  clientDashboardData: DashboardData | null = null;
  ordersTrendChartData: any;
  packageUsageChartData: any;
  deliveryTimeTrendChartData: any;
  revisionStatsChartData: any;
  totalOrders = 0;
  completedOrders = 0;
  revisionRequests = 0;
  averageDeliveryTimeHours = 0;
  chartOptions: any;
  barChartOptions: any;
  lineChartOptions: any;
  deliveryChartOptions: any;
  revisionChartOptions: any;

  // Expand/collapse for section blocks
  orderChartsCollapsed = false;
  workflowCollapsed = false;
  designerCollapsed = false;
  clientCollapsed = false;
  revenueCollapsed = false;
  systemCollapsed = false;
  forecastCollapsed = false;

  constructor(
    private authService: AuthService,
    private adminAnalytics: AdminAnalyticsService,
    private dashboardService: DashboardService,
    private apiService: ApiService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadAnalyticsData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get isAdmin(): boolean {
    const user = this.authService.getCurrentUser();
    return user?.role === 'SuperAdmin' || user?.role === 'Admin';
  }

  get isClient(): boolean {
    const user = this.authService.getCurrentUser();
    return user?.role === 'Client' || user?.roleName === 'Client';
  }

  loadAnalyticsData(): void {
    if (this.isClient) {
      this.loadClientAnalytics();
      return;
    }

    if (!this.isAdmin) {
      this.loading = false;
      return;
    }

    this.loading = true;
    this.apiError = false;
    forkJoin({
      overview: this.adminAnalytics.getOverview().pipe(catchError((err) => { console.error('Analytics overview error:', err); return of(null); })),
      orders: this.adminAnalytics.getOrderAnalytics().pipe(catchError((err) => { console.error('Order analytics error:', err); return of(null); })),
      revenue: this.adminAnalytics.getRevenueAnalytics().pipe(catchError((err) => { console.error('Revenue analytics error:', err); return of(null); })),
      designers: this.adminAnalytics.getDesignerAnalytics().pipe(catchError((err) => { console.error('Designer analytics error:', err); return of(null); })),
      clients: this.adminAnalytics.getClientAnalytics().pipe(catchError((err) => { console.error('Client analytics error:', err); return of(null); })),
      workflow: this.adminAnalytics.getWorkflowAnalytics().pipe(catchError((err) => { console.error('Workflow analytics error:', err); return of(null); })),
      system: this.adminAnalytics.getSystemAnalytics().pipe(catchError((err) => { console.error('System analytics error:', err); return of(null); })),
      forecast: this.adminAnalytics.getForecastAnalytics().pipe(catchError((err) => { console.error('Forecast analytics error:', err); return of(null); })),
      insights: this.adminAnalytics.getInsights().pipe(catchError((err) => { console.error('Insights error:', err); return of(null); })),
      recentOrders: this.apiService.get<any>('orders?page=1&pageSize=500').pipe(
        map(res => ApiService.extractItems<any>(res)),
        catchError(() => of([]))
      )
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.overview = data.overview;
        this.orderAnalytics = data.orders;
        this.revenueAnalytics = data.revenue;
        this.designerAnalytics = data.designers;
        this.clientAnalytics = data.clients;
        this.workflowAnalytics = data.workflow;
        this.systemAnalytics = data.system;
        this.forecastAnalytics = data.forecast;
        this.insightsAnalytics = data.insights;
        this.recentOrders = Array.isArray(data.recentOrders)
          ? data.recentOrders
            .sort((a: any, b: any) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
            .slice(0, 10)
          : [];
        this.apiError = !data.overview;
        if (this.apiError) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Analytics API Unavailable',
            detail: 'Could not load analytics data. Ensure the backend is running and the API URL is correct (check environment.ts).'
          });
        }
        this.loading = false;
      },
      error: (err) => {
        console.error('Analytics load error:', err);
        this.apiError = true;
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load analytics. Check console for details.'
        });
      }
    });
  }

  private loadClientAnalytics(): void {
    this.loading = true;
    this.initClientChartOptions();
    this.dashboardService.getDashboardData()
      .pipe(takeUntil(this.destroy$), catchError(() => of(null)))
      .subscribe((data: DashboardData | null) => {
        this.clientDashboardData = data;
        if (data) {
          this.buildClientAnalytics(data);
        }
        this.loading = false;
      });
  }

  private initClientChartOptions(): void {
    this.chartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } }
    };
    this.barChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { position: 'bottom' } },
      scales: { y: { beginAtZero: true } }
    };
    this.lineChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: { y: { beginAtZero: true } }
    };
  }

  private buildClientAnalytics(data: DashboardData): void {
    const orders = data.allOrders || data.recentOrders || [];
    const nonCancelled = orders.filter(o =>
      o.status !== OrderStatus.Cancelled &&
      o.status !== OrderStatus.CancelledByUser &&
      o.status !== OrderStatus.CancelledByAdmin
    );

    this.totalOrders = data.stats.totalOrders ?? orders.length;
    this.completedOrders = data.stats.completedOrders ?? orders.filter(o => o.status === OrderStatus.Completed).length;
    this.revisionRequests = nonCancelled.filter(o => ((o as any).revisionCount || 0) > 0).length;
    const avgDays = data.stats.averageDeliveryTime ?? 0;
    this.averageDeliveryTimeHours = Math.round(avgDays * 24);

    this.buildOrdersTrendChart(orders);
    this.buildPackageUsageChart(data);
    this.buildDeliveryTimeTrendChart(orders);
    this.buildRevisionStatsChart(orders);
  }

  private buildOrdersTrendChart(orders: any[]): void {
    const monthCounts = new Map<string, number>();
    orders.forEach(o => {
      const d = new Date(o.createdAt);
      const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
      monthCounts.set(key, (monthCounts.get(key) || 0) + 1);
    });
    const sorted = Array.from(monthCounts.entries()).sort((a, b) => a[0].localeCompare(b[0])).slice(-12);
    if (sorted.length === 0) {
      this.ordersTrendChartData = null;
      return;
    }
    this.ordersTrendChartData = {
      labels: sorted.map(([m]) => {
        const [y, mo] = m.split('-');
        return new Date(parseInt(y), parseInt(mo) - 1).toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
      }),
      datasets: [{
        label: 'Orders',
        data: sorted.map(([, c]) => c),
        fill: true,
        borderColor: 'rgba(13, 71, 161, 1)',
        backgroundColor: 'rgba(13, 71, 161, 0.2)',
        tension: 0.4
      }]
    };
  }

  private buildPackageUsageChart(data: DashboardData): void {
    const orders = data.allOrders || data.recentOrders || [];
    const packageCounts = new Map<string, number>();
    orders.forEach(o => {
      const pkg = (o as any).packageType || (o as any).package || 'Standard';
      packageCounts.set(pkg, (packageCounts.get(pkg) || 0) + 1);
    });
    const sorted = Array.from(packageCounts.entries()).sort((a, b) => b[1] - a[1]);
    if (sorted.length === 0) {
      this.packageUsageChartData = null;
      return;
    }
    this.packageUsageChartData = {
      labels: sorted.map(([p]) => p),
      datasets: [{
        label: 'Orders',
        data: sorted.map(([, c]) => c),
        backgroundColor: ['rgba(59, 130, 246, 0.6)', 'rgba(16, 185, 129, 0.6)', 'rgba(245, 158, 11, 0.6)', 'rgba(139, 92, 246, 0.6)'],
        borderColor: ['rgba(59, 130, 246, 1)', 'rgba(16, 185, 129, 1)', 'rgba(245, 158, 11, 1)', 'rgba(139, 92, 246, 1)'],
        borderWidth: 2
      }]
    };
  }

  private buildDeliveryTimeTrendChart(orders: any[]): void {
    const completed = orders.filter(o => o.status === OrderStatus.Completed && (o.dueDate || o.updatedAt));
    const monthHours = new Map<string, number[]>();
    completed.forEach(o => {
      const created = new Date(o.createdAt);
      const completedDate = o.dueDate ? new Date(o.dueDate) : new Date(o.updatedAt || o.createdAt);
      const hours = Math.max(0, (completedDate.getTime() - created.getTime()) / (1000 * 60 * 60));
      const key = `${created.getFullYear()}-${String(created.getMonth() + 1).padStart(2, '0')}`;
      const arr = monthHours.get(key) || [];
      arr.push(hours);
      monthHours.set(key, arr);
    });
    const sorted = Array.from(monthHours.entries()).sort((a, b) => a[0].localeCompare(b[0])).slice(-12);
    if (sorted.length === 0) {
      this.deliveryTimeTrendChartData = null;
      return;
    }
    this.deliveryTimeTrendChartData = {
      labels: sorted.map(([m]) => {
        const [y, mo] = m.split('-');
        return new Date(parseInt(y), parseInt(mo) - 1).toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
      }),
      datasets: [{
        label: 'Avg Delivery (hrs)',
        data: sorted.map(([, arr]) => Math.round(arr.reduce((s, h) => s + h, 0) / arr.length)),
        fill: true,
        borderColor: 'rgba(16, 185, 129, 1)',
        backgroundColor: 'rgba(16, 185, 129, 0.2)',
        tension: 0.4
      }]
    };
    this.deliveryChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        y: { beginAtZero: true, ticks: { callback: (v: number) => v + ' hrs' } }
      }
    };
  }

  private buildRevisionStatsChart(orders: any[]): void {
    const nonCancelled = orders.filter(o =>
      o.status !== OrderStatus.Cancelled &&
      o.status !== OrderStatus.CancelledByUser &&
      o.status !== OrderStatus.CancelledByAdmin
    );
    let noRevision = 0;
    let oneRevision = 0;
    let twoPlusRevisions = 0;
    nonCancelled.forEach(o => {
      const rc = (o as any).revisionCount ?? 0;
      if (rc === 0) noRevision++;
      else if (rc === 1) oneRevision++;
      else twoPlusRevisions++;
    });
    const total = noRevision + oneRevision + twoPlusRevisions;
    if (total === 0) {
      this.revisionStatsChartData = null;
      return;
    }
    this.revisionStatsChartData = {
      labels: ['No revision', '1 revision', '2+ revisions'],
      datasets: [{
        data: [noRevision, oneRevision, twoPlusRevisions],
        backgroundColor: ['#10b981', '#f59e0b', '#6366f1'],
        borderWidth: 0
      }]
    };
    this.revisionChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      cutout: '60%',
      plugins: {
        legend: { position: 'bottom' },
        tooltip: {
          callbacks: {
            label: (ctx: any) => {
              const pct = total > 0 ? Math.round((ctx.parsed / total) * 100) : 0;
              return ` ${ctx.label}: ${pct}%`;
            }
          }
        }
      }
    };
  }
}
