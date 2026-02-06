import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ClientsRoutingModule } from './clients-routing.module';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';
import { ClientListComponent } from './client-list/client-list.component';
import { ClientDetailComponent } from './client-detail/client-detail.component';
import { ClientListModule } from './client-list/client-list.module';

@NgModule({
  declarations: [ClientDetailComponent],
  imports: [
    CommonModule,
    ClientsRoutingModule,
    LayoutModule,
    SharedModule,
    ClientListModule
  ]
})
export class ClientsModule { }
