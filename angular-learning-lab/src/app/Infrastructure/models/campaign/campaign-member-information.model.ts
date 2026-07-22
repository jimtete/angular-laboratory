export enum Skill {
  Acrobatics = 1,
  AnimalHandling = 2,
  Arcana = 3,
  Athletics = 4,
  Deception = 5,
  History = 6,
  Insight = 7,
  Intimidation = 8,
  Investigation = 9,
  Medicine = 10,
  Nature = 11,
  Perception = 12,
  Performance = 13,
  Persuasion = 14,
  Religion = 15,
  SleightOfHand = 16,
  Stealth = 17,
  Survival = 18,
}

export type SkillValue = Skill | keyof typeof Skill | string | number;

export type CampaignMemberAbilityValue = string | number;

export interface CampaignMemberAbilityValueModel {
  ability: CampaignMemberAbilityValue;
  value: number;
}

export interface CampaignMemberSkillValueModel {
  skill: SkillValue;
  value: number;
}

export interface CampaignMemberInformationModel {
  userId: string;
  username: string;
  firstName: string;
  lastName: string;
  nickname: string | null;
  halfProficientSkills: SkillValue[];
  proficientSkills: SkillValue[];
  expertiseSkills: SkillValue[];
  abilityValues: CampaignMemberAbilityValueModel[];
  skillValues: CampaignMemberSkillValueModel[];
}
