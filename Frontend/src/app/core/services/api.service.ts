import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly baseUrl = `${environment.apiUrl}/api${environment.apiVersion ? '/' + environment.apiVersion : ''}`;

  getBaseUrl(): string {
    return this.baseUrl;
  }

  constructor(private http: HttpClient) {}

  // Generic HTTP methods
  get<T>(endpoint: string): Observable<T> {
    const fullUrl = this.buildUrl(endpoint);
    return this.http.get<T>(fullUrl);
  }

  post<T>(endpoint: string, body: any): Observable<T> {
    const fullUrl = this.buildUrl(endpoint);
    if (body instanceof FormData) {
      return this.http.post<T>(fullUrl, body);
    } else {
      // For JSON, explicitly set Content-Type header
      const headers = new HttpHeaders({
        'Content-Type': 'application/json'
      });
      return this.http.post<T>(fullUrl, body, { headers });
    }
  }

  put<T>(endpoint: string, body: any): Observable<T> {
    return this.http.put<T>(this.buildUrl(endpoint), body);
  }

  delete<T>(endpoint: string, body?: any): Observable<T> {
    const fullUrl = this.buildUrl(endpoint);
    if (body) {
      const headers = new HttpHeaders({
        'Content-Type': 'application/json'
      });
      return this.http.delete<T>(fullUrl, { headers, body });
    }
    return this.http.delete<T>(fullUrl);
  }

  patch<T>(endpoint: string, body: any): Observable<T> {
    return this.http.patch<T>(this.buildUrl(endpoint), body);
  }

  getBlob(endpoint: string): Observable<Blob> {
    return this.http.get(this.buildUrl(endpoint), { responseType: 'blob' });
  }

  private buildUrl(endpoint: string): string {
    let clean = endpoint.startsWith('/') ? endpoint.substring(1) : endpoint;
    if (clean.startsWith('api/')) clean = clean.substring(4);
    return `${this.baseUrl}/${clean}`;
  }

  /** Extract items from paginated API response (handles both { data: { items } } and legacy { items } formats) */
  static extractItems<T>(response: any): T[] {
    if (!response) return [];
    const items = response?.data?.items ?? response?.items;
    return Array.isArray(items) ? items : [];
  }

  /** Extract pagination meta from API response */
  static extractPagedMeta(response: any): { total: number; page: number; pageSize: number; totalPages: number } {
    const data = response?.data ?? response;
    const meta = response?.meta ?? {};
    return {
      total: data?.total ?? meta?.total ?? 0,
      page: data?.page ?? meta?.page ?? 1,
      pageSize: data?.pageSize ?? meta?.pageSize ?? 20,
      totalPages: meta?.totalPages ?? (data?.pageSize > 0 && data?.total != null
        ? Math.ceil(data.total / data.pageSize) : 0)
    };
  }
}
