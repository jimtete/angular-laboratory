import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  LucideArrowLeft,
  LucideBadgeDollarSign,
  LucideBed,
  LucideBeer,
  LucideBookOpen,
  LucideBriefcase,
  LucideCheck,
  LucideChevronDown,
  LucideChevronUp,
  LucideCoffee,
  LucideDrama,
  LucideFence,
  LucideFish,
  LucideFlaskConical,
  LucideHammer,
  LucideHandCoins,
  LucideHeartHandshake,
  LucideHeartPulse,
  LucideLandmark,
  LucideLeaf,
  LucideMail,
  LucideMapPinned,
  LucidePackage,
  LucidePackageSearch,
  LucidePencil,
  LucidePlus,
  LucideSave,
  LucideShield,
  LucideShirt,
  LucideSparkles,
  LucideWandSparkles,
  LucideX,
} from '@lucide/angular';
import { forkJoin, finalize } from 'rxjs';

import {
  ApiError,
  AssetModel,
  CampaignApiService,
  CampaignStoreModel,
  StoreLockState,
  StoreItemModel,
  StoreType,
  UpdateStoreItemRequest,
} from '../../../Infrastructure';
import { ModalHelper } from '../../../shared/helpers/modal.helper';

interface StoreEditItemDraft {
  draftId: number;
  storeItemId: number | null;
  assetId: number | null;
  itemName: string;
  itemDescription: string;
  itemPrice: string;
  itemPriceDiscount: string;
  itemPricePercentageDiscount: string;
  quantity: string;
  isRemoved: boolean;
}

