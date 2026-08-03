import { Component, ElementRef, OnInit, computed, inject, signal, viewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LucideMap, LucidePlus, LucideUpload, LucideX } from '@lucide/angular';

import {
  API_BASE_URL,
  ApiError,
  CampaignApiService,
  CampaignMapCategory,
  CampaignMapModel,
  CreateCampaignMapRequest,
} from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';

interface CampaignMapCategoryRow {
  category: CampaignMapCategory;
  label: string;
  description: string;
}

interface CampaignMapFormValues {
  name: string;
  description: string;
  category: CampaignMapCategory;
}

interface CampaignMapFormErrors {
  name?: string;
  mapFile?: string;
  category?: string;
}

const CAMPAIGN_MAP_CATEGORY_ROWS: CampaignMapCategoryRow[] = [
  {
    category: CampaignMapCategory.World,
    label: 'World Maps',
    description: 'Parentless world maps',
  },
  {
    category: CampaignMapCategory.Regional,
    label: 'Regional Maps',
    description: 'Parentless regional maps',
  },
  {
    category: CampaignMapCategory.City,
    label: 'City Maps',
    description: 'Parentless city maps',
  },
  {
    category: CampaignMapCategory.District,
    label: 'District Maps',
    description: 'Parentless district maps',
  },
];

const MAX_MAP_UPLOAD_BYTES = 20 * 1024 * 1024;

@Component({
  selector: 'app-campaign-maps',
  imports: [LucideMap, LucidePlus, LucideUpload, LucideX],
  templateUrl: './campaign-maps.html',
  styleUrl: './campaign-maps.css',
})
export class CampaignMaps implements OnInit {
  private readonly createMapForm = viewChild<ElementRef<HTMLFormElement>>('createMapForm');
  private readonly mapFileInput = viewChild<ElementRef<HTMLInputElement>>('mapFileInput');
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly modalHelper = inject(ModalHelper);
  private readonly apiBaseUrl = inject(API_BASE_URL).replace(/\/$/, '');

  protected readonly categoryRows = CAMPAIGN_MAP_CATEGORY_ROWS;
  protected readonly maps = signal<CampaignMapModel[]>([]);
  protected readonly isLoadingMaps = signal(false);
  protected readonly isCreateMapOpen = signal(false);
  protected readonly isUploadingMap = signal(false);
  protected readonly selectedMapFile = signal<File | null>(null);
  protected readonly selectedMapPreviewUrl = signal<string | null>(null);
  protected readonly selectedMapImageSize = signal<{ width: number; height: number } | null>(null);
  protected readonly validationErrors = signal<CampaignMapFormErrors>({});
  protected readonly campaignId = computed(() => {
    return this.route.parent?.snapshot.paramMap.get('campaignId') ?? null;
  });

  ngOnInit(): void {
    this.loadMaps();
  }

  refreshCampaignPage(): boolean {
    this.loadMaps(true);

    return true;
  }

  isRefreshingCampaignPage(): boolean {
    return this.isLoadingMaps();
  }

  protected openCreateMapDialog(category: CampaignMapCategory = CampaignMapCategory.World): void {
    this.resetCreateMapForm();
    this.isCreateMapOpen.set(true);

    window.setTimeout(() => {
      const form = this.createMapForm()?.nativeElement;
      const categorySelect = form?.elements.namedItem('category') as HTMLSelectElement | null;

      if (categorySelect) {
        categorySelect.value = category.toString();
      }
    });
  }

  protected closeCreateMapDialog(): void {
    if (this.isUploadingMap()) {
      return;
    }

    this.isCreateMapOpen.set(false);
    this.resetCreateMapForm();
  }

  protected selectMapFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    if (!file) {
      this.selectedMapFile.set(null);
      this.selectedMapPreviewUrl.set(null);
      this.selectedMapImageSize.set(null);
      return;
    }

