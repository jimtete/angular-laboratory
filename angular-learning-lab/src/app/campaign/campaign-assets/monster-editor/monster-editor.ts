import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  LucideArrowDown,
  LucideArrowLeft,
  LucideArrowUp,
  LucideCheck,
  LucidePencil,
  LucidePlus,
  LucideTrash2,
  LucideX,
} from '@lucide/angular';
import { finalize } from 'rxjs';

import {
  ApiError,
  MonsterAbilityRequest,
  MonsterApiService,
  MonsterFeatureCategory,
  MonsterFeatureModel,
  MonsterFeatureRequest,
  MonsterModel,
  MonsterProficiencyRequest,
  MonsterSpellcastingRequest,
  MonsterSpellSlotRequest,
  UpdateMonsterBasicInformationRequest,
} from '../../../Infrastructure';
import { ModalHelper } from '../../../shared/helpers/modal.helper';
import {
  MONSTER_ABILITY_OPTIONS,
  MONSTER_RACE_OPTIONS,
  MONSTER_SKILL_PROFICIENCY_OPTIONS,
  MONSTER_SIZE_OPTIONS,
} from '../monster-form-options';

interface MonsterFeatureSection {
  category: MonsterFeatureCategory;
  label: string;
  addLabel: string;
}

interface MonsterAbilityDraft {
  draftId: number;
  name: string;
  value: string;
  modifier: string;
  notes: string;
}

interface MonsterProficiencyDraft {
  draftId: number;
  name: string;
  bonus: string;
  notes: string;
}

interface MonsterFeatureDraft {
  id: number | null;
  name: string;
  description: string;
  category: MonsterFeatureCategory;
  usageNote: string;
  resourceCost: string;
  isSpell: boolean;
  spellLevel: string;
  castingTime: string;
  range: string;
  duration: string;
  concentration: boolean;
  sortOrder: number;
}

interface MonsterSpellcastingDraft {
  spellcastingAbility: string;
  spellSaveDC: string;
  spellAttackBonus: string;
  notes: string;
}

interface MonsterSpellSlotDraft {
  draftId: number;
  spellLevel: string;
  maximumSlots: string;
  remainingSlots: string;
}

const FEATURE_SECTIONS: MonsterFeatureSection[] = [
  { category: MonsterFeatureCategory.PassiveTrait, label: 'Passive Traits', addLabel: 'Add Passive Trait' },
  { category: MonsterFeatureCategory.Action, label: 'Actions', addLabel: 'Add Action' },
  { category: MonsterFeatureCategory.BonusAction, label: 'Bonus Actions', addLabel: 'Add Bonus Action' },
  { category: MonsterFeatureCategory.Reaction, label: 'Reactions', addLabel: 'Add Reaction' },
  { category: MonsterFeatureCategory.FreeAction, label: 'Free Actions / Switches', addLabel: 'Add Free Action' },
  { category: MonsterFeatureCategory.LegendaryAction, label: 'Legendary Actions', addLabel: 'Add Legendary Action' },
  { category: MonsterFeatureCategory.MythicAction, label: 'Mythic Actions', addLabel: 'Add Mythic Action' },
  { category: MonsterFeatureCategory.Spell, label: 'Spells', addLabel: 'Add Spell' },
];

