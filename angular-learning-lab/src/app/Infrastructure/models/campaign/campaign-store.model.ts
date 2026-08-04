export enum StoreType {
  General = 0,
  Blacksmith = 1,
  Medic = 2,
  Bookstore = 3,
  Alchemic = 4,
  BlackMarket = 5,
  FishMarket = 6,
  Armorsmith = 7,
  ClothesStore = 8,
  MagicStore = 9,
  PawnShop = 10,
  Enchanter = 11,
  Herbalist = 12,
  Cartographer = 13,
  Stable = 14,
  Smuggler = 15,
  Tavern = 16,
  Inn = 17,
  Brothel = 18,
  Theatre = 19,
  Cafe = 20,
  Bank = 21,
  Office = 22,
  PostOffice = 23,
}

export type StoreTypeValue = StoreType | keyof typeof StoreType | string | number;

export enum StoreLockState {
  Locked = 0,
  Unlocked = 1,
}

export type StoreLockStateValue = StoreLockState | keyof typeof StoreLockState | string | number;

export interface StoreItemModel {
  storeItemId: number;
  storeId: number;
  quantity: number | null;
  timesSold: number;
  itemName: string;
  itemDescription: string | null;
  itemPrice: number;
  itemPriceDiscount: number;
  itemPricePercentageDiscount: number;
}

export interface CampaignStoreModel {
  storeId: number;
  campaignId: string;
  storeType: StoreTypeValue;
  lockState: StoreLockStateValue;
  storeLocation: string;
  storeName: string | null;
  storeDescription: string | null;
  storeDiscountPercentage: number;
  items: StoreItemModel[];
  unlockedStores: CampaignStoreModel[];
}

export interface CreateStoreItemRequest {
  quantity: number | null;
  itemName: string;
  itemDescription: string | null;
  itemPrice: number;
  itemPriceDiscount: number;
  itemPricePercentageDiscount: number;
}

export interface CreateStoreRequest {
  storeType: StoreType;
  storeLocation: string;
  storeName: string | null;
  storeDescription: string | null;
  storeDiscountPercentage: number;
  items: CreateStoreItemRequest[];
}

export interface UpdateStoreItemRequest {
  storeItemId: number | null;
  quantity: number | null;
  itemName: string;
  itemDescription: string | null;
  itemPrice: number;
  itemPriceDiscount: number;
  itemPricePercentageDiscount: number;
}

export interface UpdateStoreRequest {
  storeType: StoreType;
  storeLocation: string;
  storeName: string | null;
  storeDescription: string | null;
  storeDiscountPercentage: number;
  items: UpdateStoreItemRequest[];
}

export interface UpdateStoreItemPurchaseRequest {
  storeItemId: number;
  timesSold: number;
}

export interface UpdateStoreItemPurchaseStateRequest {
  items: UpdateStoreItemPurchaseRequest[];
}

export interface UpdateStoreLockStateRequest {
  lockState: StoreLockState;
}
