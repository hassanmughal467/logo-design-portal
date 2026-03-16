import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface FinancialInsight {
  message: string;
  severity: string;
  icon?: string;
}

export interface FinancialInsightsResponse {
  insights: FinancialInsight[];
}

@Injectable({
  providedIn: 'root'
})
export class ClientFinancialInsightsService {
  private readonly endpoint = 'client/financial-insights';

  constructor(private apiService: ApiService) {}

  getFinancialInsights(): Observable<FinancialInsightsResponse> {
    return this.apiService.get<FinancialInsightsResponse>(this.endpoint);
  }
}
