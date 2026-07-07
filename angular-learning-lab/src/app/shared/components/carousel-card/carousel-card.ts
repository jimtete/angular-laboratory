import { Component, computed, input } from '@angular/core';
import { LucideBrainCircuit, LucideLightbulb, LucideTheater } from '@lucide/angular';
import { CarouselCardData } from '../../data/carousel-cards.data';

@Component({
  selector: 'app-carousel-card',
  imports: [LucideBrainCircuit, LucideLightbulb, LucideTheater],
  templateUrl: './carousel-card.html',
  styleUrl: './carousel-card.css',
})
export class CarouselCard {
  private readonly longWordThreshold = 12;
  private readonly softHyphen = '\u00ad';

  readonly card = input.required<CarouselCardData>();

  protected readonly backgroundImageStyle = computed(() => `url("${this.card().backgroundImage}")`);
  protected readonly formattedTitle = computed(() => this.formatTitle(this.card().title));

  private formatTitle(title: string): string {
    return title
      .split(' ')
      .map((word) => this.addBreakOpportunity(word))
      .join(' ');
  }

  private addBreakOpportunity(word: string): string {
    if (word.length < this.longWordThreshold || word.includes('-')) {
      return word;
    }

    const breakIndex = Math.round(word.length * 0.6);

    return `${word.slice(0, breakIndex)}${this.softHyphen}${word.slice(breakIndex)}`;
  }
}
