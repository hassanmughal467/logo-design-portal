import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
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
  errorMessage: string | undefined = undefined;

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    console.log('DesignerListComponent initialized');
    this.loadDesigners();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadDesigners(): void {
    console.log('Loading designers...');
    this.loading = true;
    this.errorMessage = undefined;
    // Backend endpoint: GET /api/users/designer-profiles
    this.apiService.get<DesignerProfile[]>('users/designer-profiles')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (designers) => {
          console.log('Designers loaded:', designers);
          this.designers = designers || [];
          this.loading = false;
          console.log('Designers count:', this.designers.length);
          if (this.designers.length === 0) {
            console.log('No designers found');
            this.messageService.add({
              severity: 'info',
              summary: 'No Designers',
              detail: 'No designer profiles found in the system.'
            });
          }
        },
        error: (error) => {
          console.error('Error loading designers:', error);
          console.error('Error status:', error?.status);
          console.error('Error message:', error?.message);
          console.error('Error body:', error?.error);
          this.errorMessage = error?.error?.error || error?.message || 'Failed to load designers. Please try again.';
          this.designers = [];
          this.loading = false;
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: this.errorMessage || 'Failed to load designers. Please try again.',
            life: 5000
          });
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

  formatCurrency(amount: number | null | undefined): string {
    if (amount == null || amount === undefined) {
      return 'N/A';
    }
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  viewDesigner(designer: DesignerProfile): void {
    if (designer?.userId) {
      this.router.navigate(['/designers', designer.userId]);
    }
  }
}
