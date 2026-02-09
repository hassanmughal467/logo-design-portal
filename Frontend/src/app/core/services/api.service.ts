import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = `${environment.apiUrl}/api${environment.apiVersion ? '/' + environment.apiVersion : ''}`;

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
    // FormData will be handled automatically by HttpClient (browser sets Content-Type with boundary)
    return this.http.post<T>(`${this.baseUrl}/${endpoint}`, body);
  }

  put<T>(endpoint: string, body: any): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}/${endpoint}`, body);
  }

  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}/${endpoint}`);
  }

  patch<T>(endpoint: string, body: any): Observable<T> {
    return this.http.patch<T>(`${this.baseUrl}/${endpoint}`, body);
  }
}
