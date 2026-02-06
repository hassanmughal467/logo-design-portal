import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Order, OrderStatus, OrderPriority } from '@shared/models/order.model';

@Component({
  selector: 'app-order-list',
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.scss']
})
export class OrderListComponent implements OnInit, OnDestroy {
  orders: Order[] = [];
  loading = false;
  selectedStatus: OrderStatus | null = null;
  globalFilter = '';
  first = 0;
  rows = 10;

  statuses = Object.values(OrderStatus);
  priorities = Object.values(OrderPriority);

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private messageService: MessageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadOrders(): void {
    this.loading = true;
    const user = this.authService.getCurrentUser();
    if (!user) return;

    let endpoint = 'orders';
    if (user.role === 'Client') {
      endpoint = 'orders/my-orders';
    } else if (user.role === 'Designer') {
      endpoint = 'orders/assigned-orders';
    }

    this.apiService.get<Order[]>(endpoint)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (orders) => {
          this.orders = orders;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading orders:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load orders'
          });
          this.loading = false;
        }
      });
  }

  getStatusSeverity(status: OrderStatus): string {
    const severityMap: { [key: string]: string } = {
      'Pending': 'warning',
      'InProgress': 'info',
      'Review': 'secondary',
      'Completed': 'success',
      'Cancelled': 'danger'
    };
    return severityMap[status] || 'secondary';
  }

  getPrioritySeverity(priority: OrderPriority): string {
    const severityMap: { [key: string]: string } = {
      'Low': 'success',
      'Medium': 'warning',
      'High': 'warning',
      'Urgent': 'danger'
    };
    return severityMap[priority] || 'secondary';
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  viewOrder(orderId: string): void {
    this.router.navigate(['/orders', orderId]);
  }

  canCreateOrder(): boolean {
    return this.authService.hasRole('Client');
  }

  createOrder(): void {
    this.router.navigate(['/orders/create']);
  }

  isOverdue(dueDate: Date | string | undefined): boolean {
    if (!dueDate) return false;
    return new Date(dueDate) < new Date() && new Date(dueDate).getTime() !== 0;
  }
}
