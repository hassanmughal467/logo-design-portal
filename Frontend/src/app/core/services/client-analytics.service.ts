import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface ClientAnalyticsOverview {
  totalClients: number;
  activeClients: number;
  inactiveClients: number;
  topClientRevenue: number;
}

export interface TopClientItem {
  clientId: string;
  clientName: string;
  totalOrders: number;
  totalRevenue: number;
}

export interface ClientRevenueTrendItem {
  monthKey: string;
  month: string;
  revenue: number;
  orderCount: number;
}

export interface ClientMonthlyRevenueItem {
  monthKey: string;
  month: string;
  revenue: number;
}

export interface ClientMonthlyRevenue {
  clientId: string;
  clientName: string;
  items: ClientMonthlyRevenueItem[];
}

export interface InactiveClientItem {
  clientId: string;
  clientName: string;
  lastOrderDate?: string;
  daysSinceLastOrder: number;
}

export interface ClientLifetimeValueItem {
  clientId: string;
  clientName: string;
  lifetimeRevenue: number;
  totalOrders: number;
}

export interface ClientGrowthItem {
  clientId: string;
  clientName: string;
  lastMonthRevenue: number;
  previousMonthRevenue: number;
  revenueChangePercent: number;
  growthStatus: string;
}

export interface ClientActivityItem {
  clientId: string;
  clientName: string;
  activityStatus: string;
  lastOrderDate?: string;
  daysSinceLastOrder: number;
}

export interface ClientAlertItem {
  alertType: string;
  message: string;
  clientId?: string;
  clientName?: string;
  occurredAt: string;
}

export interface ClientDropdownItem {
  clientId: string;
  clientName: string;
}

@Injectable({
  providedIn: 'root'
})
export class ClientAnalyticsService {
  private readonly basePath = 'admin/client-analytics';

  constructor(private apiService: ApiService) {}

  getOverview(): Observable<ClientAnalyticsOverview> {
    return this.apiService.get<ClientAnalyticsOverview>(`${this.basePath}/overview`);
  }

  getTopClients(limit = 10): Observable<{ items: TopClientItem[] }> {
    return this.apiService.get<{ items: TopClientItem[] }>(`${this.basePath}/top-clients?limit=${limit}`);
  }

  getRevenueTrend(months = 6): Observable<{ items: ClientRevenueTrendItem[] }> {
    return this.apiService.get<{ items: ClientRevenueTrendItem[] }>(`${this.basePath}/revenue-trend?months=${months}`);
  }

  getMonthlyRevenue(clientId: string, months = 12): Observable<ClientMonthlyRevenue> {
    return this.apiService.get<ClientMonthlyRevenue>(`${this.basePath}/monthly-revenue?clientId=${clientId}&months=${months}`);
  }

  getInactiveClients(inactiveDaysThreshold = 30): Observable<{ items: InactiveClientItem[] }> {
    return this.apiService.get<{ items: InactiveClientItem[] }>(`${this.basePath}/inactive-clients?inactiveDaysThreshold=${inactiveDaysThreshold}`);
  }

  getLifetimeValue(): Observable<{ items: ClientLifetimeValueItem[] }> {
    return this.apiService.get<{ items: ClientLifetimeValueItem[] }>(`${this.basePath}/lifetime-value`);
  }

  getClientGrowth(): Observable<{ items: ClientGrowthItem[] }> {
    return this.apiService.get<{ items: ClientGrowthItem[] }>(`${this.basePath}/client-growth`);
  }

  getClientActivity(): Observable<{ items: ClientActivityItem[] }> {
    return this.apiService.get<{ items: ClientActivityItem[] }>(`${this.basePath}/client-activity`);
  }

  getAlerts(): Observable<{ items: ClientAlertItem[] }> {
    return this.apiService.get<{ items: ClientAlertItem[] }>(`${this.basePath}/alerts`);
  }

  getClientsForDropdown(): Observable<ClientDropdownItem[]> {
    return this.apiService.get<ClientDropdownItem[]>(`${this.basePath}/clients-dropdown`);
  }
}
