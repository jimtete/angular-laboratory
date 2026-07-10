import {
  AfterViewInit,
  Component,
  ElementRef,
  inject,
  signal,
  viewChild,
  viewChildren,
} from '@angular/core';
import {
  ImageCroppedEvent,
  ImageCropperComponent,
  ImageTransform,
  LoadedImage,
} from 'ngx-image-cropper';
import { ASSET_PATHS } from '../shared/constants/asset-paths';
import { ModalHelper } from '../shared/helpers/modal.helper';
import {
  ApiError,
  CharacterSheetModel,
  CharacterActionRequest,
  CharacterSheetApiService,
  UpdateCharacterSheetRequest,
} from '../Infrastructure';
import { API_BASE_URL } from '../Infrastructure/api.config';
import { RatingRow } from './rating-row/rating-row';
import { SheetTable } from './sheet-table/sheet-table';

const RATING_ROWS = [
  {
    imageSrc: ASSET_PATHS.images.skills.logic,
    label: 'Logic',
    hoverText: 'Logic',
  },
  {
    imageSrc: ASSET_PATHS.images.skills.psyche,
    label: 'Psyche',
    hoverText: 'Psyche',
  },
  {
    imageSrc: ASSET_PATHS.images.skills.physical,
    label: 'Physical',
    hoverText: 'Physical',
  },
  {
    imageSrc: ASSET_PATHS.images.skills.motorics,
    label: 'Motorics',
    hoverText: 'Motorics',
  },
];
const MAX_PORTRAIT_UPLOAD_BYTES = 2 * 1024 * 1024;

@Component({
  selector: 'app-character-sheet',
  imports: [ImageCropperComponent, RatingRow, SheetTable],
  templateUrl: './character-sheet.html',
  styleUrl: './character-sheet.css',
})
export class CharacterSheet implements AfterViewInit {
  private readonly characterSheetForm =
    viewChild<ElementRef<HTMLFormElement>>('characterSheetForm');
  private readonly portraitFileInput = viewChild<ElementRef<HTMLInputElement>>('portraitFileInput');
  private readonly sheetTables = viewChildren(SheetTable);
  private readonly ratingRowComponents = viewChildren(RatingRow);
  private readonly characterSheetApiService = inject(CharacterSheetApiService);
  private readonly modalHelper = inject(ModalHelper);
  private readonly apiBaseUrl = inject(API_BASE_URL).replace(/\/$/, '');

  protected readonly ratingRows = RATING_ROWS;
  protected readonly resetVersion = signal(0);
  protected readonly portraitPreviewUrl = signal<string | null>(null);
  protected readonly cropImageEvent = signal<Event | null>(null);
  protected readonly pendingPortraitUrl = signal<string | null>(null);
  protected readonly cropZoom = signal(1);
  protected readonly cropTransform = signal<ImageTransform>({ scale: 1 });
  protected readonly cropImageAspectRatio = signal(1);
  protected readonly cropError = signal<string | null>(null);
  private readonly selectedPortraitBlob = signal<Blob | null>(null);

  ngAfterViewInit(): void {
    this.fetchCharacterSheet();
  }

  protected clearSheet(): void {
    this.characterSheetForm()?.nativeElement.reset();
    this.portraitPreviewUrl.set(null);
    this.selectedPortraitBlob.set(null);
    this.resetPortraitCrop();
    this.resetVersion.update((version) => version + 1);
  }

  protected selectPortrait(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) {
      return;
    }

    if (file.size > MAX_PORTRAIT_UPLOAD_BYTES) {
      this.resetPortraitCrop();
      this.modalHelper.showError('Character profile picture must be 2 MB or smaller.');
      return;
    }

