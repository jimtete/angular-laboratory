import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import {
  ImageCroppedEvent,
  ImageCropperComponent,
  ImageTransform,
  LoadedImage,
} from 'ngx-image-cropper';
import { ASSET_PATHS } from '../shared/constants/asset-paths';
import { ModalHelper } from '../shared/helpers/modal.helper';
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
export class CharacterSheet {
  private readonly characterSheetForm =
    viewChild<ElementRef<HTMLFormElement>>('characterSheetForm');
  private readonly portraitFileInput = viewChild<ElementRef<HTMLInputElement>>('portraitFileInput');
  private readonly modalHelper = inject(ModalHelper);

  protected readonly ratingRows = RATING_ROWS;
  protected readonly resetVersion = signal(0);
  protected readonly portraitPreviewUrl = signal<string | null>(null);
  protected readonly cropImageEvent = signal<Event | null>(null);
  protected readonly pendingPortraitUrl = signal<string | null>(null);
  protected readonly cropZoom = signal(1);
  protected readonly cropTransform = signal<ImageTransform>({ scale: 1 });
  protected readonly cropImageAspectRatio = signal(1);
  protected readonly cropError = signal<string | null>(null);

  protected clearSheet(): void {
    this.characterSheetForm()?.nativeElement.reset();
    this.portraitPreviewUrl.set(null);
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

  protected publishChanges(): void {}

  protected printSheet(): void {
    window.print();
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
}
