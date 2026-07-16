import { SessionNoteModel } from './session-note.model';

export interface CampaignSessionModel {
  id: number;
  campaignId: string;
  sessionNumber: number;
  description: string | null;
  sessionDate: string | null;
  createdAt: string;
  updatedAt: string;
  notes: SessionNoteModel[];
}
