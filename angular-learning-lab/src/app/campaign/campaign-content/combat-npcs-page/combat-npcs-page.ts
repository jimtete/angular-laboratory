import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';

import { ApiError, MonsterApiService, MonsterListModel, MonsterModel } from '../../../Infrastructure';
import { ModalHelper } from '../../../shared/helpers/modal.helper';
import { MONSTER_ABILITY_OPTIONS } from '../../campaign-assets/monster-form-options';
import {
  CombatMonsterCard,
  CombatMonsterCardModel,
} from './combat-monster-card/combat-monster-card';

interface MonsterSearchResult {
  monster: MonsterListModel;
  isSelected: boolean;
}

@Component({
  selector: 'app-combat-npcs-page',
  imports: [CombatMonsterCard],
  templateUrl: './combat-npcs-page.html',
  styleUrl: './combat-npcs-page.css',
})
export class CombatNpcsPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly monsterApiService = inject(MonsterApiService);
  private readonly modalHelper = inject(ModalHelper);

  protected readonly campaignMonsters = signal<MonsterModel[]>([]);
  protected readonly allMonsters = signal<MonsterListModel[]>([]);
  protected readonly isLoadingCampaignMonsters = signal(false);
  protected readonly isLoadingMonsterSearch = signal(false);
  protected readonly isAddingMonster = signal(false);
  protected readonly isMonsterSearchOpen = signal(false);
  protected readonly monsterSearchDraft = signal('');
  protected readonly selectedMonsterDraft = signal<MonsterListModel | null>(null);
  protected readonly combatMonsterCards = computed(() => (
    this.campaignMonsters().map((monster) => this.toCombatMonsterCard(monster))
  ));
  protected readonly filteredMonsterSearchResults = computed<MonsterSearchResult[]>(() => {
    const query = this.normalizeText(this.monsterSearchDraft()).toLowerCase();
    const selectedMonsterIds = new Set(this.campaignMonsters().map((monster) => monster.id));

    return this.allMonsters()
      .filter((monster) => this.matchesMonsterQuery(monster, query))
      .map((monster) => ({
        monster,
        isSelected: selectedMonsterIds.has(monster.id),
      }));
  });

  ngOnInit(): void {
    this.loadCampaignMonsters();
  }

  protected openMonsterSearch(): void {
    this.monsterSearchDraft.set('');
    this.selectedMonsterDraft.set(null);
    this.isMonsterSearchOpen.set(true);

    if (this.allMonsters().length === 0) {
      this.loadMonsterSearch();
    }
  }

  protected closeMonsterSearch(): void {
    if (!this.isAddingMonster()) {
      this.isMonsterSearchOpen.set(false);
      this.selectedMonsterDraft.set(null);
    }
  }

  protected setMonsterSearchDraft(event: Event): void {
    this.monsterSearchDraft.set((event.target as HTMLInputElement).value);
  }

  protected selectMonsterSearchResult(result: MonsterSearchResult): void {
    if (!result.isSelected) {
      this.selectedMonsterDraft.set(result.monster);
    }
  }

  protected isSelectedMonsterDraft(monster: MonsterListModel): boolean {
    return this.selectedMonsterDraft()?.id === monster.id;
  }

  protected addSelectedMonsterToCampaign(): void {
    const campaignId = this.getCampaignId();
    const selectedMonster = this.selectedMonsterDraft();

    if (!campaignId || !selectedMonster || this.isAddingMonster()) {
      return;
    }

    this.isAddingMonster.set(true);

    this.monsterApiService
      .addMonsterToCampaign(campaignId, selectedMonster.id)
      .pipe(finalize(() => this.isAddingMonster.set(false)))
      .subscribe({
        next: (response) => {
          const addedMonster = response.data ?? this.toMonsterModel(selectedMonster);

          this.campaignMonsters.update((monsters) => (
            monsters.some((monster) => monster.id === addedMonster.id)
              ? monsters
              : [...monsters, addedMonster].sort((first, second) => (
                first.name.localeCompare(second.name) || first.id - second.id
              ))
          ));
          this.isMonsterSearchOpen.set(false);
          this.selectedMonsterDraft.set(null);
          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Monster could not be added to campaign.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected openMonsterPresentation(monster: CombatMonsterCardModel): void {
    const campaignId = this.getCampaignId();

    void this.router.navigate(
      ['/assets/monsters', monster.monsterId, 'presentation'],
      {
        queryParams: {
          campaignId,
          returnTab: 'combat-npcs',
        },
      },
    );
  }

  protected monsterSummary(monster: MonsterListModel): string {
    return [monster.size, monster.race, monster.class]
      .map((value) => this.normalizeText(value))
      .filter((value) => value.length > 0)
      .join(' - ');
  }

  private loadCampaignMonsters(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingCampaignMonsters()) {
      return;
    }

    this.isLoadingCampaignMonsters.set(true);

    this.monsterApiService
      .fetchCampaignMonsterDetails(campaignId)
      .pipe(finalize(() => this.isLoadingCampaignMonsters.set(false)))
      .subscribe({
        next: (response) => {
          this.campaignMonsters.set(response.data ?? []);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign monsters could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadMonsterSearch(): void {
    if (this.isLoadingMonsterSearch()) {
      return;
    }

    this.isLoadingMonsterSearch.set(true);

    this.monsterApiService
      .fetchMonsters()
      .pipe(finalize(() => this.isLoadingMonsterSearch.set(false)))
      .subscribe({
        next: (response) => {
          this.allMonsters.set(response.data ?? []);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Monsters could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private toCombatMonsterCard(monster: MonsterModel): CombatMonsterCardModel {
    return {
      monsterId: monster.id,
      name: monster.name,
      race: monster.race || 'Unknown Race',
      className: monster.class || 'Unknown Class',
      stats: this.toAbilityScores(monster),
    };
  }

  private toAbilityScores(monster: MonsterModel): number[] {
    const abilityScores = new Map(monster.abilities.map((ability) => [
      ability.name.toLowerCase(),
      ability.value ?? 10,
    ]));

    return MONSTER_ABILITY_OPTIONS.map((abilityName) => (
      abilityScores.get(abilityName.toLowerCase()) ?? 10
    ));
  }

  private toMonsterModel(monster: MonsterListModel): MonsterModel {
    return {
      ...monster,
      abilities: [],
      proficiencies: [],
      spellcasting: null,
      features: [],
      notes: null,
    };
  }

  private matchesMonsterQuery(monster: MonsterListModel, query: string): boolean {
    if (!query) {
      return true;
    }

    return [
      monster.name,
      monster.size,
      monster.race,
      monster.class,
      ...(monster.tags ?? []),
    ].some((value) => this.normalizeText(value).toLowerCase().includes(query));
  }

  private getCampaignId(): string | null {
    return this.route.pathFromRoot
      .map((candidate) => candidate.snapshot.paramMap.get('campaignId'))
      .find((campaignId): campaignId is string => !!campaignId) ?? null;
  }

  private normalizeText(value: string | null | undefined): string {
    return value?.trim() ?? '';
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
