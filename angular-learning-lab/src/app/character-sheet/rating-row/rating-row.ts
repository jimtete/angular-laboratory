import { Component, computed, effect, input, signal } from '@angular/core';

@Component({
  selector: 'app-rating-row',
  templateUrl: './rating-row.html',
  styleUrl: './rating-row.css',
})
export class RatingRow {
  readonly imageSrc = input.required<string>();
  readonly label = input('Rating');
  readonly hoverText = input('SkillName');
  readonly isPrintReversed = input(false);
  readonly resetVersion = input(0);

  protected readonly options = Array.from({ length: 15 }, (_, index) => index + 1);
  protected readonly selectedValue = signal(0);
  protected readonly valueText = computed(() => String(this.selectedValue()));

  constructor() {
    effect(() => {
      this.resetVersion();
      this.selectedValue.set(0);
    });
  }

  protected selectImage(): void {
    this.selectedValue.set(0);
  }

  protected selectOption(value: number): void {
    this.selectedValue.update((currentValue) =>
      currentValue === value && value === 1 ? 0 : value,
    );
  }
}
