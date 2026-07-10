export interface UpdateCharacterSheetRequest {
  background: string | null;
  information: string | null;
  firstName: string;
  lastName: string;
  characterClass: string;
  nationality: string | null;
  height: number | null;
  weight: number | null;
  actions: CharacterActionRequest[];
  traits: string[];
  equipment: string[];
  logicRating: number;
  psycheRating: number;
  physicalRating: number;
  motoricsRating: number;
}

export interface CharacterActionRequest {
  actionType: 0 | 1 | 2 | 3;
  title: string | null;
  description: string | null;
}
