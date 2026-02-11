import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { DashboardService, DashboardData } from '@core/services/dashboard.service';
import { User } from '@shared/models/user.model';
import { Order, OrderStatus } from '@shared/models/order.model';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

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

  constructor(
    private authService: AuthService,
    private dashboardService: DashboardService,
    private messageService: MessageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Initialize stats with default values
    this.updateStats(this.dashboardData);
    
    // Load user first
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.user = user;
        // Always load dashboard data, even if user is null (will show empty state)
        this.loadDashboardData();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadDashboardData(): void {
    this.loading = true;
    this.dashboardService.getDashboardData()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.dashboardData = data;
          this.updateStats(data);
          this.setupCharts(data);
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

  private updateStats(data: DashboardData): void {
    const user = this.authService.getCurrentUser();
    const isAdmin = user?.role === 'SuperAdmin' || user?.role === 'Admin';

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
    // Status Chart (Pie Chart) - only if we have data
    if (data.ordersByStatus && data.ordersByStatus.length > 0) {
      const statusLabels = data.ordersByStatus.map(s => this.formatStatus(s.status));
      const statusValues = data.ordersByStatus.map(s => s.count);
      const statusColors = this.getStatusColors(data.ordersByStatus.map(s => s.status));

      this.statusChartData = {
        labels: statusLabels,
        datasets: [{
          data: statusValues,
          backgroundColor: statusColors,
          borderWidth: 2,
          borderColor: '#ffffff'
        }]
      };
    } else {
      this.statusChartData = null;
    }

    this.statusChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'bottom',
          labels: {
            padding: 15,
            usePointStyle: true
          }
        },
        tooltip: {
          callbacks: {
            label: (context: any) => {
              const label = context.label || '';
              const value = context.parsed || 0;
              const total = context.dataset.data.reduce((a: number, b: number) => a + b, 0);
              const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
              return `${label}: ${value} (${percentage}%)`;
            }
          }
        }
      }
    };

    // Monthly Chart (Line Chart) - only if we have data
    if (data.ordersByMonth && data.ordersByMonth.length > 0) {
      const monthLabels = data.ordersByMonth.map(m => this.formatMonth(m.month));
      const monthValues = data.ordersByMonth.map(m => m.count);

      this.monthlyChartData = {
        labels: monthLabels,
        datasets: [{
          label: 'Orders',
          data: monthValues,
          fill: true,
          borderColor: '#0d47a1',
          backgroundColor: 'rgba(13, 71, 161, 0.1)',
          tension: 0.4,
          pointBackgroundColor: '#0d47a1',
          pointBorderColor: '#ffffff',
          pointBorderWidth: 2,
          pointRadius: 5
        }]
      };
    } else {
      this.monthlyChartData = null;
    }

    this.monthlyChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false
        },
        tooltip: {
          backgroundColor: '#ffffff',
          titleColor: '#0f172a',
          bodyColor: '#64748b',
          borderColor: '#e2e8f0',
          borderWidth: 1,
          padding: 12
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            stepSize: 1
          },
          grid: {
            color: '#e2e8f0'
          }
        },
        x: {
          grid: {
            display: false
          }
        }
      }
    };

    // Revenue by Package Chart (Bar Chart) - only if we have data
    if (data.revenueByPackage && data.revenueByPackage.length > 0) {
      const packageLabels = data.revenueByPackage.map(p => p.package);
      const revenueValues = data.revenueByPackage.map(p => p.revenue);

      this.revenueChartData = {
        labels: packageLabels,
        datasets: [{
          label: 'Revenue',
          data: revenueValues,
          backgroundColor: [
            '#0d47a1',
            '#1976d2',
            '#4caf50',
            '#ff9800',
            '#f44336',
            '#2196f3'
          ],
          borderColor: [
            '#0a3d91',
            '#1565c0',
            '#45a049',
            '#f57c00',
            '#d32f2f',
            '#dc2626',
            '#0891b2'
          ],
          borderWidth: 2
        }]
      };
    } else {
      this.revenueChartData = null;
    }

    this.revenueChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false
        },
        tooltip: {
          backgroundColor: '#ffffff',
          titleColor: '#0f172a',
          bodyColor: '#64748b',
          borderColor: '#e2e8f0',
          borderWidth: 1,
          padding: 12,
          callbacks: {
            label: (context: any) => {
              return `Revenue: $${context.parsed.y.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
            }
          }
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            callback: (value: any) => {
              return '$' + value.toLocaleString('en-US');
            }
          },
          grid: {
            color: '#e2e8f0'
          }
        },
        x: {
          grid: {
            display: false
          }
        }
      }
    };
  }

  private formatStatus(status: string): string {
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
      'InProgress': '#0d47a1',
      'Review': '#8b5cf6',
      'Completed': '#10b981',
      'Cancelled': '#ef4444'
    };
    return statuses.map(s => colorMap[s] || '#64748b');
  }

  getStatusSeverity(status: OrderStatus): string {
    const severityMap: { [key: string]: string } = {
      'Pending': 'warning',
      'InProgress': 'info',
      'Review': 'secondary',
      'Completed': 'success',
      'Cancelled': 'danger'
    };
    return severityMap[status] || 'secondary';
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

  onOrderDetailClose(): void {
    this.showOrderDetailModal = false;
    this.selectedOrderId = null;
  }

  onOrderUpdated(): void {
    // Refresh dashboard data when order is updated
    this.loadDashboardData();
  }

  // Order Create Modal
  showOrderCreateModal = false;

  navigateToOrders(): void {
    this.router.navigate(['/orders']);
  }

  openCreateOrderModal(): void {
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
    this.router.navigate([route]);
  }
}
