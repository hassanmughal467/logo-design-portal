import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { BillingService } from './billing.service';
import { environment } from '@environments/environment';

describe('BillingService', () => {
  let service: BillingService;
  let httpMock: HttpTestingController;
  const apiRoot = `${environment.apiUrl}/api/billing`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [BillingService]
    });
    service = TestBed.inject(BillingService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('getBillingQueue hits queue endpoint', () => {
    service.getBillingQueue().subscribe();
    const req = httpMock.expectOne(`${apiRoot}/queue`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('getBillingQueueFiltered passes query params', () => {
    service
      .getBillingQueueFiltered({ clientId: 'c1', onlyUninvoiced: false, fromDate: '2025-01-01' })
      .subscribe();
    const req = httpMock.expectOne(
      (r) => r.url.startsWith(`${apiRoot}/queue`) && r.url.includes('clientId=c1')
    );
    expect(req.request.method).toBe('GET');
    req.flush({ orders: [], totalAmountPreview: 0 });
  });

  it('createInvoiceFromOrders posts payload', () => {
    service
      .createInvoiceFromOrders('client-1', {
        orders: [{ orderId: 'o1', price: 10 }],
        billingPeriod: 'March'
      })
      .subscribe();
    const req = httpMock.expectOne(`${apiRoot}/clients/client-1/create-invoice`);
    expect(req.request.method).toBe('POST');
    expect((req.request.body as { orders: unknown[] }).orders.length).toBe(1);
    req.flush({});
  });
});
