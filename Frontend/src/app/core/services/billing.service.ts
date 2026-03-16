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

@Injectable({ providedIn: 'root' })
export class BillingService {
  private readonly apiUrl = `${environment.apiUrl}/api/billing`;

  constructor(private http: HttpClient) {}

  getBillingQueue(): Observable<BillingQueueOverview[]> {
    return this.http.get<BillingQueueOverview[]>(`${this.apiUrl}/queue`);
  }

  getEligibleOrders(clientId: string): Observable<BillingEligibleOrder[]> {
    return this.http.get<BillingEligibleOrder[]>(`${this.apiUrl}/clients/${clientId}/eligible-orders`);
  }

  createInvoiceFromOrders(clientId: string, request: CreateInvoiceFromOrdersRequest): Observable<unknown> {
    return this.http.post<unknown>(`${this.apiUrl}/clients/${clientId}/create-invoice`, request);
  }
}
