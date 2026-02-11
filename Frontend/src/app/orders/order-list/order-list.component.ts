import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { MessageService } from 'primeng/api';
import { FileUpload } from 'primeng/fileupload';
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
  filteredOrders: Order[] = [];
  loading = false;
  selectedStatus: OrderStatus | null = null;
  globalFilter = '';
  first = 0;
  rows = 10;
  
  // User role
  isClient = false;
  isAdmin = false;
  isSuperAdmin = false;
  
  // Order Detail Modal
  showOrderDetailModal = false;
  selectedOrderId: string | null = null;

  statuses = (() => {
    const statusValues = Object.values(OrderStatus);
    console.log('All OrderStatus values:', statusValues);
    
    const statusMap = statusValues.map(status => {
      // Custom label formatting for better readability
      let formattedLabel = status.replace(/([A-Z])/g, ' $1').trim();
      
      // Special formatting for specific statuses
      const labelMap: { [key: string]: string } = {
        'WaitingForAdminApproval': 'Waiting For Admin Approval',
        'Waiting For Admin Approval': 'Waiting For Admin Approval',
        'PriceApprovalPending': 'Price Approval Pending',
        'Price Approval Pending': 'Price Approval Pending',
        'InProgress': 'In Progress',
        'In Progress': 'In Progress',
        'PreviewDelivered': 'Preview Delivered',
        'Preview Delivered': 'Preview Delivered',
        'RevisionRequested': 'Revision Requested',
        'Revision Requested': 'Revision Requested',
        'FinalApproved': 'Approved',
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
      
      // Try both original status value and formatted label
      let displayLabel = labelMap[status] || labelMap[formattedLabel] || formattedLabel;
      
      // Explicit handling for statuses that might not match due to formatting
      if (!displayLabel) {
        if (status === 'Completed') {
          displayLabel = 'Completed';
        } else if (status === 'FinalApproved') {
          displayLabel = 'Approved';
        } else {
          displayLabel = formattedLabel;
        }
      }
      
      const result = {
        label: displayLabel,
        value: status,
        originalLabel: formattedLabel
      };
      
      // Debug specific statuses
      if (status === 'Completed' || status === 'FinalApproved') {
        console.log(`Status: ${status}, Formatted: ${formattedLabel}, Display: ${displayLabel}`);
      }
      
      return result;
    });
    
    // Sort statuses to show important ones first
    const priority: { [key: string]: number } = {
      'WaitingForAdminApproval': 1,
      'PriceApprovalPending': 2,
      'InProgress': 3,
      'PreviewDelivered': 4,
      'RevisionRequested': 5,
      'FinalApproved': 6,
      'Completed': 7,
      'Pending': 8,
      'Paid': 9,
      'Processing': 10,
      'Cancelled': 11,
      'CancelledByUser': 12,
      'CancelledByAdmin': 13,
      'Refunded': 14,
      'Failed': 15,
      'Archived': 16
    };
    
    const sorted = statusMap.sort((a, b) => {
      return (priority[a.value] || 99) - (priority[b.value] || 99);
    });
    
    console.log('Final sorted statuses:', sorted.map(s => ({ label: s.label, value: s.value })));
    console.log('Completed status found:', sorted.find(s => s.value === 'Completed'));
    console.log('FinalApproved status found:', sorted.find(s => s.value === 'FinalApproved'));
    
    return sorted;
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
  availableDesigners: any[] = [];
  selectedDesignerId: string | null = null;
  newStatus: { label: string; value: OrderStatus } | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private messageService: MessageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    console.log('OrderListComponent ngOnInit called');
    this.filteredOrders = [];
    
    // Check user role
    const user = this.authService.getCurrentUser();
    this.isClient = user?.role === 'Client';
    this.isAdmin = user?.role === 'Admin';
    this.isSuperAdmin = user?.role === 'SuperAdmin';
    
    this.loadOrders();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadOrders(): void {
    console.log('loadOrders() called');
    this.loading = true;
    const user = this.authService.getCurrentUser();
    console.log('Current user:', user);
    
    if (!user) {
      console.warn('No user found, cannot load orders');
      this.loading = false;
      this.orders = [];
      this.filteredOrders = [];
      return;
    }

    let endpoint = 'orders';
    if (user.role === 'Client') {
      endpoint = 'orders/my-orders';
    } else if (user.role === 'Designer') {
      endpoint = 'orders/assigned-orders';
    }
    
    console.log('Loading orders from endpoint:', endpoint);
    console.log('User role:', user.role);

    this.apiService.get<any[]>(endpoint)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (orders) => {
          console.log('Raw orders from API:', orders);
          // Handle case where orders might be null or undefined
          if (!orders || !Array.isArray(orders)) {
            console.warn('Orders is not an array:', orders);
            this.orders = [];
            this.filteredOrders = [];
            this.loading = false;
            return;
          }
          
          // Transform backend response to frontend model
          this.orders = orders.map(order => {
            const transformed = this.transformOrder(order);
            console.log('Transformed order:', transformed);
            return transformed;
          });
          console.log('Final orders array:', this.orders);
          this.applyFilters();
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading orders:', error);
          console.error('Error details:', {
            status: error.status,
            statusText: error.statusText,
            message: error.message,
            error: error.error
          });
          
          let errorMessage = 'Failed to load orders';
          if (error.status === 403) {
            errorMessage = 'You do not have permission to view orders';
          } else if (error.status === 401) {
            errorMessage = 'Please log in to view orders';
          } else if (error.error?.error) {
            errorMessage = error.error.error;
          }
          
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: errorMessage
          });
          this.loading = false;
          this.orders = [];
          this.filteredOrders = [];
        }
      });
  }

  private transformOrder(backendOrder: any): Order {
    // Map backend field names to frontend model
    // Backend uses PascalCase (Status, Id, Deadline) and frontend uses camelCase
    const status = this.mapStatus(backendOrder.status || backendOrder.Status);
    
    return {
      id: backendOrder.id || backendOrder.Id,
      clientId: backendOrder.clientId || backendOrder.ClientId || '',
      designerId: backendOrder.designerId || backendOrder.DesignerId,
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
      requiresPriceApproval: backendOrder.requiresPriceApproval || backendOrder.RequiresPriceApproval || false,
      priceApproved: backendOrder.priceApproved || backendOrder.PriceApproved || false,
      instructions: backendOrder.instructions || backendOrder.Instructions,
      requiredFormats: backendOrder.requiredFormats || backendOrder.RequiredFormats,
      requirements: backendOrder.requirements || backendOrder.Requirements,
      colorPreferences: backendOrder.colorPreferences || backendOrder.ColorPreferences,
      stylePreferences: backendOrder.stylePreferences || backendOrder.StylePreferences,
      fileCount: backendOrder.fileCount || backendOrder.FileCount || 0,
      visibleFileCount: backendOrder.visibleFileCount || backendOrder.VisibleFileCount || 0,
      revisionCount: backendOrder.revisionCount || backendOrder.RevisionCount || 0,
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
      } : undefined
    };
  }

  private mapStatus(status: string): OrderStatus {
    // Map backend status strings to frontend enum
    if (!status) return OrderStatus.WaitingForAdminApproval;
    
    const statusStr = status.toString().trim();
    switch (statusStr) {
      case 'WaitingForAdminApproval':
      case '1':
        return OrderStatus.WaitingForAdminApproval;
      case 'PriceApprovalPending':
      case '2':
        return OrderStatus.PriceApprovalPending;
      case 'InProgress':
      case 'In Progress':
      case '3':
        return OrderStatus.InProgress;
      case 'PreviewDelivered':
      case '4':
        return OrderStatus.PreviewDelivered;
      case 'RevisionRequested':
      case '5':
        return OrderStatus.RevisionRequested;
      case 'FinalApproved':
      case '6':
        return OrderStatus.FinalApproved;
      case 'Completed':
      case '7':
        return OrderStatus.Completed;
      case 'Cancelled':
      case '8':
        return OrderStatus.Cancelled;
      default:
        return OrderStatus.WaitingForAdminApproval;
    }
  }

  applyFilters(): void {
    let filtered = [...this.orders];
    
    // Apply status filter
    if (this.selectedStatus) {
      filtered = filtered.filter(order => order.status === this.selectedStatus);
    }
    
    this.filteredOrders = filtered;
  }

  onStatusFilterChange(): void {
    this.applyFilters();
    this.first = 0; // Reset pagination
  }

  getStatusSeverity(status: OrderStatus): string {
    const severityMap: { [key: string]: string } = {
      'WaitingForAdminApproval': 'warning',
      'PriceApprovalPending': 'info',
      'InProgress': 'info',
      'PreviewDelivered': 'success',
      'RevisionRequested': 'warn',
      'FinalApproved': 'success',
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
    this.selectedOrderId = orderId;
    this.showOrderDetailModal = true;
  }

  onOrderDetailClose(): void {
    this.showOrderDetailModal = false;
    this.selectedOrderId = null;
  }

  onOrderUpdated(): void {
    this.loadOrders(); // Refresh the list when order is updated
  }

  canCreateOrder(): boolean {
    return this.authService.hasRole('Client');
  }

  // Order Create Modal
  showOrderCreateModal = false;

  createOrder(): void {
    this.showOrderCreateModal = true;
  }

  onOrderCreateClose(): void {
    this.showOrderCreateModal = false;
  }

  onOrderCreated(order: any): void {
    // Refresh orders list when order is created
    this.loadOrders();
    // Optionally open the created order in detail modal
    if (order && order.id) {
      this.selectedOrderId = order.id;
      this.showOrderDetailModal = true;
    }
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

  canChangeStatus(): boolean {
    return this.isAdmin || this.isSuperAdmin;
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
    // Find the status object that matches the order's current status
    const currentStatusObj = this.statuses.find(s => s.value === order.status);
    this.newStatus = currentStatusObj || this.statuses[0];
    
    // Debug: Log available statuses
    console.log('=== STATUS DROPDOWN DEBUG ===');
    console.log('Total statuses available:', this.statuses.length);
    console.log('All statuses:', this.statuses.map(s => ({ label: s.label, value: s.value })));
    console.log('Current order status:', order.status);
    console.log('Selected status:', this.newStatus);
    
    // Verify Completed and FinalApproved are in the list
    const completedStatus = this.statuses.find(s => s.value === 'Completed');
    const approvedStatus = this.statuses.find(s => s.value === 'FinalApproved');
    console.log('Completed status in list:', completedStatus);
    console.log('FinalApproved status in list:', approvedStatus);
    console.log('===========================');
    
    this.showStatusDialog = true;
  }

  changeStatus(): void {
    if (!this.selectedOrder || !this.newStatus) return;

    // Handle both object format {label, value} and direct enum value
    const statusValue = typeof this.newStatus === 'object' ? this.newStatus.value : this.newStatus;

    this.apiService.put(`orders/${this.selectedOrder.id}/status`, { status: statusValue })
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
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to update order status'
          });
        }
      });
  }

  openUploadDialog(order: Order): void {
    this.selectedOrder = order;
    this.selectedFiles = [];
    this.uploadSuccess = false;
    this.uploadedFilesCount = 0;
    this.showUploadDialog = true;
  }

  onFileSelect(event: any): void {
    // PrimeNG p-fileUpload onSelect event provides files in event.files
    const files: File[] = event.files ? Array.from(event.files) : [];
    
    if (files.length === 0) {
      return;
    }
    
    // Validate file sizes (max 10MB each)
    const maxSize = 10 * 1024 * 1024; // 10MB
    const invalidFiles = files.filter(file => file.size > maxSize);
    
    if (invalidFiles.length > 0) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: `${invalidFiles.length} file(s) exceed the 10MB limit and were not added`
      });
    }
    
    // Add valid files (avoid duplicates by checking file name and size)
    const validFiles = files.filter(file => {
      if (file.size > maxSize) {
        return false;
      }
      // Check if file already exists
      const exists = this.selectedFiles.some(
        existingFile => existingFile.name === file.name && existingFile.size === file.size
      );
      return !exists;
    });
    
    if (validFiles.length > 0) {
      this.selectedFiles = [...this.selectedFiles, ...validFiles];
      this.uploadSuccess = false; // Reset success state when new files are added
      
      // Clear the file upload component to allow selecting more files
      if (this.fileUploadComponent) {
        this.fileUploadComponent.clear();
      }
    }
  }

  removeFile(index: number): void {
    this.selectedFiles.splice(index, 1);
  }

  submitFiles(): void {
    if (this.selectedFiles.length === 0 || !this.selectedOrder) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select at least one file'
      });
      return;
    }

    const user = this.authService.getCurrentUser();
    // Show confirmation dialog for designers
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
    if (this.selectedFiles.length === 0 || !this.selectedOrder) {
      return;
    }

    this.uploadingFiles = true;
    const filesToUpload = [...this.selectedFiles];

    if (this.selectedFiles.length === 1) {
      // Single file upload
      const formData = new FormData();
      formData.append('file', this.selectedFiles[0]);
      formData.append('fileType', 'Reference');

      this.apiService.post(`files/upload/${this.selectedOrder.id}`, formData)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.uploadedFilesCount += filesToUpload.length;
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: `${filesToUpload.length} file(s) uploaded successfully. You can add more files or close the dialog.`
            });
            this.selectedFiles = [];
            this.uploadingFiles = false;
            this.uploadSuccess = true;
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: error.error?.error || 'Failed to upload file'
            });
            this.uploadingFiles = false;
          }
        });
    } else {
      // Multiple file upload
      const formData = new FormData();
      this.selectedFiles.forEach(file => {
        formData.append('files', file);
      });
      formData.append('fileType', 'Reference');

      this.apiService.post(`files/upload-multiple/${this.selectedOrder.id}`, formData)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (results: any) => {
            const uploadedCount = Array.isArray(results) ? results.length : 0;
            this.uploadedFilesCount += uploadedCount;
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: `${uploadedCount} file(s) uploaded successfully. You can add more files or close the dialog.`
            });
            this.selectedFiles = [];
            this.uploadingFiles = false;
            this.uploadSuccess = true;
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: error.error?.error || 'Failed to upload files'
            });
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
