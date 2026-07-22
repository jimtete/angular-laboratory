import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  ApiResponse,
  AssetModel,
  CampaignInformationModel,
  CampaignInviteResolutionModel,
  CampaignMemberInformationModel,
  CampaignMilestoneModel,
  CampaignMilestoneRequest,
  CampaignModel,
  CampaignPendingInviteModel,
  CampaignParticipationInviteModel,
  CampaignQuestModel,
  CampaignSessionModel,
  CampaignSettingsModel,
  CreateStoryBlockRequest,
  CreateDecisionStoryBeatRequest,
  CreateInformationStoryBeatRequest,
  CreateMilestoneStoryBeatRequest,
  CreateNarrativeStoryBeatRequest,
  CreateCampaignNpcRequest,
  CreateRoleplayingStoryBeatRequest,
  CampaignUsernamesModel,
  CreateCampaignParticipationInviteRequest,
  CreateCampaignRequest,
  CreateAssetFolderRequest,
  CreateItemAssetRequest,
  CreateCampaignQuestRequest,
  CampaignNpcModel,
  StoryBeatModel,
  StoryBlockModel,
  UpdateDecisionStoryBeatRequest,
  UpdateInformationStoryBeatRequest,
  UpdateCampaignNpcRequest,
  UpdateMilestoneStoryBeatRequest,
  UpdateNarrativeStoryBeatRequest,
  UpdateRoleplayingStoryBeatRequest,
  UpdateStoryBlockTitleRequest,
  UpdateCampaignMemberNicknameRequest,
  UpdateCampaignMemberSkillsRequest,
  UpdateCampaignSettingsRequest,
  UpdateItemAssetRequest,
} from '../models';
import { ApiClient } from './api-client.service';

@Injectable({
  providedIn: 'root',
})
export class CampaignApiService {
  private readonly apiClient = inject(ApiClient);

  fetchAvailableCampaigns(): Observable<ApiResponse<CampaignModel[]>> {
    return this.apiClient.get<ApiResponse<CampaignModel[]>>('/api/campaigns');
  }

  fetchPendingInvites(): Observable<ApiResponse<CampaignPendingInviteModel[]>> {
    return this.apiClient.get<ApiResponse<CampaignPendingInviteModel[]>>(
      '/api/campaigns/invites',
    );
  }

  fetchCampaignUsernames(
    campaignId: string,
  ): Observable<ApiResponse<CampaignUsernamesModel>> {
    return this.apiClient.get<ApiResponse<CampaignUsernamesModel>>(
      `/api/campaigns/${campaignId}/users`,
    );
  }

  fetchCampaignInformation(
    campaignId: string,
  ): Observable<ApiResponse<CampaignInformationModel>> {
    return this.apiClient.get<ApiResponse<CampaignInformationModel>>(
      `/api/campaigns/${campaignId}/settings/information`,
    );
  }

  createCampaign(
    request: CreateCampaignRequest,
    campaignPicture: Blob | null = null,
  ): Observable<ApiResponse<CampaignModel>> {
    const formData = new FormData();
    formData.append('CampaignName', request.campaignName);
    formData.append('Version', request.version);

    if (campaignPicture) {
      formData.append('campaignPicture', campaignPicture, 'campaign-picture.jpg');
    }

    return this.apiClient.post<ApiResponse<CampaignModel>, FormData>(
      '/api/campaigns',
      formData,
    );
  }

  fetchCampaignSettings(
    campaignId: string,
  ): Observable<ApiResponse<CampaignSettingsModel>> {
    return this.apiClient.get<ApiResponse<CampaignSettingsModel>>(
      `/api/campaigns/${campaignId}/settings`,
    );
  }

  updateCampaignSettings(
    campaignId: string,
    request: UpdateCampaignSettingsRequest,
  ): Observable<ApiResponse<CampaignSettingsModel>> {
    return this.apiClient.put<
      ApiResponse<CampaignSettingsModel>,
      UpdateCampaignSettingsRequest
    >(`/api/campaigns/${campaignId}/settings`, request);
  }

  fetchCampaignSessions(
    campaignId: string,
  ): Observable<ApiResponse<CampaignSessionModel[]>> {
    return this.apiClient.get<ApiResponse<CampaignSessionModel[]>>(
      `/api/campaigns/${campaignId}/sessions`,
    );
  }

  createCampaignSession(
    campaignId: string,
  ): Observable<ApiResponse<CampaignSessionModel>> {
    return this.apiClient.post<ApiResponse<CampaignSessionModel>, null>(
      `/api/campaigns/${campaignId}/sessions`,
      null,
    );
  }

