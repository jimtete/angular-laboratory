import { Routes } from '@angular/router';

import { CharacterSheet } from './character-sheet/character-sheet';
import { Home } from './home/home';

export const routes: Routes = [
  {
    path: '',
    component: Home
  },
  {
    path: 'character-sheet',
    component: CharacterSheet
  }
];
