import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ClientPricingRoutingModule } from './client-pricing-routing.module';
import { ClientPricingManagementComponent } from './client-pricing-management/client-pricing-management.component';

import { CardModule } from 'primeng/card';
import { DropdownModule } from 'primeng/dropdown';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { TooltipModule } from 'primeng/tooltip';
import { DividerModule } from 'primeng/divider';
import { ToolbarModule } from 'primeng/toolbar';

@NgModule({
  declarations: [ClientPricingManagementComponent],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    ClientPricingRoutingModule,
    CardModule,
    DropdownModule,
    ButtonModule,
    TableModule,
    TagModule,
    DialogModule,
    InputNumberModule,
    CheckboxModule,
    TooltipModule,
    DividerModule,
    ToolbarModule
  ]
})
export class ClientPricingModule { }
