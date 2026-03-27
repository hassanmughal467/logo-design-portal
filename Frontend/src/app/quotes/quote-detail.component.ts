import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { Quote } from '@shared/models/quote.model';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-quote-detail',
  templateUrl: './quote-detail.component.html',
  styleUrls: ['./quote-detail.component.scss']
})
export class QuoteDetailComponent implements OnInit {
  quote: Quote | null = null;
  loading = false;
  isClient = false;
  quoteId = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private api: ApiService,
    private auth: AuthService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.isClient = this.auth.getCurrentUser()?.role === 'Client';
    this.quoteId = this.route.snapshot.paramMap.get('id') ?? '';
    if (this.quoteId) this.loadQuote();
  }

  loadQuote(): void {
    this.loading = true;
    this.api.get<Quote>(`quotes/${this.quoteId}`).subscribe({
      next: (data) => {
        this.quote = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load quote details' });
        this.router.navigate(['/quotes']);
      }
    });
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
