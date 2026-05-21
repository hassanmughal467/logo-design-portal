import { Component, OnInit, ViewChild } from '@angular/core';
import { FileUpload } from 'primeng/fileupload';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { MessageService, ConfirmationService } from 'primeng/api';
import { FileType } from '@shared/models/file.model';
import { isOrderLocked } from '@shared/utils/order-locking';
import { DesignCategory, DesignType, DesignerPricingInfo } from '@shared/models/design-pricing.model';
import { MAX_UPLOAD_BYTES, combinedFileBytes } from '@core/constants/upload-limits';

@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.scss']
})
export class FileUploadComponent implements OnInit {
  @ViewChild('fileUpload') private primeFileUpload?: FileUpload;

  readonly maxUploadFileSize = MAX_UPLOAD_BYTES;
  uploadForm: FormGroup;
  orderId: string | null = null;
  loading = false;
  uploadingFiles = false;
  selectedFiles: File[] = [];
  showConfirmDialog = false;
  userRole: string | null = null;
  uploadSuccess = false;
  uploadedFilesCount = 0;
  order: any = null;

  fileTypes = [
    { label: 'Reference', value: FileType.Reference },
    { label: 'Preview', value: FileType.Preview },
    { label: 'Final', value: FileType.Final }
  ];

  designPricingInfo: DesignerPricingInfo[] = [];
  /** Static design types. DefaultPrice from DesignPricing API (SuperAdmin sets) or 0 until added. */
  designTypesByCategory: { designCategory: number; designType: number; label: string; defaultPrice: number | null }[] = [
    { designCategory: DesignCategory.EmbroideryDigitizing, designType: DesignType.LeftChest, label: 'Left Chest', defaultPrice: 0 },
    { designCategory: DesignCategory.EmbroideryDigitizing, designType: DesignType.JacketBack, label: 'Jacket Back', defaultPrice: 0 },
    { designCategory: DesignCategory.VectorScreenPrinting, designType: DesignType.SimpleVector, label: 'Simple Vector', defaultPrice: 0 },
    { designCategory: DesignCategory.VectorScreenPrinting, designType: DesignType.ComplexVector, label: 'Complex Vector', defaultPrice: null }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private authService: AuthService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {
    this.uploadForm = this.fb.group({
      fileType: [FileType.Reference, Validators.required],
      description: [''],
      designCategory: [null as number | null],
      designType: [null as number | null],
      proposedPrice: [null as number | null],
      reason: ['']
    });
  }

  ngOnInit(): void {
    this.orderId = this.route.snapshot.paramMap.get('orderId');
    if (!this.orderId) {
      this.router.navigate(['/orders']);
      return;
    }

    const user = this.authService.getCurrentUser();
    this.userRole = user?.role || user?.roleName || null;
    
    // Load order to check status and upload permissions
    this.loadOrder();
    
    const role = user?.role || user?.roleName || '';
    if (role === 'Client') {
      this.fileTypes = [{ label: 'Reference', value: FileType.Reference }];
      // Clients can only upload reference files
      this.uploadForm.patchValue({ fileType: FileType.Reference });
      this.uploadForm.get('fileType')?.disable();
    } else if (role === 'Designer') {
      this.fileTypes = [
        { label: 'Preview', value: FileType.Preview },
        { label: 'Final', value: FileType.Final }
      ];
      // Designers can upload preview or final
      this.uploadForm.patchValue({ fileType: FileType.Preview });
    } else {
      // Admin/SuperAdmin keep full upload options
      this.fileTypes = [
        { label: 'Reference', value: FileType.Reference },
        { label: 'Preview', value: FileType.Preview },
        { label: 'Final', value: FileType.Final }
      ];
    }

    this.loadDesignPricingInfo();
  }

  loadDesignPricingInfo(): void {
    this.apiService.get<DesignerPricingInfo[]>('designer-payout/pricing-info').subscribe({
      next: (info) => {
        this.designPricingInfo = info;
        this.mergeApiPricesIntoDesignTypes();
        this.prefillDesignFromOrder();
      },
      error: () => {
        this.designPricingInfo = [];
      }
    });
  }

  /** True when order has design category+type from client - designer should not re-select. */
  get orderHasDesignFromClient(): boolean {
    const { designCategory, designType } = this.mapOrderDesignToForm(this.order);
    return designCategory != null && designType != null;
  }

  /** Optionally update default prices from API; static design types always used for dropdown. */
  private mergeApiPricesIntoDesignTypes(): void {
    const typeMap: Record<string, number> = {
      LeftChest: DesignType.LeftChest,
      JacketBack: DesignType.JacketBack,
      SimpleVector: DesignType.SimpleVector,
      ComplexVector: DesignType.ComplexVector
    };
    for (const p of this.designPricingInfo) {
      const dt = typeMap[p.designType];
      if (dt != null && p.defaultPrice != null) {
        const item = this.designTypesByCategory.find(d => d.designType === dt);
        if (item) item.defaultPrice = p.defaultPrice;
      }
    }
  }

  get filteredDesignTypes(): { designCategory: number; designType: number; label: string; defaultPrice: number | null }[] {
    const cat = this.uploadForm.get('designCategory')?.value;
    if (!cat) return this.designTypesByCategory;
    return this.designTypesByCategory.filter(d => d.designCategory === cat);
  }

  get selectedDesignTypeDefaultPrice(): number | null {
    const designType = this.uploadForm.get('designType')?.value;
    const info = this.designTypesByCategory.find(d => d.designType === designType);
    return info?.defaultPrice ?? null;
  }

  /** Proposed price is editable; ComplexVector has no default so designer must enter. */
  get isProposedPriceDisabled(): boolean {
    return false;
  }

  /** True when proposed price differs from default (requires admin approval). */
  get requiresAdminApproval(): boolean {
    if (!this.isDesignerPricingRequired) return false;
    const def = this.selectedDesignTypeDefaultPrice;
    const prop = this.uploadForm.get('proposedPrice')?.value;
    if (def == null) return true; // ComplexVector always requires approval
    return prop != null && prop !== def;
  }

  /** Default price label: "PKR 350" or "Custom" for ComplexVector. */
  get defaultPriceLabel(): string {
    const def = this.selectedDesignTypeDefaultPrice;
    return def != null ? `PKR ${def}` : 'Custom';
  }

  get isDesigner(): boolean {
    return this.userRole === 'Designer';
  }

  /** Designer uploading Final files - always needs design category, type, proposed price. */
  get isDesignerFinalUpload(): boolean {
    return this.isDesigner && this.uploadForm.value.fileType === FileType.Final;
  }

  /** Designer uploading Preview when order has no pricing yet - backend requires design category, type, proposed price for first preview batch. */
  get isDesignerPreviewNeedsPricing(): boolean {
    return this.isDesigner
      && this.uploadForm.value.fileType === FileType.Preview
      && this.order != null
      && (this.order.proposedPrice == null && this.order.ProposedPrice == null);
  }

  /** Show design pricing fields when Final upload OR first Preview batch (order has no pricing). */
  get isDesignerPricingRequired(): boolean {
    return this.isDesignerFinalUpload || this.isDesignerPreviewNeedsPricing;
  }

  onFileTypeChange(): void {
    if (this.orderHasDesignFromClient) {
      this.prefillDesignFromOrder();
    } else {
      this.uploadForm.patchValue({ designCategory: null, designType: null, proposedPrice: null });
    }
  }

  onDesignTypeChange(): void {
    const designType = this.uploadForm.get('designType')?.value;
    const info = this.designTypesByCategory.find(d => d.designType === designType);
    if (info && info.defaultPrice != null) {
      this.uploadForm.patchValue({ proposedPrice: info.defaultPrice });
    } else if (info && info.defaultPrice == null) {
      this.uploadForm.patchValue({ proposedPrice: null });
    }
  }

  onDesignCategoryChange(): void {
    this.uploadForm.patchValue({ designType: null, proposedPrice: null });
  }

  /** Map API designCategory/designType (string or number) to enum numbers. */
  private mapOrderDesignToForm(order: any): { designCategory: number | null; designType: number | null } {
    const cat = order?.designCategory ?? order?.DesignCategory;
    const typ = order?.designType ?? order?.DesignType;
    const categoryMap: Record<string, number> = {
      EmbroideryDigitizing: DesignCategory.EmbroideryDigitizing,
      VectorScreenPrinting: DesignCategory.VectorScreenPrinting,
      CustomPatch: DesignCategory.CustomPatch
    };
    const typeMap: Record<string, number> = {
      LeftChest: DesignType.LeftChest,
      JacketBack: DesignType.JacketBack,
      SimpleVector: DesignType.SimpleVector,
      ComplexVector: DesignType.ComplexVector
    };
    const catNum = typeof cat === 'number' ? cat : (cat != null ? categoryMap[String(cat)] : null);
    const typNum = typeof typ === 'number' ? typ : (typ != null ? typeMap[String(typ)] : null);
    return { designCategory: catNum ?? null, designType: typNum ?? null };
  }

  /** Default price from DesignPricing for given category/type. */
  private getDefaultPriceForDesign(designCategory: number, designType: number): number | null {
    const item = this.designTypesByCategory.find(d => d.designCategory === designCategory && d.designType === designType);
    return item?.defaultPrice ?? null;
  }

  /** Pre-fill design category, type, proposed price from order when client already selected. */
  private prefillDesignFromOrder(): void {
    if (!this.order || !this.isDesigner) return;
    const { designCategory, designType } = this.mapOrderDesignToForm(this.order);
    if (designCategory != null && designType != null) {
      const currentProposed = this.uploadForm.get('proposedPrice')?.value;
      const defaultPrice = this.getDefaultPriceForDesign(designCategory, designType);
      this.uploadForm.patchValue({
        designCategory,
        designType,
        proposedPrice: currentProposed != null && currentProposed > 0 ? currentProposed : (defaultPrice ?? null)
      });
    }
  }

  loadOrder(): void {
    if (!this.orderId) return;
    
    this.loading = true;
    this.apiService.get<any>(`orders/${this.orderId}`)
      .subscribe({
        next: (order) => {
          this.order = order;
          this.loading = false;
          this.prefillDesignFromOrder();
          
          // Check if order is locked (terminal status)
          const orderLocked = isOrderLocked(order.status);
          
          // Check if order is completed or final approved (legacy check)
          const isCompleted = order.status === 'Completed' || order.status === 'ClientApproved';
          
          // Check if uploads are disabled
          if (orderLocked) {
            this.messageService.add({
              severity: 'info',
              summary: 'Order Closed',
              detail: 'This order is completed and is now read-only. File uploads are not allowed.'
            });
            this.uploadForm.disable();
          } else if (isCompleted) {
            // For completed orders, only allow if admin has explicitly enabled uploads
            if (this.userRole !== 'Admin' && this.userRole !== 'SuperAdmin') {
              // Clients and designers cannot upload on completed orders unless admin enabled it
              if (order.allowUploads !== true) {
                this.messageService.add({
                  severity: 'warn',
                  summary: 'Upload Disabled',
                  detail: 'File uploads are disabled for this completed order. Please contact an administrator if you need to upload files.'
                });
                this.uploadForm.disable();
              }
            } else if (order.allowUploads === false) {
              // Admin can see the form but it's disabled
              this.messageService.add({
                severity: 'info',
                summary: 'Upload Disabled',
                detail: 'File uploads are currently disabled for this completed order. You can enable them from the order details page.'
              });
              this.uploadForm.disable();
            }
          } else if (order.allowUploads === false) {
            // For non-completed orders, check allowUploads flag
            this.messageService.add({
              severity: 'warn',
              summary: 'Upload Disabled',
              detail: 'File uploads are disabled for this order. Please contact an administrator to enable uploads.'
            });
            this.uploadForm.disable();
          }
        },
        error: (error) => {
          this.loading = false;
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load order information'
          });
        }
      });
  }

  onFileSelect(event: any): void {
    const files: File[] = Array.from(event.files || []);
    const oversize = files.filter(file => file.size > MAX_UPLOAD_BYTES);
    if (oversize.length > 0) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: `${oversize.length} file(s) exceed the maximum size of 500MB each and were not added`
      });
    }

    const validFiles = files.filter(file => file.size <= MAX_UPLOAD_BYTES);
    const merged = [...this.selectedFiles, ...validFiles];
    if (combinedFileBytes(merged) > MAX_UPLOAD_BYTES) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Combined file size cannot exceed 500MB.'
      });
      this.primeFileUpload?.clear();
      return;
    }

    this.selectedFiles = merged;
    // PrimeNG basic mode hides the file input when it has internal files; clear so user can add more.
    this.primeFileUpload?.clear();
  }

  removeFile(index: number): void {
    this.selectedFiles.splice(index, 1);
  }

  onSubmit(event?: Event): void {
    // Prevent default form submission
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }

    if (this.selectedFiles.length === 0 || !this.orderId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select at least one file'
      });
      return;
    }

    if (this.isDesignerPricingRequired) {
      const { designCategory, designType, proposedPrice } = this.uploadForm.value;
      if (designCategory == null || designType == null) {
        this.messageService.add({
          severity: 'warn',
          summary: 'Required',
          detail: 'Design category and type are required.'
        });
        return;
      }
      const defaultPrice = this.selectedDesignTypeDefaultPrice;
      const price = proposedPrice ?? defaultPrice;
      if (price == null || price <= 0) {
        const msg = defaultPrice == null
          ? 'Complex Vector requires a proposed price greater than zero.'
          : 'Please enter a valid proposed price.';
        this.messageService.add({
          severity: 'warn',
          summary: 'Required',
          detail: msg
        });
        return;
      }
    }

    if (this.uploadForm.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please fill in all required fields'
      });
      return;
    }

    // Show confirmation dialog for designers
    if (this.userRole === 'Designer') {
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
    if (this.selectedFiles.length === 0 || !this.orderId) {
      return;
    }

    this.uploadingFiles = true;
    const filesToUpload = [...this.selectedFiles];
    
    const formValue = this.uploadForm.value;
    const appendDesignPricing = (fd: FormData): void => {
      if (this.isDesignerPricingRequired && formValue.designCategory != null && formValue.designType != null && formValue.proposedPrice != null) {
        fd.append('designCategory', String(formValue.designCategory));
        fd.append('designType', String(formValue.designType));
        fd.append('proposedPrice', String(formValue.proposedPrice));
      }
    };

    // Use multiple file upload endpoint if more than one file, otherwise use single
    if (this.selectedFiles.length === 1) {
      // Single file upload
      const formData = new FormData();
      formData.append('file', this.selectedFiles[0]);
      formData.append('fileType', formValue.fileType);
      if (formValue.description) {
        formData.append('description', formValue.description);
      }
      appendDesignPricing(formData);

      this.apiService.post(`files/upload/${this.orderId}`, formData).subscribe({
        next: () => {
          this.uploadedFilesCount += filesToUpload.length;
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: `${filesToUpload.length} file(s) uploaded successfully. You can add more files or click Done to finish.`
          });
          // Clear selected files but keep form open
          this.selectedFiles = [];
          this.uploadForm.patchValue({ description: '' });
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
      formData.append('fileType', formValue.fileType);
      if (formValue.description) {
        formData.append('description', formValue.description);
      }
      appendDesignPricing(formData);

      this.apiService.post(`files/upload-multiple/${this.orderId}`, formData).subscribe({
        next: (results: any) => {
          const uploadedCount = Array.isArray(results) ? results.length : 0;
          this.uploadedFilesCount += uploadedCount;
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: `${uploadedCount} file(s) uploaded successfully. You can add more files or click Done to finish.`
          });
          // Clear selected files but keep form open
          this.selectedFiles = [];
          this.uploadForm.patchValue({ description: '' });
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

  resetForm(): void {
    this.selectedFiles = [];
    this.uploadForm.patchValue({
      description: ''
    });
    this.uploadingFiles = false;
    this.uploadSuccess = false;
    this.uploadedFilesCount = 0;
  }

  done(): void {
    // Navigate back to order detail page
    this.router.navigate(['/orders', this.orderId]);
  }

  cancel(): void {
    if (this.uploadSuccess && this.uploadedFilesCount > 0) {
      // If files were uploaded, confirm before canceling
      this.confirmationService.confirm({
        message: 'You have uploaded files. Are you sure you want to leave?',
        header: 'Confirm',
        icon: 'pi pi-exclamation-triangle',
        accept: () => {
          this.router.navigate(['/orders', this.orderId]);
        }
      });
    } else {
      this.router.navigate(['/orders', this.orderId]);
    }
  }

  isOrderLocked = isOrderLocked;

  get f() {
    return this.uploadForm.controls;
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }
}
