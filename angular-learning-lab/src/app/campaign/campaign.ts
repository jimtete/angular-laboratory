import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterOutlet } from '@angular/router';
import { LucideCog, LucideHouse, LucideUsers } from '@lucide/angular';

import { TokenStorageService } from '../Infrastructure';

@Component({
  selector: 'app-campaign',
  imports: [RouterOutlet, LucideCog, LucideHouse, LucideUsers],
  templateUrl: './campaign.html',
  styleUrl: './campaign.css',
})
export class Campaign {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly tokenStorage = inject(TokenStorageService);

  protected readonly isSideNavCollapsed = signal(false);
  protected readonly campaignId = computed(() => this.route.snapshot.paramMap.get('campaignId'));
  protected readonly isMaster = computed(() => this.tokenStorage.hasAnyRole('Master'));
  protected readonly isCampaignHome = computed(() => {
    return !this.isCampaignMembers() && !this.isCampaignSettings();
  });
  protected readonly isCampaignMembers = computed(() => {
    return this.router.url.includes('/campaign-members');
  });
  protected readonly isCampaignSettings = computed(() => {
    return this.router.url.includes('/campaign-settings');
  });

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
}
