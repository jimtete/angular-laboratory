import { PassiveSkillsCheck } from './campaign-settings.model';

export interface UpdateCampaignSettingsRequest {
  maxNumberOfPlayers: number;
  passiveSkillsCheck: PassiveSkillsCheck;
  campaignDescription: string;
}
