import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ClientsRoutingModule } from './clients-routing.module';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';
import { ClientListComponent } from './client-list/client-list.component';
import { ClientDetailComponent } from './client-detail/client-detail.component';
import { ClientListModule } from './client-list/client-list.module';

// PrimeNG
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TabViewModule } from 'primeng/tabview';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ChartModule } from 'primeng/chart';

@NgModule({
  declarations: [ClientDetailComponent],
  imports: [
    CommonModule,
    FormsModule,
    ClientsRoutingModule,
    LayoutModule,
    SharedModule,
    ClientListModule,
    CardModule,
    TableModule,
    ButtonModule,
    TagModule,
    TabViewModule,
    TooltipModule,
    ProgressSpinnerModule,
    ChartModule
  ]
})
export class ClientsModule { }
