import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReviewsRoutingModule } from './reviews-routing.module';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';
import { ReviewListModule } from './review-list/review-list.module';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    ReviewsRoutingModule,
    LayoutModule,
    SharedModule,
    ReviewListModule
  ]
})
export class ReviewsModule { }
