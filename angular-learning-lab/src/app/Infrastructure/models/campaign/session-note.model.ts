export enum SessionNoteType {
  GeneralNotes = 1,
  ImportantChoice = 2,
  CampaignMilestone = 3,
  ItemFound = 4,
  LevelUpOrMechanicsChange = 5,
}

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

export interface SessionNoteModel {
  id: number;
  sessionId: number;
  order: number;
  type: SessionNoteType | keyof typeof SessionNoteType | string | number;
  content: string;
  choices: SessionNoteChoiceModel[];
  mechanicsChanges: SessionNoteMechanicsChangeModel[];
  createdAt: string;
  updatedAt: string;
}
