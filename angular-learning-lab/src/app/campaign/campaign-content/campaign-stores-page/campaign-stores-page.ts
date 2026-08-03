import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  LucideBookOpen,
  LucideFish,
  LucideFlaskConical,
  LucideHammer,
  LucideHandCoins,
  LucideHeartPulse,
  LucidePackage,
  LucidePlus,
  LucideShield,
  LucideShirt,
  LucideSparkles,
  LucideX,
} from '@lucide/angular';
import { finalize } from 'rxjs';

import {
  ApiError,
  AssetModel,
  CampaignApiService,
  CampaignStoreModel,
  CreateStoreItemRequest,
  StoreType,
} from '../../../Infrastructure';
import { ModalHelper } from '../../../shared/helpers/modal.helper';

interface StoreTypeOption {
  value: StoreType;
  label: string;
  className: string;
}

interface StoreTypeGroup {
  option: StoreTypeOption;
  stores: CampaignStoreModel[];
}

type StoreFormStep = 'type' | 'details' | 'inventory';

interface StoreItemDraft {
  draftId: number;
  assetId: number;
  itemName: string;
  itemDescription: string;
  itemPrice: string;
  quantity: string;
}

@Component({
  selector: 'app-campaign-stores-page',
  imports: [
    LucideBookOpen,
    LucideFish,
    LucideFlaskConical,
    LucideHammer,
    LucideHandCoins,
    LucideHeartPulse,
    LucidePackage,
    LucidePlus,
    LucideShield,
    LucideShirt,
    LucideSparkles,
    LucideX,
  ],
  templateUrl: './campaign-stores-page.html',
  styleUrl: './campaign-stores-page.css',
})
export class CampaignStoresPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly modalHelper = inject(ModalHelper);

  protected readonly StoreType = StoreType;
  protected readonly isCreateStoreDialogOpen = signal(false);
  protected readonly isAssetBrowserOpen = signal(false);
  protected readonly isLoadingStores = signal(false);
  protected readonly isLoadingAvailableItems = signal(false);
  protected readonly isCreatingStore = signal(false);
  protected readonly storeFormStep = signal<StoreFormStep>('type');
  protected readonly selectedStoreTypeDraft = signal<StoreType | null>(null);
  protected readonly storeLocationDraft = signal('');
  protected readonly storeNameDraft = signal('');
  protected readonly storeDescriptionDraft = signal('');
  protected readonly storeItemDrafts = signal<StoreItemDraft[]>([]);
  protected readonly stores = signal<CampaignStoreModel[]>([]);
  protected readonly availableItemAssets = signal<AssetModel[]>([]);
  protected readonly storeGroups = computed<StoreTypeGroup[]>(() => (
    this.storeTypeOptions
      .map((option) => ({
        option,
        stores: this.stores().filter((store) => this.storeTypeFor(store) === option.value),
      }))
      .filter((group) => group.stores.length > 0)
  ));
  private nextStoreItemDraftId = 1;
  protected readonly storeTypeOptions: StoreTypeOption[] = [
    { value: StoreType.General, label: 'General', className: 'general' },
    { value: StoreType.Blacksmith, label: 'Blacksmith', className: 'blacksmith' },
    { value: StoreType.Medic, label: 'Medic', className: 'medic' },
    { value: StoreType.Bookstore, label: 'Bookstore', className: 'bookstore' },
    { value: StoreType.Alchemic, label: 'Alchemic', className: 'alchemic' },
    { value: StoreType.BlackMarket, label: 'Black Market', className: 'black-market' },
    { value: StoreType.FishMarket, label: 'Fish Market', className: 'fish-market' },
    { value: StoreType.Armorsmith, label: 'Armorsmith', className: 'armorsmith' },
    { value: StoreType.ClothesStore, label: 'Clothes Store', className: 'clothes-store' },
    { value: StoreType.MagicStore, label: 'Magic Store', className: 'magic-store' },
  ];

  ngOnInit(): void {
    this.loadCampaignStores();
  }

  protected openCreateStoreDialog(): void {
    this.storeFormStep.set('type');
    this.selectedStoreTypeDraft.set(null);
    this.storeLocationDraft.set('');
    this.storeNameDraft.set('');
    this.storeDescriptionDraft.set('');
    this.storeItemDrafts.set([]);
    this.isCreateStoreDialogOpen.set(true);
  }

  protected closeCreateStoreDialog(): void {
    if (this.isCreatingStore()) {
      return;
    }

    this.isCreateStoreDialogOpen.set(false);
    this.isAssetBrowserOpen.set(false);
    this.storeFormStep.set('type');
    this.selectedStoreTypeDraft.set(null);
    this.storeLocationDraft.set('');
    this.storeNameDraft.set('');
    this.storeDescriptionDraft.set('');
    this.storeItemDrafts.set([]);
  }

  protected selectStoreType(storeType: StoreType): void {
    this.selectedStoreTypeDraft.set(storeType);
  }

  protected continueCreateStore(): void {
    if (this.storeFormStep() === 'type' && this.selectedStoreTypeDraft() !== null) {
      this.storeFormStep.set('details');
      return;
    }

    if (this.storeFormStep() === 'details' && this.normalizeText(this.storeLocationDraft()).length > 0) {
      this.storeFormStep.set('inventory');
    }
  }

  protected returnToStoreTypeStep(): void {
    this.storeFormStep.set('type');
  }

  protected returnToStoreDetailsStep(): void {
    this.storeFormStep.set('details');
  }

  protected setStoreLocationDraft(event: Event): void {
    this.storeLocationDraft.set((event.target as HTMLInputElement).value);
  }

  protected setStoreNameDraft(event: Event): void {
    this.storeNameDraft.set((event.target as HTMLInputElement).value);
  }

  protected setStoreDescriptionDraft(event: Event): void {
    this.storeDescriptionDraft.set((event.target as HTMLTextAreaElement).value);
  }

  protected openAssetBrowser(): void {
    this.isAssetBrowserOpen.set(true);

    if (this.availableItemAssets().length === 0) {
      this.loadAvailableItemAssets();
    }
  }

  protected closeAssetBrowser(): void {
    if (!this.isLoadingAvailableItems()) {
      this.isAssetBrowserOpen.set(false);
    }
  }

  protected addAssetAsStoreItem(asset: AssetModel): void {
    this.storeItemDrafts.update((items) => [
      ...items,
      {
        draftId: this.nextStoreItemDraftId++,
        assetId: asset.id,
        itemName: asset.name,
        itemDescription: asset.description ?? '',
        itemPrice: '',
        quantity: '',
      },
    ]);
    this.isAssetBrowserOpen.set(false);
  }

  protected removeStoreItemDraft(draftId: number): void {
    this.storeItemDrafts.update((items) => items.filter((item) => item.draftId !== draftId));
  }

  protected setStoreItemNameDraft(draftId: number, event: Event): void {
    this.updateStoreItemDraft(draftId, {
      itemName: (event.target as HTMLInputElement).value,
    });
  }

  protected setStoreItemDescriptionDraft(draftId: number, event: Event): void {
    this.updateStoreItemDraft(draftId, {
      itemDescription: (event.target as HTMLTextAreaElement).value,
    });
  }

  protected setStoreItemPriceDraft(draftId: number, event: Event): void {
    this.updateStoreItemDraft(draftId, {
      itemPrice: (event.target as HTMLInputElement).value,
    });
  }

  protected setStoreItemQuantityDraft(draftId: number, event: Event): void {
    this.updateStoreItemDraft(draftId, {
      quantity: (event.target as HTMLInputElement).value,
    });
  }

  protected createStore(): void {
    const campaignId = this.getCampaignId();
    const selectedStoreType = this.selectedStoreTypeDraft();

    if (!campaignId || selectedStoreType === null || !this.canCreateStore() || this.isCreatingStore()) {
      return;
    }

    this.isCreatingStore.set(true);

    this.campaignApiService
      .createCampaignStore(campaignId, {
        storeType: selectedStoreType,
        storeLocation: this.normalizeText(this.storeLocationDraft()),
        storeName: this.toNullableText(this.storeNameDraft()),
        storeDescription: this.toNullableText(this.storeDescriptionDraft()),
        items: this.storeItemDrafts().map((item) => this.toStoreItemRequest(item)),
      })
      .pipe(finalize(() => this.isCreatingStore.set(false)))
      .subscribe({
        next: (response) => {
          this.modalHelper.showSuccess(response.message, { statusCode: response.statusCode });
          this.isCreatingStore.set(false);
          this.closeCreateStoreDialog();
          this.loadCampaignStores();
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign store could not be created.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected isSelectedStoreType(storeType: StoreType): boolean {
    return this.selectedStoreTypeDraft() === storeType;
  }

  protected selectedStoreTypeLabel(): string {
    return this.storeTypeOptions.find((option) => option.value === this.selectedStoreTypeDraft())?.label ?? 'Store';
  }

  protected storeDisplayName(store: CampaignStoreModel): string {
    return this.normalizeText(store.storeName) || store.storeLocation || 'Unnamed Store';
  }

  protected storeTypeFor(store: CampaignStoreModel): StoreType {
    return this.toStoreType(store.storeType);
  }

  protected storeTypeClassName(store: CampaignStoreModel): string {
    return this.storeTypeOptions.find((option) => option.value === this.storeTypeFor(store))?.className ?? 'general';
  }

  protected storeFormTitle(): string {
    switch (this.storeFormStep()) {
      case 'type':
        return 'Select Type of Store';
      case 'details':
        return `${this.selectedStoreTypeLabel()} Details`;
      case 'inventory':
        return `${this.selectedStoreTypeLabel()} Inventory`;
      default:
        return 'Create Store';
    }
  }

  protected canCreateStore(): boolean {
    return this.selectedStoreTypeDraft() !== null &&
      this.normalizeText(this.storeLocationDraft()).length > 0 &&
      this.storeItemDrafts().length > 0 &&
      this.storeItemDrafts().every((item) => this.isValidStoreItem(item));
  }

  private loadAvailableItemAssets(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingAvailableItems()) {
      return;
    }

    this.isLoadingAvailableItems.set(true);

    this.campaignApiService
      .fetchAvailableCampaignItems(campaignId)
      .pipe(finalize(() => this.isLoadingAvailableItems.set(false)))
      .subscribe({
        next: (response) => {
          this.availableItemAssets.set(response.data ?? []);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Available item assets could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadCampaignStores(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingStores()) {
      return;
    }

    this.isLoadingStores.set(true);

    this.campaignApiService
      .fetchCampaignStores(campaignId)
      .pipe(finalize(() => this.isLoadingStores.set(false)))
      .subscribe({
        next: (response) => {
          this.stores.set(response.data ?? []);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign stores could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private updateStoreItemDraft(
    draftId: number,
    changes: Partial<Omit<StoreItemDraft, 'draftId' | 'assetId'>>,
  ): void {
    this.storeItemDrafts.update((items) => items.map((item) => (
      item.draftId === draftId ? { ...item, ...changes } : item
    )));
  }

  private toStoreItemRequest(item: StoreItemDraft): CreateStoreItemRequest {
    return {
      quantity: this.toNullableNumber(item.quantity),
      itemName: this.normalizeText(item.itemName),
      itemDescription: this.toNullableText(item.itemDescription),
      itemPrice: this.toRequiredNumber(item.itemPrice),
      itemPriceDiscount: 0,
      itemPricePercentageDiscount: 0,
    };
  }

  private isValidStoreItem(item: StoreItemDraft): boolean {
    return this.normalizeText(item.itemName).length > 0 &&
      this.isValidRequiredNumber(item.itemPrice) &&
      this.isValidOptionalNumber(item.quantity);
  }

  private toRequiredNumber(value: string): number {
    return Number(this.normalizeText(value));
  }

  private toNullableNumber(value: string): number | null {
    const normalizedValue = this.normalizeText(value);
    return normalizedValue.length === 0 ? null : Number(normalizedValue);
  }

  private isValidRequiredNumber(value: string): boolean {
    const normalizedValue = this.normalizeText(value);
    const parsedValue = Number(normalizedValue);
    return normalizedValue.length > 0 && Number.isInteger(parsedValue) && parsedValue >= 0;
  }

  private isValidOptionalNumber(value: string): boolean {
    const normalizedValue = this.normalizeText(value);
    const parsedValue = Number(normalizedValue);
    return normalizedValue.length === 0 || (Number.isInteger(parsedValue) && parsedValue >= 0);
  }

  private toNullableText(value: string): string | null {
    const normalizedValue = this.normalizeText(value);
    return normalizedValue.length > 0 ? normalizedValue : null;
  }

  private toStoreType(storeType: CampaignStoreModel['storeType']): StoreType {
    if (typeof storeType === 'number') {
      return storeType in StoreType ? storeType as StoreType : StoreType.General;
    }

    const parsedStoreType = Number(storeType);

    if (Number.isFinite(parsedStoreType)) {
      return parsedStoreType in StoreType ? parsedStoreType as StoreType : StoreType.General;
    }

    return StoreType[storeType as keyof typeof StoreType] ?? StoreType.General;
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
