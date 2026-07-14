export interface CampaignModel {
  campaignId: string;
  gameMasterId: string;
  gameMasterUsername: string;
  campaignName: string;
  version: string;
  campaignPictureUrl: string | null;
  campaignPictureBase64?: string | null;
  campaignPictureContentType?: string | null;
}
