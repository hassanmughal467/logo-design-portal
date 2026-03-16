import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { OrderDetailComponent } from './order-detail.component';
import { OrderProgressTimelineComponent } from '../order-progress-timeline/order-progress-timeline.component';
import { OrderEditModule } from '../order-edit/order-edit.module';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TabViewModule } from 'primeng/tabview';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectButtonModule } from 'primeng/selectbutton';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { FileUploadModule } from 'primeng/fileupload';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ConfirmationService } from 'primeng/api';

const routes: Routes = [
  {
    path: ':id',
    component: OrderDetailComponent
  }
];

@NgModule({
  declarations: [OrderDetailComponent, OrderProgressTimelineComponent],
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
    SelectButtonModule,
    DropdownModule,
    CheckboxModule,
    FileUploadModule,
    ConfirmDialogModule,
    TooltipModule,
    ProgressSpinnerModule,
    OrderEditModule
  ],
  providers: [ConfirmationService],
  exports: [OrderDetailComponent]
})
export class OrderDetailModule { }
