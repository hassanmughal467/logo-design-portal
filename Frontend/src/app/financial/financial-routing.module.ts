import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FinancialComponent } from './financial.component';
import { DesignerPayoutComponent } from './designer-payout/designer-payout.component';
import { InvoiceDetailComponent } from './designer-payout/invoice-detail/invoice-detail.component';

const routes: Routes = [
  { path: '', component: FinancialComponent },
  { path: 'designer-payout', component: DesignerPayoutComponent },
  { path: 'designer-payout/invoice/:id', component: InvoiceDetailComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FinancialRoutingModule { }
