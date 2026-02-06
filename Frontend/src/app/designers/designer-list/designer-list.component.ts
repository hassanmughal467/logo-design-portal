import { Component, OnInit, OnDestroy } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

export interface DesignerProfile {
  id: string;
  userId: string;
  userEmail: string;
  userFirstName: string;
  userLastName: string;
  specialization: string;
  bio: string;
  hourlyRate: number;
  isAvailable: boolean;
  createdAt: Date;
}

@Component({
  selector: 'app-designer-list',
  templateUrl: './designer-list.component.html',
  styleUrls: ['./designer-list.component.scss']
})
export class DesignerListComponent implements OnInit, OnDestroy {
  designers: DesignerProfile[] = [];
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
    this.loadDesigners();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadDesigners(): void {
    this.loading = true;
    // Note: Backend endpoint might be /users/designer-profiles or /designers
    this.apiService.get<DesignerProfile[]>('users/designer-profiles')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (designers) => {
          this.designers = designers;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading designers:', error);
          // If endpoint doesn't exist, show empty state
          this.designers = [];
          this.loading = false;
        }
      });
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }
}
