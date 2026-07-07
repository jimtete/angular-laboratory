import { ASSET_PATHS } from '../constants/asset-paths';

export interface CarouselCardIconData {
  id: string;
  label: string;
  lucideIcon?: 'lightbulb' | 'brain-circuit' | 'theater';
  source?: string;
  alt?: string;
}

export interface CarouselCardData {
  id: number;
  title: string;
  backgroundImage: string;
  icons: CarouselCardIconData[];
}

const DEFAULT_CARD_ICONS: CarouselCardIconData[] = [
  {
    id: 'concept',
    label: 'Concept',
    lucideIcon: 'lightbulb',
  },
  {
    id: 'logic',
    label: 'Logic',
    lucideIcon: 'brain-circuit',
  },
  {
    id: 'drama',
    label: 'Drama',
    lucideIcon: 'theater',
  },
];

export const CAROUSEL_CARDS: CarouselCardData[] = [
  {
    id: 1,
    title: 'Kongens Nytorv',
    backgroundImage: ASSET_PATHS.images.kongensNytorv,
    icons: DEFAULT_CARD_ICONS,
  },
  {
    id: 2,
    title: 'Gammel Strand',
    backgroundImage: ASSET_PATHS.images.gammelStrand,
    icons: DEFAULT_CARD_ICONS,
  },
  {
    id: 3,
    title: 'Norrebro',
    backgroundImage: ASSET_PATHS.images.norrebro,
    icons: DEFAULT_CARD_ICONS,
  },
  {
    id: 4,
    title: 'Osterbro',
    backgroundImage: ASSET_PATHS.images.osterbro,
    icons: DEFAULT_CARD_ICONS,
  },
  {
    id: 5,
    title: 'Lundtofteparken',
    backgroundImage: ASSET_PATHS.images.lundtofteparken,
    icons: DEFAULT_CARD_ICONS,
  },
  {
    id: 6,
    title: 'Nyhavn',
    backgroundImage: ASSET_PATHS.images.nyhavn,
    icons: DEFAULT_CARD_ICONS,
  },
];
