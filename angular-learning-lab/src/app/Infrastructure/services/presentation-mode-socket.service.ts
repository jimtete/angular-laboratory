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
  FinishPresentationStoryBeatRequest,
  InitiatePresentationModeRequest,
  MarkPresentationRoleplayingInformationRequest,
  PresentStoryBeatRequest,
  PresentationModeSocketErrorModel,
  PresentationModeStoryBlockModel,
  PresentationModeStoryBeatPlayedModel,
  PresentationModeStoryBeatReferenceMarkedModel,
  PresentationModeWorkspaceModel,
  TakePresentationDecisionOptionRequest,
} from '../models';
import { TokenStorageService } from './token-storage.service';

const presentationModeLoadedEvent = 'presentationModeLoaded';
const presentationModeStoryBlockLoadedEvent = 'presentationModeStoryBlockLoaded';
const presentationModeEnabledEvent = 'presentationModeEnabled';
const presentationModeDisabledEvent = 'presentationModeDisabled';
const presentationModeUpdatedEvent = 'presentationModeUpdated';
const presentationModeStoryBeatPlayedEvent = 'presentationModeStoryBeatPlayed';
const presentationModeStoryBeatReferenceMarkedEvent = 'presentationModeStoryBeatReferenceMarked';
const presentationModeDecisionTakenEvent = 'presentationModeDecisionTaken';
const presentationModeErrorEvent = 'presentationModeError';
const subscribeMethod = 'SubscribeToPresentationMode';
const unsubscribeMethod = 'UnsubscribeFromPresentationMode';
const getPresentationModeMethod = 'GetPresentationMode';
const getPresentationModeStoryBlockMethod = 'GetPresentationModeStoryBlock';
const enablePresentationModeMethod = 'EnablePresentationMode';
const disablePresentationModeMethod = 'DisablePresentationMode';
const presentStoryBeatMethod = 'PresentStoryBeat';
const finishStoryBeatMethod = 'FinishStoryBeat';
const markRoleplayingInformationGivenMethod = 'MarkRoleplayingInformationGiven';
const takeDecisionOptionMethod = 'TakeDecisionOption';

@Injectable({
  providedIn: 'root',
})
export class PresentationModeSocketService {
  private readonly apiBaseUrl = inject(API_BASE_URL).replace(/\/$/, '');
  private readonly tokenStorage = inject(TokenStorageService);
  private connection?: HubConnection;
  private startPromise?: Promise<void>;
  private subscribedCampaignId: string | null = null;
  private subscribedSessionId: number | null = null;

  readonly workspace = signal<PresentationModeWorkspaceModel | null>(null);
  readonly storyBeatPlayed = signal<PresentationModeStoryBeatPlayedModel | null>(null);
  readonly storyBeatReferenceMarked = signal<PresentationModeStoryBeatReferenceMarkedModel | null>(null);
  readonly decisionTaken = signal<PresentationModeStoryBeatReferenceMarkedModel | null>(null);
  readonly lastError = signal<PresentationModeSocketErrorModel | null>(null);

  async connect(campaignId: string, sessionId: number): Promise<void> {
    if (!this.canConnect()) {
      return;
    }

    await this.ensureConnectionStarted();

    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      throw new Error('Presentation mode socket is not connected.');
    }

    if (this.subscribedCampaignId === campaignId && this.subscribedSessionId === sessionId) {
      return;
    }

    if (this.subscribedCampaignId && this.subscribedSessionId !== null) {
      await this.tryUnsubscribe(this.subscribedCampaignId, this.subscribedSessionId);
    }

    try {
      await this.connection.invoke(subscribeMethod, campaignId, sessionId);
    } catch (error: unknown) {
      if (!this.isPresentationModeNotInitiatedError(error)) {
        throw error;
      }
    }

