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
  createdAt: string;
  updatedAt: string;
}
