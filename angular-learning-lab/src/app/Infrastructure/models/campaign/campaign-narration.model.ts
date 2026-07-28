export enum CampaignEventType {
  BooleanFlag = 0,
  SingleChoice = 1,
  TextValue = 2,
  NumericValue = 3,
}

export enum OutcomeEffectOperation {
  Set = 0,
  Clear = 1,
  Increment = 2,
  Decrement = 3,
}

export enum ChoiceSelectionMode {
  Single = 0,
  Multiple = 1,
  ExactlyOne = 2,
}

export enum RuleGroupOperator {
  And = 0,
  Or = 1,
  ExactlyOne = 2,
}

export enum RuleComparisonOperator {
  IsSet = 0,
  IsNotSet = 1,
  Equals = 2,
  NotEquals = 3,
  GreaterThan = 4,
  GreaterThanOrEqual = 5,
  LessThan = 6,
  LessThanOrEqual = 7,
}

export enum ConditionalTargetType {
  StoryBlock = 0,
  StoryBeat = 1,
  InformationBeat = 2,
}

export enum ConditionalRuleEffectType {
  RequiredForAvailability = 0,
  RequiredForVisibility = 1,
  ExclusivePath = 2,
  OptionalInformation = 3,
}

export enum OutcomeSourceType {
  StoryBlock = 0,
  StoryBeat = 1,
  ChoiceOption = 2,
  DecisionChoice = 3,
  RoleplayingNpcInteraction = 4,
  RoleplayingInformation = 5,
}


export type CampaignEventTypeValue =
  CampaignEventType | keyof typeof CampaignEventType | string | number;

export type OutcomeEffectOperationValue =
  OutcomeEffectOperation | keyof typeof OutcomeEffectOperation | string | number;

export type ChoiceSelectionModeValue =
  ChoiceSelectionMode | keyof typeof ChoiceSelectionMode | string | number;

export type RuleGroupOperatorValue =
  RuleGroupOperator | keyof typeof RuleGroupOperator | string | number;

export type RuleComparisonOperatorValue =
  RuleComparisonOperator | keyof typeof RuleComparisonOperator | string | number;

export type ConditionalTargetTypeValue =
  ConditionalTargetType | keyof typeof ConditionalTargetType | string | number;

export type ConditionalRuleEffectTypeValue =
  ConditionalRuleEffectType | keyof typeof ConditionalRuleEffectType | string | number;

export type OutcomeSourceTypeValue =
  OutcomeSourceType | keyof typeof OutcomeSourceType | string | number;


export interface CampaignEventModel {
  id: string;
  campaignId: string;
  key: string;
  name: string;
  description: string | null;
  eventType: CampaignEventTypeValue;
  type?: CampaignEventTypeValue;
  isRepeatable: boolean;
  options: CampaignEventOptionModel[];
}

export interface CampaignEventOptionModel {
  id: string;
  campaignEventDefinitionId: string;
  key: string;
  label: string;
  description: string | null;
  sortOrder: number;
}

export interface CampaignEventOptionRequest {
  key: string | null;
  label: string | null;
  description: string | null;
  sortOrder: number;
}

export interface CampaignEventRequest {
  key: string | null;
  name: string | null;
  description: string | null;
  eventType: CampaignEventType;
  isRepeatable: boolean;
}

export interface CampaignEventStateModel {
  id: string;
  campaignSessionId: number;
  campaignEventDefinitionId: string;
  eventKey: string;
  eventType: CampaignEventTypeValue;
  booleanValue: boolean | null;
  selectedOptionId: string | null;
  selectedOptionKey: string | null;
  textValue: string | null;
  numericValue: number | null;
  sourceStoryBlockId: string | null;
  sourceStoryBeatId: string | null;
  resolvedAtUtc: string;
  updatedAtUtc: string;
}

export interface CampaignEventStateRequest {
  booleanValue: boolean | null;
  selectedOptionId: string | null;
  textValue: string | null;
  numericValue: number | null;
  sourceStoryBlockId: string | null;
  sourceStoryBeatId: string | null;
}

