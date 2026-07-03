import { Injectable } from '@angular/core';
import { Observable, forkJoin, of } from 'rxjs';
import { map, catchError, shareReplay, switchMap, timeout } from 'rxjs/operators';
import { User, UserRole } from '@shared/models/user.model';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { NotificationService } from './notification.service';
import { SharedListDataService } from './shared-list-data.service';
import { Order, OrderStatus } from '@shared/models/order.model';
import { resolveSingleCurrencyCode } from '@core/utils/currency-format';

/** Client "active" orders — keep in sync with order-list.component getStatusCounts().active */
const CLIENT_ACTIVE_ORDER_STATUSES: readonly OrderStatus[] = [
  OrderStatus.WaitingForAdminApproval,
  OrderStatus.PriceApprovalPending,
  OrderStatus.InProgress,
  OrderStatus.PreviewDelivered,
  OrderStatus.RevisionRequested
];

export interface DashboardStats {
  totalOrders: number;
  pendingOrders: number;
  inProgressOrders: number;
  completedOrders: number;
  totalClients: number;
  newClientsThisMonth: number;
  totalRevenue: number;
  averageDeliveryTime: number; // in days
  // Client-specific stats
  activeOrders?: number;
  ordersAwaitingApproval?: number;
  pendingInvoices?: number;
  dueAmount?: number;
  overdueAmount?: number;
  amountPaidThisMonth?: number;
  // Client analytics & productivity
  ordersThisMonth?: number;
  ordersThisWeek?: number;
  ordersToday?: number;
  completedOrdersThisMonth?: number;
  lifetimeSpend?: number;
  monthlySpend?: number;
  averageOrderValue?: number;
}

