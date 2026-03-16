import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ClientIntelligenceRoutingModule } from './client-intelligence-routing.module';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';

import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DropdownModule } from 'primeng/dropdown';
import { NgApexchartsModule } from 'ng-apexcharts';

import { ClientIntelligenceComponent } from './client-intelligence.component';
import { ClientIntelligenceDashboardComponent } from './client-intelligence-dashboard/client-intelligence-dashboard.component';
import { TopClientsChartComponent } from './components/top-clients-chart/top-clients-chart.component';
import { ClientRevenueTrendChartComponent } from './components/client-revenue-trend-chart/client-revenue-trend-chart.component';
import { ClientMonthlyRevenueChartComponent } from './components/client-monthly-revenue-chart/client-monthly-revenue-chart.component';
import { ClientGrowthChartComponent } from './components/client-growth-chart/client-growth-chart.component';
import { ClientLifetimeValueChartComponent } from './components/client-lifetime-value-chart/client-lifetime-value-chart.component';
import { InactiveClientsTableComponent } from './components/inactive-clients-table/inactive-clients-table.component';
import { ClientActivityTableComponent } from './components/client-activity-table/client-activity-table.component';
import { ClientAlertsPanelComponent } from './components/client-alerts-panel/client-alerts-panel.component';
import { ClientChurnDashboardComponent } from './components/client-churn-dashboard/client-churn-dashboard.component';
import { ClientRiskTableComponent } from './components/client-risk-table/client-risk-table.component';
import { ClientRetentionChartComponent } from './components/client-retention-chart/client-retention-chart.component';
import { ClientHealthDistributionComponent } from './components/client-health-distribution/client-health-distribution.component';
import { ClientChurnAlertsPanelComponent } from './components/client-churn-alerts-panel/client-churn-alerts-panel.component';

@NgModule({
  declarations: [
    ClientIntelligenceComponent,
    ClientIntelligenceDashboardComponent,
    TopClientsChartComponent,
    ClientRevenueTrendChartComponent,
    ClientMonthlyRevenueChartComponent,
    ClientGrowthChartComponent,
    ClientLifetimeValueChartComponent,
    InactiveClientsTableComponent,
    ClientActivityTableComponent,
    ClientAlertsPanelComponent,
    ClientChurnDashboardComponent,
    ClientRiskTableComponent,
    ClientRetentionChartComponent,
    ClientHealthDistributionComponent,
    ClientChurnAlertsPanelComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ClientIntelligenceRoutingModule,
    LayoutModule,
    SharedModule,
    CardModule,
    TableModule,
    TagModule,
    ProgressSpinnerModule,
    DropdownModule,
    NgApexchartsModule
  ]
})
export class ClientIntelligenceModule { }
