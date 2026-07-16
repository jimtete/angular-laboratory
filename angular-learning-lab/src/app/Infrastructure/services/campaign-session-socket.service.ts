import { Injectable, inject, signal } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  HttpTransportType,
  LogLevel,
} from '@microsoft/signalr';

import { API_BASE_URL } from '../api.config';
import {
  AchieveCampaignMilestoneRequest,
  CampaignSessionModel,
  ImportantChoiceSessionNoteRequest,
  SessionNoteModel,
} from '../models';
import { TokenStorageService } from './token-storage.service';

const campaignSessionsLoadedEvent = 'campaignSessionsLoaded';
const sessionNotesLoadedEvent = 'sessionNotesLoaded';
const campaignSessionCreatedEvent = 'campaignSessionCreated';
const campaignSessionUpdatedEvent = 'campaignSessionUpdated';
const subscribeMethod = 'SubscribeToCampaignSessions';
const unsubscribeMethod = 'UnsubscribeFromCampaignSessions';
const getSessionNotesMethod = 'GetSessionNotes';
const updateDateMethod = 'UpdateCampaignSessionDate';
const updateDescriptionMethod = 'UpdateCampaignSessionDescription';
const createGenericNoteMethod = 'CreateGenericSessionNote';
const createImportantChoiceNoteMethod = 'CreateImportantChoiceSessionNote';
const achieveCampaignMilestoneMethod = 'AchieveCampaignMilestone';
const updateSessionNoteMethod = 'UpdateSessionNote';
const updateImportantChoiceNoteMethod = 'UpdateImportantChoiceSessionNote';
const deleteSessionNoteMethod = 'DeleteSessionNote';

@Injectable({
  providedIn: 'root',
})
export class CampaignSessionSocketService {
  private readonly apiBaseUrl = inject(API_BASE_URL).replace(/\/$/, '');
  private readonly tokenStorage = inject(TokenStorageService);
  private connection?: HubConnection;
  private startPromise?: Promise<void>;
  private subscribedCampaignId: string | null = null;
  private loadedNotesSessionId: number | null = null;

  readonly sessions = signal<CampaignSessionModel[]>([]);
  readonly sessionNotes = signal<SessionNoteModel[]>([]);

  async connect(campaignId: string): Promise<void> {
    if (!this.canConnect()) {
      return;
    }

    await this.ensureConnectionStarted();

    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      throw new Error('Campaign session socket is not connected.');
    }

    if (this.subscribedCampaignId === campaignId) {
      return;
    }

    if (this.subscribedCampaignId) {
      await this.tryUnsubscribe(this.subscribedCampaignId);
    }

