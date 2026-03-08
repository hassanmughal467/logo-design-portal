import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { MessageService, ConfirmationService } from 'primeng/api';
import { FileType } from '@shared/models/file.model';
import { isOrderLocked } from '@shared/utils/order-locking';

@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.scss']
})
export class FileUploadComponent implements OnInit {
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
      description: ['']
    });
  }

  ngOnInit(): void {
    this.orderId = this.route.snapshot.paramMap.get('orderId');
    if (!this.orderId) {
      this.router.navigate(['/orders']);
      return;
    }

    const user = this.authService.getCurrentUser();
    this.userRole = user?.role || null;
    
    // Load order to check status and upload permissions
    this.loadOrder();
    
    if (user?.role === 'Client') {
      // Clients can only upload reference files
      this.uploadForm.patchValue({ fileType: FileType.Reference });
      this.uploadForm.get('fileType')?.disable();
    } else if (user?.role === 'Designer') {
      // Designers can upload preview or final
      this.uploadForm.patchValue({ fileType: FileType.Preview });
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
          
          // Check if order is locked (terminal status)
          const orderLocked = isOrderLocked(order.status);
          
          // Check if order is completed or final approved (legacy check)
          const isCompleted = order.status === 'Completed' || order.status === 'FinalApproved';
          
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
    
    const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp'];
    const vectorExtensions = ['.svg', '.pdf', '.ai', '.eps', '.psd'];
    const imageMaxBytes = 10 * 1024 * 1024;  // 10MB
    const vectorMaxBytes = 25 * 1024 * 1024;  // 25MB

    const getMaxSize = (fileName: string): number => {
      const ext = '.' + (fileName.split('.').pop() || '').toLowerCase();
      if (imageExtensions.includes(ext)) return imageMaxBytes;
      if (vectorExtensions.includes(ext)) return vectorMaxBytes;
      return imageMaxBytes;
    };

    const invalidFiles = files.filter(file => file.size > getMaxSize(file.name));
    if (invalidFiles.length > 0) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: `${invalidFiles.length} file(s) exceed allowed size (images: 10MB, vector/docs: 25MB) and were not added`
      });
    }

    const validFiles = files.filter(file => file.size <= getMaxSize(file.name));
    this.selectedFiles = [...this.selectedFiles, ...validFiles];
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
    
    // Use multiple file upload endpoint if more than one file, otherwise use single
    if (this.selectedFiles.length === 1) {
      // Single file upload
      const formData = new FormData();
      formData.append('file', this.selectedFiles[0]);
      formData.append('fileType', this.uploadForm.value.fileType);
      if (this.uploadForm.value.description) {
        formData.append('description', this.uploadForm.value.description);
      }

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
      formData.append('fileType', this.uploadForm.value.fileType);
      if (this.uploadForm.value.description) {
        formData.append('description', this.uploadForm.value.description);
      }

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
