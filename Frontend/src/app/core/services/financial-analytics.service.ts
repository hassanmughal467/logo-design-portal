import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface FinancialRevenueByCurrencyItem {
  currencyCode: string;
  totalRevenue: number;
  monthlyRevenue: number;
  completedCount: number;
  averageOrderValue: number;
}

export interface FinancialOverview {
  totalRevenue: number;
  revenueCurrencyCode?: string;
  revenueCurrencyMixed?: boolean;
  revenueByCurrency?: FinancialRevenueByCurrencyItem[];
  revenueThisMonth: number;
  revenueThisWeek: number;
  averageOrderValue: number;
  totalOrders: number;
  revenueGrowthPercent: number;
  invoicesPaid: number;
  invoicesPending: number;
  overdueInvoices: number;
  outstandingBalance: number;
  collectedRevenue: number;
  refundedAmount: number;
}

export interface RevenueTrendItem {
  month: string;
  monthKey: string;
  revenue: number;
}

export interface OrdersVsRevenueItem {
  month: string;
  monthKey: string;
  ordersCount: number;
  revenue: number;
}

export interface PackageRevenueItem {
  package: string;
  revenue: number;
}

export interface DesignerRevenueItem {
  designerId: string;
  designerName: string;
  totalRevenue: number;
  ordersCompleted: number;
}

export interface ClientRevenueItem {
  clientId: string;
  clientName: string;
  ordersCount: number;
  totalRevenue: number;
  currencyCode?: string;
}

export interface InvoiceStatusItem {
  status: string;
  count: number;
}

export interface WeeklyRevenueItem {
  dayOfWeek: string;
  dayIndex: number;
  revenue: number;
}

export interface OrderValueTrendItem {
  month: string;
  monthKey: string;
  averageOrderValue: number;
}

export interface FinancialActivityItem {
  type: string;
  description: string;
  amount?: number;
  occurredAt: string;
  currencyCode?: string;
}

export interface RevenueForecastItem {
  month: string;
  monthKey: string;
  actual?: number;
  predicted?: number;
}

@Injectable({
  providedIn: 'root'
})
export class FinancialAnalyticsService {
  private readonly basePath = 'admin/financial';

  constructor(private apiService: ApiService) {}

  getOverview(): Observable<FinancialOverview> {
    return this.apiService.get<FinancialOverview>(`${this.basePath}/overview`);
  }

  getRevenueTrend(): Observable<{ items: RevenueTrendItem[] }> {
    return this.apiService.get<{ items: RevenueTrendItem[] }>(`${this.basePath}/revenue-trend`);
  }

  getOrdersVsRevenue(): Observable<{ items: OrdersVsRevenueItem[] }> {
    return this.apiService.get<{ items: OrdersVsRevenueItem[] }>(`${this.basePath}/orders-vs-revenue`);
  }

  getPackageRevenue(): Observable<{ items: PackageRevenueItem[] }> {
    return this.apiService.get<{ items: PackageRevenueItem[] }>(`${this.basePath}/package-revenue`);
  }

  getDesignerRevenue(): Observable<{ items: DesignerRevenueItem[] }> {
    return this.apiService.get<{ items: DesignerRevenueItem[] }>(`${this.basePath}/designer-revenue`);
  }

  getClientRevenue(): Observable<{ items: ClientRevenueItem[] }> {
    return this.apiService.get<{ items: ClientRevenueItem[] }>(`${this.basePath}/client-revenue`);
  }

  getInvoiceStatus(): Observable<{ items: InvoiceStatusItem[] }> {
    return this.apiService.get<{ items: InvoiceStatusItem[] }>(`${this.basePath}/invoice-status`);
  }

  getWeeklyRevenue(): Observable<{ items: WeeklyRevenueItem[] }> {
    return this.apiService.get<{ items: WeeklyRevenueItem[] }>(`${this.basePath}/weekly-revenue`);
  }

  getOrderValueTrend(): Observable<{ items: OrderValueTrendItem[] }> {
    return this.apiService.get<{ items: OrderValueTrendItem[] }>(`${this.basePath}/order-value-trend`);
  }

  getActivityFeed(): Observable<{ items: FinancialActivityItem[] }> {
    return this.apiService.get<{ items: FinancialActivityItem[] }>(`${this.basePath}/activity-feed`);
  }

  getRevenueForecast(): Observable<{ items: RevenueForecastItem[] }> {
    return this.apiService.get<{ items: RevenueForecastItem[] }>(`${this.basePath}/revenue-forecast`);
  }
}
