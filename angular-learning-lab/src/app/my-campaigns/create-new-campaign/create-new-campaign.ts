import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { Router } from '@angular/router';
import {
  ImageCroppedEvent,
  ImageCropperComponent,
  ImageTransform,
  LoadedImage,
} from 'ngx-image-cropper';

import {
  ApiError,
  CampaignApiService,
  CampaignCacheService,
  CreateCampaignRequest,
} from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';

interface CreateCampaignFormValues {
  campaignName: string;
  version: string;
}

interface CreateCampaignFormErrors {
  campaignName?: string;
  version?: string;
}

const MAX_CAMPAIGN_PICTURE_UPLOAD_BYTES = 5 * 1024 * 1024;

@Component({
  selector: 'app-create-new-campaign',
  imports: [ImageCropperComponent],
  templateUrl: './create-new-campaign.html',
  styleUrl: './create-new-campaign.css',
})
export class CreateNewCampaign {
  private readonly createCampaignForm =
    viewChild<ElementRef<HTMLFormElement>>('createCampaignForm');
  private readonly campaignPictureFileInput =
    viewChild<ElementRef<HTMLInputElement>>('campaignPictureFileInput');
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly campaignCache = inject(CampaignCacheService);
  private readonly modalHelper = inject(ModalHelper);
  private readonly router = inject(Router);

  protected readonly validationErrors = signal<CreateCampaignFormErrors>({});
  protected readonly isSubmitting = signal(false);
  protected readonly campaignPicturePreviewUrl = signal<string | null>(null);
  protected readonly cropImageEvent = signal<Event | null>(null);
  protected readonly pendingCampaignPictureUrl = signal<string | null>(null);
  protected readonly cropZoom = signal(1);
  protected readonly cropTransform = signal<ImageTransform>({ scale: 1 });
  protected readonly cropImageAspectRatio = signal(1);
  protected readonly cropError = signal<string | null>(null);
  private readonly campaignPictureBlob = signal<Blob | null>(null);

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
    this.campaignApiService.createCampaign(
      this.toCreateCampaignRequest(formValues),
      this.campaignPictureBlob(),
    ).subscribe({
      next: (response) => {
        this.isSubmitting.set(false);
        this.modalHelper.showSuccess(response.message, {
          statusCode: response.statusCode,
        });
        this.campaignCache.loadAvailableCampaigns(true).subscribe({
          error: () => {},
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

  protected selectCampaignPicture(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) {
      return;
    }

    if (file.size > MAX_CAMPAIGN_PICTURE_UPLOAD_BYTES) {
      this.resetCampaignPictureCrop();
      this.modalHelper.showError('Campaign picture must be 5 MB or smaller.');
      return;
    }

    this.pendingCampaignPictureUrl.set(null);
    this.cropError.set(null);
    this.cropZoom.set(1);
    this.cropTransform.set({ scale: 1 });
    this.cropImageEvent.set(event);
  }

  protected updateCampaignPictureCrop(event: ImageCroppedEvent): void {
    this.pendingCampaignPictureUrl.set(event.base64 ?? null);
  }

  protected setCropImageDimensions(image: LoadedImage): void {
    const { width, height } = image.transformed.size;

    this.cropImageAspectRatio.set(height > 0 ? width / height : 1);
  }

  protected setCropZoom(event: Event): void {
    const scale = Number((event.target as HTMLInputElement).value);

    this.cropZoom.set(scale);
    this.cropTransform.set({
      ...this.cropTransform(),
      scale,
    });
  }

  protected applyCampaignPictureCrop(): void {
    const croppedCampaignPicture = this.pendingCampaignPictureUrl();

    if (!croppedCampaignPicture) {
      return;
    }

    this.campaignPicturePreviewUrl.set(croppedCampaignPicture);
    this.campaignPictureBlob.set(this.dataUrlToBlob(croppedCampaignPicture));
    this.resetCampaignPictureCrop();
  }

  protected cancelCampaignPictureCrop(): void {
    this.resetCampaignPictureCrop();
  }

  protected handleCampaignPictureLoadFailure(): void {
    this.pendingCampaignPictureUrl.set(null);
    this.cropError.set('The selected image could not be loaded.');
  }

  private getFormValues(form: HTMLFormElement): CreateCampaignFormValues {
    const formData = new FormData(form);

    return {
      campaignName: this.getStringValue(formData, 'campaignName'),
      version: this.getStringValue(formData, 'version'),
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
    };
  }

  private dataUrlToBlob(dataUrl: string): Blob {
    const [metadata, base64Data] = dataUrl.split(',');
    const contentType = metadata.match(/data:(.*);base64/)?.[1] ?? 'image/jpeg';
    const binary = window.atob(base64Data);
    const bytes = new Uint8Array(binary.length);

    for (let index = 0; index < binary.length; index += 1) {
      bytes[index] = binary.charCodeAt(index);
    }

    return new Blob([bytes], { type: contentType });
  }

  private resetCampaignPictureCrop(clearFileInput = true): void {
    this.cropImageEvent.set(null);
    this.pendingCampaignPictureUrl.set(null);
    this.cropError.set(null);
    this.cropZoom.set(1);
    this.cropTransform.set({ scale: 1 });
    this.cropImageAspectRatio.set(1);

    if (clearFileInput) {
      const input = this.campaignPictureFileInput()?.nativeElement;

      if (input) {
        input.value = '';
      }
    }
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
