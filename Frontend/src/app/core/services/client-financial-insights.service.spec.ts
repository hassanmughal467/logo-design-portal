import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { ClientFinancialInsightsService } from './client-financial-insights.service';
import { ApiService } from './api.service';

describe('ClientFinancialInsightsService', () => {
  let service: ClientFinancialInsightsService;
  let api: jasmine.SpyObj<ApiService>;

  beforeEach(() => {
    api = jasmine.createSpyObj('ApiService', ['get']);
    TestBed.configureTestingModule({
      providers: [ClientFinancialInsightsService, { provide: ApiService, useValue: api }]
    });
    service = TestBed.inject(ClientFinancialInsightsService);
  });

  it('getFinancialInsights calls api route', (done) => {
    api.get.and.returnValue(of({ insights: [] }));
    service.getFinancialInsights().subscribe((res) => {
      expect(res.insights).toEqual([]);
      expect(api.get).toHaveBeenCalledWith('client/financial-insights');
      done();
    });
  });
});
