import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ClientPricingManagementComponent } from './client-pricing-management/client-pricing-management.component';

const routes: Routes = [
  { path: '', component: ClientPricingManagementComponent, data: { breadcrumb: 'Client Pricing' } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ClientPricingRoutingModule { }
