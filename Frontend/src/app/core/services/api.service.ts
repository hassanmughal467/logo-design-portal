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
    // Remove leading slash from endpoint if present to avoid double slashes
    const cleanEndpoint = endpoint.startsWith('/') ? endpoint.substring(1) : endpoint;
    const fullUrl = `${this.baseUrl}/${cleanEndpoint}`;
    console.log('API GET request:', fullUrl);
    return this.http.get<T>(fullUrl);
  }

  post<T>(endpoint: string, body: any): Observable<T> {
    // Check if body is FormData - if so, let browser set Content-Type with boundary
    // Otherwise, explicitly set Content-Type to application/json
    if (body instanceof FormData) {
      // FormData will be handled automatically by HttpClient (browser sets Content-Type with boundary)
      return this.http.post<T>(`${this.baseUrl}/${endpoint}`, body);
    } else {
      // For JSON, explicitly set Content-Type header
      const headers = new HttpHeaders({
        'Content-Type': 'application/json'
      });
      return this.http.post<T>(`${this.baseUrl}/${endpoint}`, body, { headers });
    }
  }

  put<T>(endpoint: string, body: any): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}/${endpoint}`, body);
  }

  delete<T>(endpoint: string, body?: any): Observable<T> {
    const cleanEndpoint = endpoint.startsWith('/') ? endpoint.substring(1) : endpoint;
    const fullUrl = `${this.baseUrl}/${cleanEndpoint}`;
    if (body) {
      const headers = new HttpHeaders({
        'Content-Type': 'application/json'
      });
      return this.http.delete<T>(fullUrl, { headers, body });
    }
    return this.http.delete<T>(fullUrl);
  }

  patch<T>(endpoint: string, body: any): Observable<T> {
    return this.http.patch<T>(`${this.baseUrl}/${endpoint}`, body);
  }

  // Download file as blob (for file downloads with authentication)
  getBlob(endpoint: string): Observable<Blob> {
    const cleanEndpoint = endpoint.startsWith('/') ? endpoint.substring(1) : endpoint;
    const fullUrl = `${this.baseUrl}/${cleanEndpoint}`;
    return this.http.get(fullUrl, { responseType: 'blob' });
  }
}
