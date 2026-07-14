export interface CampaignPendingInviteModel {
  campaignId: string;
  campaignName: string;
  version: string;
  campaignPictureUrl: string | null;
  gameMasterId: string;
  gameMasterUsername: string;
  dateInvited: string;
}
