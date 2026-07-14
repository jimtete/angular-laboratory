import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { LucideCheck, LucideX } from '@lucide/angular';
import { finalize } from 'rxjs';

import {
  API_BASE_URL,
  ApiError,
  CampaignApiService,
  CampaignPendingInviteModel,
} from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';

@Component({
  selector: 'app-my-campaign-invites',
  imports: [LucideCheck, LucideX],
  templateUrl: './my-campaign-invites.html',
  styleUrl: './my-campaign-invites.css',
})
export class MyCampaignInvites implements OnInit {
  private readonly apiBaseUrl = inject(API_BASE_URL).replace(/\/$/, '');
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly modalHelper = inject(ModalHelper);
  private readonly router = inject(Router);

  protected readonly invites = signal<CampaignPendingInviteModel[]>([]);
  protected readonly pendingAction = signal<'accept' | 'reject' | null>(null);
  protected readonly selectedInvite = signal<CampaignPendingInviteModel | null>(null);
  protected readonly resolvingCampaignId = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchInvites();
  }

  protected goBack(): void {
    void this.router.navigate(['/my-campaigns']);
  }

  protected openConfirmation(
    invite: CampaignPendingInviteModel,
    action: 'accept' | 'reject',
  ): void {
    if (this.resolvingCampaignId()) {
      return;
    }

    this.selectedInvite.set(invite);
    this.pendingAction.set(action);
  }

  protected closeConfirmation(): void {
    if (this.resolvingCampaignId()) {
      return;
    }

    this.selectedInvite.set(null);
    this.pendingAction.set(null);
  }

  protected confirmInviteResolution(): void {
    const invite = this.selectedInvite();
    const action = this.pendingAction();

    if (!invite || !action || this.resolvingCampaignId()) {
      return;
    }

    const request = action === 'accept'
      ? this.campaignApiService.acceptInvite(invite.campaignId)
      : this.campaignApiService.rejectInvite(invite.campaignId);

    this.resolvingCampaignId.set(invite.campaignId);

    request
      .pipe(finalize(() => this.resolvingCampaignId.set(null)))
      .subscribe({
        next: (response) => {
          this.invites.update((invites) =>
            invites.filter((item) => item.campaignId !== invite.campaignId),
          );
          this.selectedInvite.set(null);
          this.pendingAction.set(null);
          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(this.getErrorMessage(error), {
            statusCode: this.getErrorStatusCode(error),
          });
        },
      });
  }

  protected confirmationTitle(): string {
    return this.pendingAction() === 'accept'
      ? 'Accept Invite?'
      : 'Reject Invite?';
  }

  protected confirmationMessage(): string {
    const invite = this.selectedInvite();
    const campaignName = invite?.campaignName ?? 'this campaign';

    return this.pendingAction() === 'accept'
      ? `Are you sure you want to join ${campaignName}?`
      : `Are you sure you want to reject ${campaignName}?`;
  }

  protected getCampaignPictureSource(invite: CampaignPendingInviteModel): string | null {
    const imageSource = invite.campaignPictureUrl;

    if (!imageSource) {
      return null;
    }

    if (/^(https?:\/\/|data:)/i.test(imageSource)) {
      return imageSource;
    }

    return imageSource.startsWith('/')
      ? `${this.apiBaseUrl}${imageSource}`
      : `${this.apiBaseUrl}/${imageSource}`;
  }

  protected formatDate(value: string): string {
    const date = new Date(value);

    return Number.isNaN(date.getTime())
      ? value
      : date.toLocaleDateString(undefined, {
          year: 'numeric',
          month: 'short',
          day: 'numeric',
        });
  }

  private fetchInvites(): void {
    this.campaignApiService.fetchPendingInvites().subscribe({
      next: (response) => {
        this.invites.set(response.data ?? []);
      },
      error: (error: unknown) => {
        this.modalHelper.showError(this.getErrorMessage(error), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  private getErrorStatusCode(error: unknown): number | undefined {
    return this.isApiError(error) ? error.status : undefined;
  }

  private getErrorMessage(error: unknown): string {
    return this.isApiError(error) ? error.message : 'Campaign invites could not be fetched.';
  }

  private isApiError(error: unknown): error is ApiError {
    return (
      typeof error === 'object' &&
      error !== null &&
      'status' in error &&
      'message' in error &&
      typeof error.status === 'number' &&
      typeof error.message === 'string'
    );
  }
}
