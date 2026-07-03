import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { ApiService } from '@core/services/api.service';
import { DashboardService, DashboardData } from '@core/services/dashboard.service';
import { ClientFinancialInsightsService, FinancialInsight } from '@core/services/client-financial-insights.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { Order, OrderStatus } from '@shared/models/order.model';
import { DEFAULT_CLIENT_CURRENCY } from '@core/constants/currency-options';
import { compactCurrencyLabel, formatCurrencyAmount } from '@core/utils/currency-format';

export type DateRangePreset = 'all_time' | 'this_month' | 'last_month' | 'last_3_months' | 'last_6_months' | 'this_year' | 'custom';

export interface DateRangeBounds {
  start: Date;
  end: Date;
}

@Component({
  selector: 'app-financial',
  templateUrl: './financial.component.html',
  styleUrls: ['./financial.component.scss']
})
export class FinancialComponent implements OnInit, OnDestroy {
  isAdmin = false;
  isClient = false;
  clientCurrencyCode = DEFAULT_CLIENT_CURRENCY;
  private destroy$ = new Subject<void>();

  // Date range filter (filtered by ClientId + Date Range)
  dateRangeOptions: { label: string; value: DateRangePreset }[] = [
    { label: 'This Month', value: 'this_month' },
    { label: 'Last Month', value: 'last_month' },
    { label: 'Last 3 Months', value: 'last_3_months' },
    { label: 'Last 6 Months', value: 'last_6_months' },
    { label: 'This Year', value: 'this_year' },
    { label: 'All Time', value: 'all_time' },
    { label: 'Custom Range', value: 'custom' }
  ];
  selectedDateRange: DateRangePreset = 'all_time';
  customStartDate: Date | null = null;
  customEndDate: Date | null = null;

  // Raw data (unfiltered by date - used as source for filtering)
  private rawOrders: Order[] = [];
  private rawInvoices: any[] = [];

  // Financial snapshot (filtered by ClientId - all metrics scoped to logged-in client)
  financialSnapshot = {
    ordersThisMonth: 0,     // COUNT orders created this month
    totalOrders: 0,         // COUNT all orders
    completedOrders: 0,    // COUNT orders with status Completed
    pendingOrders: 0,       // COUNT orders not Completed, not Cancelled
    lifetimeSpend: 0,       // SUM(Amount) for completed orders only
    spendThisMonth: 0,      // SUM(Amount) for completed orders created this month
    averageOrderValue: 0,   // lifetimeSpend / totalOrders
    totalInvoicesPaid: 0,
    totalInvoicesPending: 0
  };

  // Invoices data
  invoices: any[] = [];
  selectedInvoiceTabIndex = 0;
  selectedInvoices: any[] = [];
  showPaymentDialog = false;
  paymentMethod = '';

  // Client charts (filtered by ClientId)
  monthlySpendingChartData: any;
  spendingByPackageChartData: any;
  invoiceStatusChartData: any;
  spendingChartOptions: any;
  packageChartOptions: any;
  invoiceStatusChartOptions: any;

  // Weekly and monthly summaries
  weeklySummary = { total: 0, paid: 0, pending: 0 };
  monthlySummary = { total: 0, paid: 0, pending: 0 };

  // Spending insights (from API)
  financialInsights: FinancialInsight[] = [];
  insightsLoading = false;

