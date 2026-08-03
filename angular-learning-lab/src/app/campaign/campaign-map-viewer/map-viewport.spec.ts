import {
  MAP_VIEWPORT_MAX_SCALE,
  clampMapViewport,
  createInitialMapViewport,
  getMapCoordinateAtViewportPoint,
  getMinimumMapScale,
  panMapViewport,
  zoomMapViewport,
} from './map-viewport';

describe('map viewport utilities', () => {
  it('uses a cover-style minimum scale so the map fills the viewport', () => {
    expect(getMinimumMapScale(
      { width: 800, height: 600 },
      { width: 1600, height: 1200 },
    )).toBe(0.5);

    expect(getMinimumMapScale(
      { width: 800, height: 600 },
      { width: 400, height: 1200 },
    )).toBe(2);

    expect(getMinimumMapScale(
      { width: 1600, height: 900 },
      { width: 400, height: 1200 },
    )).toBe(4);
  });

  it('centres a portrait map vertically when cover-scaled into a landscape viewer', () => {
    const viewport = createInitialMapViewport(
      { width: 1600, height: 900 },
      { width: 400, height: 1200 },
    );

    expect(viewport.scale).toBe(4);
    expect(viewport.translateX).toBe(0);
    expect(viewport.translateY).toBe(-1950);
  });

  it('allows the minimum scale to exceed the normal maximum when needed to fill the viewport', () => {
    expect(getMinimumMapScale(
      { width: 800, height: 600 },
      { width: 1000, height: 100 },
    )).toBe(6);
  });

  it('clamps to the cover minimum when the requested scale is too small', () => {
    const viewport = clampMapViewport(
      { scale: 1, translateX: -100, translateY: -100 },
      { width: 800, height: 600 },
      { width: 1000, height: 100 },
    );

    expect(viewport.scale).toBe(6);
    expect(viewport.translateY).toBe(0);
  });

  it('clamps panning so no empty area can be revealed', () => {
    const viewport = panMapViewport(
      { scale: 1, translateX: -200, translateY: -150 },
      { width: 800, height: 600 },
      { width: 1600, height: 1200 },
      -2000,
      2000,
    );

    expect(viewport.translateX).toBe(-800);
    expect(viewport.translateY).toBe(0);
  });

  it('preserves the map coordinate beneath the zoom focal point', () => {
    const container = { width: 800, height: 600 };
    const map = { width: 1600, height: 1200 };
    const focus = { x: 250, y: 190 };
    const viewport = createInitialMapViewport(container, map);
    const before = getMapCoordinateAtViewportPoint(viewport, focus);
    const zoomed = zoomMapViewport(viewport, container, map, focus, 1.5);
    const after = getMapCoordinateAtViewportPoint(zoomed, focus);

    expect(after.x).toBeCloseTo(before.x, 5);
    expect(after.y).toBeCloseTo(before.y, 5);
  });

  it('does not zoom beyond the configured maximum', () => {
    const viewport = zoomMapViewport(
      { scale: MAP_VIEWPORT_MAX_SCALE, translateX: -100, translateY: -100 },
      { width: 800, height: 600 },
      { width: 1600, height: 1200 },
      { x: 400, y: 300 },
      2,
    );

    expect(viewport.scale).toBe(MAP_VIEWPORT_MAX_SCALE);
  });
});
