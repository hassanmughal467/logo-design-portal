import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';

export interface CreatePaymentRequest {
  invoiceId: string;
  paymentMethod: string;
  amount: number;
  currency?: string;
  returnUrl?: string;
  cancelUrl?: string;
}

export interface PaymentResponse {
  id: string;
  invoiceId: string;
  invoiceNumber: string;
  paymentMethod: string;
  amount: number;
  currency: string;
  status: string;
  paymentLink?: string;
  transactionId?: string;
  createdAt: Date;
  completedAt?: Date;
  errorMessage?: string;
}

export interface PaymentLinkResponse {
  paymentLink: string;
  paymentId: string;
  invoiceId: string;
  invoiceNumber: string;
  amount: number;
  currency: string;
  paymentMethod: string;
  expiresAt: Date;
  qrCode?: string;
}

export interface BankDetailsResponse {
  bankName: string;
  accountHolderName: string;
  accountNumber: string;
  iban?: string;
  swift?: string;
  routingNumber?: string;
  branchAddress?: string;
  currency: string;
  reference?: string;
}

export interface ProcessPaymentRequest {
  paymentId: string;
  paypalOrderId?: string;
  wiseTransferId?: string;
  bankReference?: string;
  additionalData?: { [key: string]: string };
}

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  constructor(private apiService: ApiService) {}

  createPayment(request: CreatePaymentRequest): Observable<PaymentResponse> {
    return this.apiService.post<PaymentResponse>('payments', request);
  }

  getPayment(paymentId: string): Observable<PaymentResponse> {
    return this.apiService.get<PaymentResponse>(`payments/${paymentId}`);
  }

  getPaymentsByInvoice(invoiceId: string): Observable<PaymentResponse[]> {
    return this.apiService.get<PaymentResponse[]>(`payments/invoice/${invoiceId}`);
  }

  generatePaymentLink(invoiceId: string, paymentMethod: string): Observable<PaymentLinkResponse> {
    return this.apiService.post<PaymentLinkResponse>('payments/link', {
      invoiceId,
      paymentMethod
    });
  }

  processPayment(request: ProcessPaymentRequest): Observable<PaymentResponse> {
    return this.apiService.post<PaymentResponse>('payments/process', request);
  }

  getBankDetails(): Observable<BankDetailsResponse> {
    return this.apiService.get<BankDetailsResponse>('payments/bank-details');
  }

  verifyPayPalPayment(orderId: string, paymentId: string): Observable<{ verified: boolean }> {
    return this.apiService.post<{ verified: boolean }>('payments/verify/paypal', {
      orderId,
      paymentId
    });
  }

  updatePaymentStatus(paymentId: string, status: string, transactionId?: string): Observable<PaymentResponse> {
    return this.apiService.put<PaymentResponse>(`payments/${paymentId}/status`, {
      status,
      transactionId
    });
  }
}
