import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { SharedListDataService } from '@core/services/shared-list-data.service';
import { MessageService } from 'primeng/api';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Order, OrderStatus } from '@shared/models/order.model';
import { getOrderStatusLabel, getOrderStatusSeverity, OrderStatusSeverity } from '@shared/utils/order-status-display';
import { TagSeverity } from '@shared/types/primeng.types';
import { compactCurrencyLabel, formatCurrencyAmount } from '@core/utils/currency-format';

export interface Client {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  companyName?: string;
  phoneNumber?: string;
  customerType?: 'Residential' | 'Business' | 'Student';
  totalOrders: number;
  totalSpent: number;
  currencyCode?: string;
  createdAt: Date;
  lastOrderDate?: Date;
  status?: 'Active' | 'Inactive';
}

@Component({
  selector: 'app-client-detail',
  templateUrl: './client-detail.component.html',
  styleUrls: ['./client-detail.component.scss']
})
export class ClientDetailComponent implements OnInit, OnDestroy {
  clientId: string | null = null;
  client: Client | null = null;
  loadFailed = false;
  errorMessage = '';
  activeTab: number = 0;

  // Tab data
  orders: Order[] = [];
  invoices: any[] = [];
  files: any[] = [];
  messages: any[] = [];

  // Loading states
  ordersLoading = false;
  invoicesLoading = false;
  filesLoading = false;
  messagesLoading = false;

  // Detail Analytics (filtered by ClientId)
  analyticsKpis = {
    totalOrders: 0,
    ordersThisMonth: 0,
    completedOrders: 0,
    revisionRequests: 0,
    averageDeliveryTime: 0
  };
  ordersPerMonthChartData: any;
  deliveryTimeTrendChartData: any;
  packageUsageChartData: any;
  revisionStatsChartData: any;
  chartOptions: any;
  doughnutOptions: any;
  lineChartOptions: any;
  revisionChartOptions: any;

  // Insights panel (filtered by ClientId)
  insights: { message: string; severity: string; icon?: string }[] = [];

  // Financial Overview (filtered by ClientId)
  financialKpis = {
    ordersThisMonth: 0,
    totalOrders: 0,
    completedOrders: 0,
    pendingOrders: 0,
    lifetimeSpend: 0,
    spendThisMonth: 0,
    averageOrderValue: 0,
    paidInvoices: 0,
    pendingInvoices: 0
  };
  monthlySpendingChartData: any;
  spendingByPackageChartData: any;
  invoiceStatusChartData: any;
  ordersVsSpendingChartData: any;
  spendingChartOptions: any;
  packageChartOptions: any;
  invoiceStatusChartOptions: any;
  ordersVsSpendingOptions: any;

