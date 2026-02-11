import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { OrderDetailComponent } from './order-detail.component';
import { OrderEditModule } from '../order-edit/order-edit.module';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TabViewModule } from 'primeng/tabview';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { FileUploadModule } from 'primeng/fileupload';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';

const routes: Routes = [
  {
    path: ':id',
    component: OrderDetailComponent
  }
];

@NgModule({
  declarations: [OrderDetailComponent],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule.forChild(routes),
    ButtonModule,
    TagModule,
    TabViewModule,
    TableModule,
    DialogModule,
    InputTextareaModule,
    InputNumberModule,
    DropdownModule,
    CheckboxModule,
    FileUploadModule,
    ConfirmDialogModule,
    TooltipModule,
    OrderEditModule
  ],
  providers: [ConfirmationService],
  exports: [OrderDetailComponent]
})
export class OrderDetailModule { }
