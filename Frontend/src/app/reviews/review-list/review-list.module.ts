import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReviewListComponent } from './review-list.component';

// PrimeNG
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';

@NgModule({
  declarations: [ReviewListComponent],
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    CardModule,
    InputTextModule
  ]
})
export class ReviewListModule { }