  fetchCampaignMilestones(
    campaignId: string,
  ): Observable<ApiResponse<CampaignMilestoneModel[]>> {
    return this.apiClient.get<ApiResponse<CampaignMilestoneModel[]>>(
      `/api/campaigns/${campaignId}/content/milestones`,
    );
  }

  fetchUnachievedCampaignMilestones(
    campaignId: string,
  ): Observable<ApiResponse<CampaignMilestoneModel[]>> {
    return this.apiClient.get<ApiResponse<CampaignMilestoneModel[]>>(
      `/api/campaigns/${campaignId}/content/milestones/unachieved`,
    );
  }

  createCampaignMilestone(
    campaignId: string,
    request: CampaignMilestoneRequest,
  ): Observable<ApiResponse<CampaignMilestoneModel>> {
    return this.apiClient.post<
      ApiResponse<CampaignMilestoneModel>,
      CampaignMilestoneRequest
    >(`/api/campaigns/${campaignId}/content/milestones`, request);
  }

  updateCampaignMilestone(
    campaignId: string,
    milestoneId: number,
    request: CampaignMilestoneRequest,
  ): Observable<ApiResponse<CampaignMilestoneModel>> {
    return this.apiClient.put<
      ApiResponse<CampaignMilestoneModel>,
      CampaignMilestoneRequest
    >(`/api/campaigns/${campaignId}/content/milestones/${milestoneId}`, request);
  }

  deleteCampaignMilestone(
    campaignId: string,
    milestoneId: number,
  ): Observable<ApiResponse<object>> {
    return this.apiClient.delete<ApiResponse<object>>(
      `/api/campaigns/${campaignId}/content/milestones/${milestoneId}`,
    );
  }

  fetchCampaignQuests(
    campaignId: string,
  ): Observable<ApiResponse<CampaignQuestModel[]>> {
    return this.apiClient.get<ApiResponse<CampaignQuestModel[]>>(
      `/api/campaigns/${campaignId}/content/quests`,
    );
  }

  createCampaignQuest(
    campaignId: string,
    request: CreateCampaignQuestRequest,
  ): Observable<ApiResponse<CampaignQuestModel>> {
    return this.apiClient.post<
      ApiResponse<CampaignQuestModel>,
      CreateCampaignQuestRequest
    >(`/api/campaigns/${campaignId}/content/quests`, request);
  }

  fetchStoryBlocks(
    campaignId: string,
  ): Observable<ApiResponse<StoryBlockModel[]>> {
    return this.apiClient.get<ApiResponse<StoryBlockModel[]>>(
      `/api/campaigns/${campaignId}/content/story-blocks`,
    );
  }

  fetchRoleplayingStoryBeatNpcs(
    campaignId: string,
  ): Observable<ApiResponse<CampaignNpcModel[]>> {
    return this.apiClient.get<ApiResponse<CampaignNpcModel[]>>(
      `/api/campaigns/${campaignId}/content/story-blocks/roleplaying-npcs`,
    );
  }

  createRoleplayingNpc(
    campaignId: string,
    request: CreateCampaignNpcRequest,
  ): Observable<ApiResponse<CampaignNpcModel>> {
    return this.apiClient.post<ApiResponse<CampaignNpcModel>, CreateCampaignNpcRequest>(
      `/api/campaigns/${campaignId}/content/story-blocks/roleplaying-npcs`,
      request,
    );
  }

  updateRoleplayingNpc(
    campaignId: string,
    npcTag: string,
    request: UpdateCampaignNpcRequest,
  ): Observable<ApiResponse<CampaignNpcModel>> {
    return this.apiClient.put<ApiResponse<CampaignNpcModel>, UpdateCampaignNpcRequest>(
      `/api/campaigns/${campaignId}/content/story-blocks/roleplaying-npcs/${encodeURIComponent(npcTag)}`,
      request,
    );
  }

  createStoryBlock(
    campaignId: string,
    request: CreateStoryBlockRequest,
  ): Observable<ApiResponse<StoryBlockModel>> {
    return this.apiClient.post<ApiResponse<StoryBlockModel>, CreateStoryBlockRequest>(
      `/api/campaigns/${campaignId}/content/story-blocks`,
      request,
    );
  }

  updateStoryBlockTitle(
    campaignId: string,
    storyBlockId: string,
    request: UpdateStoryBlockTitleRequest,
  ): Observable<ApiResponse<StoryBlockModel>> {
    return this.apiClient.patch<ApiResponse<StoryBlockModel>, UpdateStoryBlockTitleRequest>(
      `/api/campaigns/${campaignId}/content/story-blocks/${storyBlockId}/title`,
      request,
    );
  }

