import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface AnalyticsOverview {
  totalOrders: number;
  ordersToday: number;
  ordersThisMonth: number;
  ordersThisYear: number;
  totalRevenue: number;
  monthlyRevenue: number;
  averageOrderValue: number;
  totalClients: number;
  activeDesigners: number;
  pendingOrders: number;
  ordersInProgress: number;
  ordersAwaitingAdminReview: number;
  ordersAwaitingClientApproval: number;
  revisionRate: number;
  approvalRate: number;
  averageDeliveryTimeDays: number;
  overdueOrders: number;
}

export interface OrdersTrendItem {
  month: string;
  monthKey: string;
  count: number;
  completedCount?: number;
}

export interface OrdersByStatusItem {
  status: string;
  count: number;
}

export interface OrdersByPackageItem {
  package: string;
  count: number;
}

export interface OrdersByDayOfWeekItem {
  dayOfWeek: string;
  dayIndex: number;
  count: number;
}

export interface OrdersByHourItem {
  hour: number;
  count: number;
}

export interface DailyActivityItem {
  date: string;
  dateKey: string;
  ordersCreated: number;
  filesUploaded: number;
  revisionsRequested: number;
  approvalsCompleted: number;
}

export interface OrdersTrendDailyItem {
  dateKey: string;
  date: string;
  count: number;
}

export interface OrdersByClientItem {
  clientId: string;
  clientName: string;
  orderCount: number;
}

export interface OrdersByDesignerItem {
  designerId: string;
  designerName: string;
  orderCount: number;
}

export interface OrderAnalytics {
  ordersTrend: OrdersTrendItem[];
  ordersTrendDaily: OrdersTrendDailyItem[];
  ordersByStatus: OrdersByStatusItem[];
  ordersByPackage: OrdersByPackageItem[];
  ordersByDayOfWeek: OrdersByDayOfWeekItem[];
  ordersByHour: OrdersByHourItem[];
  ordersByClient: OrdersByClientItem[];
  ordersByDesigner: OrdersByDesignerItem[];
  dailyActivity: DailyActivityItem[];
}

export interface RevenueTrendItem {
  month: string;
  monthKey: string;
  revenue: number;
}

export interface RevenueByPackageItem {
  package: string;
  revenue: number;
}

export interface AverageOrderValueTrendItem {
  month: string;
  monthKey: string;
  averageOrderValue: number;
}

export interface TopClientByRevenue {
  clientId: string;
  clientName: string;
  revenue: number;
  orderCount: number;
}

export interface RevenueTrendDailyItem {
  dateKey: string;
  date: string;
  revenue: number;
}

export interface RevenueAnalytics {
  revenueTrend: RevenueTrendItem[];
  revenueTrendDaily: RevenueTrendDailyItem[];
  revenueByPackage: RevenueByPackageItem[];
  averageOrderValueTrend: AverageOrderValueTrendItem[];
  topClientsByRevenue: TopClientByRevenue[];
  revenueGrowthRate: number;
}

export interface DesignerPerformanceItem {
  designerId: string;
  designerName: string;
  ordersCompleted: number;
  approvalRate: number;
  revisionRate: number;
  averageCompletionTimeDays: number;
  currentWorkload: number;
}

export interface DesignerActivityTimelineItem {
  dateKey: string;
  date: string;
  designerId: string;
  designerName: string;
  ordersCompleted: number;
  filesUploaded: number;
}

export interface DesignerAnalytics {
  designerPerformance: DesignerPerformanceItem[];
  designerActivityTimeline: DesignerActivityTimelineItem[];
}

export interface OrdersPerClientItem {
  clientName: string;
  orderCount: number;
}

export interface ClientRetentionTrendItem {
  month: string;
  monthKey: string;
  newClients: number;
  returningOrders: number;
}

export interface TopClientByOrdersItem {
  clientId: string;
  clientName: string;
  orderCount: number;
}

export interface ClientLifetimeValueItem {
  clientId: string;
  clientName: string;
  revenue: number;
  orderCount: number;
}

