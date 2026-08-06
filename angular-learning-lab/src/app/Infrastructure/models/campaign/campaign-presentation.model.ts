import { CampaignQuestModel, StoryBeatQuestTaskModel } from './campaign-quest.model';
import {
  CampaignEventStateModel,
  OutcomeEffectModel,
  OutcomeSourceTypeValue,
  TargetAvailabilityResult,
} from './campaign-narration.model';
import { CampaignSessionModel } from './campaign-session.model';
import { StoryBeatIndexPathRuleModel, StoryBeatModel, StoryBlockModel } from './campaign-story.model';

export interface CampaignPresentationEntryModel {
  id: number;
  campaignPresentationId: number;
  sequence: number;
  entryType: string | number;
  storyBlockId: string | null;
  storyBeatId: string | null;
  createdAt: string;
}

export interface CampaignPresentationStoryBeatSelectionModel {
  id: number;
  campaignPresentationId: number;
  storyBlockId: string;
  orderIndex: number;
  selectedSecondaryOrderIndex: number;
  selectedStoryBeatId: string;
  selectedAt: string;
}

export interface CampaignPresentationModel {
  id: number;
  campaignSessionId: number;
  status: string | number;
  activeStoryBlockId: string | null;
  currentStoryBeatId: string | null;
  startedAt: string;
  updatedAt: string;
  endedAt: string | null;
  latestEntry: CampaignPresentationEntryModel | null;
  storyBeatSelections: CampaignPresentationStoryBeatSelectionModel[];
}

export interface PresentationModeStoryBeatChoiceGroupModel {
  orderIndex: number;
  indexPathRule: StoryBeatIndexPathRuleModel | null;
  storyBeats: StoryBeatModel[];
}

export interface PresentationModeSatisfiedRuleModel {
  ruleId: string;
  explanation: string;
}

export interface PresentationModeBlockingEventModel {
  ruleId: string;
  eventDefinitionId: string;
  eventKey: string;
  clauseId: string | null;
  isMissing: boolean;
  explanation: string;
}

export interface PresentationModePendingOutcomeEffectModel {
  storyBeatId: string;
  sourceType: OutcomeSourceTypeValue;
  sourceId: string;
  effects: OutcomeEffectModel[];
}

export interface PresentationModeStoryBeatAvailabilityModel {
  storyBeatId: string;
  isAvailable: boolean;
  isAvailableByRule: boolean;
  satisfiedRules: PresentationModeSatisfiedRuleModel[];
  blockingEvents: PresentationModeBlockingEventModel[];
  pendingOutcomeEffects: PresentationModePendingOutcomeEffectModel[];
  availability: TargetAvailabilityResult | null;
}

export interface PresentationModeStoryBlockModel {
  storyBlock: StoryBlockModel;
  storyBeats: StoryBeatModel[];
  storyBeatAvailability: PresentationModeStoryBeatAvailabilityModel[];
  indexPathChoiceGroups: PresentationModeStoryBeatChoiceGroupModel[];
  quests: CampaignQuestModel[];
  storyBeatQuestTaskLinks: StoryBeatQuestTaskModel[];
}

export interface PresentationModeWorkspaceModel {
  presentation: CampaignPresentationModel;
  storyBlocks: PresentationModeStoryBlockModel[];
  quests: CampaignQuestModel[];
  storyBeatQuestTaskLinks: StoryBeatQuestTaskModel[];
}

export interface InitiatePresentationModeRequest {
  storyBlockId: string | null;
}

export interface PresentStoryBeatRequest {
  storyBeatId: string;
}

export interface FinishPresentationStoryBeatRequest {
  storyBeatId: string;
  content: string | null;
}

export interface PresentationModeStoryBeatPlayedModel {
  workspace: PresentationModeWorkspaceModel;
  session: CampaignSessionModel;
  changedEventStates: CampaignEventStateModel[];
}

export interface MarkPresentationRoleplayingInformationRequest {
  storyBeatId: string;
  informationId: string;
  content: string | null;
}

export interface PresentationModeStoryBeatReferenceMarkedModel {
  workspace: PresentationModeWorkspaceModel;
  session: CampaignSessionModel;
}

export interface TakePresentationDecisionOptionRequest {
  storyBeatId: string;
  decisionOptionId: string;
  content: string | null;
}

export interface PresentationModeSocketErrorModel {
  operation: string;
  errorCode: string;
  message: string;
  campaignId: string | null;
  sessionId: number | null;
}
