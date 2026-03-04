import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { ApiService } from '@core/services/api.service';
import { DashboardService, DashboardData } from '@core/services/dashboard.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-financial',
  templateUrl: './financial.component.html',
  styleUrls: ['./financial.component.scss']
})
export class FinancialComponent implements OnInit, OnDestroy {
  loading = true;
  private destroy$ = new Subject<void>();

  // Financial snapshot
  financialSnapshot = {
    lifetimeSpend: 0,
    monthlySpend: 0,
    averageOrderValue: 0,
    totalInvoicesPaid: 0,
    totalInvoicesPending: 0
  };

  // Invoices data
  invoices: any[] = [];
  selectedInvoiceTabIndex = 0;
  selectedInvoices: any[] = [];
  showPaymentDialog = false;
  paymentMethod = '';

  // Client trend chart
  clientTrendChartData: any;
  clientTrendChartOptions: any;

  // Weekly and monthly summaries
  weeklySummary = { total: 0, paid: 0, pending: 0 };
  monthlySummary = { total: 0, paid: 0, pending: 0 };

  constructor(
    private authService: AuthService,
    private apiService: ApiService,
    private dashboardService: DashboardService,
    private messageService: MessageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadFinancialData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadFinancialData(): void {
    this.loading = true;
    const user = this.authService.getCurrentUser();
    const isClient = user?.role === 'Client' || user?.roleName === 'Client';

    // Load dashboard data (works for both client and admin/designer)
    this.dashboardService.getDashboardData()
      .pipe(
        takeUntil(this.destroy$),
        catchError(error => {
          console.error('Error loading financial data:', error);
          this.loading = false;
          return of(null);
        })
      )
      .subscribe((data: DashboardData | null) => {
        if (data) {
          if (isClient) {
            this.invoices = data.invoices || [];
            this.setupClientTrendChart(data);
            this.calculateSummaries();
          }
          this.buildFinancialSnapshot(data);
        }
        this.loading = false;
      });
  }

  private buildFinancialSnapshot(data: DashboardData): void {
    this.financialSnapshot = {
      lifetimeSpend: data.stats.lifetimeSpend || 0,
      monthlySpend: data.stats.monthlySpend || 0,
      averageOrderValue: data.stats.averageOrderValue || 0,
      totalInvoicesPaid: this.invoices.filter(inv => inv.status === 'Paid').length,
      totalInvoicesPending: this.invoices.filter(inv => inv.status !== 'Paid').length
    };
  }

  private setupClientTrendChart(data: DashboardData): void {
    // Setup trend chart similar to dashboard
    if (data.ordersByMonth && data.ordersByMonth.length > 0) {
      const labels = data.ordersByMonth.map((item: { month: string; count: number }) => {
        const date = new Date(item.month);
        return date.toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
      });
      
      const orderCounts = data.ordersByMonth.map((item: { month: string; count: number }) => item.count || 0);
      
      this.clientTrendChartData = {
        labels: labels,
        datasets: [
          {
            label: 'Orders',
            data: orderCounts,
            backgroundColor: 'rgba(54, 162, 235, 0.6)',
            borderColor: 'rgba(54, 162, 235, 1)',
            borderWidth: 2
          }
        ]
      };

      this.clientTrendChartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            padding: 12,
            titleFont: { size: 14, weight: 'bold' },
            bodyFont: { size: 13 }
          }
        },
        scales: {
          y: {
            beginAtZero: true,
            ticks: {
              stepSize: 1
            }
          }
        }
      };
    }
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

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 2
    }).format(value);
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
}
