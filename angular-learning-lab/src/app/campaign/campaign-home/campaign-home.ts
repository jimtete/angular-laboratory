import { Component, OnInit, computed, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { CampaignCacheService } from '../../Infrastructure';

@Component({
  selector: 'app-campaign-home',
  templateUrl: './campaign-home.html',
  styleUrl: './campaign-home.css',
})
export class CampaignHome implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly campaignCache = inject(CampaignCacheService);

  protected readonly campaignId = computed(() => {
    return this.route.parent?.snapshot.paramMap.get('campaignId') ?? null;
  });
  protected readonly campaignTitle = computed(() => {
    const campaignId = this.campaignId();

    return this.campaignCache.campaigns()
      .find((campaign) => campaign.campaignId === campaignId)
      ?.campaignName ?? '';
  });
  protected readonly currentMembers = 3;
  protected readonly maxMembers = 6;

  ngOnInit(): void {
    this.campaignCache.loadAvailableCampaigns().subscribe({
      error: () => {},
    });
  }
}
