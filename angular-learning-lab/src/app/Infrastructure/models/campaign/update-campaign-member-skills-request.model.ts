import { Skill } from './campaign-member-information.model';
import { Ability } from './campaign-story.model';

export interface CampaignMemberAbilityValueRequest {
  ability: Ability;
  value: number;
}

export interface CampaignMemberSkillValueRequest {
  skill: Skill;
  value: number;
}

export interface UpdateCampaignMemberSkillsRequest {
  halfProficientSkills: Skill[];
  proficientSkills: Skill[];
  expertiseSkills: Skill[];
  abilityValues: CampaignMemberAbilityValueRequest[];
  skillValues: CampaignMemberSkillValueRequest[];
}
