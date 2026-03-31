import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { RealtimeNotificationService } from '@core/services/realtime-notification.service';
import { SharedListDataService, UserTypeaheadItem } from '@core/services/shared-list-data.service';
import { DashboardService } from '@core/services/dashboard.service';
import { MessageService } from 'primeng/api';
import { FileUpload } from 'primeng/fileupload';
import { Observable, Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, finalize, map, switchMap, takeUntil } from 'rxjs/operators';
import { Order, OrderStatus, OrderPriority, OrderSource } from '@shared/models/order.model';
import { isOrderLocked as checkOrderLocked } from '@shared/utils/order-locking';

interface SummaryCard {
  label: string;
  value: number;
  icon: string;
  color: string;
  filterStatus?: OrderStatus | null;
}

interface QuickFilterChip {
  label: string;
  status: OrderStatus | null;
  icon: string;
  count: number;
}

interface UserPickOption {
  id: string;
  label: string;
}

@Component({
  selector: 'app-order-list',
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.scss']
})
export class OrderListComponent implements OnInit, OnDestroy {
  orders: Order[] = [];
  filteredOrders: Order[] = [];
  selectedStatus: OrderStatus | null = null;
  globalFilter = '';
  first = 0;
  rows = 10;

  // User role
  isClient = false;
  isDesigner = false;
  isAdmin = false;
  isSuperAdmin = false;

  // Summary cards
  summaryCards: SummaryCard[] = [];

  // Quick filter chips
  quickFilterChips: QuickFilterChip[] = [];
  activeQuickFilter: OrderStatus | null | 'all' = 'all';

  // Order Detail Modal
  showOrderDetailModal = false;
  selectedOrderId: string | null = null;

  statuses = (() => {
    const statusValues = Object.values(OrderStatus);
    const statusMap = statusValues.map(status => {
      let formattedLabel = status.replace(/([A-Z])/g, ' $1').trim();
      const labelMap: { [key: string]: string } = {
        'WaitingForAdminApproval': 'Awaiting Admin',
        'Waiting For Admin Approval': 'Awaiting Admin',
        'PriceApprovalPending': 'Price Approval Pending',
        'Price Approval Pending': 'Price Approval Pending',
        'InProgress': 'In Progress',
        'In Progress': 'In Progress',
        'PreviewDelivered': 'Preview Delivered',
        'Preview Delivered': 'Preview Delivered',
        'RevisionRequested': 'Revision Requested',
        'Revision Requested': 'Revision Requested',
        'ClientApproved': 'Approved',
        'Final Approved': 'Approved',
        'Completed': 'Completed',
        'Cancelled': 'Cancelled',
        'Pending': 'Pending',
        'Paid': 'Paid',
        'Processing': 'Processing',
        'CancelledByUser': 'Cancelled By User',
        'Cancelled By User': 'Cancelled By User',
        'CancelledByAdmin': 'Cancelled By Admin',
        'Cancelled By Admin': 'Cancelled By Admin',
        'Refunded': 'Refunded',
        'Failed': 'Failed',
        'Archived': 'Archived'
      };
      let displayLabel = labelMap[status] || labelMap[formattedLabel] || formattedLabel;
      if (!displayLabel) {
        displayLabel = formattedLabel;
      }
      return { label: displayLabel, value: status, originalLabel: formattedLabel };
    });

    const priority: { [key: string]: number } = {
      'WaitingForAdminApproval': 1, 'PriceApprovalPending': 2, 'InProgress': 3,
      'PreviewDelivered': 4, 'RevisionRequested': 5, 'ClientApproved': 6,
      'Completed': 7, 'Pending': 8, 'Paid': 9, 'Processing': 10,
      'Cancelled': 11, 'CancelledByUser': 12, 'CancelledByAdmin': 13,
      'Refunded': 14, 'Failed': 15, 'Archived': 16
    };
    return statusMap.sort((a, b) => (priority[a.value] || 99) - (priority[b.value] || 99));
  })();
  priorities = Object.values(OrderPriority);

  // Dialog states
  showAssignDialog = false;
  showStatusDialog = false;
  showUploadDialog = false;
  selectedFiles: File[] = [];
  uploadingFiles = false;
  uploadSuccess = false;
  uploadedFilesCount = 0;
  showConfirmDialog = false;
  selectedOrder: Order | null = null;

  @ViewChild('fileUpload') fileUploadComponent!: FileUpload;
  @ViewChild('quickCompletedFileUpload') private quickCompletedFileUpload?: FileUpload;
  /** Debounced user search for autocomplete (shared pipeline). */
  private readonly userSearch$ = new Subject<{ scope: 'qc' | 'qd' | 'ad'; query: string }>();
  quickClientSuggestions: UserPickOption[] = [];
  quickDesignerSuggestions: UserPickOption[] = [];
  assignDesignerSuggestions: UserPickOption[] = [];
  selectedQuickClient: UserPickOption | null = null;
  selectedQuickDesigner: UserPickOption | null = null;
  selectedAssignDesigner: UserPickOption | null = null;
  newStatus: { label: string; value: OrderStatus } | null = null;
  availableStatuses: { label: string; value: OrderStatus }[] = [];

  private destroy$ = new Subject<void>();

  isOrdersLoading = false;
  hasOrdersLoadedOnce = false;

