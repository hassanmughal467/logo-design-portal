import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FileListComponent } from './file-list.component';
import { FileListRoutingModule } from './file-list-routing.module';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { PaginatorModule } from 'primeng/paginator';

@NgModule({
  declarations: [FileListComponent],
  imports: [
    CommonModule,
    FormsModule,
    FileListRoutingModule,
    TableModule,
    ButtonModule,
    CardModule,
    TagModule,
    TooltipModule,
    InputTextModule,
    DropdownModule,
    CalendarModule,
    PaginatorModule
  ]
})
export class FileListModule { }
