import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { LucideFolder, LucidePackage, LucideSkull, LucideTrash2, LucideX } from '@lucide/angular';
import { finalize } from 'rxjs';

import {
  ApiError,
  AssetItemType,
  AssetModel,
  AssetType,
  CampaignModel,
  CampaignApiService,
  CreateAssetFolderRequest,
  CreateItemAssetRequest,
  CreateMonsterRequest,
  MonsterApiService,
  MonsterListModel,
  UpdateItemAssetRequest,
} from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';
import {
  MONSTER_ABILITY_OPTIONS,
  MONSTER_RACE_OPTIONS,
  MONSTER_SIZE_OPTIONS,
} from './monster-form-options';

@Component({
  selector: 'app-campaign-assets',
  imports: [LucideFolder, LucidePackage, LucideSkull, LucideTrash2, LucideX],
  templateUrl: './campaign-assets.html',
  styleUrl: './campaign-assets.css',
})
export class CampaignAssets implements OnInit {
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly monsterApiService = inject(MonsterApiService);
  private readonly modalHelper = inject(ModalHelper);
  private readonly router = inject(Router);

  protected readonly assets = signal<AssetModel[]>([]);
  protected readonly monsters = signal<MonsterListModel[]>([]);
  protected readonly availableCampaigns = signal<CampaignModel[]>([]);
  protected readonly folderStack = signal<AssetModel[]>([]);
  protected readonly isLoadingAssets = signal(false);
  protected readonly isLoadingMonsters = signal(false);
  protected readonly isLoadingCampaigns = signal(false);
  protected readonly isCreateFolderDialogOpen = signal(false);
  protected readonly isCreateItemDialogOpen = signal(false);
  protected readonly isCampaignPickerOpen = signal(false);
  protected readonly isCreatingFolder = signal(false);
  protected readonly isCreatingItem = signal(false);
  protected readonly isCreatingMonster = signal(false);
  protected readonly editingItemAsset = signal<AssetModel | null>(null);
  protected readonly monsterDeleteConfirmation = signal<MonsterListModel | null>(null);
  protected readonly deletingMonsterId = signal<number | null>(null);
  protected readonly folderNameDraft = signal('');
  protected readonly itemNameDraft = signal('');
  protected readonly itemDescriptionDraft = signal('');
  protected readonly monsterNameDraft = signal('');
  protected readonly monsterSizeDraft = signal('');
  protected readonly monsterRaceDraft = signal('');
  protected readonly monsterClassDraft = signal('');
  protected readonly monsterTagsDraft = signal<string[]>([]);
  protected readonly monsterTagDraft = signal('');
  protected readonly monsterNotesDraft = signal('');
  protected readonly selectedAssetTypeDraft = signal<'Item' | 'Monster'>('Item');
  protected readonly selectedItemTypeDraft = signal<AssetItemType>(AssetItemType.Equipment);
  protected readonly selectedCampaignIds = signal<string[]>([]);
  protected readonly currentParentAssetId = computed(() => {
    const folders = this.folderStack();

    return folders.length > 0 ? folders[folders.length - 1].id : null;
  });
  protected readonly currentFolderLabel = computed(() => {
    const folders = this.folderStack();

    return folders.length > 0 ? folders[folders.length - 1].name : 'Assets';
  });
  protected readonly canCreateFolder = computed(() => (
    this.normalizeText(this.folderNameDraft()).length > 0 &&
    !this.isCreatingFolder()
  ));
  protected readonly canCreateItem = computed(() => (
    this.normalizeText(this.itemNameDraft()).length > 0 &&
    this.selectedAssetTypeDraft() === 'Item' &&
    !this.isCreatingItem()
  ));
  protected readonly canCreateMonster = computed(() => (
    this.normalizeText(this.monsterNameDraft()).length > 0 &&
    this.selectedAssetTypeDraft() === 'Monster' &&
    !this.isCreatingMonster()
  ));
  protected readonly visibleMonsters = computed(() => (
    this.currentParentAssetId() === null ? this.monsters() : []
  ));
  protected readonly isEditingItem = computed(() => this.editingItemAsset() !== null);
  protected readonly itemDialogTitle = computed(() => (
    this.isEditingItem() ? 'Edit asset' : 'Create asset'
  ));
  protected readonly itemDialogButtonLabel = computed(() => {
    if (this.isCreatingItem() || this.isCreatingMonster()) {
      return this.isEditingItem() ? 'Saving...' : 'Creating...';
    }

    return this.isEditingItem() ? 'Save' : 'Create';
  });
  protected readonly selectedCampaigns = computed(() => {
    const selectedIds = new Set(this.selectedCampaignIds());

    return this.availableCampaigns().filter((campaign) => selectedIds.has(campaign.campaignId));
  });
  protected readonly availableCampaignOptions = computed(() => {
    const selectedIds = new Set(this.selectedCampaignIds());

    return this.availableCampaigns().filter((campaign) => !selectedIds.has(campaign.campaignId));
  });
  protected readonly assetTypeOptions = ['Item', 'Monster'];
  protected readonly monsterSizeOptions = MONSTER_SIZE_OPTIONS;
  protected readonly monsterRaceOptions = MONSTER_RACE_OPTIONS;
  protected readonly itemTypeOptions = [
    { value: AssetItemType.Equipment, label: 'Equipment' },
    { value: AssetItemType.Weapon, label: 'Weapon' },
    { value: AssetItemType.Armor, label: 'Armor' },
    { value: AssetItemType.Other, label: 'Other' },
  ];

