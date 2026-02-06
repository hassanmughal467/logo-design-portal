import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InvoicesRoutingModule } from './invoices-routing.module';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';
import { InvoiceListModule } from './invoice-list/invoice-list.module';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    InvoicesRoutingModule,
    LayoutModule,
    SharedModule,
    InvoiceListModule
  ]
})
export class InvoicesModule { }
