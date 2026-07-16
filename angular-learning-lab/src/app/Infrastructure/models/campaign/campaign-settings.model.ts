export enum PassiveSkillsCheck {
  ProfficiencyBased = 1,
  StatsBased = 2,
  Manual = 3,
}

export interface CampaignSettingsModel {
  campaignId: string;
  maxNumberOfPlayers: number;
  passiveSkillsCheck: PassiveSkillsCheck | keyof typeof PassiveSkillsCheck | string | number;
  campaignDescription?: string;
}
