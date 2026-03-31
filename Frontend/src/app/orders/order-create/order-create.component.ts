import { Component, OnInit, OnChanges, SimpleChanges, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { MessageService } from 'primeng/api';
import { CreateOrderRequest, OrderPriority } from '@shared/models/order.model';
import { FileType } from '@shared/models/file.model';
import { DesignCategory, DesignType } from '@shared/models/design-pricing.model';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-order-create',
  templateUrl: './order-create.component.html',
  styleUrls: ['./order-create.component.scss']
})
export class OrderCreateComponent implements OnInit, OnChanges {
  @Input() visible: boolean = false;
  @Input() prefillData: any = null; // For quick reorder
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() orderCreated = new EventEmitter<any>();

  orderForm: FormGroup;
  loading = false;
  uploadingFiles = false;
  submitError = '';
  minDate: Date = OrderCreateComponent.todayAtMidnight();
  selectedFiles: File[] = [];
  isDragOver = false;
  priorityOptions = [
    { label: 'Normal', value: 2, description: 'Standard timeline' },
    { label: 'Rush', value: 4, description: 'Rush order — fastest delivery' }
  ];

  logoCategoryOptions = [
    { label: 'Embroidery / Digitizing', value: 'embroideryDigitizing' },
    { label: 'Vector/Screen Printing', value: 'vector' },
    { label: 'Custom Patch', value: 'customPatch' }
  ];

  placementOptions = [
    { label: 'Left Chest', value: DesignType.LeftChest },
    { label: 'Jacket Back', value: DesignType.JacketBack }
  ];

  stitchTypeOptions = [
    { label: 'Running Stitch', value: 'RUNNING STITCH' },
    { label: 'Satin', value: 'SATIN' },
    { label: 'Tatami', value: 'TATAMI' },
    { label: 'Chain Stitch', value: 'CHAIN STITCH' }
  ];

  showPlacementDropdown = false;
  showStylePreferencesDropdown = false;

