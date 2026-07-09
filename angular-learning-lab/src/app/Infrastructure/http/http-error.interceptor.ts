import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export interface ApiError {
  status: number;
  message: string;
  details?: unknown;
}

export const httpErrorInterceptor: HttpInterceptorFn = (request, next) => {
  return next(request).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        return throwError(() => toApiError(error));
      }

      return throwError(() => error);
    })
  );
};

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