    this.pendingPortraitUrl.set(null);
    this.cropError.set(null);
    this.cropZoom.set(1);
    this.cropTransform.set({ scale: 1 });
    this.cropImageEvent.set(event);
  }

  protected updatePortraitCrop(event: ImageCroppedEvent): void {
    this.pendingPortraitUrl.set(event.base64 ?? null);
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

  protected applyPortraitCrop(): void {
    const croppedPortrait = this.pendingPortraitUrl();

    if (!croppedPortrait) {
      return;
    }

    this.portraitPreviewUrl.set(croppedPortrait);
    this.selectedPortraitBlob.set(this.dataUrlToBlob(croppedPortrait));
    this.resetPortraitCrop();
    this.modalHelper.showSuccess('Character profile picture has successfully been set.');
  }

  protected cancelPortraitCrop(): void {
    this.resetPortraitCrop();
  }

  protected handlePortraitLoadFailure(): void {
    this.pendingPortraitUrl.set(null);
    this.cropError.set('The selected image could not be loaded.');
  }

  protected publishChanges(): void {
    const form = this.characterSheetForm()?.nativeElement;

    if (!form) {
      return;
    }

    const selectedPortraitBlob = this.selectedPortraitBlob();

    if (selectedPortraitBlob) {
      this.characterSheetApiService.uploadProfilePicture(selectedPortraitBlob).subscribe({
        next: () => this.selectedPortraitBlob.set(null),
        error: (error: unknown) => {
          this.modalHelper.showError(this.getErrorMessage(error), {
            statusCode: this.getErrorStatusCode(error),
          });
        },
      });
    }

    this.characterSheetApiService.saveCharacterSheet(this.toUpdateRequest(form)).subscribe({
      next: (response) => {
        this.modalHelper.showSuccess(response.message, {
          statusCode: response.statusCode,
        });
      },
      error: (error: unknown) => {
        this.modalHelper.showError(this.getErrorMessage(error), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  protected printSheet(): void {
    window.print();
  }

  private fetchCharacterSheet(): void {
    this.characterSheetApiService.fetchCharacterSheet().subscribe({
      next: (response) => {
        if (response.data) {
          this.populateCharacterSheet(response.data);
        }
      },
      error: (error: unknown) => {
        this.modalHelper.showError(this.getErrorMessage(error), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  private populateCharacterSheet(characterSheet: CharacterSheetModel): void {
    this.selectedPortraitBlob.set(null);
    this.portraitPreviewUrl.set(this.resolveApiAssetUrl(characterSheet.portraitUrl));
    this.setFormValue('background', characterSheet.background);
    this.setFormValue('information', characterSheet.information);
    this.setFormValue('firstName', characterSheet.firstName);
    this.setFormValue('lastName', characterSheet.lastName);
    this.setFormValue('characterClass', characterSheet.characterClass);
    this.setFormValue('nationality', characterSheet.nationality);
    this.setFormValue('height', characterSheet.height);
    this.setFormValue('weight', characterSheet.weight);
    this.setTableValues(
      'Actions/Bonus Actions',
      characterSheet.actions.map((action) => action.title ?? action.description ?? ''),
    );
    this.setTableValues('Traits', characterSheet.traits);
    this.setTableValues('Equipment', characterSheet.equipment);
    this.setRatingValue('Logic', characterSheet.logicRating);
    this.setRatingValue('Psyche', characterSheet.psycheRating);
    this.setRatingValue('Physical', characterSheet.physicalRating);
    this.setRatingValue('Motorics', characterSheet.motoricsRating);
  }

  private resetPortraitCrop(clearFileInput = true): void {
    this.cropImageEvent.set(null);
    this.pendingPortraitUrl.set(null);
    this.cropError.set(null);
    this.cropZoom.set(1);
    this.cropTransform.set({ scale: 1 });
    this.cropImageAspectRatio.set(1);

    if (clearFileInput) {
      const input = this.portraitFileInput()?.nativeElement;

      if (input) {
        input.value = '';
      }
    }
  }

  private setFormValue(name: string, value: string | number | null): void {
    const form = this.characterSheetForm()?.nativeElement;
    const control = form?.elements.namedItem(name);

    if (control instanceof HTMLTextAreaElement || control instanceof HTMLInputElement) {
      control.value = value?.toString() ?? '';
    }
  }

  private setTableValues(title: string, values: string[]): void {
    this.sheetTables()
      .find((table) => table.title() === title)
      ?.setValues(values);
  }

  private setRatingValue(label: string, value: number): void {
    this.ratingRowComponents()
      .find((ratingRow) => ratingRow.label() === label)
      ?.setValue(value);
  }

  private resolveApiAssetUrl(url: string | null): string | null {
    if (!url) {
      return null;
    }

    if (/^https?:\/\//i.test(url)) {
      return url;
    }

    return url.startsWith('/') ? `${this.apiBaseUrl}${url}` : `${this.apiBaseUrl}/${url}`;
  }

  private toUpdateRequest(form: HTMLFormElement): UpdateCharacterSheetRequest {
    const formData = new FormData(form);
    const tablesByTitle = new Map(
      this.sheetTables().map((table) => [table.title(), table.getValues()]),
    );

    return {
      background: this.getNullableStringValue(formData, 'background'),
      information: this.getNullableStringValue(formData, 'information'),
      firstName: this.getStringValue(formData, 'firstName'),
      lastName: this.getStringValue(formData, 'lastName'),
      characterClass: this.getStringValue(formData, 'characterClass'),
      nationality: this.getNullableStringValue(formData, 'nationality'),
      height: this.getNullableNumberValue(formData, 'height'),
      weight: this.getNullableNumberValue(formData, 'weight'),
      actions: this.toActionRequests(tablesByTitle.get('Actions/Bonus Actions') ?? []),
      traits: tablesByTitle.get('Traits') ?? [],
      equipment: tablesByTitle.get('Equipment') ?? [],
      logicRating: this.getRatingValue('Logic'),
      psycheRating: this.getRatingValue('Psyche'),
      physicalRating: this.getRatingValue('Physical'),
      motoricsRating: this.getRatingValue('Motorics'),
    };
  }

  private toActionRequests(values: string[]): CharacterActionRequest[] {
    return values.map((value) => ({
      actionType: 0,
      title: value,
      description: null,
    }));
  }

  private getRatingValue(label: string): number {
    return this.ratingRowComponents()
      .find((ratingRow) => ratingRow.label() === label)
      ?.getValue() ?? 0;
  }

  private getStringValue(formData: FormData, key: string): string {
    const value = formData.get(key);

    return typeof value === 'string' ? value.trim() : '';
  }

  private getNullableStringValue(formData: FormData, key: string): string | null {
    const value = this.getStringValue(formData, key);

    return value.length > 0 ? value : null;
  }

  private getNullableNumberValue(formData: FormData, key: string): number | null {
    const value = this.getStringValue(formData, key);

    if (value.length === 0) {
      return null;
    }

    const numberValue = Number(value);

    return Number.isFinite(numberValue) ? numberValue : null;
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

  private getErrorStatusCode(error: unknown): number | undefined {
    return this.isApiError(error) ? error.status : undefined;
  }

  private getErrorMessage(error: unknown): string {
    return this.isApiError(error) ? error.message : 'Character sheet update failed.';
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
