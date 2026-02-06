import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

export interface FileItem {
  id: string;
  orderId: string;
  fileName: string;
  fileSize: number;
  fileType: string;
  uploadedBy: string;
  uploadedAt: Date;
}

@Component({
  selector: 'app-file-list',
  templateUrl: './file-list.component.html',
  styleUrls: ['./file-list.component.scss']
})
export class FileListComponent implements OnInit, OnDestroy {
  files: FileItem[] = [];
  loading = false;
  globalFilter = '';
  first = 0;
  rows = 10;
  orderId: string | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.orderId = params['orderId'] || null;
      this.loadFiles();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadFiles(): void {
    this.loading = true;
    const endpoint = this.orderId 
      ? `files/order/${this.orderId}`
      : 'files';

    this.apiService.get<FileItem[]>(endpoint)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (files) => {
          this.files = files;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading files:', error);
          this.files = [];
          this.loading = false;
        }
      });
  }

  downloadFile(fileId: string, fileName: string): void {
    // Create download link - backend should handle file download with proper auth headers
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    const url = `http://localhost:5000/api/files/${fileId}/download`;
    
    // Create temporary link for download
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.target = '_blank';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
