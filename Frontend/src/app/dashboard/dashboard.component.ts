import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { ApiService } from '@core/services/api.service';
import { DashboardService, DashboardData } from '@core/services/dashboard.service';
import { AdminAnalyticsService } from '@core/services/admin-analytics.service';
import { NotificationService } from '@core/services/notification.service';
import { RealtimeNotificationService } from '@core/services/realtime-notification.service';
import { User } from '@shared/models/user.model';
import { Order, OrderStatus } from '@shared/models/order.model';
import { MessageService } from 'primeng/api';
import { Observable, Subject, firstValueFrom, forkJoin } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

export interface ActionRequiredItem {
  type: 'approval' | 'revision' | 'invoice' | 'price' | 'designerPrice';
  icon: string;
  severity: string;
  title: string;
  subtitle: string;
  actionLabel: string;
  actionIcon: string;
  data: any;
}

export interface OrderTimelineEvent {
  action: string;
  previousStatus?: string;
  newStatus?: string;
  performedBy: string;
  note?: string;
  createdAt: Date;
  icon: string;
  color: string;
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  user: User | null = null;
    dashboardData: DashboardData = {
    stats: {
      totalOrders: 0,
      pendingOrders: 0,
      inProgressOrders: 0,
      completedOrders: 0,
      totalClients: 0,
      newClientsThisMonth: 0,
      totalRevenue: 0,
      averageDeliveryTime: 0
    },
    recentOrders: [],
    ordersByStatus: [],
    ordersByMonth: [],
    revenueByPackage: []
  };
  loading = true;
  private destroy$ = new Subject<void>();

  // Chart data
  statusChartData: any;
  statusChartOptions: any;
  statusChartTotalOrders = 0;
  monthlyChartData: any;
  monthlyChartOptions: any;
  revenueChartData: any;
  revenueChartOptions: any;

  // Table columns
  recentOrdersColumns = [
    { field: 'title', header: 'Title' },
    { field: 'status', header: 'Status' },
    { field: 'createdAt', header: 'Created' },
    { field: 'dueDate', header: 'Due Date' }
  ];

  stats: any[] = [];

  // Client analytics card groups
  clientOrderAnalytics: any[] = [];
  clientProductivityMetrics: any[] = [];
  clientFinancialCards: any[] = [];

  // Compact summary bar metrics (always visible)
  summaryMetrics: any[] = [];

  // Client trend chart
  clientTrendChartData: any;
  clientTrendChartOptions: any;

  // Client-specific data
  invoices: any[] = [];
  galleryItems: any[] = [];
  notifications: any[] = [];
  filteredGalleryItems: any[] = [];
  gallerySearchTerm = '';
  invoicesPanelCollapsed = true;
  selectedInvoiceTabIndex = 0;
  selectedInvoices: any[] = [];
  showPaymentDialog = false;
  paymentMethod = '';
  reportMenu: any[] = [];

  // Action Required panel
  actionRequiredItems: ActionRequiredItem[] = [];
  actionRequiredCollapsed = false;

  // Notifications panel (collapsed by default)
  notificationsPanelCollapsed = true;

  // Recent Orders (Super Admin) - expand/collapse
  recentOrdersCollapsed = false;

  // Order Activity Timeline
  showTimelineDialog = false;
  timelineOrderTitle = '';
  timelineEvents: OrderTimelineEvent[] = [];
  loadingTimeline = false;

  // Full Order History
  allOrders: Order[] = [];
  filteredOrders: Order[] = [];
  orderStatusFilter = '';
  orderSearchFilter = '';
  showArchived = false;
  orderStatusOptions = [
    { label: 'All Statuses', value: '' },
    { label: 'In Progress', value: 'InProgress' },
    { label: 'Preview Delivered', value: 'PreviewDelivered' },
    { label: 'Revision Requested', value: 'RevisionRequested' },
    { label: 'Completed', value: 'Completed' },
    { label: 'Waiting Approval', value: 'WaitingForAdminApproval' },
    { label: 'Price Approval', value: 'PriceApprovalPending' },
    { label: 'Cancelled', value: 'Cancelled' },
    { label: 'Archived', value: 'Archived' }
  ];

  // Quick Reorder
  reorderData: any = null;

  // Client preferences (admin-only editable, client read-only)
  clientPreferences: any = null;
  clientNotes = '';
  savingPreferences = false;

  // Security info
  lastLoginDate: Date | null = null;
  accountCreatedDate: Date | null = null;
  showChangePasswordDialog = false;
  passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' };
  changingPassword = false;

  // Financial snapshot
  financialSnapshot = {
    lifetimeSpend: 0,
    monthlySpend: 0,
    averageOrderValue: 0,
    totalInvoicesPaid: 0,
    totalInvoicesPending: 0
  };

  notificationUnreadCount$!: Observable<number>;

  constructor(
    private authService: AuthService,
    private dashboardService: DashboardService,
    private adminAnalytics: AdminAnalyticsService,
    private notificationService: NotificationService,
    private realtimeNotification: RealtimeNotificationService,
    private messageService: MessageService,
    private router: Router,
    private apiService: ApiService
  ) {
    this.notificationUnreadCount$ = this.notificationService.unreadCount$;
  }

