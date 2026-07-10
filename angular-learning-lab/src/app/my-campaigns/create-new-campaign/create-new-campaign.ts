import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { Router } from '@angular/router';

import { ApiError, CampaignApiService, CreateCampaignRequest } from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';

interface CreateCampaignFormValues {
  campaignName: string;
  version: string;
  campaignPictureUrl: string;
}

interface CreateCampaignFormErrors {
  campaignName?: string;
  version?: string;
}

@Component({
  selector: 'app-create-new-campaign',
  templateUrl: './create-new-campaign.html',
  styleUrl: './create-new-campaign.css',
})
export class CreateNewCampaign {
  private readonly createCampaignForm =
    viewChild<ElementRef<HTMLFormElement>>('createCampaignForm');
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly modalHelper = inject(ModalHelper);
  private readonly router = inject(Router);

  protected readonly validationErrors = signal<CreateCampaignFormErrors>({});
  protected readonly isSubmitting = signal(false);

  protected goBack(): void {
    void this.router.navigate(['/my-campaigns']);
  }

  protected createCampaign(): void {
    const form = this.createCampaignForm()?.nativeElement;

    if (!form) {
      return;
    }

    const formValues = this.getFormValues(form);
    const validationErrors = this.validateCreateCampaignForm(formValues);

    this.validationErrors.set(validationErrors);

    if (Object.keys(validationErrors).length > 0) {
      this.modalHelper.showError(this.getValidationErrorMessages(validationErrors), {
        onClose: () => this.validationErrors.set({}),
      });
      return;
    }

    this.isSubmitting.set(true);
    this.campaignApiService.createCampaign(this.toCreateCampaignRequest(formValues)).subscribe({
      next: (response) => {
        this.isSubmitting.set(false);
        this.modalHelper.showSuccess(response.message, {
          statusCode: response.statusCode,
        });
        void this.router.navigate(['/my-campaigns']);
      },
      error: (error: unknown) => {
        this.isSubmitting.set(false);
        this.modalHelper.showError(this.getErrorMessage(error), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  private getFormValues(form: HTMLFormElement): CreateCampaignFormValues {
    const formData = new FormData(form);

    return {
      campaignName: this.getStringValue(formData, 'campaignName'),
      version: this.getStringValue(formData, 'version'),
      campaignPictureUrl: this.getStringValue(formData, 'campaignPictureUrl'),
    };
  }

  private validateCreateCampaignForm(
    formValues: CreateCampaignFormValues,
  ): CreateCampaignFormErrors {
    const errors: CreateCampaignFormErrors = {};

    if (formValues.campaignName.length === 0) {
      errors.campaignName = 'Campaign name is required.';
    }

    if (formValues.version.length === 0) {
      errors.version = 'Version is required.';
    }

    return errors;
  }

  private getValidationErrorMessages(errors: CreateCampaignFormErrors): string[] {
    return [errors.campaignName, errors.version].filter((error): error is string => Boolean(error));
  }

  private getStringValue(formData: FormData, key: keyof CreateCampaignFormValues): string {
    const value = formData.get(key);

    return typeof value === 'string' ? value.trim() : '';
  }

  private toCreateCampaignRequest(formValues: CreateCampaignFormValues): CreateCampaignRequest {
    return {
      campaignName: formValues.campaignName,
      version: formValues.version,
      campaignPictureUrl: formValues.campaignPictureUrl || null,
    };
  }

  private getErrorStatusCode(error: unknown): number | undefined {
    return this.isApiError(error) ? error.status : undefined;
  }

  private getErrorMessage(error: unknown): string {
    return this.isApiError(error) ? error.message : 'Campaign could not be created.';
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
