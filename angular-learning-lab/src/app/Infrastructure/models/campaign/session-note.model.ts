export enum SessionNoteType {
  GeneralNotes = 1,
  ImportantChoice = 2,
  CampaignMilestone = 3,
  ItemFound = 4,
  LevelUpOrMechanicsChange = 5,
  StoryBeatPlayed = 6,
}

export enum SessionNoteStoryBeatReferenceType {
  StoryBeat = 1,
  RoleplayingNpcInteraction = 2,
  RoleplayingInformation = 3,
  DecisionOption = 4,
}

export enum SessionNoteStoryBeatReferenceOutcome {
  Presented = 0,
  Taken = 1,
}

export type SessionNoteStoryBeatReferenceTypeValue =
  SessionNoteStoryBeatReferenceType | keyof typeof SessionNoteStoryBeatReferenceType | string | number;

export type SessionNoteStoryBeatReferenceOutcomeValue =
  SessionNoteStoryBeatReferenceOutcome | keyof typeof SessionNoteStoryBeatReferenceOutcome | string | number;

export interface SessionNoteChoiceModel {
  id: number;
  sessionNoteId: number;
  order: number;
  choiceText: string;
  isChosen: boolean;
}

export interface SessionNoteChoiceRequest {
  choiceText: string;
  isChosen: boolean;
}

export interface ImportantChoiceSessionNoteRequest {
  content: string;
  choices: SessionNoteChoiceRequest[];
}

export interface SessionNoteMechanicsChangeModel {
  id: number;
  sessionNoteId: number;
  order: number;
  playerId: string;
  changeText: string | null;
}

export interface SessionNoteMechanicsChangeRequest {
  playerId: string;
  changeText: string | null;
}

export interface LevelUpOrMechanicsChangeSessionNoteRequest {
  content: string;
  mechanicsChanges: SessionNoteMechanicsChangeRequest[];
}

export interface AchieveCampaignMilestoneRequest {
  milestoneId: number;
  content?: string | null;
}

export interface CreateStoryBeatPlayedSessionNoteRequest {
  storyBeatId: string;
  content?: string | null;
}

export interface CreateStoryBeatReferenceSessionNoteRequest {
  storyBeatId: string;
  referenceType: SessionNoteStoryBeatReferenceTypeValue;
  referenceId: string | null;
  content?: string | null;
}

export interface UpdateStoryBeatReferenceSessionNoteRequest {
  storyBeatId: string;
  referenceType: SessionNoteStoryBeatReferenceTypeValue;
  referenceId?: string | null;
  isPlayed: boolean;
  content?: string | null;
}

export interface SessionNoteStoryBeatModel {
  storyBeatId: string;
  storyBlockId: string;
  orderIndex: number;
  secondaryOrderIndex: number;
  title: string;
  storyBeatType: string | number;
}

export interface SessionNoteStoryBeatReferenceModel {
  id: number;
  sessionNoteId: number;
  storyBeatId: string;
  referenceType: SessionNoteStoryBeatReferenceTypeValue;
  referenceId: string | null;
  referenceOutcome: SessionNoteStoryBeatReferenceOutcomeValue;
  npcTag: string | null;
  contentSnapshot: string;
  createdAt: string;
}

export interface SessionNoteModel {
  id: number;
  sessionId: number;
  order: number;
  type: SessionNoteType | keyof typeof SessionNoteType | string | number;
  content: string;
  storyBeatId: string | null;
  storyBeat: SessionNoteStoryBeatModel | null;
  storyBeatReferences: SessionNoteStoryBeatReferenceModel[];
  choices: SessionNoteChoiceModel[];
  mechanicsChanges: SessionNoteMechanicsChangeModel[];
  createdAt: string;
  updatedAt: string;
}
