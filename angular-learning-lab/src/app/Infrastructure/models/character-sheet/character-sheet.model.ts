export interface CharacterSheetModel {
  portraitUrl: string;
  characterClass: string;
  background: string;
  information: string;
  firstName: string;
  lastName: string;
  nationality: string;
  height: string;
  weight: string;
  actionsAndBonusActions: string[];
  traits: string[];
  equipment: string[];
  ratings: CharacterRatings;
}

export interface CharacterRatings {
  logic: number;
  psyche: number;
  physical: number;
  motorics: number;
}
