import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { PaymentService } from './payment.service';
import { ApiService } from './api.service';

describe('PaymentService', () => {
  let service: PaymentService;
  let api: jasmine.SpyObj<ApiService>;

  beforeEach(() => {
    api = jasmine.createSpyObj('ApiService', ['get', 'post', 'put']);
    TestBed.configureTestingModule({
      providers: [PaymentService, { provide: ApiService, useValue: api }]
    });
    service = TestBed.inject(PaymentService);
  });

  it('createPayment delegates to api post', () => {
    api.post.and.returnValue(of({} as any));
    service
      .createPayment({
        invoiceId: 'i1',
        paymentMethod: 'PayPal',
        amount: 50
      })
      .subscribe();
    expect(api.post).toHaveBeenCalledWith('payments', jasmine.any(Object));
  });

  it('getPayment uses id route', () => {
    api.get.and.returnValue(of({} as any));
    service.getPayment('p1').subscribe();
    expect(api.get).toHaveBeenCalledWith('payments/p1');
  });

  it('getPaymentsByInvoice uses nested route', () => {
    api.get.and.returnValue(of([]));
    service.getPaymentsByInvoice('inv1').subscribe();
    expect(api.get).toHaveBeenCalledWith('payments/invoice/inv1');
  });

  it('generatePaymentLink posts invoice and method', () => {
    api.post.and.returnValue(of({} as any));
    service.generatePaymentLink('i1', 'Wise').subscribe();
    expect(api.post).toHaveBeenCalledWith('payments/link', { invoiceId: 'i1', paymentMethod: 'Wise' });
  });

  it('getBankDetails uses fixed route', () => {
    api.get.and.returnValue(of({} as any));
    service.getBankDetails().subscribe();
    expect(api.get).toHaveBeenCalledWith('payments/bank-details');
  });
});
