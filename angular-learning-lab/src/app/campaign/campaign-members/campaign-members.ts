import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LucideCheck, LucideUserPlus, LucideX } from '@lucide/angular';
import { finalize, forkJoin } from 'rxjs';

import {
  ApiError,
  CampaignApiService,
  CampaignInformationCacheService,
  CampaignMemberInformationModel,
  TokenStorageService,
  UserApiService,
} from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';

@Component({
  selector: 'app-campaign-members',
  imports: [LucideCheck, LucideUserPlus, LucideX],
  templateUrl: './campaign-members.html',
  styleUrl: './campaign-members.css',
})
export class CampaignMembers {
  private readonly route = inject(ActivatedRoute);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly userApiService = inject(UserApiService);
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly campaignInformationCache = inject(CampaignInformationCacheService);
  private readonly modalHelper = inject(ModalHelper);

  protected readonly isMaster = computed(() => this.tokenStorage.hasAnyRole('Master'));
  protected readonly members = computed(() => this.campaignInformationCache.joinedMembers());
  protected readonly isInvitePopupOpen = signal(false);
  protected readonly playerUsernames = signal<string[]>([]);
  protected readonly invitingUsername = signal<string | null>(null);
  protected readonly nicknameDrafts = signal<Record<string, string>>({});
  protected readonly savingNicknameUsername = signal<string | null>(null);
  private readonly unavailableUsernames = signal<ReadonlySet<string>>(new Set<string>());

  protected openInvitePopup(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || !this.isMaster()) {
      return;
    }

    forkJoin({
      players: this.userApiService.fetchPlayerUsernames(),
      campaignUsers: this.campaignApiService.fetchCampaignUsernames(campaignId),
    }).subscribe({
      next: ({ players, campaignUsers }) => {
        const unavailableUsernames = this.toUsernameSet([
          ...(campaignUsers.data?.joinedUsernames ?? []),
          ...(campaignUsers.data?.invitedUsernames ?? []),
        ]);

        this.unavailableUsernames.set(unavailableUsernames);
        this.playerUsernames.set(
          (players.data ?? []).filter(
            (username) => !unavailableUsernames.has(this.normalizeUsername(username)),
          ),
        );
        this.isInvitePopupOpen.set(true);
      },
      error: (error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Players could not be loaded.'),
          { statusCode: this.getErrorStatus(error) },
        );
      },
    });
  }

  protected closeInvitePopup(): void {
    this.isInvitePopupOpen.set(false);
  }

  protected invitePlayer(username: string): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.invitingUsername()) {
      return;
    }

    this.invitingUsername.set(username);

    this.campaignApiService
      .invitePlayer(campaignId, { username })
      .pipe(finalize(() => this.invitingUsername.set(null)))
      .subscribe({
        next: (response) => {
          this.unavailableUsernames.update((usernames) => {
            const nextUsernames = new Set(usernames);
            nextUsernames.add(this.normalizeUsername(username));
            return nextUsernames;
          });
          this.playerUsernames.update((usernames) =>
            usernames.filter(
              (existingUsername) =>
                this.normalizeUsername(existingUsername) !== this.normalizeUsername(username),
            ),
          );
          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Player could not be invited.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected isInviteDisabled(username: string): boolean {
    return (
      this.invitingUsername() !== null ||
      this.unavailableUsernames().has(this.normalizeUsername(username))
    );
  }

  protected nicknameValue(member: CampaignMemberInformationModel): string {
    return this.nicknameDrafts()[member.username] ?? member.nickname ?? '';
  }

  protected setNicknameDraft(username: string, event: Event): void {
    const value = (event.target as HTMLInputElement).value;

    this.nicknameDrafts.update((drafts) => ({
      ...drafts,
      [username]: value,
    }));
  }

  protected isNicknameDirty(member: CampaignMemberInformationModel): boolean {
    return this.normalizeNickname(this.nicknameValue(member)) !==
      this.normalizeNickname(member.nickname);
  }

  protected discardNickname(member: CampaignMemberInformationModel): void {
    this.nicknameDrafts.update((drafts) => ({
      ...drafts,
      [member.username]: member.nickname ?? '',
    }));
  }

  protected saveNickname(member: CampaignMemberInformationModel): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.savingNicknameUsername() || !this.isNicknameDirty(member)) {
      return;
    }

    this.savingNicknameUsername.set(member.username);

    this.campaignApiService
      .updateCampaignMemberNickname(campaignId, member.username, {
        nickname: this.toNullableNickname(this.nicknameValue(member)),
      })
      .pipe(finalize(() => this.savingNicknameUsername.set(null)))
      .subscribe({
        next: (response) => {
          if (response.data) {
            this.campaignInformationCache.updateJoinedMember(response.data);
            this.nicknameDrafts.update((drafts) => ({
              ...drafts,
              [response.data!.username]: response.data!.nickname ?? '',
            }));
          }

          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Nickname could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private getCampaignId(): string | null {
    return (
      this.route.parent?.snapshot.paramMap.get('campaignId') ??
      this.route.snapshot.paramMap.get('campaignId')
    );
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return this.isApiError(error) ? error.message : fallback;
  }

  private getErrorStatus(error: unknown): number | undefined {
    return this.isApiError(error) ? error.status : undefined;
  }

  private isApiError(error: unknown): error is ApiError {
    return (
      typeof error === 'object' &&
      error !== null &&
      'message' in error &&
      typeof error.message === 'string' &&
      'status' in error &&
      typeof error.status === 'number'
    );
  }

  private toUsernameSet(usernames: string[]): ReadonlySet<string> {
    return new Set(usernames.map((username) => this.normalizeUsername(username)));
  }

  private normalizeUsername(username: string): string {
    return username.trim().toLowerCase();
  }

  private normalizeNickname(nickname: string | null | undefined): string {
    return nickname?.trim() ?? '';
  }

  private toNullableNickname(nickname: string): string | null {
    const trimmedNickname = nickname.trim();

    return trimmedNickname ? trimmedNickname : null;
  }
}
