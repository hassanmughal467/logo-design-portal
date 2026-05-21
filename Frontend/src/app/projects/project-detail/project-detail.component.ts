import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Subject, firstValueFrom } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { MAX_UPLOAD_BYTES, combinedFileBytes } from '@core/constants/upload-limits';

export interface Project {
  id: string;
  title: string;
  description: string;
  status: 'Pending' | 'InProgress' | 'Review' | 'Completed' | 'Cancelled';
  orderId?: string;
  clientId?: string;
  designerId?: string;
  revisionsLeft: number;
  maxRevisions: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface Revision {
  id: string;
  projectId: string;
  version: number;
  notes: string;
  files: any[];
  createdAt: Date;
  createdBy: string;
}

export interface Comment {
  id: string;
  projectId: string;
  content: string;
  author: string;
  authorRole: string;
  createdAt: Date;
}

@Component({
  selector: 'app-project-detail',
  templateUrl: './project-detail.component.html',
  styleUrls: ['./project-detail.component.scss']
})
export class ProjectDetailComponent implements OnInit, OnDestroy {
  projectId: string | null = null;
  project: Project | null = null;
  activeTab: number = 0;

  // Revisions
  revisions: Revision[] = [];
  revisionsLoading = false;
  newRevisionNotes = '';

  // Comments
  comments: Comment[] = [];
  commentsLoading = false;
  newComment = '';

  // File upload
  selectedFiles: File[] = [];
  uploading = false;

  // Status change
  showStatusDialog = false;
  newStatus: string | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id');
    if (this.projectId) {
      this.loadProject();
      this.loadRevisions();
      this.loadComments();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProject(): void {
    // Try to load from orders API (projects might be orders)
    this.apiService.get<any>(`orders/${this.projectId}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (order) => {
          this.project = {
            id: order.id,
            title: order.title,
            description: order.description || '',
            status: order.status,
            orderId: order.id,
            clientId: order.clientId,
            designerId: order.designerId,
            revisionsLeft: (order as any).revisionsLeft || 3,
            maxRevisions: (order as any).maxRevisions || 3,
            createdAt: new Date(order.createdAt),
            updatedAt: new Date(order.updatedAt || order.createdAt)
          };
        },
        error: () => {
          // If order not found, create a mock project
          this.project = {
            id: this.projectId!,
            title: 'Logo Design Project',
            description: 'Project description',
            status: 'InProgress',
            revisionsLeft: 2,
            maxRevisions: 3,
            createdAt: new Date(),
            updatedAt: new Date()
          };
        }
      });
  }

  loadRevisions(): void {
    this.revisionsLoading = true;
    // Load revisions (might be from files or a separate endpoint)
    this.apiService.get<any[]>(`files/order/${this.projectId}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (files) => {
          // Group files by version or create revision entries
          this.revisions = files.map((file, index) => ({
            id: file.id,
            projectId: this.projectId!,
            version: index + 1,
            notes: file.notes || `Revision ${index + 1}`,
            files: [file],
            createdAt: new Date(file.uploadedAt || file.createdAt),
            createdBy: file.uploadedBy || 'Designer'
          }));
          this.revisionsLoading = false;
        },
        error: () => {
          this.revisions = [];
          this.revisionsLoading = false;
        }
      });
  }

