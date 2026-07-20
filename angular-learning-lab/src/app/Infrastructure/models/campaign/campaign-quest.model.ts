export enum CampaignQuestType {
  MainQuest = 1,
  SideQuest = 2,
  PersonalQuest = 3,
  CollectibleHunt = 4,
}

export interface CampaignQuestTaskModel {
  questTaskId: string;
  questId: string;
  title: string;
  description: string;
  dateCompleted: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CampaignQuestModel {
  questId: string;
  campaignId: string;
  type: CampaignQuestType | keyof typeof CampaignQuestType | string | number;
  title: string;
  description: string;
  givenBy: string;
  reward: string;
  completedAt: string | null;
  tasks: CampaignQuestTaskModel[];
  createdAt: string;
  updatedAt: string;
}

export interface CampaignQuestTaskRequest {
  title: string;
  description: string;
  dateCompleted: string | null;
}

export interface CreateCampaignQuestRequest {
  type: CampaignQuestType;
  title: string;
  description: string;
  givenBy: string;
  reward: string;
  completedAt: string | null;
  tasks: CampaignQuestTaskRequest[];
}