@Component({
  selector: 'app-campaign-store-page',
  imports: [
    LucideArrowLeft,
    LucideBadgeDollarSign,
    LucideBed,
    LucideBeer,
    LucideBookOpen,
    LucideBriefcase,
    LucideCheck,
    LucideChevronDown,
    LucideChevronUp,
    LucideCoffee,
    LucideDrama,
    LucideFence,
    LucideFish,
    LucideFlaskConical,
    LucideHammer,
    LucideHandCoins,
    LucideHeartHandshake,
    LucideHeartPulse,
    LucideLandmark,
    LucideLeaf,
    LucideMail,
    LucideMapPinned,
    LucidePackage,
    LucidePackageSearch,
    LucidePencil,
    LucidePlus,
    LucideSave,
    LucideShield,
    LucideShirt,
    LucideSparkles,
    LucideWandSparkles,
    LucideX,
  ],
  templateUrl: './campaign-store-page.html',
  styleUrl: './campaign-store-page.css',
})
export class CampaignStorePage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly modalHelper = inject(ModalHelper);

  protected readonly StoreType = StoreType;
  protected readonly store = signal<CampaignStoreModel | null>(null);
  protected readonly campaignStores = signal<CampaignStoreModel[]>([]);
  protected readonly isLoadingStore = signal(false);
  protected readonly isLoadingCampaignStores = signal(false);
  protected readonly isSavingPurchases = signal(false);
  protected readonly isEditingStore = signal(false);
  protected readonly isSavingStore = signal(false);
  protected readonly isSavingLockState = signal(false);
  protected readonly isAssetBrowserOpen = signal(false);
  protected readonly isLoadingAvailableItems = signal(false);
  protected readonly purchaseDrafts = signal<Record<number, number>>({});
  protected readonly storeNameDraft = signal('');
  protected readonly storeDescriptionDraft = signal('');
  protected readonly storeDiscountPercentageDraft = signal('0');
  protected readonly storeItemDrafts = signal<StoreEditItemDraft[]>([]);
  protected readonly availableItemAssets = signal<AssetModel[]>([]);
  private nextStoreItemDraftId = 1;

  ngOnInit(): void {
    this.loadStore();
    this.loadCampaignStores();
  }

  protected goBack(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId) {
      return;
    }

    const originMapId = this.getOriginMapId();

    if (originMapId !== null) {
      void this.router.navigate(['/campaigns', campaignId, 'maps', originMapId]);
      return;
    }

    void this.router.navigate(['/campaigns', campaignId, 'campaign-content', 'campaign-stores']);
  }

  protected storeDisplayName(store: CampaignStoreModel): string {
    return this.normalizeText(store.storeName) || store.storeLocation || 'Unnamed Store';
  }

  protected storeTypeFor(store: CampaignStoreModel): StoreType {
    return this.toStoreType(store.storeType);
  }

  protected storeTypeLabelFor(store: CampaignStoreModel): string {
    switch (this.storeTypeFor(store)) {
      case StoreType.Blacksmith:
        return 'Blacksmith';
      case StoreType.Medic:
        return 'Medic';
      case StoreType.Bookstore:
        return 'Bookstore';
      case StoreType.Alchemic:
        return 'Alchemic';
      case StoreType.BlackMarket:
        return 'Black Market';
      case StoreType.FishMarket:
        return 'Fish Market';
      case StoreType.Armorsmith:
        return 'Armorsmith';
      case StoreType.ClothesStore:
        return 'Clothes Store';
      case StoreType.MagicStore:
        return 'Magic Store';
      case StoreType.PawnShop:
        return 'Pawn Shop';
      case StoreType.Enchanter:
        return 'Enchanter';
      case StoreType.Herbalist:
        return 'Herbalist';
      case StoreType.Cartographer:
        return 'Cartographer';
      case StoreType.Stable:
        return 'Stable';
      case StoreType.Smuggler:
        return 'Smuggler';
      case StoreType.Tavern:
        return 'Tavern';
      case StoreType.Inn:
        return 'Inn';
      case StoreType.Brothel:
        return 'Brothel';
      case StoreType.Theatre:
        return 'Theatre';
      case StoreType.Cafe:
        return 'Cafe';
      case StoreType.Bank:
        return 'Bank';
      case StoreType.Office:
        return 'Office';
      case StoreType.PostOffice:
        return 'Post Office';
      case StoreType.General:
      default:
        return 'General';
    }
  }

  protected storeTypeClassNameFor(store: CampaignStoreModel): string {
    switch (this.storeTypeFor(store)) {
      case StoreType.Blacksmith:
        return 'blacksmith';
      case StoreType.Medic:
        return 'medic';
      case StoreType.Bookstore:
        return 'bookstore';
      case StoreType.Alchemic:
        return 'alchemic';
      case StoreType.BlackMarket:
        return 'black-market';
      case StoreType.FishMarket:
        return 'fish-market';
      case StoreType.Armorsmith:
        return 'armorsmith';
      case StoreType.ClothesStore:
        return 'clothes-store';
      case StoreType.MagicStore:
        return 'magic-store';
      case StoreType.PawnShop:
        return 'pawn-shop';
      case StoreType.Enchanter:
        return 'enchanter';
      case StoreType.Herbalist:
        return 'herbalist';
      case StoreType.Cartographer:
        return 'cartographer';
      case StoreType.Stable:
        return 'stable';
      case StoreType.Smuggler:
        return 'smuggler';
      case StoreType.Tavern:
        return 'tavern';
      case StoreType.Inn:
        return 'inn';
      case StoreType.Brothel:
        return 'brothel';
      case StoreType.Theatre:
        return 'theatre';
      case StoreType.Cafe:
        return 'cafe';
      case StoreType.Bank:
        return 'bank';
      case StoreType.Office:
        return 'office';
      case StoreType.PostOffice:
        return 'post-office';
      case StoreType.General:
      default:
        return 'general';
    }
  }

  protected storeInventoryResponses(store: CampaignStoreModel): CampaignStoreModel[] {
    const matchingStores = new Map<number, CampaignStoreModel>();

    matchingStores.set(store.storeId, store);

    for (const unlockedStore of store.unlockedStores ?? []) {
      matchingStores.set(unlockedStore.storeId, this.normalizeStoreResponse(unlockedStore));
    }

    for (const campaignStore of this.campaignStores()) {
      const normalizedStore = this.normalizeStoreResponse(campaignStore);

      if (
        normalizedStore.storeId !== store.storeId &&
        this.storeTypeFor(normalizedStore) === this.storeTypeFor(store) &&
        this.isStoreUnlocked(normalizedStore)
      ) {
        matchingStores.set(normalizedStore.storeId, normalizedStore);
      }
    }

    return Array.from(matchingStores.values());
  }

  protected isSelectedStoreResponse(
    selectedStore: CampaignStoreModel,
    inventoryStore: CampaignStoreModel,
  ): boolean {
    return selectedStore.storeId === inventoryStore.storeId;
  }

  protected itemQuantityLabel(item: StoreItemModel): string {
    return item.quantity === null ? 'Unlimited' : `Qty ${item.quantity}`;
  }

  protected isStoreUnlocked(store: CampaignStoreModel): boolean {
    return this.storeLockStateFor(store) === StoreLockState.Unlocked;
  }

  protected getStoreLockStateLabel(store: CampaignStoreModel): string {
    return this.isStoreUnlocked(store) ? 'Unlocked' : 'Locked';
  }

  protected toggleStoreLockState(store: CampaignStoreModel): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isSavingLockState()) {
      return;
    }

    const nextLockState = this.isStoreUnlocked(store)
      ? StoreLockState.Locked
      : StoreLockState.Unlocked;

    this.isSavingLockState.set(true);
    this.campaignApiService
      .updateCampaignStoreLockState(campaignId, store.storeId, {
        lockState: nextLockState,
      })
      .pipe(finalize(() => this.isSavingLockState.set(false)))
      .subscribe({
        next: (response) => {
          if (response.data) {
            this.applyStore(response.data);
            this.loadCampaignStores();
          }
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign store lock state could not be updated.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected finalItemPrice(store: CampaignStoreModel, item: StoreItemModel): number {
    const afterFlatDiscount = Math.max(0, item.itemPrice - item.itemPriceDiscount);
    const afterItemPercentageDiscount = this.applyPercentageDiscount(
      afterFlatDiscount,
      item.itemPricePercentageDiscount,
    );

    return this.applyPercentageDiscount(afterItemPercentageDiscount, store.storeDiscountPercentage);
  }

  protected hasItemDiscount(store: CampaignStoreModel, item: StoreItemModel): boolean {
    return item.itemPriceDiscount > 0 ||
      item.itemPricePercentageDiscount > 0 ||
      store.storeDiscountPercentage > 0;
  }

  protected itemPriceBreakdown(store: CampaignStoreModel, item: StoreItemModel): string {
    const parts = [`Base ${item.itemPrice} gold`];

    if (item.itemPriceDiscount > 0) {
      parts.push(`-${item.itemPriceDiscount} gold`);
    }

    if (item.itemPricePercentageDiscount > 0) {
      parts.push(`-${item.itemPricePercentageDiscount}%`);
    }

    if (store.storeDiscountPercentage > 0) {
      parts.push(`Store -${store.storeDiscountPercentage}%`);
    }

    return parts.join(' / ');
  }

  protected purchasedCount(item: StoreItemModel): number {
    return this.purchaseDrafts()[item.storeItemId] ?? item.timesSold;
  }

  protected increasePurchased(item: StoreItemModel): void {
    const currentCount = this.purchasedCount(item);
    const nextCount = item.quantity === null ? currentCount + 1 : Math.min(item.quantity, currentCount + 1);

    this.setPurchasedCount(item.storeItemId, nextCount);
  }

  protected decreasePurchased(item: StoreItemModel): void {
    this.setPurchasedCount(item.storeItemId, Math.max(0, this.purchasedCount(item) - 1));
  }

  protected savePurchasedItems(): void {
    const campaignId = this.getCampaignId();
    const store = this.store();

    if (!campaignId || !store || this.isSavingPurchases()) {
      return;
    }

    const stores = this.storeInventoryResponses(store);

    if (stores.length === 0) {
      return;
    }

    this.isSavingPurchases.set(true);

    forkJoin(stores.map((inventoryStore) => this.campaignApiService
      .updateCampaignStoreItemPurchases(campaignId, inventoryStore.storeId, {
        items: inventoryStore.items.map((item) => ({
          storeItemId: item.storeItemId,
          timesSold: this.purchasedCount(item),
        })),
      })))
      .pipe(finalize(() => this.isSavingPurchases.set(false)))
      .subscribe({
        next: (responses) => {
          for (const response of responses) {
            if (response.data) {
              this.applySavedStoreResponse(response.data);
            }
          }
          this.modalHelper.showSuccess('Campaign store purchases saved successfully.');
          this.loadCampaignStores();
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Store purchase state could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected beginStoreEdit(): void {
    const store = this.store();

    if (!store) {
      return;
    }

    this.storeNameDraft.set(store.storeName ?? '');
    this.storeDescriptionDraft.set(store.storeDescription ?? '');
    this.storeDiscountPercentageDraft.set(String(store.storeDiscountPercentage ?? 0));
    this.storeItemDrafts.set(store.items.map((item) => ({
      draftId: this.nextStoreItemDraftId++,
      storeItemId: item.storeItemId,
      assetId: null,
      itemName: item.itemName,
      itemDescription: item.itemDescription ?? '',
      itemPrice: String(item.itemPrice),
      itemPriceDiscount: String(item.itemPriceDiscount),
      itemPricePercentageDiscount: String(item.itemPricePercentageDiscount),
      quantity: item.quantity === null ? '' : String(item.quantity),
      isRemoved: false,
    })));
    this.isEditingStore.set(true);
  }

  protected cancelStoreEdit(): void {
    if (this.isSavingStore()) {
      return;
    }

    this.isEditingStore.set(false);
    this.isAssetBrowserOpen.set(false);
    this.storeItemDrafts.set([]);
  }

  protected setStoreNameDraft(event: Event): void {
    this.storeNameDraft.set((event.target as HTMLInputElement).value);
  }

  protected setStoreDescriptionDraft(event: Event): void {
    this.storeDescriptionDraft.set((event.target as HTMLTextAreaElement).value);
  }

  protected setStoreDiscountPercentageDraft(event: Event): void {
    this.storeDiscountPercentageDraft.set((event.target as HTMLInputElement).value);
  }

  protected setStoreItemQuantityDraft(draftId: number, event: Event): void {
    this.updateStoreItemDraft(draftId, { quantity: (event.target as HTMLInputElement).value });
  }

  protected setStoreItemPriceDraft(draftId: number, event: Event): void {
    this.updateStoreItemDraft(draftId, { itemPrice: (event.target as HTMLInputElement).value });
  }

  protected setStoreItemDiscountDraft(draftId: number, event: Event): void {
    this.updateStoreItemDraft(draftId, { itemPriceDiscount: (event.target as HTMLInputElement).value });
  }

  protected setStoreItemPercentageDiscountDraft(draftId: number, event: Event): void {
    this.updateStoreItemDraft(draftId, { itemPricePercentageDiscount: (event.target as HTMLInputElement).value });
  }

  protected toggleStoreItemRemoved(draftId: number): void {
    this.storeItemDrafts.update((items) => items.map((item) => (
      item.draftId === draftId ? { ...item, isRemoved: !item.isRemoved } : item
    )));
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
        storeItemId: null,
        assetId: asset.id,
        itemName: asset.name,
        itemDescription: asset.description ?? '',
        itemPrice: '',
        itemPriceDiscount: '0',
        itemPricePercentageDiscount: '0',
        quantity: '',
        isRemoved: false,
      },
    ]);
    this.isAssetBrowserOpen.set(false);
  }

  protected saveStoreEdit(): void {
    const campaignId = this.getCampaignId();
    const storeId = this.getStoreId();
    const store = this.store();

    if (!campaignId || storeId === null || !store || !this.canSaveStoreEdit() || this.isSavingStore()) {
      return;
    }

    this.isSavingStore.set(true);

    this.campaignApiService
      .updateCampaignStore(campaignId, storeId, {
        storeType: this.storeTypeFor(store),
        storeLocation: store.storeLocation,
        storeName: this.toNullableText(this.storeNameDraft()),
        storeDescription: this.toNullableText(this.storeDescriptionDraft()),
        storeDiscountPercentage: this.toRequiredNumber(this.storeDiscountPercentageDraft()),
        items: this.storeItemDrafts()
          .filter((item) => !item.isRemoved)
          .map((item) => this.toStoreItemUpdateRequest(item)),
      })
      .pipe(finalize(() => this.isSavingStore.set(false)))
      .subscribe({
        next: (response) => {
          this.modalHelper.showSuccess(response.message, { statusCode: response.statusCode });
          if (response.data) {
            this.applyStore(response.data);
            this.loadCampaignStores();
          }
          this.isEditingStore.set(false);
          this.isAssetBrowserOpen.set(false);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Store could not be updated.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected canSaveStoreEdit(): boolean {
    const activeItems = this.storeItemDrafts().filter((item) => !item.isRemoved);

    return this.isValidPercentage(this.storeDiscountPercentageDraft()) &&
      activeItems.length > 0 &&
      activeItems.every((item) => this.isValidStoreItemDraft(item));
  }

  private loadStore(): void {
    const campaignId = this.getCampaignId();
    const storeId = this.getStoreId();

    if (!campaignId || storeId === null || this.isLoadingStore()) {
      return;
    }

    this.isLoadingStore.set(true);

    this.campaignApiService
      .fetchCampaignStore(campaignId, storeId)
      .pipe(finalize(() => this.isLoadingStore.set(false)))
      .subscribe({
        next: (response) => {
          if (response.data) {
            this.applyStore(response.data);
          }
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign store could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadCampaignStores(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingCampaignStores()) {
      return;
    }

    this.isLoadingCampaignStores.set(true);

    this.campaignApiService
      .fetchCampaignStores(campaignId)
      .pipe(finalize(() => this.isLoadingCampaignStores.set(false)))
      .subscribe({
        next: (response) => {
          this.campaignStores.set((response.data ?? []).map((store) => this.normalizeStoreResponse(store)));
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign stores could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
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

  private applyStore(store: CampaignStoreModel): void {
    const normalizedStore = this.normalizeStoreResponse(store);

    this.store.set(normalizedStore);
    this.syncPurchaseDraftsForStores([normalizedStore, ...normalizedStore.unlockedStores]);
  }

  private applySavedStoreResponse(store: CampaignStoreModel): void {
    const normalizedStore = this.normalizeStoreResponse(store);
    const selectedStore = this.store();

    if (selectedStore?.storeId === normalizedStore.storeId) {
      this.store.set({
        ...normalizedStore,
        unlockedStores: selectedStore.unlockedStores,
      });
      this.syncPurchaseDraftsForStores([normalizedStore]);
      return;
    }

    this.campaignStores.update((stores) => {
      const existingStoreIndex = stores.findIndex((candidate) => candidate.storeId === normalizedStore.storeId);

      if (existingStoreIndex === -1) {
        return [...stores, normalizedStore];
      }

      return stores.map((candidate, index) => (
        index === existingStoreIndex ? normalizedStore : candidate
      ));
    });
    this.syncPurchaseDraftsForStores([normalizedStore]);
  }

  private syncPurchaseDraftsForStores(stores: CampaignStoreModel[]): void {
    this.purchaseDrafts.update((drafts) => {
      const nextDrafts = { ...drafts };

      for (const store of stores) {
        for (const item of store.items) {
          nextDrafts[item.storeItemId] = item.timesSold;
        }
      }

      return nextDrafts;
    });
  }

  private normalizeStoreResponse(store: CampaignStoreModel): CampaignStoreModel {
    return {
      ...store,
      items: store.items ?? [],
      unlockedStores: (store.unlockedStores ?? []).map((unlockedStore) => ({
        ...unlockedStore,
        items: unlockedStore.items ?? [],
        unlockedStores: unlockedStore.unlockedStores ?? [],
      })),
    };
  }

  private setPurchasedCount(storeItemId: number, timesSold: number): void {
    this.purchaseDrafts.update((drafts) => ({
      ...drafts,
      [storeItemId]: timesSold,
    }));
  }

  private updateStoreItemDraft(draftId: number, changes: Partial<StoreEditItemDraft>): void {
    this.storeItemDrafts.update((items) => items.map((item) => (
      item.draftId === draftId ? { ...item, ...changes } : item
    )));
  }

  private toStoreItemUpdateRequest(item: StoreEditItemDraft): UpdateStoreItemRequest {
    return {
      storeItemId: item.storeItemId,
      quantity: this.toNullableNumber(item.quantity),
      itemName: this.normalizeText(item.itemName),
      itemDescription: this.toNullableText(item.itemDescription),
      itemPrice: this.toRequiredNumber(item.itemPrice),
      itemPriceDiscount: this.toRequiredNumber(item.itemPriceDiscount),
      itemPricePercentageDiscount: this.toRequiredNumber(item.itemPricePercentageDiscount),
    };
  }

  private isValidStoreItemDraft(item: StoreEditItemDraft): boolean {
    const itemPrice = this.toRequiredNumber(item.itemPrice);
    const itemDiscount = this.toRequiredNumber(item.itemPriceDiscount);

    return this.normalizeText(item.itemName).length > 0 &&
      this.isValidOptionalNumber(item.quantity) &&
      this.isValidRequiredNumber(item.itemPrice) &&
      this.isValidRequiredNumber(item.itemPriceDiscount) &&
      itemDiscount <= itemPrice &&
      this.isValidPercentage(item.itemPricePercentageDiscount);
  }

  private isValidPercentage(value: string): boolean {
    const parsedValue = this.toRequiredNumber(value);
    return this.isValidRequiredNumber(value) && parsedValue >= 0 && parsedValue <= 100;
  }

  private toRequiredNumber(value: string): number {
    return Number(this.normalizeText(value));
  }

  private applyPercentageDiscount(price: number, percentage: number): number {
    const normalizedPercentage = this.clampNumber(percentage, 0, 100);
    return Math.max(0, Math.ceil(price * (100 - normalizedPercentage) / 100));
  }

  private clampNumber(value: number, min: number, max: number): number {
    if (!Number.isFinite(value)) {
      return min;
    }

    return Math.min(max, Math.max(min, value));
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

  private storeLockStateFor(store: CampaignStoreModel): StoreLockState {
    if (typeof store.lockState === 'number') {
      return store.lockState in StoreLockState
        ? store.lockState as StoreLockState
        : StoreLockState.Locked;
    }

    const parsedLockState = Number(store.lockState);

    if (Number.isFinite(parsedLockState)) {
      return parsedLockState in StoreLockState
        ? parsedLockState as StoreLockState
        : StoreLockState.Locked;
    }

    return StoreLockState[store.lockState as keyof typeof StoreLockState] ?? StoreLockState.Locked;
  }

  private getCampaignId(): string | null {
    return this.route.pathFromRoot
      .map((candidate) => candidate.snapshot.paramMap.get('campaignId'))
      .find((campaignId): campaignId is string => !!campaignId) ?? null;
  }

  private getStoreId(): number | null {
    const storeId = Number(this.route.snapshot.paramMap.get('storeId'));
    return Number.isInteger(storeId) && storeId > 0 ? storeId : null;
  }

  private getOriginMapId(): number | null {
    const originMapId = Number(this.route.snapshot.queryParamMap.get('fromMapId'));

    return Number.isInteger(originMapId) && originMapId > 0
      ? originMapId
      : null;
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
