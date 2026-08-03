export enum PassiveSkillsCheck {
  ProfficiencyBased = 1,
  StatsBased = 2,
  Manual = 3,
}

export enum StoreMechanics {
  UnlockingStores = 1,
  SeparateStores = 2,
  GlobalStores = 3,
}

export interface CampaignSettingsModel {
  campaignId: string;
  maxNumberOfPlayers: number;
  passiveSkillsCheck: PassiveSkillsCheck | keyof typeof PassiveSkillsCheck | string | number;
  storeMechanics: StoreMechanics | keyof typeof StoreMechanics | string | number;
  campaignDescription?: string;
}
