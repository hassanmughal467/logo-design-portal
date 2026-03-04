import { Component, OnInit, Input, Output, EventEmitter, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { PaymentService, PaymentLinkResponse, BankDetailsResponse } from '@core/services/payment.service';
import { MessageService } from 'primeng/api';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-payment',
  templateUrl: './payment.component.html',
  styleUrls: ['./payment.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PaymentComponent implements OnInit {
  @Input() invoiceId: string = '';
  @Input() invoiceNumber: string = '';
  @Input() amount: number = 0;
  @Input() currency: string = 'USD';
  @Output() paymentComplete = new EventEmitter<void>();
  @Output() paymentCancelled = new EventEmitter<void>();

  selectedPaymentMethod: string = '';
  paymentMethods = [
    { label: 'PayPal', value: 'PayPal', icon: 'pi pi-paypal' },
    { label: 'Wise', value: 'Wise', icon: 'pi pi-send' },
    { label: 'Bank Transfer', value: 'BankTransfer', icon: 'pi pi-building' }
  ];

  showBankDetails = false;
  bankDetails: BankDetailsResponse | null = null;
  paymentLink: string = '';
  showPaymentLink = false;
  loading = false;
  processingPayment = false;

  constructor(
    private paymentService: PaymentService,
    private messageService: MessageService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Auto-select first payment method
    if (this.paymentMethods.length > 0) {
      this.selectedPaymentMethod = this.paymentMethods[0].value;
    }
  }

  async onPaymentMethodChange(): Promise<void> {
    this.showBankDetails = false;
    this.showPaymentLink = false;
    this.paymentLink = '';
    this.cdr.markForCheck();

    if (this.selectedPaymentMethod === 'BankTransfer') {
      await this.loadBankDetails();
    }
  }

  async loadBankDetails(): Promise<void> {
    try {
      this.loading = true;
      this.cdr.markForCheck();
      this.bankDetails = await firstValueFrom(this.paymentService.getBankDetails());
      this.showBankDetails = true;
      if (this.bankDetails) {
        this.bankDetails.reference = this.invoiceNumber;
      }
    } catch (error: any) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: error.error?.error || 'Failed to load bank details'
      });
    } finally {
      this.loading = false;
      this.cdr.markForCheck();
    }
  }

  async generatePaymentLink(): Promise<void> {
    if (!this.selectedPaymentMethod) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select a payment method'
      });
      return;
    }

    try {
      this.loading = true;
      this.cdr.markForCheck();

      const response = await firstValueFrom(
        this.paymentService.generatePaymentLink(this.invoiceId, this.selectedPaymentMethod)
      );

      this.paymentLink = response.paymentLink;
      this.showPaymentLink = true;

      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Payment link generated successfully'
      });
    } catch (error: any) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: error.error?.error || 'Failed to generate payment link'
      });
    } finally {
      this.loading = false;
      this.cdr.markForCheck();
    }
  }

  async processPayment(): Promise<void> {
    if (!this.selectedPaymentMethod) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select a payment method'
      });
      return;
    }

    try {
      this.processingPayment = true;
      this.cdr.markForCheck();

      const request = {
        invoiceId: this.invoiceId,
        paymentMethod: this.selectedPaymentMethod,
        amount: this.amount,
        currency: this.currency,
        returnUrl: `${window.location.origin}/invoices?payment=success`,
        cancelUrl: `${window.location.origin}/invoices?payment=cancelled`
      };

      const payment = await firstValueFrom(this.paymentService.createPayment(request));

      if (payment.paymentLink) {
        // Redirect to payment gateway
        if (this.selectedPaymentMethod === 'PayPal') {
          window.location.href = payment.paymentLink;
        } else if (this.selectedPaymentMethod === 'Wise') {
          window.open(payment.paymentLink, '_blank');
        } else {
          // For bank transfer, show bank details
          await this.loadBankDetails();
          this.showBankDetails = true;
        }
      } else {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: payment.errorMessage || 'Failed to create payment'
        });
      }
    } catch (error: any) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: error.error?.error || 'Failed to process payment'
      });
    } finally {
      this.processingPayment = false;
      this.cdr.markForCheck();
    }
  }

  copyPaymentLink(): void {
    if (this.paymentLink) {
      navigator.clipboard.writeText(this.paymentLink).then(() => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Payment link copied to clipboard'
        });
      });
    }
  }

  copyBankDetails(): void {
    if (!this.bankDetails) return;

    const details = [
      `Bank Name: ${this.bankDetails.bankName}`,
      `Account Holder: ${this.bankDetails.accountHolderName}`,
      `Account Number: ${this.bankDetails.accountNumber}`,
      this.bankDetails.iban ? `IBAN: ${this.bankDetails.iban}` : '',
      this.bankDetails.swift ? `SWIFT: ${this.bankDetails.swift}` : '',
      this.bankDetails.routingNumber ? `Routing Number: ${this.bankDetails.routingNumber}` : '',
      `Reference: ${this.bankDetails.reference || this.invoiceNumber}`
    ].filter(Boolean).join('\n');

    navigator.clipboard.writeText(details).then(() => {
      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Bank details copied to clipboard'
      });
    });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: this.currency
    }).format(amount);
  }

  cancel(): void {
    this.paymentCancelled.emit();
  }
}
