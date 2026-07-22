export enum MonsterFeatureCategory {
  PassiveTrait = 1,
  Action = 2,
  BonusAction = 3,
  Reaction = 4,
  LegendaryAction = 5,
  MythicAction = 6,
  FreeAction = 7,
  Spell = 8,
}

export interface MonsterListModel {
  id: number;
  name: string;
  size: string | null;
  race: string | null;
  class: string | null;
  tags: string[] | null;
}

export interface MonsterAbilityModel {
  name: string;
  value: number | null;
  modifier: number | null;
  notes: string | null;
}

export interface MonsterProficiencyModel {
  name: string;
  bonus: number | null;
  notes: string | null;
}

export interface MonsterFeatureModel {
  id: number;
  name: string;
  description: string | null;
  category: MonsterFeatureCategory | keyof typeof MonsterFeatureCategory | string | number;
  usageNote: string | null;
  resourceCost: number | null;
  isSpell: boolean;
  spellLevel: number | null;
  castingTime: string | null;
  range: string | null;
  duration: string | null;
  concentration: boolean | null;
  sortOrder: number;
}

export interface MonsterSpellSlotModel {
  spellLevel: number;
  maximumSlots: number | null;
  remainingSlots: number | null;
}

export interface MonsterSpellcastingModel {
  spellcastingAbility: string | null;
  spellSaveDC: number | null;
  spellAttackBonus: number | null;
  notes: string | null;
  spellSlots: MonsterSpellSlotModel[];
}

export interface MonsterModel extends MonsterListModel {
  abilities: MonsterAbilityModel[];
  proficiencies: MonsterProficiencyModel[];
  spellcasting: MonsterSpellcastingModel | null;
  features: MonsterFeatureModel[];
  notes: string | null;
}

export interface MonsterAbilityRequest {
  name: string | null;
  value: number | null;
  modifier: number | null;
  notes: string | null;
}

export interface MonsterProficiencyRequest {
  name: string | null;
  bonus: number | null;
  notes: string | null;
}

export interface MonsterFeatureRequest {
  name: string | null;
  description: string | null;
  category: MonsterFeatureCategory;
  usageNote: string | null;
  resourceCost: number | null;
  isSpell: boolean;
  spellLevel: number | null;
  castingTime: string | null;
  range: string | null;
  duration: string | null;
  concentration: boolean | null;
  sortOrder: number;
}

export interface MonsterSpellSlotRequest {
  spellLevel: number;
  maximumSlots: number | null;
  remainingSlots: number | null;
}

export interface MonsterSpellcastingRequest {
  spellcastingAbility: string | null;
  spellSaveDC: number | null;
  spellAttackBonus: number | null;
  notes: string | null;
  spellSlots: MonsterSpellSlotRequest[] | null;
}

export interface CreateMonsterRequest {
  name: string | null;
  size: string | null;
  race: string | null;
  class: string | null;
  tags: string[] | null;
  abilities: MonsterAbilityRequest[] | null;
  proficiencies: MonsterProficiencyRequest[] | null;
  spellcasting: MonsterSpellcastingRequest | null;
  features: MonsterFeatureRequest[] | null;
  notes: string | null;
}

export interface UpdateMonsterBasicInformationRequest {
  name: string | null;
  size: string | null;
  race: string | null;
  class: string | null;
  tags: string[] | null;
  abilities: MonsterAbilityRequest[] | null;
  proficiencies: MonsterProficiencyRequest[] | null;
  notes: string | null;
}

export interface MonsterFeatureOrderRequest {
  featureId: number;
  sortOrder: number;
}

export interface ReorderMonsterFeaturesRequest {
  features: MonsterFeatureOrderRequest[] | null;
}

export interface UpdateRemainingSpellSlotsRequest {
  remainingSlots: number | null;
}