  // Detail Analytics expand/collapse
  clientDetailKpiCollapsed = false;
  clientDetailChartsCollapsed = false;
  clientDetailInsightsCollapsed = false;
  financialActivityCollapsed = false;
  financialOverviewCollapsed = false;
  financialChartsCollapsed = false;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private messageService: MessageService,
    private sharedListData: SharedListDataService
  ) {}

  ngOnInit(): void {
    this.clientId = this.route.snapshot.paramMap.get('id');
    if (this.clientId) {
      this.loadClient();
      this.loadOrders();
      this.loadInvoices();
      this.loadFiles();
      this.loadMessages();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadClient(): void {
    this.loadFailed = false;
    this.errorMessage = '';
    // Use new comprehensive client detail endpoint
    this.apiService.get<any>(`users/clients/${this.clientId}/detail`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (clientDetail) => {
          const user = clientDetail.user;
          if (user.role === 'Client' || user.roleName === 'Client') {
            this.client = {
              id: user.id,
              email: user.email,
              firstName: user.firstName,
              lastName: user.lastName,
              companyName: clientDetail.clientProfile?.companyName,
              phoneNumber: clientDetail.clientProfile?.phoneNumber,
              customerType: clientDetail.clientProfile?.customerType as 'Residential' | 'Business' | 'Student' | undefined,
              totalOrders: clientDetail.orderHistory?.length || 0,
              totalSpent: clientDetail.invoices?.filter((inv: any) => inv.status === 'Paid').reduce((sum: number, inv: any) => sum + (inv.totalAmount || inv.amount || 0), 0) || 0,
              currencyCode: clientDetail.clientProfile?.currencyCode,
              createdAt: new Date(user.createdAt),
              status: user.isActive ? 'Active' : 'Inactive',
              lastOrderDate: clientDetail.orderHistory?.length > 0 
                ? new Date(clientDetail.orderHistory[0].createdAt) 
                : undefined
            };
            
            // Load data from detail response
            this.orders = clientDetail.orderHistory || [];
            this.invoices = clientDetail.invoices || [];
            this.files = clientDetail.files || [];
            this.ordersLoading = false;
            this.invoicesLoading = false;
            this.filesLoading = false;
            this.buildAnalytics();
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'User is not a client'
            });
            this.router.navigate(['/clients']);
          }
        },
        error: (err) => {
          // Fallback to old endpoint if new one doesn't exist
          this.loadClientLegacy(err);
        }
      });
  }

  private loadClientLegacy(_previousErr?: any): void {
    this.apiService.get<any>(`users/${this.clientId}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (user) => {
          if (user.role === 'Client' || user.roleName === 'Client') {
            this.client = {
              id: user.id,
              email: user.email,
              firstName: user.firstName,
              lastName: user.lastName,
              companyName: (user as any).companyName,
              phoneNumber: (user as any).phoneNumber,
              totalOrders: 0,
              totalSpent: 0,
              currencyCode: (user as any).clientProfile?.currencyCode,
              createdAt: new Date(user.createdAt),
              status: 'Active'
            };
            this.loadOrders();
            this.loadInvoices();
            this.loadFiles();
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'User is not a client'
            });
            this.router.navigate(['/clients']);
          }
        },
        error: (err) => {
          this.loadFailed = true;
          this.errorMessage = err.error?.error || 'Failed to load client details';
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: this.errorMessage
          });
        }
      });
  }

  loadOrders(): void {
    if (this.orders.length > 0) return; // Already loaded from detail endpoint
    this.ordersLoading = true;
    this.sharedListData
      .fetchAllOrdersUncached()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (ordersRaw) => {
          const orders = ordersRaw as Order[];
          this.orders = orders.filter(o => (o as any).clientId === this.clientId);
          if (this.client) {
            this.client.totalOrders = this.orders.length;
            const cancelledStatuses = [OrderStatus.Cancelled, OrderStatus.CancelledByUser, OrderStatus.CancelledByAdmin];
            this.client.totalSpent = this.orders
              .filter(o => !cancelledStatuses.includes(o.status))
              .reduce((sum, o) => {
                const price = (o as any).price || 0;
                const refund = (o as any).isRefunded ? ((o as any).refundAmount || 0) : 0;
                return sum + Math.max(0, price - refund);
              }, 0);
            this.client.lastOrderDate = this.orders.length > 0 
              ? new Date(this.orders[0].createdAt) 
              : undefined;
          }
          this.buildAnalytics();
          this.ordersLoading = false;
        },
        error: () => {
          this.orders = [];
          this.ordersLoading = false;
        }
      });
  }

  loadInvoices(): void {
    if (this.invoices.length > 0) return; // Already loaded from detail endpoint
    this.invoicesLoading = true;
    this.apiService.get<any[]>('invoices')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (invoices) => {
          this.invoices = invoices.filter(inv => inv.clientId === this.clientId);
          this.invoicesLoading = false;
        },
        error: () => {
          this.invoices = [];
          this.invoicesLoading = false;
        }
      });
  }

  loadFiles(): void {
    if (this.files.length > 0) return; // Already loaded from detail endpoint
    this.filesLoading = true;
    if (this.orders.length > 0) {
      forkJoin(this.orders.map(order => this.apiService.get<any[]>(`files/order/${order.id}`)))
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (fileArrays) => {
            this.files = fileArrays.flat().filter(f => f);
            this.filesLoading = false;
          },
          error: () => {
            this.files = [];
            this.filesLoading = false;
          }
        });
    } else {
      this.files = [];
      this.filesLoading = false;
    }
  }

  loadMessages(): void {
    this.messagesLoading = true;
    this.apiService.get<any[]>('messages')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (messages) => {
          // Filter messages where client is sender or recipient
          this.messages = messages.filter((m: any) => 
            m.senderId === this.clientId || m.recipientId === this.clientId
          ).map((m: any) => ({
            id: m.id,
            senderName: m.senderName,
            recipientName: m.recipientName,
            content: m.content,
            createdAt: m.createdAt
          })).sort((a: any, b: any) => 
            new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
          );
          this.messagesLoading = false;
        },
        error: () => {
          this.messages = [];
          this.messagesLoading = false;
        }
      });
  }

  viewOrder(orderId: string): void {
    this.router.navigate(['/orders', orderId]);
  }

  viewInvoice(invoiceId: string): void {
    this.router.navigate(['/invoices', invoiceId]);
  }

  downloadFile(file: { id: string; originalFileName?: string; fileName?: string }): void {
    const fileName = file.originalFileName || file.fileName || `file-${file.id}`;
    this.apiService.getBlob(`files/${file.id}/download`).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.click();
        URL.revokeObjectURL(url);
        this.messageService.add({ severity: 'success', summary: 'Download', detail: 'File downloaded successfully' });
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Failed to download file' });
      }
    });
  }

  formatCurrency(amount: number, currencyCode?: string | null): string {
    return formatCurrencyAmount(amount, currencyCode ?? this.client?.currencyCode);
  }

  private chartCurrencyTick(v: number): string {
    return compactCurrencyLabel(v, this.client?.currencyCode);
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  formatStatus(status: string): string {
    return getOrderStatusLabel(status);
  }

  getStatusSeverity(status: string): TagSeverity {
    const orderSeverity = getOrderStatusSeverity(status);
    if (orderSeverity !== 'secondary') {
      return orderSeverity;
    }
    const invoiceSeverityMap: Record<string, TagSeverity> = {
      Paid: 'success',
      Unpaid: 'warning',
      Overdue: 'danger'
    };
    return invoiceSeverityMap[status] || 'secondary';
  }

  goBack(): void {
    this.router.navigate(['/clients']);
  }

  getOrderPrice(order: Order): number {
    return (order as any).price || 0;
  }

  private buildAnalytics(): void {
    this.initChartOptions();
    const orders = this.orders;
    const invoices = this.invoices;

    if (!orders.length) {
      this.analyticsKpis = { totalOrders: 0, ordersThisMonth: 0, completedOrders: 0, revisionRequests: 0, averageDeliveryTime: 0 };
      this.insights = [{ message: 'No orders yet for this client.', severity: 'info', icon: 'pi pi-info-circle' }];
      this.buildFinancialOverview(orders, invoices);
      return;
    }

    const now = new Date();
    const startOfThisMonth = new Date(now.getFullYear(), now.getMonth(), 1);
    const startOfLastMonth = new Date(now.getFullYear(), now.getMonth() - 1, 1);
    const endOfLastMonth = new Date(now.getFullYear(), now.getMonth(), 0, 23, 59, 59, 999);

    const completedOrders = orders.filter(o => o.status === OrderStatus.Completed);
    const ordersThisMonth = orders.filter(o => new Date(o.createdAt) >= startOfThisMonth);
    const ordersLastMonth = orders.filter(o => {
      const d = new Date(o.createdAt);
      return d >= startOfLastMonth && d <= endOfLastMonth;
    });
    const revisionRequests = orders.reduce((sum, o) => sum + ((o as any).revisionCount || 0), 0);

    let averageDeliveryTime = 0;
    const completedWithDue = completedOrders.filter(o => o.dueDate || o.updatedAt);
    if (completedWithDue.length > 0) {
      const deliveryTimes = completedWithDue.map(o => {
        const created = new Date(o.createdAt);
        const completed = o.dueDate ? new Date(o.dueDate) : (o.updatedAt ? new Date(o.updatedAt) : new Date());
        return Math.max(0, Math.ceil((completed.getTime() - created.getTime()) / (1000 * 60 * 60 * 24)));
      });
      averageDeliveryTime = Math.round(deliveryTimes.reduce((a, b) => a + b, 0) / deliveryTimes.length);
    }

    this.analyticsKpis = {
      totalOrders: orders.length,
      ordersThisMonth: ordersThisMonth.length,
      completedOrders: completedOrders.length,
      revisionRequests,
      averageDeliveryTime
    };

    this.buildOrdersPerMonthChart(orders);
    this.buildDeliveryTimeTrendChart(completedOrders);
    this.buildPackageUsageChart(orders);
    this.buildRevisionStatsChart(orders);
    this.buildInsights(orders, ordersThisMonth, ordersLastMonth);
    this.buildFinancialOverview(orders, invoices);
  }

  private buildInsights(orders: Order[], ordersThisMonth: Order[], ordersLastMonth: Order[]): void {
    this.insights = [];
    const getPackageFromPrice = (price: number): string => {
      if (price < 200) return 'Basic';
      if (price < 500) return 'Standard';
      if (price < 1000) return 'Premium';
      return 'Custom';
    };

    // Orders placed this month
    const countThisMonth = ordersThisMonth.length;
    this.insights.push({
      message: countThisMonth === 0
        ? 'No orders placed this month yet.'
        : countThisMonth === 1
          ? '1 order placed this month.'
          : `${countThisMonth} orders placed this month.`,
      severity: 'info',
      icon: 'pi pi-shopping-cart'
    });

    // Activity increase or decrease compared to last month
    const countLastMonth = ordersLastMonth.length;
    if (countLastMonth > 0) {
      const change = countThisMonth - countLastMonth;
      const pct = Math.round((change / countLastMonth) * 100);
      const direction = change >= 0 ? 'increase' : 'decrease';
      this.insights.push({
        message: `Activity ${direction} of ${Math.abs(pct)}% compared to last month (${countLastMonth} orders last month).`,
        severity: change >= 0 ? 'success' : 'warning',
        icon: change >= 0 ? 'pi pi-arrow-up' : 'pi pi-arrow-down'
      });
    } else if (countThisMonth > 0) {
      this.insights.push({
        message: 'New activity this month (no orders last month).',
        severity: 'success',
        icon: 'pi pi-chart-line'
      });
    }

    // Most used package
    const nonCancelled = orders.filter(o =>
      o.status !== OrderStatus.Cancelled &&
      o.status !== OrderStatus.CancelledByUser &&
      o.status !== OrderStatus.CancelledByAdmin
    );
    if (nonCancelled.length > 0) {
      const byPackage = new Map<string, number>();
      nonCancelled.forEach(o => {
        const pkg = (o as any).packageType || (o as any).package || getPackageFromPrice((o as any).price || 0);
        byPackage.set(pkg, (byPackage.get(pkg) || 0) + 1);
      });
      const sorted = Array.from(byPackage.entries()).sort((a, b) => b[1] - a[1]);
      const [mostUsed, count] = sorted[0];
      this.insights.push({
        message: `Most used package: ${mostUsed} (${count} order${count === 1 ? '' : 's'}).`,
        severity: 'info',
        icon: 'pi pi-box'
      });
    }
  }

  private buildRevisionStatsChart(orders: Order[]): void {
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

  private buildFinancialOverview(orders: Order[], invoices: any[]): void {
    const now = new Date();
    const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1, 0, 0, 0, 0);
    const cancelledStatuses = [OrderStatus.Cancelled, OrderStatus.CancelledByUser, OrderStatus.CancelledByAdmin];

    const completedOrdersList = orders.filter(o => o.status === OrderStatus.Completed);
    const nonCancelledOrders = orders.filter(o => !cancelledStatuses.includes(o.status));

    // Lifetime Spend = SUM(Amount) for completed orders only
    const lifetimeSpend = completedOrdersList.reduce((sum, o) => {
      const price = (o as any).price || 0;
      const refund = (o as any).isRefunded ? ((o as any).refundAmount || 0) : 0;
      return sum + Math.max(0, price - refund);
    }, 0);

    // Spend This Month = SUM(Amount) for completed orders created this month
    const spendThisMonth = completedOrdersList
      .filter(o => new Date(o.createdAt) >= startOfMonth)
      .reduce((sum, o) => sum + ((o as any).price || 0), 0);

    const totalOrders = orders.length;
    const ordersThisMonth = orders.filter(o => new Date(o.createdAt) >= startOfMonth).length;
    const pendingOrders = nonCancelledOrders.filter(o => o.status !== OrderStatus.Completed).length;

    const paidInvoices = invoices.filter((inv: any) => inv.status === 'Paid').length;
    const pendingInvoices = invoices.filter((inv: any) => inv.status !== 'Paid' && inv.status !== 'Cancelled').length;

    this.financialKpis = {
      ordersThisMonth,
      totalOrders,
      completedOrders: completedOrdersList.length,
      pendingOrders,
      lifetimeSpend,
      spendThisMonth,
      averageOrderValue: completedOrdersList.length > 0 ? Math.round((lifetimeSpend / completedOrdersList.length) * 100) / 100 : 0,
      paidInvoices,
      pendingInvoices
    };

    this.buildMonthlySpendingChart(orders);
    this.buildSpendingByPackageChart(orders);
    this.buildInvoiceStatusChart(invoices);
    this.buildOrdersVsSpendingChart(orders);
  }

  private buildMonthlySpendingChart(orders: Order[]): void {
    const completedOrders = orders.filter(o => o.status === OrderStatus.Completed);
    const monthSpend = new Map<string, number>();
    completedOrders.forEach(o => {
        const d = new Date(o.createdAt);
        const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
        const amt = (o as any).price || 0;
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
    this.spendingChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        y: { beginAtZero: true, ticks: { callback: (v: number) => this.chartCurrencyTick(v) } }
      }
    };
  }

  private buildSpendingByPackageChart(orders: Order[]): void {
    const getPackageFromPrice = (price: number): string => {
      if (price < 200) return 'Basic';
      if (price < 500) return 'Standard';
      if (price < 1000) return 'Premium';
      return 'Custom';
    };
    const completedOrders = orders.filter(o => o.status === OrderStatus.Completed);
    const packageRevenue = new Map<string, number>();
    completedOrders.forEach(o => {
        const price = (o as any).price || 0;
        const pkg = (o as any).packageType || (o as any).package || getPackageFromPrice(price);
        packageRevenue.set(pkg, (packageRevenue.get(pkg) || 0) + price);
      });
    const sorted = Array.from(packageRevenue.entries()).sort((a, b) => b[1] - a[1]);
    if (sorted.length === 0) {
      this.spendingByPackageChartData = null;
      return;
    }
    this.spendingByPackageChartData = {
      labels: sorted.map(([p]) => p),
      datasets: [{
        label: 'Spending',
        data: sorted.map(([, r]) => r),
        backgroundColor: ['rgba(59, 130, 246, 0.6)', 'rgba(16, 185, 129, 0.6)', 'rgba(245, 158, 11, 0.6)', 'rgba(139, 92, 246, 0.6)'],
        borderColor: ['rgba(59, 130, 246, 1)', 'rgba(16, 185, 129, 1)', 'rgba(245, 158, 11, 1)', 'rgba(139, 92, 246, 1)'],
        borderWidth: 2
      }]
    };
    this.packageChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        y: { beginAtZero: true, ticks: { callback: (v: number) => this.chartCurrencyTick(v) } }
      }
    };
  }

  private buildInvoiceStatusChart(invoices: any[]): void {
    const paidCount = invoices.filter((inv: any) => inv.status === 'Paid').length;
    const pendingCount = invoices.filter((inv: any) => inv.status !== 'Paid' && inv.status !== 'Cancelled').length;
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
    this.invoiceStatusChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      cutout: '60%',
      plugins: { legend: { position: 'bottom' } }
    };
  }

  private buildOrdersVsSpendingChart(orders: Order[]): void {
    const completedOrders = orders.filter(o => o.status === OrderStatus.Completed);
    const monthData = new Map<string, { count: number; spend: number }>();
    completedOrders
      .forEach(o => {
        const d = new Date(o.createdAt);
        const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
        const existing = monthData.get(key) || { count: 0, spend: 0 };
        existing.count += 1;
        existing.spend += (o as any).price || 0;
        monthData.set(key, existing);
      });
    const sorted = Array.from(monthData.entries()).sort((a, b) => a[0].localeCompare(b[0])).slice(-12);
    if (sorted.length === 0) {
      this.ordersVsSpendingChartData = null;
      return;
    }
    this.ordersVsSpendingChartData = {
      labels: sorted.map(([m]) => {
        const [y, mo] = m.split('-');
        return new Date(parseInt(y), parseInt(mo) - 1).toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
      }),
      datasets: [
        {
          label: 'Orders',
          data: sorted.map(([, v]) => v.count),
          type: 'bar',
          backgroundColor: 'rgba(13, 71, 161, 0.6)',
          borderColor: 'rgba(13, 71, 161, 1)',
          borderWidth: 2,
          yAxisID: 'y'
        },
        {
          label: 'Spending',
          data: sorted.map(([, v]) => v.spend),
          type: 'line',
          borderColor: 'rgba(16, 185, 129, 1)',
          backgroundColor: 'rgba(16, 185, 129, 0.2)',
          fill: true,
          tension: 0.4,
          yAxisID: 'y1'
        }
      ]
    };
    this.ordersVsSpendingOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { position: 'top' } },
      scales: {
        y: { type: 'linear', position: 'left', beginAtZero: true, title: { display: true, text: 'Orders' } },
        y1: { type: 'linear', position: 'right', beginAtZero: true, grid: { drawOnChartArea: false }, ticks: { callback: (v: number) => this.chartCurrencyTick(v) } }
      }
    };
  }

  private initChartOptions(): void {
    this.chartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        y: { beginAtZero: true, ticks: { stepSize: 1 } }
      }
    };
    this.doughnutOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { position: 'bottom' } },
      cutout: '60%'
    };
    this.lineChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        y: { beginAtZero: true, title: { display: true, text: 'Days' } }
      }
    };
  }

  private buildOrdersPerMonthChart(orders: Order[]): void {
    const monthCounts = new Map<string, number>();
    orders.forEach(o => {
      const d = new Date(o.createdAt);
      const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
      monthCounts.set(key, (monthCounts.get(key) || 0) + 1);
    });
    const sorted = Array.from(monthCounts.entries()).sort((a, b) => a[0].localeCompare(b[0])).slice(-12);
    if (sorted.length === 0) {
      this.ordersPerMonthChartData = null;
      return;
    }
    this.ordersPerMonthChartData = {
      labels: sorted.map(([m]) => {
        const [y, mo] = m.split('-');
        return new Date(parseInt(y), parseInt(mo) - 1).toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
      }),
      datasets: [{ label: 'Orders', data: sorted.map(([, c]) => c), backgroundColor: 'rgba(13, 71, 161, 0.6)', borderColor: 'rgba(13, 71, 161, 1)', borderWidth: 2 }]
    };
  }

  private buildDeliveryTimeTrendChart(completedOrders: Order[]): void {
    const monthDelivery = new Map<string, number[]>();
    completedOrders.forEach(o => {
      const completed = o.dueDate ? new Date(o.dueDate) : (o.updatedAt ? new Date(o.updatedAt) : null);
      if (!completed) return;
      const key = `${completed.getFullYear()}-${String(completed.getMonth() + 1).padStart(2, '0')}`;
      const days = Math.ceil((completed.getTime() - new Date(o.createdAt).getTime()) / (1000 * 60 * 60 * 24));
      if (!monthDelivery.has(key)) monthDelivery.set(key, []);
      monthDelivery.get(key)!.push(days);
    });
    const sorted = Array.from(monthDelivery.entries())
      .map(([m, days]) => [m, days.reduce((a, b) => a + b, 0) / days.length] as [string, number])
      .sort((a, b) => a[0].localeCompare(b[0]))
      .slice(-12);
    if (sorted.length === 0) {
      this.deliveryTimeTrendChartData = null;
      return;
    }
    this.deliveryTimeTrendChartData = {
      labels: sorted.map(([m]) => {
        const [y, mo] = m.split('-');
        return new Date(parseInt(y), parseInt(mo) - 1).toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
      }),
      datasets: [{ label: 'Avg Days', data: sorted.map(([, v]) => Math.round(v * 10) / 10), fill: true, borderColor: 'rgba(16, 185, 129, 1)', backgroundColor: 'rgba(16, 185, 129, 0.2)', tension: 0.4 }]
    };
  }

  private buildPackageUsageChart(orders: Order[]): void {
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
      datasets: [{ label: 'Orders', data: sorted.map(([, c]) => c), backgroundColor: 'rgba(139, 92, 246, 0.6)', borderColor: 'rgba(139, 92, 246, 1)', borderWidth: 2 }]
    };
  }
}
