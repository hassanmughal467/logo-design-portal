import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface ClientRiskScoreItem {
  clientId: string;
  clientName: string;
  riskScore: number;
  riskLevel: string;
  daysSinceLastOrder: number;
  orderCount: number;
  totalRevenue: number;
  revenueContributionPercent: number;
  lastOrderDate?: string;
  currencyCode?: string;
}

export interface ClientChurnAlertItem {
  clientId: string;
  clientName: string;
  daysSinceLastOrder: number;
  lastOrderDate?: string;
  totalRevenue: number;
  orderCount: number;
  alertMessage: string;
  currencyCode?: string;
}

export interface ClientRetentionTrendItem {
  monthKey: string;
  month: string;
  activeClients: number;
  churnedClients: number;
  retentionRate: number;
}

export interface ClientRetentionStats {
  totalClientsWithOrders: number;
  healthyCount: number;
  warningCount: number;
  highRiskCount: number;
  churnLikelyCount: number;
  retentionTrend: ClientRetentionTrendItem[];
}

@Injectable({
  providedIn: 'root'
})
export class ClientChurnAnalyticsService {
  private readonly basePath = 'admin/client-analytics';

  constructor(private apiService: ApiService) {}

  getClientRiskScores(): Observable<{ items: ClientRiskScoreItem[] }> {
    return this.apiService.get<{ items: ClientRiskScoreItem[] }>(`${this.basePath}/churn-risk`);
  }

  getClientChurnAlerts(inactiveDaysThreshold = 30): Observable<{ items: ClientChurnAlertItem[] }> {
    return this.apiService.get<{ items: ClientChurnAlertItem[] }>(
      `${this.basePath}/churn-alerts?inactiveDaysThreshold=${inactiveDaysThreshold}`
    );
  }

  getClientRetentionStats(months = 12): Observable<ClientRetentionStats> {
    return this.apiService.get<ClientRetentionStats>(`${this.basePath}/retention-stats?months=${months}`);
  }
}
