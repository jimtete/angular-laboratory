import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  CampaignEventModel,
  CampaignEventType,
  OutcomeEffectOperation,
  OutcomeEffectRequest,
} from '../../../Infrastructure';

@Component({
  selector: 'app-outcome-effect-editor',
  imports: [FormsModule],
  templateUrl: './outcome-effect-editor.html',
  styleUrl: './outcome-effect-editor.css',
})
export class OutcomeEffectEditor {
  @Input() effects: OutcomeEffectRequest[] = [];
  @Input() eventOptions: CampaignEventModel[] = [];
  @Output() effectsChange = new EventEmitter<OutcomeEffectRequest[]>();

  private readonly allOperations = [
    { value: OutcomeEffectOperation.Set, label: 'Set' },
    { value: OutcomeEffectOperation.Increment, label: 'Increment' },
    { value: OutcomeEffectOperation.Decrement, label: 'Decrement' },
    { value: OutcomeEffectOperation.Clear, label: 'Clear' },
  ];
  private readonly nonNumericOperations = this.allOperations.filter((operation) => (
    operation.value === OutcomeEffectOperation.Set ||
    operation.value === OutcomeEffectOperation.Clear
  ));

  protected addEffect(): void {
    this.effects = [
      ...this.effects,
      {
        eventDefinitionId: undefined,
        eventId: null,
        eventKey: null,
        operationType: OutcomeEffectOperation.Set,
        operation: OutcomeEffectOperation.Set,
        booleanValue: null,
        selectedOptionId: null,
        textValue: null,
        numericValue: null,
        value: null,
        sortOrder: this.effects.length + 1,
      },
    ];
    this.emitChange();
  }

  protected removeEffect(index: number): void {
    this.effects = this.effects
      .filter((_, effectIndex) => effectIndex !== index)
      .map((effect, sortOrder) => ({ ...effect, sortOrder: sortOrder + 1 }));
    this.emitChange();
  }

  protected setEvent(index: number, eventId: string): void {
    const event = this.eventOptions.find((option) => option.id === eventId) ?? null;
    const operation = this.toCampaignEventType(event?.eventType) === CampaignEventType.NumericValue
      ? this.selectedOperation(this.effects[index])
      : this.toNonNumericOperation(this.selectedOperation(this.effects[index]));

    this.updateEffect(index, {
      eventDefinitionId: event?.id ?? undefined,
      eventId: event?.id ?? null,
      eventKey: event?.key ?? null,
      operationType: operation,
      operation,
      booleanValue: null,
      selectedOptionId: null,
      textValue: null,
      numericValue: null,
      });
  }

  protected updateEffect(index: number, patch: Partial<OutcomeEffectRequest>): void {
    this.effects = this.effects.map((effect, effectIndex) => (
      effectIndex === index ? { ...effect, ...patch } : effect
    ));
    this.emitChange();
  }

  protected setOperation(index: number, operation: OutcomeEffectOperation): void {
    const selectedEvent = this.selectedEvent(this.effects[index]);
    const normalizedOperation = this.toCampaignEventType(selectedEvent?.eventType) === CampaignEventType.NumericValue
      ? operation
      : this.toNonNumericOperation(operation);

    this.updateEffect(index, {
      operation: normalizedOperation,
      operationType: normalizedOperation,
      booleanValue: normalizedOperation === OutcomeEffectOperation.Clear ? null : this.effects[index]?.booleanValue ?? null,
      selectedOptionId: normalizedOperation === OutcomeEffectOperation.Clear ? null : this.effects[index]?.selectedOptionId ?? null,
      textValue: normalizedOperation === OutcomeEffectOperation.Clear ? null : this.effects[index]?.textValue ?? null,
      numericValue: normalizedOperation === OutcomeEffectOperation.Clear ? null : this.effects[index]?.numericValue ?? null,
      value: normalizedOperation === OutcomeEffectOperation.Clear ? null : this.effects[index]?.value ?? null,
    });
  }

  protected selectedEvent(effect: OutcomeEffectRequest): CampaignEventModel | null {
    return this.eventOptions.find((option) => option.id === (effect.eventDefinitionId ?? effect.eventId)) ?? null;
  }

  protected selectedOperation(effect: OutcomeEffectRequest): OutcomeEffectOperation {
    return this.toOutcomeEffectOperation(effect.operationType ?? effect.operation);
  }

  protected isClearOperation(effect: OutcomeEffectRequest): boolean {
    return this.selectedOperation(effect) === OutcomeEffectOperation.Clear;
  }

  protected isBooleanEvent(effect: OutcomeEffectRequest): boolean {
    return this.toCampaignEventType(this.selectedEvent(effect)?.eventType) === CampaignEventType.BooleanFlag;
  }

  protected isSingleChoiceEvent(effect: OutcomeEffectRequest): boolean {
    return this.toCampaignEventType(this.selectedEvent(effect)?.eventType) === CampaignEventType.SingleChoice;
  }

  protected isNumericEvent(effect: OutcomeEffectRequest): boolean {
    return this.toCampaignEventType(this.selectedEvent(effect)?.eventType) === CampaignEventType.NumericValue;
  }

  protected operationOptions(effect: OutcomeEffectRequest): { value: OutcomeEffectOperation; label: string }[] {
    return this.isNumericEvent(effect) ? this.allOperations : this.nonNumericOperations;
  }

  protected setBooleanValue(index: number, value: string): void {
    this.updateEffect(index, {
      booleanValue: value === '' ? null : value === 'true',
      selectedOptionId: null,
      textValue: null,
      numericValue: null,
      value: value === '' ? null : value === 'true',
    });
  }

  protected setOptionValue(index: number, value: string | null): void {
    this.updateEffect(index, {
      booleanValue: null,
      selectedOptionId: value || null,
      textValue: null,
      numericValue: null,
      value: value || null,
    });
  }

  protected setTextValue(index: number, value: string): void {
    this.updateEffect(index, {
      booleanValue: null,
      selectedOptionId: null,
      textValue: value || null,
      numericValue: null,
      value: value || null,
    });
  }

  protected setNumericValue(index: number, value: string): void {
    const numericValue = Number(value);

    this.updateEffect(index, {
      booleanValue: null,
      selectedOptionId: null,
      textValue: null,
      numericValue: Number.isFinite(numericValue) ? numericValue : null,
      value: Number.isFinite(numericValue) ? numericValue : null,
    });
  }

  protected trackEffect(index: number): number {
    return index;
  }

  private emitChange(): void {
    this.effectsChange.emit(this.effects.map((effect) => ({ ...effect })));
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

  private toNonNumericOperation(operation: OutcomeEffectOperation): OutcomeEffectOperation {
    return operation === OutcomeEffectOperation.Clear
      ? OutcomeEffectOperation.Clear
      : OutcomeEffectOperation.Set;
  }

  private toOutcomeEffectOperation(
    operation: OutcomeEffectOperation | keyof typeof OutcomeEffectOperation | string | number,
  ): OutcomeEffectOperation {
    if (typeof operation === 'number') {
      return operation as OutcomeEffectOperation;
    }

    const parsedOperation = Number(operation);

    if (Number.isFinite(parsedOperation)) {
      return parsedOperation as OutcomeEffectOperation;
    }

    return OutcomeEffectOperation[operation as keyof typeof OutcomeEffectOperation] ?? OutcomeEffectOperation.Set;
  }
}
