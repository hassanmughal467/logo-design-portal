import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InvoiceListComponent } from './invoice-list.component';
import { SharedModule } from '@shared/shared.module';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { MultiSelectModule } from 'primeng/multiselect';
import { DialogModule } from 'primeng/dialog';
import { TabViewModule } from 'primeng/tabview';
import { CheckboxModule } from 'primeng/checkbox';
import { PaymentsModule } from '../../payments/payments.module';

@NgModule({
  declarations: [InvoiceListComponent],
  imports: [
    CommonModule,
    FormsModule,
    SharedModule,
    TableModule,
    ButtonModule,
    CardModule,
    TagModule,
    TooltipModule,
    InputTextModule,
    InputTextareaModule,
    InputNumberModule,
    CalendarModule,
    DropdownModule,
    MultiSelectModule,
    DialogModule,
    TabViewModule,
    CheckboxModule,
    PaymentsModule
  ]
})
export class InvoiceListModule { }
