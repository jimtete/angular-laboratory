import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LucideUserPlus } from '@lucide/angular';
import { finalize } from 'rxjs';

import {
  ApiError,
  CampaignApiService,
  TokenStorageService,
  UserApiService,
} from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';

@Component({
  selector: 'app-campaign-members',
  imports: [LucideUserPlus],
  templateUrl: './campaign-members.html',
  styleUrl: './campaign-members.css',
})
export class CampaignMembers {
  private readonly route = inject(ActivatedRoute);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly userApiService = inject(UserApiService);
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly modalHelper = inject(ModalHelper);

  protected readonly isMaster = computed(() => this.tokenStorage.hasAnyRole('Master'));
  protected readonly isInvitePopupOpen = signal(false);
  protected readonly playerUsernames = signal<string[]>([]);
  protected readonly invitedUsernames = signal<ReadonlySet<string>>(new Set<string>());
  protected readonly invitingUsername = signal<string | null>(null);

  protected openInvitePopup(): void {
    this.isInvitePopupOpen.set(true);
    this.fetchPlayerUsernames();
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
          this.invitedUsernames.update((usernames) => {
            const nextUsernames = new Set(usernames);
            nextUsernames.add(username);
            return nextUsernames;
          });
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
    return this.invitingUsername() !== null || this.invitedUsernames().has(username);
  }

  private fetchPlayerUsernames(): void {
    this.userApiService.fetchPlayerUsernames().subscribe({
      next: (response) => {
        this.playerUsernames.set(response.data ?? []);
      },
      error: (error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Players could not be loaded.'),
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
}
