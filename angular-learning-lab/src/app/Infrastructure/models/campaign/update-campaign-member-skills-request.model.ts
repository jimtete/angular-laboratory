import { Skill } from './campaign-member-information.model';

export interface UpdateCampaignMemberSkillsRequest {
  halfProficientSkills: Skill[];
  proficientSkills: Skill[];
  expertiseSkills: Skill[];
}
