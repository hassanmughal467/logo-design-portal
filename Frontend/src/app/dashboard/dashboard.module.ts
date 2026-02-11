import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardComponent } from './dashboard.component';
import { DashboardRoutingModule } from './dashboard-routing.module';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';
import { OrderDetailModule } from '../orders/order-detail/order-detail.module';
import { OrderCreateModule } from '../orders/order-create/order-create.module';

// PrimeNG
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ChartModule } from 'primeng/chart';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';

@NgModule({
  declarations: [
    DashboardComponent
  ],
  imports: [
    CommonModule,
    DashboardRoutingModule,
    LayoutModule,
    SharedModule,
    OrderDetailModule,
    OrderCreateModule,
    CardModule,
    TableModule,
    TagModule,
    ProgressSpinnerModule,
    ChartModule,
    ButtonModule,
    TooltipModule
  ]
})
export class DashboardModule { }
