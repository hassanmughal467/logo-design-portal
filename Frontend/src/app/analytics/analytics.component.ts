import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { DashboardService, DashboardData } from '@core/services/dashboard.service';
import { Subject } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-analytics',
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.scss']
})
export class AnalyticsComponent implements OnInit, OnDestroy {
  loading = true;
  private destroy$ = new Subject<void>();

  // Analytics data
  clientOrderAnalytics: any[] = [];
  clientProductivityMetrics: any[] = [];
  clientFinancialCards: any[] = [];

  // Chart data
  statusChartData: any;
  statusChartOptions: any;

  constructor(
    private authService: AuthService,
    private dashboardService: DashboardService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadAnalyticsData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadAnalyticsData(): void {
    this.loading = true;
    const user = this.authService.getCurrentUser();

    // Load dashboard data (works for both client and admin/designer)
    this.dashboardService.getDashboardData()
      .pipe(
        takeUntil(this.destroy$),
        catchError(error => {
          console.error('Error loading analytics data:', error);
          this.loading = false;
          return of(null);
        })
      )
      .subscribe((data: DashboardData | null) => {
        if (data) {
          this.buildAnalyticsData(data);
          this.setupStatusChart(data);
        }
        this.loading = false;
      });
  }

  private buildAnalyticsData(data: DashboardData): void {
    const user = this.authService.getCurrentUser();
    const isClient = user?.role === 'Client' || user?.roleName === 'Client';

    if (isClient) {
      this.clientOrderAnalytics = [
        { label: 'Total Orders', value: data.stats.totalOrders, icon: 'pi pi-list', color: 'primary', route: '/orders' },
        { label: 'This Month', value: data.stats.ordersThisMonth || 0, icon: 'pi pi-calendar', color: 'info', route: '/orders' },
        { label: 'This Week', value: data.stats.ordersThisWeek || 0, icon: 'pi pi-clock', color: 'warning', route: '/orders' },
        { label: 'Today', value: data.stats.ordersToday || 0, icon: 'pi pi-bolt', color: 'success', route: '/orders' }
      ];

      this.clientProductivityMetrics = [
        { label: 'Completed This Month', value: data.stats.completedOrdersThisMonth || 0, icon: 'pi pi-check-circle', color: 'success', route: '/orders' },
        { label: 'Lifetime Spend', value: data.stats.lifetimeSpend || 0, icon: 'pi pi-wallet', color: 'primary', isCurrency: true, route: '/invoices' },
        { label: 'Monthly Spend', value: data.stats.monthlySpend || 0, icon: 'pi pi-chart-line', color: 'info', isCurrency: true, route: '/invoices' },
        { label: 'Avg Order Value', value: data.stats.averageOrderValue || 0, icon: 'pi pi-percentage', color: 'warning', isCurrency: true, route: '/orders' }
      ];

      this.clientFinancialCards = [
        { label: 'Active Orders', value: data.stats.activeOrders || 0, icon: 'pi pi-shopping-cart', color: 'info', route: '/orders' },
        { label: 'Awaiting Approval', value: data.stats.ordersAwaitingApproval || 0, icon: 'pi pi-hourglass', color: 'warning', route: '/orders' },
        { label: 'Pending Invoices', value: data.stats.pendingInvoices || 0, icon: 'pi pi-file', color: 'danger', route: '/invoices' },
        { label: 'Due Amount', value: data.stats.overdueAmount || 0, icon: 'pi pi-dollar', color: 'danger', isCurrency: true, route: '/invoices' },
        { label: 'Paid This Month', value: data.stats.amountPaidThisMonth || 0, icon: 'pi pi-money-bill', color: 'success', isCurrency: true, route: '/invoices' }
      ];
    }
  }

  private setupStatusChart(data: DashboardData): void {
    if (data.ordersByStatus && data.ordersByStatus.length > 0) {
      const labels = data.ordersByStatus.map((item: { status: string; count: number }) => item.status || 'Unknown');
      const counts = data.ordersByStatus.map((item: { status: string; count: number }) => item.count || 0);

      this.statusChartData = {
        labels: labels,
        datasets: [
          {
            label: 'Orders by Status',
            data: counts,
            backgroundColor: [
              'rgba(54, 162, 235, 0.6)',
              'rgba(255, 99, 132, 0.6)',
              'rgba(255, 206, 86, 0.6)',
              'rgba(75, 192, 192, 0.6)',
              'rgba(153, 102, 255, 0.6)'
            ],
            borderColor: [
              'rgba(54, 162, 235, 1)',
              'rgba(255, 99, 132, 1)',
              'rgba(255, 206, 86, 1)',
              'rgba(75, 192, 192, 1)',
              'rgba(153, 102, 255, 1)'
            ],
            borderWidth: 2
          }
        ]
      };

      this.statusChartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: true,
            position: 'bottom'
          }
        }
      };
    }
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 2
    }).format(value);
  }

  navigateToRoute(route: string): void {
    if (route) {
      this.router.navigate([route]);
    }
  }
}

