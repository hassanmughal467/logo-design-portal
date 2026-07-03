import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { SharedListDataService } from '@core/services/shared-list-data.service';
import { formatCurrencyAmount } from '@core/utils/currency-format';
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
  currencyCode?: string;
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
  globalFilter = '';
  first = 0;
  rows = 10;

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private messageService: MessageService,
    private router: Router,
    private sharedListData: SharedListDataService
  ) {}

  ngOnInit(): void {
    this.loadClients();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadClients(): void {
    // Fetch users with Client role and transform to Client interface
    this.sharedListData
      .getAllUsers()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (usersRaw) => {
          const users = usersRaw as any[];
          // Filter and transform to clients
          this.clients = users
            .filter(u => u.role === 'Client' || u.roleName === 'Client')
            .map(user => ({
              id: user.id,
              email: user.email,
              firstName: user.firstName,
              lastName: user.lastName,
              companyName: (user as any).companyName ?? (user as any).clientProfile?.companyName,
              phoneNumber: (user as any).phoneNumber,
              currencyCode: (user as any).clientProfile?.currencyCode,
              totalOrders: 0,
              totalSpent: 0,
              createdAt: new Date(user.createdAt),
              lastOrderDate: undefined
            }));
          
          // Load orders to calculate stats
          this.loadClientStats();
        },
        error: (error) => {
          this.clients = [];
        }
      });
  }

  private loadClientStats(): void {
    // Load orders to calculate client statistics
    this.sharedListData
      .fetchAllOrdersUncached()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (ordersRaw) => {
          const orders = ordersRaw as any[];
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

  formatCurrency(amount: number, currencyCode?: string): string {
    return formatCurrencyAmount(amount, currencyCode);
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