  private static todayAtMidnight(): Date {
    const d = new Date();
    d.setHours(0, 0, 0, 0);
    return d;
  }

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    private authService: AuthService,
    private messageService: MessageService
  ) {
    const today = OrderCreateComponent.todayAtMidnight();
    this.orderForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', [Validators.required, Validators.maxLength(1000)]],
      price: [0], // Hidden: admin sets price when no client pricing exists
      priority: [2], // Default to Medium (2)
      deadline: [today],
      logoCategory: [null],
      placement: [null],
      requiredFormats: [''],
      colorPreferences: [''],
      stylePreferences: ['']
    });
  }

  ngOnInit(): void {
    // Form is already initialized in constructor
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible'] && changes['visible'].currentValue) {
      if (this.prefillData) {
        this.prefillForm(this.prefillData);
      } else {
        this.resetForm();
      }
    }
  }

  private prefillForm(data: any): void {
    const today = OrderCreateComponent.todayAtMidnight();
    this.minDate = today;
    this.orderForm.patchValue({
      title: data.title || '',
      description: data.description || '',
      price: data.price || 0,
      priority: data.priority || 2,
      deadline: today,
      logoCategory: this.normalizeLogoCategoryForForm(data.logoCategory),
      placement: data.placement || null,
      requiredFormats: data.requiredFormats || '',
      colorPreferences: data.colorPreferences || '',
      stylePreferences: data.stylePreferences || ''
    });
    this.updatePlacementVisibility();
    this.updateStylePreferencesVisibility();
    this.orderForm.markAsUntouched();
    this.selectedFiles = [];
  }

  resetForm(): void {
    const today = OrderCreateComponent.todayAtMidnight();
    this.minDate = today;
    this.submitError = '';
    this.orderForm.reset({
      title: '',
      description: '',
      price: 0,
      deadline: today,
      priority: 2, // Default to Medium (2)
      logoCategory: null,
      placement: null,
      requiredFormats: '',
      colorPreferences: '',
      stylePreferences: ''
    });
    this.showPlacementDropdown = false;
    this.showStylePreferencesDropdown = false;
    this.updateStylePreferencesVisibility();
    this.orderForm.markAsUntouched();
    this.selectedFiles = [];
  }

  onLogoCategoryChange(): void {
    this.updatePlacementVisibility();
    this.updateStylePreferencesVisibility();
    if (!this.showPlacementDropdown) {
      this.orderForm.patchValue({ placement: null });
    }
    if (!this.showStylePreferencesDropdown) {
      this.orderForm.patchValue({ stylePreferences: '' });
    }
  }

  private normalizeLogoCategoryForForm(value: string | null | undefined): string | null {
    if (value === 'embroidery' || value === 'digitizing' || value === 'embroideryDigitizing') {
      return 'embroideryDigitizing';
    }
    return value ?? null;
  }

  private updatePlacementVisibility(): void {
    const category = this.orderForm.get('logoCategory')?.value;
    this.showPlacementDropdown = category === 'embroideryDigitizing' || category === 'customPatch';
  }

  private updateStylePreferencesVisibility(): void {
    const category = this.orderForm.get('logoCategory')?.value;
    this.showStylePreferencesDropdown = category === 'embroideryDigitizing';
    const styleControl = this.orderForm.get('stylePreferences');
    if (styleControl) {
      if (this.showStylePreferencesDropdown) {
        styleControl.setValidators(Validators.required);
      } else {
        styleControl.clearValidators();
      }
      styleControl.updateValueAndValidity();
    }
  }

  closeModal(): void {
    this.visible = false;
    this.visibleChange.emit(false);
    this.resetForm();
  }

  onSubmit(): void {
    if (this.orderForm.invalid) {
      this.markFormGroupTouched(this.orderForm);
      return;
    }

    this.submitError = '';
    this.loading = true;
    const formValue = this.orderForm.value;
    
    // Helper function to convert empty strings to undefined
    const cleanValue = (value: any): any => {
      if (value === null || value === undefined || value === '') {
        return undefined;
      }
      return value;
    };
    
    // Build order data object, only including defined values
    const orderData: any = {
      title: formValue.title.trim(),
      description: formValue.description.trim(),
      price: formValue.price
    };
    
    // Add priority (default to 2 = Medium if not set)
    if (formValue.priority !== null && formValue.priority !== undefined) {
      orderData.priority = formValue.priority;
    } else {
      orderData.priority = 2; // Default to Medium
    }
    
    // Add optional fields only if they have values
    if (formValue.deadline) {
      orderData.deadline = new Date(formValue.deadline).toISOString();
    }
    
    // Map logo category and placement to backend designCategory/designType
    const logoCategory = formValue.logoCategory;
    const placement = formValue.placement;
    if (logoCategory) {
      const categoryMap: Record<string, number> = {
        embroideryDigitizing: DesignCategory.EmbroideryDigitizing,
        embroidery: DesignCategory.EmbroideryDigitizing,
        digitizing: DesignCategory.EmbroideryDigitizing,
        vector: DesignCategory.VectorScreenPrinting,
        customPatch: DesignCategory.CustomPatch
      };
      orderData.designCategory = categoryMap[logoCategory];
      if (this.showPlacementDropdown && placement != null) {
        orderData.designType = placement;
      }
    }

    const optionalFields = ['requiredFormats', 'colorPreferences', 'stylePreferences'];
    optionalFields.forEach(field => {
      const value = cleanValue(formValue[field]);
      if (value !== undefined) {
        orderData[field] = value;
      }
    });

    if (this.selectedFiles.length > 0) {
      this.createOrderWithFiles(orderData);
    } else {
      this.apiService.post<any>('orders', orderData).subscribe({
        next: (order) => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Order created successfully'
          });
          this.loading = false;
          this.orderCreated.emit(order);
          this.closeModal();
        },
        error: (error) => {
          const msg = error.error?.error || error.error?.message || (typeof error.error === 'string' ? error.error : null);
          const detail = msg || (error.error?.errors ? JSON.stringify(error.error.errors) : 'Failed to create order');
          this.submitError = typeof detail === 'string' ? detail : 'Failed to create order';
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: this.submitError
          });
          this.loading = false;
        }
      });
    }
  }

  private createOrderWithFiles(orderData: any): void {
    const formData = new FormData();
    formData.append('order', JSON.stringify(orderData));
    this.selectedFiles.forEach(file => {
      formData.append('files', file);
    });

    this.apiService.post<any>('orders/with-files', formData).subscribe({
      next: (order) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `Order created with ${this.selectedFiles.length} file(s) successfully`
        });
        this.loading = false;
        this.orderCreated.emit(order);
        this.closeModal();
      },
      error: (error) => {
        const msg = error.error?.error || error.error?.message || (typeof error.error === 'string' ? error.error : null);
        const detail = msg || (error.error?.errors ? JSON.stringify(error.error.errors) : 'Failed to create order');
        console.error('Order creation failed:', { status: error.status, error: error.error, detail });
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: typeof detail === 'string' ? detail : 'Failed to create order'
        });
        this.loading = false;
      }
    });
  }

  onNativeFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;
    
    const files: File[] = Array.from(input.files);
    this.addFiles(files);
    
    // Reset the input so the same file can be selected again
    input.value = '';
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
    
    if (event.dataTransfer?.files) {
      const files: File[] = Array.from(event.dataTransfer.files);
      this.addFiles(files);
    }
  }

  private addFiles(files: File[]): void {
    const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp'];
    const vectorExtensions = ['.svg', '.pdf', '.ai', '.eps', '.psd'];
    const embroiderExtensions = ['.pes', '.dst', '.jef', '.exp', '.vp3', '.xxx', '.hus', '.art', '.vip', '.vip3', '.shv', '.pec', '.jpm', '.sew', '.emb', '.csd', '.pcs', '.phb', '.phc', '.stx', '.s10', '.dsb', '.zsk'];
    const allowedExtensions = [...imageExtensions, ...vectorExtensions, ...embroiderExtensions];
    const imageMaxBytes = 10 * 1024 * 1024;  // 10MB
    const vectorMaxBytes = 25 * 1024 * 1024;  // 25MB

    const getMaxSize = (fileName: string): number => {
      const ext = '.' + (fileName.split('.').pop() || '').toLowerCase();
      if (imageExtensions.includes(ext)) return imageMaxBytes;
      if (vectorExtensions.includes(ext) || embroiderExtensions.includes(ext)) return vectorMaxBytes;
      return imageMaxBytes;
    };

    const invalidSize = files.filter(f => f.size > getMaxSize(f.name));
    const invalidType = files.filter(f => {
      const ext = '.' + (f.name.split('.').pop() || '').toLowerCase();
      return !allowedExtensions.includes(ext) && !f.type.startsWith('image/');
    });

    if (invalidSize.length > 0) {
      this.messageService.add({
        severity: 'error',
        summary: 'File Too Large',
        detail: `${invalidSize.length} file(s) exceed allowed size (images: 10MB, vector/docs: 25MB)`
      });
    }

    if (invalidType.length > 0) {
      this.messageService.add({
        severity: 'error',
        summary: 'Invalid File Type',
        detail: `${invalidType.length} file(s) have unsupported formats`
      });
    }

    // Filter to valid files and avoid duplicates
    const validFiles = files.filter(f => {
      const ext = '.' + (f.name.split('.').pop() || '').toLowerCase();
      const validSize = f.size <= getMaxSize(f.name);
      const validType = allowedExtensions.includes(ext) || f.type.startsWith('image/');
      const notDuplicate = !this.selectedFiles.some(existing => existing.name === f.name && existing.size === f.size);
      return validSize && validType && notDuplicate;
    });
    
    this.selectedFiles = [...this.selectedFiles, ...validFiles];
  }

  removeFile(index: number): void {
    this.selectedFiles.splice(index, 1);
  }

  clearAllFiles(): void {
    this.selectedFiles = [];
  }

  getFileIcon(fileName: string): string {
    const ext = fileName.split('.').pop()?.toLowerCase() || '';
    const iconMap: { [key: string]: string } = {
      'pdf': 'pi-file-pdf',
      'jpg': 'pi-image',
      'jpeg': 'pi-image',
      'png': 'pi-image',
      'gif': 'pi-image',
      'svg': 'pi-image',
      'webp': 'pi-image',
      'ai': 'pi-file',
      'eps': 'pi-file',
      'psd': 'pi-file',
      'pes': 'pi-file', 'dst': 'pi-file', 'jef': 'pi-file', 'exp': 'pi-file', 'vp3': 'pi-file',
      'xxx': 'pi-file', 'hus': 'pi-file', 'art': 'pi-file', 'vip': 'pi-file', 'vip3': 'pi-file',
      'shv': 'pi-file', 'pec': 'pi-file', 'jpm': 'pi-file', 'sew': 'pi-file', 'emb': 'pi-file',
      'csd': 'pi-file', 'pcs': 'pi-file', 'phb': 'pi-file', 'phc': 'pi-file', 'stx': 'pi-file',
      's10': 'pi-file', 'dsb': 'pi-file', 'zsk': 'pi-file'
    };
    return iconMap[ext] || 'pi-file';
  }

  private uploadFiles(orderId: string): void {
    if (this.selectedFiles.length === 0) {
      return;
    }

    this.uploadingFiles = true;
    
    // Use the new multiple file upload endpoint
    const formData = new FormData();
    
    // Append all files to FormData
    // Note: The backend expects files[] array, so we append each file with the same key
    this.selectedFiles.forEach(file => {
      formData.append('files', file);
    });
    
    formData.append('fileType', FileType.Reference); // Clients can only upload reference files
    
    this.apiService.post(`files/upload-multiple/${orderId}`, formData).subscribe({
      next: (results: any) => {
        const uploadedCount = Array.isArray(results) ? results.length : 0;
        
        if (uploadedCount === this.selectedFiles.length) {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: `Order created and ${uploadedCount} file(s) uploaded successfully`
          });
        } else {
          this.messageService.add({
            severity: 'warn',
            summary: 'Partial Success',
            detail: `Order created. ${uploadedCount} file(s) uploaded successfully. You can upload remaining files later from the order details page.`
          });
        }
        this.uploadingFiles = false;
        this.orderCreated.emit({ id: orderId });
        this.closeModal();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'warn',
          summary: 'Warning',
          detail: error.error?.error || 'Order created but files failed to upload. You can upload them later from the order details page.'
        });
        this.uploadingFiles = false;
        this.orderCreated.emit({ id: orderId });
        this.closeModal();
      }
    });
  }

  cancel(): void {
    this.closeModal();
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  get f() {
    return this.orderForm.controls;
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }
}
