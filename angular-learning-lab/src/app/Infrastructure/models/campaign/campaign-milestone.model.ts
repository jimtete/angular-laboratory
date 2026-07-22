export enum CampaignMilestoneImportance {
  Low = 1,
  High = 2,
  Optional = 3,
}

export type CampaignMilestoneImportanceValue =
  CampaignMilestoneImportance |
  keyof typeof CampaignMilestoneImportance |
  string |
  number;

export interface CampaignMilestoneModel {
  id: number;
  campaignId: string;
  title: string;
  description: string | null;
  achievedAt: string | null;
  importance: CampaignMilestoneImportanceValue;
  createdAt: string;
  updatedAt: string;
}

export interface CampaignMilestoneRequest {
  title: string;
  description: string | null;
  achievedAt: string | null;
  importance: CampaignMilestoneImportance;
}

export function toCampaignMilestoneImportance(
  importance: CampaignMilestoneImportanceValue,
): CampaignMilestoneImportance {
  if (typeof importance === 'number') {
    return importance as CampaignMilestoneImportance;
  }

  const parsedImportance = Number(importance);

  if (Number.isFinite(parsedImportance)) {
    return parsedImportance as CampaignMilestoneImportance;
  }

  return CampaignMilestoneImportance[importance as keyof typeof CampaignMilestoneImportance] ??
    CampaignMilestoneImportance.Low;
}

export function getCampaignMilestoneImportanceLabel(
  importance: CampaignMilestoneImportanceValue,
): string {
  switch (toCampaignMilestoneImportance(importance)) {
    case CampaignMilestoneImportance.High:
      return 'High';
    case CampaignMilestoneImportance.Optional:
      return 'Optional';
    case CampaignMilestoneImportance.Low:
    default:
      return 'Low';
  }
}

export function getCampaignMilestoneImportanceSlug(
  importance: CampaignMilestoneImportanceValue,
): string {
  switch (toCampaignMilestoneImportance(importance)) {
    case CampaignMilestoneImportance.High:
      return 'high';
    case CampaignMilestoneImportance.Optional:
      return 'optional';
    case CampaignMilestoneImportance.Low:
    default:
      return 'low';
  }
}
