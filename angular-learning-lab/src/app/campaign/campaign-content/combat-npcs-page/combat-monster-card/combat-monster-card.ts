import { Component, input, output } from '@angular/core';

export interface CombatMonsterCardModel {
  monsterId: number;
  name: string;
  race: string;
  className: string;
  stats: readonly number[];
}

@Component({
  selector: 'app-combat-monster-card',
  templateUrl: './combat-monster-card.html',
  styleUrl: './combat-monster-card.css',
})
export class CombatMonsterCard {
  readonly monster = input.required<CombatMonsterCardModel>();
  readonly selected = output<CombatMonsterCardModel>();

  protected readonly statLabels = ['STR', 'DEX', 'CON', 'INT', 'WIS', 'CHA'];

  protected selectMonster(): void {
    this.selected.emit(this.monster());
  }
}
