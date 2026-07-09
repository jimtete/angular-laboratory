import { Component, ElementRef, computed, inject, signal, viewChild } from '@angular/core';

import { ApiError, AuthApiService, RegisterUserRequest } from '../Infrastructure';

interface RegisterFormValues {
  username: string;
  password: string;
  repeatPassword: string;
  firstName: string;
  lastName: string;
}

interface RegisterFormErrors {
  username?: string;
  password?: string;
  repeatPassword?: string;
  firstName?: string;
  lastName?: string;
}

interface RegisterNotification {
  type: 'success' | 'error';
  statusCode?: number;
  messages: string[];
}

@Component({
  selector: 'app-register',
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private readonly registerForm = viewChild<ElementRef<HTMLFormElement>>('registerForm');
  private readonly authApiService = inject(AuthApiService);
  private readonly errorPopupAnimationDurationMs = 200;

  protected readonly validationErrors = signal<RegisterFormErrors>({});
  protected readonly registerNotification = signal<RegisterNotification | null>(null);
  protected readonly isErrorPopupDismissing = signal(false);
  protected readonly validationErrorMessages = computed(() => {
    const errors = this.validationErrors();

    return [
      errors.username,
      errors.password,
      errors.repeatPassword,
      errors.firstName,
      errors.lastName,
    ].filter((error): error is string => Boolean(error));
  });
  protected readonly notificationMessages = computed(() => {
    return this.registerNotification()?.messages ?? this.validationErrorMessages();
  });
  protected readonly notificationStatusCode = computed(() => {
    return this.registerNotification()?.statusCode;
  });
  protected readonly notificationType = computed(() => {
    return this.registerNotification()?.type ?? 'error';
  });
  protected readonly isNotificationVisible = computed(() => {
    return this.notificationMessages().length > 0;
  });
  protected readonly isSubmitting = signal(false);

  protected clearForm(): void {
    this.registerForm()?.nativeElement.reset();
    this.validationErrors.set({});
    this.registerNotification.set(null);
    this.isErrorPopupDismissing.set(false);
  }

  protected register(): void {
    const form = this.registerForm()?.nativeElement;

    if (!form) {
      return;
    }

    const formValues = this.getFormValues(form);
    const validationErrors = this.validateRegisterForm(formValues);

    this.validationErrors.set(validationErrors);
    this.registerNotification.set(null);
    this.isErrorPopupDismissing.set(false);

    if (Object.keys(validationErrors).length > 0) {
      return;
    }

    this.isSubmitting.set(true);
    this.authApiService.register(this.toRegisterRequest(formValues)).subscribe({
      next: (response) => {
        this.isSubmitting.set(false);
        this.registerNotification.set({
          type: 'success',
          statusCode: response.statusCode,
          messages: [response.message],
        });
        form.reset();
      },
      error: (error: unknown) => {
        this.isSubmitting.set(false);
        this.registerNotification.set({
          type: 'error',
          statusCode: this.getErrorStatusCode(error),
          messages: [this.getErrorMessage(error)],
        });
      },
    });
  }

  protected dismissErrorPopup(): void {
    if (!this.isNotificationVisible() || this.isErrorPopupDismissing()) {
      return;
    }

    this.isErrorPopupDismissing.set(true);

    window.setTimeout(() => {
      this.validationErrors.set({});
      this.registerNotification.set(null);
      this.isErrorPopupDismissing.set(false);
    }, this.errorPopupAnimationDurationMs);
  }

  private getFormValues(form: HTMLFormElement): RegisterFormValues {
    const formData = new FormData(form);

    return {
      username: this.getStringValue(formData, 'username'),
      password: this.getStringValue(formData, 'password'),
      repeatPassword: this.getStringValue(formData, 'repeatPassword'),
      firstName: this.getStringValue(formData, 'firstName'),
      lastName: this.getStringValue(formData, 'lastName'),
    };
  }

  private validateRegisterForm(formValues: RegisterFormValues): RegisterFormErrors {
    const errors: RegisterFormErrors = {};

    if (formValues.username.length < 6) {
      errors.username = 'Username must be at least 6 characters.';
    }

    if (formValues.password.length < 8) {
      errors.password = 'Password must be at least 8 characters.';
    }

    if (formValues.repeatPassword !== formValues.password) {
      errors.repeatPassword = 'Repeat password must match password.';
    }

    if (formValues.firstName.length < 2) {
      errors.firstName = 'First name must be at least 2 characters.';
    }

    if (formValues.lastName.length < 2) {
      errors.lastName = 'Last name must be at least 2 characters.';
    }

    return errors;
  }

  private getStringValue(formData: FormData, key: keyof RegisterFormValues): string {
    const value = formData.get(key);

    return typeof value === 'string' ? value.trim() : '';
  }

  private toRegisterRequest(formValues: RegisterFormValues): RegisterUserRequest {
    return {
      username: formValues.username,
      password: formValues.password,
      firstName: formValues.firstName,
      lastName: formValues.lastName,
    };
  }

  private getErrorStatusCode(error: unknown): number | undefined {
    return this.isApiError(error) ? error.status : undefined;
  }

  private getErrorMessage(error: unknown): string {
    if (this.isApiError(error)) {
      return error.message;
    }

    return 'Register failed.';
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