  deleteStoryBlock(
    campaignId: string,
    storyBlockId: string,
  ): Observable<ApiResponse<object>> {
    return this.apiClient.delete<ApiResponse<object>>(
      `/api/campaigns/${campaignId}/content/story-blocks/${storyBlockId}`,
    );
  }

  fetchStoryBeats(
    campaignId: string,
    storyBlockId: string,
  ): Observable<ApiResponse<StoryBeatModel[]>> {
    return this.apiClient.get<ApiResponse<StoryBeatModel[]>>(
      `/api/campaigns/${campaignId}/content/story-blocks/${storyBlockId}/beats`,
    );
  }

  createInformationStoryBeat(
    campaignId: string,
    storyBlockId: string,
    request: CreateInformationStoryBeatRequest,
  ): Observable<ApiResponse<StoryBeatModel>> {
    return this.apiClient.post<
      ApiResponse<StoryBeatModel>,
      CreateInformationStoryBeatRequest
    >(
      `/api/campaigns/${campaignId}/content/story-blocks/${storyBlockId}/beats/information`,
      request,
    );
  }

  createNarrativeStoryBeat(
    campaignId: string,
    storyBlockId: string,
    request: CreateNarrativeStoryBeatRequest,
  ): Observable<ApiResponse<StoryBeatModel>> {
    return this.apiClient.post<
      ApiResponse<StoryBeatModel>,
      CreateNarrativeStoryBeatRequest
    >(
      `/api/campaigns/${campaignId}/content/story-blocks/${storyBlockId}/beats/narrative`,
      request,
    );
  }

  createRoleplayingStoryBeat(
    campaignId: string,
    storyBlockId: string,
    request: CreateRoleplayingStoryBeatRequest,
  ): Observable<ApiResponse<StoryBeatModel>> {
    return this.apiClient.post<
      ApiResponse<StoryBeatModel>,
      CreateRoleplayingStoryBeatRequest
    >(
      `/api/campaigns/${campaignId}/content/story-blocks/${storyBlockId}/beats/roleplaying`,
      request,
    );
  }

  createDecisionStoryBeat(
    campaignId: string,
    storyBlockId: string,
    request: CreateDecisionStoryBeatRequest,
  ): Observable<ApiResponse<StoryBeatModel>> {
    return this.apiClient.post<
      ApiResponse<StoryBeatModel>,
      CreateDecisionStoryBeatRequest
    >(
      `/api/campaigns/${campaignId}/content/story-blocks/${storyBlockId}/beats/decision`,
      request,
    );
  }

  createMilestoneStoryBeat(
    campaignId: string,
    storyBlockId: string,
    request: CreateMilestoneStoryBeatRequest,
  ): Observable<ApiResponse<StoryBeatModel>> {
    return this.apiClient.post<
      ApiResponse<StoryBeatModel>,
      CreateMilestoneStoryBeatRequest
    >(
      `/api/campaigns/${campaignId}/content/story-blocks/${storyBlockId}/beats/milestone`,
      request,
    );
  }

  updateInformationStoryBeat(
    campaignId: string,
    storyBlockId: string,
    storyBeatId: string,
    request: UpdateInformationStoryBeatRequest,
  ): Observable<ApiResponse<StoryBeatModel>> {
    return this.apiClient.put<
      ApiResponse<StoryBeatModel>,
      UpdateInformationStoryBeatRequest
    >(
      `/api/campaigns/${campaignId}/content/story-blocks/${storyBlockId}/beats/${storyBeatId}/information`,
      request,
    );
  }

  updateNarrativeStoryBeat(
    campaignId: string,
    storyBlockId: string,
    storyBeatId: string,
    request: UpdateNarrativeStoryBeatRequest,
  ): Observable<ApiResponse<StoryBeatModel>> {
    return this.apiClient.put<
      ApiResponse<StoryBeatModel>,
      UpdateNarrativeStoryBeatRequest
    >(
      `/api/campaigns/${campaignId}/content/story-blocks/${storyBlockId}/beats/${storyBeatId}/narrative`,
      request,
    );
  }

  updateRoleplayingStoryBeat(
    campaignId: string,
    storyBlockId: string,
    storyBeatId: string,
    request: UpdateRoleplayingStoryBeatRequest,
  ): Observable<ApiResponse<StoryBeatModel>> {
    return this.apiClient.put<
      ApiResponse<StoryBeatModel>,
      UpdateRoleplayingStoryBeatRequest
    >(
      `/api/campaigns/${campaignId}/content/story-blocks/${storyBlockId}/beats/${storyBeatId}/roleplaying`,
      request,
    );
  }