    this.subscribedCampaignId = campaignId;
    this.subscribedSessionId = sessionId;
  }

  async disconnect(): Promise<void> {
    const connection = this.connection;
    const campaignId = this.subscribedCampaignId;
    const sessionId = this.subscribedSessionId;

    this.connection = undefined;
    this.startPromise = undefined;
    this.subscribedCampaignId = null;
    this.subscribedSessionId = null;
    this.workspace.set(null);
    this.storyBeatPlayed.set(null);
    this.storyBeatReferenceMarked.set(null);
    this.decisionTaken.set(null);
    this.lastError.set(null);

    if (!connection || connection.state === HubConnectionState.Disconnected) {
      return;
    }

    if (campaignId && sessionId !== null && connection.state === HubConnectionState.Connected) {
      try {
        await connection.invoke(unsubscribeMethod, campaignId, sessionId);
      } catch {
        // Stopping the SignalR connection removes group membership server-side.
      }
    }

    await connection.stop();
  }

  async getPresentationMode(campaignId: string, sessionId: number): Promise<PresentationModeWorkspaceModel | null> {
    const connection = await this.getReadyConnection(campaignId, sessionId);
    const workspace = await connection.invoke<PresentationModeWorkspaceModel | null>(
      getPresentationModeMethod,
      campaignId,
      sessionId,
    );

    this.workspace.set(workspace ?? null);

    return workspace ?? null;
  }

  async enablePresentationMode(
    campaignId: string,
    sessionId: number,
    request: InitiatePresentationModeRequest = { storyBlockId: null },
  ): Promise<PresentationModeWorkspaceModel | null> {
    const connection = await this.getReadyConnection(campaignId, sessionId);
    const workspace = await connection.invoke<PresentationModeWorkspaceModel | null>(
      enablePresentationModeMethod,
      campaignId,
      sessionId,
      request,
    );

    this.workspace.set(workspace ?? null);

    return workspace ?? null;
  }

  async getPresentationModeStoryBlock(
    campaignId: string,
    sessionId: number,
    storyBlockId: string,
  ): Promise<PresentationModeStoryBlockModel | null> {
    const connection = await this.getReadyConnection(campaignId, sessionId);
    const storyBlock = await connection.invoke<PresentationModeStoryBlockModel | null>(
      getPresentationModeStoryBlockMethod,
      campaignId,
      sessionId,
      storyBlockId,
    );

    if (storyBlock) {
      this.upsertStoryBlock(storyBlock);
    }

    return storyBlock ?? null;
  }

  async disablePresentationMode(campaignId: string, sessionId: number): Promise<PresentationModeWorkspaceModel | null> {
    const connection = await this.getReadyConnection(campaignId, sessionId);
    const workspace = await connection.invoke<PresentationModeWorkspaceModel | null>(
      disablePresentationModeMethod,
      campaignId,
      sessionId,
    );

    this.workspace.set(null);

    return workspace ?? null;
  }

  async presentStoryBeat(
    campaignId: string,
    sessionId: number,
    request: PresentStoryBeatRequest,
  ): Promise<PresentationModeWorkspaceModel | null> {
    const connection = await this.getReadyConnection(campaignId, sessionId);
    const workspace = await connection.invoke<PresentationModeWorkspaceModel | null>(
      presentStoryBeatMethod,
      campaignId,
      sessionId,
      request,
    );

    this.workspace.set(workspace ?? null);

    return workspace ?? null;
  }

  async finishStoryBeat(
    campaignId: string,
    sessionId: number,
    request: FinishPresentationStoryBeatRequest,
  ): Promise<PresentationModeStoryBeatPlayedModel | null> {
    const connection = await this.getReadyConnection(campaignId, sessionId);
    this.storyBeatPlayed.set(null);

    const result = await this.withSocketTimeout(
      connection.invoke<PresentationModeStoryBeatPlayedModel | null>(
        finishStoryBeatMethod,
        campaignId,
        sessionId,
        request,
      ),
      'Finish story beat timed out before the server returned an updated session.',
    );

    if (result) {
      this.workspace.set(result.workspace);
      this.storyBeatPlayed.set(result);
    }

    return result ?? null;
  }

  async markRoleplayingInformationGiven(
    campaignId: string,
    sessionId: number,
    request: MarkPresentationRoleplayingInformationRequest,
  ): Promise<PresentationModeStoryBeatReferenceMarkedModel | null> {
    const connection = await this.getReadyConnection(campaignId, sessionId);
    const result = await connection.invoke<PresentationModeStoryBeatReferenceMarkedModel | null>(
      markRoleplayingInformationGivenMethod,
      campaignId,
      sessionId,
      request,
    );

    if (result) {
      this.workspace.set(result.workspace);
      this.storyBeatReferenceMarked.set(result);
    }

    return result ?? null;
  }

  async takeDecisionOption(
    campaignId: string,
    sessionId: number,
    request: TakePresentationDecisionOptionRequest,
  ): Promise<PresentationModeStoryBeatReferenceMarkedModel | null> {
    const connection = await this.getReadyConnection(campaignId, sessionId);
    const result = await connection.invoke<PresentationModeStoryBeatReferenceMarkedModel | null>(
      takeDecisionOptionMethod,
      campaignId,
      sessionId,
      request,
    );

    if (result) {
      this.workspace.set(result.workspace);
      this.decisionTaken.set(result);
    }

    return result ?? null;
  }

  private async getReadyConnection(campaignId: string, sessionId: number): Promise<HubConnection> {
    await this.connect(campaignId, sessionId);

    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      throw new Error('Presentation mode socket is not connected.');
    }

    return this.connection;
  }

  private async withSocketTimeout<T>(
    promise: Promise<T>,
    message: string,
    timeoutMs = 15000,
  ): Promise<T> {
    let timeout: ReturnType<typeof setTimeout> | undefined;
    const timeoutPromise = new Promise<T>((_, reject) => {
      timeout = setTimeout(() => reject(new Error(message)), timeoutMs);
    });

    return Promise.race([promise, timeoutPromise])
      .finally(() => {
        if (timeout) {
          clearTimeout(timeout);
        }
      });
  }

  private buildConnection(): HubConnection {
    const connection = new HubConnectionBuilder()
      .withUrl(`${this.apiBaseUrl}/sockets/presentation-mode`, {
        accessTokenFactory: () => this.tokenStorage.getAccessToken() ?? '',
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on(presentationModeLoadedEvent, (workspace: PresentationModeWorkspaceModel) => {
      this.workspace.set(workspace);
      this.lastError.set(null);
    });

    connection.on(presentationModeStoryBlockLoadedEvent, (storyBlock: PresentationModeStoryBlockModel) => {
      this.upsertStoryBlock(storyBlock);
      this.lastError.set(null);
    });

    connection.on(presentationModeEnabledEvent, (workspace: PresentationModeWorkspaceModel) => {
      this.workspace.set(workspace);
      this.lastError.set(null);
    });

    connection.on(presentationModeDisabledEvent, () => {
      this.workspace.set(null);
      this.lastError.set(null);
    });

    connection.on(presentationModeUpdatedEvent, (workspace: PresentationModeWorkspaceModel) => {
      this.workspace.set(workspace);
      this.lastError.set(null);
    });

    connection.on(presentationModeStoryBeatPlayedEvent, (result: PresentationModeStoryBeatPlayedModel) => {
      this.workspace.set(result.workspace);
      this.storyBeatPlayed.set(result);
      this.lastError.set(null);
    });

    connection.on(
      presentationModeStoryBeatReferenceMarkedEvent,
      (result: PresentationModeStoryBeatReferenceMarkedModel) => {
        this.workspace.set(result.workspace);
        this.storyBeatReferenceMarked.set(result);
        this.lastError.set(null);
      },
    );

    connection.on(presentationModeDecisionTakenEvent, (result: PresentationModeStoryBeatReferenceMarkedModel) => {
      this.workspace.set(result.workspace);
      this.decisionTaken.set(result);
      this.lastError.set(null);
    });

    connection.on(presentationModeErrorEvent, (error: PresentationModeSocketErrorModel) => {
      this.lastError.set(error);
    });

    return connection;
  }

  private upsertStoryBlock(storyBlock: PresentationModeStoryBlockModel): void {
    this.workspace.update((workspace) => {
      if (!workspace) {
        return workspace;
      }

      const storyBlockId = storyBlock.storyBlock.storyBlockId;
      const storyBlockExists = workspace.storyBlocks
        .some((existingBlock) => existingBlock.storyBlock.storyBlockId === storyBlockId);
      const storyBlockQuestIds = new Set(storyBlock.quests.map((quest) => quest.questId));
      const storyBlockQuestTaskIds = new Set(storyBlock.storyBeatQuestTaskLinks.map((link) => link.questTaskId));

      return {
        ...workspace,
        storyBlocks: storyBlockExists
          ? workspace.storyBlocks.map((existingBlock) => (
            existingBlock.storyBlock.storyBlockId === storyBlockId ? storyBlock : existingBlock
          ))
          : [...workspace.storyBlocks, storyBlock]
            .sort((first, second) => first.storyBlock.orderIndex - second.storyBlock.orderIndex),
        quests: [
          ...workspace.quests.filter((quest) => !storyBlockQuestIds.has(quest.questId)),
          ...storyBlock.quests,
        ],
        storyBeatQuestTaskLinks: [
          ...workspace.storyBeatQuestTaskLinks.filter((link) => !storyBlockQuestTaskIds.has(link.questTaskId)),
          ...storyBlock.storyBeatQuestTaskLinks,
        ],
      };
    });
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

  private async tryUnsubscribe(campaignId: string, sessionId: number): Promise<void> {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      return;
    }

    try {
      await this.connection.invoke(unsubscribeMethod, campaignId, sessionId);
    } catch {
      // Stopping the SignalR connection removes group membership server-side.
    }
  }

  private isPresentationModeNotInitiatedError(error: unknown): boolean {
    const lastError = this.lastError();

    if (lastError?.errorCode === 'CampaignPresentationNotFound') {
      return true;
    }

    return error instanceof Error &&
      error.message.includes('Presentation mode has not been initiated for this session.');
  }
}
