import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  computed,
  signal,
  viewChild,
} from '@angular/core';
import { CarouselCard } from '../shared/components/carousel-card/carousel-card';
import { ASSET_PATHS } from '../shared/constants/asset-paths';
import { CAROUSEL_CARDS, CarouselCardData } from '../shared/data/carousel-cards.data';

@Component({
  selector: 'app-home',
  imports: [CarouselCard],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home implements OnInit, AfterViewInit, OnDestroy {
  private readonly carouselAnimationDurationMs = 225;
  private readonly homePage = viewChild<ElementRef<HTMLElement>>('homePage');
  private readonly homeBackground = viewChild<ElementRef<HTMLImageElement>>('homeBackground');
  private resizeObserver: ResizeObserver | undefined;
  private animationFrameId: number | undefined;

  protected readonly assets = ASSET_PATHS;
  protected readonly cards = CAROUSEL_CARDS;
  protected readonly isHeroImageLoaded = signal(false);
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

  ngOnInit(): void {
    this.preloadHeroImage();
  }

  ngAfterViewInit(): void {
    window.addEventListener('scroll', this.scheduleParallaxUpdate, { passive: true });
    window.addEventListener('resize', this.scheduleParallaxUpdate, { passive: true });

    this.resizeObserver = new ResizeObserver(this.scheduleParallaxUpdate);

    const page = this.homePage()?.nativeElement;
    const background = this.homeBackground()?.nativeElement;

    if (page) {
      this.resizeObserver.observe(page);
    }

    if (background) {
      this.resizeObserver.observe(background);
    }

    this.scheduleParallaxUpdate();
  }

  ngOnDestroy(): void {
    window.removeEventListener('scroll', this.scheduleParallaxUpdate);
    window.removeEventListener('resize', this.scheduleParallaxUpdate);
    this.resizeObserver?.disconnect();

    if (this.animationFrameId !== undefined) {
      cancelAnimationFrame(this.animationFrameId);
    }
  }

  protected refreshParallax(): void {
    this.scheduleParallaxUpdate();
  }

  private preloadHeroImage(): void {
    const heroImage = new Image();

    heroImage.onload = () => this.isHeroImageLoaded.set(true);
    heroImage.onerror = () => this.isHeroImageLoaded.set(true);
    heroImage.src = this.assets.images.deadAsDisco;
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

  private readonly scheduleParallaxUpdate = (): void => {
    if (this.animationFrameId !== undefined) {
      return;
    }

    this.animationFrameId = requestAnimationFrame(() => {
      this.animationFrameId = undefined;
      this.updateParallax();
    });
  };

  private updateParallax(): void {
    const page = this.homePage()?.nativeElement;
    const background = this.homeBackground()?.nativeElement;

    if (!page || !background || background.naturalHeight === 0) {
      return;
    }

    const navHeight = this.getNavHeight(page);
    const viewportHeight = Math.max(0, window.innerHeight - navHeight);
    const pageTop = page.getBoundingClientRect().top + window.scrollY;
    const scrollStart = pageTop - navHeight;
    const scrollDistance = Math.max(1, page.offsetHeight - viewportHeight);
    const scrollProgress = Math.min(
      1,
      Math.max(0, (window.scrollY - scrollStart) / scrollDistance),
    );
    const translationRange = viewportHeight - background.offsetHeight;
    const translateY = translationRange * scrollProgress;

    background.style.transform = `translate3d(0, ${translateY}px, 0)`;
  }

  private getNavHeight(page: HTMLElement): number {
    const navHeight = Number.parseFloat(
      getComputedStyle(page).getPropertyValue('--nav-height'),
    );

    return Number.isFinite(navHeight) ? navHeight : 64;
  }
}
