import { Component, computed, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';

import {
  ApiError,
  CampaignApiService,
  CampaignInformationCacheService,
  CampaignMemberInformationModel,
  Skill,
  SkillValue,
  UpdateCampaignMemberSkillsRequest,
} from '../../../Infrastructure';
import { ModalHelper } from '../../../shared/helpers/modal.helper';

type ProficiencyLevel = 'none' | 'half' | 'full' | 'expertise';

interface SkillDefinition {
  skill: Skill;
  label: string;
}

interface SkillGroup {
  ability: string;
  shortLabel?: string;
  color: string;
  skills: SkillDefinition[];
}

@Component({
  selector: 'app-campaign-member-proficiencies',
  templateUrl: './campaign-member-proficiencies.html',
  styleUrl: './campaign-member-proficiencies.css',
})
export class CampaignMemberProficiencies {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly campaignInformationCache = inject(CampaignInformationCacheService);
  private readonly modalHelper = inject(ModalHelper);
  private readonly userId = this.route.snapshot.paramMap.get('userId');

  protected readonly draftLevels = signal<Record<number, ProficiencyLevel>>({});
  private readonly initialLevels = signal<Record<number, ProficiencyLevel>>({});
  protected readonly isSaving = signal(false);
  protected readonly member = computed(() => (
    this.campaignInformationCache.joinedMembers()
      .find((member) => member.userId === this.userId) ?? null
  ));
  protected readonly memberName = computed(() => {
    const member = this.member();

    return member?.nickname || member?.username || 'Campaign Member';
  });
  protected readonly leftSkillGroups: SkillGroup[] = [
    {
      ability: 'Strength',
      shortLabel: 'STR',
      color: '#b91c1c',
      skills: [{ skill: Skill.Athletics, label: 'Athletics' }],
    },
    {
      ability: 'Dexterity',
      color: '#15803d',
      skills: [
        { skill: Skill.Acrobatics, label: 'Acrobatics' },
        { skill: Skill.SleightOfHand, label: 'Sleight of Hand' },
        { skill: Skill.Stealth, label: 'Stealth' },
      ],
    },
    {
      ability: 'Intelligence',
      color: '#4f46e5',
      skills: [
        { skill: Skill.Arcana, label: 'Arcana' },
        { skill: Skill.History, label: 'History' },
        { skill: Skill.Investigation, label: 'Investigation' },
        { skill: Skill.Nature, label: 'Nature' },
        { skill: Skill.Religion, label: 'Religion' },
      ],
    },
  ];
  protected readonly rightSkillGroups: SkillGroup[] = [
    {
      ability: 'Wisdom',
      color: '#0e7490',
      skills: [
        { skill: Skill.AnimalHandling, label: 'Animal Handling' },
        { skill: Skill.Insight, label: 'Insight' },
        { skill: Skill.Medicine, label: 'Medicine' },
        { skill: Skill.Perception, label: 'Perception' },
        { skill: Skill.Survival, label: 'Survival' },
      ],
    },
    {
      ability: 'Charisma',
      color: '#a21caf',
      skills: [
        { skill: Skill.Deception, label: 'Deception' },
        { skill: Skill.Intimidation, label: 'Intimidation' },
        { skill: Skill.Performance, label: 'Performance' },
        { skill: Skill.Persuasion, label: 'Persuasion' },
      ],
    },
  ];

  constructor() {
    effect(() => {
      const member = this.member();

      if (!member) {
        return;
      }

      const levels = this.toLevels(member);
      this.initialLevels.set(levels);
      this.draftLevels.set(levels);
    });
  }

  protected levelFor(skill: Skill): ProficiencyLevel {
    return this.draftLevels()[skill] ?? 'none';
  }

  protected cycleSkill(skill: Skill): void {
    if (this.isSaving()) {
      return;
    }

    this.draftLevels.update((levels) => ({
      ...levels,
      [skill]: this.nextLevel(levels[skill] ?? 'none'),
    }));
  }

  protected goBack(): void {
    const campaignId = this.route.parent?.snapshot.paramMap.get('campaignId');

    void this.router.navigate(['/campaigns', campaignId, 'campaign-members']);
  }

  protected discardChanges(): void {
    if (this.isSaving()) {
      return;
    }

    this.draftLevels.set({ ...this.initialLevels() });
  }

  protected saveChanges(): void {
    const campaignId = this.getCampaignId();
    const member = this.member();

    if (!campaignId || !member || this.isSaving()) {
      return;
    }

    this.isSaving.set(true);

    this.campaignApiService
      .updateCampaignMemberSkills(campaignId, member.username, this.toUpdateRequest())
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (response) => {
          if (response.data) {
            this.campaignInformationCache.updateJoinedMember(response.data);
          }

          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign member skills could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private nextLevel(level: ProficiencyLevel): ProficiencyLevel {
    switch (level) {
      case 'none':
        return 'half';
      case 'half':
        return 'full';
      case 'full':
        return 'expertise';
      case 'expertise':
        return 'none';
    }
  }

  private toLevels(member: CampaignMemberInformationModel): Record<number, ProficiencyLevel> {
    const levels: Record<number, ProficiencyLevel> = {};

    this.toSkillSet(member.halfProficientSkills).forEach((skill) => {
      levels[skill] = 'half';
    });
    this.toSkillSet(member.proficientSkills).forEach((skill) => {
      levels[skill] = 'full';
    });
    this.toSkillSet(member.expertiseSkills).forEach((skill) => {
      levels[skill] = 'expertise';
    });

    return levels;
  }

  private toUpdateRequest(): UpdateCampaignMemberSkillsRequest {
    const request: UpdateCampaignMemberSkillsRequest = {
      halfProficientSkills: [],
      proficientSkills: [],
      expertiseSkills: [],
    };

    Object.entries(this.draftLevels()).forEach(([skillValue, level]) => {
      const skill = Number(skillValue) as Skill;

      switch (level) {
        case 'half':
          request.halfProficientSkills.push(skill);
          break;
        case 'full':
          request.proficientSkills.push(skill);
          break;
        case 'expertise':
          request.expertiseSkills.push(skill);
          break;
        case 'none':
          break;
      }
    });

    return request;
  }

  private getCampaignId(): string | null {
    return this.route.parent?.snapshot.paramMap.get('campaignId') ?? null;
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
