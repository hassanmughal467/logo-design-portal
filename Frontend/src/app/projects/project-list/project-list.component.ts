import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Order, OrderStatus } from '@shared/models/order.model';

export interface LogoProject {
  id: string;
  orderId: string;
  title: string;
  clientName: string;
  designerName?: string;
  packageType: 'Basic' | 'Premium' | 'Enterprise';
  status: OrderStatus;
  revisionCount: number;
  approvalStatus: 'Pending' | 'Approved' | 'Rejected';
  deadline: Date;
  createdAt: Date;
}

@Component({
  selector: 'app-project-list',
  templateUrl: './project-list.component.html',
  styleUrls: ['./project-list.component.scss']
})
export class ProjectListComponent implements OnInit, OnDestroy {
  projects: LogoProject[] = [];
  loading = false;
  globalFilter = '';
  selectedStatus: string | null = null;
  first = 0;
  rows = 10;

  statuses = Object.values(OrderStatus);
  packageTypes = ['Basic', 'Premium', 'Enterprise'];

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProjects();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProjects(): void {
    this.loading = true;
    // Transform orders to projects
    this.apiService.get<Order[]>('orders')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (orders) => {
          this.projects = orders.map(order => ({
            id: order.id,
            orderId: order.id,
            title: order.title,
            clientName: order.client ? `${order.client.firstName} ${order.client.lastName}` : 'N/A',
            designerName: order.designer ? `${order.designer.firstName} ${order.designer.lastName}` : undefined,
            packageType: (order as any).packageType || 'Basic',
            status: order.status,
            revisionCount: (order as any).revisionCount || 0,
            approvalStatus: (order as any).approvalStatus || 'Pending',
            deadline: order.dueDate ? new Date(order.dueDate) : new Date(),
            createdAt: new Date(order.createdAt)
          }));
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading projects:', error);
          this.projects = [];
          this.loading = false;
        }
      });
  }

  viewProject(projectId: string): void {
    this.router.navigate(['/projects', projectId]);
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

  getPackageSeverity(packageType: string): string {
    const severityMap: { [key: string]: string } = {
      'Basic': 'secondary',
      'Premium': 'info',
      'Enterprise': 'success'
    };
    return severityMap[packageType] || 'secondary';
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
