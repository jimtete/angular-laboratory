import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';

import { API_BASE_URL } from '../api.config';
import { ApiLoadingService } from '../services/api-loading.service';

export const apiLoadingInterceptor: HttpInterceptorFn = (request, next) => {
  if (!isApiRequest(request.url)) {
    return next(request);
  }

  const apiLoadingService = inject(ApiLoadingService);
  apiLoadingService.startRequest();

  return next(request).pipe(
    finalize(() => {
      apiLoadingService.finishRequest();
    }),
  );
};

function isApiRequest(url: string): boolean {
  if (url.startsWith('/api')) {
    return true;
  }

  const apiBaseUrl = inject(API_BASE_URL).replace(/\/$/, '');

  return url.startsWith(`${apiBaseUrl}/api`);
}
