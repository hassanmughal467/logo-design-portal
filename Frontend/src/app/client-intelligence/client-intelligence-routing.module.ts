import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ClientIntelligenceComponent } from './client-intelligence.component';

const routes: Routes = [
  { path: '', component: ClientIntelligenceComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ClientIntelligenceRoutingModule { }
