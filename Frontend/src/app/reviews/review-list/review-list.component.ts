import { Component, OnInit, OnDestroy } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

export interface Review {
  id: string;
  orderId: string;
  clientId: string;
  clientName: string;
  designerId?: string;
  designerName?: string;
  rating: number;
  comment: string;
  createdAt: Date;
}

@Component({
  selector: 'app-review-list',
  templateUrl: './review-list.component.html',
  styleUrls: ['./review-list.component.scss']
})
export class ReviewListComponent implements OnInit, OnDestroy {
  reviews: Review[] = [];
  loading = false;
  globalFilter = '';
  first = 0;
  rows = 10;

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadReviews();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadReviews(): void {
    this.loading = true;
    // Try to fetch reviews (will fail gracefully if endpoint doesn't exist)
    this.apiService.get<Review[]>('reviews')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (reviews) => {
          this.reviews = reviews;
          this.loading = false;
        },
        error: () => {
          this.reviews = [];
          this.loading = false;
        }
      });
  }

  getRatingStars(rating: number): boolean[] {
    return Array(5).fill(false).map((_, i) => i < rating);
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
