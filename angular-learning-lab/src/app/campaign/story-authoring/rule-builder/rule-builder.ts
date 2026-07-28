import { NgTemplateOutlet } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  CampaignEventModel,
  CampaignEventType,
  CampaignRuleGroupRequest,
  RuleComparisonOperator,
  RuleConditionRequest,
  RuleGroupOperator,
} from '../../../Infrastructure';

@Component({
  selector: 'app-rule-builder',
  imports: [FormsModule, NgTemplateOutlet],
  templateUrl: './rule-builder.html',
  styleUrl: './rule-builder.css',
})
export class RuleBuilder implements OnChanges {
  @Input() rule: CampaignRuleGroupRequest | null = null;
  @Input() eventOptions: CampaignEventModel[] = [];
  @Output() ruleChange = new EventEmitter<CampaignRuleGroupRequest | null>();

  protected readonly editableRule = signal<CampaignRuleGroupRequest | null>(null);
  protected readonly groupOperators = [
    { value: RuleGroupOperator.And, label: 'AND' },
    { value: RuleGroupOperator.Or, label: 'OR' },
    { value: RuleGroupOperator.ExactlyOne, label: 'EXACTLY ONE' },
  ];
  protected readonly comparisons = [
    { value: RuleComparisonOperator.Equals, label: 'Equals' },
    { value: RuleComparisonOperator.NotEquals, label: 'Does not equal' },
    { value: RuleComparisonOperator.GreaterThan, label: 'Greater than' },
    { value: RuleComparisonOperator.GreaterThanOrEqual, label: 'Greater or equal' },
    { value: RuleComparisonOperator.LessThan, label: 'Less than' },
    { value: RuleComparisonOperator.LessThanOrEqual, label: 'Less or equal' },
    { value: RuleComparisonOperator.IsSet, label: 'Is set' },
    { value: RuleComparisonOperator.IsNotSet, label: 'Is not set' },
  ];
  private readonly equalityComparisons = this.comparisons.filter((comparison) => (
    comparison.value === RuleComparisonOperator.Equals ||
    comparison.value === RuleComparisonOperator.NotEquals
  ));

  ngOnChanges(changes: SimpleChanges): void {
    if ('rule' in changes) {
      this.editableRule.set(this.cloneRule(this.rule));
    }
  }

  protected createRule(): void {
    this.editableRule.set(this.createGroup());
    this.emitChange();
  }

  protected clearRule(): void {
    this.editableRule.set(null);
    this.ruleChange.emit(null);
  }

  protected addCondition(group: CampaignRuleGroupRequest): void {
    group.clauses = [
      ...group.clauses,
      {
        eventDefinitionId: null,
        comparisonOperator: RuleComparisonOperator.Equals,
        booleanValue: null,
        expectedOptionId: null,
        textValue: null,
        numericValue: null,
      },
    ];
    this.emitChange();
  }

  protected addGroup(group: CampaignRuleGroupRequest): void {
    group.groups = [...group.groups, this.createGroup()];
    this.emitChange();
  }

  protected removeCondition(group: CampaignRuleGroupRequest, index: number): void {
    group.clauses = group.clauses.filter((_, conditionIndex) => conditionIndex !== index);
    this.emitChange();
  }

  protected removeGroup(group: CampaignRuleGroupRequest, index: number): void {
    group.groups = group.groups.filter((_, groupIndex) => groupIndex !== index);
    this.emitChange();
  }

  protected setGroupOperator(group: CampaignRuleGroupRequest, operator: RuleGroupOperator): void {
    group.operator = operator;
    this.emitChange();
  }

  protected toggleGroup(group: CampaignRuleGroupRequest): void {
    group.isCollapsed = !group.isCollapsed;
    this.emitChange();
  }

  protected setConditionEvent(condition: RuleConditionRequest, eventId: string): void {
    const event = this.eventOptions.find((option) => option.id === eventId) ?? null;

    condition.eventDefinitionId = event?.id ?? null;
    condition.comparisonOperator = this.toCampaignEventType(event?.eventType) === CampaignEventType.SingleChoice &&
      !this.isEqualityComparison(condition.comparisonOperator)
      ? RuleComparisonOperator.Equals
      : condition.comparisonOperator;
    condition.booleanValue = null;
    condition.expectedOptionId = null;
    condition.textValue = null;
    condition.numericValue = null;
    this.emitChange();
  }

