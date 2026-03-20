import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface BillingQueueOverview {
  clientId: string;
  clientName: string;
  companyName: string;
  uninvoicedOrderCount: number;
  totalPendingAmount: number;
}

export interface BillingEligibleOrder {
  orderId: string;
  orderNumber: string;
  title: string;
  price: number;
  completedDate?: string;
}

export interface CreateInvoiceOrderItem {
  orderId: string;
  price: number;
}

export interface CreateInvoiceFromOrdersRequest {
  orderIds?: string[];
  orders?: CreateInvoiceOrderItem[];
  billingPeriod?: string;
}

export interface BillingQueueResult {
  orders: BillingEligibleOrder[];
  totalAmountPreview: number;
}

export interface BillingQueueFilters {
  clientId?: string;
  fromDate?: string;
  toDate?: string;
  onlyUninvoiced?: boolean;
}

export interface GenerateFlexibleInvoiceRequest {
  clientId: string;
  fromDate?: string;
  toDate?: string;
  selectedOrderIds?: string[];
  includeUninvoicedOnly: boolean;
  notes?: string;
}

@Injectable({ providedIn: 'root' })
export class BillingService {
  private readonly apiUrl = `${environment.apiUrl}/api/billing`;

  constructor(private http: HttpClient) {}

  getBillingQueue(): Observable<BillingQueueOverview[]> {
    return this.http.get<BillingQueueOverview[]>(`${this.apiUrl}/queue`);
  }

  getBillingQueueFiltered(filters: BillingQueueFilters): Observable<BillingQueueResult> {
    const params = new URLSearchParams();
    if (filters.clientId) params.set('clientId', filters.clientId);
    if (filters.fromDate) params.set('fromDate', filters.fromDate);
    if (filters.toDate) params.set('toDate', filters.toDate);
    params.set('onlyUninvoiced', String(filters.onlyUninvoiced ?? true));
    return this.http.get<BillingQueueResult>(`${this.apiUrl}/queue?${params.toString()}`);
  }

  getEligibleOrders(clientId: string): Observable<BillingEligibleOrder[]> {
    return this.http.get<BillingEligibleOrder[]>(`${this.apiUrl}/clients/${clientId}/eligible-orders`);
  }

  createInvoiceFromOrders(clientId: string, request: CreateInvoiceFromOrdersRequest): Observable<unknown> {
    return this.http.post<unknown>(`${this.apiUrl}/clients/${clientId}/create-invoice`, request);
  }

  generateFlexibleInvoice(request: GenerateFlexibleInvoiceRequest): Observable<unknown> {
    return this.http.post<unknown>(`${environment.apiUrl}/api/invoices/generate-flexible`, request);
  }
}