export interface ClientAnalytics {
  newClients: number;
  returningClients: number;
  ordersPerClient: OrdersPerClientItem[];
  clientRetentionTrend: ClientRetentionTrendItem[];
  topClientsByOrders: TopClientByOrdersItem[];
  clientLifetimeValue: ClientLifetimeValueItem[];
}

export interface OrderFunnel {
  ordersCreated: number;
  assignedToDesigner: number;
  designSubmitted: number;
  revisionRequested: number;
  clientApproval: number;
  completed: number;
}

export interface RevisionDistributionItem {
  revisionCount: number;
  orderCount: number;
}

export interface RevisionQuality {
  ordersWithNoRevisions: number;
  ordersWith1Revision: number;
  ordersWith2PlusRevisions: number;
}

export interface OrdersStuckInStageItem {
  stage: string;
  count: number;
}

export interface WorkflowAnalytics {
  orderFunnel: OrderFunnel;
  averageOrderCompletionTimeDays: number;
  revisionDistribution: RevisionDistributionItem[];
  averageRevisionsPerOrder: number;
  revisionQuality: RevisionQuality;
  ordersStuckInStage: OrdersStuckInStageItem[];
}

@Injectable({
  providedIn: 'root'
})
export class AdminAnalyticsService {
  private readonly basePath = 'admin/analytics';

  constructor(private apiService: ApiService) {}

  getOverview(): Observable<AnalyticsOverview> {
    return this.apiService.get<AnalyticsOverview>(`${this.basePath}/overview`);
  }

  getOrderAnalytics(): Observable<OrderAnalytics> {
    return this.apiService.get<OrderAnalytics>(`${this.basePath}/orders`);
  }

  getRevenueAnalytics(): Observable<RevenueAnalytics> {
    return this.apiService.get<RevenueAnalytics>(`${this.basePath}/revenue`);
  }

  getDesignerAnalytics(): Observable<DesignerAnalytics> {
    return this.apiService.get<DesignerAnalytics>(`${this.basePath}/designers`);
  }

  getClientAnalytics(): Observable<ClientAnalytics> {
    return this.apiService.get<ClientAnalytics>(`${this.basePath}/clients`);
  }

  getWorkflowAnalytics(): Observable<WorkflowAnalytics> {
    return this.apiService.get<WorkflowAnalytics>(`${this.basePath}/workflow`);
  }

  getSystemAnalytics(): Observable<SystemAnalytics> {
    return this.apiService.get<SystemAnalytics>(`${this.basePath}/system`);
  }

  getForecastAnalytics(): Observable<ForecastAnalytics> {
    return this.apiService.get<ForecastAnalytics>(`${this.basePath}/forecast`);
  }

  getInsights(): Observable<InsightsAnalytics> {
    return this.apiService.get<InsightsAnalytics>(`${this.basePath}/insights`);
  }
}

export interface NotificationActivityItem {
  type: string;
  count: number;
}

export interface FileUploadByDayItem {
  dateKey: string;
  date: string;
  count: number;
}

export interface FileUploadByTypeItem {
  fileType: string;
  count: number;
}

export interface FileUploadAnalytics {
  totalUploads: number;
  uploadsByDay: FileUploadByDayItem[];
  uploadsByType: FileUploadByTypeItem[];
}

export interface SystemAnalytics {
  dailyActivity: DailyActivityItem[];
  notificationActivity: NotificationActivityItem[];
  fileUploadAnalytics: FileUploadAnalytics;
}

export interface OrderForecastItem {
  periodKey: string;
  period: string;
  actual: number;
  predicted?: number;
}

export interface RevenueForecastItem {
  periodKey: string;
  period: string;
  actual: number;
  predicted?: number;
}

export interface ForecastAnalytics {
  orderGrowthPrediction: OrderForecastItem[];
  revenueForecast: RevenueForecastItem[];
}

export interface InsightsAnalytics {
  insights: string[];
}
