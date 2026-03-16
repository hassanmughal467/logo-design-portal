import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FinancialRoutingModule } from './financial-routing.module';
import { FinancialComponent } from './financial.component';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';

// PrimeNG
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ChartModule } from 'primeng/chart';
import { ButtonModule } from 'primeng/button';
import { TabViewModule } from 'primeng/tabview';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';

// ApexCharts
import { NgApexchartsModule } from 'ng-apexcharts';

// Financial components
import { FinancialDashboardComponent } from './components/financial-dashboard/financial-dashboard.component';
import { FinancialKpiCardsComponent } from './components/financial-kpi-cards/financial-kpi-cards.component';
import { RevenueTrendChartComponent } from './components/revenue-trend-chart/revenue-trend-chart.component';
import { OrdersRevenueChartComponent } from './components/orders-revenue-chart/orders-revenue-chart.component';
import { PackageRevenueChartComponent } from './components/package-revenue-chart/package-revenue-chart.component';
import { DesignerRevenueChartComponent } from './components/designer-revenue-chart/designer-revenue-chart.component';
import { ClientRevenueChartComponent } from './components/client-revenue-chart/client-revenue-chart.component';
import { InvoiceStatusChartComponent } from './components/invoice-status-chart/invoice-status-chart.component';
import { WeeklyRevenueChartComponent } from './components/weekly-revenue-chart/weekly-revenue-chart.component';
import { OrderValueChartComponent } from './components/order-value-chart/order-value-chart.component';
import { FinancialActivityFeedComponent } from './components/financial-activity-feed/financial-activity-feed.component';
import { RevenueForecastChartComponent } from './components/revenue-forecast-chart/revenue-forecast-chart.component';
import { BillingQueueComponent } from './components/billing-queue/billing-queue.component';
import { DesignerPayoutOverviewComponent } from './components/designer-payout-overview/designer-payout-overview.component';
import { DesignerPayoutComponent } from './designer-payout/designer-payout.component';
import { InvoiceDetailComponent } from './designer-payout/invoice-detail/invoice-detail.component';

// PrimeNG additional
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';

@NgModule({
  declarations: [
    FinancialComponent,
    FinancialDashboardComponent,
    FinancialKpiCardsComponent,
    RevenueTrendChartComponent,
    OrdersRevenueChartComponent,
    PackageRevenueChartComponent,
    DesignerRevenueChartComponent,
    ClientRevenueChartComponent,
    InvoiceStatusChartComponent,
    WeeklyRevenueChartComponent,
    OrderValueChartComponent,
    FinancialActivityFeedComponent,
    RevenueForecastChartComponent,
    BillingQueueComponent,
    DesignerPayoutOverviewComponent,
    DesignerPayoutComponent,
    InvoiceDetailComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    FinancialRoutingModule,
    LayoutModule,
    SharedModule,
    CardModule,
    TableModule,
    TagModule,
    ProgressSpinnerModule,
    ChartModule,
    ButtonModule,
    TabViewModule,
    DropdownModule,
    CalendarModule,
    InputNumberModule,
    InputTextModule,
    NgApexchartsModule,
    DialogModule,
    CheckboxModule,
    ConfirmDialogModule,
    TooltipModule
  ]
})
export class FinancialModule { }
