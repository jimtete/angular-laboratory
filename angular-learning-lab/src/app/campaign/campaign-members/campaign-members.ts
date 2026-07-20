import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  LucideAngry,
  LucideBrain,
  LucideCheck,
  LucideChurch,
  LucideDog,
  LucideDumbbell,
  LucideEye,
  LucideFingerprint,
  LucideFootprints,
  LucideHandCoins,
  LucideHandshake,
  LucideHeartPulse,
  LucideMicVocal,
  LucideMountain,
  LucideScrollText,
  LucideSearch,
  LucideSparkles,
  LucideTheater,
  LucideTrees,
  LucideUserPlus,
  LucideX,
} from '@lucide/angular';
import { finalize, forkJoin } from 'rxjs';

import {
  ApiError,
  CampaignApiService,
  CampaignInformationCacheService,
  CampaignMemberInformationModel,
  Skill,
  SkillValue,
  TokenStorageService,
  UserApiService,
} from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';

type ProficiencyLevel = 'half' | 'full' | 'expertise';
type SkillIconKey =
  | 'acrobatics'
  | 'animalHandling'
  | 'arcana'
  | 'athletics'
  | 'deception'
  | 'history'
  | 'insight'
  | 'intimidation'
  | 'investigation'
  | 'medicine'
  | 'nature'
  | 'perception'
  | 'performance'
  | 'persuasion'
  | 'religion'
  | 'sleightOfHand'
  | 'stealth'
  | 'survival';

interface SkillDefinition {
  skill: Skill;
  label: string;
  icon: SkillIconKey;
}

interface MemberProficiencyIcon extends SkillDefinition {
  level: ProficiencyLevel;
  levelLabel: string;
}

@Component({
  selector: 'app-campaign-members',
  imports: [
    LucideAngry,
    LucideBrain,
    LucideCheck,
    LucideChurch,
    LucideDog,
    LucideDumbbell,
    LucideEye,
    LucideFingerprint,
    LucideFootprints,
    LucideHandCoins,
    LucideHandshake,
    LucideHeartPulse,
    LucideMicVocal,
    LucideMountain,
    LucideScrollText,
    LucideSearch,
    LucideSparkles,
    LucideTheater,
    LucideTrees,
    LucideUserPlus,
    LucideX,
  ],
  templateUrl: './campaign-members.html',
  styleUrl: './campaign-members.css',
})
export class CampaignMembers {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
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
  protected readonly skillDefinitions: SkillDefinition[] = [
    { skill: Skill.Acrobatics, label: 'Acrobatics', icon: 'acrobatics' },
    { skill: Skill.AnimalHandling, label: 'Animal Handling', icon: 'animalHandling' },
    { skill: Skill.Arcana, label: 'Arcana', icon: 'arcana' },
    { skill: Skill.Athletics, label: 'Athletics', icon: 'athletics' },
    { skill: Skill.Deception, label: 'Deception', icon: 'deception' },
    { skill: Skill.History, label: 'History', icon: 'history' },
    { skill: Skill.Insight, label: 'Insight', icon: 'insight' },
    { skill: Skill.Intimidation, label: 'Intimidation', icon: 'intimidation' },
    { skill: Skill.Investigation, label: 'Investigation', icon: 'investigation' },
    { skill: Skill.Medicine, label: 'Medicine', icon: 'medicine' },
    { skill: Skill.Nature, label: 'Nature', icon: 'nature' },
    { skill: Skill.Perception, label: 'Perception', icon: 'perception' },
    { skill: Skill.Performance, label: 'Performance', icon: 'performance' },
    { skill: Skill.Persuasion, label: 'Persuasion', icon: 'persuasion' },
    { skill: Skill.Religion, label: 'Religion', icon: 'religion' },
    { skill: Skill.SleightOfHand, label: 'Sleight of Hand', icon: 'sleightOfHand' },
    { skill: Skill.Stealth, label: 'Stealth', icon: 'stealth' },
    { skill: Skill.Survival, label: 'Survival', icon: 'survival' },
  ];

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

  protected openProficiencies(member: CampaignMemberInformationModel): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || !this.isMaster()) {
      return;
    }

    void this.router.navigate([
      '/campaigns',
      campaignId,
      'campaign-members',
      member.userId,
      'proficiencies',
    ]);
  }

  protected handleProficienciesKeydown(
    member: CampaignMemberInformationModel,
    event: KeyboardEvent,
  ): void {
    if (event.key !== 'Enter' && event.key !== ' ') {
      return;
    }

    event.preventDefault();
    this.openProficiencies(member);
  }

  protected memberProficiencies(member: CampaignMemberInformationModel): MemberProficiencyIcon[] {
    const halfProficientSkills = this.toSkillSet(member.halfProficientSkills);
    const proficientSkills = this.toSkillSet(member.proficientSkills);
    const expertiseSkills = this.toSkillSet(member.expertiseSkills);

    return this.skillDefinitions
      .map((definition): MemberProficiencyIcon | null => {
        if (expertiseSkills.has(definition.skill)) {
          return { ...definition, level: 'expertise', levelLabel: 'Expertise' };
        }

        if (proficientSkills.has(definition.skill)) {
          return { ...definition, level: 'full', levelLabel: 'Proficient' };
        }

        if (halfProficientSkills.has(definition.skill)) {
          return { ...definition, level: 'half', levelLabel: 'Half proficient' };
        }

        return null;
      })
      .filter((proficiency): proficiency is MemberProficiencyIcon => proficiency !== null);
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

  private toSkillSet(skills: SkillValue[] | null | undefined): ReadonlySet<Skill> {
    return new Set(
      (skills ?? [])
        .map((skill) => this.toSkill(skill))
        .filter((skill): skill is Skill => skill !== null),
    );
  }

  private toSkill(skill: SkillValue): Skill | null {
    if (typeof skill === 'number') {
      return skill as Skill;
    }

    const parsedSkill = Number(skill);

    if (Number.isFinite(parsedSkill)) {
      return parsedSkill as Skill;
    }

    return Skill[skill as keyof typeof Skill] ?? null;
  }
}
