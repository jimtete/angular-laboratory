import { Skill, SkillValue } from './campaign-member-information.model';
import type { MapPinModel } from './campaign-map.model';
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

export enum StoryBeatIndexPathRuleRelationType {
  And = 0,
  Or = 1,
  ExactlyOne = 2,
}

export type StoryBeatIndexPathRuleRelationTypeValue =
  StoryBeatIndexPathRuleRelationType | keyof typeof StoryBeatIndexPathRuleRelationType | string | number;

export interface StoryBlockModel {
  storyBlockId: string;
  campaignId: string;
  title: string;
  orderIndex: number;
  mapPins?: MapPinModel[];
  musicFiles?: StoryBlockMusicFileModel[];
}

export interface CreateStoryBlockRequest {
  title: string | null;
}

export interface UpdateStoryBlockTitleRequest {
  title: string | null;
}

export interface ReorderStoryBlocksRequest {
  storyBlockIds: string[];
}

export interface StoryBeatIndexPathRuleModel {
  id: string;
  campaignId: string;
  storyBlockId: string;
  orderIndex: number;
  relationType: StoryBeatIndexPathRuleRelationTypeValue;
  isRequired: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface UpsertStoryBeatIndexPathRuleRequest {
  relationType: StoryBeatIndexPathRuleRelationType;
  isRequired: boolean;
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
  id?: string;
  tag: string;
  name: string;
  description: string;
}

export interface StoryBeatRoleplayingNpcReferenceModel {
  id: string;
  npcTag: string;
  tag?: string;
}

export interface StoryBeatRoleplayingInformationModel {
  id?: string;
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
  npcReferences?: StoryBeatRoleplayingNpcReferenceModel[];
  npcs?: StoryBeatRoleplayingNpcModel[];
  discoverableInformation: StoryBeatRoleplayingInformationModel[];
}

export interface StoryBeatDecisionOptionModel {
  id: string;
  orderIndex: number;
  title: string;
  description: string;
  isSelected: boolean;
}

export interface StoryBeatDecisionModel {
  description: string;
  decisions: StoryBeatDecisionOptionModel[];
}

export interface StoryBeatCombatEnemyNpcModel {
  monsterId: number;
  amount: number;
}

export interface StoryBeatCombatModel {
  description: string;
  rewards: string[] | null;
  enemyNpcs: StoryBeatCombatEnemyNpcModel[];
}

export interface StoryBeatTransitionConclusionModel {
  sourceStoryBeatId: string;
  sourceTitle: string;
  sourceStoryBeatType: StoryBeatType | keyof typeof StoryBeatType | string | number;
  category: string;
  text: string;
}

export interface StoryBeatTransitionModel {
  description: string;
  conclusions: StoryBeatTransitionConclusionModel[];
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
  orderIndex: number;
  secondaryOrderIndex: number;
  title: string;
  storyBeatType: StoryBeatType | keyof typeof StoryBeatType | string | number;
  information: StoryBeatInformationModel | null;
  narrative: StoryBeatNarrativeModel | null;
  roleplaying: StoryBeatRoleplayingModel | null;
  decision: StoryBeatDecisionModel | null;
  combat: StoryBeatCombatModel | null;
  transition: StoryBeatTransitionModel | null;
  milestone: CampaignMilestoneModel | null;
  indexPathRule: StoryBeatIndexPathRuleModel | null;
  musicFiles?: StoryBlockMusicFileModel[];
}

export interface StoryBlockMusicFileModel {
  id: string;
  storyBlockId: string;
  storyBeatId: string | null;
  musicFileId: number;
  orderIndex: number;
  loop: boolean;
  continueAcrossStoryBlocks: boolean;
  uploadedByUserId: string;
  parentFolderId: number | null;
  displayName: string;
  originalFileName: string;
  storedFileName: string;
  storagePath: string;
  contentType: string;
  fileSizeBytes: number;
  durationMilliseconds: number | null;
  createdAt: string;
  updatedAt: string;
}

export interface StoryBlockMusicFileRequest {
  musicFileId: number;
  storyBeatId: string | null;
  orderIndex: number | null;
  loop: boolean | null;
  continueAcrossStoryBlocks: boolean | null;
}

export interface UpdateStoryBlockMusicFilesRequest {
  musicFiles: StoryBlockMusicFileRequest[];
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
  id?: string | null;
  title: string | null;
  description: string | null;
  isSelected: boolean;
}

export interface StoryBeatDecisionRequest {
  description: string | null;
  decisions: StoryBeatDecisionOptionRequest[];
}

export interface StoryBeatCombatEnemyNpcRequest {
  monsterId: number;
  amount: number;
}

export interface StoryBeatCombatRequest {
  description: string | null;
  rewards: (string | null)[] | null;
  enemyNpcs: StoryBeatCombatEnemyNpcRequest[];
}

export interface StoryBeatTransitionRequest {
  description: string | null;
}

export interface CreateInformationStoryBeatRequest {
  title: string | null;
  orderIndex?: number | null;
  secondaryOrderIndex?: number | null;
  information: StoryBeatInformationRequest | null;
}

export interface UpdateInformationStoryBeatRequest {
  title: string | null;
  information: StoryBeatInformationRequest | null;
}

export interface CreateNarrativeStoryBeatRequest {
  title: string | null;
  orderIndex?: number | null;
  secondaryOrderIndex?: number | null;
  narrative: StoryBeatNarrativeRequest | null;
}

export interface UpdateNarrativeStoryBeatRequest {
  title: string | null;
  narrative: StoryBeatNarrativeRequest | null;
}

export interface CreateRoleplayingStoryBeatRequest {
  title: string | null;
  orderIndex?: number | null;
  secondaryOrderIndex?: number | null;
  roleplaying: StoryBeatRoleplayingRequest | null;
}

export interface UpdateRoleplayingStoryBeatRequest {
  title: string | null;
  roleplaying: StoryBeatRoleplayingRequest | null;
}

export interface CreateDecisionStoryBeatRequest {
  title: string | null;
  orderIndex?: number | null;
  secondaryOrderIndex?: number | null;
  decision: StoryBeatDecisionRequest | null;
}

export interface UpdateDecisionStoryBeatRequest {
  title: string | null;
  decision: StoryBeatDecisionRequest | null;
}

export interface CreateCombatStoryBeatRequest {
  title: string | null;
  orderIndex?: number | null;
  secondaryOrderIndex?: number | null;
  combat: StoryBeatCombatRequest | null;
}

export interface UpdateCombatStoryBeatRequest {
  title: string | null;
  combat: StoryBeatCombatRequest | null;
}

export interface CreateTransitionStoryBeatRequest {
  title: string | null;
  orderIndex?: number | null;
  secondaryOrderIndex?: number | null;
  transition: StoryBeatTransitionRequest | null;
}

export interface UpdateTransitionStoryBeatRequest {
  title: string | null;
  transition: StoryBeatTransitionRequest | null;
}

export interface CreateMilestoneStoryBeatRequest {
  title: string | null;
  orderIndex?: number | null;
  secondaryOrderIndex?: number | null;
  milestoneId: number;
}

export interface UpdateMilestoneStoryBeatRequest {
  title: string | null;
  milestoneId: number;
}

export interface ReorderStoryBeatRequest {
  storyBeatId: string;
  orderIndex: number;
  secondaryOrderIndex: number;
}

export interface ReorderStoryBeatsRequest {
  storyBeats: ReorderStoryBeatRequest[];
}
