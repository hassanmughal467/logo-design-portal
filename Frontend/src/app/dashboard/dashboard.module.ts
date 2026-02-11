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
import { DialogModule } from 'primeng/dialog';
import { TabViewModule } from 'primeng/tabview';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { CheckboxModule } from 'primeng/checkbox';
import { DropdownModule } from 'primeng/dropdown';
import { PanelModule } from 'primeng/panel';

@NgModule({
  declarations: [
    DashboardComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
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
    TooltipModule,
    DialogModule,
    TabViewModule,
    InputTextModule,
    CheckboxModule,
    DropdownModule,
    PanelModule
  ]
})
export class DashboardModule { }