  get showOrdersSkeleton(): boolean {
    return this.isOrdersLoading && !this.hasOrdersLoadedOnce;
  }

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private realtimeNotification: RealtimeNotificationService,
    private messageService: MessageService,
    private router: Router,
    private route: ActivatedRoute,
    private sharedListData: SharedListDataService,
    private dashboardService: DashboardService
  ) {}

  ngOnInit(): void {
    this.filteredOrders = [];

    this.userSearch$
      .pipe(
        debounceTime(300),
        distinctUntilChanged((a, b) => a.scope === b.scope && a.query === b.query),
        switchMap(({ scope, query }) => {
          const role = scope === 'qc' ? 'Client' : 'Designer';
          return this.sharedListData.searchUsers(query, role, 15).pipe(
            map((items: UserTypeaheadItem[]) => ({
              scope,
              items: items.map((u) => ({ id: u.id, label: u.label }))
            }))
          );
        }),
        takeUntil(this.destroy$)
      )
      .subscribe(({ scope, items }) => {
        if (scope === 'qc') {
          this.quickClientSuggestions = items;
        } else if (scope === 'qd') {
          this.quickDesignerSuggestions = items;
        } else {
          this.assignDesignerSuggestions = items;
        }
      });

    // Check user role
    const user = this.authService.getCurrentUser();
    this.isClient = user?.role === 'Client';
    this.isDesigner = user?.role === 'Designer';
    this.isAdmin = user?.role === 'Admin';
    this.isSuperAdmin = user?.role === 'SuperAdmin';

    this.loadOrders();
    this.handleQuotePrefillFromQuery();

    // Real-time order updates: refresh grid when order events arrive
    this.realtimeNotification.orderUpdates$
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => this.handleOrderUpdate(data));
  }

  private handleOrderUpdate(data: { orderId: string; status?: string; invoiceId?: string }): void {
    const orderId = data.orderId?.toLowerCase?.() ?? data.orderId;
    if (orderId === '**reconnect**') {
      if (this.isAdminOrSuper) {
        this.sharedListData.clearOrdersAdminCache();
      }
      this.loadOrders();
      return;
    }
    const existing = this.orders.find(o => (o.id ?? '').toLowerCase() === orderId);
    if (existing) {
      if (data.status) {
        existing.status = data.status as OrderStatus;
      }
      if (data.invoiceId) {
        existing.hasInvoice = true;
      }
      this.buildSummaryCards();
      this.buildQuickFilterChips();
      this.applyFilters();
    } else {
      // Order not in current view (e.g. new order for admin) - full refresh
      if (this.isAdminOrSuper) {
        this.sharedListData.clearOrdersAdminCache();
      }
      this.loadOrders();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ═══ ROLE HELPERS ═══
  get isAdminOrSuper(): boolean {
    return this.isAdmin || this.isSuperAdmin;
  }

  get pageTitle(): string {
    if (this.isClient) return 'My Orders';
    if (this.isDesigner) return 'Orders';
    return 'Orders Management';
  }

  get pageSubtitle(): string {
    if (this.isClient) return 'Track and manage your orders';
    if (this.isDesigner) return 'Orders assigned to you';
    return 'View and manage all orders';
  }

  // ═══ DATA LOADING ═══
  loadOrders(): void {
    const user = this.authService.getCurrentUser();

    if (!user) {
      this.orders = [];
      this.filteredOrders = [];
      this.isOrdersLoading = false;
      this.hasOrdersLoadedOnce = true;
      return;
    }

    this.isOrdersLoading = true;

    const track = <T>(source: Observable<T>) =>
      source.pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.isOrdersLoading = false;
          this.hasOrdersLoadedOnce = true;
        })
      );

    if (user.role === 'Client') {
      track(this.apiService.get<any>('orders/my-orders')).subscribe({
        next: (response) => this.applyOrdersFromResponse(response, false),
        error: (err) => this.finishOrdersLoadError(err)
      });
      return;
    }
    if (user.role === 'Designer') {
      track(this.apiService.get<any>('orders/assigned-orders')).subscribe({
        next: (response) => this.applyOrdersFromResponse(response, false),
        error: (err) => this.finishOrdersLoadError(err)
      });
      return;
    }

    track(this.sharedListData.fetchAllOrdersUncached()).subscribe({
      next: (list) => {
        const orders = Array.isArray(list) ? list : [];
        this.applyOrdersArray(orders);
      },
      error: (err) => this.finishOrdersLoadError(err)
    });
  }

  private applyOrdersFromResponse(response: unknown, isPagedItems: boolean): void {
    const orders = isPagedItems
      ? ApiService.extractItems<any>(response)
      : Array.isArray(response)
        ? response
        : [];
    this.applyOrdersArray(orders);
  }

  private applyOrdersArray(orders: unknown[]): void {
    if (!orders || !Array.isArray(orders)) {
      this.orders = [];
      this.filteredOrders = [];
      return;
    }

    this.orders = orders.map(order => this.transformOrder(order as any));
    this.buildSummaryCards();
    this.buildQuickFilterChips();
    this.applyFilters();
  }

  private finishOrdersLoadError(error?: unknown): void {
    let errorMessage = 'Failed to load orders';
    const err = error as { status?: number; error?: { error?: string } } | undefined;
    if (err?.status === 403) {
      errorMessage = 'You do not have permission to view orders';
    } else if (err?.status === 401) {
      errorMessage = 'Please log in to view orders';
    } else if (err?.error?.error) {
      errorMessage = err.error.error;
    }
    this.messageService.add({ severity: 'error', summary: 'Error', detail: errorMessage });
    this.orders = [];
    this.filteredOrders = [];
  }

  // ═══ SUMMARY CARDS ═══
  private buildSummaryCards(): void {
    const counts = this.getStatusCounts();

    if (this.isClient) {
      this.summaryCards = [
        { label: 'Total Orders', value: this.orders.length, icon: 'pi pi-list', color: 'primary', filterStatus: null },
        { label: 'Active Orders', value: counts.active, icon: 'pi pi-shopping-cart', color: 'info', filterStatus: OrderStatus.InProgress },
        { label: 'Awaiting Approval', value: counts.awaitingApproval, icon: 'pi pi-clock', color: 'warning', filterStatus: OrderStatus.PreviewDelivered },
        { label: 'Completed', value: counts.completed, icon: 'pi pi-check-circle', color: 'success', filterStatus: OrderStatus.Completed }
      ];
    } else if (this.isDesigner) {
      this.summaryCards = [
        { label: 'Total Orders', value: this.orders.length, icon: 'pi pi-briefcase', color: 'primary', filterStatus: null },
        { label: 'In Progress', value: counts.inProgress, icon: 'pi pi-briefcase', color: 'info', filterStatus: OrderStatus.InProgress },
        { label: 'Due Today', value: counts.dueToday, icon: 'pi pi-calendar', color: 'warning', filterStatus: null },
        { label: 'Completed', value: counts.completed, icon: 'pi pi-check-circle', color: 'success', filterStatus: OrderStatus.Completed }
      ];
    } else {
      // Admin / SuperAdmin
      this.summaryCards = [
        { label: 'New Orders', value: counts.newOrders, icon: 'pi pi-inbox', color: 'primary', filterStatus: OrderStatus.WaitingForAdminApproval },
        { label: 'In QA', value: counts.inQA, icon: 'pi pi-search', color: 'info', filterStatus: OrderStatus.PreviewDelivered },
        { label: 'Pending Approval', value: counts.pendingClientApproval, icon: 'pi pi-clock', color: 'warning', filterStatus: OrderStatus.PriceApprovalPending },
        { label: 'Overdue', value: counts.overdue, icon: 'pi pi-exclamation-triangle', color: 'danger', filterStatus: null }
      ];
    }
  }

  private getStatusCounts(): any {
    const now = new Date();
    const todayStart = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const todayEnd = new Date(todayStart.getTime() + 24 * 60 * 60 * 1000);

    const activeStatuses = [OrderStatus.InProgress, OrderStatus.RevisionRequested, OrderStatus.PreviewDelivered, OrderStatus.PriceApprovalPending, OrderStatus.WaitingForAdminApproval];

    return {
      active: this.orders.filter(o => activeStatuses.includes(o.status)).length,
      awaitingApproval: this.orders.filter(o =>
        o.status === OrderStatus.PreviewDelivered || o.status === OrderStatus.PriceApprovalPending
      ).length,
      completed: this.orders.filter(o =>
        o.status === OrderStatus.Completed || o.status === OrderStatus.ClientApproved
      ).length,
      inProgress: this.orders.filter(o => o.status === OrderStatus.InProgress).length,
      dueToday: this.orders.filter(o => {
        if (!o.dueDate) return false;
        const d = new Date(o.dueDate);
        return d >= todayStart && d < todayEnd && !this.isTerminalStatus(o.status);
      }).length,
      overdue: this.orders.filter(o => this.isOverdue(o.dueDate) && !this.isTerminalStatus(o.status)).length,
      newOrders: this.orders.filter(o => o.status === OrderStatus.WaitingForAdminApproval).length,
      inQA: this.orders.filter(o => o.status === OrderStatus.PreviewDelivered).length,
      pendingClientApproval: this.orders.filter(o => o.status === OrderStatus.PriceApprovalPending).length
    };
  }

  isTerminalStatus(status: OrderStatus): boolean {
    return [OrderStatus.Completed, OrderStatus.Cancelled,
      OrderStatus.CancelledByUser, OrderStatus.CancelledByAdmin, OrderStatus.Refunded,
      ].includes(status);
  }

  onSummaryCardClick(card: SummaryCard): void {
    if (card.filterStatus !== undefined) {
      this.selectedStatus = card.filterStatus;
      this.activeQuickFilter = card.filterStatus;
      this.applyFilters();
      this.first = 0;
    }
  }

  // ═══ QUICK FILTER CHIPS ═══
  private buildQuickFilterChips(): void {
    const chipConfigs: { status: OrderStatus | null; label: string; icon: string; roles: string[] }[] = [
      { status: null, label: 'All', icon: 'pi pi-list', roles: ['Client', 'Designer', 'Admin', 'SuperAdmin'] },
      { status: OrderStatus.WaitingForAdminApproval, label: 'Awaiting Admin', icon: 'pi pi-inbox', roles: ['Admin', 'SuperAdmin'] },
      { status: OrderStatus.PriceApprovalPending, label: 'Price Pending', icon: 'pi pi-dollar', roles: ['Client', 'Admin', 'SuperAdmin'] },
      { status: OrderStatus.InProgress, label: 'In Progress', icon: 'pi pi-spin pi-spinner', roles: ['Client', 'Designer', 'Admin', 'SuperAdmin'] },
      { status: OrderStatus.PreviewDelivered, label: 'Preview Ready', icon: 'pi pi-eye', roles: ['Client', 'Admin', 'SuperAdmin'] },
      { status: OrderStatus.RevisionRequested, label: 'Revisions', icon: 'pi pi-refresh', roles: ['Designer', 'Admin', 'SuperAdmin'] },
      { status: OrderStatus.ClientApproved, label: 'Approved', icon: 'pi pi-check', roles: ['Client', 'Admin', 'SuperAdmin'] },
      { status: OrderStatus.Completed, label: 'Completed', icon: 'pi pi-check-circle', roles: ['Client', 'Designer', 'Admin', 'SuperAdmin'] },
      { status: OrderStatus.Cancelled, label: 'Cancelled', icon: 'pi pi-times', roles: ['Client', 'Admin', 'SuperAdmin'] }
    ];

    const userRole = this.authService.getCurrentUser()?.role || '';

    this.quickFilterChips = chipConfigs
      .filter(c => c.roles.includes(userRole))
      .map(c => {
        const count = c.status === null ? this.orders.length : c.status === OrderStatus.Cancelled
          ? this.orders.filter(o => [OrderStatus.Cancelled, OrderStatus.CancelledByUser, OrderStatus.CancelledByAdmin].includes(o.status)).length
          : this.orders.filter(o => o.status === c.status).length;
        return { label: c.label, status: c.status, icon: c.icon, count };
      })
      .filter(c => c.status === null || c.count > 0); // hide zero-count chips (except "All")
  }

  onQuickFilterClick(chip: QuickFilterChip): void {
    this.activeQuickFilter = chip.status === null ? 'all' : chip.status;
    this.selectedStatus = chip.status;
    this.applyFilters();
    this.first = 0;
  }

  isActiveChip(chip: QuickFilterChip): boolean {
    if (chip.status === null) return this.activeQuickFilter === 'all';
    return this.activeQuickFilter === chip.status;
  }

  // ═══ FILTERING ═══
  applyFilters(): void {
    let filtered = [...this.orders];

    // Apply status filter (Cancelled includes CancelledByUser and CancelledByAdmin)
    if (this.selectedStatus) {
      if (this.selectedStatus === OrderStatus.Cancelled) {
        const cancelledStatuses = [OrderStatus.Cancelled, OrderStatus.CancelledByUser, OrderStatus.CancelledByAdmin];
        filtered = filtered.filter(order => cancelledStatuses.includes(order.status));
      } else {
        filtered = filtered.filter(order => order.status === this.selectedStatus);
      }
    }

    // Apply global text search
    if (this.globalFilter && this.globalFilter.trim()) {
      const search = this.globalFilter.trim().toLowerCase();
      filtered = filtered.filter(order => {
        const titleMatch = order.title?.toLowerCase().includes(search);
        const statusMatch = order.status?.toLowerCase().includes(search);
        const clientMatch = order.client ?
          `${order.client.firstName} ${order.client.lastName} ${order.client.companyName}`.toLowerCase().includes(search) : false;
        const designerMatch = order.designer ?
          `${order.designer.firstName} ${order.designer.lastName}`.toLowerCase().includes(search) : false;
        return titleMatch || statusMatch || clientMatch || designerMatch;
      });
    }

    this.filteredOrders = filtered;
  }

  onStatusFilterChange(): void {
    this.activeQuickFilter = this.selectedStatus || 'all';
    this.applyFilters();
    this.first = 0;
  }

  onGlobalFilterChange(): void {
    this.applyFilters();
    this.first = 0;
  }

  clearAllFilters(): void {
    this.selectedStatus = null;
    this.globalFilter = '';
    this.activeQuickFilter = 'all';
    this.applyFilters();
    this.first = 0;
  }

  // ═══ TRANSFORM & MAP ═══
  private transformOrder(backendOrder: any): Order {
    const status = this.mapStatus(backendOrder.status || backendOrder.Status);

    return {
      id: backendOrder.id || backendOrder.Id,
      clientId: backendOrder.clientId || backendOrder.ClientId || '',
      designerId: (backendOrder.designer || backendOrder.Designer)?.userId || (backendOrder.designer || backendOrder.Designer)?.UserId || backendOrder.designerId || backendOrder.DesignerId,
      title: backendOrder.title || backendOrder.Title || '',
      description: backendOrder.description || backendOrder.Description || '',
      status: status,
      priority: backendOrder.priority || backendOrder.Priority ?
        (typeof (backendOrder.priority || backendOrder.Priority) === 'string' ?
          (backendOrder.priority || backendOrder.Priority) as OrderPriority :
          OrderPriority.Medium
        ) : OrderPriority.Medium,
      price: backendOrder.price || backendOrder.Price || 0,
      proposedPrice: backendOrder.proposedPrice || backendOrder.ProposedPrice,
      approvedPrice: backendOrder.approvedPrice || backendOrder.ApprovedPrice,
      priceApprovalStatus: backendOrder.priceApprovalStatus || backendOrder.PriceApprovalStatus,
      requiresPriceApproval: backendOrder.requiresPriceApproval || backendOrder.RequiresPriceApproval || false,
      priceApproved: backendOrder.priceApproved || backendOrder.PriceApproved || false,
      designCategory: backendOrder.designCategory || backendOrder.DesignCategory,
      designType: backendOrder.designType || backendOrder.DesignType,
      instructions: backendOrder.instructions || backendOrder.Instructions,
      requiredFormats: backendOrder.requiredFormats || backendOrder.RequiredFormats,
      requirements: backendOrder.requirements || backendOrder.Requirements,
      colorPreferences: backendOrder.colorPreferences || backendOrder.ColorPreferences,
      stylePreferences: backendOrder.stylePreferences || backendOrder.StylePreferences,
      fileCount: backendOrder.fileCount || backendOrder.FileCount || 0,
      visibleFileCount: backendOrder.visibleFileCount || backendOrder.VisibleFileCount || 0,
      revisionCount: backendOrder.revisionCount ?? backendOrder.RevisionCount ?? 0,
      revisionLimit: backendOrder.revisionLimit ?? backendOrder.RevisionLimit,
      revisionLimitExceeded: backendOrder.revisionLimitExceeded ?? backendOrder.RevisionLimitExceeded ?? false,
      commentCount: backendOrder.commentCount || backendOrder.CommentCount || 0,
      createdAt: backendOrder.createdAt ? new Date(backendOrder.createdAt) : (backendOrder.CreatedAt ? new Date(backendOrder.CreatedAt) : new Date()),
      updatedAt: backendOrder.updatedAt ? new Date(backendOrder.updatedAt) : (backendOrder.UpdatedAt ? new Date(backendOrder.UpdatedAt) : undefined),
      dueDate: backendOrder.dueDate ? new Date(backendOrder.dueDate) : (backendOrder.Deadline ? new Date(backendOrder.Deadline) : undefined),
      client: backendOrder.client || backendOrder.Client ? {
        id: (backendOrder.client || backendOrder.Client)?.id || (backendOrder.client || backendOrder.Client)?.Id,
        companyName: (backendOrder.client || backendOrder.Client)?.companyName || (backendOrder.client || backendOrder.Client)?.CompanyName || '',
        firstName: (backendOrder.client || backendOrder.Client)?.firstName || (backendOrder.client || backendOrder.Client)?.FirstName || '',
        lastName: (backendOrder.client || backendOrder.Client)?.lastName || (backendOrder.client || backendOrder.Client)?.LastName || '',
        email: (backendOrder.client || backendOrder.Client)?.email || (backendOrder.client || backendOrder.Client)?.Email,
        phoneNumber: (backendOrder.client || backendOrder.Client)?.phoneNumber || (backendOrder.client || backendOrder.Client)?.PhoneNumber
      } : undefined,
      designer: backendOrder.designer || backendOrder.Designer ? {
        id: (backendOrder.designer || backendOrder.Designer)?.id || (backendOrder.designer || backendOrder.Designer)?.Id,
        firstName: (backendOrder.designer || backendOrder.Designer)?.firstName || (backendOrder.designer || backendOrder.Designer)?.FirstName || '',
        lastName: (backendOrder.designer || backendOrder.Designer)?.lastName || (backendOrder.designer || backendOrder.Designer)?.LastName || '',
        email: (backendOrder.designer || backendOrder.Designer)?.email || (backendOrder.designer || backendOrder.Designer)?.Email
      } : undefined,
      assignedDesignerDisplayName: backendOrder.assignedDesignerDisplayName || backendOrder.AssignedDesignerDisplayName,
      orderSource: (backendOrder.orderSource || backendOrder.OrderSource || OrderSource.Portal) as OrderSource
    };
  }

  private mapStatus(status: string): OrderStatus {
    if (!status) return OrderStatus.WaitingForAdminApproval;

    const statusStr = status.toString().trim();
    switch (statusStr) {
      case 'WaitingForAdminApproval': case '1': return OrderStatus.WaitingForAdminApproval;
      case 'PriceApprovalPending': case '2': return OrderStatus.PriceApprovalPending;
      case 'InProgress': case 'In Progress': case '3': return OrderStatus.InProgress;
      case 'PreviewDelivered': case '4': return OrderStatus.PreviewDelivered;
      case 'RevisionRequested': case '5': return OrderStatus.RevisionRequested;
      case 'FinalApproved': case 'ClientApproved': case '6': return OrderStatus.ClientApproved;
      case 'Completed': case '7': return OrderStatus.Completed;
      case 'Cancelled': case '8': return OrderStatus.Cancelled;
      case 'CancelledByUser': case '13': return OrderStatus.CancelledByUser;
      case 'CancelledByAdmin': case '14': return OrderStatus.CancelledByAdmin;
      case 'Refunded': case '15': return OrderStatus.Refunded;
      default: return OrderStatus.WaitingForAdminApproval;
    }
  }

  // ═══ STATUS & FORMATTING ═══
  getStatusSeverity(status: OrderStatus): string {
    const severityMap: { [key: string]: string } = {
      'WaitingForAdminApproval': 'warning',
      'PriceApprovalPending': 'info',
      'InProgress': 'info',
      'PreviewDelivered': 'success',
      'RevisionRequested': 'warning',
      'ClientApproved': 'success',
      'Completed': 'success',
      'Cancelled': 'danger',
      'CancelledByUser': 'danger',
      'CancelledByAdmin': 'danger',
      'Refunded': 'warning',
      'Failed': 'danger',
      'Archived': 'secondary'
    };
    return severityMap[status] || 'secondary';
  }

  getStatusLabel(status: OrderStatus): string {
    const found = this.statuses.find(s => s.value === status);
    return found?.label || status;
  }

  getPrioritySeverity(priority: OrderPriority): string {
    const severityMap: { [key: string]: string } = {
      'Low': 'success', 'Medium': 'warning', 'High': 'warning', 'Urgent': 'danger'
    };
    return severityMap[priority] || 'secondary';
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    try {
      const dateObj = new Date(date);
      if (isNaN(dateObj.getTime())) return 'N/A';
      return dateObj.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
    } catch {
      return 'N/A';
    }
  }

  formatRelativeDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    try {
      const dateStr = typeof date === 'string' ? date : date.toISOString();
      const dateObj = new Date(dateStr.endsWith('Z') || dateStr.includes('+') ? dateStr : dateStr + 'Z');
      if (isNaN(dateObj.getTime())) return 'N/A';
      const now = new Date();
      const diffMs = now.getTime() - dateObj.getTime();
      const diffMins = Math.floor(diffMs / 60000);
      if (diffMins < 1) return 'Just now';
      if (diffMins < 60) return `${diffMins}m ago`;
      const diffHrs = Math.floor(diffMins / 60);
      if (diffHrs < 24) return `${diffHrs}h ago`;
      const diffDays = Math.floor(diffHrs / 24);
      if (diffDays < 7) return `${diffDays}d ago`;
      return this.formatDate(date);
    } catch {
      return 'N/A';
    }
  }

  isOverdue(dueDate: Date | string | undefined): boolean {
    if (!dueDate) return false;
    const d = new Date(dueDate);
    return d < new Date() && d.getTime() !== 0;
  }

  // ═══ ORDER DETAIL MODAL ═══
  viewOrder(orderId: string): void {
    this.selectedOrderId = orderId;
    this.showOrderDetailModal = true;
  }

  onOrderDetailClose(): void {
    this.showOrderDetailModal = false;
    this.selectedOrderId = null;
  }

  onOrderUpdated(): void {
    this.dashboardService.invalidateDashboardCache();
    this.loadOrders();
  }

  // ═══ ORDER CREATE ═══
  showOrderCreateModal = false;
  quotePrefillData: any = null;
  quoteConversionId: string | null = null;
  showQuickCompletedDialog = false;
  quickCompletedSubmitting = false;
  quickCompletedFiles: File[] = [];
  quickCompletedUseNewClient = false;
  quickCompletedModel: any = {
    clientUserId: null,
    designerUserId: null,
    title: '',
    description: '',
    price: null,
    completedAt: new Date(),
    notes: '',
    newClient: {
      email: '',
      firstName: '',
      lastName: '',
      companyName: ''
    }
  };

  canCreateOrder(): boolean {
    return this.authService.hasRole('Client');
  }

  canAddCompletedOrder(): boolean {
    return this.isAdminOrSuper;
  }

  createOrder(): void {
    this.quotePrefillData = null;
    this.showOrderCreateModal = true;
  }

  openQuoteCreateDialog(): void {
    // Reuse the Quotes page "Get a Quote" flow by opening the client dialog via query param.
    this.router.navigate(['/quotes'], { queryParams: { openCreate: 'true' } });
  }

  private handleQuotePrefillFromQuery(): void {
    this.route.queryParamMap
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        if (params.get('source') !== 'quote') return;
        this.quotePrefillData = {
          title: params.get('logoName') ?? '',
          description: params.get('description') ?? '',
          price: Number(params.get('price') ?? '0')
        };
        this.quoteConversionId = params.get('quoteId');
        this.showOrderCreateModal = true;
      });
  }

  openQuickCompletedOrderDialog(): void {
    this.quickCompletedSubmitting = false;
    this.quickCompletedFiles = [];
    this.quickCompletedUseNewClient = false;
    this.quickCompletedModel = {
      clientUserId: null,
      designerUserId: null,
      title: '',
      description: '',
      price: null,
      completedAt: new Date(),
      notes: '',
        newClient: { email: '', firstName: '', lastName: '', companyName: '' }
    };
    this.selectedQuickClient = null;
    this.selectedQuickDesigner = null;
    this.quickClientSuggestions = [];
    this.quickDesignerSuggestions = [];
    this.showQuickCompletedDialog = true;
  }

  onQuickClientComplete(event: { query: string }): void {
    this.userSearch$.next({ scope: 'qc', query: event.query ?? '' });
  }

  onQuickDesignerComplete(event: { query: string }): void {
    this.userSearch$.next({ scope: 'qd', query: event.query ?? '' });
  }

  onAssignDesignerComplete(event: { query: string }): void {
    this.userSearch$.next({ scope: 'ad', query: event.query ?? '' });
  }

  onQuickCompletedFileSelect(event: any): void {
    const files: File[] = event.files ? Array.from(event.files) : [];
    const deduped = files.filter(file => !this.quickCompletedFiles.some(f => f.name === file.name && f.size === file.size));
    this.quickCompletedFiles = [...this.quickCompletedFiles, ...deduped];
  }

  removeQuickCompletedFile(index: number): void {
    this.quickCompletedFiles.splice(index, 1);
  }

  submitQuickCompletedOrder(): void {
    const m = this.quickCompletedModel;
    const clientId = this.selectedQuickClient?.id ?? m.clientUserId;
    const designerId = this.selectedQuickDesigner?.id ?? m.designerUserId;
    const missingClient = !this.quickCompletedUseNewClient && !clientId;
    const missingNewClient = this.quickCompletedUseNewClient && (!m.newClient?.email || !m.newClient?.firstName || !m.newClient?.lastName || !m.newClient?.companyName);
    if (missingClient || missingNewClient || !designerId || !m.title?.trim() || !m.price || !m.completedAt || this.quickCompletedFiles.length === 0) {
      this.messageService.add({ severity: 'warn', summary: 'Missing Fields', detail: 'Please complete all required fields.' });
      return;
    }

    const payload: any = {
      clientUserId: this.quickCompletedUseNewClient ? null : clientId,
      newClient: this.quickCompletedUseNewClient ? m.newClient : null,
      title: m.title.trim(),
      description: m.description?.trim() || null,
      designerUserId: designerId,
      price: Number(m.price),
      completedAt: new Date(m.completedAt).toISOString(),
      notes: m.notes?.trim() || null
    };

    const formData = new FormData();
    formData.append('order', JSON.stringify(payload));
    this.quickCompletedFiles.forEach(file => formData.append('files', file));

    this.quickCompletedSubmitting = true;
    this.apiService.post<any>('orders/manual-completed', formData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (created) => {
          this.quickCompletedSubmitting = false;
          this.showQuickCompletedDialog = false;
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Completed order added successfully' });
          this.sharedListData.clearAll();
          this.dashboardService.invalidateDashboardCache();
          this.loadOrders();
          if (created?.id) {
            this.selectedOrderId = created.id;
            this.showOrderDetailModal = true;
          }
        },
        error: (error) => {
          this.quickCompletedSubmitting = false;
          this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error?.error || 'Failed to add completed order' });
        }
      });
  }

  onOrderCreateClose(): void {
    this.showOrderCreateModal = false;
  }

  onOrderCreated(order: any): void {
    this.dashboardService.invalidateDashboardCache();
    if (this.quoteConversionId && order?.id) {
      this.apiService.post(`quotes/${this.quoteConversionId}/convert-to-order`, { orderId: order.id })
        .pipe(takeUntil(this.destroy$))
        .subscribe({ next: () => this.loadOrders() });
      this.quoteConversionId = null;
      this.quotePrefillData = null;
    }
    this.loadOrders();
    if (order && order.id) {
      this.selectedOrderId = order.id;
      this.showOrderDetailModal = true;
    }
  }

  // ═══ PERMISSION CHECKS ═══
  canAssignDesigner(): boolean {
    return this.isAdmin || this.isSuperAdmin;
  }

  canChangeStatus(): boolean {
    return this.isAdmin || this.isSuperAdmin;
  }

  canUploadFiles(): boolean {
    return this.isAdmin || this.isSuperAdmin || this.isDesigner;
  }

  isOrderLocked(status: string | undefined | null): boolean {
    return checkOrderLocked(status);
  }

  /** Billable queue / flexible invoice only includes Completed + BillingEligible + uninvoiced orders. */
  canAddToInvoice(order: Order): boolean {
    return (this.isAdmin || this.isSuperAdmin)
      && order.status === OrderStatus.Completed
      && !order.hasInvoice;
  }

  // Client-specific actions
  canApproveOrder(order: Order): boolean {
    return this.isClient && order.status === OrderStatus.PreviewDelivered;
  }

  canRequestRevision(order: Order): boolean {
    return this.isClient
      && order.status === OrderStatus.PreviewDelivered
      && !order.revisionLimitExceeded;
  }

  // Designer-specific actions
  canSubmitPreview(order: Order): boolean {
    // Designers cannot change status - only Admin can set PreviewDelivered via SendFilesToClient
    return false;
  }

  // ═══ ACTION DIALOGS ═══
  openAssignDialog(order: Order): void {
    this.selectedOrder = order;
    this.assignDesignerSuggestions = [];
    this.selectedAssignDesigner = order.designerId
      ? { id: order.designerId, label: 'Current designer — type to search and replace' }
      : null;
    this.showAssignDialog = true;
  }

  assignDesigner(): void {
    if (!this.selectedOrder || !this.selectedAssignDesigner?.id) return;

    this.apiService.post(`orders/${this.selectedOrder.id}/assign`, { designerId: this.selectedAssignDesigner.id })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Designer assigned successfully' });
          this.showAssignDialog = false;
          this.dashboardService.invalidateDashboardCache();
          this.loadOrders();
        },
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to assign designer' });
        }
      });
  }

  openStatusDialog(order: Order): void {
    this.selectedOrder = order;
    const allowedStatuses = this.getAllowedStatusesForRole(order);
    const filteredStatuses = this.statuses.filter(s => allowedStatuses.includes(s.value));
    const currentStatusObj = filteredStatuses.find(s => s.value === order.status);
    this.newStatus = currentStatusObj || filteredStatuses[0];
    this.availableStatuses = filteredStatuses;
    this.showStatusDialog = true;
  }

  private getAllowedStatusesForRole(order: Order): OrderStatus[] {
    const user = this.authService.getCurrentUser();
    if (!user) return [];

    const currentStatus = order.status as OrderStatus;
    const isOrderOwner = user.role === 'Client' && order.clientId && user.id === order.clientId;
    const allowedStatuses: OrderStatus[] = [];

    switch (user.role) {
      case 'Client':
        if (currentStatus === OrderStatus.PreviewDelivered) {
          allowedStatuses.push(OrderStatus.RevisionRequested, OrderStatus.ClientApproved);
        }
        if (isOrderOwner && (
          currentStatus === OrderStatus.WaitingForAdminApproval ||
          currentStatus === OrderStatus.PriceApprovalPending
        )) {
          allowedStatuses.push(OrderStatus.Cancelled);
        }
        break;
      case 'Designer':
        // Designers cannot change order status - upload preview files only
        break;
      case 'Admin':
      case 'SuperAdmin':
        // Revision workflow: from RevisionRequested, Admin can only set PreviewDelivered or CancelledByAdmin (not InProgress)
        if (currentStatus === OrderStatus.RevisionRequested) {
          allowedStatuses.push(OrderStatus.PreviewDelivered, OrderStatus.CancelledByAdmin);
        } else {
          allowedStatuses.push(OrderStatus.InProgress, OrderStatus.PreviewDelivered, OrderStatus.Completed, OrderStatus.CancelledByAdmin);
        }
        break;
    }

    if (!allowedStatuses.includes(currentStatus)) {
      allowedStatuses.push(currentStatus);
    }
    return allowedStatuses;
  }

  changeStatus(): void {
    if (!this.selectedOrder || !this.newStatus) return;
    const statusValue = typeof this.newStatus === 'object' ? this.newStatus.value : this.newStatus;

    this.apiService.put(`orders/${this.selectedOrder.id}/status`, { status: statusValue })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Order status updated successfully' });
          this.showStatusDialog = false;
          this.dashboardService.invalidateDashboardCache();
          this.loadOrders();
        },
        error: (error) => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error?.error || 'Failed to update order status' });
        }
      });
  }

  // Quick status update for client actions
  quickApprove(order: Order): void {
    this.apiService.put(`orders/${order.id}/status`, { status: OrderStatus.ClientApproved })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Approved', detail: 'Order has been approved' });
          this.dashboardService.invalidateDashboardCache();
          this.loadOrders();
        },
        error: (error) => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error?.error || 'Failed to approve order' });
        }
      });
  }

  quickRequestRevision(order: Order): void {
    this.apiService.put(`orders/${order.id}/status`, { status: OrderStatus.RevisionRequested })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Revision Requested', detail: 'Revision has been requested' });
          this.dashboardService.invalidateDashboardCache();
          this.loadOrders();
        },
        error: (error) => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error?.error || 'Failed to request revision' });
        }
      });
  }

  // Designer submit preview
  quickSubmitPreview(order: Order): void {
    this.apiService.put(`orders/${order.id}/status`, { status: OrderStatus.PreviewDelivered })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Preview Submitted', detail: 'Preview has been delivered to client' });
          this.dashboardService.invalidateDashboardCache();
          this.loadOrders();
        },
        error: (error) => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error?.error || 'Failed to submit preview' });
        }
      });
  }

  // ═══ FILE UPLOAD ═══
  openUploadDialog(order: Order): void {
    this.selectedOrder = order;
    this.selectedFiles = [];
    this.uploadSuccess = false;
    this.uploadedFilesCount = 0;
    this.showUploadDialog = true;
  }

  onFileSelect(event: any): void {
    const files: File[] = event.files ? Array.from(event.files) : [];
    if (files.length === 0) return;

    const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp'];
    const vectorExtensions = ['.svg', '.pdf', '.ai', '.eps', '.psd'];
    const embroiderExtensions = ['.pes', '.dst', '.jef', '.exp', '.vp3', '.xxx', '.hus', '.art', '.vip', '.vip3', '.shv', '.pec', '.jpm', '.sew', '.emb', '.csd', '.pcs', '.phb', '.phc', '.stx', '.s10', '.dsb', '.zsk'];
    const imageMaxBytes = 10 * 1024 * 1024;  // 10MB
    const vectorMaxBytes = 25 * 1024 * 1024;  // 25MB

    const getMaxSize = (fileName: string): number => {
      const ext = '.' + (fileName.split('.').pop() || '').toLowerCase();
      if (imageExtensions.includes(ext)) return imageMaxBytes;
      if (vectorExtensions.includes(ext) || embroiderExtensions.includes(ext)) return vectorMaxBytes;
      return imageMaxBytes;
    };

    const invalidFiles = files.filter(file => file.size > getMaxSize(file.name));
    if (invalidFiles.length > 0) {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: `${invalidFiles.length} file(s) exceed allowed size (images: 10MB, vector/docs: 25MB)` });
    }

    const validFiles = files.filter(file => {
      if (file.size > getMaxSize(file.name)) return false;
      return !this.selectedFiles.some(ef => ef.name === file.name && ef.size === file.size);
    });

    if (validFiles.length > 0) {
      this.selectedFiles = [...this.selectedFiles, ...validFiles];
      this.uploadSuccess = false;
    }
    this.fileUploadComponent?.clear();
  }

  removeFile(index: number): void {
    this.selectedFiles.splice(index, 1);
  }

  submitFiles(): void {
    if (this.selectedFiles.length === 0 || !this.selectedOrder) {
      this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Please select at least one file' });
      return;
    }

    const user = this.authService.getCurrentUser();
    if (user?.role === 'Designer') {
      this.showConfirmDialog = true;
    } else {
      this.uploadFiles();
    }
  }

  confirmUpload(): void {
    this.showConfirmDialog = false;
    this.uploadFiles();
  }

  cancelUpload(): void {
    this.showConfirmDialog = false;
  }

  private uploadFiles(): void {
    if (this.selectedFiles.length === 0 || !this.selectedOrder) return;

    this.uploadingFiles = true;
    const filesToUpload = [...this.selectedFiles];

    if (this.selectedFiles.length === 1) {
      const formData = new FormData();
      formData.append('file', this.selectedFiles[0]);
      formData.append('fileType', 'Reference');

      this.apiService.post(`files/upload/${this.selectedOrder.id}`, formData)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.uploadedFilesCount += filesToUpload.length;
            this.messageService.add({ severity: 'success', summary: 'Success', detail: `${filesToUpload.length} file(s) uploaded successfully.` });
            this.selectedFiles = [];
            this.uploadingFiles = false;
            this.uploadSuccess = true;
            this.closeUploadDialog();
          },
          error: (error) => {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error?.error || 'Failed to upload file' });
            this.uploadingFiles = false;
          }
        });
    } else {
      const formData = new FormData();
      this.selectedFiles.forEach(file => formData.append('files', file));
      formData.append('fileType', 'Reference');

      this.apiService.post(`files/upload-multiple/${this.selectedOrder.id}`, formData)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (results: any) => {
            const uploadedCount = Array.isArray(results) ? results.length : 0;
            this.uploadedFilesCount += uploadedCount;
            this.messageService.add({ severity: 'success', summary: 'Success', detail: `${uploadedCount} file(s) uploaded successfully.` });
            this.selectedFiles = [];
            this.uploadingFiles = false;
            this.uploadSuccess = true;
            this.closeUploadDialog();
          },
          error: (error) => {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error?.error || 'Failed to upload files' });
            this.uploadingFiles = false;
          }
        });
    }
  }

  closeUploadDialog(): void {
    this.showUploadDialog = false;
    this.selectedFiles = [];
    this.uploadSuccess = false;
    this.uploadedFilesCount = 0;
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  addToInvoice(order: Order): void {
    this.router.navigate(['/invoices/flexible-builder'], {
      queryParams: {
        clientId: order.clientId,
        orderId: order.id,
        mode: 'manual'
      }
    });
  }
}
