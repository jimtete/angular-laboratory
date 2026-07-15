import { Component, OnInit, computed, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { CampaignCacheService, CampaignInformationCacheService } from '../../Infrastructure';

@Component({
  selector: 'app-campaign-home',
  templateUrl: './campaign-home.html',
  styleUrl: './campaign-home.css',
})
export class CampaignHome implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly campaignCache = inject(CampaignCacheService);
  private readonly campaignInformationCache = inject(CampaignInformationCacheService);

  protected readonly campaignId = computed(() => {
    return this.route.parent?.snapshot.paramMap.get('campaignId') ?? null;
  });
  protected readonly campaignTitle = computed(() => {
    const campaignId = this.campaignId();

    return this.campaignCache.campaigns()
      .find((campaign) => campaign.campaignId === campaignId)
      ?.campaignName ?? '';
  });
  protected readonly currentMembers = computed(
    () => this.campaignInformationCache.joinedMembers().length,
  );
  protected readonly maxMembers = 6;
  protected readonly membersRingFill = computed(() => {
    const fillPercentage = (this.currentMembers() / this.maxMembers) * 100;
    const boundedFillPercentage = Math.max(0, Math.min(100, fillPercentage));

    return `${boundedFillPercentage}%`;
  });

  ngOnInit(): void {
    this.campaignCache.loadAvailableCampaigns().subscribe({
      error: () => {},
    });
  }
}
