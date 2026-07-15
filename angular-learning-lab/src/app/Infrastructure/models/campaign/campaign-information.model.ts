import { CampaignMemberInformationModel } from './campaign-member-information.model';

export interface CampaignInformationModel {
  campaignId: string;
  joinedMembers: CampaignMemberInformationModel[];
}
