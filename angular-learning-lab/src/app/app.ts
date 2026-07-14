import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { ApiLoadingService, AuthApiService, TokenStorageService } from './Infrastructure';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly authApiService = inject(AuthApiService);
  private readonly apiLoadingService = inject(ApiLoadingService);
  private readonly router = inject(Router);

  protected readonly isMenuOpen = signal(false);
  protected readonly isApiLoading = computed(() => this.apiLoadingService.isLoading());
  protected readonly hasValidAccessToken = computed(() => this.tokenStorage.hasValidAccessToken());
  protected readonly hasCampaignAccess = computed(() =>
    this.tokenStorage.hasAnyRole('Master', 'Player'),
  );

  protected toggleMenu(): void {
    this.isMenuOpen.update((isOpen) => !isOpen);
  }

  protected logout(): void {
    this.authApiService.logout();
    this.isMenuOpen.set(false);
    void this.router.navigate(['/']);
  }
}
