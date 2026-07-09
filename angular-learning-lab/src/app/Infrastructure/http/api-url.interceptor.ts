import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { API_BASE_URL } from '../api.config';

const absoluteUrlPattern = /^https?:\/\//i;

export const apiUrlInterceptor: HttpInterceptorFn = (request, next) => {
  if (absoluteUrlPattern.test(request.url) || !request.url.startsWith('/api')) {
    return next(request);
  }

  const apiBaseUrl = inject(API_BASE_URL).replace(/\/$/, '');
  const apiRequest = request.clone({
    url: `${apiBaseUrl}${request.url}`
  });

  return next(apiRequest);
};

