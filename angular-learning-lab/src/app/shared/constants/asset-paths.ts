const ASSET_ROOT = 'assets';

export const ASSET_FOLDERS = {
  root: ASSET_ROOT,
  images: `${ASSET_ROOT}/images`,
  skillImages: `${ASSET_ROOT}/images/skills`,
  icons: `${ASSET_ROOT}/icons`,
  videos: `${ASSET_ROOT}/videos`,
};

export const ASSET_PATHS = {
  images: {
    deadAsDisco: `${ASSET_FOLDERS.images}/disco_elysium_low_res.png`,
    gammelStrand: `${ASSET_FOLDERS.images}/Gammel-Strand.jfif`,
    kongensNytorv: `${ASSET_FOLDERS.images}/Kongens-Nytorv.jfif`,
    norrebro: `${ASSET_FOLDERS.images}/Norrebro.jfif`,
    osterbro: `${ASSET_FOLDERS.images}/Osterbro.jpg`,
    lundtofteparken: `${ASSET_FOLDERS.images}/Lundtofteparken.jpeg`,
    nyhavn: `${ASSET_FOLDERS.images}/Nyhavn.jpg`,
    profilePicture: `${ASSET_FOLDERS.images}/IMG_0855.jpeg`,
    skills: {
      logic: `${ASSET_FOLDERS.skillImages}/logic.webp`,
      motorics: `${ASSET_FOLDERS.skillImages}/motorics.webp`,
      physical: `${ASSET_FOLDERS.skillImages}/physical.webp`,
      psyche: `${ASSET_FOLDERS.skillImages}/psyche.png`,
    },
  },
  icons: {
    // Add icons here later
  },
  videos: {
    // Add videos here later
  },
};
