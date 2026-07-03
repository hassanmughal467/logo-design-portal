import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { QuotesComponent } from './quotes.component';
import { QuoteDetailComponent } from './quote-detail.component';
import { QuotesRoutingModule } from './quotes-routing.module';
import { SharedModule } from '@shared/shared.module';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { InputNumberModule } from 'primeng/inputnumber';
import { MessageService } from 'primeng/api';

@NgModule({
  declarations: [QuotesComponent, QuoteDetailComponent],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    QuotesRoutingModule,
    SharedModule,
    TableModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    InputTextareaModule,
    DropdownModule,
    TagModule,
    ProgressSpinnerModule,
    InputNumberModule
  ],
  providers: [MessageService]
})
export class QuotesModule { }
