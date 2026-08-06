export enum CampaignMapCategory {
  World = 1,
  Regional = 2,
  City = 3,
  District = 4,
}

export interface CampaignMapModel {
  id: number;
  parentMapId: number | null;
  assetId: number;
  assetUrl: string | null;
  contentType: string | null;
  fileSizeBytes: number | null;
  category: CampaignMapCategory | keyof typeof CampaignMapCategory | string | number;
  imageWidthPixels: number;
  imageHeightPixels: number;
  name: string;
  description: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCampaignMapRequest {
  parentMapId: number | null;
  category: CampaignMapCategory;
  imageWidthPixels: number;
  imageHeightPixels: number;
  name: string;
  description: string;
}

export enum MapPinTargetType {
  Placeholder = 0,
  StoryBlock = 1,
  Map = 2,
  Store = 3,
  PlayersPosition = 4,
  PlayerPosition = PlayersPosition,
}

export interface MapPinModel {
  id: number;
  mapId: number;
  xCoordinate: number;
  yCoordinate: number;
  label: string;
  description: string;
  targetType: MapPinTargetType | keyof typeof MapPinTargetType | string | number;
  targetId: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface MapPinDetailsModel extends MapPinModel {
  targetData: unknown | null;
}

export enum MapPinConnectionDistanceUnit {
  Minutes = 1,
  Hours = 2,
  Days = 3,
  Weeks = 4,
}

export interface MapPinConnectionModel {
  id: number;
  mapId: number;
  mapPinAId: number;
  mapPinBId: number;
  distanceValue: number | null;
  distanceUnit: MapPinConnectionDistanceUnit | keyof typeof MapPinConnectionDistanceUnit | string | number | null;
  createdAt: string;
  updatedAt: string;
}

export interface MapPinsByMapModel {
  mapId: number;
  pinTypes: unknown[];
  pins: MapPinDetailsModel[];
  connections: MapPinConnectionModel[];
}

export interface CreateMapPinRequest {
  xCoordinate: number;
  yCoordinate: number;
  label: string;
  description: string;
  targetType: MapPinTargetType;
  targetId: string | null;
}

export type UpdateMapPinRequest = CreateMapPinRequest;

export interface CreateMapPinConnectionRequest {
  mapPinAId: number;
  mapPinBId: number;
  distanceValue: number | null;
  distanceUnit: MapPinConnectionDistanceUnit | null;
}

export type UpdateMapPinConnectionRequest = CreateMapPinConnectionRequest;
