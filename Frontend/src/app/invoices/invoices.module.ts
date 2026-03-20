import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InvoicesRoutingModule } from './invoices-routing.module';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';
import { InvoiceListModule } from './invoice-list/invoice-list.module';
import { FlexibleInvoiceBuilderComponent } from './flexible-invoice-builder/flexible-invoice-builder.component';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { CheckboxModule } from 'primeng/checkbox';
import { TableModule } from 'primeng/table';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@NgModule({
  declarations: [FlexibleInvoiceBuilderComponent],
  imports: [
    CommonModule,
    FormsModule,
    InvoicesRoutingModule,
    LayoutModule,
    SharedModule,
    InvoiceListModule,
    CardModule,
    ButtonModule,
    DropdownModule,
    CalendarModule,
    CheckboxModule,
    TableModule,
    InputTextareaModule,
    ProgressSpinnerModule
  ]
})
export class InvoicesModule { }
