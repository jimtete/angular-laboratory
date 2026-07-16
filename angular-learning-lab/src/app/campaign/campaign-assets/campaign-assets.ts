import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { LucideFolder, LucidePackage, LucideX } from '@lucide/angular';
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
  UpdateItemAssetRequest,
} from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';

@Component({
  selector: 'app-campaign-assets',
  imports: [LucideFolder, LucidePackage, LucideX],
  templateUrl: './campaign-assets.html',
  styleUrl: './campaign-assets.css',
})
export class CampaignAssets implements OnInit {
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly modalHelper = inject(ModalHelper);

  protected readonly assets = signal<AssetModel[]>([]);
  protected readonly availableCampaigns = signal<CampaignModel[]>([]);
  protected readonly folderStack = signal<AssetModel[]>([]);
  protected readonly isLoadingAssets = signal(false);
  protected readonly isLoadingCampaigns = signal(false);
  protected readonly isCreateFolderDialogOpen = signal(false);
  protected readonly isCreateItemDialogOpen = signal(false);
  protected readonly isCampaignPickerOpen = signal(false);
  protected readonly isCreatingFolder = signal(false);
  protected readonly isCreatingItem = signal(false);
  protected readonly editingItemAsset = signal<AssetModel | null>(null);
  protected readonly folderNameDraft = signal('');
  protected readonly itemNameDraft = signal('');
  protected readonly itemDescriptionDraft = signal('');
  protected readonly selectedAssetTypeDraft = signal<'Item'>('Item');
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
  protected readonly isEditingItem = computed(() => this.editingItemAsset() !== null);
  protected readonly itemDialogTitle = computed(() => (
    this.isEditingItem() ? 'Edit asset' : 'Create asset'
  ));
  protected readonly itemDialogButtonLabel = computed(() => {
    if (this.isCreatingItem()) {
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
  protected readonly assetTypeOptions = ['Item'];
  protected readonly itemTypeOptions = [
    { value: AssetItemType.Equipment, label: 'Equipment' },
    { value: AssetItemType.Weapon, label: 'Weapon' },
    { value: AssetItemType.Armor, label: 'Armor' },
    { value: AssetItemType.Other, label: 'Other' },
  ];

  ngOnInit(): void {
    this.loadAssets();
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
    this.selectedAssetTypeDraft.set('Item');
    this.selectedItemTypeDraft.set(AssetItemType.Equipment);
    this.selectedCampaignIds.set([]);
    this.isCampaignPickerOpen.set(false);
    this.isCreateItemDialogOpen.set(true);
    this.loadCampaigns();
  }

  protected closeCreateItemDialog(): void {
    if (!this.isCreatingItem()) {
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

  protected setSelectedItemTypeDraft(event: Event): void {
    this.selectedItemTypeDraft.set(Number((event.target as HTMLSelectElement).value));
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