  constructor(
    private authService: AuthService,
    private apiService: ApiService,
    private dashboardService: DashboardService,
    private clientFinancialInsightsService: ClientFinancialInsightsService,
    private messageService: MessageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    const isDesigner = user?.role === 'Designer' || user?.roleName === 'Designer';
    if (isDesigner) {
      this.router.navigate(['/financial/designer-payout'], { replaceUrl: true });
      return;
    }
    this.loadFinancialData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadFinancialData(): void {
    const user = this.authService.getCurrentUser();
    this.isAdmin = user?.role === 'SuperAdmin' || user?.role === 'Admin';
    this.isClient = user?.role === 'Client' || user?.roleName === 'Client';
    const isClient = this.isClient;

    // Admin: financial dashboard loads its own data (never mix with client data)
    if (this.isAdmin) {
      return;
    }

    // Client/Designer: use only client-scoped data from dashboard (filtered by ClientId for clients)
    if (isClient) {
      this.loadClientCurrency();
    }
    this.initChartOptions();

    this.dashboardService.invalidateDashboardCache();

    // Load dashboard data (filtered by ClientId)
    this.dashboardService.getDashboardData()
      .pipe(
        takeUntil(this.destroy$),
        catchError(error => {
          console.error('Error loading financial data:', error);
          return of(null);
        })
      )
      .subscribe((data: DashboardData | null) => {
        if (data) {
          if (isClient) {
            this.rawOrders = data.allOrders || data.recentOrders || [];
            this.rawInvoices = data.invoices || [];
            this.applyDateRangeAndRefresh();
            this.loadFinancialInsights();
          } else {
            this.rawOrders = data.recentOrders || [];
            this.rawInvoices = [];
            this.buildFinancialSnapshot(data);
          }
        }
      });
  }

  /** Get date range bounds for the selected preset or custom range */
  getDateRangeBounds(): DateRangeBounds | null {
    const now = new Date();
    const startOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59, 999);
    const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);
    const endOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0, 23, 59, 59, 999);

    switch (this.selectedDateRange) {
      case 'all_time':
        return null; // No date filter - use all data
      case 'this_month':
        return { start: startOfMonth, end: startOfToday };
      case 'last_month':
        const lastMonthStart = new Date(now.getFullYear(), now.getMonth() - 1, 1);
        const lastMonthEnd = new Date(now.getFullYear(), now.getMonth(), 0, 23, 59, 59, 999);
        return { start: lastMonthStart, end: lastMonthEnd };
      case 'last_3_months':
        const threeMonthsAgo = new Date(now.getFullYear(), now.getMonth() - 2, 1);
        return { start: threeMonthsAgo, end: startOfToday };
      case 'last_6_months':
        const sixMonthsAgo = new Date(now.getFullYear(), now.getMonth() - 5, 1);
        return { start: sixMonthsAgo, end: startOfToday };
      case 'this_year':
        const startOfYear = new Date(now.getFullYear(), 0, 1);
        return { start: startOfYear, end: startOfToday };
      case 'custom':
        if (this.customStartDate && this.customEndDate) {
          const start = new Date(this.customStartDate);
          const end = new Date(this.customEndDate);
          start.setHours(0, 0, 0, 0);
          end.setHours(23, 59, 59, 999);
          return { start, end };
        }
        return null;
      default:
        return null;
    }
  }

  /** Filter orders and invoices by date range, then refresh all analytics */
  applyDateRangeAndRefresh(): void {
    const bounds = this.getDateRangeBounds();
    if (!bounds) {
      this.invoices = [...this.rawInvoices];
      const data = this.buildFilteredDashboardData(this.rawOrders, this.rawInvoices);
      this.setupClientCharts(data);
      this.calculateSummaries();
      this.buildFinancialSnapshotFromFiltered(data);
      return;
    }

    const filteredOrders = this.rawOrders.filter(o => {
      const d = new Date(o.createdAt);
      return d >= bounds.start && d <= bounds.end;
    });
    const filteredInvoices = this.rawInvoices.filter(inv => {
      const d = new Date(inv.createdAt || inv.issueDate || inv.dueDate || 0);
      return d >= bounds.start && d <= bounds.end;
    });

    this.invoices = filteredInvoices;
    const data = this.buildFilteredDashboardData(filteredOrders, filteredInvoices);
    this.setupClientCharts(data);
    this.calculateSummaries();
    this.buildFinancialSnapshotFromFiltered(data);
  }

  /** Build DashboardData from filtered orders/invoices for chart processing.
   * All metrics scoped to logged-in client (orders already filtered by ClientId from dashboard).
   */
  private buildFilteredDashboardData(orders: Order[], invoices: any[]): DashboardData {
    const now = new Date();
    const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1, 0, 0, 0, 0);
    const cancelledStatuses = [OrderStatus.Cancelled, OrderStatus.CancelledByUser, OrderStatus.CancelledByAdmin];
    const getPackageFromPrice = (price: number): string => {
      if (price < 200) return 'Basic';
      if (price < 500) return 'Standard';
      if (price < 1000) return 'Premium';
      return 'Custom';
    };

    const completedOrdersList = orders.filter(o => o.status === OrderStatus.Completed);
    const nonCancelledOrders = orders.filter(o => !cancelledStatuses.includes(o.status));

    // Lifetime Spend = SUM(Amount) for completed orders only
    const lifetimeSpend = completedOrdersList.reduce((sum, o) => {
      const price = (o as any).price || 0;
      const refund = (o as any).isRefunded ? ((o as any).refundAmount || 0) : 0;
      return sum + Math.max(0, price - refund);
    }, 0);

    // Orders This Month = COUNT orders created this month
    const ordersThisMonth = orders.filter(o => new Date(o.createdAt) >= startOfMonth).length;

    // Spend This Month = SUM(Amount) for completed orders created this month
    const spendThisMonth = completedOrdersList
      .filter(o => new Date(o.createdAt) >= startOfMonth)
      .reduce((sum, o) => sum + ((o as any).price || 0), 0);

    const totalOrders = orders.length;
    const completedOrders = completedOrdersList.length;
    const pendingOrders = nonCancelledOrders.filter(o => o.status !== OrderStatus.Completed).length;
    const averageOrderValue = completedOrders > 0 ? lifetimeSpend / completedOrders : 0;

    const packageRevenue = new Map<string, number>();
    completedOrdersList.forEach(o => {
      const price = (o as any).price || 0;
      const pkg = (o as any).packageType || (o as any).package || getPackageFromPrice(price);
      packageRevenue.set(pkg, (packageRevenue.get(pkg) || 0) + price);
    });
    const revenueByPackage = Array.from(packageRevenue.entries())
      .map(([p, r]) => ({ package: p, revenue: r }))
      .sort((a, b) => b.revenue - a.revenue);

    const allOrdersSorted = [...orders].sort((a, b) =>
      new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    );

    return {
      stats: {
        totalOrders,
        pendingOrders,
        inProgressOrders: 0,
        completedOrders,
        totalClients: 0,
        newClientsThisMonth: 0,
        totalRevenue: 0,
        averageDeliveryTime: 0,
        lifetimeSpend,
        monthlySpend: spendThisMonth,
        averageOrderValue,
        ordersThisMonth
      } as any,
      recentOrders: allOrdersSorted.slice(0, 10),
      allOrders: allOrdersSorted,
      ordersByStatus: [],
      ordersByMonth: [],
      revenueByPackage,
      invoices
    };
  }

  onDateRangeChange(): void {
    if (this.selectedDateRange !== 'custom') {
      this.applyDateRangeAndRefresh();
    }
  }

  onCustomRangeApply(): void {
    if (this.customStartDate && this.customEndDate && this.customStartDate <= this.customEndDate) {
      this.applyDateRangeAndRefresh();
    }
  }

  /** Fallback for start date max when end not selected - use today */
  get maxDateForStartPicker(): Date {
    return new Date();
  }

  /** Fallback for end date min when start not selected - use start of year */
  get minDateForEndPicker(): Date {
    return new Date(new Date().getFullYear(), 0, 1);
  }

  private buildFinancialSnapshot(data: DashboardData): void {
    const ordersList = data.allOrders || data.recentOrders || [];
    const startOfMonth = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
    const completedOrdersList = ordersList.filter(o => o.status === OrderStatus.Completed);
    const lifetimeSpend = completedOrdersList.reduce((sum, o) => sum + ((o as any).price || 0), 0);
    const spendThisMonth = completedOrdersList
      .filter(o => new Date(o.createdAt) >= startOfMonth)
      .reduce((sum, o) => sum + ((o as any).price || 0), 0);
    const totalOrders = ordersList.length;
    const cancelledStatuses = [OrderStatus.Cancelled, OrderStatus.CancelledByUser, OrderStatus.CancelledByAdmin];
    const pendingOrders = ordersList.filter(o => !cancelledStatuses.includes(o.status) && o.status !== OrderStatus.Completed).length;

    this.financialSnapshot = {
      ordersThisMonth: data.stats.ordersThisMonth ?? ordersList.filter(o => new Date(o.createdAt) >= startOfMonth).length,
      totalOrders: data.stats.totalOrders ?? totalOrders,
      completedOrders: data.stats.completedOrders ?? completedOrdersList.length,
      pendingOrders: data.stats.pendingOrders ?? pendingOrders,
      lifetimeSpend: data.stats.lifetimeSpend ?? lifetimeSpend,
      spendThisMonth: data.stats.monthlySpend ?? spendThisMonth,
      averageOrderValue: completedOrdersList.length > 0 ? (data.stats.lifetimeSpend ?? lifetimeSpend) / completedOrdersList.length : 0,
      totalInvoicesPaid: this.invoices.filter(inv => inv.status === 'Paid').length,
      totalInvoicesPending: this.invoices.filter(inv => inv.status !== 'Paid' && inv.status !== 'Cancelled').length
    };
  }

  private buildFinancialSnapshotFromFiltered(data: DashboardData): void {
    this.financialSnapshot = {
      ordersThisMonth: data.stats.ordersThisMonth ?? 0,
      totalOrders: data.stats.totalOrders ?? 0,
      completedOrders: data.stats.completedOrders ?? 0,
      pendingOrders: data.stats.pendingOrders ?? 0,
      lifetimeSpend: data.stats.lifetimeSpend ?? 0,
      spendThisMonth: data.stats.monthlySpend ?? 0,
      averageOrderValue: data.stats.averageOrderValue ?? 0,
      totalInvoicesPaid: this.invoices.filter(inv => inv.status === 'Paid').length,
      totalInvoicesPending: this.invoices.filter(inv => inv.status !== 'Paid' && inv.status !== 'Cancelled').length
    };
  }

  private setupClientCharts(data: DashboardData): void {
    this.buildMonthlySpendingChart(data);
    this.buildSpendingByPackageChart(data);
    this.buildInvoiceStatusChart();
  }

  private loadClientCurrency(): void {
    const userId = this.authService.getCurrentUser()?.id;
    if (!userId) {
      return;
    }
    this.apiService.get<{ clientProfile?: { currencyCode?: string } }>(`users/${userId}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (profileUser) => {
          this.clientCurrencyCode = profileUser?.clientProfile?.currencyCode || DEFAULT_CLIENT_CURRENCY;
          this.initChartOptions();
        },
        error: () => {
          this.clientCurrencyCode = DEFAULT_CLIENT_CURRENCY;
        }
      });
  }

  private initChartOptions(): void {
    const tick = (v: number) => compactCurrencyLabel(v, this.clientCurrencyCode);
    this.spendingChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        y: { beginAtZero: true, ticks: { callback: tick } }
      }
    };
    this.packageChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        y: { beginAtZero: true, ticks: { callback: tick } }
      }
    };
    this.invoiceStatusChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { position: 'bottom' } },
      cutout: '60%'
    };
  }

  private buildMonthlySpendingChart(data: DashboardData): void {
    // Monthly Spending Trend: SUM(Amount) for completed orders only, grouped by month
    const orders = data.allOrders || data.recentOrders || [];
    const completedOrders = orders.filter(o => o.status === OrderStatus.Completed);
    const monthSpend = new Map<string, number>();
    completedOrders.forEach((o: any) => {
      const d = new Date(o.createdAt);
      const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
      const amt = o.price || 0;
      monthSpend.set(key, (monthSpend.get(key) || 0) + amt);
    });
    const sorted = Array.from(monthSpend.entries()).sort((a, b) => a[0].localeCompare(b[0])).slice(-12);
    if (sorted.length === 0) {
      this.monthlySpendingChartData = null;
      return;
    }
    this.monthlySpendingChartData = {
      labels: sorted.map(([m]) => {
        const [y, mo] = m.split('-');
        return new Date(parseInt(y), parseInt(mo) - 1).toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
      }),
      datasets: [{
        label: 'Spending',
        data: sorted.map(([, v]) => v),
        fill: true,
        borderColor: 'rgba(13, 71, 161, 1)',
        backgroundColor: 'rgba(13, 71, 161, 0.2)',
        tension: 0.4
      }]
    };
  }

  private buildSpendingByPackageChart(data: DashboardData): void {
    if (data.revenueByPackage && data.revenueByPackage.length > 0) {
      this.spendingByPackageChartData = {
        labels: data.revenueByPackage.map(p => p.package),
        datasets: [{
          label: 'Spending',
          data: data.revenueByPackage.map(p => p.revenue),
          backgroundColor: ['rgba(59, 130, 246, 0.6)', 'rgba(16, 185, 129, 0.6)', 'rgba(245, 158, 11, 0.6)', 'rgba(139, 92, 246, 0.6)'],
          borderColor: ['rgba(59, 130, 246, 1)', 'rgba(16, 185, 129, 1)', 'rgba(245, 158, 11, 1)', 'rgba(139, 92, 246, 1)'],
          borderWidth: 2
        }]
      };
    } else {
      this.spendingByPackageChartData = null;
    }
  }

  private buildInvoiceStatusChart(): void {
    // Invoice Status Distribution: paid vs pending (client invoices only)
    const paidCount = this.invoices.filter(inv => inv.status === 'Paid').length;
    const pendingCount = this.invoices.filter(inv => inv.status !== 'Paid').length;
    if (paidCount === 0 && pendingCount === 0) {
      this.invoiceStatusChartData = null;
      return;
    }
    this.invoiceStatusChartData = {
      labels: ['Paid', 'Pending'],
      datasets: [{
        data: [paidCount, pendingCount],
        backgroundColor: ['#10b981', '#f59e0b'],
        borderWidth: 0
      }]
    };
  }

  private calculateSummaries(): void {
    this.weeklySummary = this.getWeeklySummary();
    this.monthlySummary = this.getMonthlySummary();
  }

  getWeeklySummary(): { total: number; paid: number; pending: number } {
    const now = new Date();
    const weekStart = new Date(now.setDate(now.getDate() - now.getDay()));
    weekStart.setHours(0, 0, 0, 0);

    const weekInvoices = this.invoices.filter(inv => {
      const invDate = new Date(inv.createdAt || inv.issueDate);
      return invDate >= weekStart;
    });

    return {
      total: weekInvoices.reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0),
      paid: weekInvoices
        .filter(inv => inv.status === 'Paid')
        .reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0),
      pending: weekInvoices
        .filter(inv => inv.status !== 'Paid')
        .reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0)
    };
  }

  getMonthlySummary(): { total: number; paid: number; pending: number } {
    const now = new Date();
    const monthStart = new Date(now.getFullYear(), now.getMonth(), 1);

    const monthInvoices = this.invoices.filter(inv => {
      const invDate = new Date(inv.createdAt || inv.issueDate);
      return invDate >= monthStart;
    });

    return {
      total: monthInvoices.reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0),
      paid: monthInvoices
        .filter(inv => inv.status === 'Paid')
        .reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0),
      pending: monthInvoices
        .filter(inv => inv.status !== 'Paid')
        .reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0)
    };
  }

  formatCurrency(value: number, currencyCode?: string | null): string {
    return formatCurrencyAmount(value, currencyCode ?? this.clientCurrencyCode);
  }

  getUnpaidInvoices(): any[] {
    return this.invoices.filter(inv => inv.status === 'Pending' || inv.status === 'Overdue');
  }

  getPendingInvoices(): any[] {
    return this.invoices.filter(inv => inv.status === 'Pending');
  }

  getDueInvoices(): any[] {
    return this.invoices.filter(inv => inv.status === 'Overdue');
  }

  getPaidInvoices(): any[] {
    return this.invoices.filter(inv => inv.status === 'Paid');
  }

  navigateToInvoices(): void {
    this.router.navigate(['/invoices']);
  }

  loadFinancialInsights(): void {
    const user = this.authService.getCurrentUser();
    const isClient = user?.role === 'Client' || user?.roleName === 'Client';
    if (!isClient) return;

    this.insightsLoading = true;
    this.clientFinancialInsightsService.getFinancialInsights()
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => {
          this.insightsLoading = false;
          return of({ insights: [] });
        })
      )
      .subscribe(res => {
        this.financialInsights = res.insights || [];
        this.insightsLoading = false;
      });
  }

  getInsightSeverityClass(severity: string): string {
    const map: Record<string, string> = {
      info: 'insight-info',
      success: 'insight-success',
      warning: 'insight-warning',
      danger: 'insight-danger'
    };
    return map[severity] || 'insight-info';
  }
}
