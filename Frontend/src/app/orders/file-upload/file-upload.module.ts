import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { FileUploadComponent } from './file-upload.component';
import { ButtonModule } from 'primeng/button';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumberModule } from 'primeng/inputnumber';
import { FileUploadModule as PrimeFileUploadModule } from 'primeng/fileupload';
import { DialogModule } from 'primeng/dialog';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';

const routes: Routes = [
  {
    path: '',
    component: FileUploadComponent
  }
];

@NgModule({
  declarations: [FileUploadComponent],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule.forChild(routes),
    ButtonModule,
    InputTextareaModule,
    DropdownModule,
    InputNumberModule,
    PrimeFileUploadModule,
    DialogModule,
    TooltipModule,
    ConfirmDialogModule
  ],
  providers: [ConfirmationService]
})
export class FileUploadModule { }