  ngOnInit(): void {
    this.loadAssets();
    this.loadMonsters();
    this.loadCampaigns();
  }

  protected openCreateFolderDialog(): void {
    this.folderNameDraft.set('');
    this.isCreateFolderDialogOpen.set(true);
  }

  protected closeCreateFolderDialog(): void {
    if (!this.isCreatingFolder()) {
      this.isCreateFolderDialogOpen.set(false);
    }
  }

  protected openCreateItemDialog(): void {
    this.editingItemAsset.set(null);
    this.itemNameDraft.set('');
    this.itemDescriptionDraft.set('');
    this.resetMonsterDrafts();
    this.selectedAssetTypeDraft.set('Item');
    this.selectedItemTypeDraft.set(AssetItemType.Equipment);
    this.selectedCampaignIds.set([]);
    this.isCampaignPickerOpen.set(false);
    this.isCreateItemDialogOpen.set(true);
    this.loadCampaigns();
  }

  protected closeCreateItemDialog(): void {
    if (!this.isCreatingItem() && !this.isCreatingMonster()) {
      this.isCreateItemDialogOpen.set(false);
      this.editingItemAsset.set(null);
      this.isCampaignPickerOpen.set(false);
    }
  }

  protected setFolderNameDraft(event: Event): void {
    this.folderNameDraft.set((event.target as HTMLInputElement).value);
  }

  protected setItemNameDraft(event: Event): void {
    this.itemNameDraft.set((event.target as HTMLInputElement).value);
  }

  protected setItemDescriptionDraft(event: Event): void {
    this.itemDescriptionDraft.set((event.target as HTMLTextAreaElement).value);
  }

  protected setSelectedAssetTypeDraft(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;

    this.selectedAssetTypeDraft.set(value === 'Monster' ? 'Monster' : 'Item');
  }

  protected setSelectedItemTypeDraft(event: Event): void {
    this.selectedItemTypeDraft.set(Number((event.target as HTMLSelectElement).value));
  }

  protected setMonsterNameDraft(event: Event): void {
    this.monsterNameDraft.set((event.target as HTMLInputElement).value);
  }

  protected setMonsterSizeDraft(event: Event): void {
    this.monsterSizeDraft.set((event.target as HTMLInputElement).value);
  }

  protected setMonsterRaceDraft(event: Event): void {
    this.monsterRaceDraft.set((event.target as HTMLInputElement).value);
  }

  protected setMonsterClassDraft(event: Event): void {
    this.monsterClassDraft.set((event.target as HTMLInputElement).value);
  }

  protected setMonsterTagDraft(event: Event): void {
    this.monsterTagDraft.set((event.target as HTMLInputElement).value);
  }

  protected setMonsterNotesDraft(event: Event): void {
    this.monsterNotesDraft.set((event.target as HTMLTextAreaElement).value);
  }

  protected addMonsterTagFromDraft(): void {
    const tag = this.normalizeText(this.monsterTagDraft());

    if (!tag) {
      return;
    }

    const existingTags = new Set(this.monsterTagsDraft().map((value) => value.toLowerCase()));

    if (!existingTags.has(tag.toLowerCase())) {
      this.monsterTagsDraft.update((tags) => [...tags, tag]);
    }

    this.monsterTagDraft.set('');
  }

  protected handleMonsterTagKeydown(event: KeyboardEvent): void {
    if (event.key !== 'Enter') {
      return;
    }

    event.preventDefault();
    this.addMonsterTagFromDraft();
  }

  protected removeMonsterTag(tag: string): void {
    this.monsterTagsDraft.update((tags) => tags.filter((value) => value !== tag));
  }

