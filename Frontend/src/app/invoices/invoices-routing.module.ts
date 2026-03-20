import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { InvoiceListComponent } from './invoice-list/invoice-list.component';
import { FlexibleInvoiceBuilderComponent } from './flexible-invoice-builder/flexible-invoice-builder.component';

const routes: Routes = [
  { path: 'flexible-builder', component: FlexibleInvoiceBuilderComponent },
  { path: '', component: InvoiceListComponent },
  { path: ':id', component: InvoiceListComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class InvoicesRoutingModule { }
