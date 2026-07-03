import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { Quote, QuoteStatus } from '@shared/models/quote.model';
import { MessageService } from 'primeng/api';
import { MAX_UPLOAD_BYTES, combinedFileBytes } from '@core/constants/upload-limits';
import { DEFAULT_CLIENT_CURRENCY } from '@core/constants/currency-options';
import {
  formatCurrencyAmount,
  localeForCurrency
} from '@core/utils/currency-format';
import {
  getPreviewableQuoteAttachments,
  isPreviewableQuoteAttachment
} from '@core/utils/quote-attachment.util';
import { Subscription } from 'rxjs';

const QUOTE_ALLOWED_EXTENSIONS = [
  '.jpg', '.jpeg', '.png', '.gif', '.webp', '.pdf', '.svg', '.ai', '.eps', '.psd'
];

@Component({
  selector: 'app-quotes',
  templateUrl: './quotes.component.html',
  styleUrls: ['./quotes.component.scss']
})
export class QuotesComponent implements OnInit, OnDestroy {
  quotes: Quote[] = [];
  showCreateDialog = false;
  showRespondDialog = false;
  quoteForm: FormGroup;
  respondForm: FormGroup;
  selectedFiles: File[] = [];
  selectedQuote: Quote | null = null;
  selectedStatusFilter: QuoteStatus | null = null;

  isClient = false;
  isAdmin = false;

  statusOptions = Object.values(QuoteStatus).map(s => ({ label: s, value: s }));

  currencyCode = DEFAULT_CLIENT_CURRENCY;
  currencyLocale = localeForCurrency(DEFAULT_CLIENT_CURRENCY);

