import { Component, OnInit, ViewChild } from '@angular/core';
import { FileUpload } from 'primeng/fileupload';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { Quote, QuoteStatus } from '@shared/models/quote.model';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-quotes',
  templateUrl: './quotes.component.html',
  styleUrls: ['./quotes.component.scss']
})
export class QuotesComponent implements OnInit {
  @ViewChild('quoteFileUpload') private quoteFileUpload?: FileUpload;

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

    // If opened from the dashboard, auto-open the create dialog (client-only).
    const openCreateRaw = this.route.snapshot.queryParamMap.get('openCreate') ?? '';
    const openCreate = openCreateRaw.toLowerCase();
    if (this.isClient && ['1', 'true', 'yes'].includes(openCreate)) {
      this.showCreateDialog = true;
    }

    this.loadQuotes();
  }

  loadQuotes(): void {
    const statusQuery = this.selectedStatusFilter ? `?status=${this.selectedStatusFilter}` : '';
    this.api.get<Quote[]>(`quotes${statusQuery}`).subscribe({
      next: data => {
        this.quotes = data ?? [];
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
    this.selectedFiles.forEach(f => formData.append('files', f));

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
    this.selectedQuote = quote;
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

  onFileSelect(event: any): void {
    const files: File[] = event.files ? Array.from(event.files) : [];
    const deduped = files.filter(file => !this.selectedFiles.some(f => f.name === file.name && f.size === file.size));
    this.selectedFiles = [...this.selectedFiles, ...deduped];
    this.quoteFileUpload?.clear();
  }
}
