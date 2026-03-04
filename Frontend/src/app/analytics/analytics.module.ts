import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AnalyticsRoutingModule } from './analytics-routing.module';
import { AnalyticsComponent } from './analytics.component';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';

// PrimeNG
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ChartModule } from 'primeng/chart';

@NgModule({
  declarations: [AnalyticsComponent],
  imports: [
    CommonModule,
    FormsModule,
    AnalyticsRoutingModule,
    LayoutModule,
    SharedModule,
    CardModule,
    ProgressSpinnerModule,
    ChartModule
  ]
})
export class AnalyticsModule { }