  protected openEditItemDialog(asset: AssetModel): void {
    if (this.isFolder(asset)) {
      return;
    }

    this.editingItemAsset.set(asset);
    this.itemNameDraft.set(asset.name);
    this.itemDescriptionDraft.set(asset.description);
    this.selectedAssetTypeDraft.set('Item');
    this.selectedItemTypeDraft.set(this.toItemType(asset.itemType) ?? AssetItemType.Equipment);
    this.selectedCampaignIds.set([...(asset.campaignIds ?? [])]);
    this.isCampaignPickerOpen.set(false);
    this.isCreateItemDialogOpen.set(true);
    this.loadCampaigns();
  }

  protected toggleCampaignPicker(): void {
    if (!this.isLoadingCampaigns()) {
      this.isCampaignPickerOpen.update((isOpen) => !isOpen);
    }
  }

  protected addCampaign(campaignId: string): void {
    this.selectedCampaignIds.update((campaignIds) => (
      campaignIds.includes(campaignId) ? campaignIds : [...campaignIds, campaignId]
    ));
    this.isCampaignPickerOpen.set(false);
  }

  protected removeCampaign(campaignId: string): void {
    this.selectedCampaignIds.update((campaignIds) => (
      campaignIds.filter((selectedCampaignId) => selectedCampaignId !== campaignId)
    ));
  }

  protected openFolder(asset: AssetModel): void {
    if (!this.isFolder(asset)) {
      return;
    }

    this.folderStack.update((folders) => [...folders, asset]);
    this.loadAssets();
  }

  protected openRootFolder(): void {
    this.folderStack.set([]);
    this.loadAssets();
  }

  protected openBreadcrumbFolder(index: number): void {
    this.folderStack.update((folders) => folders.slice(0, index + 1));
    this.loadAssets();
  }

