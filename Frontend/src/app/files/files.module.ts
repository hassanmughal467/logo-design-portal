import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FilesRoutingModule } from './files-routing.module';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';

// PrimeNG
import { FileUploadModule } from 'primeng/fileupload';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    FilesRoutingModule,
    LayoutModule,
    SharedModule,
    FileUploadModule
  ]
})
export class FilesModule { }
