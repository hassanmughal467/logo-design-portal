import { Component, OnInit, OnDestroy, OnChanges, SimpleChanges, Input, Output, EventEmitter } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { MessageService, ConfirmationService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Order, OrderStatus } from '@shared/models/order.model';
import { isOrderLocked } from '@shared/utils/order-locking';
import { LogoFile } from '@shared/models/file.model';
import { OrderRevision } from '@shared/models/revision.model';
import { OrderComment } from '@shared/models/comment.model';

@Component({
  selector: 'app-order-detail',
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.scss']
})
export class OrderDetailComponent implements OnInit, OnDestroy, OnChanges {
  @Input() orderId: string | null = null;
  @Input() visible: boolean = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() orderUpdated = new EventEmitter<void>();

  order: Order | null = null;
  files: LogoFile[] = [];
  revisions: OrderRevision[] = [];
  comments: OrderComment[] = [];
  loading = false;
  activeTab = 0;
  
  // Dialogs
  showPriceApprovalDialog = false;
  showApproveDialog = false;
  showAssignDialog = false;
  showSendFilesDialog = false;
  showRevisionDialog = false;
  showCommentDialog = false;
  showEditOrderDialog = false;
  showCancelOrderDialog = false;
  showArchiveConfirmDialog = false;
  showRefundDialog = false;
  
  // Cancel/Archive data
  cancellationReason = '';
  archiveNote = '';
  refundAmount = 0;
  refundReason = '';
  
  // Form data
  proposedPrice = 0;
  priceApprovalNotes = '';
  selectedDesignerId: string | null = null;
  availableDesigners: any[] = [];
  selectedFileIds: string[] = [];
  revisionInstructions = '';
  commentContent = '';
  isInternalComment = true;
  
  // Permissions
  isClient = false;
  isDesigner = false;
  isAdmin = false;
  isSuperAdmin = false;
  
  private destroy$ = new Subject<void>();
  private isRouteMode = false;