  loadComments(): void {
    this.commentsLoading = true;
    // Try to load comments (might be from messages API)
    this.apiService.get<any[]>(`messages`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (messages) => {
          this.comments = messages
            .filter(m => (m as any).orderId === this.projectId)
            .map(m => ({
              id: m.id,
              projectId: this.projectId!,
              content: (m as any).content || (m as any).message || '',
              author: (m as any).senderName || 'User',
              authorRole: (m as any).senderRole || 'Client',
              createdAt: new Date((m as any).createdAt)
            }))
            .sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime());
          this.commentsLoading = false;
        },
        error: () => {
          this.comments = [];
          this.commentsLoading = false;
        }
      });
  }

  onFilesSelected(event: any): void {
    const files: File[] = Array.from(event.target.files || []);
    const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp'];
    const vectorExtensions = ['.svg', '.pdf', '.ai', '.eps', '.psd'];
    const embroiderExtensions = ['.pes', '.dst', '.jef', '.exp', '.vp3', '.xxx', '.hus', '.art', '.vip', '.vip3', '.shv', '.pec', '.jpm', '.sew', '.emb', '.csd', '.pcs', '.phb', '.phc', '.stx', '.s10', '.dsb', '.zsk'];
    const allowedExtensions = [...imageExtensions, ...vectorExtensions, ...embroiderExtensions];
    const invalidSize = files.filter(f => f.size > MAX_UPLOAD_BYTES);
    const invalidType = files.filter(f => {
      const ext = '.' + (f.name.split('.').pop() || '').toLowerCase();
      return !allowedExtensions.includes(ext) && !f.type.startsWith('image/');
    });

    if (invalidSize.length > 0) {
      this.messageService.add({
        severity: 'error',
        summary: 'File Too Large',
        detail: `${invalidSize.length} file(s) exceed the maximum size of 500MB each`
      });
    }

    if (invalidType.length > 0) {
      this.messageService.add({
        severity: 'error',
        summary: 'Invalid File Type',
        detail: `${invalidType.length} file(s) have unsupported formats`
      });
    }

    const validFiles = files.filter(f => {
      const ext = '.' + (f.name.split('.').pop() || '').toLowerCase();
      return f.size <= MAX_UPLOAD_BYTES && (allowedExtensions.includes(ext) || f.type.startsWith('image/'));
    });
    if (combinedFileBytes(validFiles) > MAX_UPLOAD_BYTES) {
      this.messageService.add({
        severity: 'error',
        summary: 'File Too Large',
        detail: 'Combined file size cannot exceed 500MB.'
      });
      (event.target as HTMLInputElement).value = '';
      return;
    }
    this.selectedFiles = validFiles;
    (event.target as HTMLInputElement).value = '';
  }

  uploadFiles(): void {
    if (!this.selectedFiles.length || !this.projectId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select files to upload'
      });
      return;
    }

    this.uploading = true;
    
    // Upload files one by one (backend accepts single file)
    const uploadPromises = this.selectedFiles.map(file => {
      const formData = new FormData();
      formData.append('file', file);
      
      return firstValueFrom(this.apiService.post(`files/upload/${this.projectId}`, formData));
    });

    Promise.all(uploadPromises)
      .then(() => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `${this.selectedFiles.length} file(s) uploaded successfully`
        });
        this.selectedFiles = [];
        this.newRevisionNotes = '';
        this.loadRevisions();
        this.uploading = false;
      })
      .catch(() => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to upload some files'
        });
        this.uploading = false;
      });
  }

  addComment(): void {
    if (!this.newComment.trim() || !this.projectId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please enter a comment'
      });
      return;
    }

    const commentContent = this.newComment;
    this.newComment = '';

    // Save to backend
    this.apiService.post('messages', {
      orderId: this.projectId,
      content: commentContent
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (message: any) => {
          // Add to comments list
          const comment: Comment = {
            id: message.id,
            projectId: this.projectId!,
            content: message.content,
            author: message.senderName,
            authorRole: message.senderRole || 'User',
            createdAt: new Date(message.createdAt)
          };
          this.comments.unshift(comment);
          
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Comment added successfully'
          });
        },
        error: (error) => {
          console.error('Add comment error:', error);
          this.newComment = commentContent; // Restore comment text
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to add comment'
          });
        }
      });
  }

  openStatusDialog(): void {
    if (this.project) {
      this.newStatus = this.project.status;
      this.showStatusDialog = true;
    }
  }

  changeStatus(): void {
    if (!this.project || !this.newStatus) return;

    this.apiService.put(`orders/${this.project.id}/status`, { status: this.newStatus })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          if (this.project) {
            this.project.status = this.newStatus as any;
          }
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Status updated successfully'
          });
          this.showStatusDialog = false;
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to update status'
          });
        }
      });
  }

  downloadFile(file: { id: string; originalFileName?: string; fileName?: string }): void {
    const fileName = file.originalFileName || file.fileName || `file-${file.id}`;
    this.apiService.getBlob(`files/${file.id}/download`).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.click();
        URL.revokeObjectURL(url);
        this.messageService.add({ severity: 'success', summary: 'Download', detail: 'File downloaded successfully' });
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Failed to download file' });
      }
    });
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

  getStatusSeverity(status: string): string {
    const severityMap: { [key: string]: string } = {
      'Pending': 'warning',
      'InProgress': 'info',
      'Review': 'secondary',
      'Completed': 'success',
      'Cancelled': 'danger'
    };
    return severityMap[status] || 'secondary';
  }

  goBack(): void {
    this.router.navigate(['/projects']);
  }
}
