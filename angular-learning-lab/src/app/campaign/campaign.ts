import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterOutlet } from '@angular/router';
import { LucideCog, LucideHouse, LucideRefreshCw, LucideUsers } from '@lucide/angular';

import { ApiError, CampaignInformationCacheService, TokenStorageService } from '../Infrastructure';
import { ModalHelper } from '../shared/helpers/modal.helper';

@Component({
  selector: 'app-campaign',
  imports: [RouterOutlet, LucideCog, LucideHouse, LucideRefreshCw, LucideUsers],
  templateUrl: './campaign.html',
  styleUrl: './campaign.css',
})
export class Campaign implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly campaignInformationCache = inject(CampaignInformationCacheService);
  private readonly modalHelper = inject(ModalHelper);

  protected readonly isSideNavCollapsed = signal(false);
  protected readonly campaignId = signal<string | null>(null);
  protected readonly isMaster = computed(() => this.tokenStorage.hasAnyRole('Master'));
  protected readonly isReloadingCampaignInformation =
    this.campaignInformationCache.isLoadingInformation;
  protected readonly isCampaignHome = computed(() => {
    return !this.isCampaignMembers() && !this.isCampaignSettings();
  });
  protected readonly isCampaignMembers = computed(() => {
    return this.router.url.includes('/campaign-members');
  });
  protected readonly isCampaignSettings = computed(() => {
    return this.router.url.includes('/campaign-settings');
  });

  ngOnInit(): void {
    this.route.paramMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((paramMap) => {
        this.campaignId.set(paramMap.get('campaignId'));
        this.loadCampaignInformation();
      });
  }

  protected goToCampaignHome(): void {
    void this.router.navigate(['/campaigns', this.campaignId()]);
  }

  protected toggleSideNav(): void {
    this.isSideNavCollapsed.update((isCollapsed) => !isCollapsed);
  }

  protected goToCampaignMembers(): void {
    void this.router.navigate(['/campaigns', this.campaignId(), 'campaign-members']);
  }

  protected goToCampaignSettings(): void {
    void this.router.navigate(['/campaigns', this.campaignId(), 'campaign-settings']);
  }

  protected reloadCampaignInformation(): void {
    this.loadCampaignInformation(true);
  }

  private loadCampaignInformation(forceRefresh = false): void {
    const campaignId = this.campaignId();

    if (!campaignId || this.isReloadingCampaignInformation()) {
      return;
    }

    this.campaignInformationCache
      .loadCampaignInformation(campaignId, forceRefresh)
      .subscribe({
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign information could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return this.isApiError(error) ? error.message : fallback;
  }

  private getErrorStatus(error: unknown): number | undefined {
    return this.isApiError(error) ? error.status : undefined;
  }

  private isApiError(error: unknown): error is ApiError {
    return (
      typeof error === 'object' &&
      error !== null &&
      'message' in error &&
      typeof error.message === 'string' &&
      'status' in error &&
      typeof error.status === 'number'
    );
  }
}