    await this.connection.invoke(subscribeMethod, campaignId);
    this.subscribedCampaignId = campaignId;
  }

  async disconnect(): Promise<void> {
    const connection = this.connection;
    this.connection = undefined;
    this.startPromise = undefined;
    this.subscribedCampaignId = null;
    this.loadedNotesSessionId = null;
    this.sessionNotes.set([]);

    if (!connection || connection.state === HubConnectionState.Disconnected) {
      return;
    }

    await connection.stop();
  }

  async updateSessionDate(
    campaignId: string,
    sessionId: number,
    sessionDate: string | null,
  ): Promise<CampaignSessionModel | null> {
    const connection = await this.getReadyConnection(campaignId);
    const updatedSession = await connection.invoke<CampaignSessionModel | null>(
      updateDateMethod,
      campaignId,
      sessionId,
      sessionDate,
    );

    if (updatedSession) {
      this.upsertSession(updatedSession);
    }

    return updatedSession ?? null;
  }

  async updateSessionDescription(
    campaignId: string,
    sessionId: number,
    description: string | null,
  ): Promise<CampaignSessionModel | null> {
    const connection = await this.getReadyConnection(campaignId);
    const updatedSession = await connection.invoke<CampaignSessionModel | null>(
      updateDescriptionMethod,
      campaignId,
      sessionId,
      description,
    );

    if (updatedSession) {
      this.upsertSession(updatedSession);
    }

    return updatedSession ?? null;
  }

  async getSessionNotes(
    campaignId: string,
    sessionId: number,
  ): Promise<SessionNoteModel[]> {
    const connection = await this.getReadyConnection(campaignId);

    this.loadedNotesSessionId = sessionId;

    const notes = await connection.invoke<SessionNoteModel[] | null>(
      getSessionNotesMethod,
      campaignId,
      sessionId,
    );
    const orderedNotes = this.orderNotes(notes ?? []);

    this.sessionNotes.set(orderedNotes);

    return orderedNotes;
  }

  async createGenericSessionNote(
    campaignId: string,
    sessionId: number,
    content: string,
  ): Promise<CampaignSessionModel | null> {
    const connection = await this.getReadyConnection(campaignId);
    const updatedSession = await connection.invoke<CampaignSessionModel | null>(
      createGenericNoteMethod,
      campaignId,
      sessionId,
      content,
    );

    if (updatedSession) {
      this.upsertSession(updatedSession);
      this.setActiveSessionNotes(updatedSession);
    }

    return updatedSession ?? null;
  }

  async createImportantChoiceSessionNote(
    campaignId: string,
    sessionId: number,
    request: ImportantChoiceSessionNoteRequest,
  ): Promise<CampaignSessionModel | null> {
    const connection = await this.getReadyConnection(campaignId);
    const updatedSession = await connection.invoke<CampaignSessionModel | null>(
      createImportantChoiceNoteMethod,
      campaignId,
      sessionId,
      request,
    );

    if (updatedSession) {
      this.upsertSession(updatedSession);
      this.setActiveSessionNotes(updatedSession);
    }

    return updatedSession ?? null;
  }

  async achieveCampaignMilestone(
    campaignId: string,
    sessionId: number,
    request: AchieveCampaignMilestoneRequest,
  ): Promise<CampaignSessionModel | null> {
    const connection = await this.getReadyConnection(campaignId);
    const updatedSession = await connection.invoke<CampaignSessionModel | null>(
      achieveCampaignMilestoneMethod,
      campaignId,
      sessionId,
      request,
    );

    if (updatedSession) {
      this.upsertSession(updatedSession);
      this.setActiveSessionNotes(updatedSession);
    }

    return updatedSession ?? null;
  }

  async updateSessionNote(
    campaignId: string,
    sessionId: number,
    noteId: number,
    content: string,
  ): Promise<CampaignSessionModel | null> {
    const connection = await this.getReadyConnection(campaignId);
    const updatedSession = await connection.invoke<CampaignSessionModel | null>(
      updateSessionNoteMethod,
      campaignId,
      sessionId,
      noteId,
      content,
    );

    if (updatedSession) {
      this.upsertSession(updatedSession);
      this.setActiveSessionNotes(updatedSession);
    }

    return updatedSession ?? null;
  }

  async updateImportantChoiceSessionNote(
    campaignId: string,
    sessionId: number,
    noteId: number,
    request: ImportantChoiceSessionNoteRequest,
  ): Promise<CampaignSessionModel | null> {
    const connection = await this.getReadyConnection(campaignId);
    const updatedSession = await connection.invoke<CampaignSessionModel | null>(
      updateImportantChoiceNoteMethod,
      campaignId,
      sessionId,
      noteId,
      request,
    );

    if (updatedSession) {
      this.upsertSession(updatedSession);
      this.setActiveSessionNotes(updatedSession);
    }

    return updatedSession ?? null;
  }

  async deleteSessionNote(
    campaignId: string,
    sessionId: number,
    noteId: number,
  ): Promise<CampaignSessionModel | null> {
    const connection = await this.getReadyConnection(campaignId);
    const updatedSession = await connection.invoke<CampaignSessionModel | null>(
      deleteSessionNoteMethod,
      campaignId,
      sessionId,
      noteId,
    );

    if (updatedSession) {
      this.upsertSession(updatedSession);
      this.setActiveSessionNotes(updatedSession);
    }

    return updatedSession ?? null;
  }

  upsertSession(session: CampaignSessionModel): void {
    this.sessions.update((sessions) => {
      const existingIndex = sessions.findIndex((existingSession) => existingSession.id === session.id);

      if (existingIndex < 0) {
        return [...sessions, session].sort((first, second) => first.sessionNumber - second.sessionNumber);
      }

      return sessions.map((existingSession, index) => index === existingIndex ? session : existingSession);
    });
  }

  private async getReadyConnection(campaignId: string): Promise<HubConnection> {
    await this.connect(campaignId);

    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      throw new Error('Campaign session socket is not connected.');
    }

    return this.connection;
  }

  private buildConnection(): HubConnection {
    const connection = new HubConnectionBuilder()
      .withUrl(`${this.apiBaseUrl}/sockets/campaign-sessions`, {
        accessTokenFactory: () => this.tokenStorage.getAccessToken() ?? '',
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on(campaignSessionsLoadedEvent, (sessions: CampaignSessionModel[]) => {
      this.sessions.set(sessions);
    });

    connection.on(sessionNotesLoadedEvent, (notes: SessionNoteModel[]) => {
      this.sessionNotes.set(this.orderNotes(notes));
    });

    connection.on(campaignSessionCreatedEvent, (session: CampaignSessionModel) => {
      this.upsertSession(session);
    });

    connection.on(campaignSessionUpdatedEvent, (session: CampaignSessionModel) => {
      this.upsertSession(session);
      this.setActiveSessionNotes(session);
    });

    return connection;
  }

  private canConnect(): boolean {
    return this.tokenStorage.hasValidAccessToken() && this.tokenStorage.hasAnyRole('Master');
  }

  private async ensureConnectionStarted(): Promise<void> {
    if (!this.connection || this.connection.state === HubConnectionState.Disconnected) {
      this.connection = this.buildConnection();
      this.startPromise = this.connection.start()
        .finally(() => {
          this.startPromise = undefined;
        });

      await this.startPromise;
      return;
    }

    if (this.startPromise) {
      await this.startPromise;
    }
  }

  private async tryUnsubscribe(campaignId: string): Promise<void> {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      return;
    }

    try {
      await this.connection.invoke(unsubscribeMethod, campaignId);
    } catch {
      // Stopping the SignalR connection removes group membership server-side.
    }
  }

  private orderNotes(notes: SessionNoteModel[]): SessionNoteModel[] {
    return [...notes].sort((first, second) => (
      first.order - second.order ||
      first.id - second.id
    ));
  }

  private setActiveSessionNotes(session: CampaignSessionModel): void {
    if (session.id !== this.loadedNotesSessionId) {
      return;
    }

    this.sessionNotes.set(this.orderNotes(session.notes ?? []));
  }
}
