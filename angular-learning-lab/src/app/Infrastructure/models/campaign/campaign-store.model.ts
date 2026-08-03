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
}

export type StoreTypeValue = StoreType | keyof typeof StoreType | string | number;

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
  storeLocation: string;
  storeName: string | null;
  storeDescription: string | null;
  items: StoreItemModel[];
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
  items: CreateStoreItemRequest[];
}

export interface UpdateStoreItemPurchaseRequest {
  storeItemId: number;
  timesSold: number;
}

export interface UpdateStoreItemPurchaseStateRequest {
  items: UpdateStoreItemPurchaseRequest[];
}
