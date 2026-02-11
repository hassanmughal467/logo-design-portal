import { Component, OnInit, OnChanges, SimpleChanges, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { MessageService } from 'primeng/api';
import { CreateOrderRequest, OrderPriority } from '@shared/models/order.model';
import { FileType } from '@shared/models/file.model';
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
  minDate: Date = new Date();
  selectedFiles: File[] = [];
  priorityOptions = [
    { label: 'Low', value: 1 },
    { label: 'Normal (Medium)', value: 2 },
    { label: 'High', value: 3 },
    { label: 'Urgent', value: 4 }
  ];

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    private authService: AuthService,
    private messageService: MessageService
  ) {
    this.orderForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', [Validators.required, Validators.minLength(10)]],
      price: [0, [Validators.required, Validators.min(0.01)]],
      priority: [2], // Default to Medium (2)
      deadline: [null],
      instructions: [''],
      requiredFormats: [''],
      requirements: [''],
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
    this.orderForm.patchValue({
      title: data.title || '',
      description: data.description || '',
      price: data.price || 0,
      priority: data.priority || 2,
      deadline: null, // Always reset deadline for reorders
      instructions: data.instructions || '',
      requiredFormats: data.requiredFormats || '',
      requirements: data.requirements || '',
      colorPreferences: data.colorPreferences || '',
      stylePreferences: data.stylePreferences || ''
    });
    this.orderForm.markAsUntouched();
    this.selectedFiles = [];
  }

  resetForm(): void {
    this.orderForm.reset({
      title: '',
      description: '',
      price: 0,
      deadline: null,
      priority: 2, // Default to Medium (2)
      instructions: '',
      requiredFormats: '',
      requirements: '',
      colorPreferences: '',
      stylePreferences: ''
    });
    this.orderForm.markAsUntouched();
    this.selectedFiles = [];
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
    
    const optionalFields = ['instructions', 'requiredFormats', 'requirements', 'colorPreferences', 'stylePreferences'];
    optionalFields.forEach(field => {
      const value = cleanValue(formValue[field]);
      if (value !== undefined) {
        orderData[field] = value;
      }
    });

    this.apiService.post<any>('orders', orderData).subscribe({
      next: (order) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Order created successfully'
        });
        this.loading = false;
        
        // Upload files if any were selected
        if (this.selectedFiles.length > 0) {
          this.uploadFiles(order.id);
        } else {
          this.orderCreated.emit(order);
          this.closeModal();
        }
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.error || 'Failed to create order'
        });
        this.loading = false;
      }
    });
  }

  onFileSelect(event: any): void {
    const files: File[] = Array.from(event.files || []);
    
    // Validate file sizes (max 10MB each)
    const maxSize = 10 * 1024 * 1024; // 10MB
    const invalidFiles = files.filter(file => file.size > maxSize);
    
    if (invalidFiles.length > 0) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Some files exceed the 10MB limit and were not added'
      });
    }
    
    // Add valid files
    const validFiles = files.filter(file => file.size <= maxSize);
    this.selectedFiles = [...this.selectedFiles, ...validFiles];
  }

  removeFile(index: number): void {
    this.selectedFiles.splice(index, 1);
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
