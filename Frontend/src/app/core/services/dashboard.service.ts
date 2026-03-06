import { Injectable } from '@angular/core';
import { Observable, forkJoin, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { NotificationService } from './notification.service';
import { Order, OrderStatus } from '@shared/models/order.model';

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
  ordersByMonth: { month: string; count: number }[];
  revenueByPackage: { package: string; revenue: number }[];
  // Client-specific data
  invoices?: any[];
  galleryItems?: any[];
  notifications?: any[];
  // Client analytics
  ordersByWeek?: { weekLabel: string; count: number }[];
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  getDashboardData(): Observable<DashboardData> {
    const user = this.authService.getCurrentUser();
    if (!user) {
      return of(this.getEmptyDashboardData());
    }

    // Fetch orders based on user role
    let orders$: Observable<Order[]>;
    
    switch (user.role) {
      case 'Client':
        orders$ = this.apiService.get<Order[]>('orders/my-orders');
        break;
      case 'Designer':
        orders$ = this.apiService.get<Order[]>('orders/assigned-orders');
        break;
      case 'SuperAdmin':
      case 'Admin':
        orders$ = this.apiService.get<Order[]>('orders');
        break;
      default:
        orders$ = of([]);
    }

    // Fetch additional data for admin users
    let clients$: Observable<any[]> = of([]);
    let invoices$: Observable<any[]> = of([]);
    let galleryItems$: Observable<any[]> = of([]);
    let notifications$: Observable<any[]> = of([]);
    
    if (user.role === 'SuperAdmin' || user.role === 'Admin') {
      // Try to fetch clients (if endpoint exists)
      clients$ = this.apiService.get<any[]>('users').pipe(
        map(users => users.filter(u => u.role === 'Client' || u.roleName === 'Client')),
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
    } else if (user.role === 'Client') {
      // Fetch client-specific data
      invoices$ = this.apiService.get<any[]>('invoices').pipe(
        catchError(() => of([]))
      );
      
      galleryItems$ = this.apiService.get<any[]>('gallery/my-gallery').pipe(
        catchError(() => of([]))
      );
      
      notifications$ = this.notificationService.getNotifications(false).pipe(
        catchError(() => of([]))
      );
    }

    return forkJoin({
      orders: orders$,
      clients: clients$,
      invoices: invoices$,
      galleryItems: galleryItems$,
      notifications: notifications$
    }).pipe(
      map(({ orders, clients, invoices, galleryItems, notifications }) => 
        this.processDashboardData(orders, clients, invoices, galleryItems, notifications, user.role)
      ),
      catchError(error => {
        console.error('Error fetching dashboard data:', error);
        return of(this.getEmptyDashboardData());
      })
    );
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
      // Active orders (InProgress, PreviewDelivered, RevisionRequested)
      activeOrders = orders.filter(o => 
        o.status === OrderStatus.InProgress || 
        o.status === OrderStatus.PreviewDelivered || 
        o.status === OrderStatus.RevisionRequested
      ).length;

      // Orders awaiting approval (PreviewDelivered, PriceApprovalPending)
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

      // Lifetime spend (sum of paid invoice amounts only)
      lifetimeSpend = invoices
        .filter(inv => inv.status === 'Paid')
        .reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0);

      // Monthly spend (paid invoices this month)
      monthlySpend = invoices
        .filter(inv => {
          if (inv.status !== 'Paid' || !inv.paidDate) return false;
          const paidDate = new Date(inv.paidDate);
          return paidDate >= startOfMonth;
        })
        .reduce((sum, inv) => sum + (inv.totalAmount || inv.amount || 0), 0);

      // Average order value
      const nonCancelledOrders = orders.filter(o =>
        o.status !== OrderStatus.Cancelled && o.status !== OrderStatus.CancelledByUser && o.status !== OrderStatus.CancelledByAdmin
      );
      averageOrderValue = nonCancelledOrders.length > 0
        ? nonCancelledOrders.reduce((sum, o) => sum + (o.price || 0), 0) / nonCancelledOrders.length
        : 0;

      // Orders by week (last 8 weeks) for trend chart
      ordersByWeek = this.calculateOrdersByWeek(orders, 8);
    }

    // Calculate statistics
    const stats: DashboardStats = {
      totalOrders: orders.length,
      pendingOrders: orders.filter(o => o.status === OrderStatus.WaitingForAdminApproval).length,
      inProgressOrders: orders.filter(o => o.status === OrderStatus.InProgress).length,
      completedOrders: orders.filter(o => o.status === OrderStatus.Completed).length,
      totalClients: clients.length,
      newClientsThisMonth: newClientsThisMonth,
      totalRevenue: totalRevenue,
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

    // Revenue by package (from completed orders)
    const packageRevenue = new Map<string, number>();
    orders
      .filter(o => o.status === OrderStatus.Completed)
      .forEach(order => {
        const packageName = (order as any).packageType || (order as any).package || 'Standard';
        const price = (order as any).price || 0;
        const current = packageRevenue.get(packageName) || 0;
        packageRevenue.set(packageName, current + price);
      });
    const revenueByPackage = Array.from(packageRevenue.entries())
      .map(([packageName, revenue]) => ({ package: packageName, revenue }))
      .sort((a, b) => b.revenue - a.revenue);

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
      ordersByWeek: userRole === 'Client' ? ordersByWeek : undefined
    };
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
