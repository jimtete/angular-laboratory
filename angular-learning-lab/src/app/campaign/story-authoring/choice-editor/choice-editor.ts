import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  CampaignChoiceOptionRequest,
  CampaignChoiceRequest,
  CampaignEventModel,
  ChoiceSelectionMode,
} from '../../../Infrastructure';
import { OutcomeEffectEditor } from '../outcome-effect-editor/outcome-effect-editor';
import { RuleBuilder } from '../rule-builder/rule-builder';

@Component({
  selector: 'app-choice-editor',
  imports: [FormsModule, OutcomeEffectEditor, RuleBuilder],
  templateUrl: './choice-editor.html',
  styleUrl: './choice-editor.css',
})
export class ChoiceEditor implements OnChanges {
  @Input() choice: CampaignChoiceRequest | null = null;
  @Input() eventOptions: CampaignEventModel[] = [];
  @Output() choiceChange = new EventEmitter<CampaignChoiceRequest>();

  protected readonly editableChoice = signal<CampaignChoiceRequest>(this.createChoice());
  protected readonly selectionModes = [
    { value: ChoiceSelectionMode.Single, label: 'Single' },
    { value: ChoiceSelectionMode.Multiple, label: 'Multiple' },
    { value: ChoiceSelectionMode.ExactlyOne, label: 'Exactly One' },
  ];

  ngOnChanges(changes: SimpleChanges): void {
    if ('choice' in changes) {
      this.editableChoice.set(this.cloneChoice(this.choice ?? this.createChoice()));
    }
  }

  protected updateChoice(patch: Partial<CampaignChoiceRequest>): void {
    this.editableChoice.update((choice) => ({ ...choice, ...patch }));
    this.emitChange();
  }

  protected addOption(): void {
    this.editableChoice.update((choice) => ({
      ...choice,
      options: [...choice.options, this.createOption(choice.options.length)],
    }));
    this.emitChange();
  }

  protected removeOption(index: number): void {
    this.editableChoice.update((choice) => ({
      ...choice,
      options: choice.options
        .filter((_, optionIndex) => optionIndex !== index)
        .map((option, sortOrder) => ({ ...option, sortOrder })),
    }));
    this.emitChange();
  }

  protected updateOption(index: number, patch: Partial<CampaignChoiceOptionRequest>): void {
    this.editableChoice.update((choice) => ({
      ...choice,
      options: choice.options.map((option, optionIndex) => (
        optionIndex === index ? { ...option, ...patch } : option
      )),
    }));
    this.emitChange();
  }

  protected trackOption(index: number): number {
    return index;
  }

  private createChoice(): CampaignChoiceRequest {
    return {
      title: null,
      description: null,
      selectionMode: ChoiceSelectionMode.Single,
      options: [this.createOption(0), this.createOption(1)],
      conditionalRule: null,
    };
  }

  private createOption(sortOrder: number): CampaignChoiceOptionRequest {
    return {
      id: null,
      title: null,
      description: null,
      linkedStoryBeatId: null,
      outcomeEffects: [],
      sortOrder,
    };
  }

  private cloneChoice(choice: CampaignChoiceRequest): CampaignChoiceRequest {
    return {
      ...choice,
      options: choice.options.map((option) => ({
        ...option,
        outcomeEffects: option.outcomeEffects.map((effect) => ({ ...effect })),
      })),
      conditionalRule: choice.conditionalRule
        ? {
          ...choice.conditionalRule,
          clauses: choice.conditionalRule.clauses.map((condition) => ({ ...condition })),
          groups: choice.conditionalRule.groups.map((group) => ({ ...group })),
        }
        : null,
    };
  }

  private emitChange(): void {
    this.choiceChange.emit(this.cloneChoice(this.editableChoice()));
  }
}