  constructor(
    public router: Router,
    private route: ActivatedRoute,
    private apiService: ApiService,
    private authService: AuthService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    this.isClient = user?.role === 'Client';
    this.isDesigner = user?.role === 'Designer';
    this.isAdmin = user?.role === 'Admin';
    this.isSuperAdmin = user?.role === 'SuperAdmin';

    // When used as route (/orders/:id), load order from route params
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
      const id = params.get('id');
      if (id && !this.orderId) {
        this.isRouteMode = true;
        this.visible = true;
        this.orderId = id;
        this.loadOrder(id);
      }
    });
  }

  ngOnChanges(): void {
    if (this.visible && this.orderId) {
      this.loadOrder(this.orderId);
    } else if (!this.visible) {
      // Reset when modal closes
      this.order = null;
      this.files = [];
      this.revisions = [];
      this.comments = [];
      this.activeTab = 0;
    }
  }

  closeModal(): void {
    if (this.isRouteMode) {
      this.router.navigate(['/orders']);
    } else {
      this.visible = false;
      this.visibleChange.emit(false);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadOrder(orderId: string): void {
    this.loading = true;
    this.apiService.get<Order>(`orders/${orderId}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (order) => {
          this.order = order;
          this.loadFiles(orderId);
          this.loadRevisions(orderId);
          this.loadComments(orderId);
          this.loading = false;
        },
        error: (error) => {
          // Handle 403 Forbidden (authorization failure) vs 401 Unauthorized (authentication failure)
          if (error.status === 403) {
            this.messageService.add({
              severity: 'warn',
              summary: 'Access Denied',
              detail: error.error?.error || 'You do not have access to view this order.'
            });
          } else if (error.status === 404) {
            this.messageService.add({
              severity: 'error',
              summary: 'Not Found',
              detail: 'Order not found.'
            });
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: error.error?.error || 'Failed to load order'
            });
          }
          this.loading = false;
          if (this.isRouteMode) {
            this.router.navigate(['/orders']);
          } else {
            this.closeModal();
          }
        }
      });
  }

  loadFiles(orderId: string): void {
    this.apiService.get<LogoFile[]>(`files/order/${orderId}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (files) => {
          this.files = files;
        },
        error: () => {
          this.files = [];
        }
      });
  }

  loadRevisions(orderId: string): void {
    // Only load revisions for Admin, SuperAdmin, and Designer
    // Clients don't need to see revision details
    const user = this.authService.getCurrentUser();
    if (user?.role === 'Client') {
      this.revisions = [];
      return;
    }

    // Get the latest revision (Admin/Designer can see it)
    this.apiService.get<OrderRevision>(`revisions/orders/${orderId}/latest`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (revision) => {
          // Convert single revision to array for display
          this.revisions = revision ? [revision] : [];
        },
        error: (error) => {
          // 404 is expected when there are no revisions, so handle gracefully
          if (error.status === 404) {
            this.revisions = [];
          } else {
            this.revisions = [];
            console.warn('Error loading revisions:', error);
          }
        }
      });
  }

  loadComments(orderId: string): void {
    this.apiService.get<OrderComment[]>(`comments/orders/${orderId}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (comments) => {
          this.comments = comments;
        },
        error: () => {
          this.comments = [];
        }
      });
  }

  // Price Approval
  openPriceApprovalDialog(): void {
    this.proposedPrice = this.order?.price || 0;
    this.priceApprovalNotes = '';
    this.showPriceApprovalDialog = true;
  }

  requestPriceApproval(): void {
    if (!this.order || this.proposedPrice <= 0) return;

    this.apiService.post(`orders/${this.order.id}/request-price-approval`, {
      proposedPrice: this.proposedPrice,
      notes: this.priceApprovalNotes
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Price approval requested'
        });
        this.showPriceApprovalDialog = false;
        this.loadOrder(this.order!.id);
        this.orderUpdated.emit();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.error || 'Failed to request price approval'
        });
      }
    });
  }

  approvePrice(approved: boolean): void {
    if (!this.order) return;

    this.apiService.post(`orders/${this.order.id}/approve-price`, {
      approved,
      comment: this.priceApprovalNotes
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: approved ? 'Price approved' : 'Price rejected'
        });
        this.showPriceApprovalDialog = false;
        this.loadOrder(this.order!.id);
        this.orderUpdated.emit();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.error || 'Failed to process price approval'
        });
      }
    });
  }

  // Order Approval
  approveOrder(): void {
    if (!this.order) return;

    this.apiService.post(`orders/${this.order.id}/approve`, {})
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Order approved successfully'
          });
          this.loadOrder(this.order!.id);
          this.orderUpdated.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to approve order'
          });
        }
      });
  }

  // Designer Assignment
  openAssignDialog(): void {
    this.selectedDesignerId = this.order?.designerId || null;
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
            .map(u => {
              const fullName = [u.firstName, u.lastName].filter(Boolean).join(' ').trim();
              return { label: fullName || u.email || 'Unknown Designer', value: u.id };
            });
        },
        error: () => {
          this.availableDesigners = [];
        }
      });
  }

  assignDesigner(): void {
    if (!this.order || !this.selectedDesignerId) return;

    this.apiService.post(`orders/${this.order.id}/assign`, {
      designerId: this.selectedDesignerId
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Designer assigned successfully'
        });
        this.showAssignDialog = false;
        this.loadOrder(this.order!.id);
        this.orderUpdated.emit();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.error || 'Failed to assign designer'
        });
      }
    });
  }

  // Send Files to Client
  openSendFilesDialog(): void {
    this.selectedFileIds = [];
    this.showSendFilesDialog = true;
  }

  selectAllFilesToSend(): void {
    this.selectedFileIds = this.files.map(f => f.id);
  }

  deselectAllFilesToSend(): void {
    this.selectedFileIds = [];
  }

  sendFilesToClient(): void {
    if (!this.order || this.selectedFileIds.length === 0) return;

    this.apiService.post(`orders/${this.order.id}/send-files-to-client`, this.selectedFileIds)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Files sent to client successfully'
          });
        this.showSendFilesDialog = false;
        this.loadOrder(this.order!.id);
        this.loadFiles(this.order!.id);
        this.orderUpdated.emit();
      },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to send files'
          });
        }
      });
  }

  // Revision
  revisionFiles: File[] = [];
  
  openRevisionDialog(): void {
    this.revisionInstructions = '';
    this.revisionFiles = [];
    this.showRevisionDialog = true;
  }

  onRevisionFileSelect(event: any): void {
    const files: File[] = event.files ? Array.from(event.files) : [];
    if (files.length > 0) {
      this.revisionFiles = [...this.revisionFiles, ...files];
    }
  }

  removeRevisionFile(index: number): void {
    this.revisionFiles.splice(index, 1);
  }

  createRevision(): void {
    if (!this.order || !this.revisionInstructions.trim()) return;

    const formData = new FormData();
    formData.append('instructions', this.revisionInstructions);
    
    // Add files if any
    if (this.revisionFiles.length > 0) {
      this.revisionFiles.forEach((file, index) => {
        formData.append(`files`, file);
      });
    }

    this.apiService.post(`revisions/orders/${this.order.id}/request`, formData)
      .pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Revision requested successfully. Previous preview files have been deleted.'
          });
          this.showRevisionDialog = false;
          this.revisionInstructions = '';
          this.revisionFiles = [];
          this.loadOrder(this.order!.id);
          this.orderUpdated.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to create revision'
          });
        }
      });
  }

  approveLogo(): void {
    if (!this.order) return;

    this.confirmationService.confirm({
      message: 'Once approved, your logo will be marked as completed and ready for invoice.<br>All preview and revision files will be permanently deleted, and only final approved files will be saved. Are you sure you want to proceed?',
      header: 'Approve Logo',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Yes, Approve',
      rejectLabel: 'Cancel',
      acceptButtonStyleClass: 'p-button-primary',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this.apiService.post(`revisions/orders/${this.order!.id}/approve-logo`, {})
          .pipe(takeUntil(this.destroy$)).subscribe({
            next: () => {
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Logo approved successfully. Order is now completed.'
              });
              this.loadOrder(this.order!.id);
              this.orderUpdated.emit();
            },
            error: (error) => {
              this.messageService.add({
                severity: 'error',
                summary: 'Error',
                detail: error.error?.error || 'Failed to approve logo'
              });
            }
          });
      },
      reject: () => {}
    });
  }

  // Comments
  openCommentDialog(): void {
    this.commentContent = '';
    this.isInternalComment = !this.isClient;
    this.showCommentDialog = true;
  }

  createComment(): void {
    if (!this.order || !this.commentContent.trim()) return;

    this.apiService.post(`comments/orders/${this.order.id}`, {
      content: this.commentContent,
      isInternal: this.isInternalComment
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Comment added successfully'
        });
        this.showCommentDialog = false;
        this.loadComments(this.order!.id);
        this.orderUpdated.emit();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.error || 'Failed to add comment'
        });
      }
    });
  }

  // File download
  downloadFile(file: LogoFile): void {
    this.apiService.getBlob(`files/${file.id}/download`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
      next: (blob: Blob) => {
        // Create a blob URL and trigger download
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = file.originalFileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        // Clean up the blob URL
        window.URL.revokeObjectURL(url);
        
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'File downloaded successfully'
        });
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.error || 'Failed to download file'
        });
      }
    });
  }

  getStatusSeverity(status: string): string {
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

  canApproveOrder(): boolean {
    return (this.isAdmin || this.isSuperAdmin) && this.order?.status === OrderStatus.WaitingForAdminApproval;
  }

  canRequestPriceApproval(): boolean {
    return (this.isAdmin || this.isSuperAdmin) && this.order?.status !== OrderStatus.Completed && this.order?.status !== OrderStatus.Cancelled;
  }

  canApprovePrice(): boolean {
    return this.isClient && this.order?.status === OrderStatus.PriceApprovalPending;
  }

  canAssignDesigner(): boolean {
    return (this.isAdmin || this.isSuperAdmin) && !this.order?.designerId;
  }

  canRequestRevision(): boolean {
    // Only allow revision if:
    // 1. User is a client
    // 2. Order status is PreviewDelivered (strict requirement)
    if (!this.isClient || !this.order) {
      return false;
    }

    // Strict rule: Only allow when status is PreviewDelivered
    return this.order.status === OrderStatus.PreviewDelivered;
  }

  canApproveLogo(): boolean {
    // Allow approval if:
    // 1. User is a client, admin, or superadmin
    // 2. Order status is PreviewDelivered
    if (!this.order) {
      return false;
    }

    // Check status - use string comparison to be safe
    const status = this.order.status?.toString() || this.order.status;
    if (status !== OrderStatus.PreviewDelivered && status !== 'PreviewDelivered') {
      return false;
    }

    // Client can approve their own orders
    if (this.isClient) {
      return true;
    }

    // Admin and SuperAdmin can also approve
    if (this.isAdmin || this.isSuperAdmin) {
      return true;
    }

    return false;
  }

  shouldShowOrderDetails(): boolean {
    // Hide order details from client when revision is requested
    if (this.isClient && this.order?.status === OrderStatus.RevisionRequested) {
      return false;
    }
    return true;
  }

  /** Admin: Designer uploaded preview files not yet sent to client */
  hasPendingPreviewFiles(): boolean {
    return (this.isAdmin || this.isSuperAdmin) && this.files.some(f => !f.isVisibleToClient);
  }

  canSendFiles(): boolean {
    return (this.isAdmin || this.isSuperAdmin) && this.files.some(f => !f.isVisibleToClient && f.isAdminApproved);
  }

  canEditOrder(): boolean {
    return !!(this.isClient && this.order && 
           (this.order.status === OrderStatus.WaitingForAdminApproval || 
            this.order.status === OrderStatus.PriceApprovalPending));
  }

  canCancelOrder(): boolean {
    if (!this.order) return false;
    
    // Check if already cancelled or refunded
    if (this.order.status === OrderStatus.CancelledByUser || 
        this.order.status === OrderStatus.CancelledByAdmin ||
        this.order.status === OrderStatus.Refunded) {
      return false;
    }
    
    // Admin and SuperAdmin can cancel any order at any stage
    if (this.isAdmin || this.isSuperAdmin) {
      return true;
    }
    
    // Clients can only cancel if status is Pending or Paid (or WaitingForAdminApproval/PriceApprovalPending for backward compatibility)
    if (this.isClient) {
      return this.order.status === OrderStatus.Pending || 
             this.order.status === OrderStatus.Paid ||
             this.order.status === OrderStatus.WaitingForAdminApproval || 
             this.order.status === OrderStatus.PriceApprovalPending;
    }
    
    return false;
  }

  canArchiveOrder(): boolean {
    // Only Admin and SuperAdmin can archive
    return (this.isAdmin || this.isSuperAdmin) && 
           this.order !== null && 
           !this.order.isArchived;
  }

  canUnarchiveOrder(): boolean {
    // Only Admin and SuperAdmin can unarchive
    return (this.isAdmin || this.isSuperAdmin) && 
           this.order !== null && 
           this.order.isArchived === true;
  }

  canRefundOrder(): boolean {
    // Only Admin and SuperAdmin can refund, and order must not be already refunded
    return (this.isAdmin || this.isSuperAdmin) && 
           this.order !== null && 
           !this.order.isRefunded &&
           (this.order.status === OrderStatus.Completed || 
            this.order.status === OrderStatus.Paid);
  }

  approveFile(file: LogoFile): void {
    this.apiService.put(`files/${file.id}/approve`, {
      approved: true,
      makeVisibleToClient: true
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'File approved successfully'
        });
        this.loadFiles(this.order!.id);
        this.orderUpdated.emit();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.error || 'Failed to approve file'
        });
      }
    });
  }

  // Edit Order
  openEditOrderDialog(): void {
    this.showEditOrderDialog = true;
  }

  onOrderUpdated(updatedOrder: Order): void {
    this.order = updatedOrder;
    this.loadOrder(this.order.id);
    this.showEditOrderDialog = false;
    this.orderUpdated.emit();
  }

  // Cancel Order
  openCancelOrderDialog(): void {
    this.cancellationReason = '';
    this.showCancelOrderDialog = true;
  }

  confirmCancelOrder(): void {
    if (!this.order || !this.cancellationReason.trim()) return;

    this.apiService.post(`orders/${this.order.id}/cancel`, {
      reason: this.cancellationReason
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Order cancelled successfully'
          });
          this.showCancelOrderDialog = false;
          this.loadOrder(this.order!.id);
          this.orderUpdated.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to cancel order'
          });
        }
      });
  }

  // Archive Order
  openArchiveConfirmDialog(): void {
    this.archiveNote = '';
    this.showArchiveConfirmDialog = true;
  }

  confirmArchiveOrder(): void {
    if (!this.order) return;

    this.apiService.post(`orders/${this.order.id}/archive`, {
      note: this.archiveNote || undefined
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Order archived successfully'
          });
          this.showArchiveConfirmDialog = false;
          this.loadOrder(this.order!.id);
          this.orderUpdated.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to archive order'
          });
        }
      });
  }

  unarchiveOrder(): void {
    if (!this.order) return;

    this.apiService.post(`orders/${this.order.id}/unarchive`, {})
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Order unarchived successfully'
          });
          this.loadOrder(this.order!.id);
          this.orderUpdated.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to unarchive order'
          });
        }
      });
  }

  // Refund Order
  openRefundDialog(): void {
    this.refundAmount = this.order?.price || 0;
    this.refundReason = '';
    this.showRefundDialog = true;
  }

  confirmRefundOrder(): void {
    if (!this.order || !this.refundReason.trim() || this.refundAmount <= 0) return;

    if (this.refundAmount > this.order.price) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Refund amount cannot exceed order price'
      });
      return;
    }

    this.apiService.post(`orders/${this.order.id}/refund`, {
      amount: this.refundAmount,
      reason: this.refundReason
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Order refunded successfully'
          });
          this.showRefundDialog = false;
          this.loadOrder(this.order!.id);
          this.orderUpdated.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to refund order'
          });
        }
      });
  }

  isOrderLocked = isOrderLocked;

  canUploadFiles(): boolean {
    if (!this.order) return false;
    
    // Check if order is completed or final approved
    const isCompleted = this.order.status === OrderStatus.Completed || 
                       this.order.status === OrderStatus.FinalApproved;
    
    // If completed, uploads are disabled by default (unless admin explicitly enabled them)
    if (isCompleted) {
      // Only allow if admin has explicitly enabled uploads
      return (this.isAdmin || this.isSuperAdmin) && this.order.allowUploads === true;
    }
    
    // For non-completed orders, check role and allowUploads flag
    if (this.isClient) {
      return this.order.allowUploads !== false;
    }
    
    // Designers and admins can upload if uploads are allowed
    return (this.isDesigner || this.isAdmin || this.isSuperAdmin) && 
           (this.order.allowUploads !== false);
  }

  canToggleUploads(): boolean {
    // Only admins can toggle uploads, and only for completed/final approved orders
    if (!this.order || (!this.isAdmin && !this.isSuperAdmin)) {
      return false;
    }
    
    const isCompleted = this.order.status === OrderStatus.Completed || 
                        this.order.status === OrderStatus.FinalApproved;
    return isCompleted;
  }

  toggleUploads(): void {
    if (!this.order) return;

    const newValue = !this.order.allowUploads;
    this.apiService.put<Order>(`orders/${this.order.id}/allow-uploads`, {
      allowUploads: newValue
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (updatedOrder) => {
          this.order = updatedOrder;
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: `File uploads ${newValue ? 'enabled' : 'disabled'} successfully`
          });
          this.orderUpdated.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to update upload settings'
          });
        }
      });
  }

}
