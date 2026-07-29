import { SessionNoteModel } from './session-note.model';
import { CampaignMemberInformationModel } from './campaign-member-information.model';

export interface CampaignSessionModel {
  id: number;
  campaignId: string;
  sessionNumber: number;
  description: string | null;
  sessionDate: string | null;
  createdAt: string;
  updatedAt: string;
  players: CampaignMemberInformationModel[];
  notes: SessionNoteModel[];
}