export interface OutcomeEffectModel {
  id: string;
  campaignId?: string;
  sourceType?: OutcomeSourceTypeValue;
  sourceId?: string;
  eventDefinitionId?: string;
  eventId: string | null;
  eventKey: string | null;
  operationType?: OutcomeEffectOperationValue;
  operation: OutcomeEffectOperationValue;
  booleanValue?: boolean | null;
  selectedOptionId?: string | null;
  textValue?: string | null;
  numericValue?: number | null;
  value: string | number | boolean | null;
  sortOrder: number;
}

export interface OutcomeEffectRequest {
  sourceType?: OutcomeSourceType;
  sourceId?: string;
  eventDefinitionId?: string;
  eventId: string | null;
  eventKey: string | null;
  operationType?: OutcomeEffectOperation;
  operation: OutcomeEffectOperation;
  booleanValue?: boolean | null;
  selectedOptionId?: string | null;
  textValue?: string | null;
  numericValue?: number | null;
  value: string | number | boolean | null;
  sortOrder: number;
}

export interface RuleConditionModel {
  id: string;
  eventDefinitionId: string | null;
  eventId?: string | null;
  eventKey: string | null;
  comparisonOperator: RuleComparisonOperatorValue;
  comparison?: RuleComparisonOperatorValue;
  booleanValue: boolean | null;
  expectedOptionId: string | null;
  textValue: string | null;
  numericValue: number | null;
  expectedValue?: string | number | boolean | null;
  sortOrder?: number;
}

export interface RuleConditionRequest {
  eventDefinitionId: string | null;
  comparisonOperator: RuleComparisonOperator;
  booleanValue: boolean | null;
  expectedOptionId: string | null;
  textValue: string | null;
  numericValue: number | null;
}

export interface CampaignRuleGroupModel {
  id: string;
  operator: RuleGroupOperatorValue;
  negate: boolean;
  clauses: RuleConditionModel[];
  groups: CampaignRuleGroupModel[];
  isCollapsed?: boolean;
}

export interface CampaignRuleGroupRequest {
  operator: RuleGroupOperator;
  negate: boolean;
  clauses: RuleConditionRequest[];
  groups: CampaignRuleGroupRequest[];
  isCollapsed?: boolean;
}

export interface ConditionalRuleRequest {
  effectType: ConditionalRuleEffectType;
  targetType: ConditionalTargetType;
  targetId: string;
  root: CampaignRuleGroupRequest | null;
}

export interface ConditionalRuleModel extends ConditionalRuleRequest {
  id: string;
  campaignId: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CampaignChoiceOptionModel {
  id: string;
  title: string;
  description: string | null;
  linkedStoryBeatId: string | null;
  outcomeEffects: OutcomeEffectModel[];
  sortOrder: number;
}

export interface CampaignChoiceOptionRequest {
  id?: string | null;
  title: string | null;
  description: string | null;
  linkedStoryBeatId: string | null;
  outcomeEffects: OutcomeEffectRequest[];
  sortOrder: number;
}

export interface CampaignChoiceModel {
  id: string;
  ownerType: string;
  ownerId: string;
  title: string;
  description: string | null;
  selectionMode: ChoiceSelectionModeValue;
  options: CampaignChoiceOptionModel[];
  conditionalRule: CampaignRuleGroupModel | null;
}

export interface CampaignChoiceRequest {
  title: string | null;
  description: string | null;
  selectionMode: ChoiceSelectionMode;
  options: CampaignChoiceOptionRequest[];
  conditionalRule: CampaignRuleGroupRequest | null;
}


export interface RuleEvaluationRequest {
  ruleId: string | null;
  targetType: ConditionalTargetType | null;
  targetId: string | null;
}

export interface RuleEvaluationResult {
  ruleId: string;
  isSatisfied: boolean;
  humanReadableExplanation: string;
}

export interface TargetAvailabilityResult {
  targetType: ConditionalTargetTypeValue;
  targetId: string;
  isAvailable: boolean;
  ruleResults: RuleEvaluationResult[];
}

export interface ApplyOutcomeRequest {
  sourceType: OutcomeSourceType;
  sourceId: string;
}

export interface ApplyOutcomeResult {
  changedEventStates: CampaignEventStateModel[];
}

export interface CampaignChoiceSelectionModel {
  id: string;
  campaignSessionId: number;
  campaignChoiceDefinitionId: string;
  campaignChoiceOptionId: string;
  selectedAtUtc: string;
}



