import { Component, ElementRef, computed, inject, signal, viewChild } from '@angular/core';
import { Router } from '@angular/router';

import { ApiError, AuthApiService, LoginRequest } from '../Infrastructure';

interface LoginFormValues {
  username: string;
  password: string;
}

interface LoginFormErrors {
  username?: string;
  password?: string;
}

@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly loginForm = viewChild<ElementRef<HTMLFormElement>>('loginForm');
  private readonly authApiService = inject(AuthApiService);
  private readonly router = inject(Router);

  protected readonly validationErrors = signal<LoginFormErrors>({});
  protected readonly apiError = signal<string | null>(null);
  protected readonly isSubmitting = signal(false);
  protected readonly errorMessages = computed(() => {
    const errors = this.validationErrors();

    return [errors.username, errors.password, this.apiError()]
      .filter((error): error is string => Boolean(error));
  });

  protected login(event: SubmitEvent): void {
    event.preventDefault();

    const form = this.loginForm()?.nativeElement;

    if (!form) {
      return;
    }

    const formValues = this.getFormValues(form);
    const validationErrors = this.validateLoginForm(formValues);

    this.validationErrors.set(validationErrors);
    this.apiError.set(null);

    if (Object.keys(validationErrors).length > 0) {
      return;
    }

    this.isSubmitting.set(true);
    this.authApiService.login(this.toLoginRequest(formValues)).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        void this.router.navigate(['/character-sheet']);
      },
      error: (error: unknown) => {
        this.isSubmitting.set(false);
        this.apiError.set(this.getErrorMessage(error));
      },
    });
  }

  protected dismissErrorPopup(): void {
    this.validationErrors.set({});
    this.apiError.set(null);
  }

  private getFormValues(form: HTMLFormElement): LoginFormValues {
    const formData = new FormData(form);

    return {
      username: this.getStringValue(formData, 'username'),
      password: this.getStringValue(formData, 'password'),
    };
  }

  private validateLoginForm(formValues: LoginFormValues): LoginFormErrors {
    const errors: LoginFormErrors = {};

    if (formValues.username.length < 6) {
      errors.username = 'Username must be at least 6 characters.';
    }

    if (formValues.password.length < 8) {
      errors.password = 'Password must be at least 8 characters.';
    }

    return errors;
  }

  private getStringValue(formData: FormData, key: keyof LoginFormValues): string {
    const value = formData.get(key);

    return typeof value === 'string' ? value.trim() : '';
  }

  private toLoginRequest(formValues: LoginFormValues): LoginRequest {
    return {
      username: formValues.username,
      password: formValues.password,
    };
  }

  private getErrorMessage(error: unknown): string {
    return this.isApiError(error) ? error.message : 'Login failed.';
  }

  private isApiError(error: unknown): error is ApiError {
    return typeof error === 'object'
      && error !== null
      && 'status' in error
      && 'message' in error
      && typeof error.status === 'number'
      && typeof error.message === 'string';
  }
}
