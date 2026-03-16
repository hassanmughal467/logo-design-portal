import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DesignerPricingRoutingModule } from './designer-pricing-routing.module';
import { DesignerPricingManagementComponent } from './designer-pricing-management/designer-pricing-management.component';

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
  declarations: [DesignerPricingManagementComponent],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    DesignerPricingRoutingModule,
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
export class DesignerPricingModule { }