export interface DashboardData {
  stats: DashboardStats;
  recentOrders: Order[];
  allOrders?: Order[]; // Full order list for client history
  ordersByStatus: { status: string; count: number }[];
  ordersByMonth: { month: string; count: number; completedCount?: number }[];
  revenueByPackage: { package: string; revenue: number }[];
  // Client-specific data
  invoices?: any[];
  galleryItems?: any[];
  notifications?: any[];
  // Client analytics
  ordersByWeek?: { weekLabel: string; count: number }[];
  /** ISO code for revenue KPIs when all completed orders share one currency (e.g. GBP). */
  revenueCurrencyCode?: string;
  /** True when completed orders use more than one currency (KPIs use USD label). */
  revenueCurrencyMixed?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private dashboardDataCache: { key: string; stream$: Observable<DashboardData> } | null = null;

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private notificationService: NotificationService,
    private sharedListData: SharedListDataService
  ) {}

  getDashboardData(forceFresh = false): Observable<DashboardData> {
    const user = this.authService.getCurrentUser();
    if (!user) {
      return of(this.getEmptyDashboardData());
    }

    const role = this.resolveUserRole(user);

    // Client KPIs must not depend on gallery/invoices forkJoin or shareReplay cache.
    if (role === UserRole.Client) {
      return this.buildClientDashboard$();
    }

    const cacheKey = `${user.id}:${role}`;
    if (!forceFresh && this.dashboardDataCache?.key === cacheKey) {
      return this.dashboardDataCache.stream$;
    }

    // Fetch orders based on user role
    let orders$: Observable<Order[]>;
    
    switch (role) {
      case UserRole.Designer:
        orders$ = this.fetchOrdersForRole('orders/assigned-orders');
        break;
      case UserRole.SuperAdmin:
      case UserRole.Admin:
        // Paginated full scan via shared stream (backend caps pageSize at 100).
        orders$ = this.sharedListData.getAllOrdersAdmin().pipe(
          map((items) => this.normalizeOrdersResponse(items)),
          catchError(() => of([]))
        );
        break;
      default:
        orders$ = of([]);
    }

    // Fetch additional data for admin users
    let clients$: Observable<any[]> = of([]);
    let invoices$: Observable<any[]> = of([]);
    let galleryItems$: Observable<any[]> = of([]);
    let notifications$: Observable<any[]> = of([]);
    
    if (role === UserRole.SuperAdmin || role === UserRole.Admin) {
      // Reuse shared users fetch; filter to clients for dashboard stats.
      clients$ = this.sharedListData.getAllUsers().pipe(
        map((list) =>
          (list as any[]).filter((u: any) => u.role === 'Client' || u.roleName === 'Client')
        ),
        catchError(() => of([]))
      );
      
      // Try to fetch invoices (if endpoint exists - will fail gracefully)
      invoices$ = this.apiService.get<any[]>('invoices').pipe(
        catchError(() => of([]))
      );

      // Fetch notifications for Admin (e.g. "New Order Submitted") - use NotificationService for normalized data
      notifications$ = this.notificationService.getNotifications(false).pipe(
        catchError(() => of([]))
      );
    }

    // forkJoin waits for every source to complete; a single hung HTTP would spin the UI forever.
    const stream$ = forkJoin({
      orders: orders$,
      clients: clients$,
      invoices: invoices$,
      galleryItems: galleryItems$,
      notifications: notifications$
    }).pipe(
      timeout(120000),
      map(({ orders, clients, invoices, galleryItems, notifications }) =>
        this.processDashboardData(orders, clients, invoices, galleryItems, notifications, role)
      ),
      catchError(() => {
        // Do not keep serving a cached empty dashboard after a failed load (e.g. startup timeout).
        this.dashboardDataCache = null;
        return of(this.getEmptyDashboardData());
      }),
      shareReplay({ bufferSize: 1, refCount: true })
    );

    this.dashboardDataCache = { key: cacheKey, stream$ };
    return stream$;
  }

  /** Call after mutations that should refresh admin dashboard aggregates (optional; TTL/cache refCount also clears). */
  invalidateDashboardCache(): void {
    this.dashboardDataCache = null;
    this.sharedListData.clearAll();
  }

  private processDashboardData(
    orders: Order[], 
    clients: any[] = [], 
    invoices: any[] = [],
    galleryItems: any[] = [],
    notifications: any[] = [],
    userRole?: string
  ): DashboardData {
    // Calculate client statistics
    const now = new Date();
    const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);
    const newClientsThisMonth = clients.filter(c => {
      const createdAt = new Date(c.createdAt);
      return createdAt >= startOfMonth;
    }).length;

    // Calculate revenue from completed orders or invoices
    let totalRevenue = 0;
    if (invoices.length > 0) {
      totalRevenue = invoices
        .filter(inv => inv.status === 'Paid')
        .reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0);
    } else {
      // Fallback: calculate from completed orders if price field exists
      totalRevenue = orders
        .filter(o => o.status === OrderStatus.Completed)
        .reduce((sum, o) => sum + ((o as any).price || 0), 0);
    }

    // Calculate average delivery time (for completed orders)
    let averageDeliveryTime = 0;
    const completedOrders = orders.filter(o => o.status === OrderStatus.Completed && o.dueDate);
    if (completedOrders.length > 0) {
      const deliveryTimes = completedOrders.map(o => {
        const created = new Date(o.createdAt);
        const completed = o.dueDate ? new Date(o.dueDate) : new Date();
        return Math.max(0, Math.ceil((completed.getTime() - created.getTime()) / (1000 * 60 * 60 * 24)));
      });
      averageDeliveryTime = deliveryTimes.reduce((sum, time) => sum + time, 0) / deliveryTimes.length;
    }

    // Calculate client-specific statistics
    let activeOrders = 0;
    let ordersAwaitingApproval = 0;
    let pendingInvoices = 0;
    let dueAmount = 0;
    let overdueAmount = 0;
    let amountPaidThisMonth = 0;

    // Client analytics variables
    let ordersThisMonth = 0;
    let ordersThisWeek = 0;
    let ordersToday = 0;
    let completedOrdersThisMonth = 0;
    let lifetimeSpend = 0;
    let monthlySpend = 0;
    let averageOrderValue = 0;
    let ordersByWeek: { weekLabel: string; count: number }[] = [];

    if (userRole === 'Client') {
      // Active = non-completed work in progress (includes Awaiting Admin — same as My Orders page)
      activeOrders = orders.filter(o => CLIENT_ACTIVE_ORDER_STATUSES.includes(o.status)).length;

      // Client action needed on preview/price (excludes WaitingForAdminApproval — admin queue)
      ordersAwaitingApproval = orders.filter(o =>
        o.status === OrderStatus.PreviewDelivered ||
        o.status === OrderStatus.PriceApprovalPending
      ).length;

      // Invoice statistics
      pendingInvoices = invoices.filter(inv => 
        inv.status === 'Pending' || inv.status === 'Overdue'
      ).length;

      // Due amount = total unpaid (Pending + Overdue)
      dueAmount = invoices
        .filter(inv => inv.status === 'Pending' || inv.status === 'Overdue')
        .reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0);

      // Overdue amount = only overdue invoices
      overdueAmount = invoices
        .filter(inv => inv.status === 'Overdue')
        .reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0);

      amountPaidThisMonth = invoices
        .filter(inv => {
          if (inv.status !== 'Paid' || !inv.paidDate) return false;
          const paidDate = new Date(inv.paidDate);
          return paidDate >= startOfMonth;
        })
        .reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0);

      // --- Client Analytics & Productivity ---
      const today = new Date();
      const startOfToday = new Date(today.getFullYear(), today.getMonth(), today.getDate());
      const dayOfWeek = today.getDay(); // 0=Sun
      const startOfWeek = new Date(today);
      startOfWeek.setDate(today.getDate() - dayOfWeek);
      startOfWeek.setHours(0, 0, 0, 0);

      // Orders this month (by creation date)
      ordersThisMonth = orders.filter(o => new Date(o.createdAt) >= startOfMonth).length;

      // Orders this week (by creation date)
      ordersThisWeek = orders.filter(o => new Date(o.createdAt) >= startOfWeek).length;

      // Orders today (by creation date)
      ordersToday = orders.filter(o => new Date(o.createdAt) >= startOfToday).length;

      // Completed orders this month (by updatedAt or createdAt — closest to completion date)
      completedOrdersThisMonth = orders.filter(o => {
        if (o.status !== OrderStatus.Completed) return false;
        const completionDate = new Date(o.updatedAt || o.createdAt);
        return completionDate >= startOfMonth;
      }).length;

      // Lifetime spend = SUM(Paid Invoice Amounts) — what the client has actually paid.
      // Fallback: SUM(Order Price) for non-cancelled orders, minus refunds.
      if (invoices.length > 0) {
        lifetimeSpend = invoices
          .filter(inv => inv.status === 'Paid')
          .reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0);
      } else {
        const cancelledStatuses = [OrderStatus.Cancelled, OrderStatus.CancelledByUser, OrderStatus.CancelledByAdmin];
        lifetimeSpend = orders
          .filter(o => !cancelledStatuses.includes(o.status))
          .reduce((sum, o) => {
            const price = (o as any).price || 0;
            const refund = (o as any).isRefunded ? ((o as any).refundAmount || 0) : 0;
            return sum + Math.max(0, price - refund);
          }, 0);
      }

      // Monthly spend = SUM(Paid Invoice Amounts) for current month, or fallback to order prices
      if (invoices.length > 0) {
        monthlySpend = invoices
          .filter(inv => {
            if (inv.status !== 'Paid' || !inv.paidDate) return false;
            const paidDate = new Date(inv.paidDate);
            return paidDate >= startOfMonth;
          })
          .reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0);
      } else {
        monthlySpend = orders
          .filter(o => new Date(o.createdAt) >= startOfMonth)
          .filter(o => ![OrderStatus.Cancelled, OrderStatus.CancelledByUser, OrderStatus.CancelledByAdmin].includes(o.status))
          .reduce((sum, o) => sum + ((o as any).price || 0), 0);
      }

      // Total orders = COUNT(OrderId) for the client (orders.length)
      // Avg Order Value = Lifetime Spend / Total Orders
      const totalOrdersForClient = orders.length;
      averageOrderValue = totalOrdersForClient > 0 ? lifetimeSpend / totalOrdersForClient : 0;

      // Orders by week (last 8 weeks) for trend chart
      ordersByWeek = this.calculateOrdersByWeek(orders, 8);
    }

    // Calculate statistics (Client: never include admin/global metrics)
    const stats: DashboardStats = {
      totalOrders: orders.length,
      pendingOrders: orders.filter(o => o.status === OrderStatus.WaitingForAdminApproval).length,
      inProgressOrders: orders.filter(o => o.status === OrderStatus.InProgress).length,
      completedOrders: orders.filter(o => o.status === OrderStatus.Completed).length,
      totalClients: userRole === 'Client' ? 0 : clients.length,
      newClientsThisMonth: userRole === 'Client' ? 0 : newClientsThisMonth,
      totalRevenue: userRole === 'Client' ? 0 : totalRevenue,
      averageDeliveryTime: Math.round(averageDeliveryTime * 10) / 10, // Round to 1 decimal
      // Client-specific stats
      activeOrders,
      ordersAwaitingApproval,
      pendingInvoices,
      dueAmount,
      overdueAmount,
      amountPaidThisMonth,
      // Client analytics
      ordersThisMonth,
      ordersThisWeek,
      ordersToday,
      completedOrdersThisMonth,
      lifetimeSpend,
      monthlySpend,
      averageOrderValue
    };

    // Get recent orders (last 10, sorted by creation date)
    const recentOrders = [...orders]
      .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
      .slice(0, 10);

    // Orders by status for chart
    const statusCounts = new Map<string, number>();
    orders.forEach(order => {
      const count = statusCounts.get(order.status) || 0;
      statusCounts.set(order.status, count + 1);
    });
    const ordersByStatus = Array.from(statusCounts.entries()).map(([status, count]) => ({
      status,
      count
    }));

    // Orders by month for chart
    const monthCounts = new Map<string, number>();
    orders.forEach(order => {
      const date = new Date(order.createdAt);
      const monthKey = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
      const count = monthCounts.get(monthKey) || 0;
      monthCounts.set(monthKey, count + 1);
    });
    const ordersByMonth = Array.from(monthCounts.entries())
      .map(([month, count]) => ({ month, count }))
      .sort((a, b) => a.month.localeCompare(b.month))
      .slice(-6); // Last 6 months

    // Revenue by package: for Client = all orders grouped by package (derived from price); for Admin = completed orders
    const packageRevenue = new Map<string, number>();
    const getPackageFromPrice = (price: number): string => {
      if (price < 200) return 'Basic';
      if (price < 500) return 'Standard';
      if (price < 1000) return 'Premium';
      return 'Custom';
    };
    const ordersForPackage = userRole === 'Client'
      ? orders.filter(o => o.status !== OrderStatus.Cancelled && o.status !== OrderStatus.CancelledByUser && o.status !== OrderStatus.CancelledByAdmin)
      : orders.filter(o => o.status === OrderStatus.Completed);
    ordersForPackage.forEach(order => {
      const price = (order as any).price || 0;
      const packageName = (order as any).packageType || (order as any).package || getPackageFromPrice(price);
      const current = packageRevenue.get(packageName) || 0;
      packageRevenue.set(packageName, current + price);
    });
    const revenueByPackage = Array.from(packageRevenue.entries())
      .map(([packageName, revenue]) => ({ package: packageName, revenue }))
      .sort((a, b) => b.revenue - a.revenue);

    // KPI currency follows completed order pricing (not unrelated paid invoices in other currencies).
    const revenueCurrency = resolveSingleCurrencyCode(
      orders
        .filter((o) => o.status === OrderStatus.Completed)
        .map((o) => o.currencyCode)
    );

    // Full sorted order list for client history
    const allOrdersSorted = [...orders]
      .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

    return {
      stats,
      recentOrders,
      allOrders: userRole === 'Client' ? allOrdersSorted : undefined,
      ordersByStatus,
      ordersByMonth,
      revenueByPackage,
      // Client-specific data
      invoices: userRole === 'Client' ? invoices : undefined,
      galleryItems: userRole === 'Client' ? galleryItems : undefined,
      // Notifications for both Client and Admin (Admin gets "New Order Submitted", etc.)
      notifications: notifications,
      ordersByWeek: userRole === 'Client' ? ordersByWeek : undefined,
      revenueCurrencyCode: revenueCurrency.code,
      revenueCurrencyMixed: revenueCurrency.mixed
    };
  }

  private resolveUserRole(user: Pick<User, 'role' | 'roleName'>): string {
    const r = user.roleName || user.role;
    return r ? String(r) : '';
  }

  /** Client dashboard: orders load first; invoices/gallery/notifications cannot zero out KPIs. */
  private buildClientDashboard$(): Observable<DashboardData> {
    return this.fetchOrdersForRole('orders/my-orders').pipe(
      switchMap((orders) =>
        forkJoin({
          invoices: this.apiService.get<unknown>('invoices').pipe(
            map((r) => this.normalizeListResponse(r)),
            catchError(() => of([]))
          ),
          galleryItems: this.apiService.get<unknown>('gallery/my-gallery').pipe(
            timeout(15000),
            map((r) => this.normalizeListResponse(r)),
            catchError(() => of([]))
          ),
          notifications: this.notificationService.getNotifications(false).pipe(
            catchError(() => of([]))
          )
        }).pipe(
          map(({ invoices, galleryItems, notifications }) =>
            this.processDashboardData(
              orders,
              [],
              invoices,
              galleryItems,
              notifications,
              UserRole.Client
            )
          ),
          catchError(() =>
            of(this.processDashboardData(orders, [], [], [], [], UserRole.Client))
          )
        )
      ),
      catchError(() => of(this.getEmptyDashboardData()))
    );
  }

  private fetchOrdersForRole(endpoint: string): Observable<Order[]> {
    return this.apiService.get<unknown>(endpoint).pipe(
      map((response) => this.normalizeOrdersResponse(response)),
      catchError(() => of([]))
    );
  }

  private normalizeListResponse(response: unknown): any[] {
    if (Array.isArray(response)) {
      return response;
    }
    if (response && typeof response === 'object') {
      const r = response as Record<string, unknown>;
      if (Array.isArray(r['data'])) {
        return r['data'] as any[];
      }
      const items = ApiService.extractItems<unknown>(response);
      if (items.length > 0) {
        return items;
      }
    }
    return [];
  }

  private normalizeOrdersResponse(response: unknown): Order[] {
    const raw = this.extractOrdersArray(response);
    return raw.map((row) => this.normalizeOrder(row));
  }

  private extractOrdersArray(response: unknown): unknown[] {
    if (!response) {
      return [];
    }
    if (Array.isArray(response)) {
      return response;
    }
    if (typeof response === 'object') {
      const r = response as Record<string, unknown>;
      if (Array.isArray(r['data'])) {
        return r['data'];
      }
      const items = ApiService.extractItems<unknown>(response);
      if (items.length > 0) {
        return items;
      }
    }
    return [];
  }

  /** Map API status strings/numbers to OrderStatus (aligned with order-list mapStatus). */
  private normalizeOrder(backendOrder: unknown): Order {
    const o = backendOrder as Record<string, unknown>;
    const status = this.mapOrderStatus(
      (o['status'] ?? o['Status']) as string | number | undefined
    );

    return {
      id: String(o['id'] ?? o['Id'] ?? ''),
      clientId: String(o['clientId'] ?? o['ClientId'] ?? ''),
      title: String(o['title'] ?? o['Title'] ?? ''),
      description: String(o['description'] ?? o['Description'] ?? ''),
      status,
      createdAt: o['createdAt']
        ? new Date(o['createdAt'] as string)
        : o['CreatedAt']
          ? new Date(o['CreatedAt'] as string)
          : new Date(),
      updatedAt: o['updatedAt']
        ? new Date(o['updatedAt'] as string)
        : o['UpdatedAt']
          ? new Date(o['UpdatedAt'] as string)
          : undefined,
      dueDate: o['dueDate']
        ? new Date(o['dueDate'] as string)
        : o['Deadline']
          ? new Date(o['Deadline'] as string)
          : undefined,
      price: Number(
        o['clientChargePrice'] ??
          o['ClientChargePrice'] ??
          o['price'] ??
          o['Price'] ??
          0
      ),
      currencyCode: (o['currencyCode'] ?? o['CurrencyCode'] ?? undefined) as string | undefined
    } as Order;
  }

  private mapOrderStatus(status: string | number | undefined): OrderStatus {
    if (status === undefined || status === null) {
      return OrderStatus.WaitingForAdminApproval;
    }
    const statusStr = String(status).trim();
    switch (statusStr) {
      case 'WaitingForAdminApproval':
      case '1':
        return OrderStatus.WaitingForAdminApproval;
      case 'PriceApprovalPending':
      case '2':
        return OrderStatus.PriceApprovalPending;
      case 'ApprovedUnassigned':
      case 'Approved Unassigned':
      case 'approved_unassigned':
      case '16':
        return OrderStatus.ApprovedUnassigned;
      case 'InProgress':
      case 'In Progress':
      case '3':
        return OrderStatus.InProgress;
      case 'PreviewDelivered':
      case '4':
        return OrderStatus.PreviewDelivered;
      case 'RevisionRequested':
      case '5':
        return OrderStatus.RevisionRequested;
      case 'FinalApproved':
      case 'ClientApproved':
      case '6':
        return OrderStatus.ClientApproved;
      case 'Completed':
      case '7':
        return OrderStatus.Completed;
      case 'Cancelled':
      case '8':
        return OrderStatus.Cancelled;
      case 'CancelledByUser':
      case '13':
        return OrderStatus.CancelledByUser;
      case 'CancelledByAdmin':
      case '14':
        return OrderStatus.CancelledByAdmin;
      case 'Refunded':
      case '15':
        return OrderStatus.Refunded;
      default:
        return OrderStatus.WaitingForAdminApproval;
    }
  }

  private calculateOrdersByWeek(orders: Order[], weeks: number): { weekLabel: string; count: number }[] {
    const result: { weekLabel: string; count: number }[] = [];
    const now = new Date();

    for (let i = weeks - 1; i >= 0; i--) {
      const weekStart = new Date(now);
      weekStart.setDate(now.getDate() - (now.getDay() + (i * 7)));
      weekStart.setHours(0, 0, 0, 0);

      const weekEnd = new Date(weekStart);
      weekEnd.setDate(weekStart.getDate() + 7);

      const count = orders.filter(o => {
        const created = new Date(o.createdAt);
        return created >= weekStart && created < weekEnd;
      }).length;

      const monthName = weekStart.toLocaleDateString('en-US', { month: 'short' });
      const day = weekStart.getDate();
      result.push({
        weekLabel: `${monthName} ${day}`,
        count
      });
    }

    return result;
  }

  private getEmptyDashboardData(): DashboardData {
    return {
      stats: {
        totalOrders: 0,
        pendingOrders: 0,
        inProgressOrders: 0,
        completedOrders: 0,
        totalClients: 0,
        newClientsThisMonth: 0,
        totalRevenue: 0,
        averageDeliveryTime: 0,
        activeOrders: 0,
        ordersAwaitingApproval: 0,
        pendingInvoices: 0,
        dueAmount: 0,
        overdueAmount: 0,
        amountPaidThisMonth: 0,
        ordersThisMonth: 0,
        ordersThisWeek: 0,
        ordersToday: 0,
        completedOrdersThisMonth: 0,
        lifetimeSpend: 0,
        monthlySpend: 0,
        averageOrderValue: 0
      },
      recentOrders: [],
      allOrders: [],
      ordersByStatus: [],
      ordersByMonth: [],
      revenueByPackage: [],
      invoices: [],
      galleryItems: [],
      notifications: [],
      ordersByWeek: []
    };
  }
}
