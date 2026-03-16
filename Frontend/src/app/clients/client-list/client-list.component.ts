import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

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
}

@Component({
  selector: 'app-client-list',
  templateUrl: './client-list.component.html',
  styleUrls: ['./client-list.component.scss']
})
export class ClientListComponent implements OnInit, OnDestroy {
  clients: Client[] = [];
  loading = false;
  globalFilter = '';
  first = 0;
  rows = 10;

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private messageService: MessageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadClients();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadClients(): void {
    this.loading = true;
    // Fetch users with Client role and transform to Client interface
    this.apiService.get<any>('users?page=1&pageSize=500')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          const users = ApiService.extractItems<any>(response);
          // Filter and transform to clients
          this.clients = users
            .filter(u => u.role === 'Client' || u.roleName === 'Client')
            .map(user => ({
              id: user.id,
              email: user.email,
              firstName: user.firstName,
              lastName: user.lastName,
              companyName: (user as any).companyName,
              phoneNumber: (user as any).phoneNumber,
              totalOrders: 0, // Will be calculated from orders
              totalSpent: 0, // Will be calculated from invoices
              createdAt: new Date(user.createdAt),
              lastOrderDate: undefined
            }));
          
          // Load orders to calculate stats
          this.loadClientStats();
          this.loading = false;
        },
        error: (error) => {
          this.clients = [];
          this.loading = false;
        }
      });
  }

  private loadClientStats(): void {
    // Load orders to calculate client statistics
    this.apiService.get<any>('orders?page=1&pageSize=500')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          const orders = ApiService.extractItems<any>(response);
          // Calculate stats per client
          const clientStats = new Map<string, { orders: number; spent: number; lastOrder?: Date }>();
          
          orders.forEach((order: any) => {
            const clientId = order.clientId;
            if (!clientStats.has(clientId)) {
              clientStats.set(clientId, { orders: 0, spent: 0 });
            }
            const stats = clientStats.get(clientId)!;
            stats.orders++;
            stats.spent += (order.price || 0);
            const orderDate = new Date(order.createdAt);
            if (!stats.lastOrder || orderDate > stats.lastOrder) {
              stats.lastOrder = orderDate;
            }
          });

          // Update clients with stats
          this.clients = this.clients.map(client => {
            const stats = clientStats.get(client.id);
            return {
              ...client,
              totalOrders: stats?.orders || 0,
              totalSpent: stats?.spent || 0,
              lastOrderDate: stats?.lastOrder
            };
          });
        },
        error: () => {
          // Silently fail - stats will remain 0
        }
      });
  }

  viewClient(clientId: string): void {
    this.router.navigate(['/clients', clientId]);
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
}
