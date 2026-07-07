import { Component, computed, signal } from '@angular/core';
import { CarouselCard } from '../shared/components/carousel-card/carousel-card';
import { ASSET_PATHS } from '../shared/constants/asset-paths';
import { CAROUSEL_CARDS, CarouselCardData } from '../shared/data/carousel-cards.data';

@Component({
  selector: 'app-home',
  imports: [CarouselCard],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  private readonly carouselAnimationDurationMs = 225;

  protected readonly assets = ASSET_PATHS;
  protected readonly cards = CAROUSEL_CARDS;
  protected readonly visibleCardCount = 3;
  protected readonly carouselStartIndex = signal(0);
  protected readonly targetCarouselStartIndex = signal<number | null>(null);
  protected readonly carouselAnimationDirection = signal<'next' | 'previous' | null>(null);

  protected readonly carouselPages = computed(() => {
    const currentPage = this.getCardsFromIndex(this.carouselStartIndex());
    const targetIndex = this.targetCarouselStartIndex();

    if (targetIndex === null) {
      return [currentPage];
    }

    const targetPage = this.getCardsFromIndex(targetIndex);

    return this.carouselAnimationDirection() === 'previous'
      ? [targetPage, currentPage]
      : [currentPage, targetPage];
  });

  protected nextCarouselPage(): void {
    this.moveCarousel('next');
  }

  protected previousCarouselPage(): void {
    this.moveCarousel('previous');
  }

  private moveCarousel(direction: 'next' | 'previous'): void {
    if (this.carouselAnimationDirection() !== null) {
      return;
    }

    const targetIndex =
      direction === 'next'
        ? (this.carouselStartIndex() + this.visibleCardCount) % this.cards.length
        : (this.carouselStartIndex() - this.visibleCardCount + this.cards.length) %
          this.cards.length;

    this.targetCarouselStartIndex.set(targetIndex);
    this.carouselAnimationDirection.set(direction);

    window.setTimeout(() => {
      this.carouselStartIndex.set(targetIndex);
      this.targetCarouselStartIndex.set(null);
      this.carouselAnimationDirection.set(null);
    }, this.carouselAnimationDurationMs);
  }

  private getCardsFromIndex(startIndex: number): CarouselCardData[] {
    return Array.from({ length: this.visibleCardCount }, (_, offset) => {
      const cardIndex = (startIndex + offset) % this.cards.length;
      return this.cards[cardIndex];
    });
  }
}
