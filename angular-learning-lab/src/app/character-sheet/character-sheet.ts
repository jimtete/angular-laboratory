import { Component, ElementRef, signal, viewChild } from '@angular/core';
import { ASSET_PATHS } from '../shared/constants/asset-paths';
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

@Component({
  selector: 'app-character-sheet',
  imports: [RatingRow, SheetTable],
  templateUrl: './character-sheet.html',
  styleUrl: './character-sheet.css',
})
export class CharacterSheet {
  private readonly characterSheetForm = viewChild<ElementRef<HTMLFormElement>>('characterSheetForm');

  protected readonly assets = ASSET_PATHS;
  protected readonly ratingRows = RATING_ROWS;
  protected readonly resetVersion = signal(0);

  protected clearSheet(): void {
    this.characterSheetForm()?.nativeElement.reset();
    this.resetVersion.update((version) => version + 1);
  }

  protected printSheet(): void {
    window.print();
  }
}
