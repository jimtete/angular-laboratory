import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { TokenStorageService } from './Infrastructure';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly tokenStorage = inject(TokenStorageService);

  protected readonly isMenuOpen = signal(false);
  protected readonly hasValidAccessToken = computed(() => this.tokenStorage.hasValidAccessToken());

  protected toggleMenu(): void {
    this.isMenuOpen.update((isOpen) => !isOpen);
  }
}