    if (file.size > MAX_MAP_UPLOAD_BYTES) {
      input.value = '';
      this.selectedMapFile.set(null);
      this.selectedMapPreviewUrl.set(null);
      this.selectedMapImageSize.set(null);
      this.modalHelper.showError('Map file must be 20 MB or smaller.');
      return;
    }

    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type)) {
      input.value = '';
      this.selectedMapFile.set(null);
      this.selectedMapPreviewUrl.set(null);
      this.selectedMapImageSize.set(null);
      this.modalHelper.showError('Map file must be a JPEG, PNG, or WebP image.');
      return;
    }

    this.selectedMapFile.set(file);
    const previewUrl = URL.createObjectURL(file);

    this.selectedMapPreviewUrl.set(previewUrl);
    this.readImageSize(previewUrl);
    this.validationErrors.update((errors) => ({
      ...errors,
      mapFile: undefined,
    }));
  }

  protected createMap(event: Event): void {
    event.preventDefault();

    const campaignId = this.campaignId();
    const form = this.createMapForm()?.nativeElement;
    const mapFile = this.selectedMapFile();

    if (!campaignId || !form) {
      return;
    }

    const formValues = this.getFormValues(form);
    const validationErrors = this.validateForm(formValues, mapFile);

    this.validationErrors.set(validationErrors);

    if (Object.keys(validationErrors).some((key) => Boolean(validationErrors[key as keyof CampaignMapFormErrors]))) {
      this.modalHelper.showError(this.getValidationErrorMessages(validationErrors), {
        onClose: () => this.validationErrors.set({}),
      });
      return;
    }

    if (!mapFile) {
      return;
    }

    this.isUploadingMap.set(true);
    this.campaignApiService.uploadCampaignMap(
      campaignId,
      this.toCreateMapRequest(formValues),
      mapFile,
    ).subscribe({
      next: (response) => {
        this.isUploadingMap.set(false);
        this.isCreateMapOpen.set(false);
        this.resetCreateMapForm();

        if (response.data) {
          this.maps.update((maps) => [...maps, response.data as CampaignMapModel]);
        } else {
          this.loadMaps(true);
        }

        this.modalHelper.showSuccess(response.message, {
          statusCode: response.statusCode,
        });
      },
      error: (error: unknown) => {
        this.isUploadingMap.set(false);
        this.modalHelper.showError(this.getErrorMessage(error, 'Campaign map could not be uploaded.'), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  protected parentlessMapsFor(category: CampaignMapCategory): CampaignMapModel[] {
    return this.maps()
      .filter((map) => map.parentMapId === null && this.getCategoryValue(map.category) === category)
      .sort((first, second) => (
        first.name.localeCompare(second.name) ||
        first.id - second.id
      ));
  }

  protected resolveMapAssetUrl(map: CampaignMapModel): string | null {
    const assetUrl = map.assetUrl;

    if (!assetUrl) {
      return null;
    }

    if (/^(https?:\/\/|data:)/i.test(assetUrl)) {
      return assetUrl;
    }

    return assetUrl.startsWith('/')
      ? `${this.apiBaseUrl}${assetUrl}`
      : `${this.apiBaseUrl}/${assetUrl}`;
  }

  protected formatFileSize(map: CampaignMapModel): string {
    const size = map.fileSizeBytes;

    if (!size || size <= 0) {
      return 'Unknown size';
    }

    if (size < 1024 * 1024) {
      return `${Math.ceil(size / 1024)} KB`;
    }

    return `${(size / (1024 * 1024)).toFixed(1)} MB`;
  }

  protected openMap(map: CampaignMapModel): void {
    const campaignId = this.campaignId();

    if (!campaignId) {
      return;
    }

    void this.router.navigate(['/campaigns', campaignId, 'maps', map.id]);
  }

  private loadMaps(forceRefresh = false): void {
    const campaignId = this.campaignId();

    if (!campaignId || (this.isLoadingMaps() && !forceRefresh)) {
      return;
    }

    this.isLoadingMaps.set(true);
    this.campaignApiService.fetchCampaignMaps(campaignId).subscribe({
      next: (response) => {
        this.maps.set(response.data ?? []);
        this.isLoadingMaps.set(false);
      },
      error: (error: unknown) => {
        this.isLoadingMaps.set(false);
        this.modalHelper.showError(this.getErrorMessage(error, 'Campaign maps could not be loaded.'), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  private getFormValues(form: HTMLFormElement): CampaignMapFormValues {
    const formData = new FormData(form);
    const category = Number(formData.get('category'));

    return {
      name: this.getStringValue(formData, 'name'),
      description: this.getStringValue(formData, 'description'),
      category: this.isCampaignMapCategory(category)
        ? category
        : CampaignMapCategory.World,
    };
  }

  private validateForm(
    formValues: CampaignMapFormValues,
    mapFile: File | null,
  ): CampaignMapFormErrors {
    const errors: CampaignMapFormErrors = {};

    if (formValues.name.length === 0) {
      errors.name = 'Map name is required.';
    }

    if (!this.isCampaignMapCategory(formValues.category)) {
      errors.category = 'Map category is invalid.';
    }

    if (!mapFile) {
      errors.mapFile = 'Map image is required.';
    }

    return errors;
  }

  private getValidationErrorMessages(errors: CampaignMapFormErrors): string[] {
    return [errors.name, errors.category, errors.mapFile]
      .filter((error): error is string => Boolean(error));
  }

  private toCreateMapRequest(formValues: CampaignMapFormValues): CreateCampaignMapRequest {
    return {
      parentMapId: null,
      category: formValues.category,
      imageWidthPixels: this.selectedMapImageSize()?.width ?? 1,
      imageHeightPixels: this.selectedMapImageSize()?.height ?? 1,
      name: formValues.name,
      description: formValues.description,
    };
  }

  private resetCreateMapForm(): void {
    const previewUrl = this.selectedMapPreviewUrl();

    if (previewUrl) {
      URL.revokeObjectURL(previewUrl);
    }

    this.createMapForm()?.nativeElement.reset();
    this.mapFileInput()?.nativeElement.value && (this.mapFileInput()!.nativeElement.value = '');
    this.selectedMapFile.set(null);
    this.selectedMapPreviewUrl.set(null);
    this.selectedMapImageSize.set(null);
    this.validationErrors.set({});
  }

  private readImageSize(url: string): void {
    const image = new Image();

    image.onload = () => {
      this.selectedMapImageSize.set({
        width: image.naturalWidth,
        height: image.naturalHeight,
      });
    };

    image.onerror = () => {
      this.selectedMapImageSize.set(null);
    };

    image.src = url;
  }

  private getStringValue(formData: FormData, key: keyof CampaignMapFormValues): string {
    const value = formData.get(key);

    return typeof value === 'string' ? value.trim() : '';
  }

  private getCategoryValue(
    category: CampaignMapModel['category'],
  ): number {
    if (typeof category === 'number') {
      return category;
    }

    if (typeof category === 'string') {
      const numericCategory = Number(category);

      if (Number.isFinite(numericCategory)) {
        return numericCategory;
      }

      return CampaignMapCategory[category as keyof typeof CampaignMapCategory] ?? 0;
    }

    return 0;
  }

  private isCampaignMapCategory(value: number): value is CampaignMapCategory {
    return Object.values(CampaignMapCategory)
      .filter((category): category is number => typeof category === 'number')
      .includes(value);
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return this.isApiError(error) ? error.message : fallback;
  }

  private getErrorStatusCode(error: unknown): number | undefined {
    return this.isApiError(error) ? error.status : undefined;
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
