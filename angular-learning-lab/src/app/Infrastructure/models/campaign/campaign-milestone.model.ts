export enum CampaignMilestoneImportance {
  Low = 1,
  High = 2,
  Optional = 3,
}

export interface CampaignMilestoneModel {
  id: number;
  campaignId: string;
  title: string;
  description: string | null;
  achievedAt: string | null;
  importance: CampaignMilestoneImportance | keyof typeof CampaignMilestoneImportance | string | number;
  createdAt: string;
  updatedAt: string;
}

export interface CampaignMilestoneRequest {
  title: string;
  description: string | null;
  achievedAt: string | null;
  importance: CampaignMilestoneImportance;
}
