import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { SharedListDataService } from '@core/services/shared-list-data.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Order, OrderStatus } from '@shared/models/order.model';
import { TagSeverity } from '@shared/types/primeng.types';

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
  globalFilter = '';
  selectedStatus: string | null = null;
  first = 0;
  rows = 10;

  statuses = Object.values(OrderStatus);
  packageTypes = ['Basic', 'Premium', 'Enterprise'];

  private destroy$ = new Subject<void>();

  constructor(
    private messageService: MessageService,
    private router: Router,
    private sharedListData: SharedListDataService
  ) {}

  ngOnInit(): void {
    this.loadProjects();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProjects(): void {
    // Transform orders to projects
    this.sharedListData
      .fetchAllOrdersUncached()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (ordersRaw) => {
          const orders = ordersRaw as Order[];
          this.projects = orders.map(order => ({
            id: order.id,
            orderId: order.id,
            title: order.title,
            clientName: order.client ? (() => {
              const parts: string[] = [];
              if (order.client.companyName) {
                parts.push(order.client.companyName);
              }
              const personName = `${order.client.firstName || ''} ${order.client.lastName || ''}`.trim();
              if (personName) {
                parts.push(personName);
              }
              return parts.length > 0 ? parts.join(' - ') : (order.client.email || 'N/A');
            })() : 'N/A',
            designerName: order.designer ? `${order.designer.firstName} ${order.designer.lastName}` : undefined,
            packageType: (order as any).packageType || 'Basic',
            status: order.status,
            revisionCount: order.revisionCount || 0,
            approvalStatus: this.deriveApprovalStatus(order.status),
            deadline: order.dueDate ? new Date(order.dueDate) : new Date(),
            createdAt: new Date(order.createdAt)
          }));
        },
        error: (error) => {
          console.error('Error loading projects:', error);
          this.projects = [];
        }
      });
  }

  viewProject(projectId: string): void {
    this.router.navigate(['/projects', projectId]);
  }

  getStatusSeverity(status: OrderStatus): TagSeverity {
    const severityMap: Record<string, TagSeverity> = {
      'Pending': 'warning',
      'InProgress': 'info',
      'Review': 'secondary',
      'Completed': 'success',
      'Cancelled': 'danger'
    };
    return severityMap[status] || 'secondary';
  }

  /**
   * Approval is encoded in the order status itself:
   *   Completed / ClientApproved        -> Approved
   *   Cancelled* / Refunded             -> Rejected
   *   anything else (still in flight)   -> Pending
   */
  private deriveApprovalStatus(status: OrderStatus): 'Pending' | 'Approved' | 'Rejected' {
    switch (status) {
      case OrderStatus.ClientApproved:
      case OrderStatus.Completed:
        return 'Approved';
      case OrderStatus.Cancelled:
      case OrderStatus.CancelledByUser:
      case OrderStatus.CancelledByAdmin:
      case OrderStatus.Refunded:
        return 'Rejected';
      default:
        return 'Pending';
    }
  }

  getPackageSeverity(packageType: string): TagSeverity {
    const severityMap: Record<string, TagSeverity> = {
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
