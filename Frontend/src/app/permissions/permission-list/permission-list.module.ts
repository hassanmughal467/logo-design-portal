import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PermissionListComponent } from './permission-list.component';
import { PermissionListRoutingModule } from './permission-list-routing.module';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';

@NgModule({
  declarations: [PermissionListComponent],
  imports: [
    CommonModule,
    FormsModule,
    PermissionListRoutingModule,
    TableModule,
    ButtonModule,
    CardModule,
    TagModule,
    InputTextModule,
    CheckboxModule
  ]
})
export class PermissionListModule { }
