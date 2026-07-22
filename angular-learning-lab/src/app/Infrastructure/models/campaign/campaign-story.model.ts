import { Skill, SkillValue } from './campaign-member-information.model';
import { CampaignMilestoneModel } from './campaign-milestone.model';

export enum Ability {
  STRENGTH = 1,
  DEXTERITY = 2,
  CONSTITUTION = 3,
  INTELLIGENCE = 4,
  CHARISMA = 5,
  WISDOM = 6,
}

export type AbilityValue = Ability | keyof typeof Ability | string | number;

export enum StoryBeatType {
  Information = 0,
  Narrative = 1,
  Roleplaying = 2,
  Decision = 3,
  Combat = 4,
  Transition = 5,
  Milestone = 6,
}

export enum StoryBeatOptionalInformationPlacement {
  Appended = 0,
  Inline = 1,
}

export interface StoryBlockModel {
  storyBlockId: string;
  campaignId: string;
  title: string;
}

export interface CreateStoryBlockRequest {
  title: string | null;
}

export interface UpdateStoryBlockTitleRequest {
  title: string | null;
}

export interface StoryBeatOptionalInformationModel {
  skill: SkillValue;
  difficultyClass: number;
  information: string;
  placement: StoryBeatOptionalInformationPlacement | keyof typeof StoryBeatOptionalInformationPlacement | string | number;
  narrativeOffset: number | null;
}

export interface StoryBeatInformationModel {
  narrative: string;
  optionalInformation: StoryBeatOptionalInformationModel[];
}

export interface StoryBeatNarrativeModel {
  paragraphs: string[];
}

export enum StoryBeatRoleplayingCheckType {
  None = 0,
  Skill = 1,
  Ability = 2,
}

export interface StoryBeatRoleplayingNpcModel {
  tag: string;
  name: string;
  description: string;
}

export interface StoryBeatRoleplayingInformationModel {
  npcTag: string;
  npcName?: string;
  checkType: StoryBeatRoleplayingCheckType | keyof typeof StoryBeatRoleplayingCheckType | string | number;
  skill: SkillValue | null;
  ability: AbilityValue | null;
  difficultyClass: number | null;
  information: string;
}

export interface StoryBeatRoleplayingModel {
  mainDescription: string;
  npcTags: string[];
  npcs?: StoryBeatRoleplayingNpcModel[];
  discoverableInformation: StoryBeatRoleplayingInformationModel[];
}

export interface StoryBeatDecisionOptionModel {
  orderIndex: number;
  title: string;
  description: string;
  isSelected: boolean;
}

export interface StoryBeatDecisionModel {
  description: string;
  decisions: StoryBeatDecisionOptionModel[];
}

export interface CampaignNpcModel {
  campaignNpcId: string;
  campaignId: string;
  tag: string;
  name: string;
  displayName: string;
  display_name?: string | null;
  description: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCampaignNpcRequest {
  tag: string | null;
  name: string | null;
  displayName: string | null;
  description: string | null;
}

export interface UpdateCampaignNpcRequest {
  name: string | null;
  displayName: string | null;
  description: string | null;
}

export interface StoryBeatModel {
  storyBeatId: string;
  storyBlockId: string;
  title: string;
  storyBeatType: StoryBeatType | keyof typeof StoryBeatType | string | number;
  information: StoryBeatInformationModel | null;
  narrative: StoryBeatNarrativeModel | null;
  roleplaying: StoryBeatRoleplayingModel | null;
  decision: StoryBeatDecisionModel | null;
  milestone: CampaignMilestoneModel | null;
}

export interface StoryBeatOptionalInformationRequest {
  skill: Skill;
  difficultyClass: number;
  information: string | null;
  placement: StoryBeatOptionalInformationPlacement;
  narrativeOffset: number | null;
}

export interface StoryBeatInformationRequest {
  narrative: string | null;
  optionalInformation: StoryBeatOptionalInformationRequest[];
}

export interface StoryBeatNarrativeRequest {
  paragraphs: (string | null)[];
}

export interface StoryBeatRoleplayingNpcRequest {
  tag: string | null;
  name: string | null;
  description: string | null;
}

export interface StoryBeatRoleplayingInformationRequest {
  npcTag: string | null;
  checkType: StoryBeatRoleplayingCheckType;
  skill: Skill | null;
  ability: Ability | null;
  difficultyClass: number | null;
  information: string | null;
}

export interface StoryBeatRoleplayingRequest {
  mainDescription: string | null;
  npcTags: string[];
  discoverableInformation: StoryBeatRoleplayingInformationRequest[];
}

export interface StoryBeatDecisionOptionRequest {
  title: string | null;
  description: string | null;
  isSelected: boolean;
}

export interface StoryBeatDecisionRequest {
  description: string | null;
  decisions: StoryBeatDecisionOptionRequest[];
}

export interface CreateInformationStoryBeatRequest {
  title: string | null;
  information: StoryBeatInformationRequest | null;
}

export interface UpdateInformationStoryBeatRequest {
  title: string | null;
  information: StoryBeatInformationRequest | null;
}

export interface CreateNarrativeStoryBeatRequest {
  title: string | null;
  narrative: StoryBeatNarrativeRequest | null;
}

export interface UpdateNarrativeStoryBeatRequest {
  title: string | null;
  narrative: StoryBeatNarrativeRequest | null;
}

export interface CreateRoleplayingStoryBeatRequest {
  title: string | null;
  roleplaying: StoryBeatRoleplayingRequest | null;
}

export interface UpdateRoleplayingStoryBeatRequest {
  title: string | null;
  roleplaying: StoryBeatRoleplayingRequest | null;
}

export interface CreateDecisionStoryBeatRequest {
  title: string | null;
  decision: StoryBeatDecisionRequest | null;
}

export interface UpdateDecisionStoryBeatRequest {
  title: string | null;
  decision: StoryBeatDecisionRequest | null;
}

export interface CreateMilestoneStoryBeatRequest {
  title: string | null;
  milestoneId: number;
}

export interface UpdateMilestoneStoryBeatRequest {
  title: string | null;
  milestoneId: number;
}