  updateDecisionStoryBeat(
    campaignId: string,
    storyBlockId: string,
    storyBeatId: string,
    request: UpdateDecisionStoryBeatRequest,
  ): Observable<ApiResponse<StoryBeatModel>> {
    return this.apiClient.put<
      ApiResponse<StoryBeatModel>,
      UpdateDecisionStoryBeatRequest
    >(
      `/api/campaigns/${campaignId}/content/story-blocks/${storyBlockId}/beats/${storyBeatId}/decision`,
      request,
    );
  }

  updateMilestoneStoryBeat(
    campaignId: string,
    storyBlockId: string,
    storyBeatId: string,
    request: UpdateMilestoneStoryBeatRequest,
  ): Observable<ApiResponse<StoryBeatModel>> {
    return this.apiClient.put<
      ApiResponse<StoryBeatModel>,
      UpdateMilestoneStoryBeatRequest
    >(
      `/api/campaigns/${campaignId}/content/story-blocks/${storyBlockId}/beats/${storyBeatId}/milestone`,
      request,
    );
  }

  deleteStoryBeat(
    campaignId: string,
    storyBlockId: string,
    storyBeatId: string,
  ): Observable<ApiResponse<object>> {
    return this.apiClient.delete<ApiResponse<object>>(
      `/api/campaigns/${campaignId}/content/story-blocks/${storyBlockId}/beats/${storyBeatId}`,
    );
  }

  fetchAssets(parentAssetId: number | null = null): Observable<ApiResponse<AssetModel[]>> {
    return this.apiClient.get<ApiResponse<AssetModel[]>>(
      '/api/assets',
      parentAssetId === null ? undefined : { params: { parentAssetId } },
    );
  }

  fetchAvailableCampaignItems(
    campaignId: string,
  ): Observable<ApiResponse<AssetModel[]>> {
    return this.apiClient.get<ApiResponse<AssetModel[]>>(
      `/api/campaigns/${campaignId}/assets/items`,
    );
  }

  createAssetFolder(
    request: CreateAssetFolderRequest,
  ): Observable<ApiResponse<AssetModel>> {
    return this.apiClient.post<ApiResponse<AssetModel>, CreateAssetFolderRequest>(
      '/api/assets/folders',
      request,
    );
  }

  createItemAsset(
    request: CreateItemAssetRequest,
  ): Observable<ApiResponse<AssetModel>> {
    return this.apiClient.post<ApiResponse<AssetModel>, CreateItemAssetRequest>(
      '/api/assets/items',
      request,
    );
  }

  updateItemAsset(
    assetId: number,
    request: UpdateItemAssetRequest,
  ): Observable<ApiResponse<AssetModel>> {
    return this.apiClient.put<ApiResponse<AssetModel>, UpdateItemAssetRequest>(
      `/api/assets/items/${assetId}`,
      request,
    );
  }

  invitePlayer(
    campaignId: string,
    request: CreateCampaignParticipationInviteRequest,
  ): Observable<ApiResponse<CampaignParticipationInviteModel>> {
    return this.apiClient.post<
      ApiResponse<CampaignParticipationInviteModel>,
      CreateCampaignParticipationInviteRequest
    >(`/api/campaigns/${campaignId}/invites`, request);
  }

  updateCampaignMemberNickname(
    campaignId: string,
    username: string,
    request: UpdateCampaignMemberNicknameRequest,
  ): Observable<ApiResponse<CampaignMemberInformationModel>> {
    return this.apiClient.put<
      ApiResponse<CampaignMemberInformationModel>,
      UpdateCampaignMemberNicknameRequest
    >(
      `/api/campaigns/${campaignId}/settings/members/${encodeURIComponent(username)}/nickname`,
      request,
    );
  }

  updateCampaignMemberSkills(
    campaignId: string,
    username: string,
    request: UpdateCampaignMemberSkillsRequest,
  ): Observable<ApiResponse<CampaignMemberInformationModel>> {
    return this.apiClient.put<
      ApiResponse<CampaignMemberInformationModel>,
      UpdateCampaignMemberSkillsRequest
    >(
      `/api/campaigns/${campaignId}/users/${encodeURIComponent(username)}/skills`,
      request,
    );
  }

  acceptInvite(campaignId: string): Observable<ApiResponse<CampaignInviteResolutionModel>> {
    return this.apiClient.post<ApiResponse<CampaignInviteResolutionModel>, null>(
      `/api/campaigns/${campaignId}/invites/accept`,
      null,
    );
  }

  rejectInvite(campaignId: string): Observable<ApiResponse<CampaignInviteResolutionModel>> {
    return this.apiClient.post<ApiResponse<CampaignInviteResolutionModel>, null>(
      `/api/campaigns/${campaignId}/invites/reject`,
      null,
    );
  }
}
