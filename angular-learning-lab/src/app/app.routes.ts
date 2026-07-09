import { Routes } from '@angular/router';

import { authGuard } from './Infrastructure';
import { CharacterSheet } from './character-sheet/character-sheet';
import { Home } from './home/home';
import { Login } from './login/login';
import { Register } from './register/register';

export const routes: Routes = [
  {
    path: '',
    component: Home
  },
  {
    path: 'login',
    component: Login
  },
  {
    path: 'register',
    component: Register
  },
  {
    path: 'character-sheet',
    component: CharacterSheet,
    canActivate: [authGuard]
  }
];
