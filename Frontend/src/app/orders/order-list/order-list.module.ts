import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrderListComponent } from './order-list.component';
import { OrderListRoutingModule } from './order-list-routing.module';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { DialogModule } from 'primeng/dialog';
import { MessageService } from 'primeng/api';

@NgModule({
  declarations: [OrderListComponent],
  imports: [
    CommonModule,
    FormsModule,
    OrderListRoutingModule,
    TableModule,
    ButtonModule,
    CardModule,
    TagModule,
    TooltipModule,
    InputTextModule,
    DropdownModule,
    DialogModule
  ],
  providers: [MessageService]
})
export class OrderListModule { }