  ngOnInit(): void {
    // Initialize stats with default values
    this.updateStats(this.dashboardData);
    
    // Setup report menu
    this.reportMenu = [
      {
        label: 'Download CSV',
        icon: 'pi pi-file',
        command: () => this.downloadInvoiceReport('csv')
      },
      {
        label: 'Download PDF',
        icon: 'pi pi-file-pdf',
        command: () => this.downloadInvoiceReport('pdf')
      }
    ];
    
    // Load user first
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.user = user;
        // Always load dashboard data, even if user is null (will show empty state)
        this.loadDashboardData();
      });

    // Real-time order updates: refresh dashboard when order events arrive
    this.realtimeNotification.orderUpdates$
      .pipe(takeUntil(this.destroy$))
      .subscribe((data) => this.handleOrderUpdate(data));
  }

  private handleOrderUpdate(data?: { orderId?: string }): void {
    // On reconnect, always refresh to recover from missed events
    if (data?.orderId === '**reconnect**' || this.user) {
      this.loadDashboardData();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadDashboardData(): void {
    this.loading = true;
    const isAdmin = this.authService.getCurrentUser()?.role === 'SuperAdmin' || this.authService.getCurrentUser()?.role === 'Admin';

    if (isAdmin) {
      forkJoin({
        dashboard: this.dashboardService.getDashboardData(),
        overview: this.adminAnalytics.getOverview().pipe(catchError(() => of(null))),
        orderAnalytics: this.adminAnalytics.getOrderAnalytics().pipe(catchError(() => of(null))),
        revenueAnalytics: this.adminAnalytics.getRevenueAnalytics().pipe(catchError(() => of(null)))
      })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: ({ dashboard: data, overview, orderAnalytics, revenueAnalytics }) => {
            this.dashboardData = data;
            if (overview) {
              const completedFromStatus = orderAnalytics?.ordersByStatus?.find((s: any) => s.status === 'Completed')?.count ?? 0;
              this.dashboardData.stats = {
                ...this.dashboardData.stats,
                totalOrders: overview.totalOrders,
                pendingOrders: overview.pendingOrders,
                inProgressOrders: overview.ordersInProgress,
                completedOrders: completedFromStatus,
                totalClients: overview.totalClients,
                newClientsThisMonth: this.dashboardData.stats.newClientsThisMonth,
                totalRevenue: overview.totalRevenue,
                averageDeliveryTime: overview.averageDeliveryTimeDays
              };
            }
            if (orderAnalytics) {
              this.dashboardData.ordersByStatus = orderAnalytics.ordersByStatus || [];
              this.dashboardData.ordersByMonth = (orderAnalytics.ordersTrend || []).map((t: any) => ({
                month: t.monthKey,
                count: t.count,
                completedCount: t.completedCount ?? 0
              }));
            }
            if (revenueAnalytics?.revenueByPackage?.length) {
              this.dashboardData.revenueByPackage = revenueAnalytics.revenueByPackage.map((p: any) => ({
                package: p.package,
                revenue: p.revenue
              }));
            }
            this.updateStats(this.dashboardData);
            this.setupCharts(this.dashboardData);
            this.notifications = data.notifications || [];
            this.loading = false;
          },
          error: () => {
            this.dashboardService.getDashboardData().pipe(takeUntil(this.destroy$)).subscribe({
              next: (d) => { this.dashboardData = d; this.updateStats(d); this.setupCharts(d); this.notifications = d.notifications || []; this.loading = false; },
              error: () => { this.loading = false; this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Unable to load dashboard data.' }); }
            });
          }
        });
    } else {
      this.dashboardService.getDashboardData()
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (data) => {
            this.dashboardData = data;
            this.updateStats(data);
            this.setupCharts(data);
          // Load client-specific data
          // Notifications for both Client and Admin
          this.notifications = data.notifications || [];
          if (this.user?.role === 'Client') {
            this.invoices = data.invoices || [];
            this.galleryItems = data.galleryItems || [];
            this.filteredGalleryItems = this.galleryItems;
            this.notifications = (data.notifications || []).filter((n: any) => 
              n.type?.includes('Order') || n.type?.includes('Invoice')
            );
            // Build action required items
            this.buildActionRequiredItems(data);
            // Build full order history (use allOrders for complete list)
            this.allOrders = [...(data.allOrders || data.recentOrders || [])];
            this.applyOrderFilters();
            // Build financial snapshot
            this.buildFinancialSnapshot(data);
            // Load security info
            this.loadSecurityInfo();
          }
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading dashboard data:', error);
          // Set empty data on error so UI still renders
          this.dashboardData = {
            stats: {
              totalOrders: 0,
              pendingOrders: 0,
              inProgressOrders: 0,
              completedOrders: 0,
              totalClients: 0,
              newClientsThisMonth: 0,
              totalRevenue: 0,
              averageDeliveryTime: 0
            },
            recentOrders: [],
            ordersByStatus: [],
            ordersByMonth: [],
            revenueByPackage: []
          };
          this.updateStats(this.dashboardData);
          this.loading = false;
          this.messageService.add({
            severity: 'warn',
            summary: 'Warning',
            detail: 'Unable to load dashboard data. Showing empty state.'
          });
        }
      });
    }
  }

  private updateStats(data: DashboardData): void {
    const user = this.authService.getCurrentUser();
    // Fixed: Use optional chaining to handle null user
    const isAdmin = user?.role === 'SuperAdmin' || user?.role === 'Admin';
    const isClient = user?.role === 'Client';

    if (isClient) {
      // Order Analytics row
      this.clientOrderAnalytics = [
        { label: 'Total Orders', value: data.stats.totalOrders, icon: 'pi pi-list', color: 'primary', clickable: true, route: '/orders' },
        { label: 'This Month', value: data.stats.ordersThisMonth || 0, icon: 'pi pi-calendar', color: 'info', clickable: true, route: '/orders' },
        { label: 'This Week', value: data.stats.ordersThisWeek || 0, icon: 'pi pi-clock', color: 'warning', clickable: true, route: '/orders' },
        { label: 'Today', value: data.stats.ordersToday || 0, icon: 'pi pi-bolt', color: 'success', clickable: true, route: '/orders' }
      ];

      // Productivity & Spend row
      this.clientProductivityMetrics = [
        { label: 'Completed This Month', value: data.stats.completedOrdersThisMonth || 0, icon: 'pi pi-check-circle', color: 'success', clickable: true, route: '/orders' },
        { label: 'Lifetime Spend', value: data.stats.lifetimeSpend || 0, icon: 'pi pi-wallet', color: 'primary', isCurrency: true, clickable: true, route: '/invoices' },
        { label: 'Monthly Spend', value: data.stats.monthlySpend || 0, icon: 'pi pi-chart-line', color: 'info', isCurrency: true, clickable: true, route: '/invoices' },
        { label: 'Avg Order Value', value: data.stats.averageOrderValue || 0, icon: 'pi pi-percentage', color: 'warning', isCurrency: true, clickable: true, route: '/orders' }
      ];

      // Quick Status row (existing cards, trimmed)
      this.clientFinancialCards = [
        { label: 'Active Orders', value: data.stats.activeOrders || 0, icon: 'pi pi-shopping-cart', color: 'info', clickable: true, route: '/orders' },
        { label: 'Awaiting Approval', value: data.stats.ordersAwaitingApproval || 0, icon: 'pi pi-hourglass', color: 'warning', clickable: true, route: '/orders' },
        { label: 'Pending Invoices', value: data.stats.pendingInvoices || 0, icon: 'pi pi-file', color: 'danger', clickable: true, route: '#invoices-panel' },
        { label: 'Due Amount', value: data.stats.overdueAmount || 0, icon: 'pi pi-dollar', color: 'danger', isCurrency: true, clickable: true, route: '#invoices-panel' },
        { label: 'Paid This Month', value: data.stats.amountPaidThisMonth || 0, icon: 'pi pi-money-bill', color: 'success', isCurrency: true, clickable: true, route: '#invoices-panel' }
      ];

      // Compact summary bar (always visible at top)
      this.summaryMetrics = [
        { label: 'Total Orders', value: data.stats.totalOrders, icon: 'pi pi-list', color: 'primary', isCurrency: false, route: '/orders' },
        { label: 'Active', value: data.stats.activeOrders || 0, icon: 'pi pi-shopping-cart', color: 'info', isCurrency: false, route: '/orders' },
        { label: 'Pending Invoices', value: data.stats.pendingInvoices || 0, icon: 'pi pi-file', color: 'warning', isCurrency: false, route: '#invoices-panel' },
        { label: 'Due Amount', value: data.stats.overdueAmount || 0, icon: 'pi pi-dollar', color: 'danger', isCurrency: true, route: '#invoices-panel' },
        { label: 'Lifetime Spend', value: data.stats.lifetimeSpend || 0, icon: 'pi pi-wallet', color: 'success', isCurrency: true, route: '#invoices-panel' }
      ];

      // Keep stats empty for client (we use the grouped arrays above)
      this.stats = [];

      // Setup client trend chart
      this.setupClientTrendChart(data);
    } else {
      // Admin/Designer stats
      this.stats = [
        { 
          label: 'Total Orders', 
          value: data.stats.totalOrders, 
          icon: 'pi pi-shopping-cart', 
          color: 'primary',
          trend: null,
          clickable: true,
          route: '/orders'
        },
        { 
          label: 'Pending Orders', 
          value: data.stats.pendingOrders, 
          icon: 'pi pi-clock', 
          color: 'warning',
          trend: null,
          clickable: true,
          route: '/orders'
        },
        { 
          label: 'In Progress', 
          value: data.stats.inProgressOrders, 
          icon: 'pi pi-spinner', 
          color: 'info',
          trend: null,
          clickable: true,
          route: '/orders'
        },
        { 
          label: 'Completed', 
          value: data.stats.completedOrders, 
          icon: 'pi pi-check-circle', 
          color: 'success',
          trend: null,
          clickable: true,
          route: '/orders'
        }
      ];
    }

    // Add business metrics for Admin/SuperAdmin
    if (isAdmin) {
      this.stats.push(
        { 
          label: 'Total Clients', 
          value: data.stats.totalClients, 
          icon: 'pi pi-users', 
          color: 'primary',
          trend: null,
          clickable: true,
          route: '/clients'
        },
        { 
          label: 'New Clients (This Month)', 
          value: data.stats.newClientsThisMonth, 
          icon: 'pi pi-user-plus', 
          color: 'info',
          trend: null,
          clickable: true,
          route: '/clients'
        },
        { 
          label: 'Total Revenue', 
          value: data.stats.totalRevenue, 
          icon: 'pi pi-dollar', 
          color: 'secondary',
          trend: null,
          clickable: true,
          route: '/invoices',
          isCurrency: true
        },
        { 
          label: 'Avg Delivery Time', 
          value: data.stats.averageDeliveryTime, 
          icon: 'pi pi-calendar', 
          color: 'success',
          trend: null,
          clickable: false,
          suffix: ' days'
        }
      );
    }
  }

  private setupCharts(data: DashboardData): void {
    // Status Chart (Donut) - count, percentage, total in center, improved tooltip
    if (data.ordersByStatus && data.ordersByStatus.length > 0) {
      const statusLabels = data.ordersByStatus.map(s => this.formatStatus(s.status));
      const statusValues = data.ordersByStatus.map(s => s.count);
      const statusColors = this.getStatusColors(data.ordersByStatus.map(s => s.status));
      const totalOrders = statusValues.reduce((a, b) => a + b, 0);

      this.statusChartData = {
        labels: statusLabels,
        datasets: [{
          data: statusValues,
          backgroundColor: statusColors,
          borderWidth: 2,
          borderColor: '#ffffff'
        }]
      };

      this.statusChartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '65%',
        layout: {
          padding: { bottom: 10 }
        },
        plugins: {
          legend: {
            position: 'bottom',
            labels: {
              padding: 16,
              usePointStyle: true,
              pointStyleWidth: 10,
              boxWidth: 8,
              boxHeight: 8,
              font: { size: 12 }
            }
          },
          tooltip: {
            backgroundColor: '#1e293b',
            titleColor: '#f8fafc',
            bodyColor: '#e2e8f0',
            borderColor: '#334155',
            borderWidth: 1,
            padding: 12,
            displayColors: true,
            callbacks: {
              label: (context: any) => {
                const value = context.parsed || 0;
                const percentage = totalOrders > 0 ? ((value / totalOrders) * 100).toFixed(1) : '0';
                return ` ${context.label}: ${value} orders (${percentage}%)`;
              },
              afterBody: () => `\nTotal: ${totalOrders} orders`
            }
          },
        }
      };
      this.statusChartTotalOrders = totalOrders;
    } else {
      this.statusChartData = null;
      this.statusChartOptions = {};
      this.statusChartTotalOrders = 0;
    }

    // Orders Trend (Line Chart) - last 6 months, monthly count + completed line
    if (data.ordersByMonth && data.ordersByMonth.length > 0) {
      const monthLabels = data.ordersByMonth.map(m => this.formatMonth(m.month));
      const monthValues = data.ordersByMonth.map(m => m.count);
      const completedValues = data.ordersByMonth.map(m => m.completedCount ?? 0);

      this.monthlyChartData = {
        labels: monthLabels,
        datasets: [
          {
            label: 'Total Orders',
            data: monthValues,
            fill: true,
            borderColor: '#0d47a1',
            backgroundColor: 'rgba(13, 71, 161, 0.1)',
            tension: 0.4,
            pointBackgroundColor: '#0d47a1',
            pointBorderColor: '#ffffff',
            pointBorderWidth: 2,
            pointRadius: 5
          },
          {
            label: 'Completed',
            data: completedValues,
            fill: false,
            borderColor: '#10b981',
            backgroundColor: 'transparent',
            tension: 0.4,
            pointBackgroundColor: '#10b981',
            pointBorderColor: '#ffffff',
            pointBorderWidth: 2,
            pointRadius: 5
          }
        ]
      };
    } else {
      this.monthlyChartData = null;
    }

    this.monthlyChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'top',
          labels: { padding: 16, usePointStyle: true }
        },
        tooltip: {
          backgroundColor: '#ffffff',
          titleColor: '#0f172a',
          bodyColor: '#64748b',
          borderColor: '#e2e8f0',
          borderWidth: 1,
          padding: 12,
          callbacks: {
            label: (context: any) => `${context.dataset.label}: ${context.parsed.y} orders`
          }
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: { stepSize: 1 },
          grid: { color: '#e2e8f0' }
        },
        x: {
          grid: { display: false }
        }
      }
    };

    // Revenue by Package Chart (Horizontal Bar) - Basic, Standard, Premium, Custom
    if (data.revenueByPackage && data.revenueByPackage.length > 0) {
      const packageLabels = data.revenueByPackage.map(p => p.package);
      const revenueValues = data.revenueByPackage.map(p => p.revenue);

      this.revenueChartData = {
        labels: packageLabels,
        datasets: [{
          label: 'Revenue',
          data: revenueValues,
          backgroundColor: ['#0d47a1', '#1976d2', '#4caf50', '#ff9800'],
          borderColor: ['#0a3d91', '#1565c0', '#45a049', '#f57c00'],
          borderWidth: 2
        }]
      };
    } else {
      this.revenueChartData = null;
    }

    this.revenueChartOptions = {
      indexAxis: 'y',
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: '#ffffff',
          titleColor: '#0f172a',
          bodyColor: '#64748b',
          borderColor: '#e2e8f0',
          borderWidth: 1,
          padding: 12,
          callbacks: {
            label: (context: any) => {
              const val = context.parsed.x ?? context.parsed.y ?? 0;
              return `Revenue: $${Number(val).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
            }
          }
        }
      },
      scales: {
        x: {
          beginAtZero: true,
          ticks: {
            callback: (value: any) => '$' + Number(value).toLocaleString('en-US')
          },
          grid: { color: '#e2e8f0' }
        },
        y: {
          grid: { display: false }
        }
      }
    };
  }

  private setupClientTrendChart(data: DashboardData): void {
    const weeks = data.ordersByWeek || [];
    if (weeks.length === 0) {
      this.clientTrendChartData = null;
      return;
    }

    this.clientTrendChartData = {
      labels: weeks.map(w => w.weekLabel),
      datasets: [{
        label: 'Orders',
        data: weeks.map(w => w.count),
        backgroundColor: 'rgba(13, 71, 161, 0.7)',
        borderColor: '#0d47a1',
        borderWidth: 2,
        borderRadius: 6,
        barPercentage: 0.6,
        categoryPercentage: 0.7
      }]
    };

    this.clientTrendChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: '#ffffff',
          titleColor: '#0f172a',
          bodyColor: '#64748b',
          borderColor: '#e2e8f0',
          borderWidth: 1,
          padding: 12,
          callbacks: {
            label: (context: any) => `${context.parsed.y} order${context.parsed.y !== 1 ? 's' : ''}`
          }
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: { stepSize: 1, font: { size: 12 } },
          grid: { color: '#e2e8f0' }
        },
        x: {
          grid: { display: false },
          ticks: { font: { size: 11 } }
        }
      }
    };
  }

  formatStatus(status: string): string {
    if (status === 'ClientApproved') return 'Approved';
    return status.replace(/([A-Z])/g, ' $1').trim();
  }

  private formatMonth(monthKey: string): string {
    const [year, month] = monthKey.split('-');
    const date = new Date(parseInt(year), parseInt(month) - 1);
    return date.toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
  }

  private getStatusColors(statuses: string[]): string[] {
    const colorMap: { [key: string]: string } = {
      'Pending': '#f59e0b',
      'WaitingForAdminApproval': '#f97316',
      'PriceApprovalPending': '#eab308',
      'InProgress': '#0d47a1',
      'PreviewDelivered': '#8b5cf6',
      'RevisionRequested': '#6366f1',
      'ClientApproved': '#14b8a6',
      'Completed': '#10b981',
      'Cancelled': '#ef4444',
      'CancelledByUser': '#dc2626',
      'CancelledByAdmin': '#b91c1c',
      'Paid': '#059669',
      'Processing': '#06b6d4',
      'Refunded': '#6b7280',
      'Failed': '#991b1b',
      'Archived': '#9ca3af',
      'Review': '#8b5cf6'
    };
    return statuses.map(s => colorMap[s] || '#64748b');
  }

  getStatusSeverity(status: OrderStatus): string {
    const severityMap: { [key: string]: string } = {
      'Pending': 'warning',
      'InProgress': 'info',
      'Review': 'secondary',
      'PreviewDelivered': 'info',
      'RevisionRequested': 'warning',
      'ClientApproved': 'success',
      'Completed': 'success',
      'Cancelled': 'danger',
      'CancelledByUser': 'danger',
      'CancelledByAdmin': 'danger'
    };
    return severityMap[status] || 'secondary';
  }

  /** Count of orders in "other" statuses (not Pending, In Progress, or Completed) */
  getOtherOrdersCount(): number {
    const s = this.dashboardData.stats;
    const other = (s.totalOrders || 0) - (s.pendingOrders || 0) - (s.activeOrders || 0) - (s.completedOrders || 0);
    return Math.max(0, other);
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  // Order Detail Modal
  showOrderDetailModal = false;
  selectedOrderId: string | null = null;

  navigateToOrder(orderId: string): void {
    this.selectedOrderId = orderId;
    this.showOrderDetailModal = true;
  }

  /** Navigate to the appropriate page when clicking a notification */
  onNotificationClick(notification: any): void {
    const url = notification.redirectUrl?.trim();
    if (url) {
      this.router.navigateByUrl(url.startsWith('/') ? url : `/${url}`);
      return;
    }
    const refType = (notification.referenceType ?? 'Order').toLowerCase();
    const refId = notification.referenceId ?? notification.orderId;
    const orderId = notification.orderId ?? (refType === 'order' ? refId : null);
    switch (refType) {
      case 'order':
        if (refId) this.router.navigate(['/orders', refId]);
        break;
      case 'invoice':
        if (refId) this.router.navigate(['/invoices', refId]);
        break;
      case 'message':
        if (orderId) this.router.navigate(['/orders', orderId]);
        else if (refId) this.router.navigate(['/messages']);
        break;
      case 'system':
        this.router.navigate(['/users']);
        break;
      default:
        if (orderId || refId) this.router.navigate(['/orders', orderId ?? refId]);
    }
  }

  onOrderDetailClose(): void {
    this.showOrderDetailModal = false;
    this.selectedOrderId = null;
  }

  onOrderUpdated(): void {
    // Refresh dashboard data when order is updated
    this.loadDashboardData();
  }

  canGenerateInvoice(order: Order): boolean {
    const isAdmin = this.user?.role === 'SuperAdmin' || this.user?.role === 'Admin';
    return !!isAdmin
      && (order.status === OrderStatus.Completed || order.status === OrderStatus.ClientApproved)
      && !order.hasInvoice;
  }

  generateInvoice(order: Order): void {
    this.apiService.post('invoices', { orderId: order.id })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Invoice generated successfully' });
          this.loadDashboardData();
        },
        error: (error) => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error?.error || 'Failed to generate invoice' });
        }
      });
  }

  // Order Create Modal
  showOrderCreateModal = false;

  navigateToOrders(): void {
    this.router.navigate(['/orders']);
  }

  openCreateOrderModal(): void {
    this.reorderData = null; // Reset prefill data for fresh order
    this.showOrderCreateModal = true;
  }

  onOrderCreateClose(): void {
    this.showOrderCreateModal = false;
  }

  onOrderCreated(order: any): void {
    // Refresh dashboard data when order is created
    this.loadDashboardData();
    // Optionally open the created order in detail modal
    if (order && order.id) {
      this.selectedOrderId = order.id;
      this.showOrderDetailModal = true;
    }
  }

  isOverdue(dueDate: Date | string | undefined): boolean {
    if (!dueDate) return false;
    return new Date(dueDate) < new Date() && new Date(dueDate).getTime() !== 0;
  }

  navigateToRoute(route: string): void {
    if (route === '#invoices-panel') {
      this.scrollToInvoicesPanel();
      return;
    }
    this.router.navigate([route]);
  }

  scrollToInvoicesPanel(): void {
    // Expand the panel if collapsed
    this.invoicesPanelCollapsed = false;
    // Scroll to the panel after it expands
    setTimeout(() => {
      const el = document.getElementById('invoices-panel');
      if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    }, 200);
  }

  // Client-specific methods
  canApproveOrder(order: Order): boolean {
    return order.status === OrderStatus.PreviewDelivered || 
           order.status === OrderStatus.PriceApprovalPending;
  }

  canRequestRevision(order: Order): boolean {
    return order.status === OrderStatus.PreviewDelivered && !order.revisionLimitExceeded;
  }

  approveOrder(order: Order): void {
    if (order.status === OrderStatus.PriceApprovalPending) {
      // Approve price
      this.apiService.post(`orders/${order.id}/approve-price`, { approved: true })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Price approved successfully'
            });
            this.loadDashboardData();
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: error.error?.error || 'Failed to approve price'
            });
          }
        });
    } else if (order.status === OrderStatus.PreviewDelivered) {
      // Approve final design
      this.apiService.put(`orders/${order.id}/status`, { status: OrderStatus.ClientApproved })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Order approved successfully'
            });
            this.loadDashboardData();
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: error.error?.error || 'Failed to approve order'
            });
          }
        });
    }
  }

  requestRevision(order: Order): void {
    this.selectedOrderId = order.id;
    this.showOrderDetailModal = true;
    // The order detail modal should have revision request functionality
  }

  filterGallery(): void {
    if (!this.gallerySearchTerm) {
      this.filteredGalleryItems = this.galleryItems;
      return;
    }
    const search = this.gallerySearchTerm.toLowerCase();
    this.filteredGalleryItems = this.galleryItems.filter(item =>
      item.orderTitle?.toLowerCase().includes(search) ||
      item.originalFileName?.toLowerCase().includes(search) ||
      item.format?.toLowerCase().includes(search)
    );
  }

  downloadGalleryFile(item: any): void {
    const fileId = item.fileId || item.id;
    const fileName = item.originalFileName || item.fileName || `file-${fileId}`;
    this.apiService.getBlob(`files/${fileId}/download`).subscribe({
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

  getSelectedInvoicesTotal(): number {
    return this.selectedInvoices.reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0);
  }

  cancelPayment(): void {
    this.showPaymentDialog = false;
    this.selectedInvoices = [];
    this.paymentMethod = '';
  }

  openPaymentDialog(invoice?: any): void {
    if (invoice) {
      this.selectedInvoices = [invoice];
    }
    this.showPaymentDialog = true;
  }

  processPayment(): void {
    if (this.selectedInvoices.length === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select at least one invoice'
      });
      return;
    }

    // Process payments (for multiple invoices or manual payment)
    const paymentPromises = this.selectedInvoices.map(invoice =>
      firstValueFrom(
        this.apiService.put(`invoices/${invoice.id}/mark-paid`, {
          paymentMethod: this.paymentMethod || 'Manual'
        })
      )
    );

    Promise.all(paymentPromises)
      .then(() => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `Payment processed for ${this.selectedInvoices.length} invoice(s)`
        });
        this.showPaymentDialog = false;
        this.selectedInvoices = [];
        this.paymentMethod = '';
        this.loadDashboardData();
      })
      .catch((error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.error || 'Failed to process payment'
        });
      });
  }

  onPaymentComplete(): void {
    // Called when payment component completes payment
    this.showPaymentDialog = false;
    this.selectedInvoices = [];
    this.paymentMethod = '';
    this.loadDashboardData();
  }

  downloadInvoiceReport(format: 'csv' | 'pdf'): void {
    const invoices = this.getInvoicesForCurrentTab();
    if (format === 'csv') {
      this.downloadCSV(invoices);
    } else {
      this.downloadPDF(invoices);
    }
  }

  private getInvoicesForCurrentTab(): any[] {
    switch (this.selectedInvoiceTabIndex) {
      case 0:
        return this.getUnpaidInvoices();
      case 1:
        return this.getPendingInvoices();
      case 2:
        return this.getDueInvoices();
      case 3:
        return this.getPaidInvoices();
      default:
        return [];
    }
  }

  private downloadCSV(invoices: any[]): void {
    const headers = ['Invoice Number', 'Amount', 'Status', 'Due Date', 'Paid Date'];
    const rows = invoices.map(inv => [
      inv.invoiceNumber,
      inv.totalAmount || inv.amount,
      inv.status,
      inv.dueDate ? new Date(inv.dueDate).toLocaleDateString() : 'N/A',
      inv.paidDate ? new Date(inv.paidDate).toLocaleDateString() : 'N/A'
    ]);

    const csvContent = [
      headers.join(','),
      ...rows.map(row => row.map(cell => `"${cell}"`).join(','))
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    const tabNames = ['unpaid', 'pending', 'due', 'paid'];
    link.download = `invoices_${tabNames[this.selectedInvoiceTabIndex] || 'all'}_${new Date().toISOString().split('T')[0]}.csv`;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  private downloadPDF(invoices: any[]): void {
    // Map tab index to status for the backend filter
    const statusMap: { [key: number]: string } = {
      0: '', // Unpaid = all non-paid
      1: 'Pending',
      2: 'Overdue',
      3: 'Paid'
    };
    const status = statusMap[this.selectedInvoiceTabIndex] ?? '';
    const endpoint = status ? `invoices/report?status=${status}` : 'invoices/report';

    this.apiService.getBlob(endpoint)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          const tabNames = ['unpaid', 'pending', 'due', 'paid'];
          link.download = `invoices_${tabNames[this.selectedInvoiceTabIndex] || 'all'}_${new Date().toISOString().split('T')[0]}.pdf`;
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
          window.URL.revokeObjectURL(url);
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Invoice report PDF downloaded successfully'
          });
        },
        error: (error) => {
          console.error('Error downloading invoice report PDF:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to download invoice report PDF'
          });
        }
      });
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

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  // ── Action Required Panel ──────────────────────────────────────────

  private buildActionRequiredItems(data: DashboardData): void {
    const items: ActionRequiredItem[] = [];
    const orders = data.allOrders || data.recentOrders || [];

    // Orders awaiting approval (PreviewDelivered)
    orders.filter(o => o.status === OrderStatus.PreviewDelivered).forEach(order => {
      items.push({
        type: 'approval',
        icon: 'pi pi-check-circle',
        severity: 'success',
        title: `Approve "${order.title}"`,
        subtitle: 'Preview delivered — review and approve the final design',
        actionLabel: 'Review & Approve',
        actionIcon: 'pi pi-eye',
        data: order
      });
    });

    // Orders awaiting client price approval
    orders.filter(o => o.status === OrderStatus.PriceApprovalPending).forEach(order => {
      items.push({
        type: 'price',
        icon: 'pi pi-dollar',
        severity: 'warning',
        title: `Price approval for "${order.title}"`,
        subtitle: `Proposed price: $${(order as any).proposedPrice || order.price || 0}`,
        actionLabel: 'Review Price',
        actionIcon: 'pi pi-eye',
        data: order
      });
    });

    // Designer price approval needed (Admin/SuperAdmin only) - orders with designer proposed price pending
    const isAdmin = this.user?.role === 'SuperAdmin' || this.user?.role === 'Admin';
    if (isAdmin) {
      orders.filter(o =>
        o.requiresPriceApproval &&
        (o.priceApprovalStatus === 'PendingApproval' || o.priceApprovalStatus === 'Modified')
      ).forEach(order => {
        items.push({
          type: 'designerPrice',
          icon: 'pi pi-money-bill',
          severity: 'warning',
          title: `Designer price approval for "${order.title}"`,
          subtitle: `Designer proposed PKR ${(order as any).proposedPrice || 0} — Approve to add to designer invoice`,
          actionLabel: 'Approve Price',
          actionIcon: 'pi pi-check',
          data: order
        });
      });
    }

    // Orders with revision requests pending
    orders.filter(o => o.status === OrderStatus.RevisionRequested).forEach(order => {
      items.push({
        type: 'revision',
        icon: 'pi pi-refresh',
        severity: 'info',
        title: `Revision in progress: "${order.title}"`,
        subtitle: 'Your revision request is being worked on',
        actionLabel: 'View Details',
        actionIcon: 'pi pi-eye',
        data: order
      });
    });

    // Overdue invoices
    (this.invoices || []).filter(inv => inv.status === 'Overdue').forEach(inv => {
      items.push({
        type: 'invoice',
        icon: 'pi pi-exclamation-triangle',
        severity: 'danger',
        title: `Overdue Invoice #${inv.invoiceNumber}`,
        subtitle: `Amount: ${this.formatCurrency(inv.totalAmount || inv.amount)} — Due: ${this.formatDate(inv.dueDate)}`,
        actionLabel: 'Pay Now',
        actionIcon: 'pi pi-money-bill',
        data: inv
      });
    });

    // Pending invoices approaching due date
    (this.invoices || []).filter(inv => {
      if (inv.status !== 'Pending' || !inv.dueDate) return false;
      const daysUntilDue = Math.ceil((new Date(inv.dueDate).getTime() - Date.now()) / (1000 * 60 * 60 * 24));
      return daysUntilDue <= 7 && daysUntilDue >= 0;
    }).forEach(inv => {
      items.push({
        type: 'invoice',
        icon: 'pi pi-clock',
        severity: 'warning',
        title: `Invoice #${inv.invoiceNumber} due soon`,
        subtitle: `Amount: ${this.formatCurrency(inv.totalAmount || inv.amount)} — Due: ${this.formatDate(inv.dueDate)}`,
        actionLabel: 'Pay Now',
        actionIcon: 'pi pi-money-bill',
        data: inv
      });
    });

    this.actionRequiredItems = items;
  }

  handleActionRequired(item: ActionRequiredItem): void {
    switch (item.type) {
      case 'approval':
      case 'revision':
      case 'price':
      case 'designerPrice':
        this.navigateToOrder(item.data.id);
        break;
      case 'invoice':
        this.openPaymentDialog(item.data);
        break;
    }
  }

  // ── Order Activity Timeline ────────────────────────────────────────

  openTimeline(order: Order): void {
    this.showTimelineDialog = true;
    this.timelineOrderTitle = order.title;
    this.timelineEvents = [];
    this.loadingTimeline = true;

    this.apiService.get<any[]>(`orders/${order.id}/logs`)
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => {
          this.messageService.add({
            severity: 'warn',
            summary: 'Warning',
            detail: 'Could not load activity timeline'
          });
          return of([]);
        })
      )
      .subscribe(logs => {
        this.timelineEvents = (logs || [])
          .filter(log => {
            // Exclude internal admin/designer/QA details
            const role = (log.performedBy || '').toLowerCase();
            return !role.includes('designer') && !role.includes('qa');
          })
          .map(log => ({
            action: this.formatTimelineAction(log.action),
            previousStatus: log.previousStatus ? this.formatStatus(log.previousStatus) : undefined,
            newStatus: log.newStatus ? this.formatStatus(log.newStatus) : undefined,
            performedBy: this.sanitizePerformedBy(log.performedBy),
            note: log.note,
            createdAt: new Date(log.createdAt?.endsWith?.('Z') ? log.createdAt : log.createdAt + 'Z'),
            icon: this.getTimelineIcon(log.action),
            color: this.getTimelineColor(log.action)
          }))
          .sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime());
        this.loadingTimeline = false;
      });
  }

  private formatTimelineAction(action: string): string {
    const actionMap: { [key: string]: string } = {
      'Created': 'Order Created',
      'StatusChanged': 'Status Changed',
      'PriceProposed': 'Price Proposed',
      'PriceApproved': 'Price Approved',
      'PriceRejected': 'Price Rejected',
      'Assigned': 'Designer Assigned',
      'PreviewUploaded': 'Preview Delivered',
      'Approved': 'Design Approved',
      'RevisionRequested': 'Revision Requested',
      'Completed': 'Order Completed',
      'Cancelled': 'Order Cancelled',
      'Refunded': 'Order Refunded',
      'Archived': 'Order Archived'
    };
    return actionMap[action] || action.replace(/([A-Z])/g, ' $1').trim();
  }

  private sanitizePerformedBy(role: string): string {
    // Hide internal roles from client view
    if (!role) return 'System';
    const lower = role.toLowerCase();
    if (lower === 'client') return 'You';
    if (lower === 'system') return 'System';
    return 'Team'; // Generalize admin/designer to "Team"
  }

  private getTimelineIcon(action: string): string {
    const iconMap: { [key: string]: string } = {
      'Created': 'pi pi-plus-circle',
      'StatusChanged': 'pi pi-arrow-right',
      'PriceProposed': 'pi pi-dollar',
      'PriceApproved': 'pi pi-check',
      'PriceRejected': 'pi pi-times',
      'Assigned': 'pi pi-user',
      'PreviewUploaded': 'pi pi-image',
      'Approved': 'pi pi-check-circle',
      'RevisionRequested': 'pi pi-refresh',
      'Completed': 'pi pi-flag',
      'Cancelled': 'pi pi-ban',
      'Refunded': 'pi pi-undo',
      'Archived': 'pi pi-box'
    };
    return iconMap[action] || 'pi pi-circle';
  }

  private getTimelineColor(action: string): string {
    const colorMap: { [key: string]: string } = {
      'Created': '#0d47a1',
      'Approved': '#10b981',
      'PriceApproved': '#10b981',
      'Completed': '#10b981',
      'PreviewUploaded': '#1976d2',
      'Assigned': '#1976d2',
      'RevisionRequested': '#f59e0b',
      'PriceProposed': '#f59e0b',
      'PriceRejected': '#ef4444',
      'Cancelled': '#ef4444',
      'Refunded': '#ef4444'
    };
    return colorMap[action] || '#64748b';
  }

  // ── Full Order History with Filters ────────────────────────────────

  applyOrderFilters(): void {
    let filtered = [...this.allOrders];

    // Status filter
    if (this.orderStatusFilter) {
      if (this.orderStatusFilter === 'Archived') {
        filtered = filtered.filter(o => (o as any).isArchived);
      } else {
        filtered = filtered.filter(o => o.status === this.orderStatusFilter);
      }
    }

    // Text search filter
    if (this.orderSearchFilter) {
      const search = this.orderSearchFilter.toLowerCase();
      filtered = filtered.filter(o =>
        o.title.toLowerCase().includes(search) ||
        o.description?.toLowerCase().includes(search)
      );
    }

    // Archive toggle
    if (!this.showArchived && !this.orderStatusFilter) {
      filtered = filtered.filter(o => !(o as any).isArchived);
    }

    this.filteredOrders = filtered;
  }

  onOrderStatusFilterChange(): void {
    this.applyOrderFilters();
  }

  onOrderSearchFilterChange(): void {
    this.applyOrderFilters();
  }

  toggleShowArchived(): void {
    this.showArchived = !this.showArchived;
    this.applyOrderFilters();
  }

  // ── Quick Reorder (form prefill) ───────────────────────────────────

  quickReorder(order: Order): void {
    const dc = (order as any).designCategory;
    const dt = (order as any).designType;
    let logoCategory: string | null = null;
    let placement: number | null = null;
    if (dc === 'EmbroideryDigitizing') logoCategory = dt === 'LeftChest' || dt === 'JacketBack' ? 'embroidery' : 'digitizing';
    else if (dc === 'VectorScreenPrinting') logoCategory = 'vector';
    else if (dc === 'CustomPatch') logoCategory = 'customPatch';
    if (dt === 'LeftChest') placement = 1;
    else if (dt === 'JacketBack') placement = 2;

    this.reorderData = {
      title: order.title + ' (Reorder)',
      description: order.description,
      price: order.price,
      priority: (order as any).priority || 2,
      logoCategory: logoCategory || undefined,
      placement: placement ?? undefined,
      requiredFormats: (order as any).requiredFormats || '',
      colorPreferences: (order as any).colorPreferences || '',
      stylePreferences: (order as any).stylePreferences || ''
    };
    this.showOrderCreateModal = true;
  }

  // ── Financial Snapshot ─────────────────────────────────────────────

  private buildFinancialSnapshot(data: DashboardData): void {
    this.financialSnapshot = {
      lifetimeSpend: data.stats.lifetimeSpend || 0,
      monthlySpend: data.stats.monthlySpend || 0,
      averageOrderValue: data.stats.averageOrderValue || 0,
      totalInvoicesPaid: this.invoices.filter(inv => inv.status === 'Paid').length,
      totalInvoicesPending: this.invoices.filter(inv => inv.status !== 'Paid').length
    };
  }

  // ── Security Info ──────────────────────────────────────────────────

  private loadSecurityInfo(): void {
    this.accountCreatedDate = this.user?.createdAt ? new Date(this.user.createdAt) : null;
    // Try to load last login from session or API
    const storedLogin = sessionStorage.getItem('lastLogin');
    this.lastLoginDate = storedLogin ? new Date(storedLogin) : new Date();
    // Store current login time for next session
    sessionStorage.setItem('lastLogin', new Date().toISOString());
  }

  openChangePasswordDialog(): void {
    this.passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' };
    this.showChangePasswordDialog = true;
  }

  changePassword(): void {
    if (!this.passwordForm.currentPassword || !this.passwordForm.newPassword || !this.passwordForm.confirmPassword) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please fill in all password fields'
      });
      return;
    }

    if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'New passwords do not match'
      });
      return;
    }

    if (this.passwordForm.newPassword.length < 6) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Password must be at least 6 characters'
      });
      return;
    }

    this.changingPassword = true;
    this.authService.changePassword(this.passwordForm)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Password changed successfully'
          });
          this.showChangePasswordDialog = false;
          this.changingPassword = false;
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to change password'
          });
          this.changingPassword = false;
        }
      });
  }

  // ── Client Preferences & Notes ─────────────────────────────────────

  get isAdmin(): boolean {
    return this.user?.role === 'SuperAdmin' || this.user?.role === 'Admin';
  }

  get isClient(): boolean {
    return this.user?.role === 'Client';
  }


  formatRelativeDate(date: Date | string | null): string {
    if (!date) return 'N/A';
    const dateStr = typeof date === 'string' ? date : date.toISOString();
    // Backend returns UTC dates — ensure we parse as UTC
    const d = new Date(dateStr.endsWith('Z') || dateStr.includes('+') ? dateStr : dateStr + 'Z');
    const now = new Date();
    const diffMs = now.getTime() - d.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    if (diffDays === 0) {
      const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
      if (diffHours === 0) {
        const diffMins = Math.floor(diffMs / (1000 * 60));
        return diffMins <= 1 ? 'Just now' : `${diffMins} minutes ago`;
      }
      return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    }
    if (diffDays === 1) return 'Yesterday';
    if (diffDays < 7) return `${diffDays} days ago`;
    return this.formatDate(date);
  }
}
