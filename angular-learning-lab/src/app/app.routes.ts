import { Routes } from '@angular/router';

import { authGuard, masterRoleGuard } from './Infrastructure';
import { Campaign } from './campaign/campaign';
import { CampaignHome } from './campaign/campaign-home/campaign-home';
import { CampaignMembers } from './campaign/campaign-members/campaign-members';
import { CampaignSettings } from './campaign/campaign-settings/campaign-settings';
import { CharacterSheet } from './character-sheet/character-sheet';
import { Home } from './home/home';
import { Login } from './login/login';
import { LogoutSuccess } from './logout-success/logout-success';
import { CreateNewCampaign } from './my-campaigns/create-new-campaign/create-new-campaign';
import { MyCampaignInvites } from './my-campaigns/invites/my-campaign-invites';
import { MyCampaigns } from './my-campaigns/my-campaigns';
import { Register } from './register/register';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full'
  },
  {
    path: 'home',
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
    path: 'logout-success',
    component: LogoutSuccess
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
    path: 'my-campaigns/invites',
    component: MyCampaignInvites,
    canActivate: [authGuard]
  },
  {
    path: 'character-sheet',
    component: CharacterSheet,
    canActivate: [authGuard]
  },
  {
    path: 'campaigns/:campaignId',
    component: Campaign,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        component: CampaignHome
      },
      {
        path: 'campaign-members',
        component: CampaignMembers
      },
      {
        path: 'campaign-settings',
        component: CampaignSettings,
        canActivate: [masterRoleGuard]
      }
    ]
  }
];
