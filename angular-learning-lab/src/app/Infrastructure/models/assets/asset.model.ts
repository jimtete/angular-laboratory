export enum AssetType {
  Folder = 1,
  Items = 2,
}

export enum AssetItemType {
  Weapon = 1,
  Equipment = 2,
  Armor = 3,
  Other = 4,
}

export interface AssetModel {
  id: number;
  parentAssetId: number | null;
  assetType: AssetType | keyof typeof AssetType | string | number;
  name: string;
  description: string;
  itemType: AssetItemType | keyof typeof AssetItemType | string | number | null;
  campaignIds: string[] | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateAssetFolderRequest {
  parentAssetId: number | null;
  name: string;
  description: string;
}

export interface CreateItemAssetRequest {
  parentAssetId: number | null;
  name: string;
  description: string;
  itemType: AssetItemType;
  campaignIds: string[] | null;
}

export interface UpdateItemAssetRequest {
  parentAssetId: number | null;
  name: string;
  description: string;
  itemType: AssetItemType;
  campaignIds: string[] | null;
}