@Component({
  selector: 'app-monster-editor',
  imports: [
    LucideArrowDown,
    LucideArrowLeft,
    LucideArrowUp,
    LucideCheck,
    LucidePencil,
    LucidePlus,
    LucideTrash2,
    LucideX,
  ],
  templateUrl: './monster-editor.html',
  styleUrl: './monster-editor.css',
})
export class MonsterEditor implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly monsterApiService = inject(MonsterApiService);
  private readonly modalHelper = inject(ModalHelper);

  protected readonly monster = signal<MonsterModel | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly isSavingBasicInformation = signal(false);
  protected readonly isSavingFeature = signal(false);
  protected readonly deletingFeatureId = signal<number | null>(null);
  protected readonly isReorderingFeatures = signal(false);
  protected readonly isSavingSpellcasting = signal(false);
  protected readonly isRemovingSpellcasting = signal(false);
  protected readonly loadError = signal('');
  protected readonly nameDraft = signal('');
  protected readonly sizeDraft = signal('');
  protected readonly raceDraft = signal('');
  protected readonly classDraft = signal('');
  protected readonly tagsDraft = signal<string[]>([]);
  protected readonly tagDraft = signal('');
  protected readonly notesDraft = signal('');
  protected readonly skillProficiencyDraft = signal('');
  protected readonly abilityDrafts = signal<MonsterAbilityDraft[]>([]);
  protected readonly proficiencyDrafts = signal<MonsterProficiencyDraft[]>([]);
  protected readonly featureDraft = signal<MonsterFeatureDraft | null>(null);
  protected readonly hasSpellcastingDraft = signal(false);
  protected readonly spellcastingDraft = signal<MonsterSpellcastingDraft>({
    spellcastingAbility: '',
    spellSaveDC: '',
    spellAttackBonus: '',
    notes: '',
  });
  protected readonly spellSlotDrafts = signal<MonsterSpellSlotDraft[]>([]);
  protected readonly canSaveBasicInformation = computed(() => (
    this.normalizeText(this.nameDraft()).length > 0 &&
    this.abilityDrafts().every((ability) => this.canKeepAbility(ability)) &&
    this.proficiencyDrafts().every((proficiency) => this.canKeepProficiency(proficiency)) &&
    !this.isSavingBasicInformation()
  ));
  protected readonly canSaveFeature = computed(() => {
    const draft = this.featureDraft();

    return !!draft && this.normalizeText(draft.name).length > 0 && !this.isSavingFeature();
  });
  protected readonly canSaveSpellcasting = computed(() => (
    this.hasSpellcastingDraft() &&
    this.spellSlotDrafts().every((slot) => this.canKeepSpellSlot(slot)) &&
    !this.isSavingSpellcasting()
  ));
  protected readonly selectedSkillProficiencies = computed(() => (
    this.proficiencyDrafts()
      .filter((proficiency) => MONSTER_SKILL_PROFICIENCY_OPTIONS.includes(proficiency.name))
      .map((proficiency) => proficiency.name)
  ));
  protected readonly availableSkillProficiencyOptions = computed(() => {
    const selectedSkillNames = new Set(this.selectedSkillProficiencies());

    return this.monsterSkillProficiencyOptions.filter((option) => !selectedSkillNames.has(option));
  });
  protected readonly featureSections = FEATURE_SECTIONS;
  protected readonly monsterSizeOptions = MONSTER_SIZE_OPTIONS;
  protected readonly monsterRaceOptions = MONSTER_RACE_OPTIONS;
  protected readonly monsterAbilityOptions = MONSTER_ABILITY_OPTIONS;
  protected readonly monsterSkillProficiencyOptions = MONSTER_SKILL_PROFICIENCY_OPTIONS;

  private monsterId = 0;
  private nextAbilityDraftId = 1;
  private nextProficiencyDraftId = 1;
  private nextSpellSlotDraftId = 1;

  ngOnInit(): void {
    const monsterId = Number(this.route.snapshot.paramMap.get('monsterId'));

    if (!Number.isInteger(monsterId) || monsterId < 1) {
      this.loadError.set('Monster id is invalid.');
      return;
    }

    this.monsterId = monsterId;
    this.loadMonster();
  }

  protected backToAssets(): void {
    void this.router.navigate(['/assets']);
  }

  protected setNameDraft(event: Event): void {
    this.nameDraft.set(this.getInputValue(event));
  }

  protected setSizeDraft(event: Event): void {
    this.sizeDraft.set(this.getInputValue(event));
  }

  protected setRaceDraft(event: Event): void {
    this.raceDraft.set(this.getInputValue(event));
  }

  protected setClassDraft(event: Event): void {
    this.classDraft.set(this.getInputValue(event));
  }

  protected setNotesDraft(event: Event): void {
    this.notesDraft.set(this.getTextAreaValue(event));
  }

  protected setTagDraft(event: Event): void {
    this.tagDraft.set(this.getInputValue(event));
  }

  protected setSkillProficiencyDraft(event: Event): void {
    this.skillProficiencyDraft.set(this.getInputValue(event));
  }

  protected handleTagKeydown(event: KeyboardEvent): void {
    if (event.key !== 'Enter') {
      return;
    }

    event.preventDefault();
    this.addTagFromDraft();
  }

  protected addTagFromDraft(): void {
    const tag = this.normalizeText(this.tagDraft());

    if (!tag) {
      return;
    }

    const existingTags = new Set(this.tagsDraft().map((value) => value.toLowerCase()));

    if (!existingTags.has(tag.toLowerCase())) {
      this.tagsDraft.update((tags) => [...tags, tag]);
    }

    this.tagDraft.set('');
  }

  protected removeTag(tag: string): void {
    this.tagsDraft.update((tags) => tags.filter((value) => value !== tag));
  }

  protected updateAbilityDraft(
    draftId: number,
    field: keyof Omit<MonsterAbilityDraft, 'draftId'>,
    event: Event,
  ): void {
    const value = field === 'notes' ? this.getTextAreaValue(event) : this.getInputValue(event);

    this.abilityDrafts.update((abilities) => abilities.map((ability) => (
      ability.draftId === draftId
        ? {
          ...ability,
          [field]: value,
          modifier: field === 'value' ? this.calculateModifierInput(value) : ability.modifier,
        }
        : ability
    )));
  }

  protected removeAbilityDraft(draftId: number): void {
    this.abilityDrafts.update((abilities) => abilities.filter((ability) => ability.draftId !== draftId));
  }

  protected toggleAbilityProficiency(ability: MonsterAbilityDraft): void {
    const proficiencyName = this.toAbilitySavingThrowProficiencyName(ability.name);

    if (!proficiencyName) {
      return;
    }

    const existingProficiency = this.findProficiencyByName(proficiencyName);

    if (existingProficiency) {
      this.removeProficiencyDraft(existingProficiency.draftId);
      return;
    }

    this.addProficiencyByName(proficiencyName);
  }

  protected isAbilityProficient(ability: MonsterAbilityDraft): boolean {
    const proficiencyName = this.toAbilitySavingThrowProficiencyName(ability.name);

    return !!proficiencyName && !!this.findProficiencyByName(proficiencyName);
  }

  protected removeProficiencyDraft(draftId: number): void {
    this.proficiencyDrafts.update((proficiencies) => (
      proficiencies.filter((proficiency) => proficiency.draftId !== draftId)
    ));
  }

  protected handleSkillProficiencyKeydown(event: KeyboardEvent): void {
    if (event.key !== 'Enter') {
      return;
    }

    event.preventDefault();
    this.addSkillProficiencyFromDraft();
  }

  protected addSkillProficiencyFromDraft(): void {
    const proficiencyName = this.normalizeText(this.skillProficiencyDraft());

    if (
      !proficiencyName ||
      !MONSTER_SKILL_PROFICIENCY_OPTIONS.includes(proficiencyName) ||
      this.findProficiencyByName(proficiencyName)
    ) {
      this.skillProficiencyDraft.set('');
      return;
    }

    this.addProficiencyByName(proficiencyName);
    this.skillProficiencyDraft.set('');
  }

  protected removeSkillProficiency(proficiencyName: string): void {
    const proficiency = this.findProficiencyByName(proficiencyName);

    if (proficiency) {
      this.removeProficiencyDraft(proficiency.draftId);
    }
  }

  protected saveBasicInformation(): void {
    if (!this.canSaveBasicInformation()) {
      return;
    }

    const request: UpdateMonsterBasicInformationRequest = {
      name: this.normalizeText(this.nameDraft()),
      size: this.toNullableText(this.sizeDraft()),
      race: this.toNullableText(this.raceDraft()),
      class: this.toNullableText(this.classDraft()),
      tags: this.tagsDraft().length > 0 ? this.tagsDraft() : null,
      abilities: this.toAbilityRequests(),
      proficiencies: this.toProficiencyRequests(),
      notes: this.toNullableText(this.notesDraft()),
    };

    this.isSavingBasicInformation.set(true);

    this.monsterApiService
      .updateMonsterBasicInformation(this.monsterId, request)
      .pipe(finalize(() => this.isSavingBasicInformation.set(false)))
      .subscribe({
        next: (response) => {
          if (response.data) {
            this.monster.set(response.data);
            this.populateBasicDrafts(response.data);
          }

          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Monster basic information could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected featuresForCategory(category: MonsterFeatureCategory): MonsterFeatureModel[] {
    return (this.monster()?.features ?? [])
      .filter((feature) => this.toFeatureCategory(feature.category) === category)
      .sort((first, second) => first.sortOrder - second.sortOrder || first.id - second.id);
  }

  protected openAddFeature(category: MonsterFeatureCategory): void {
    this.featureDraft.set({
      id: null,
      name: '',
      description: '',
      category,
      usageNote: '',
      resourceCost: '',
      isSpell: category === MonsterFeatureCategory.Spell,
      spellLevel: '',
      castingTime: '',
      range: '',
      duration: '',
      concentration: false,
      sortOrder: 0,
    });
  }

  protected openEditFeature(feature: MonsterFeatureModel): void {
    this.featureDraft.set({
      id: feature.id,
      name: feature.name,
      description: feature.description ?? '',
      category: this.toFeatureCategory(feature.category) ?? MonsterFeatureCategory.Action,
      usageNote: feature.usageNote ?? '',
      resourceCost: this.toInputNumber(feature.resourceCost),
      isSpell: feature.isSpell,
      spellLevel: this.toInputNumber(feature.spellLevel),
      castingTime: feature.castingTime ?? '',
      range: feature.range ?? '',
      duration: feature.duration ?? '',
      concentration: feature.concentration ?? false,
      sortOrder: feature.sortOrder,
    });
  }

  protected closeFeatureDraft(): void {
    if (!this.isSavingFeature()) {
      this.featureDraft.set(null);
    }
  }

  protected updateFeatureDraft(
    field: keyof Omit<MonsterFeatureDraft, 'id'>,
    event: Event,
  ): void {
    const draft = this.featureDraft();

    if (!draft) {
      return;
    }

    let value: string | boolean | MonsterFeatureCategory;

    if (field === 'isSpell' || field === 'concentration') {
      value = (event.target as HTMLInputElement).checked;
    } else if (field === 'category') {
      value = Number((event.target as HTMLSelectElement).value) as MonsterFeatureCategory;
    } else {
      value = field === 'description' ? this.getTextAreaValue(event) : this.getInputValue(event);
    }

    this.featureDraft.set({ ...draft, [field]: value });
  }

  protected saveFeature(): void {
    const draft = this.featureDraft();

    if (!draft || !this.canSaveFeature()) {
      return;
    }

    const request = this.toFeatureRequest(draft);
    this.isSavingFeature.set(true);

    const saveFeature$ = draft.id === null
      ? this.monsterApiService.addMonsterFeature(this.monsterId, request)
      : this.monsterApiService.updateMonsterFeature(this.monsterId, draft.id, request);

    saveFeature$
      .pipe(finalize(() => this.isSavingFeature.set(false)))
      .subscribe({
        next: (response) => {
          if (response.data) {
            this.upsertLocalFeature(response.data);
          }

          this.featureDraft.set(null);
          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Monster feature could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected deleteFeature(feature: MonsterFeatureModel): void {
    if (this.deletingFeatureId()) {
      return;
    }

    this.deletingFeatureId.set(feature.id);

    this.monsterApiService
      .deleteMonsterFeature(this.monsterId, feature.id)
      .pipe(finalize(() => this.deletingFeatureId.set(null)))
      .subscribe({
        next: (response) => {
          this.monster.update((monster) => monster
            ? {
              ...monster,
              features: monster.features.filter((candidate) => candidate.id !== feature.id),
            }
            : monster);
          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Monster feature could not be deleted.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected moveFeature(category: MonsterFeatureCategory, feature: MonsterFeatureModel, direction: -1 | 1): void {
    const categoryFeatures = this.featuresForCategory(category);
    const currentIndex = categoryFeatures.findIndex((candidate) => candidate.id === feature.id);
    const targetIndex = currentIndex + direction;

    if (currentIndex < 0 || targetIndex < 0 || targetIndex >= categoryFeatures.length || this.isReorderingFeatures()) {
      return;
    }

    const reorderedCategoryFeatures = [...categoryFeatures];
    [reorderedCategoryFeatures[currentIndex], reorderedCategoryFeatures[targetIndex]] = [
      reorderedCategoryFeatures[targetIndex],
      reorderedCategoryFeatures[currentIndex],
    ];

    const orderedFeatures = this.featureSections.flatMap((section) => (
      section.category === category ? reorderedCategoryFeatures : this.featuresForCategory(section.category)
    ));

    this.isReorderingFeatures.set(true);

    this.monsterApiService
      .reorderMonsterFeatures(this.monsterId, {
        features: orderedFeatures.map((orderedFeature, index) => ({
          featureId: orderedFeature.id,
          sortOrder: index + 1,
        })),
      })
      .pipe(finalize(() => this.isReorderingFeatures.set(false)))
      .subscribe({
        next: (response) => {
          this.monster.update((monster) => monster && response.data
            ? { ...monster, features: response.data }
            : monster);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Monster features could not be reordered.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected featureCategoryLabel(category: MonsterFeatureCategory): string {
    return this.featureSections.find((section) => section.category === category)?.label ?? 'Feature';
  }

  protected featureCategorySlug(category: MonsterFeatureCategory): string {
    switch (category) {
      case MonsterFeatureCategory.PassiveTrait:
        return 'passive-trait';
      case MonsterFeatureCategory.Action:
        return 'action';
      case MonsterFeatureCategory.BonusAction:
        return 'bonus-action';
      case MonsterFeatureCategory.Reaction:
        return 'reaction';
      case MonsterFeatureCategory.LegendaryAction:
        return 'legendary-action';
      case MonsterFeatureCategory.MythicAction:
        return 'mythic-action';
      case MonsterFeatureCategory.FreeAction:
        return 'free-action';
      case MonsterFeatureCategory.Spell:
        return 'spell';
    }
  }

  protected featureMetadata(feature: MonsterFeatureModel): string[] {
    const metadata = [
      feature.resourceCost !== null ? `Cost ${feature.resourceCost}` : '',
      feature.isSpell ? 'Spell-like' : '',
      feature.spellLevel !== null ? `Level ${feature.spellLevel}` : '',
      feature.castingTime ? feature.castingTime : '',
      feature.range ? `Range ${feature.range}` : '',
      feature.duration ? feature.duration : '',
      feature.concentration ? 'Concentration' : '',
    ];

    return metadata.filter((value) => value.length > 0);
  }

  protected addSpellcasting(): void {
    this.hasSpellcastingDraft.set(true);
  }

  protected updateSpellcastingDraft(
    field: keyof MonsterSpellcastingDraft,
    event: Event,
  ): void {
    const value = field === 'notes' ? this.getTextAreaValue(event) : this.getInputValue(event);

    this.spellcastingDraft.update((draft) => ({ ...draft, [field]: value }));
  }

  protected addSpellSlotDraft(): void {
    this.spellSlotDrafts.update((slots) => [
      ...slots,
      {
        draftId: this.nextSpellSlotDraftId++,
        spellLevel: '',
        maximumSlots: '',
        remainingSlots: '',
      },
    ]);
  }

  protected updateSpellSlotDraft(
    draftId: number,
    field: keyof Omit<MonsterSpellSlotDraft, 'draftId'>,
    event: Event,
  ): void {
    const value = this.getInputValue(event);

    this.spellSlotDrafts.update((slots) => slots.map((slot) => (
      slot.draftId === draftId ? { ...slot, [field]: value } : slot
    )));
  }

  protected removeSpellSlotDraft(draftId: number): void {
    this.spellSlotDrafts.update((slots) => slots.filter((slot) => slot.draftId !== draftId));
  }

  protected saveSpellcasting(): void {
    if (!this.canSaveSpellcasting()) {
      return;
    }

    const request = this.toSpellcastingRequest();
    this.isSavingSpellcasting.set(true);

    this.monsterApiService
      .upsertMonsterSpellcasting(this.monsterId, request)
      .pipe(finalize(() => this.isSavingSpellcasting.set(false)))
      .subscribe({
        next: (response) => {
          this.monster.update((monster) => monster && response.data
            ? { ...monster, spellcasting: response.data }
            : monster);

          if (response.data) {
            this.populateSpellcastingDraft(response.data);
          }

          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Monster spellcasting could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected removeSpellcasting(): void {
    if (this.isRemovingSpellcasting()) {
      return;
    }

    this.isRemovingSpellcasting.set(true);

    this.monsterApiService
      .removeMonsterSpellcasting(this.monsterId)
      .pipe(finalize(() => this.isRemovingSpellcasting.set(false)))
      .subscribe({
        next: (response) => {
          this.monster.update((monster) => monster ? { ...monster, spellcasting: null } : monster);
          this.hasSpellcastingDraft.set(false);
          this.spellSlotDrafts.set([]);
          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Monster spellcasting could not be removed.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected hasSpellDetails(draft: MonsterFeatureDraft | null): boolean {
    return !!draft?.isSpell;
  }

  protected isFirstFeature(category: MonsterFeatureCategory, feature: MonsterFeatureModel): boolean {
    return this.featuresForCategory(category)[0]?.id === feature.id;
  }

  protected isLastFeature(category: MonsterFeatureCategory, feature: MonsterFeatureModel): boolean {
    const features = this.featuresForCategory(category);

    return features[features.length - 1]?.id === feature.id;
  }

  protected optionListWithCurrentValue(options: string[], currentValue: string): string[] {
    const normalizedValue = this.normalizeText(currentValue);

    return !normalizedValue || options.includes(normalizedValue)
      ? options
      : [normalizedValue, ...options];
  }

  protected formatModifier(value: string): string {
    const modifier = this.toOptionalNumber(value);

    if (modifier === null) {
      return '';
    }

    return modifier > 0 ? `+${modifier}` : String(modifier);
  }

  private loadMonster(): void {
    this.isLoading.set(true);
    this.loadError.set('');

    this.monsterApiService
      .fetchMonster(this.monsterId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          if (!response.data) {
            this.loadError.set('Monster was not found.');
            return;
          }

          this.monster.set(response.data);
          this.populateBasicDrafts(response.data);
          this.populateSpellcastingDraft(response.data.spellcasting);
        },
        error: (error: unknown) => {
          this.loadError.set(this.getErrorMessage(error, 'Monster could not be loaded.'));
          this.modalHelper.showError(this.loadError(), { statusCode: this.getErrorStatus(error) });
        },
      });
  }

  private populateBasicDrafts(monster: MonsterModel): void {
    const abilitiesByName = new Map(monster.abilities.map((ability) => [ability.name, ability]));
    const orderedAbilities = MONSTER_ABILITY_OPTIONS.map((abilityName) => abilitiesByName.get(abilityName) ?? {
        name: abilityName,
        value: null,
        modifier: null,
        notes: null,
      });

    this.nameDraft.set(monster.name);
    this.sizeDraft.set(monster.size ?? '');
    this.raceDraft.set(monster.race ?? '');
    this.classDraft.set(monster.class ?? '');
    this.tagsDraft.set([...(monster.tags ?? [])]);
    this.tagDraft.set('');
    this.skillProficiencyDraft.set('');
    this.notesDraft.set(monster.notes ?? '');
    this.abilityDrafts.set(orderedAbilities.map((ability) => ({
      draftId: this.nextAbilityDraftId++,
      name: ability.name,
      value: this.toInputNumber(ability.value),
      modifier: this.calculateModifierInput(this.toInputNumber(ability.value)),
      notes: ability.notes ?? '',
    })));
    this.proficiencyDrafts.set(monster.proficiencies.map((proficiency) => ({
      draftId: this.nextProficiencyDraftId++,
      name: proficiency.name,
      bonus: this.toInputNumber(proficiency.bonus),
      notes: proficiency.notes ?? '',
    })));
  }

  private populateSpellcastingDraft(spellcasting: MonsterModel['spellcasting']): void {
    this.hasSpellcastingDraft.set(spellcasting !== null);
    this.spellcastingDraft.set({
      spellcastingAbility: spellcasting?.spellcastingAbility ?? '',
      spellSaveDC: this.toInputNumber(spellcasting?.spellSaveDC ?? null),
      spellAttackBonus: this.toInputNumber(spellcasting?.spellAttackBonus ?? null),
      notes: spellcasting?.notes ?? '',
    });
    this.spellSlotDrafts.set((spellcasting?.spellSlots ?? []).map((slot) => ({
      draftId: this.nextSpellSlotDraftId++,
      spellLevel: String(slot.spellLevel),
      maximumSlots: this.toInputNumber(slot.maximumSlots),
      remainingSlots: this.toInputNumber(slot.remainingSlots),
    })));
  }

  private toAbilityRequests(): MonsterAbilityRequest[] | null {
    const abilities = this.abilityDrafts()
      .map((ability) => ({
        name: this.normalizeText(ability.name),
        value: this.toOptionalNumber(ability.value),
        modifier: this.toOptionalNumber(this.calculateModifierInput(ability.value)),
        notes: this.toNullableText(ability.notes),
      }));

    return abilities;
  }

  private toProficiencyRequests(): MonsterProficiencyRequest[] | null {
    const proficiencies = this.proficiencyDrafts()
      .filter((proficiency) => this.hasProficiencyContent(proficiency))
      .map((proficiency) => ({
        name: this.normalizeText(proficiency.name),
        bonus: this.toOptionalNumber(proficiency.bonus),
        notes: this.toNullableText(proficiency.notes),
      }));

    return proficiencies.length > 0 ? proficiencies : null;
  }

  private toFeatureRequest(draft: MonsterFeatureDraft): MonsterFeatureRequest {
    return {
      name: this.normalizeText(draft.name),
      description: this.toNullableText(draft.description),
      category: draft.category,
      usageNote: this.toNullableText(draft.usageNote),
      resourceCost: this.toOptionalNumber(draft.resourceCost),
      isSpell: draft.isSpell,
      spellLevel: this.toOptionalNumber(draft.spellLevel),
      castingTime: this.toNullableText(draft.castingTime),
      range: this.toNullableText(draft.range),
      duration: this.toNullableText(draft.duration),
      concentration: draft.isSpell ? draft.concentration : null,
      sortOrder: draft.sortOrder,
    };
  }

  private toSpellcastingRequest(): MonsterSpellcastingRequest {
    const draft = this.spellcastingDraft();
    const spellSlots = this.spellSlotDrafts()
      .filter((slot) => this.hasSpellSlotContent(slot))
      .map((slot): MonsterSpellSlotRequest => ({
        spellLevel: this.toOptionalNumber(slot.spellLevel) ?? 0,
        maximumSlots: this.toOptionalNumber(slot.maximumSlots),
        remainingSlots: this.toOptionalNumber(slot.remainingSlots),
      }));

    return {
      spellcastingAbility: this.toNullableText(draft.spellcastingAbility),
      spellSaveDC: this.toOptionalNumber(draft.spellSaveDC),
      spellAttackBonus: this.toOptionalNumber(draft.spellAttackBonus),
      notes: this.toNullableText(draft.notes),
      spellSlots: spellSlots.length > 0 ? spellSlots : null,
    };
  }

  private upsertLocalFeature(feature: MonsterFeatureModel): void {
    this.monster.update((monster) => {
      if (!monster) {
        return monster;
      }

      const existingFeatureIndex = monster.features.findIndex((candidate) => candidate.id === feature.id);
      const nextFeatures = existingFeatureIndex >= 0
        ? monster.features.map((candidate) => candidate.id === feature.id ? feature : candidate)
        : [...monster.features, feature];

      return {
        ...monster,
        features: nextFeatures.sort((first, second) => first.sortOrder - second.sortOrder || first.id - second.id),
      };
    });
  }

  private toFeatureCategory(
    category: MonsterFeatureModel['category'],
  ): MonsterFeatureCategory | null {
    if (typeof category === 'number') {
      return category in MonsterFeatureCategory ? category as MonsterFeatureCategory : null;
    }

    const numericCategory = Number(category);

    if (Number.isInteger(numericCategory) && numericCategory in MonsterFeatureCategory) {
      return numericCategory as MonsterFeatureCategory;
    }

    return MonsterFeatureCategory[category as keyof typeof MonsterFeatureCategory] ?? null;
  }

  private canKeepAbility(ability: MonsterAbilityDraft): boolean {
    return !this.hasAbilityContent(ability) || this.normalizeText(ability.name).length > 0;
  }

  private hasAbilityContent(ability: MonsterAbilityDraft): boolean {
    return [
      ability.name,
      ability.value,
      ability.notes,
    ].some((value) => this.normalizeText(value).length > 0);
  }

  private canKeepProficiency(proficiency: MonsterProficiencyDraft): boolean {
    return !this.hasProficiencyContent(proficiency) || this.normalizeText(proficiency.name).length > 0;
  }

  private hasProficiencyContent(proficiency: MonsterProficiencyDraft): boolean {
    return [
      proficiency.name,
      proficiency.bonus,
      proficiency.notes,
    ].some((value) => this.normalizeText(value).length > 0);
  }

  private canKeepSpellSlot(slot: MonsterSpellSlotDraft): boolean {
    const maximumSlots = this.toOptionalNumber(slot.maximumSlots);
    const remainingSlots = this.toOptionalNumber(slot.remainingSlots);

    return !this.hasSpellSlotContent(slot)
      || (
        this.toOptionalNumber(slot.spellLevel) !== null &&
        maximumSlots !== null &&
        maximumSlots >= 0 &&
        (remainingSlots === null || remainingSlots <= maximumSlots)
      );
  }

  private hasSpellSlotContent(slot: MonsterSpellSlotDraft): boolean {
    return [
      slot.spellLevel,
      slot.maximumSlots,
      slot.remainingSlots,
    ].some((value) => this.normalizeText(value).length > 0);
  }

  private toInputNumber(value: number | null): string {
    return value === null ? '' : String(value);
  }

  private calculateModifierInput(value: string): string {
    const abilityValue = this.toOptionalNumber(value);

    return abilityValue === null
      ? ''
      : String(Math.floor((abilityValue - 10) / 2));
  }

  private addProficiencyByName(proficiencyName: string): void {
    this.proficiencyDrafts.update((proficiencies) => [
      ...proficiencies,
      {
        draftId: this.nextProficiencyDraftId++,
        name: proficiencyName,
        bonus: '',
        notes: '',
      },
    ]);
  }

  private findProficiencyByName(proficiencyName: string): MonsterProficiencyDraft | null {
    const normalizedName = proficiencyName.toLowerCase();

    return this.proficiencyDrafts().find((proficiency) => (
      proficiency.name.toLowerCase() === normalizedName
    )) ?? null;
  }

  private toAbilitySavingThrowProficiencyName(abilityName: string): string | null {
    const normalizedAbilityName = this.normalizeText(abilityName);

    return MONSTER_ABILITY_OPTIONS.includes(normalizedAbilityName)
      ? `${normalizedAbilityName} Saving Throw`
      : null;
  }

  private toOptionalNumber(value: string): number | null {
    const normalizedValue = this.normalizeText(value);

    if (!normalizedValue) {
      return null;
    }

    const numericValue = Number(normalizedValue);

    return Number.isFinite(numericValue) ? numericValue : null;
  }

  private toNullableText(value: string | null | undefined): string | null {
    const normalizedValue = this.normalizeText(value);

    return normalizedValue || null;
  }

  private normalizeText(value: string | null | undefined): string {
    return value?.trim() ?? '';
  }

  private getInputValue(event: Event): string {
    return (event.target as HTMLInputElement | HTMLSelectElement).value;
  }

  private getTextAreaValue(event: Event): string {
    return (event.target as HTMLTextAreaElement).value;
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (this.isApiError(error) || error instanceof Error) {
      return error.message;
    }

    return fallback;
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
