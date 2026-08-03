import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LucideArrowLeft } from '@lucide/angular';
import { finalize } from 'rxjs';

import {
  ApiError,
  MonsterApiService,
  MonsterFeatureCategory,
  MonsterFeatureModel,
  MonsterModel,
} from '../../../Infrastructure';
import { ModalHelper } from '../../../shared/helpers/modal.helper';
import { MONSTER_ABILITY_OPTIONS } from '../monster-form-options';

@Component({
  selector: 'app-monster-presentation-sheet',
  imports: [LucideArrowLeft],
  templateUrl: './monster-presentation-sheet.html',
  styleUrl: './monster-presentation-sheet.css',
})
export class MonsterPresentationSheet implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly monsterApiService = inject(MonsterApiService);
  private readonly modalHelper = inject(ModalHelper);

  protected readonly monster = signal<MonsterModel | null>(null);
  protected readonly isLoadingMonster = signal(false);
  protected readonly isRemovingFromCampaign = signal(false);
  protected readonly loadError = signal('');
  protected readonly abilityNames = MONSTER_ABILITY_OPTIONS;

  ngOnInit(): void {
    this.loadMonster();
  }

  protected goBack(): void {
    const campaignId = this.getReturnCampaignId();

    if (campaignId && this.isCombatNpcsReturnContext()) {
      void this.router.navigate(
        ['/campaigns', campaignId, 'campaign-content'],
        { queryParams: { tab: 'combat-npcs' } },
      );
      return;
    }

    void this.router.navigate(['/assets']);
  }

  protected canRemoveFromCampaign(): boolean {
    return !!this.getReturnCampaignId() && this.isCombatNpcsReturnContext();
  }

  protected removeFromCampaign(): void {
    const campaignId = this.getReturnCampaignId();
    const monsterId = this.getMonsterId();

    if (!campaignId || !monsterId || this.isRemovingFromCampaign()) {
      return;
    }

    this.isRemovingFromCampaign.set(true);

    this.monsterApiService
      .removeMonsterFromCampaign(campaignId, monsterId)
      .pipe(finalize(() => this.isRemovingFromCampaign.set(false)))
      .subscribe({
        next: (response) => {
          this.modalHelper.showSuccess(response.message);
          this.goBack();
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Monster could not be removed from campaign.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected abilityScore(monster: MonsterModel, abilityName: string): number {
    return monster.abilities.find((ability) => (
      ability.name.toLowerCase() === abilityName.toLowerCase()
    ))?.value ?? 10;
  }

  protected featureSections(monster: MonsterModel): { label: string; features: MonsterFeatureModel[] }[] {
    const sections = [
      { category: MonsterFeatureCategory.PassiveTrait, label: 'Passive Traits' },
      { category: MonsterFeatureCategory.Action, label: 'Actions' },
      { category: MonsterFeatureCategory.BonusAction, label: 'Bonus Actions' },
      { category: MonsterFeatureCategory.Reaction, label: 'Reactions' },
      { category: MonsterFeatureCategory.FreeAction, label: 'Free Actions / Switches' },
      { category: MonsterFeatureCategory.LegendaryAction, label: 'Legendary Actions' },
      { category: MonsterFeatureCategory.MythicAction, label: 'Mythic Actions' },
      { category: MonsterFeatureCategory.Spell, label: 'Spells' },
    ];

    return sections
      .map((section) => ({
        label: section.label,
        features: monster.features
          .filter((feature) => this.toFeatureCategory(feature.category) === section.category)
          .sort((first, second) => first.sortOrder - second.sortOrder || first.id - second.id),
      }))
      .filter((section) => section.features.length > 0);
  }

  protected featureMetadata(feature: MonsterFeatureModel): string[] {
    return [
      feature.usageNote ?? '',
      feature.resourceCost !== null ? `Cost ${feature.resourceCost}` : '',
      feature.isSpell ? 'Spell-like' : '',
      feature.spellLevel !== null ? `Level ${feature.spellLevel}` : '',
      feature.castingTime ?? '',
      feature.range ? `Range ${feature.range}` : '',
      feature.duration ?? '',
      feature.concentration ? 'Concentration' : '',
    ].filter((value) => value.length > 0);
  }

  private loadMonster(): void {
    const monsterId = this.getMonsterId();

    if (!monsterId) {
      this.loadError.set('Monster id is invalid.');
      return;
    }

    this.isLoadingMonster.set(true);
    this.loadError.set('');

    this.monsterApiService
      .fetchMonster(monsterId)
      .pipe(finalize(() => this.isLoadingMonster.set(false)))
      .subscribe({
        next: (response) => {
          if (!response.data) {
            this.loadError.set('Monster was not found.');
            return;
          }

          this.monster.set(response.data);
        },
        error: (error: unknown) => {
          const message = this.getErrorMessage(error, 'Monster could not be loaded.');

          this.loadError.set(message);
          this.modalHelper.showError(message, { statusCode: this.getErrorStatus(error) });
        },
      });
  }

  private getMonsterId(): number | null {
    const monsterId = Number(this.route.snapshot.paramMap.get('monsterId'));

    return Number.isInteger(monsterId) && monsterId > 0
      ? monsterId
      : null;
  }

  private getReturnCampaignId(): string | null {
    return this.route.snapshot.queryParamMap.get('campaignId');
  }

  private isCombatNpcsReturnContext(): boolean {
    return this.route.snapshot.queryParamMap.get('returnTab') === 'combat-npcs';
  }

  private toFeatureCategory(
    category: MonsterFeatureModel['category'],
  ): MonsterFeatureCategory | null {
    if (typeof category === 'number') {
      return category in MonsterFeatureCategory ? category as MonsterFeatureCategory : null;
    }

    const numericCategory = Number(category);

    if (Number.isInteger(numericCategory) && numericCategory in MonsterFeatureCategory) {
      return numericCategory as MonsterFeatureCategory;
    }

    return MonsterFeatureCategory[category as keyof typeof MonsterFeatureCategory] ?? null;
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (this.isApiError(error) || error instanceof Error) {
      return error.message;
    }

    return fallback;
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
