import { CampaignMemberInformationModel } from './campaign-member-information.model';

export interface CampaignUsernamesModel {
  campaignId: string;
  joinedMembers: CampaignMemberInformationModel[];
  joinedUsernames: string[];
  invitedUsernames: string[];
}
