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

  // Dialog states
  showAssignDialog = false;
  showStatusDialog = false;
  showUploadDialog = false;
  selectedOrder: Order | null = null;
  availableDesigners: any[] = [];
  selectedDesignerId: string | null = null;
  newStatus: OrderStatus | null = null;

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

  // Action methods
  canAssignDesigner(): boolean {
    const user = this.authService.getCurrentUser();
    return user?.role === 'SuperAdmin' || user?.role === 'Admin';
  }

  canGenerateInvoice(): boolean {
    const user = this.authService.getCurrentUser();
    return user?.role === 'SuperAdmin' || user?.role === 'Admin';
  }

  openAssignDialog(order: Order): void {
    this.selectedOrder = order;
    this.selectedDesignerId = order.designerId || null;
    this.loadDesigners();
    this.showAssignDialog = true;
  }

  loadDesigners(): void {
    this.apiService.get<any[]>('users')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (users) => {
          this.availableDesigners = users
            .filter(u => u.role === 'Designer' || u.roleName === 'Designer')
            .map(u => ({ label: `${u.firstName} ${u.lastName}`, value: u.id }));
        },
        error: () => {
          this.availableDesigners = [];
        }
      });
  }

  assignDesigner(): void {
    if (!this.selectedOrder || !this.selectedDesignerId) return;

    this.apiService.post(`orders/${this.selectedOrder.id}/assign`, { designerId: this.selectedDesignerId })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Designer assigned successfully'
          });
          this.showAssignDialog = false;
          this.loadOrders();
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to assign designer'
          });
        }
      });
  }

  openStatusDialog(order: Order): void {
    this.selectedOrder = order;
    this.newStatus = order.status;
    this.showStatusDialog = true;
  }

  changeStatus(): void {
    if (!this.selectedOrder || !this.newStatus) return;

    this.apiService.put(`orders/${this.selectedOrder.id}/status`, { status: this.newStatus })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Order status updated successfully'
          });
          this.showStatusDialog = false;
          this.loadOrders();
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to update order status'
          });
        }
      });
  }

  openUploadDialog(order: Order): void {
    this.selectedOrder = order;
    this.showUploadDialog = true;
  }

  uploadFile(event: any): void {
    if (!this.selectedOrder || !event.target.files[0]) return;

    const file = event.target.files[0];
    const formData = new FormData();
    formData.append('file', file);

    this.apiService.post(`files/upload/${this.selectedOrder.id}`, formData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'File uploaded successfully'
          });
          this.showUploadDialog = false;
          // Reset file input
          event.target.value = '';
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to upload file'
          });
        }
      });
  }

  generateInvoice(order: Order): void {
    this.apiService.post(`invoices`, { orderId: order.id })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (invoice: any) => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Invoice generated successfully'
          });
          // Optionally navigate to invoices or stay on orders page
          // this.router.navigate(['/invoices']);
        },
        error: (error) => {
          console.error('Generate invoice error:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to generate invoice'
          });
        }
      });
  }
}
