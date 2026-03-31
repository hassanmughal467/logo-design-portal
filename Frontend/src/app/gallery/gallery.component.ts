import { Component, OnInit, OnDestroy } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { GalleryItem } from '@shared/models/gallery.model';
import { trackById } from '@core/utils/track-by.utils';

@Component({
  selector: 'app-gallery',
  templateUrl: './gallery.component.html',
  styleUrls: ['./gallery.component.scss']
})
export class GalleryComponent implements OnInit, OnDestroy {
  readonly trackById = trackById;

  galleryItems: GalleryItem[] = [];
  selectedItem: GalleryItem | null = null;
  showPreviewDialog = false;

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    if (!this.authService.hasRole('Client')) {
      this.messageService.add({
        severity: 'error',
        summary: 'Access Denied',
        detail: 'Only clients can access the gallery'
      });
      return;
    }
    this.loadGallery();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadGallery(): void {
    this.apiService.get<GalleryItem[]>('gallery/my-gallery')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (items) => {
          this.galleryItems = items;
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to load gallery'
          });
          this.galleryItems = [];
        }
      });
  }

  previewItem(item: GalleryItem): void {
    this.selectedItem = item;
    this.showPreviewDialog = true;
  }

  downloadFile(item: GalleryItem): void {
    const fileId = item.fileId || item.id;
    this.apiService.getBlob(`files/${fileId}/download`).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = item.originalFileName || item.fileName || `file-${fileId}`;
        link.click();
        URL.revokeObjectURL(url);
        this.messageService.add({ severity: 'success', summary: 'Download', detail: 'File downloaded successfully' });
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Failed to download file' });
      }
    });
  }

  getFileIcon(format?: string): string {
    if (!format) return 'pi-file';
    const formatLower = format.toLowerCase();
    if (['png', 'jpg', 'jpeg', 'gif'].includes(formatLower)) return 'pi-image';
    if (['pdf'].includes(formatLower)) return 'pi-file-pdf';
    if (['svg', 'ai', 'eps'].includes(formatLower)) return 'pi-file-edit';
    return 'pi-file';
  }
}
