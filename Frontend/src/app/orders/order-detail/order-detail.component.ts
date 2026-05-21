import { Component, OnInit, OnDestroy, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectorRef } from '@angular/core';
import { FileUpload } from 'primeng/fileupload';
import { Router, ActivatedRoute } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { PermissionsService } from '@core/services/permissions.service';
import { MessageService, ConfirmationService } from 'primeng/api';
import { Subject, Subscription } from 'rxjs';
import { takeUntil, finalize, timeout } from 'rxjs/operators';
import { Order, OrderStatus } from '@shared/models/order.model';
import { isOrderLocked } from '@shared/utils/order-locking';
import { LogoFile, FileType } from '@shared/models/file.model';
import { OrderRevision, RevisionAttachment } from '@shared/models/revision.model';
import { OrderComment, OrderCommentUnreadCounts } from '@shared/models/comment.model';
import { MAX_UPLOAD_BYTES, combinedFileBytes } from '@core/constants/upload-limits';

@Component({
  selector: 'app-order-detail',
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.scss']
})
export class OrderDetailComponent implements OnInit, OnDestroy, OnChanges {
  @ViewChild('revisionFileUpload') private revisionFileUpload?: FileUpload;
  readonly maxUploadFileSize = MAX_UPLOAD_BYTES;
  private readonly designerClientAliasName = 'Hawk Merchandising';
  private readonly designerClientAliasRole = 'SuperAdmin';
  @Input() orderId: string | null = null;
  @Input() visible: boolean = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() orderUpdated = new EventEmitter<void>();

  /**
   * Bound to p-dialog only. Parent passes {@link visible} via @Input; mixing [(visible)] on the
   * dialog with the same field as @Input causes PrimeNG and Angular to fight and can strand loading.
   */
  dialogVisible = false;

  order: Order | null = null;
  files: LogoFile[] = [];
  revisions: OrderRevision[] = [];
  comments: OrderComment[] = [];
  unreadCounts: OrderCommentUnreadCounts = { unreadFiles: 0, unreadRevisions: 0, unreadComments: 0, unreadPriceNegotiationNotes: 0 };
  loading = false;
  loadFailed = false;
  errorMessage = '';
  activeTab = 0;
  private commentsLoadedForOrderId: string | null = null;
  private revisionsLoadedForOrderId: string | null = null;
  
  // Dialogs
  showPriceApprovalDialog = false;
  showDesignerPriceApprovalDialog = false;
  showApproveDialog = false;
  showAssignDialog = false;
  showSendFilesDialog = false;
  showRevisionDialog = false;
  showCommentDialog = false;
  showEditOrderDialog = false;
  showCancelOrderDialog = false;
  showArchiveConfirmDialog = false;
  showRefundDialog = false;
  showEditClientPriceDialog = false;
  showRequestPriceDialog = false;
  showImagePreviewDialog = false;
  /** 'order' = logo order files; 'revision' = client revision attachments */
  imagePreviewKind: 'order' | 'revision' = 'order';
  imagePreviewFiles: LogoFile[] = [];
  revisionImagePreviewFiles: RevisionAttachment[] = [];
  imagePreviewIndex = 0;
  imagePreviewUrl: string | null = null;
  imagePreviewLoading = false;
  
  // Edit client price
  editClientChargePrice = 0;
  designerRequestedPrice = 0;
  /** Optional note with designer's price request (saved to payout negotiation history). */
  designerRequestPriceMessage = '';
  
  // Cancel/Archive data
  cancellationReason = '';
  archiveNote = '';
  refundAmount = 0;
  refundReason = '';
  
  // Form data
  proposedPrice = 0;
  priceApprovalNotes = '';
  clientPriceApprovalAction: 'Approve' | 'Modify' | 'Reject' = 'Approve';
  clientCounterPrice = 0;
  clientApprovedPrice = 0;
  designerPriceApprovalAction: 'Approve' | 'Modify' | 'Reject' = 'Approve';
  designerApprovedPrice = 0;
  /** Optional note from admin on designer price approval (saved to payout negotiation history). */
  designerPriceApprovalMessage = '';
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
  private loadOrderSubscription: Subscription | null = null;

