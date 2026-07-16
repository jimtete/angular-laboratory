import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { TokenStorageService } from '../services/token-storage.service';

export interface ApiError {
  status: number;
  message: string;
  details?: unknown;
}

export const httpErrorInterceptor: HttpInterceptorFn = (request, next) => {
  const router = inject(Router);
  const tokenStorage = inject(TokenStorageService);

  return next(request).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        if (error.status === 401 && !isAuthenticationRequest(request.url)) {
          tokenStorage.clear();
          void router.navigate(['/home']);
        }

        return throwError(() => toApiError(error));
      }

      return throwError(() => error);
    })
  );
};

function isAuthenticationRequest(url: string): boolean {
  return url.includes('/api/auth/login') || url.includes('/api/auth/register');
}

function toApiError(error: HttpErrorResponse): ApiError {
  return {
    status: error.status,
    message: getErrorMessage(error),
    details: error.error
  };
}

function getErrorMessage(error: HttpErrorResponse): string {
  if (typeof error.error === 'string' && error.error.trim()) {
    return error.error;
  }

  if (hasMessage(error.error)) {
    return error.error.message;
  }

  return error.message || 'The API request failed.';
}

function hasMessage(value: unknown): value is { message: string } {
  return typeof value === 'object'
    && value !== null
    && 'message' in value
    && typeof value.message === 'string';
}
