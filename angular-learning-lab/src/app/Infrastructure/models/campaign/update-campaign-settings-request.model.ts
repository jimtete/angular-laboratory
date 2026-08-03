import { PassiveSkillsCheck, StoreMechanics } from './campaign-settings.model';

export interface UpdateCampaignSettingsRequest {
  maxNumberOfPlayers: number;
  passiveSkillsCheck: PassiveSkillsCheck;
  storeMechanics: StoreMechanics;
  campaignDescription: string;
}