  showImagePreviewDialog = false;
  imagePreviewUrl: string | null = null;
  imagePreviewLoading = false;
  imagePreviewFiles: string[] = [];
  imagePreviewIndex = 0;
  imagePreviewQuoteId = '';
  private previewSub?: Subscription;

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private auth: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private messageService: MessageService
  ) {
    this.quoteForm = this.fb.group({
      logoName: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', [Validators.required, Validators.maxLength(2000)]],
      requestedBudget: [null]
    });

    this.respondForm = this.fb.group({
      adminQuotedPrice: [null, [Validators.required, Validators.min(0.01)]],
      adminNotes: ['']
    });
  }

  ngOnInit(): void {
    const role = this.auth.getCurrentUser()?.role;
    this.isClient = role === 'Client';
    this.isAdmin = role === 'Admin' || role === 'SuperAdmin';

    const openCreateRaw = this.route.snapshot.queryParamMap.get('openCreate') ?? '';
    const openCreate = openCreateRaw.toLowerCase();
    if (this.isClient && ['1', 'true', 'yes'].includes(openCreate)) {
      this.showCreateDialog = true;
    }

    this.loadQuotes();
    this.loadClientCurrency();
  }

  ngOnDestroy(): void {
    this.closeImagePreview();
  }

  private loadClientCurrency(): void {
    if (!this.isClient) return;
    const userId = this.auth.getCurrentUser()?.id;
    if (!userId) return;

    this.api.get<{ clientProfile?: { currencyCode?: string } }>(`users/${userId}`).subscribe({
      next: (user) => {
        const code = user?.clientProfile?.currencyCode;
        if (code) {
          this.setCurrency(code);
        }
      }
    });
  }

  private setCurrency(code: string): void {
    this.currencyCode = code;
    this.currencyLocale = localeForCurrency(code);
  }

  loadQuotes(): void {
    const statusQuery = this.selectedStatusFilter ? `?status=${this.selectedStatusFilter}` : '';
    this.api.get<Quote[]>(`quotes${statusQuery}`).subscribe({
      next: data => {
        this.quotes = data ?? [];
        if (this.isClient && data?.length && data[0].currencyCode) {
          this.setCurrency(data[0].currencyCode!);
        }
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load quotes' });
      }
    });
  }

  createQuote(): void {
    if (this.quoteForm.invalid) return;
    const payload = this.quoteForm.value;
    const formData = new FormData();
    formData.append('quote', JSON.stringify(payload));
    this.selectedFiles.forEach(f => formData.append('files', f, f.name));

    this.api.post<Quote>('quotes', formData).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Created', detail: 'Quote submitted successfully' });
        this.showCreateDialog = false;
        this.quoteForm.reset();
        this.selectedFiles = [];
        this.loadQuotes();
      },
      error: (error) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error?.error || 'Failed to submit quote' });
      }
    });
  }

  openRespondDialog(quote: Quote): void {
    this.closeImagePreview();
    this.selectedQuote = quote;
    if (quote.currencyCode) {
      this.setCurrency(quote.currencyCode);
    }
    this.respondForm.patchValue({
      adminQuotedPrice: quote.adminQuotedPrice ?? null,
      adminNotes: quote.adminNotes ?? ''
    });
    this.showRespondDialog = true;
  }

  respondToQuote(): void {
    if (!this.selectedQuote || this.respondForm.invalid) return;
    this.api.post(`quotes/${this.selectedQuote.id}/respond`, this.respondForm.value).subscribe({
      next: () => {
        this.showRespondDialog = false;
        this.messageService.add({ severity: 'success', summary: 'Sent', detail: 'Quote response sent to client' });
        this.loadQuotes();
      },
      error: (error) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error?.error || 'Failed to respond to quote' });
      }
    });
  }

  proceedWithQuote(quote: Quote): void {
    this.router.navigate(['/orders'], {
      queryParams: {
        source: 'quote',
        quoteId: quote.id,
        logoName: quote.logoName,
        description: quote.description,
        price: quote.adminQuotedPrice ?? 0
      }
    });
  }

  rejectQuote(quote: Quote): void {
    this.api.post(`quotes/${quote.id}/reject`, {}).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Updated', detail: 'Quote rejected' });
        this.loadQuotes();
      },
      error: (error) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error?.error || 'Failed to reject quote' });
      }
    });
  }

  onQuoteFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = input.files ? Array.from(input.files) : [];
    if (files.length > 0) {
      this.addQuoteFiles(files);
    }
    input.value = '';
  }

  addQuoteFiles(files: File[]): void {
    const invalidType = files.filter(f => !this.isAllowedQuoteFile(f));
    if (invalidType.length > 0) {
      this.messageService.add({
        severity: 'error',
        summary: 'Invalid file type',
        detail: 'Allowed: JPG, PNG, GIF, WEBP, SVG, PDF, AI, EPS, PSD'
      });
    }

    const valid = files.filter(f => this.isAllowedQuoteFile(f));
    const tooBig = valid.filter(f => f.size > MAX_UPLOAD_BYTES);
    if (tooBig.length > 0) {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Each file must be 500MB or smaller.' });
    }

    const ok = valid
      .filter(f => f.size <= MAX_UPLOAD_BYTES)
      .filter(f => !this.selectedFiles.some(existing => existing.name === f.name && existing.size === f.size));

    const merged = [...this.selectedFiles, ...ok];
    if (combinedFileBytes(merged) > MAX_UPLOAD_BYTES) {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Combined file size cannot exceed 500MB.' });
      return;
    }

    this.selectedFiles = merged;
  }

  removeQuoteFile(index: number): void {
    this.selectedFiles.splice(index, 1);
  }

  clearQuoteFiles(): void {
    this.selectedFiles = [];
  }

  isPreviewableAttachment(fileName: string): boolean {
    return isPreviewableQuoteAttachment(fileName);
  }

  openQuoteAttachmentPreview(quoteId: string, fileName: string): void {
    if (!this.selectedQuote || !this.isPreviewableAttachment(fileName)) return;

    const imageFiles = getPreviewableQuoteAttachments(this.selectedQuote.attachments);
    if (imageFiles.length === 0) return;

    this.imagePreviewQuoteId = quoteId;
    this.imagePreviewFiles = imageFiles;
    this.imagePreviewIndex = Math.max(0, imageFiles.indexOf(fileName));
    this.showImagePreviewDialog = true;
    this.loadCurrentImagePreview();
  }

  closeImagePreview(): void {
    this.showImagePreviewDialog = false;
    this.imagePreviewFiles = [];
    this.imagePreviewIndex = 0;
    this.imagePreviewQuoteId = '';
    this.previewSub?.unsubscribe();
    this.previewSub = undefined;
    if (this.imagePreviewUrl) {
      window.URL.revokeObjectURL(this.imagePreviewUrl);
      this.imagePreviewUrl = null;
    }
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

  downloadCurrentImagePreview(): void {
    const name = this.imagePreviewFiles[this.imagePreviewIndex];
    if (name && this.imagePreviewQuoteId) {
      this.downloadQuoteAttachment(this.imagePreviewQuoteId, name);
    }
  }

  private loadCurrentImagePreview(): void {
    const fileName = this.imagePreviewFiles[this.imagePreviewIndex];
    if (!fileName || !this.imagePreviewQuoteId) return;

    this.imagePreviewLoading = true;
    if (this.imagePreviewUrl) {
      window.URL.revokeObjectURL(this.imagePreviewUrl);
      this.imagePreviewUrl = null;
    }
    this.previewSub?.unsubscribe();

    this.previewSub = this.api
      .getBlob(`quotes/${this.imagePreviewQuoteId}/attachments/${encodeURIComponent(fileName)}`)
      .subscribe({
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

  downloadQuoteAttachment(quoteId: string, storedFileName: string): void {
    this.api.getBlob(`quotes/${quoteId}/attachments/${encodeURIComponent(storedFileName)}`).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = storedFileName;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to download attachment' });
      }
    });
  }

  formatPrice(value: number | null | undefined, currencyCode?: string | null): string {
    if (value == null) return '-';
    return formatCurrencyAmount(value, currencyCode ?? this.currencyCode);
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  private isAllowedQuoteFile(file: File): boolean {
    const ext = '.' + (file.name.split('.').pop() || '').toLowerCase();
    return QUOTE_ALLOWED_EXTENSIONS.includes(ext) || file.type.startsWith('image/');
  }
}
