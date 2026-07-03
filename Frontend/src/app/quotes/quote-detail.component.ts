import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import {
  getPreviewableQuoteAttachments,
  isPreviewableQuoteAttachment
} from '@core/utils/quote-attachment.util';
import { DEFAULT_CLIENT_CURRENCY } from '@core/constants/currency-options';
import { formatCurrencyAmount } from '@core/utils/currency-format';
import { Quote } from '@shared/models/quote.model';
import { MessageService } from 'primeng/api';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-quote-detail',
  templateUrl: './quote-detail.component.html',
  styleUrls: ['./quote-detail.component.scss']
})
export class QuoteDetailComponent implements OnInit, OnDestroy {
  quote: Quote | null = null;
  loading = false;
  isClient = false;
  canViewAttachments = false;
  quoteId = '';

  attachmentThumbnails: Record<string, string> = {};
  showImagePreviewDialog = false;
  imagePreviewUrl: string | null = null;
  imagePreviewLoading = false;
  imagePreviewFiles: string[] = [];
  imagePreviewIndex = 0;

  private previewSub?: Subscription;
  private thumbnailSubs: Subscription[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private api: ApiService,
    private auth: AuthService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    const role = this.auth.getCurrentUser()?.role;
    this.isClient = role === 'Client';
    this.canViewAttachments = role === 'Client' || role === 'Admin' || role === 'SuperAdmin';
    this.quoteId = this.route.snapshot.paramMap.get('id') ?? '';
    if (this.quoteId) this.loadQuote();
  }

  ngOnDestroy(): void {
    this.closeImagePreview();
    this.revokeAllThumbnails();
    this.thumbnailSubs.forEach(s => s.unsubscribe());
  }

  loadQuote(): void {
    this.loading = true;
    this.revokeAllThumbnails();
    this.api.get<Quote>(`quotes/${this.quoteId}`).subscribe({
      next: (data) => {
        this.quote = data;
        this.loading = false;
        this.loadAttachmentThumbnails();
      },
      error: () => {
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load quote details' });
        this.router.navigate(['/quotes']);
      }
    });
  }

  formatPrice(value: number | null | undefined): string {
    if (value == null) return '-';
    const code = this.quote?.currencyCode ?? DEFAULT_CLIENT_CURRENCY;
    return formatCurrencyAmount(value, code);
  }

  isPreviewable(fileName: string): boolean {
    return isPreviewableQuoteAttachment(fileName);
  }

  previewableAttachments(): string[] {
    return getPreviewableQuoteAttachments(this.quote?.attachments);
  }

  openAttachmentPreview(fileName: string): void {
    if (!this.quote || !this.isPreviewable(fileName)) return;

    const imageFiles = this.previewableAttachments();
    if (imageFiles.length === 0) {
      this.messageService.add({ severity: 'warn', summary: 'Preview unavailable', detail: 'No previewable images.' });
      return;
    }

    const selectedIndex = imageFiles.indexOf(fileName);
    this.imagePreviewFiles = imageFiles;
    this.imagePreviewIndex = selectedIndex >= 0 ? selectedIndex : 0;
    this.showImagePreviewDialog = true;
    this.loadCurrentImagePreview();
  }

  closeImagePreview(): void {
    this.showImagePreviewDialog = false;
    this.imagePreviewFiles = [];
    this.imagePreviewIndex = 0;
    this.previewSub?.unsubscribe();
    this.previewSub = undefined;
    this.cleanupImagePreviewUrl();
  }

  get imagePreviewDialogHeader(): string {
    return this.imagePreviewFiles[this.imagePreviewIndex] ?? 'Image preview';
  }

  get imagePreviewGalleryLength(): number {
    return this.imagePreviewFiles.length;
  }

  previousImagePreview(): void {
    const len = this.imagePreviewGalleryLength;
    if (len <= 1) return;
    this.imagePreviewIndex = (this.imagePreviewIndex - 1 + len) % len;
    this.loadCurrentImagePreview();
  }

  nextImagePreview(): void {
    const len = this.imagePreviewGalleryLength;
    if (len <= 1) return;
    this.imagePreviewIndex = (this.imagePreviewIndex + 1) % len;
    this.loadCurrentImagePreview();
  }

  downloadAttachment(storedFileName: string): void {
    if (!this.quote) return;
    this.fetchAttachmentBlob(storedFileName).subscribe({
      next: (blob) => this.saveBlob(blob, storedFileName),
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to download attachment' });
      }
    });
  }

  downloadCurrentImagePreview(): void {
    const name = this.imagePreviewFiles[this.imagePreviewIndex];
    if (name) this.downloadAttachment(name);
  }

  private loadAttachmentThumbnails(): void {
    if (!this.quote?.attachments?.length) return;

    for (const fileName of this.previewableAttachments()) {
      const sub = this.fetchAttachmentBlob(fileName).subscribe({
        next: (blob) => {
          if (blob.type.startsWith('image/') || isPreviewableQuoteAttachment(fileName)) {
            this.attachmentThumbnails[fileName] = window.URL.createObjectURL(blob);
          }
        },
        error: () => { /* thumbnail optional */ }
      });
      this.thumbnailSubs.push(sub);
    }
  }

  private loadCurrentImagePreview(): void {
    if (!this.quote) return;

    const fileName = this.imagePreviewFiles[this.imagePreviewIndex];
    if (!fileName) return;

    this.imagePreviewLoading = true;
    this.cleanupImagePreviewUrl();
    this.previewSub?.unsubscribe();

    this.previewSub = this.fetchAttachmentBlob(fileName).subscribe({
      next: (blob) => {
        this.imagePreviewUrl = window.URL.createObjectURL(blob);
        this.imagePreviewLoading = false;
      },
      error: () => {
        this.imagePreviewLoading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load image preview' });
      }
    });
  }

  private fetchAttachmentBlob(storedFileName: string) {
    return this.api.getBlob(
      `quotes/${this.quote!.id}/attachments/${encodeURIComponent(storedFileName)}`
    );
  }

  private saveBlob(blob: Blob, fileName: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  private cleanupImagePreviewUrl(): void {
    if (this.imagePreviewUrl) {
      window.URL.revokeObjectURL(this.imagePreviewUrl);
      this.imagePreviewUrl = null;
    }
  }

  private revokeAllThumbnails(): void {
    Object.values(this.attachmentThumbnails).forEach(url => window.URL.revokeObjectURL(url));
    this.attachmentThumbnails = {};
  }

  proceed(): void {
    if (!this.quote) return;
    this.router.navigate(['/orders'], {
      queryParams: {
        source: 'quote',
        quoteId: this.quote.id,
        logoName: this.quote.logoName,
        description: this.quote.description,
        price: this.quote.adminQuotedPrice ?? 0
      }
    });
  }

  cancelQuote(): void {
    if (!this.quote) return;
    this.api.post(`quotes/${this.quote.id}/reject`, {}).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Cancelled', detail: 'Quote cancelled' });
        this.loadQuote();
      },
      error: (error) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error?.error || 'Failed to cancel quote' });
      }
    });
  }
}
