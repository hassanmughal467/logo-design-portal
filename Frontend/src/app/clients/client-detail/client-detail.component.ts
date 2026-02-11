import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Order } from '@shared/models/order.model';

export interface Client {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  companyName?: string;
  phoneNumber?: string;
  totalOrders: number;
  totalSpent: number;
  createdAt: Date;
  lastOrderDate?: Date;
  status?: 'Active' | 'Inactive';
}

@Component({
  selector: 'app-client-detail',
  templateUrl: './client-detail.component.html',
  styleUrls: ['./client-detail.component.scss']
})
export class ClientDetailComponent implements OnInit, OnDestroy {
  clientId: string | null = null;
  client: Client | null = null;
  loading = false;
  activeTab: number = 0;

  // Tab data
  orders: Order[] = [];
  invoices: any[] = [];
  files: any[] = [];
  messages: any[] = [];

  // Loading states
  ordersLoading = false;
  invoicesLoading = false;
  filesLoading = false;
  messagesLoading = false;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.clientId = this.route.snapshot.paramMap.get('id');
    if (this.clientId) {
      this.loadClient();
      this.loadOrders();
      this.loadInvoices();
      this.loadFiles();
      this.loadMessages();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadClient(): void {
    this.loading = true;
    // Use new comprehensive client detail endpoint
    this.apiService.get<any>(`users/clients/${this.clientId}/detail`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (clientDetail) => {
          const user = clientDetail.user;
          if (user.role === 'Client' || user.roleName === 'Client') {
            this.client = {
              id: user.id,
              email: user.email,
              firstName: user.firstName,
              lastName: user.lastName,
              companyName: clientDetail.clientProfile?.companyName,
              phoneNumber: clientDetail.clientProfile?.phoneNumber,
              totalOrders: clientDetail.orderHistory?.length || 0,
              totalSpent: clientDetail.invoices?.reduce((sum: number, inv: any) => sum + (inv.totalAmount || 0), 0) || 0,
              createdAt: new Date(user.createdAt),
              status: user.isActive ? 'Active' : 'Inactive',
              lastOrderDate: clientDetail.orderHistory?.length > 0 
                ? new Date(clientDetail.orderHistory[0].createdAt) 
                : undefined
            };
            
            // Load data from detail response
            this.orders = clientDetail.orderHistory || [];
            this.invoices = clientDetail.invoices || [];
            this.files = clientDetail.files || [];
            this.ordersLoading = false;
            this.invoicesLoading = false;
            this.filesLoading = false;
            
            this.loading = false;
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'User is not a client'
            });
            this.router.navigate(['/clients']);
          }
        },
        error: () => {
          // Fallback to old endpoint if new one doesn't exist
          this.loadClientLegacy();
        }
      });
  }

  private loadClientLegacy(): void {
    this.apiService.get<any>(`users/${this.clientId}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (user) => {
          if (user.role === 'Client' || user.roleName === 'Client') {
            this.client = {
              id: user.id,
              email: user.email,
              firstName: user.firstName,
              lastName: user.lastName,
              companyName: (user as any).companyName,
              phoneNumber: (user as any).phoneNumber,
              totalOrders: 0,
              totalSpent: 0,
              createdAt: new Date(user.createdAt),
              status: 'Active'
            };
            this.loading = false;
            this.loadOrders();
            this.loadInvoices();
            this.loadFiles();
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'User is not a client'
            });
            this.router.navigate(['/clients']);
          }
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load client details'
          });
          this.loading = false;
        }
      });
  }

  loadOrders(): void {
    if (this.orders.length > 0) return; // Already loaded from detail endpoint
    this.ordersLoading = true;
    this.apiService.get<Order[]>('orders')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (orders) => {
          this.orders = orders.filter(o => (o as any).clientId === this.clientId);
          if (this.client) {
            this.client.totalOrders = this.orders.length;
            this.client.totalSpent = this.orders.reduce((sum, o) => sum + ((o as any).price || 0), 0);
            this.client.lastOrderDate = this.orders.length > 0 
              ? new Date(this.orders[0].createdAt) 
              : undefined;
          }
          this.ordersLoading = false;
        },
        error: () => {
          this.orders = [];
          this.ordersLoading = false;
        }
      });
  }

  loadInvoices(): void {
    if (this.invoices.length > 0) return; // Already loaded from detail endpoint
    this.invoicesLoading = true;
    this.apiService.get<any[]>('invoices')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (invoices) => {
          this.invoices = invoices.filter(inv => inv.clientId === this.clientId);
          this.invoicesLoading = false;
        },
        error: () => {
          this.invoices = [];
          this.invoicesLoading = false;
        }
      });
  }

  loadFiles(): void {
    if (this.files.length > 0) return; // Already loaded from detail endpoint
    this.filesLoading = true;
    if (this.orders.length > 0) {
      // Load files for all client orders
      const filePromises = this.orders.map(order => 
        this.apiService.get<any[]>(`files/order/${order.id}`).toPromise()
      );
      
      Promise.all(filePromises)
        .then(fileArrays => {
          this.files = fileArrays.flat().filter(f => f);
          this.filesLoading = false;
        })
        .catch(() => {
          this.files = [];
          this.filesLoading = false;
        });
    } else {
      this.files = [];
      this.filesLoading = false;
    }
  }

  loadMessages(): void {
    this.messagesLoading = true;
    this.apiService.get<any[]>('messages')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (messages) => {
          // Filter messages where client is sender or recipient
          this.messages = messages.filter((m: any) => 
            m.senderId === this.clientId || m.recipientId === this.clientId
          ).map((m: any) => ({
            id: m.id,
            senderName: m.senderName,
            recipientName: m.recipientName,
            content: m.content,
            createdAt: m.createdAt
          })).sort((a: any, b: any) => 
            new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
          );
          this.messagesLoading = false;
        },
        error: () => {
          this.messages = [];
          this.messagesLoading = false;
        }
      });
  }

  viewOrder(orderId: string): void {
    this.router.navigate(['/orders', orderId]);
  }

  viewInvoice(invoiceId: string): void {
    this.router.navigate(['/invoices', invoiceId]);
  }

  downloadFile(fileId: string): void {
    window.open(`http://localhost:5000/api/files/${fileId}/download`, '_blank');
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  getStatusSeverity(status: string): string {
    const severityMap: { [key: string]: string } = {
      'Pending': 'warning',
      'InProgress': 'info',
      'Review': 'secondary',
      'Completed': 'success',
      'Cancelled': 'danger',
      'Paid': 'success',
      'Unpaid': 'warning',
      'Overdue': 'danger'
    };
    return severityMap[status] || 'secondary';
  }

  goBack(): void {
    this.router.navigate(['/clients']);
  }

  getOrderPrice(order: Order): number {
    return (order as any).price || 0;
  }
}
