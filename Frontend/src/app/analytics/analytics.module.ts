import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AnalyticsRoutingModule } from './analytics-routing.module';
import { AnalyticsComponent } from './analytics.component';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';

import { NgApexchartsModule } from 'ng-apexcharts';

import { CardModule } from 'primeng/card';
import { ChartModule } from 'primeng/chart';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';

import { OrderAnalyticsComponent } from './components/order-analytics/order-analytics.component';
import { DesignerAnalyticsComponent } from './components/designer-analytics/designer-analytics.component';
import { RevenueAnalyticsComponent } from './components/revenue-analytics/revenue-analytics.component';
import { ClientAnalyticsComponent } from './components/client-analytics/client-analytics.component';
import { WorkflowAnalyticsComponent } from './components/workflow-analytics/workflow-analytics.component';
import { DashboardAnalyticsComponent } from './components/dashboard-analytics/dashboard-analytics.component';
import { SystemAnalyticsComponent } from './components/system-analytics/system-analytics.component';
import { ForecastAnalyticsComponent } from './components/forecast-analytics/forecast-analytics.component';

@NgModule({
  declarations: [
    AnalyticsComponent,
    OrderAnalyticsComponent,
    DesignerAnalyticsComponent,
    RevenueAnalyticsComponent,
    ClientAnalyticsComponent,
    WorkflowAnalyticsComponent,
    DashboardAnalyticsComponent,
    SystemAnalyticsComponent,
    ForecastAnalyticsComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    AnalyticsRoutingModule,
    LayoutModule,
    SharedModule,
    NgApexchartsModule,
    CardModule,
    ChartModule,
    ProgressSpinnerModule,
    TableModule,
    ButtonModule,
    TagModule
  ]
})
export class AnalyticsModule { }