  protected createFolder(): void {
    if (!this.canCreateFolder()) {
      return;
    }

    const request: CreateAssetFolderRequest = {
      parentAssetId: this.currentParentAssetId(),
      name: this.normalizeText(this.folderNameDraft()),
      description: '',
    };

    this.isCreatingFolder.set(true);

    this.campaignApiService
      .createAssetFolder(request)
      .pipe(finalize(() => this.isCreatingFolder.set(false)))
      .subscribe({
        next: () => {
          this.isCreateFolderDialogOpen.set(false);
          this.loadAssets();
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Asset folder could not be created.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected createItem(): void {
    if (this.selectedAssetTypeDraft() === 'Monster') {
      this.createMonster();
      return;
    }

    if (!this.canCreateItem()) {
      return;
    }

    const editingItem = this.editingItemAsset();
    const request: CreateItemAssetRequest | UpdateItemAssetRequest = {
      parentAssetId: editingItem?.parentAssetId ?? this.currentParentAssetId(),
      name: this.normalizeText(this.itemNameDraft()),
      description: this.normalizeText(this.itemDescriptionDraft()),
      itemType: this.selectedItemTypeDraft(),
      campaignIds: this.getSelectedCampaignIdsRequest(),
    };

    this.isCreatingItem.set(true);

    const saveItem$ = editingItem
      ? this.campaignApiService.updateItemAsset(editingItem.id, request)
      : this.campaignApiService.createItemAsset(request);

    saveItem$
      .pipe(finalize(() => this.isCreatingItem.set(false)))
      .subscribe({
        next: () => {
          this.isCreateItemDialogOpen.set(false);
          this.editingItemAsset.set(null);
          this.isCampaignPickerOpen.set(false);
          this.loadAssets();
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(
              error,
              editingItem ? 'Item asset could not be saved.' : 'Item asset could not be created.',
            ),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected openMonster(monster: MonsterListModel): void {
    void this.router.navigate(['/assets/monsters', monster.id]);
  }

  protected confirmDeleteMonster(monster: MonsterListModel, event: Event): void {
    event.stopPropagation();
    this.monsterDeleteConfirmation.set(monster);
  }

  protected cancelDeleteMonster(): void {
    if (!this.deletingMonsterId()) {
      this.monsterDeleteConfirmation.set(null);
    }
  }

  protected deleteMonster(): void {
    const monster = this.monsterDeleteConfirmation();

    if (!monster || this.deletingMonsterId()) {
      return;
    }

    this.deletingMonsterId.set(monster.id);

    this.monsterApiService
      .deleteMonster(monster.id)
      .pipe(finalize(() => this.deletingMonsterId.set(null)))
      .subscribe({
        next: (response) => {
          this.monsterDeleteConfirmation.set(null);
          this.monsters.update((monsters) => (
            monsters.filter((candidate) => candidate.id !== monster.id)
          ));
          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Monster could not be deleted.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected isFolder(asset: AssetModel): boolean {
    return this.toAssetType(asset.assetType) === AssetType.Folder;
  }

  protected getItemTypeLabel(asset: AssetModel): string {
    const itemType = this.toItemType(asset.itemType);

    return this.itemTypeOptions.find((option) => option.value === itemType)?.label ?? 'Item';
  }

  protected getCampaignPickerLabel(): string {
    if (this.isLoadingCampaigns()) {
      return 'Loading campaigns...';
    }

    return this.availableCampaignOptions().length > 0
      ? 'Add campaign'
      : 'No more campaigns';
  }

  protected getMonsterSummary(monster: MonsterListModel): string {
    return [monster.size, monster.race, monster.class]
      .map((value) => this.normalizeText(value))
      .filter((value) => value.length > 0)
      .join(' - ');
  }

  protected getMonsterTags(monster: MonsterListModel): string[] {
    return (monster.tags ?? [])
      .map((tag) => this.normalizeText(tag))
      .filter((tag) => tag.length > 0);
  }

  private loadAssets(): void {
    if (this.isLoadingAssets()) {
      return;
    }

    this.isLoadingAssets.set(true);

    this.campaignApiService
      .fetchAssets(this.currentParentAssetId())
      .pipe(finalize(() => this.isLoadingAssets.set(false)))
      .subscribe({
        next: (response) => {
          this.assets.set(response.data ?? []);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Assets could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadMonsters(): void {
    if (this.isLoadingMonsters()) {
      return;
    }

    this.isLoadingMonsters.set(true);

    this.monsterApiService
      .fetchMonsters()
      .pipe(finalize(() => this.isLoadingMonsters.set(false)))
      .subscribe({
        next: (response) => {
          this.monsters.set(response.data ?? []);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Monsters could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private createMonster(): void {
    if (!this.canCreateMonster()) {
      return;
    }

    const request: CreateMonsterRequest = {
      name: this.normalizeText(this.monsterNameDraft()),
      size: this.toNullableText(this.monsterSizeDraft()),
      race: this.toNullableText(this.monsterRaceDraft()),
      class: this.toNullableText(this.monsterClassDraft()),
      tags: this.monsterTagsDraft().length > 0 ? this.monsterTagsDraft() : null,
      abilities: MONSTER_ABILITY_OPTIONS.map((ability) => ({
        name: ability,
        value: null,
        modifier: null,
        notes: null,
      })),
      proficiencies: null,
      spellcasting: null,
      features: null,
      notes: this.toNullableText(this.monsterNotesDraft()),
    };

    this.isCreatingMonster.set(true);

    this.monsterApiService
      .createMonster(request)
      .pipe(finalize(() => this.isCreatingMonster.set(false)))
      .subscribe({
        next: (response) => {
          this.isCreateItemDialogOpen.set(false);
          this.resetMonsterDrafts();
          this.loadMonsters();

          if (response.data) {
            void this.router.navigate(['/assets/monsters', response.data.id]);
          }
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Monster could not be created.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadCampaigns(): void {
    if (this.isLoadingCampaigns() || this.availableCampaigns().length > 0) {
      return;
    }

    this.isLoadingCampaigns.set(true);

    this.campaignApiService
      .fetchAvailableCampaigns()
      .pipe(finalize(() => this.isLoadingCampaigns.set(false)))
      .subscribe({
        next: (response) => {
          this.availableCampaigns.set(response.data ?? []);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaigns could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private toAssetType(assetType: AssetModel['assetType']): AssetType | null {
    if (typeof assetType === 'number') {
      return assetType in AssetType ? assetType as AssetType : null;
    }

    return AssetType[assetType as keyof typeof AssetType] ?? null;
  }

  private toItemType(itemType: AssetModel['itemType']): AssetItemType | null {
    if (itemType === null) {
      return null;
    }

    if (typeof itemType === 'number') {
      return itemType in AssetItemType ? itemType as AssetItemType : null;
    }

    return AssetItemType[itemType as keyof typeof AssetItemType] ?? null;
  }

  private normalizeText(value: string | null | undefined): string {
    return value?.trim() ?? '';
  }

  private toNullableText(value: string | null | undefined): string | null {
    const normalizedValue = this.normalizeText(value);

    return normalizedValue || null;
  }

  private resetMonsterDrafts(): void {
    this.monsterNameDraft.set('');
    this.monsterSizeDraft.set('');
    this.monsterRaceDraft.set('');
    this.monsterClassDraft.set('');
    this.monsterTagsDraft.set([]);
    this.monsterTagDraft.set('');
    this.monsterNotesDraft.set('');
  }

  private getSelectedCampaignIdsRequest(): string[] | null {
    const campaignIds = this.selectedCampaignIds();

    return campaignIds.length > 0 ? campaignIds : null;
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
