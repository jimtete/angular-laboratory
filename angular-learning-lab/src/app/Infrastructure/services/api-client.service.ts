import { HttpClient, HttpContext, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface ApiRequestOptions {
  headers?: HttpHeaders | Record<string, string | string[]>;
  params?: HttpParams | Record<string, string | number | boolean | readonly (string | number | boolean)[]>;
  context?: HttpContext;
}

@Injectable({
  providedIn: 'root'
})
export class ApiClient {
  private readonly http = inject(HttpClient);

  get<TResponse>(url: string, options?: ApiRequestOptions): Observable<TResponse> {
    return this.http.get<TResponse>(url, options);
  }

  post<TResponse, TBody = unknown>(
    url: string,
    body: TBody,
    options?: ApiRequestOptions
  ): Observable<TResponse> {
    return this.http.post<TResponse>(url, body, options);
  }

  put<TResponse, TBody = unknown>(
    url: string,
    body: TBody,
    options?: ApiRequestOptions
  ): Observable<TResponse> {
    return this.http.put<TResponse>(url, body, options);
  }

  patch<TResponse, TBody = unknown>(
    url: string,
    body: TBody,
    options?: ApiRequestOptions
  ): Observable<TResponse> {
    return this.http.patch<TResponse>(url, body, options);
  }

  delete<TResponse>(url: string, options?: ApiRequestOptions): Observable<TResponse> {
    return this.http.delete<TResponse>(url, options);
  }
}

