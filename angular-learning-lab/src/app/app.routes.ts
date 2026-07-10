import { Routes } from '@angular/router';

import { authGuard, masterRoleGuard } from './Infrastructure';
import { CharacterSheet } from './character-sheet/character-sheet';
import { Home } from './home/home';
import { Login } from './login/login';
import { CreateNewCampaign } from './my-campaigns/create-new-campaign/create-new-campaign';
import { MyCampaigns } from './my-campaigns/my-campaigns';
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
    path: 'my-campaigns',
    component: MyCampaigns,
    canActivate: [authGuard]
  },
  {
    path: 'my-campaigns/create-new-campaign',
    component: CreateNewCampaign,
    canActivate: [authGuard, masterRoleGuard]
  },
  {
    path: 'character-sheet',
    component: CharacterSheet,
    canActivate: [authGuard]
  }
];
