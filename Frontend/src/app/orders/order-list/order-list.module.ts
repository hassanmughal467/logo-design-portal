import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrderListComponent } from './order-list.component';
import { OrderListRoutingModule } from './order-list-routing.module';
import { OrderDetailModule } from '../order-detail/order-detail.module';
import { OrderCreateModule } from '../order-create/order-create.module';
import { SharedModule } from '@shared/shared.module';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { DialogModule } from 'primeng/dialog';
import { FileUploadModule } from 'primeng/fileupload';
import { MessageService } from 'primeng/api';

@NgModule({
  declarations: [OrderListComponent],
  imports: [
    CommonModule,
    FormsModule,
    SharedModule,
    OrderListRoutingModule,
    OrderDetailModule,
    OrderCreateModule,
    TableModule,
    ButtonModule,
    CardModule,
    TagModule,
    TooltipModule,
    InputTextModule,
    DropdownModule,
    DialogModule,
    FileUploadModule
  ],
  providers: [MessageService]
})
export class OrderListModule { }