  constructor(
    public router: Router,
    private route: ActivatedRoute,
    private apiService: ApiService,
    private authService: AuthService,
    private permissionsService: PermissionsService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private cdr: ChangeDetectorRef
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
        this.dialogVisible = true;
        this.orderId = id;
        this.loadOrder(id);
      }
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible']) {
      this.dialogVisible = this.visible;
      if (!this.visible) {
        this.resetModalState();
        return;
      }
    }

    const id = this.orderId?.trim();
    if (!this.visible || !id) {
      return;
    }

    const visibleOpened =
      changes['visible'] &&
      changes['visible'].currentValue === true &&
      changes['visible'].previousValue !== true;
    const orderIdChanged =
      changes['orderId'] &&
      changes['orderId'].currentValue !== changes['orderId'].previousValue;

    if (visibleOpened || orderIdChanged) {
      this.loadOrder(id);
    }
  }

  /** PrimeNG dialog close (X, overlay) — keep in sync with parent without duplicate onHide + two-way fighting. */
  onPDialogVisibleChange(show: boolean): void {
    if (this.dialogVisible === show) {
      return;
    }
    this.dialogVisible = show;
    if (!show) {
      if (this.isRouteMode) {
        this.router.navigate(['/orders']);
      } else if (this.visible) {
        this.visibleChange.emit(false);
      }
      this.resetModalState();
    }
  }

  private resetModalState(): void {
    this.loadOrderSubscription?.unsubscribe();
    this.loadOrderSubscription = null;
    this.order = null;
    this.files = [];
    this.revisions = [];
    this.comments = [];
    this.unreadCounts = { unreadFiles: 0, unreadRevisions: 0, unreadComments: 0, unreadPriceNegotiationNotes: 0 };
    this.activeTab = 0;
    this.loadFailed = false;
    this.errorMessage = '';
    this.loading = false;
    this.commentsLoadedForOrderId = null;
    this.revisionsLoadedForOrderId = null;
  }

  closeModal(): void {
    if (this.isRouteMode) {
      this.router.navigate(['/orders']);
      return;
    }
    this.dialogVisible = false;
    this.visibleChange.emit(false);
    this.resetModalState();
  }

  ngOnDestroy(): void {
    this.cleanupImagePreviewUrl();
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadOrder(orderId: string): void {
    this.loadOrderSubscription?.unsubscribe();
    this.loading = true;
    this.loadFailed = false;
    this.errorMessage = '';
    this.loadOrderSubscription = this.apiService
      .get<Order>(`orders/${orderId}`)
      .pipe(
        timeout(60000),
        takeUntil(this.destroy$),
        finalize(() => {
          this.loading = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: (order) => {
          this.order = order;
          this.loadFiles(orderId);
          this.loadUnreadCounts(orderId);
          this.loadTabDataForCurrentTab();
        },
        error: (error) => {
          this.loadFailed = true;
          const isTimeout = error?.name === 'TimeoutError';
          this.errorMessage = isTimeout
            ? 'Request timed out. Check your connection and try again.'
            : error.error?.error ||
              (error.status === 403
                ? 'You do not have access to view this order.'
                : error.status === 404
                  ? 'Order not found.'
                  : 'Failed to load order.');

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
              detail: isTimeout ? this.errorMessage : error.error?.error || 'Failed to load order'
            });
          }
          if (this.isRouteMode) {
            this.router.navigate(['/orders']);
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
    // Get the latest revision (Admin/Designer can see it)
    this.apiService.get<OrderRevision>(`revisions/orders/${orderId}/latest`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (revision) => {
          const normalized = this.normalizeRevisionPayload(revision as unknown as Record<string, unknown>);
          this.revisions = normalized ? [normalized] : [];
        },
        error: (error) => {
          this.revisionsLoadedForOrderId = null;
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

  private normalizeRevisionPayload(raw: Record<string, unknown> | null | undefined): OrderRevision | null {
    if (!raw) return null;
    const filesRaw = (raw['files'] ?? raw['Files']) as unknown;
    const filesList = Array.isArray(filesRaw) ? filesRaw : [];
    const files: RevisionAttachment[] = filesList.map((f: Record<string, unknown>) => ({
      id: String(f['id'] ?? f['Id'] ?? ''),
      fileName: String(f['fileName'] ?? f['FileName'] ?? ''),
      originalFileName: String(f['originalFileName'] ?? f['OriginalFileName'] ?? ''),
      fileSize: Number(f['fileSize'] ?? f['FileSize'] ?? 0),
      contentType: String(f['contentType'] ?? f['ContentType'] ?? ''),
      createdAt: new Date((f['createdAt'] ?? f['CreatedAt']) as string | number | Date)
    }));
    return {
      id: String(raw['id'] ?? raw['Id'] ?? ''),
      orderId: String(raw['orderId'] ?? raw['OrderId'] ?? ''),
      instructions: String(raw['instructions'] ?? raw['Instructions'] ?? ''),
      requestedBy: String(raw['requestedBy'] ?? raw['RequestedBy'] ?? ''),
      requestedByName: String(raw['requestedByName'] ?? raw['RequestedByName'] ?? ''),
      isResolved: Boolean(raw['isResolved'] ?? raw['IsResolved']),
      resolvedAt: (raw['resolvedAt'] ?? raw['ResolvedAt'])
        ? new Date(String(raw['resolvedAt'] ?? raw['ResolvedAt']))
        : undefined,
      createdAt: new Date((raw['createdAt'] ?? raw['CreatedAt']) as string | number | Date),
      fileCount: raw['fileCount'] != null ? Number(raw['fileCount']) : (raw['FileCount'] != null ? Number(raw['FileCount']) : undefined),
      files
    };
  }

  loadComments(orderId: string): void {
    this.apiService.get<OrderComment[]>(`comments/orders/${orderId}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (comments) => {
          this.comments = comments;
          this.commentsLoadedForOrderId = orderId;
        },
        error: () => {
          this.comments = [];
          this.commentsLoadedForOrderId = null;
        }
      });
  }

  loadUnreadCounts(orderId: string): void {
    this.apiService.get<OrderCommentUnreadCounts>(`comments/orders/${orderId}/unread-counts`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (counts) => {
          this.unreadCounts = {
            ...counts,
            unreadPriceNegotiationNotes: counts.unreadPriceNegotiationNotes ?? 0
          };
        },
        error: () => {
          this.unreadCounts = { unreadFiles: 0, unreadRevisions: 0, unreadComments: 0, unreadPriceNegotiationNotes: 0 };
        }
      });
  }

  onTabChange(index: number): void {
    this.activeTab = index;
    this.loadTabDataForCurrentTab();
    if ((index === 2 || index === 3) && this.order?.id) {
      this.apiService.post(`comments/orders/${this.order.id}/mark-read`, {})
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => this.loadUnreadCounts(this.order!.id),
          error: () => {}
        });
    }
  }

  private loadTabDataForCurrentTab(): void {
    const orderId = this.order?.id;
    if (!orderId) return;

    // Files tab is index 0 and is already loaded with order.
    if (this.activeTab === 1) {
      this.loadRevisionsIfNeeded(orderId);
      return;
    }

    // Comments tab (2) + Price Negotiating Notes tab (3) both depend on comments.
    if (this.activeTab === 2 || this.activeTab === 3) {
      this.loadCommentsIfNeeded(orderId);
    }
  }

  private loadRevisionsIfNeeded(orderId: string): void {
    if (this.revisionsLoadedForOrderId === orderId) return;
    this.revisionsLoadedForOrderId = orderId;
    this.loadRevisions(orderId);
  }

  private loadCommentsIfNeeded(orderId: string): void {
    if (this.commentsLoadedForOrderId === orderId) return;
    this.commentsLoadedForOrderId = orderId;
    this.loadComments(orderId);
  }

  /** Order comments only (excludes price negotiation threads). */
  get generalComments(): OrderComment[] {
    return this.comments.filter(
      c => c.commentType !== 'PriceNegotiationClient' && c.commentType !== 'PriceNegotiationDesigner'
    );
  }

  get priceNegotiationClientNotes(): OrderComment[] {
    return this.comments.filter(c => c.commentType === 'PriceNegotiationClient');
  }

  get priceNegotiationDesignerNotes(): OrderComment[] {
    return this.comments.filter(c => c.commentType === 'PriceNegotiationDesigner');
  }

  get showPriceNegotiationTab(): boolean {
    return this.isClient || this.isDesigner || this.isAdmin || this.isSuperAdmin;
  }

  approveCommentForClient(comment: OrderComment): void {
    if (!this.order) return;
    this.apiService.put(`comments/${comment.id}/visibility`, { visibleToClient: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Comment approved for client' });
          this.loadComments(this.order!.id);
          this.loadUnreadCounts(this.order!.id);
        },
        error: (err) => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Failed to approve comment' });
        }
      });
  }

  /**
   * Client charge being discussed in Request price approval (admin → client). Not designer payout (PKR).
   */
  get clientPriceApprovalProposedAmount(): number {
    if (!this.order) return 0;
    return (
      this.order.clientPrice ??
      this.order.clientChargePrice ??
      this.order.clientBasePrice ??
      this.order.price ??
      0
    );
  }

  /**
   * True when the client accepted the proposed client charge (separate from designer PKR approval).
   * Backend sets priceUpdatedByRole to Client in ApprovePrice / respond-price-approval approve.
   */
  get clientAcceptedClientCharge(): boolean {
    return this.order?.priceUpdatedByRole === 'Client';
  }

  /** Highlight client charge row when the client has formally approved their price. */
  get clientChargeRowApproved(): boolean {
    return this.clientAcceptedClientCharge || (!!this.order?.priceApproved && this.isClient);
  }

  // Price Approval
  openPriceApprovalDialog(): void {
    // Default admin request to current client charge / base — not designer proposedPrice (legacy PKR field).
    this.proposedPrice = this.clientPriceApprovalProposedAmount;
    if (this.proposedPrice <= 0) {
      this.proposedPrice = this.order?.price ?? 0;
    }
    // Client should see admin note separately; this textarea is for the client's response.
    this.priceApprovalNotes = '';
    this.clientPriceApprovalAction = 'Approve';
    this.clientCounterPrice = this.clientPriceApprovalProposedAmount;
    this.clientApprovedPrice = this.clientCounterPrice;
    this.showPriceApprovalDialog = true;
  }

  onClientPriceApprovalActionChange(): void {
    // Keep approved price synced with the latest client-facing proposal (not designer proposed).
    if (this.clientPriceApprovalAction === 'Approve') {
      this.clientApprovedPrice = this.clientPriceApprovalProposedAmount;
      this.clientCounterPrice = this.clientApprovedPrice;
    } else if (this.clientPriceApprovalAction === 'Modify') {
      if (!this.clientCounterPrice || this.clientCounterPrice <= 0) {
        this.clientCounterPrice = this.clientPriceApprovalProposedAmount;
      }
      this.clientApprovedPrice = this.clientCounterPrice;
    } else {
      // Reject: nothing to approve
      this.clientApprovedPrice = 0;
    }
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

  submitClientPriceApproval(): void {
    if (!this.order) return;
    if ((this.clientPriceApprovalAction === 'Approve' || this.clientPriceApprovalAction === 'Modify')
      && (!this.clientApprovedPrice || this.clientApprovedPrice <= 0)) {
      this.messageService.add({ severity: 'warn', summary: 'Required', detail: 'Approved price is required.' });
      return;
    }

    this.apiService.post(`orders/${this.order.id}/respond-price-approval`, {
      action: this.clientPriceApprovalAction,
      counterPrice: this.clientPriceApprovalAction === 'Modify' ? this.clientApprovedPrice : null,
      message: this.priceApprovalNotes
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Response submitted'
        });
        this.showPriceApprovalDialog = false;
        this.loadOrder(this.order!.id);
        this.orderUpdated.emit();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.error || 'Failed to submit response'
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
          this.closeModal();
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
    // Use designer profiles so selectedDesignerId matches order.designerId (DesignerProfile.Id)
    this.apiService.get<any>('users/designer-profiles')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          const profiles = Array.isArray(response) ? response : ApiService.extractItems<any>(response);
          this.availableDesigners = (profiles || []).map((p: any) => {
            const fullName = [p.userFirstName, p.userLastName].filter(Boolean).join(' ').trim();
            return {
              label: fullName || p.userEmail || 'Unknown Designer',
              value: p.id, // DesignerProfile.Id (matches order.designerId)
              userId: p.userId // needed for assign API
            };
          });
        },
        error: () => {
          this.availableDesigners = [];
        }
      });
  }

  assignDesigner(): void {
    if (!this.order || !this.selectedDesignerId) return;

    const selected = this.availableDesigners.find(d => d.value === this.selectedDesignerId);
    const designerUserId = selected?.userId;
    if (!designerUserId) {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Selected designer is invalid.' });
      return;
    }

    this.apiService.post(`orders/${this.order.id}/assign`, {
      designerId: designerUserId
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
        this.closeModal();
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

  /** Files eligible for forwarding to client (excludes client-uploaded files) */
  get filesToForward(): LogoFile[] {
    return this.files.filter(f => f.uploadedByRole !== 'Client' && f.fileType !== FileType.Reference);
  }

  /** Client-facing preview files only. */
  get clientPreviewFiles(): LogoFile[] {
    return this.files.filter(f => f.isVisibleToClient && f.fileType === FileType.Preview);
  }

  /** Client-facing final files only. */
  get clientFinalFiles(): LogoFile[] {
    return this.files.filter(f => f.isVisibleToClient && f.fileType === FileType.Final);
  }

  /** Client-facing reference files only. */
  get clientReferenceFiles(): LogoFile[] {
    return this.files.filter(f => f.isVisibleToClient && f.fileType === FileType.Reference);
  }

  /**
   * Super Admin quick action: jump to Files and open the first pending designer preview image when possible.
   * Tab/index updates run through onTabChange + CD so PrimeNG TabView updates inside the order modal stack.
   */
  openPreviewTab(): void {
    this.activateTab(0);
    const pendingPreviewImages = this.files.filter(
      f => !f.isVisibleToClient && f.fileType === FileType.Preview && this.isPreviewableImage(f)
    );
    if (pendingPreviewImages.length > 0) {
      this.openImagePreview(pendingPreviewImages[0], pendingPreviewImages);
      return;
    }
    const pendingAnyImages = this.files.filter(f => !f.isVisibleToClient && this.isPreviewableImage(f));
    if (pendingAnyImages.length > 0) {
      this.openImagePreview(pendingAnyImages[0], pendingAnyImages);
      return;
    }
    this.messageService.add({
      severity: 'info',
      summary: 'Preview',
      detail: 'No image file to preview here. Open the Files tab to download (e.g. PDF or embroidery formats).'
    });
  }

  private cleanupImagePreviewUrl(): void {
    if (this.imagePreviewUrl) {
      window.URL.revokeObjectURL(this.imagePreviewUrl);
      this.imagePreviewUrl = null;
    }
  }

  private hasImageExtension(fileName: string): boolean {
    return /\.(png|jpe?g|gif|webp|bmp|svg)$/i.test(fileName);
  }

  isPreviewableImage(file: LogoFile): boolean {
    if (!file) return false;
    return (file.contentType || '').toLowerCase().startsWith('image/') || this.hasImageExtension(file.originalFileName || file.fileName || '');
  }

  isRevisionAttachmentPreviewable(file: RevisionAttachment): boolean {
    if (!file) return false;
    return (file.contentType || '').toLowerCase().startsWith('image/') || this.hasImageExtension(file.originalFileName || file.fileName || '');
  }

  private getPreviewableImageFiles(baseFiles?: LogoFile[]): LogoFile[] {
    const source = baseFiles ?? this.files;
    return source.filter(file => this.isPreviewableImage(file));
  }

  openImagePreview(file: LogoFile, baseFiles?: LogoFile[]): void {
    if (!this.order || !this.isPreviewableImage(file)) {
      return;
    }

    const imageFiles = this.getPreviewableImageFiles(baseFiles);
    if (imageFiles.length === 0) {
      this.messageService.add({ severity: 'warn', summary: 'Preview unavailable', detail: 'No previewable image files found.' });
      return;
    }

    const selectedIndex = imageFiles.findIndex(f => f.id === file.id);
    this.imagePreviewKind = 'order';
    this.revisionImagePreviewFiles = [];
    this.imagePreviewFiles = imageFiles;
    this.imagePreviewIndex = selectedIndex >= 0 ? selectedIndex : 0;
    this.showImagePreviewDialog = true;
    this.loadCurrentImagePreview();
  }

  openRevisionImagePreview(file: RevisionAttachment, baseFiles?: RevisionAttachment[]): void {
    if (!this.order || !file?.id || !this.isRevisionAttachmentPreviewable(file)) {
      return;
    }

    const source = baseFiles?.length ? baseFiles : [];
    const imageFiles = source.filter(f => this.isRevisionAttachmentPreviewable(f));
    if (imageFiles.length === 0) {
      this.messageService.add({ severity: 'warn', summary: 'Preview unavailable', detail: 'No previewable revision images.' });
      return;
    }

    const selectedIndex = imageFiles.findIndex(f => f.id === file.id);
    this.imagePreviewKind = 'revision';
    this.imagePreviewFiles = [];
    this.revisionImagePreviewFiles = imageFiles;
    this.imagePreviewIndex = selectedIndex >= 0 ? selectedIndex : 0;
    this.showImagePreviewDialog = true;
    this.loadCurrentImagePreview();
  }

  closeImagePreview(): void {
    this.showImagePreviewDialog = false;
    this.imagePreviewKind = 'order';
    this.imagePreviewFiles = [];
    this.revisionImagePreviewFiles = [];
    this.imagePreviewIndex = 0;
    this.cleanupImagePreviewUrl();
  }

  get currentImagePreviewFile(): LogoFile | null {
    if (!this.imagePreviewFiles.length) return null;
    return this.imagePreviewFiles[this.imagePreviewIndex] ?? null;
  }

  get currentRevisionImagePreviewFile(): RevisionAttachment | null {
    if (!this.revisionImagePreviewFiles.length) return null;
    return this.revisionImagePreviewFiles[this.imagePreviewIndex] ?? null;
  }

  get imagePreviewGalleryLength(): number {
    return this.imagePreviewKind === 'revision'
      ? this.revisionImagePreviewFiles.length
      : this.imagePreviewFiles.length;
  }

  get imagePreviewDialogHeader(): string {
    const prefix = this.imagePreviewKind === 'revision' ? '[Revision] ' : '';
    const name = this.imagePreviewKind === 'revision'
      ? (this.currentRevisionImagePreviewFile?.originalFileName || 'Image preview')
      : (this.currentImagePreviewFile?.originalFileName || 'Image preview');
    return prefix + name;
  }

  get imagePreviewAlt(): string {
    return this.imagePreviewKind === 'revision'
      ? (this.currentRevisionImagePreviewFile?.originalFileName || 'Revision preview')
      : (this.currentImagePreviewFile?.originalFileName || 'Preview image');
  }

  canGoPreviousImage(): boolean {
    return this.imagePreviewGalleryLength > 1;
  }

  canGoNextImage(): boolean {
    return this.imagePreviewGalleryLength > 1;
  }

  previousImagePreview(): void {
    const len = this.imagePreviewGalleryLength;
    if (len <= 0) return;
    this.imagePreviewIndex = (this.imagePreviewIndex - 1 + len) % len;
    this.loadCurrentImagePreview();
  }

  nextImagePreview(): void {
    const len = this.imagePreviewGalleryLength;
    if (len <= 0) return;
    this.imagePreviewIndex = (this.imagePreviewIndex + 1) % len;
    this.loadCurrentImagePreview();
  }

  downloadRevisionAttachment(file: RevisionAttachment): void {
    if (!file?.id) return;
    this.apiService.getBlob(`revisions/files/${file.id}/download`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob: Blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = file.originalFileName || 'download';
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
          window.URL.revokeObjectURL(url);
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'File downloaded' });
        },
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to download file' });
        }
      });
  }

  downloadCurrentImagePreview(): void {
    if (this.imagePreviewKind === 'revision') {
      const f = this.currentRevisionImagePreviewFile;
      if (f) this.downloadRevisionAttachment(f);
    } else {
      const f = this.currentImagePreviewFile;
      if (f) this.downloadFile(f);
    }
  }

  private loadCurrentImagePreview(): void {
    this.imagePreviewLoading = true;
    this.cleanupImagePreviewUrl();

    if (this.imagePreviewKind === 'revision') {
      const currentFile = this.currentRevisionImagePreviewFile;
      if (!currentFile) {
        this.imagePreviewLoading = false;
        return;
      }

      this.apiService.getBlob(`revisions/files/${currentFile.id}/download`)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (blob: Blob) => {
            const isImageBlob = (blob.type || '').toLowerCase().startsWith('image/');
            if (!isImageBlob) {
              this.imagePreviewLoading = false;
              this.messageService.add({
                severity: 'warn',
                summary: 'Preview unavailable',
                detail: `${currentFile.originalFileName} is not an image file.`
              });
              return;
            }

            this.imagePreviewUrl = window.URL.createObjectURL(blob);
            this.imagePreviewLoading = false;
          },
          error: () => {
            this.imagePreviewLoading = false;
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to load image preview'
            });
          }
        });
      return;
    }

    const currentFile = this.currentImagePreviewFile;
    if (!currentFile) {
      this.imagePreviewLoading = false;
      return;
    }

    this.apiService.getBlob(`files/${currentFile.id}/download`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob: Blob) => {
          const isImageBlob = (blob.type || '').toLowerCase().startsWith('image/');
          if (!isImageBlob) {
            this.imagePreviewLoading = false;
            this.messageService.add({
              severity: 'warn',
              summary: 'Preview unavailable',
              detail: `${currentFile.originalFileName} is not an image file.`
            });
            return;
          }

          this.imagePreviewUrl = window.URL.createObjectURL(blob);
          this.imagePreviewLoading = false;
        },
        error: () => {
          this.imagePreviewLoading = false;
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load image preview'
          });
        }
      });
  }

  openMessagesTab(): void {
    this.activateTab(2);
  }

  /** Ensure TabView active index and lazy tab data load run reliably inside stacked modals. */
  private activateTab(index: number): void {
    this.activeTab = index;
    this.onTabChange(index);
    this.cdr.detectChanges();
  }

  getDisplayNameForRole(originalName: string | undefined | null, role: string | undefined | null): string {
    if (this.isDesigner && role === 'Client') {
      return this.designerClientAliasName;
    }
    return (originalName || '').trim() || role || 'User';
  }

  getDisplayRole(role: string | undefined | null): string {
    if (this.isDesigner && role === 'Client') {
      return this.designerClientAliasRole;
    }
    return (role || '').trim();
  }

  getRevisionRequestedByDisplayName(revision: OrderRevision): string {
    if (this.isDesigner) {
      return this.designerClientAliasName;
    }
    return (revision.requestedByName || '').trim() || 'Client';
  }

  selectAllFilesToSend(): void {
    this.selectedFileIds = this.filesToForward.map(f => f.id);
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
          this.closeModal();
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
    if (files.length === 0) return;

    const tooBig = files.filter(f => f.size > MAX_UPLOAD_BYTES);
    if (tooBig.length > 0) {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Each file must be 500MB or smaller.' });
    }
    const ok = files.filter(f => f.size <= MAX_UPLOAD_BYTES);
    const merged = [...this.revisionFiles, ...ok];
    if (combinedFileBytes(merged) > MAX_UPLOAD_BYTES) {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Combined file size cannot exceed 500MB.' });
      this.revisionFileUpload?.clear();
      return;
    }
    this.revisionFiles = merged;
    this.revisionFileUpload?.clear();
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
          this.closeModal();
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
      message: 'Once approved, your logo will be saved and the admin will mark the order as completed.<br>All preview and revision files will be permanently deleted, and only final approved files will be saved. Are you sure you want to proceed?',
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
                detail: 'Logo approved successfully. Admin will mark the order as completed.'
              });
              this.loadOrder(this.order!.id);
              this.orderUpdated.emit();
              this.closeModal();
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

  formatStatus(status: string): string {
    const labelMap: { [key: string]: string } = {
      'ClientApproved': 'Approved',
      'PreviewDelivered': 'Preview Delivered',
      'RevisionRequested': 'Revision Requested',
      'WaitingForAdminApproval': 'Waiting For Admin Approval',
      'PriceApprovalPending': 'Price Approval Pending',
      'InProgress': 'In Progress',
      'CancelledByUser': 'Cancelled By User',
      'CancelledByAdmin': 'Cancelled By Admin'
    };
    return labelMap[status] || status.replace(/([A-Z])/g, ' $1').trim();
  }

  getStatusSeverity(status: string): string {
    const severityMap: { [key: string]: string } = {
      'WaitingForAdminApproval': 'warning',
      'PriceApprovalPending': 'info',
      'InProgress': 'info',
      'PreviewDelivered': 'success',
      'RevisionRequested': 'warn',
      'ClientApproved': 'success',
      'Completed': 'success',
      'Cancelled': 'danger'
    };
    return severityMap[status] || 'secondary';
  }

  canApproveOrder(): boolean {
    return (this.isAdmin || this.isSuperAdmin) && this.order?.status === OrderStatus.WaitingForAdminApproval;
  }

  canRequestPriceApproval(): boolean {
    if (!(this.isAdmin || this.isSuperAdmin) || !this.order) return false;
    if (isOrderLocked(this.order.status)) return false;
    if (this.order.status === OrderStatus.Completed || this.order.status === OrderStatus.Cancelled) return false;
    // Client charge already accepted — use Edit Client Price if admin needs a new amount, then request again if needed
    if (this.clientAcceptedClientCharge) return false;
    return true;
  }

  canApprovePrice(): boolean {
    return this.isClient && this.order?.status === OrderStatus.PriceApprovalPending;
  }

  /** Designer can request price change when order has design, price not yet approved */
  canDesignerRequestPrice(): boolean {
    if (!this.order || !this.isDesigner) return false;
    if (!this.order.designCategory || !this.order.designType) return false;
    const ps = this.order.priceApprovalStatus;
    if (ps === 'Approved' || ps === 'AutoApproved') return false;
    return true;
  }

  openRequestPriceDialog(): void {
    this.designerRequestedPrice = this.order?.standardPrice ?? this.order?.designerProposedPrice ?? this.order?.proposedPrice ?? 0;
    this.designerRequestPriceMessage = '';
    this.showRequestPriceDialog = true;
  }

  submitRequestPrice(): void {
    if (!this.order || this.designerRequestedPrice <= 0) return;
    const body: { orderId: string; proposedPrice: number; message?: string } = {
      orderId: this.order.id,
      proposedPrice: this.designerRequestedPrice
    };
    const msg = this.designerRequestPriceMessage?.trim();
    if (msg) body.message = msg;

    this.apiService.post('designer-payout/pricing/propose', body).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.showRequestPriceDialog = false;
        this.loadOrder(this.order!.id);
        this.orderUpdated.emit();
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Price request sent to admin. Admin will review before sending files to client.' });
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Failed to submit price request' });
      }
    });
  }

  /** Admin: Designer proposed price differs from standard; needs approval */
  canApproveDesignerPrice(): boolean {
    return (this.isAdmin || this.isSuperAdmin)
      && !!this.order
      && this.order.priceApprovalStatus === 'PendingApproval'
      && !isOrderLocked(this.order.status);
  }

  openDesignerPriceApprovalDialog(): void {
    this.designerApprovedPrice = this.order?.designerProposedPrice ?? this.order?.proposedPrice ?? 0;
    this.designerPriceApprovalAction = 'Approve';
    this.designerPriceApprovalMessage = '';
    this.showDesignerPriceApprovalDialog = true;
  }

  submitDesignerPriceApproval(): void {
    if (!this.order) return;
    const approvedPrice = this.designerPriceApprovalAction === 'Reject' ? undefined : this.designerApprovedPrice;
    if ((this.designerPriceApprovalAction === 'Approve' || this.designerPriceApprovalAction === 'Modify') && (!approvedPrice || approvedPrice <= 0)) {
      this.messageService.add({ severity: 'warn', summary: 'Required', detail: 'Approved price is required.' });
      return;
    }
    const approveBody: { action: string; approvedPrice: number | null; message?: string } = {
      action: this.designerPriceApprovalAction,
      approvedPrice: approvedPrice ?? null
    };
    const adminMsg = this.designerPriceApprovalMessage?.trim();
    if (adminMsg) approveBody.message = adminMsg;

    this.apiService.put(`designer-payout/orders/${this.order.id}/approve-price`, approveBody).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.showDesignerPriceApprovalDialog = false;
        this.loadOrder(this.order!.id);
        this.orderUpdated.emit();
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Designer price approval processed.' });
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Failed to process approval' });
      }
    });
  }

  canAssignDesigner(): boolean {
    return (this.isAdmin || this.isSuperAdmin) && !isOrderLocked(this.order?.status || '');
  }

  get assignDesignerButtonLabel(): string {
    return this.order?.designerId ? 'Change Designer' : 'Assign Designer';
  }

  /** Admin can allow extra revisions when client has exceeded package limit */
  canAllowExtraRevisions(): boolean {
    return !!(this.isAdmin || this.isSuperAdmin)
      && !!this.order
      && !!this.order.revisionLimitExceeded
      && (this.order.status === OrderStatus.PreviewDelivered || this.order.status === OrderStatus.RevisionRequested);
  }

  allowExtraRevisions(): void {
    if (!this.order) return;
    this.apiService.put<Order>(`orders/${this.order.id}/allow-extra-revisions`, { allowExtraRevisions: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (updated) => {
          this.order = updated;
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Client can now request additional revisions.' });
        },
        error: (err) => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Failed to allow extra revisions' });
        }
      });
  }

  canRequestRevision(): boolean {
    // Only allow revision if:
    // 1. User is a client
    // 2. Order status is PreviewDelivered (strict requirement)
    // 3. Revision limit not exceeded (unless admin approved extra revisions)
    if (!this.isClient || !this.order) {
      return false;
    }

    if (this.order.status !== OrderStatus.PreviewDelivered) {
      return false;
    }

    if (this.order.revisionLimitExceeded) {
      return false;
    }

    return true;
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

  /** Client Pricing: SuperAdmin always, Admin only if granted permission, Client only after designer sends */
  shouldShowClientPricing(): boolean {
    if (this.isSuperAdmin) return true;
    if (this.isAdmin) return this.permissionsService.hasPermission('ViewAllOrders');
    if (!this.isClient || !this.order) return false;
    const statusesWhenPricingVisible: OrderStatus[] = [
      OrderStatus.PreviewDelivered,
      OrderStatus.RevisionRequested,
      OrderStatus.ClientApproved,
      OrderStatus.Completed
    ];
    return statusesWhenPricingVisible.includes(this.order.status);
  }

  /** Vector/screen-printing orders use style preferences, not stitch type. */
  isVectorOrder(): boolean {
    const category = (this.order?.designCategory || '').toString().toLowerCase();
    return category.includes('vector');
  }

  /** Embroidery/patch orders use stitch type (stored in stylePreferences field). */
  isStitchTypeOrder(): boolean {
    const category = (this.order?.designCategory || '').toString().toLowerCase();
    return category.includes('embroidery') || category.includes('digitizing') || category.includes('patch');
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
    
    // Clients can only cancel before work has started
    if (this.isClient) {
      return this.order.status === OrderStatus.WaitingForAdminApproval || 
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
           this.order.status === OrderStatus.Completed;
  }

  /** Admin/SuperAdmin: Mark ClientApproved order as Completed (fast process) */
  canMarkAsCompleted(): boolean {
    return (this.isAdmin || this.isSuperAdmin) && 
           this.order?.status === OrderStatus.ClientApproved;
  }

  markOrderCompleted(): void {
    if (!this.order) return;

    this.apiService.put(`orders/${this.order.id}/status`, { status: OrderStatus.Completed })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Order marked as completed'
          });
          this.loadOrder(this.order!.id);
          this.orderUpdated.emit();
          this.closeModal();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to complete order'
          });
        }
      });
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

  openEditClientPriceDialog(): void {
    this.editClientChargePrice = this.order?.clientChargePrice ?? this.order?.clientBasePrice ?? this.order?.price ?? 0;
    this.showEditClientPriceDialog = true;
  }

  saveClientChargePrice(): void {
    if (!this.order || this.editClientChargePrice < 0) return;
    this.apiService.put(`orders/${this.order.id}/client-price`, {
      clientChargePrice: this.editClientChargePrice
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.showEditClientPriceDialog = false;
        this.loadOrder(this.order!.id);
        this.orderUpdated.emit();
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Client charge price updated.' });
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Failed to update price' });
      }
    });
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
          this.closeModal();
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
          this.closeModal();
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
    this.refundAmount = this.order?.clientChargePrice ?? this.order?.clientBasePrice ?? this.order?.price ?? 0;
    this.refundReason = '';
    this.showRefundDialog = true;
  }

  confirmRefundOrder(): void {
    if (!this.order || !this.refundReason.trim() || this.refundAmount <= 0) return;

    const maxRefund = this.order.clientChargePrice ?? this.order.clientBasePrice ?? this.order.price;
    if (this.refundAmount > maxRefund) {
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
          this.closeModal();
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

  /** True if order is cancelled (by client or admin) */
  isOrderCancelled(): boolean {
    if (!this.order) return false;
    return this.order.status === OrderStatus.Cancelled ||
           this.order.status === OrderStatus.CancelledByUser ||
           this.order.status === OrderStatus.CancelledByAdmin;
  }

  /** Banner message for cancelled orders */
  getCancelledBannerMessage(): string {
    if (!this.order) return 'Order cancelled.';
    const d = this.order.updatedAt || this.order.createdAt;
    const dateStr = d ? new Date(d).toLocaleDateString(undefined, { dateStyle: 'long' }) : '';
    if (this.order.status === OrderStatus.CancelledByUser) {
      return `Order cancelled by client${dateStr ? ' on ' + dateStr : ''}. This order is now closed.`;
    }
    if (this.order.status === OrderStatus.CancelledByAdmin) {
      return `Order cancelled by admin${dateStr ? ' on ' + dateStr : ''}. This order is now closed.`;
    }
    return `Order cancelled${dateStr ? ' on ' + dateStr : ''}. This order is now closed.`;
  }

  canUploadFiles(): boolean {
    if (!this.order) return false;
    
    // Check if order is completed or final approved
    const isCompleted = this.order.status === OrderStatus.Completed || 
                       this.order.status === OrderStatus.ClientApproved;
    
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
                        this.order.status === OrderStatus.ClientApproved;
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
