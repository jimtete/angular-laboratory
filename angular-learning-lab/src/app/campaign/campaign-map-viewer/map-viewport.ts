export interface MapViewportState {
  scale: number;
  translateX: number;
  translateY: number;
}

export interface MapViewportSize {
  width: number;
  height: number;
}

export interface MapViewportPoint {
  x: number;
  y: number;
}

export const MAP_VIEWPORT_MAX_SCALE = 4;
export const MAP_VIEWPORT_ZOOM_STEP = 1.2;

const DEFAULT_VIEWPORT_STATE: MapViewportState = {
  scale: 1,
  translateX: 0,
  translateY: 0,
};

export function getMinimumMapScale(
  containerSize: MapViewportSize,
  mapSize: MapViewportSize,
): number {
  if (!hasUsableSize(containerSize) || !hasUsableSize(mapSize)) {
    return 1;
  }

  return Math.max(
    containerSize.width / mapSize.width,
    containerSize.height / mapSize.height,
  );
}

export function createInitialMapViewport(
  containerSize: MapViewportSize,
  mapSize: MapViewportSize,
): MapViewportState {
  const scale = getMinimumMapScale(containerSize, mapSize);

  return clampMapViewport(
    {
      ...DEFAULT_VIEWPORT_STATE,
      scale,
      translateX: (containerSize.width - (mapSize.width * scale)) / 2,
      translateY: (containerSize.height - (mapSize.height * scale)) / 2,
    },
    containerSize,
    mapSize,
  );
}

export function clampMapViewport(
  viewport: MapViewportState,
  containerSize: MapViewportSize,
  mapSize: MapViewportSize,
): MapViewportState {
  if (!hasUsableSize(containerSize) || !hasUsableSize(mapSize)) {
    return DEFAULT_VIEWPORT_STATE;
  }

  const minimumScale = getMinimumMapScale(containerSize, mapSize);
  const scale = clamp(viewport.scale, minimumScale, Math.max(MAP_VIEWPORT_MAX_SCALE, minimumScale));
  const scaledWidth = mapSize.width * scale;
  const scaledHeight = mapSize.height * scale;

  return {
    scale,
    translateX: clampAxis(viewport.translateX, containerSize.width, scaledWidth),
    translateY: clampAxis(viewport.translateY, containerSize.height, scaledHeight),
  };
}

export function zoomMapViewport(
  viewport: MapViewportState,
  containerSize: MapViewportSize,
  mapSize: MapViewportSize,
  focusPoint: MapViewportPoint,
  scaleFactor: number,
): MapViewportState {
  const nextScale = clamp(
    viewport.scale * scaleFactor,
    getMinimumMapScale(containerSize, mapSize),
    Math.max(MAP_VIEWPORT_MAX_SCALE, getMinimumMapScale(containerSize, mapSize)),
  );

  return zoomMapViewportToScale(
    viewport,
    containerSize,
    mapSize,
    focusPoint,
    nextScale,
  );
}

export function zoomMapViewportToScale(
  viewport: MapViewportState,
  containerSize: MapViewportSize,
  mapSize: MapViewportSize,
  focusPoint: MapViewportPoint,
  nextScale: number,
): MapViewportState {
  if (!hasUsableSize(containerSize) || !hasUsableSize(mapSize) || viewport.scale <= 0) {
    return createInitialMapViewport(containerSize, mapSize);
  }

  const mapFocusX = (focusPoint.x - viewport.translateX) / viewport.scale;
  const mapFocusY = (focusPoint.y - viewport.translateY) / viewport.scale;

  return clampMapViewport(
    {
      scale: nextScale,
      translateX: focusPoint.x - (mapFocusX * nextScale),
      translateY: focusPoint.y - (mapFocusY * nextScale),
    },
    containerSize,
    mapSize,
  );
}

export function panMapViewport(
  viewport: MapViewportState,
  containerSize: MapViewportSize,
  mapSize: MapViewportSize,
  deltaX: number,
  deltaY: number,
): MapViewportState {
  return clampMapViewport(
    {
      ...viewport,
      translateX: viewport.translateX + deltaX,
      translateY: viewport.translateY + deltaY,
    },
    containerSize,
    mapSize,
  );
}

export function getMapCoordinateAtViewportPoint(
  viewport: MapViewportState,
  point: MapViewportPoint,
): MapViewportPoint {
  return {
    x: (point.x - viewport.translateX) / viewport.scale,
    y: (point.y - viewport.translateY) / viewport.scale,
  };
}

function clampAxis(translate: number, containerLength: number, scaledMapLength: number): number {
  if (scaledMapLength <= containerLength) {
    return (containerLength - scaledMapLength) / 2;
  }

  return clamp(translate, containerLength - scaledMapLength, 0);
}

function clamp(value: number, minimum: number, maximum: number): number {
  return Math.max(minimum, Math.min(maximum, value));
}

function hasUsableSize(size: MapViewportSize): boolean {
  return size.width > 0 && size.height > 0;
}
