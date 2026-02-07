import { Injectable } from '@angular/core';
import { Observable, forkJoin, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
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
}

export interface DashboardData {
  stats: DashboardStats;
  recentOrders: Order[];
  ordersByStatus: { status: string; count: number }[];
  ordersByMonth: { month: string; count: number }[];
  revenueByPackage: { package: string; revenue: number }[];
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  constructor(
    private apiService: ApiService,
    private authService: AuthService
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
    }

    return forkJoin({
      orders: orders$,
      clients: clients$,
      invoices: invoices$
    }).pipe(
      map(({ orders, clients, invoices }) => this.processDashboardData(orders, clients, invoices)),
      catchError(error => {
        console.error('Error fetching dashboard data:', error);
        return of(this.getEmptyDashboardData());
      })
    );
  }

  private processDashboardData(orders: Order[], clients: any[] = [], invoices: any[] = []): DashboardData {
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
        .reduce((sum, inv) => sum + (inv.amount || 0), 0);
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

    // Calculate statistics
    const stats: DashboardStats = {
      totalOrders: orders.length,
      pendingOrders: orders.filter(o => o.status === OrderStatus.Pending).length,
      inProgressOrders: orders.filter(o => o.status === OrderStatus.InProgress).length,
      completedOrders: orders.filter(o => o.status === OrderStatus.Completed).length,
      totalClients: clients.length,
      newClientsThisMonth: newClientsThisMonth,
      totalRevenue: totalRevenue,
      averageDeliveryTime: Math.round(averageDeliveryTime * 10) / 10 // Round to 1 decimal
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

    return {
      stats,
      recentOrders,
      ordersByStatus,
      ordersByMonth,
      revenueByPackage
    };
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
        averageDeliveryTime: 0
      },
      recentOrders: [],
      ordersByStatus: [],
      ordersByMonth: [],
      revenueByPackage: []
    };
  }
}