  protected setConditionPatch(
    condition: RuleConditionRequest,
    patch: Partial<RuleConditionRequest>,
  ): void {
    Object.assign(condition, patch);
    this.emitChange();
  }

  protected selectedEvent(condition: RuleConditionRequest): CampaignEventModel | null {
    return this.eventOptions.find((option) => option.id === condition.eventDefinitionId) ?? null;
  }

  protected isBooleanEvent(condition: RuleConditionRequest): boolean {
    return this.toCampaignEventType(this.selectedEvent(condition)?.eventType) === CampaignEventType.BooleanFlag;
  }

  protected isSingleChoiceEvent(condition: RuleConditionRequest): boolean {
    return this.toCampaignEventType(this.selectedEvent(condition)?.eventType) === CampaignEventType.SingleChoice;
  }

  protected isNumericEvent(condition: RuleConditionRequest): boolean {
    return this.toCampaignEventType(this.selectedEvent(condition)?.eventType) === CampaignEventType.NumericValue;
  }

  protected comparisonOptions(condition: RuleConditionRequest): { value: RuleComparisonOperator; label: string }[] {
    return this.isSingleChoiceEvent(condition) ? this.equalityComparisons : this.comparisons;
  }

  protected setComparisonOperator(condition: RuleConditionRequest, comparisonOperator: RuleComparisonOperator): void {
    this.setConditionPatch(condition, {
      comparisonOperator: this.isSingleChoiceEvent(condition) &&
        !this.isEqualityComparison(comparisonOperator)
        ? RuleComparisonOperator.Equals
        : comparisonOperator,
    });
  }

  protected setBooleanValue(condition: RuleConditionRequest, value: string): void {
    this.setConditionPatch(condition, {
      booleanValue: value === '' ? null : value === 'true',
      expectedOptionId: null,
      textValue: null,
      numericValue: null,
    });
  }

  protected setOptionValue(condition: RuleConditionRequest, value: string | null): void {
    this.setConditionPatch(condition, {
      booleanValue: null,
      expectedOptionId: value || null,
      textValue: null,
      numericValue: null,
    });
  }

  protected setTextValue(condition: RuleConditionRequest, value: string): void {
    this.setConditionPatch(condition, {
      booleanValue: null,
      expectedOptionId: null,
      textValue: value || null,
      numericValue: null,
    });
  }

  protected setNumericValue(condition: RuleConditionRequest, value: string): void {
    const numericValue = Number(value);

    this.setConditionPatch(condition, {
      booleanValue: null,
      expectedOptionId: null,
      textValue: null,
      numericValue: Number.isFinite(numericValue) ? numericValue : null,
    });
  }

  protected indent(depth: number): string {
    return '0';
  }

  protected trackIndex(index: number): number {
    return index;
  }

  private createGroup(): CampaignRuleGroupRequest {
    return {
      operator: RuleGroupOperator.And,
      negate: false,
      clauses: [],
      groups: [],
      isCollapsed: false,
    };
  }

  private cloneRule(rule: CampaignRuleGroupRequest | null): CampaignRuleGroupRequest | null {
    return rule ? this.cloneGroup(rule) : null;
  }

  private cloneGroup(group: CampaignRuleGroupRequest): CampaignRuleGroupRequest {
    return {
      operator: group.operator,
      negate: group.negate ?? false,
      isCollapsed: group.isCollapsed ?? false,
      clauses: group.clauses.map((condition) => ({ ...condition })),
      groups: group.groups.map((childGroup) => this.cloneGroup(childGroup)),
    };
  }

  private emitChange(): void {
    this.ruleChange.emit(this.cloneRule(this.editableRule()));
  }

  private isEqualityComparison(comparisonOperator: RuleComparisonOperator): boolean {
    return comparisonOperator === RuleComparisonOperator.Equals ||
      comparisonOperator === RuleComparisonOperator.NotEquals;
  }

  private toCampaignEventType(value: CampaignEventModel['eventType'] | undefined): CampaignEventType | null {
    if (value === undefined || value === null) {
      return null;
    }

    if (typeof value === 'number') {
      return value as CampaignEventType;
    }

    const parsedValue = Number(value);

    if (Number.isFinite(parsedValue)) {
      return parsedValue as CampaignEventType;
    }

    return CampaignEventType[value as keyof typeof CampaignEventType] ?? null;
  }
}
