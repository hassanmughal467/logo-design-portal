import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DesignerPricingManagementComponent } from './designer-pricing-management/designer-pricing-management.component';

const routes: Routes = [
  { path: '', component: DesignerPricingManagementComponent, data: { breadcrumb: 'Designer Pricing' } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DesignerPricingRoutingModule { }
