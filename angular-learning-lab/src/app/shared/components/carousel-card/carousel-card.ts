import { Component, input } from '@angular/core';

@Component({
  selector: 'app-carousel-card',
  templateUrl: './carousel-card.html',
  styleUrl: './carousel-card.css',
})
export class CarouselCard {
  readonly value = input.required<number>();
  readonly title = input('Placeholder');
  readonly iconLabels = input(['A', 'B', 'C']);
}
