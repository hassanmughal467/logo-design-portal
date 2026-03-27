import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardComponent } from './dashboard.component';
import { DashboardRoutingModule } from './dashboard-routing.module';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';
import { OrderDetailModule } from '../orders/order-detail/order-detail.module';
import { OrderCreateModule } from '../orders/order-create/order-create.module';
import { PaymentsModule } from '../payments/payments.module';

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
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CheckboxModule } from 'primeng/checkbox';
import { DropdownModule } from 'primeng/dropdown';
import { PanelModule } from 'primeng/panel';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { FileUploadModule } from 'primeng/fileupload';

@NgModule({
  declarations: [
    DashboardComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    DashboardRoutingModule,
    LayoutModule,
    SharedModule,
    OrderDetailModule,
    OrderCreateModule,
    PaymentsModule,
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
    PanelModule,
    InputTextareaModule,
    FileUploadModule
  ]
})
export class DashboardModule { }
