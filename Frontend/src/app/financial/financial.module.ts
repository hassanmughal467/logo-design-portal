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

@NgModule({
  declarations: [FinancialComponent],
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
    TabViewModule
  ]
})
export class FinancialModule { }
