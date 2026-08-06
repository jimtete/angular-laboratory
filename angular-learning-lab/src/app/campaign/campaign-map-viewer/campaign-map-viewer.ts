import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, computed, inject, signal, viewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LucideArrowLeft, LucideMap } from '@lucide/angular';
import { Subscription } from 'rxjs';

import {
  API_BASE_URL,
  ApiError,
  BrowserCacheService,
  CampaignApiService,
  CampaignMapCategory,
  CampaignMapModel,
  CampaignStoreModel,
  CreateMapPinConnectionRequest,
  CreateMapPinRequest,
  MapPinConnectionDistanceUnit,
  MapPinConnectionModel,
  MapPinDetailsModel,
  MapPinTargetType,
  StoryBlockModel,
  UpdateMapPinRequest,
} from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';
import {
  MAP_VIEWPORT_ZOOM_STEP,
  MapViewportPoint,
  MapViewportSize,
  MapViewportState,
  clampMapViewport,
  createInitialMapViewport,
  getMapCoordinateAtViewportPoint,
  panMapViewport,
  zoomMapViewport,
  zoomMapViewportToScale,
} from './map-viewport';

interface MapPinTool {
  label: string;
  className: string;
  targetType: MapPinTargetType | null;
}

interface MapViewportCacheEntry {
  scale: number;
  translateX: number;
  translateY: number;
}

interface PlaceholderPinFormValues {
  title: string;
  description: string;
  targetMapId: number | null;
  targetStoreId: number | null;
  targetStoryBlockId: string | null;
}

interface PlaceholderPinFormErrors {
  title?: string;
  targetMapId?: string;
  targetStoreId?: string;
  targetStoryBlockId?: string;
}

interface ConnectionFormValues {
  distanceValue: number | null;
  distanceUnit: MapPinConnectionDistanceUnit | null;
}

interface ConnectionFormErrors {
  distanceValue?: string;
}

interface ImageNaturalSize {
  width: number;
  height: number;
}

interface DraggingPinState {
  pinId: number;
  pointerId: number;
}

interface DraggingPinToolState {
  pinTool: MapPinTool;
  pointerId: number;
  startX: number;
  startY: number;
  x: number;
  y: number;
  isDragging: boolean;
  isOverMap: boolean;
}

interface MapPanState {
  pointerId: number;
  lastX: number;
  lastY: number;
}

interface MapTouchPoint {
  x: number;
  y: number;
}

interface MapPinchState {
  initialDistance: number;
  initialViewport: MapViewportState;
  initialCenter: MapViewportPoint;
}

interface MovingPinState {
  pinId: number;
  originalX: number;
  originalY: number;
  x: number;
  y: number;
  pointerId: number | null;
}

interface PinContextMenuState {
  pinId: number;
  x: number;
  y: number;
}

interface PendingMapNavigationState {
  targetMapId: number;
  targetMapName: string;
}

interface ConnectionTooltipState {
  connectionId: number;
  label: string;
  x: number;
  y: number;
}

type PlaceholderPinFormMode = 'create' | 'edit';

const MAP_PIN_TOOLS: MapPinTool[] = [
  { label: 'Placeholder', className: 'placeholder', targetType: MapPinTargetType.Placeholder },
  { label: 'Players Position', className: 'players-position', targetType: MapPinTargetType.PlayersPosition },
  { label: 'Store', className: 'store', targetType: MapPinTargetType.Store },
  { label: 'Another Map', className: 'map-link', targetType: MapPinTargetType.Map },
  { label: 'Story Block', className: 'story-block', targetType: MapPinTargetType.StoryBlock },
];
const MAP_VIEWPORT_SAVE_DEBOUNCE_MS = 180;

@Component({
  selector: 'app-campaign-map-viewer',
  imports: [LucideArrowLeft, LucideMap],
  templateUrl: './campaign-map-viewer.html',
  styleUrl: './campaign-map-viewer.css',
})
export class CampaignMapViewer implements OnInit, AfterViewInit, OnDestroy {
  private readonly placeholderPinForm = viewChild<ElementRef<HTMLFormElement>>('placeholderPinForm');
  private readonly mapStage = viewChild<ElementRef<HTMLDivElement>>('mapStage');
  private readonly mapScroll = viewChild<ElementRef<HTMLDivElement>>('mapScroll');
  private readonly mapImageLayer = viewChild<ElementRef<HTMLDivElement>>('mapImageLayer');
  private readonly mapImage = viewChild<ElementRef<HTMLImageElement>>('mapImage');
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly browserCache = inject(BrowserCacheService);
  private readonly modalHelper = inject(ModalHelper);
  private readonly apiBaseUrl = inject(API_BASE_URL).replace(/\/$/, '');
  private saveViewportTimer: ReturnType<typeof setTimeout> | undefined;
  private restoreUnlockTimer: ReturnType<typeof setTimeout> | undefined;
  private routeParamSubscription: Subscription | undefined;
  private mapResizeObserver: ResizeObserver | undefined;
  private mapViewportAnimationFrame: number | undefined;
  private hasAttemptedViewportRestore = false;
  private isRestoringViewport = false;
  private shouldSuppressPinToolClick = false;
  private mapTouchPoints = new Map<number, MapTouchPoint>();
  private readonly documentPinToolMoveListener = (event: PointerEvent) => this.dragPinTool(event);
  private readonly documentPinToolUpListener = (event: PointerEvent) => this.finishPinToolDrag(event);
  private readonly documentPinToolCancelListener = (event: PointerEvent) => this.cancelPinToolDrag(event);
  private readonly routeMapId = signal<number | null>(this.getRouteMapId());
  private readonly mapPan = signal<MapPanState | null>(null);
  private readonly mapPinch = signal<MapPinchState | null>(null);

  protected readonly maps = signal<CampaignMapModel[]>([]);
  protected readonly stores = signal<CampaignStoreModel[]>([]);
  protected readonly storyBlocks = signal<StoryBlockModel[]>([]);
  protected readonly mapPins = signal<MapPinDetailsModel[]>([]);
  protected readonly mapPinConnections = signal<MapPinConnectionModel[]>([]);
  protected readonly pinTools = MAP_PIN_TOOLS;
  protected readonly isLoadingMap = signal(false);
  protected readonly isLoadingPins = signal(false);
  protected readonly isPlaceholderPinFormOpen = signal(false);
  protected readonly isCreatingPlaceholderPin = signal(false);
  protected readonly placeholderPinValidationErrors = signal<PlaceholderPinFormErrors>({});
  protected readonly placeholderPinFormMode = signal<PlaceholderPinFormMode>('create');
  protected readonly placeholderPinFormTargetType = signal<MapPinTargetType>(MapPinTargetType.Placeholder);
  protected readonly editingPinId = signal<number | null>(null);
  protected readonly pinContextMenu = signal<PinContextMenuState | null>(null);
  protected readonly movingPin = signal<MovingPinState | null>(null);
  protected readonly isSavingMovedPin = signal(false);
  protected readonly pendingMapNavigation = signal<PendingMapNavigationState | null>(null);
  protected readonly deletingPinId = signal<number | null>(null);
  protected readonly imageNaturalSize = signal<ImageNaturalSize | null>(null);
  protected readonly draggingPin = signal<DraggingPinState | null>(null);
  protected readonly draggingPinTool = signal<DraggingPinToolState | null>(null);
  protected readonly pendingPlaceholderPinCoordinates = signal<{ x: number; y: number } | null>(null);
  protected readonly selectedPinId = signal<number | null>(null);
  protected readonly isConnectionModeActive = signal(false);
  protected readonly connectionStartPinId = signal<number | null>(null);
  protected readonly pendingConnectionPinIds = signal<{ pinAId: number; pinBId: number } | null>(null);
  protected readonly editingConnectionId = signal<number | null>(null);
  protected readonly isConnectionFormOpen = signal(false);
  protected readonly isCreatingConnection = signal(false);
  protected readonly connectionTooltip = signal<ConnectionTooltipState | null>(null);
  protected readonly connectionValidationErrors = signal<ConnectionFormErrors>({});
  protected readonly mapViewport = signal<MapViewportState>({
    scale: 1,
    translateX: 0,
    translateY: 0,
  });
  protected readonly connectionDistanceUnits = [
    { unit: MapPinConnectionDistanceUnit.Minutes, label: 'Minutes' },
    { unit: MapPinConnectionDistanceUnit.Hours, label: 'Hours' },
    { unit: MapPinConnectionDistanceUnit.Days, label: 'Days' },
    { unit: MapPinConnectionDistanceUnit.Weeks, label: 'Weeks' },
  ];
  protected readonly campaignId = computed(() => {
    return this.route.parent?.snapshot.paramMap.get('campaignId') ?? null;
  });
  protected readonly mapId = computed(() => {
    return this.routeMapId();
  });
  protected readonly selectedMap = computed(() => {
    const mapId = this.mapId();

    return mapId === null
      ? null
      : this.maps().find((map) => map.id === mapId) ?? null;
  });
  protected readonly mapLinkTargetMaps = computed(() => {
    const selectedMap = this.selectedMap();

    if (!selectedMap) {
      return [];
    }

    const selectedMapCategory = this.getCategoryValue(selectedMap.category);

    return this.maps().filter((map) => (
      map.id !== selectedMap.id &&
      this.getCategoryValue(map.category) >= selectedMapCategory
    ));
  });

  ngOnInit(): void {
    this.hasAttemptedViewportRestore = false;
    this.routeParamSubscription = this.route.paramMap.subscribe((paramMap) => {
      const nextMapId = this.normalizeRouteMapId(paramMap.get('mapId'));

      if (nextMapId === this.routeMapId()) {
        return;
      }

      this.flushPendingMapViewportSave();
      this.routeMapId.set(nextMapId);
      this.resetMapViewerForRouteChange();
      this.loadMap(true);
    });
    this.loadMap();
    this.loadStores();
    this.loadStoryBlocks();
  }

  ngAfterViewInit(): void {
    const mapScroll = this.mapScroll()?.nativeElement;

    if (mapScroll && typeof ResizeObserver !== 'undefined') {
      this.mapResizeObserver = new ResizeObserver(() => this.scheduleMapViewportClamp());
      this.mapResizeObserver.observe(mapScroll);
    }

    this.restoreMapViewportPosition();
  }

  ngOnDestroy(): void {
    this.flushPendingMapViewportSave();

    if (this.restoreUnlockTimer) {
      clearTimeout(this.restoreUnlockTimer);
      this.restoreUnlockTimer = undefined;
    }

    if (this.mapViewportAnimationFrame !== undefined) {
      cancelAnimationFrame(this.mapViewportAnimationFrame);
      this.mapViewportAnimationFrame = undefined;
    }

    this.mapResizeObserver?.disconnect();
    this.removePinToolDragListeners();
    this.routeParamSubscription?.unsubscribe();
  }

  refreshCampaignPage(): boolean {
    this.loadMap(true);

    return true;
  }

  isRefreshingCampaignPage(): boolean {
    return this.isLoadingMap() || this.isLoadingPins();
  }

  protected goBackToMaps(): void {
    const campaignId = this.campaignId();

    if (!campaignId) {
      return;
    }

    void this.router.navigate(['/campaigns', campaignId, 'maps']);
  }

  protected resolveMapAssetUrl(map: CampaignMapModel): string | null {
    const assetUrl = map.assetUrl;

    if (!assetUrl) {
      return null;
    }

    if (/^(https?:\/\/|data:)/i.test(assetUrl)) {
      return assetUrl;
    }

    return assetUrl.startsWith('/')
      ? `${this.apiBaseUrl}${assetUrl}`
      : `${this.apiBaseUrl}/${assetUrl}`;
  }

  protected getMapCategoryLabel(map: CampaignMapModel): string {
    const category = this.getCategoryValue(map.category);

    return CampaignMapCategory[category] ?? 'Map';
  }

  protected scheduleMapViewportSave(): void {
    this.queueMapViewportSave();
  }

  protected saveMapViewportAfterWheel(): void {
    this.queueMapViewportSave();
  }

  protected restoreMapViewportPosition(): void {
    window.setTimeout(() => this.restoreMapViewport(), 0);
    window.setTimeout(() => this.restoreMapViewport(), 50);
    window.setTimeout(() => this.restoreMapViewport(), 150);
  }

  protected getMapLayerWidth(): number | null {
    return this.getCoordinateImageSize()?.width ?? null;
  }

  protected getMapLayerHeight(): number | null {
    return this.getCoordinateImageSize()?.height ?? null;
  }

  protected getMapLayerTransform(): string {
    const viewport = this.mapViewport();

    return `translate(${viewport.translateX}px, ${viewport.translateY}px) scale(${viewport.scale})`;
  }

  protected zoomMapIn(): void {
    this.zoomMapAtViewportCenter(MAP_VIEWPORT_ZOOM_STEP);
  }

  protected zoomMapOut(): void {
    this.zoomMapAtViewportCenter(1 / MAP_VIEWPORT_ZOOM_STEP);
  }

  protected onMapWheel(event: WheelEvent): void {
    event.preventDefault();
    event.stopPropagation();

    const focusPoint = this.getViewportPointFromClientPoint(event.clientX, event.clientY);

    if (!focusPoint) {
      return;
    }

    this.applyMapZoomAt(focusPoint, event.deltaY < 0 ? MAP_VIEWPORT_ZOOM_STEP : 1 / MAP_VIEWPORT_ZOOM_STEP);
  }

  protected startMapPan(event: PointerEvent): void {
    if (this.movingPin()) {
      this.startMovePointPlacement(event);
      return;
    }

    if (event.button !== 0 || this.isConnectionModeActive()) {
      return;
    }

    event.preventDefault();
    this.mapTouchPoints.set(event.pointerId, {
      x: event.clientX,
      y: event.clientY,
    });
    (event.currentTarget as HTMLElement).setPointerCapture(event.pointerId);

    if (this.mapTouchPoints.size === 2) {
      this.mapPan.set(null);
      this.startMapPinch();
      return;
    }

    this.mapPan.set({
      pointerId: event.pointerId,
      lastX: event.clientX,
      lastY: event.clientY,
    });
  }

  protected panMap(event: PointerEvent): void {
    const movingPin = this.movingPin();

    if (movingPin?.pointerId === event.pointerId) {
      event.preventDefault();
      this.moveDraftPinToPointer(event);
      return;
    }

    if (!this.mapTouchPoints.has(event.pointerId)) {
      return;
    }

    event.preventDefault();
    this.mapTouchPoints.set(event.pointerId, {
      x: event.clientX,
      y: event.clientY,
    });

    if (this.mapTouchPoints.size >= 2) {
      this.updateMapPinch();
      return;
    }

    const mapPan = this.mapPan();

    if (!mapPan || mapPan.pointerId !== event.pointerId) {
      return;
    }

    this.mapViewport.update((viewport) => {
      const containerSize = this.getMapViewportContainerSize();
      const mapSize = this.getMapViewportMapSize();

      if (!containerSize || !mapSize) {
        return viewport;
      }

      return panMapViewport(
        viewport,
        containerSize,
        mapSize,
        event.clientX - mapPan.lastX,
        event.clientY - mapPan.lastY,
      );
    });
    this.mapPan.set({
      pointerId: event.pointerId,
      lastX: event.clientX,
      lastY: event.clientY,
    });
    this.queueMapViewportSave();
  }

  protected finishMapPan(event: PointerEvent): void {
    const movingPin = this.movingPin();

    if (movingPin?.pointerId === event.pointerId) {
      event.preventDefault();
      this.movingPin.set({
        ...movingPin,
        pointerId: null,
      });
      return;
    }

    this.mapTouchPoints.delete(event.pointerId);

    if (this.mapTouchPoints.size < 2) {
      this.mapPinch.set(null);
    }

    if (this.mapPan()?.pointerId === event.pointerId) {
      this.mapPan.set(null);
    }

    if (this.mapTouchPoints.size === 1) {
      const [remainingTouchPoint] = Array.from(this.mapTouchPoints.entries());

      if (remainingTouchPoint) {
        this.mapPan.set({
          pointerId: remainingTouchPoint[0],
          lastX: remainingTouchPoint[1].x,
          lastY: remainingTouchPoint[1].y,
        });
      }
    }

    this.queueMapViewportSave();
  }

  protected openPinTool(pinTool: MapPinTool): void {
    if (this.shouldSuppressPinToolClick) {
      return;
    }

    if (!this.isSupportedPinTool(pinTool)) {
      this.modalHelper.showWarning('This pin type is not wired yet.');
      return;
    }

    this.openPlaceholderPinForm(pinTool.targetType ?? MapPinTargetType.Placeholder, null);
  }

  protected closePlaceholderPinForm(): void {
    if (this.isCreatingPlaceholderPin()) {
      return;
    }

    this.isPlaceholderPinFormOpen.set(false);
    this.placeholderPinValidationErrors.set({});
    this.placeholderPinFormMode.set('create');
    this.placeholderPinFormTargetType.set(MapPinTargetType.Placeholder);
    this.editingPinId.set(null);
    this.pendingPlaceholderPinCoordinates.set(null);
    this.placeholderPinForm()?.nativeElement.reset();
  }

  protected createPlaceholderPin(event: Event): void {
    event.preventDefault();

    const campaignId = this.campaignId();
    const mapId = this.mapId();
    const form = this.placeholderPinForm()?.nativeElement;

    if (!campaignId || mapId === null || !form) {
      return;
    }

    const formValues = this.getPlaceholderPinFormValues(form);
    const validationErrors = this.validatePlaceholderPinForm(formValues);

    this.placeholderPinValidationErrors.set(validationErrors);

    if (Object.keys(validationErrors).length > 0) {
      this.modalHelper.showError(Object.values(validationErrors));
      return;
    }

    if (this.placeholderPinFormMode() === 'edit') {
      this.updatePlaceholderPin(formValues);
      return;
    }

    this.isCreatingPlaceholderPin.set(true);
    this.campaignApiService.createCampaignMapPin(
      campaignId,
      mapId,
      this.toCreatePlaceholderPinRequest(formValues),
    ).subscribe({
      next: (response) => {
        this.isCreatingPlaceholderPin.set(false);
        this.closePlaceholderPinForm();

        if (response.data) {
          this.mapPins.update((pins) => [
            ...pins,
            {
              ...response.data!,
              targetData: null,
            },
          ]);
        } else {
          this.loadPins(true);
        }

        this.loadStoryBlocks();

        this.modalHelper.showSuccess(response.message, {
          statusCode: response.statusCode,
        });
      },
      error: (error: unknown) => {
        this.isCreatingPlaceholderPin.set(false);
        this.modalHelper.showError(this.getErrorMessage(error, 'Map pin could not be created.'), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  protected openPinContextMenu(pin: MapPinDetailsModel, event: MouseEvent): void {
    event.preventDefault();
    event.stopPropagation();

    this.pinContextMenu.set({
      pinId: pin.id,
      x: event.clientX,
      y: event.clientY,
    });
  }

  protected closePinContextMenu(): void {
    this.pinContextMenu.set(null);
  }

  protected canOpenContextPinTargetMap(): boolean {
    return this.getContextPinTargetMapId() !== null;
  }

  protected canOpenContextPinTargetStore(): boolean {
    return this.getContextPinTargetStoreId() !== null;
  }

  protected openContextPinTargetMap(): void {
    const campaignId = this.campaignId();
    const targetMapId = this.getContextPinTargetMapId();

    this.pinContextMenu.set(null);

    if (!campaignId || targetMapId === null) {
      return;
    }

    void this.router.navigate(['/campaigns', campaignId, 'maps', targetMapId]);
  }

  protected openContextPinTargetStore(): void {
    const campaignId = this.campaignId();
    const mapId = this.mapId();
    const targetStoreId = this.getContextPinTargetStoreId();

    this.pinContextMenu.set(null);

    if (!campaignId || mapId === null || targetStoreId === null) {
      return;
    }

    void this.router.navigate(
      ['/campaigns', campaignId, 'campaign-content', 'campaign-stores', targetStoreId],
      { queryParams: { fromMapId: mapId } },
    );
  }

  protected handlePinClick(pin: MapPinDetailsModel, event: Event): void {
    if (this.isConnectionModeActive()) {
      this.selectPinForConnection(pin, event);
      return;
    }

    event.preventDefault();
    event.stopPropagation();

    const targetStoreId = this.getPinTargetStoreId(pin);

    if (targetStoreId !== null) {
      this.openPinTargetStore(targetStoreId);
      return;
    }

    const targetMapId = this.getPinTargetMapId(pin);

    if (targetMapId === null) {
      return;
    }

    this.pendingMapNavigation.set({
      targetMapId,
      targetMapName: this.maps().find((map) => map.id === targetMapId)?.name ?? 'another map',
    });
  }

  protected closeMapNavigationPrompt(): void {
    this.pendingMapNavigation.set(null);
  }

  protected confirmMapNavigation(): void {
    const campaignId = this.campaignId();
    const pendingMapNavigation = this.pendingMapNavigation();

    this.pendingMapNavigation.set(null);

    if (!campaignId || !pendingMapNavigation) {
      return;
    }

    void this.router.navigate(['/campaigns', campaignId, 'maps', pendingMapNavigation.targetMapId]);
  }

  protected startMoveContextPin(): void {
    const pin = this.getContextPin();

    this.pinContextMenu.set(null);

    if (!pin) {
      return;
    }

    this.selectedPinId.set(pin.id);
    this.isConnectionModeActive.set(false);
    this.connectionStartPinId.set(null);
    this.pendingConnectionPinIds.set(null);
    this.movingPin.set({
      pinId: pin.id,
      originalX: Number(pin.xCoordinate),
      originalY: Number(pin.yCoordinate),
      x: Number(pin.xCoordinate),
      y: Number(pin.yCoordinate),
      pointerId: null,
    });
  }

  protected cancelMovePoint(): void {
    if (this.isSavingMovedPin()) {
      return;
    }

    this.movingPin.set(null);
  }

  protected confirmMovePoint(): void {
    const movingPin = this.movingPin();
    const campaignId = this.campaignId();
    const mapId = this.mapId();
    const pin = movingPin
      ? this.mapPins().find((item) => item.id === movingPin.pinId) ?? null
      : null;

    if (!movingPin || !campaignId || mapId === null || !pin || this.isSavingMovedPin()) {
      return;
    }

    this.isSavingMovedPin.set(true);
    this.campaignApiService.updateCampaignMapPin(
      campaignId,
      mapId,
      pin.id,
      {
        ...this.toUpdateMapPinRequest(pin),
        xCoordinate: this.toBackendCoordinatePrecision(movingPin.x),
        yCoordinate: this.toBackendCoordinatePrecision(movingPin.y),
      },
    ).subscribe({
      next: (response) => {
        this.isSavingMovedPin.set(false);
        this.movingPin.set(null);

        if (!response.data) {
          this.loadPins(true);
          return;
        }

        this.mapPins.update((pins) => pins.map((item) => (
          item.id === response.data!.id
            ? {
              ...response.data!,
              targetData: item.targetId === response.data!.targetId
                ? item.targetData
                : null,
            }
            : item
        )));
        this.loadStoryBlocks();
        this.modalHelper.showSuccess(response.message, {
          statusCode: response.statusCode,
        });
      },
      error: (error: unknown) => {
        this.isSavingMovedPin.set(false);
        this.modalHelper.showError(this.getErrorMessage(error, 'Map pin could not be moved.'), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  protected editContextPin(): void {
    const menu = this.pinContextMenu();
    const pin = menu
      ? this.mapPins().find((item) => item.id === menu.pinId) ?? null
      : null;

    if (!pin) {
      return;
    }

    this.pinContextMenu.set(null);
    const targetType = this.getPinTargetTypeValue(pin.targetType);

    if (
      targetType !== MapPinTargetType.Placeholder &&
      targetType !== MapPinTargetType.PlayersPosition &&
      targetType !== MapPinTargetType.Map &&
      targetType !== MapPinTargetType.Store &&
      targetType !== MapPinTargetType.StoryBlock
    ) {
      this.modalHelper.showWarning('This pin type is not editable yet.');
      return;
    }

    this.placeholderPinValidationErrors.set({});
    this.placeholderPinFormMode.set('edit');
    this.placeholderPinFormTargetType.set(targetType as MapPinTargetType);
    this.editingPinId.set(pin.id);
    this.pendingPlaceholderPinCoordinates.set(null);
    this.isPlaceholderPinFormOpen.set(true);

    window.setTimeout(() => {
      const form = this.placeholderPinForm()?.nativeElement;

      if (!form) {
        return;
      }

      const titleInput = form.elements.namedItem('title') as HTMLInputElement | null;
      const descriptionInput = form.elements.namedItem('description') as HTMLTextAreaElement | null;
      const targetMapInput = form.elements.namedItem('targetMapId') as HTMLSelectElement | null;
      const targetStoreInput = form.elements.namedItem('targetStoreId') as HTMLSelectElement | null;
      const targetStoryBlockInput = form.elements.namedItem('targetStoryBlockId') as HTMLSelectElement | null;

      if (titleInput) {
        titleInput.value = pin.label;
      }

      if (descriptionInput) {
        descriptionInput.value = pin.description;
      }

      if (targetMapInput) {
        targetMapInput.value = pin.targetId ?? '';
      }

      if (targetStoreInput) {
        targetStoreInput.value = pin.targetId ?? '';
      }

      if (targetStoryBlockInput) {
        targetStoryBlockInput.value = pin.targetId ?? '';
      }
    });
  }

  protected deleteContextPin(): void {
    const campaignId = this.campaignId();
    const mapId = this.mapId();
    const menu = this.pinContextMenu();

    if (!campaignId || mapId === null || !menu || this.deletingPinId()) {
      return;
    }

    this.deletingPinId.set(menu.pinId);
    this.pinContextMenu.set(null);
    this.campaignApiService.deleteCampaignMapPin(campaignId, mapId, menu.pinId).subscribe({
      next: (response) => {
        this.deletingPinId.set(null);
        this.mapPins.update((pins) => pins.filter((pin) => pin.id !== menu.pinId));
        this.mapPinConnections.update((connections) => connections.filter((connection) => (
          connection.mapPinAId !== menu.pinId && connection.mapPinBId !== menu.pinId
        )));
        this.loadStoryBlocks();
        this.modalHelper.showSuccess(response.message, {
          statusCode: response.statusCode,
        });
      },
      error: (error: unknown) => {
        this.deletingPinId.set(null);
        this.modalHelper.showError(this.getErrorMessage(error, 'Map pin could not be deleted.'), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  protected getPlaceholderPinDialogTitle(): string {
    const isEdit = this.placeholderPinFormMode() === 'edit';

    if (this.placeholderPinFormTargetType() === MapPinTargetType.Map) {
      return isEdit ? 'Edit Map Link' : 'Add Map Link';
    }

    if (this.placeholderPinFormTargetType() === MapPinTargetType.Store) {
      return isEdit ? 'Edit Store Pin' : 'Add Store Pin';
    }

    if (this.placeholderPinFormTargetType() === MapPinTargetType.StoryBlock) {
      return isEdit ? 'Edit Story Block Pin' : 'Add Story Block Pin';
    }

    if (this.placeholderPinFormTargetType() === MapPinTargetType.PlayersPosition) {
      return isEdit ? 'Edit Players Position' : 'Add Players Position';
    }

    return isEdit ? 'Edit Map Note' : 'Add Map Note';
  }

  protected getPlaceholderPinDialogEyebrow(): string {
    switch (this.placeholderPinFormTargetType()) {
      case MapPinTargetType.Map:
        return 'Map Link Pin';
      case MapPinTargetType.Store:
        return 'Store Pin';
      case MapPinTargetType.StoryBlock:
        return 'Story Block Pin';
      case MapPinTargetType.PlayersPosition:
        return 'Players Position Pin';
      default:
        return 'Placeholder Pin';
    }
  }

  protected getPlaceholderPinSubmitLabel(): string {
    return this.placeholderPinFormMode() === 'edit'
      ? 'Save Pin'
      : 'Add Pin';
  }

  protected shouldShowMapLinkTargetField(): boolean {
    return this.placeholderPinFormTargetType() === MapPinTargetType.Map;
  }

  protected shouldShowStoreTargetField(): boolean {
    return this.placeholderPinFormTargetType() === MapPinTargetType.Store;
  }

  protected shouldShowStoryBlockTargetField(): boolean {
    return this.placeholderPinFormTargetType() === MapPinTargetType.StoryBlock;
  }

  protected isMapLinkTargetSelected(mapId: number): boolean {
    return this.getEditingMapLinkTargetMapId() === mapId;
  }

  protected isStoreTargetSelected(storeId: number): boolean {
    return this.getEditingStoreTargetStoreId() === storeId;
  }

  protected isStoryBlockTargetSelected(storyBlockId: string): boolean {
    return this.getEditingStoryBlockTargetStoryBlockId() === storyBlockId;
  }

  protected getStoryBlockMapPinUsageCount(storyBlock: StoryBlockModel): number {
    return storyBlock.mapPins?.length ?? 0;
  }

  protected storeDisplayName(store: CampaignStoreModel): string {
    return this.normalizeText(store.storeName) || store.storeLocation || 'Unnamed Store';
  }

  protected setImageNaturalSize(event: Event): void {
    const image = event.target as HTMLImageElement;

    this.imageNaturalSize.set({
      width: image.naturalWidth,
      height: image.naturalHeight,
    });
    this.restoreMapViewportPosition();
  }

  protected getPinLeft(pin: MapPinDetailsModel): string {
    const imageSize = this.getCoordinateImageSize();

    if (!imageSize || imageSize.width <= 0) {
      return '0%';
    }

    return `${Math.max(0, Math.min(100, (this.getRenderedPinX(pin) / imageSize.width) * 100))}%`;
  }

  protected getPinTop(pin: MapPinDetailsModel): string {
    const imageSize = this.getCoordinateImageSize();

    if (!imageSize || imageSize.height <= 0) {
      return '0%';
    }

    return `${Math.max(0, Math.min(100, (this.getRenderedPinY(pin) / imageSize.height) * 100))}%`;
  }

  protected getMapConnectionViewBox(): string {
    const imageSize = this.getCoordinateImageSize();

    if (!imageSize || imageSize.width <= 0 || imageSize.height <= 0) {
      return '0 0 1 1';
    }

    return `0 0 ${imageSize.width} ${imageSize.height}`;
  }

  protected canRenderConnection(connection: MapPinConnectionModel): boolean {
    return this.mapPins().some((pin) => pin.id === connection.mapPinAId)
      && this.mapPins().some((pin) => pin.id === connection.mapPinBId);
  }

  protected isPlaceholderPin(pin: MapPinDetailsModel): boolean {
    return this.getPinTargetTypeValue(pin.targetType) === MapPinTargetType.Placeholder;
  }

  protected isMapLinkPin(pin: MapPinDetailsModel): boolean {
    return this.getPinTargetTypeValue(pin.targetType) === MapPinTargetType.Map;
  }

  protected isStorePin(pin: MapPinDetailsModel): boolean {
    return this.getPinTargetTypeValue(pin.targetType) === MapPinTargetType.Store;
  }

  protected isStoryBlockPin(pin: MapPinDetailsModel): boolean {
    return this.getPinTargetTypeValue(pin.targetType) === MapPinTargetType.StoryBlock;
  }

  protected isPlayersPositionPin(pin: MapPinDetailsModel): boolean {
    return this.getPinTargetTypeValue(pin.targetType) === MapPinTargetType.PlayersPosition;
  }

  protected isSelectedPin(pin: MapPinDetailsModel): boolean {
    return this.selectedPinId() === pin.id;
  }

  protected isMovingPin(pin: MapPinDetailsModel): boolean {
    return this.movingPin()?.pinId === pin.id;
  }

  protected getPinTypeLabel(pin: MapPinDetailsModel): string {
    switch (this.getPinTargetTypeValue(pin.targetType)) {
      case MapPinTargetType.Placeholder:
        return 'Placeholder';
      case MapPinTargetType.StoryBlock:
        return 'Story Block';
      case MapPinTargetType.Map:
        return 'Map Link';
      case MapPinTargetType.Store:
        return 'Store';
      case MapPinTargetType.PlayersPosition:
        return 'Players Position';
      default:
        return 'Unknown';
    }
  }

  protected getPinTypeColor(pin: MapPinDetailsModel): string {
    switch (this.getPinTargetTypeValue(pin.targetType)) {
      case MapPinTargetType.Map:
        return '#38bdf8';
      case MapPinTargetType.Store:
        return '#facc15';
      case MapPinTargetType.StoryBlock:
        return '#d97706';
      case MapPinTargetType.PlayersPosition:
        return '#bbf7d0';
      case MapPinTargetType.Placeholder:
        return '#ffffff';
      default:
        return '#d1d5db';
    }
  }

  protected getPinDescriptionPreview(pin: MapPinDetailsModel): string {
    const description = pin.description.trim();

    if (description.length === 0) {
      return 'No description';
    }

    return description.length > 120
      ? `${description.slice(0, 117)}...`
      : description;
  }

  protected focusPin(pin: MapPinDetailsModel): void {
    const containerSize = this.getMapViewportContainerSize();
    const imageSize = this.getCoordinateImageSize();

    if (!containerSize || !imageSize || imageSize.width <= 0 || imageSize.height <= 0) {
      return;
    }

    const viewport = this.mapViewport();

    this.selectedPinId.set(pin.id);
    this.mapViewport.set(clampMapViewport(
      {
        ...viewport,
        translateX: (containerSize.width / 2) - (Number(pin.xCoordinate) * viewport.scale),
        translateY: (containerSize.height / 2) - (Number(pin.yCoordinate) * viewport.scale),
      },
      containerSize,
      imageSize,
    ));
    this.queueMapViewportSave();
  }

  protected toggleConnectionMode(): void {
    const isNextActive = !this.isConnectionModeActive();

    if (isNextActive) {
      this.movingPin.set(null);
    }

    this.isConnectionModeActive.set(isNextActive);
    this.connectionStartPinId.set(null);

    if (!isNextActive) {
      this.pendingConnectionPinIds.set(null);
      this.closeConnectionForm();
    }
  }

  protected selectPinForConnection(pin: MapPinDetailsModel, event: Event): void {
    if (!this.isConnectionModeActive()) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();

    const startPinId = this.connectionStartPinId();

    if (startPinId === null) {
      this.connectionStartPinId.set(pin.id);
      this.selectedPinId.set(pin.id);
      return;
    }

    if (startPinId === pin.id) {
      this.modalHelper.showWarning('Choose a different point for the connection.');
      return;
    }

    this.pendingConnectionPinIds.set({
      pinAId: startPinId,
      pinBId: pin.id,
    });
    this.editingConnectionId.set(null);
    this.connectionValidationErrors.set({});
    this.isConnectionFormOpen.set(true);
    this.selectedPinId.set(pin.id);
  }

  protected isConnectionStartPin(pin: MapPinDetailsModel): boolean {
    return this.connectionStartPinId() === pin.id;
  }

  protected getConnectionModeLabel(): string {
    if (!this.isConnectionModeActive()) {
      return 'Create Connection';
    }

    return this.connectionStartPinId() === null
      ? 'Select First Point'
      : 'Select Second Point';
  }

  protected getConnectionLineX(connection: MapPinConnectionModel, pinKey: 'mapPinAId' | 'mapPinBId'): number {
    return this.getConnectionPinCoordinate(connection[pinKey], 'x');
  }

  protected getConnectionLineY(connection: MapPinConnectionModel, pinKey: 'mapPinAId' | 'mapPinBId'): number {
    return this.getConnectionPinCoordinate(connection[pinKey], 'y');
  }

  protected getConnectionDurationLabel(connection: MapPinConnectionModel): string {
    if (connection.distanceValue === null || connection.distanceUnit === null) {
      return 'Duration not set';
    }

    return `${connection.distanceValue} ${this.getConnectionDistanceUnitLabel(connection.distanceUnit)}`;
  }

  protected showConnectionTooltip(connection: MapPinConnectionModel, event: PointerEvent): void {
    this.connectionTooltip.set({
      connectionId: connection.id,
      label: this.getConnectionDurationLabel(connection),
      x: event.clientX,
      y: event.clientY,
    });
  }

  protected moveConnectionTooltip(event: PointerEvent): void {
    const tooltip = this.connectionTooltip();

    if (!tooltip) {
      return;
    }

    this.connectionTooltip.set({
      ...tooltip,
      x: event.clientX,
      y: event.clientY,
    });
  }

  protected hideConnectionTooltip(): void {
    this.connectionTooltip.set(null);
  }

  protected openConnectionEditor(connection: MapPinConnectionModel, event: Event): void {
    event.preventDefault();
    event.stopPropagation();

    this.hideConnectionTooltip();
    this.isConnectionModeActive.set(false);
    this.connectionStartPinId.set(null);
    this.pendingConnectionPinIds.set({
      pinAId: connection.mapPinAId,
      pinBId: connection.mapPinBId,
    });
    this.editingConnectionId.set(connection.id);
    this.connectionValidationErrors.set({});
    this.isConnectionFormOpen.set(true);
  }

  protected saveConnection(event: Event): void {
    event.preventDefault();

    const campaignId = this.campaignId();
    const mapId = this.mapId();
    const pinIds = this.pendingConnectionPinIds();
    const form = event.currentTarget as HTMLFormElement | null;

    if (!campaignId || mapId === null || !pinIds || !form) {
      return;
    }

    const formValues = this.getConnectionFormValues(form);
    const validationErrors = this.validateConnectionForm(formValues);

    this.connectionValidationErrors.set(validationErrors);

    if (Object.keys(validationErrors).length > 0) {
      this.modalHelper.showError(Object.values(validationErrors));
      return;
    }

    this.isCreatingConnection.set(true);
    const editingConnectionId = this.editingConnectionId();
    const request = this.toCreateConnectionRequest(pinIds, formValues);
    const saveRequest = editingConnectionId === null
      ? this.campaignApiService.createCampaignMapPinConnection(
        campaignId,
        mapId,
        request,
      )
      : this.campaignApiService.updateCampaignMapPinConnection(
        campaignId,
        mapId,
        editingConnectionId,
        request,
      );

    saveRequest.subscribe({
      next: (response) => {
        this.isCreatingConnection.set(false);
        this.closeConnectionForm();
        this.isConnectionModeActive.set(false);
        this.connectionStartPinId.set(null);

        if (response.data) {
          this.mapPinConnections.update((connections) => (
            editingConnectionId === null
              ? [...connections, response.data!]
              : connections.map((connection) => (
                connection.id === response.data!.id ? response.data! : connection
              ))
          ));
        } else {
          this.loadPins(true);
        }

        this.modalHelper.showSuccess(response.message, {
          statusCode: response.statusCode,
        });
      },
      error: (error: unknown) => {
        this.isCreatingConnection.set(false);
        this.modalHelper.showError(this.getErrorMessage(error, 'Map point connection could not be saved.'), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  protected getConnectionDialogTitle(): string {
    return this.editingConnectionId() === null
      ? 'Create Connection'
      : 'Edit Connection';
  }

  protected getConnectionDialogSubmitLabel(): string {
    return this.editingConnectionId() === null
      ? 'Add Connection'
      : 'Save Connection';
  }

  protected getConnectionDialogDistanceValue(): number | null {
    const connection = this.getEditingConnection();

    return connection?.distanceValue ?? null;
  }

  protected getConnectionDialogDistanceUnit(): MapPinConnectionDistanceUnit {
    const unit = this.getEditingConnection()?.distanceUnit ?? null;
    const unitValue = this.getConnectionDistanceUnitValue(unit);

    return this.isMapPinConnectionDistanceUnit(unitValue)
      ? unitValue
      : MapPinConnectionDistanceUnit.Minutes;
  }

  protected isConnectionDialogDistanceUnitSelected(distanceUnit: MapPinConnectionDistanceUnit): boolean {
    return this.getConnectionDialogDistanceUnit() === distanceUnit;
  }

  protected closeConnectionForm(): void {
    if (this.isCreatingConnection()) {
      return;
    }

    this.isConnectionFormOpen.set(false);
    this.pendingConnectionPinIds.set(null);
    this.editingConnectionId.set(null);
    this.connectionValidationErrors.set({});
  }

  protected startPinDrag(pin: MapPinDetailsModel, event: PointerEvent): void {
    event.stopPropagation();

    if (this.isConnectionModeActive() || this.movingPin()) {
      return;
    }

    event.preventDefault();
  }

  protected dragPin(event: PointerEvent): void {
    event.stopPropagation();
  }

  protected finishPinDrag(pin: MapPinDetailsModel, event: PointerEvent): void {
    event.stopPropagation();
  }

  protected startPinToolDrag(pinTool: MapPinTool, event: PointerEvent): void {
    if (!this.isSupportedPinTool(pinTool)) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();

    this.draggingPinTool.set({
      pinTool,
      pointerId: event.pointerId,
      startX: event.clientX,
      startY: event.clientY,
      x: event.clientX,
      y: event.clientY,
      isDragging: false,
      isOverMap: this.getImageCoordinatesFromPointer(event) !== null,
    });
    this.addPinToolDragListeners();
  }

  protected dragPinTool(event: PointerEvent): void {
    const draggingPinTool = this.draggingPinTool();

    if (!draggingPinTool || draggingPinTool.pointerId !== event.pointerId) {
      return;
    }

    event.preventDefault();

    const movementDistance = Math.hypot(
      event.clientX - draggingPinTool.startX,
      event.clientY - draggingPinTool.startY,
    );

    this.draggingPinTool.set({
      ...draggingPinTool,
      x: event.clientX,
      y: event.clientY,
      isDragging: draggingPinTool.isDragging || movementDistance >= 4,
      isOverMap: this.getImageCoordinatesFromPointer(event) !== null,
    });
  }

  protected finishPinToolDrag(event: PointerEvent): void {
    const draggingPinTool = this.draggingPinTool();

    if (!draggingPinTool || draggingPinTool.pointerId !== event.pointerId) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();

    this.draggingPinTool.set(null);
    this.removePinToolDragListeners();

    if (!draggingPinTool.isDragging) {
      return;
    }

    this.shouldSuppressPinToolClick = true;
    window.setTimeout(() => {
      this.shouldSuppressPinToolClick = false;
    });

    const coordinates = this.getImageCoordinatesFromPointer(event);

    if (!coordinates) {
      this.modalHelper.showWarning('Drop the pin on the map image.');
      return;
    }

    this.openPlaceholderPinForm(draggingPinTool.pinTool.targetType ?? MapPinTargetType.Placeholder, coordinates);
  }

  protected cancelPinToolDrag(event: PointerEvent): void {
    const draggingPinTool = this.draggingPinTool();

    if (!draggingPinTool || draggingPinTool.pointerId !== event.pointerId) {
      return;
    }

    this.draggingPinTool.set(null);
    this.removePinToolDragListeners();
  }

  private loadMap(forceRefresh = false): void {
    const campaignId = this.campaignId();

    if (!campaignId || (this.isLoadingMap() && !forceRefresh)) {
      return;
    }

    this.isLoadingMap.set(true);
    this.campaignApiService.fetchCampaignMaps(campaignId).subscribe({
      next: (response) => {
        this.maps.set(response.data ?? []);
        this.isLoadingMap.set(false);
        this.hasAttemptedViewportRestore = false;
        this.isRestoringViewport = true;
        this.restoreMapViewportPosition();
        this.loadPins(forceRefresh);
      },
      error: (error: unknown) => {
        this.isLoadingMap.set(false);
        this.modalHelper.showError(this.getErrorMessage(error, 'Campaign map could not be loaded.'), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  private loadPins(forceRefresh = false): void {
    const campaignId = this.campaignId();
    const mapId = this.mapId();

    if (!campaignId || mapId === null || (this.isLoadingPins() && !forceRefresh)) {
      return;
    }

    this.isLoadingPins.set(true);
    this.campaignApiService.fetchCampaignMapPins(campaignId, mapId).subscribe({
      next: (response) => {
        this.mapPins.set(response.data?.pins ?? []);
        this.mapPinConnections.set(response.data?.connections ?? []);
        this.isLoadingPins.set(false);
      },
      error: (error: unknown) => {
        this.isLoadingPins.set(false);
        this.modalHelper.showError(this.getErrorMessage(error, 'Map pins could not be loaded.'), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  private loadStores(): void {
    const campaignId = this.campaignId();

    if (!campaignId) {
      return;
    }

    this.campaignApiService.fetchCampaignStores(campaignId).subscribe({
      next: (response) => {
        this.stores.set(response.data ?? []);
      },
      error: (error: unknown) => {
        this.modalHelper.showError(this.getErrorMessage(error, 'Campaign stores could not be loaded.'), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  private loadStoryBlocks(): void {
    const campaignId = this.campaignId();

    if (!campaignId) {
      return;
    }

    this.campaignApiService.fetchStoryBlocks(campaignId).subscribe({
      next: (response) => {
        this.storyBlocks.set(response.data ?? []);
      },
      error: (error: unknown) => {
        this.modalHelper.showError(this.getErrorMessage(error, 'Campaign story blocks could not be loaded.'), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  private resetMapViewerForRouteChange(): void {
    this.mapPins.set([]);
    this.mapPinConnections.set([]);
    this.selectedPinId.set(null);
    this.pinContextMenu.set(null);
    this.pendingMapNavigation.set(null);
    this.movingPin.set(null);
    this.isSavingMovedPin.set(false);
    this.connectionTooltip.set(null);
    this.isConnectionModeActive.set(false);
    this.connectionStartPinId.set(null);
    this.pendingConnectionPinIds.set(null);
    this.editingConnectionId.set(null);
    this.isConnectionFormOpen.set(false);
    this.imageNaturalSize.set(null);
    this.mapViewport.set({
      scale: 1,
      translateX: 0,
      translateY: 0,
    });
    this.mapTouchPoints.clear();
    this.mapPan.set(null);
    this.mapPinch.set(null);
    this.hasAttemptedViewportRestore = false;
    this.isRestoringViewport = true;
  }

  private scheduleMapViewportClamp(): void {
    this.scheduleMapViewportUpdate(() => {
      const containerSize = this.getMapViewportContainerSize();
      const mapSize = this.getMapViewportMapSize();

      if (!containerSize || !mapSize) {
        return;
      }

      this.mapViewport.update((viewport) => clampMapViewport(viewport, containerSize, mapSize));
      this.queueMapViewportSave();
    });
  }

  private scheduleMapViewportUpdate(update: () => void): void {
    if (this.mapViewportAnimationFrame !== undefined) {
      cancelAnimationFrame(this.mapViewportAnimationFrame);
    }

    this.mapViewportAnimationFrame = requestAnimationFrame(() => {
      this.mapViewportAnimationFrame = undefined;
      update();
    });
  }

  private resetMapViewport(): void {
    const containerSize = this.getMapViewportContainerSize();
    const mapSize = this.getMapViewportMapSize();

    if (!containerSize || !mapSize) {
      return;
    }

    this.mapViewport.set(createInitialMapViewport(containerSize, mapSize));
  }

  private zoomMapAtViewportCenter(scaleFactor: number): void {
    const containerSize = this.getMapViewportContainerSize();

    if (!containerSize) {
      return;
    }

    this.applyMapZoomAt(
      {
        x: containerSize.width / 2,
        y: containerSize.height / 2,
      },
      scaleFactor,
    );
  }

  private applyMapZoomAt(focusPoint: MapViewportPoint, scaleFactor: number): void {
    const containerSize = this.getMapViewportContainerSize();
    const mapSize = this.getMapViewportMapSize();

    if (!containerSize || !mapSize) {
      return;
    }

    this.mapViewport.update((viewport) => zoomMapViewport(
      viewport,
      containerSize,
      mapSize,
      focusPoint,
      scaleFactor,
    ));
    this.queueMapViewportSave();
  }

  private startMapPinch(): void {
    const pinchPoints = this.getActivePinchPoints();

    if (!pinchPoints) {
      return;
    }

    this.mapPinch.set({
      initialDistance: this.getDistanceBetweenPoints(pinchPoints[0], pinchPoints[1]),
      initialCenter: this.getCenterPoint(pinchPoints[0], pinchPoints[1]),
      initialViewport: this.mapViewport(),
    });
  }

  private updateMapPinch(): void {
    const pinch = this.mapPinch();
    const pinchPoints = this.getActivePinchPoints();
    const containerSize = this.getMapViewportContainerSize();
    const mapSize = this.getMapViewportMapSize();

    if (!pinch || !pinchPoints || !containerSize || !mapSize || pinch.initialDistance <= 0) {
      return;
    }

    const currentDistance = this.getDistanceBetweenPoints(pinchPoints[0], pinchPoints[1]);
    const currentCenter = this.getCenterPoint(pinchPoints[0], pinchPoints[1]);

    const zoomedViewport = zoomMapViewportToScale(
      pinch.initialViewport,
      containerSize,
      mapSize,
      pinch.initialCenter,
      pinch.initialViewport.scale * (currentDistance / pinch.initialDistance),
    );

    this.mapViewport.set(panMapViewport(
      zoomedViewport,
      containerSize,
      mapSize,
      currentCenter.x - pinch.initialCenter.x,
      currentCenter.y - pinch.initialCenter.y,
    ));
    this.mapPan.set(null);
    this.queueMapViewportSave();
  }

  private getActivePinchPoints(): [MapViewportPoint, MapViewportPoint] | null {
    const points = Array.from(this.mapTouchPoints.values());

    return points.length >= 2
      ? [this.toViewportPoint(points[0]), this.toViewportPoint(points[1])]
      : null;
  }

  private getDistanceBetweenPoints(pointA: MapViewportPoint, pointB: MapViewportPoint): number {
    return Math.hypot(pointB.x - pointA.x, pointB.y - pointA.y);
  }

  private getCenterPoint(pointA: MapViewportPoint, pointB: MapViewportPoint): MapViewportPoint {
    return {
      x: (pointA.x + pointB.x) / 2,
      y: (pointA.y + pointB.y) / 2,
    };
  }

  private toViewportPoint(point: MapTouchPoint): MapViewportPoint {
    return this.getViewportPointFromClientPoint(point.x, point.y) ?? { x: point.x, y: point.y };
  }

  private getViewportPointFromClientPoint(clientX: number, clientY: number): MapViewportPoint | null {
    const scrollElement = this.mapScroll()?.nativeElement;

    if (!scrollElement) {
      return null;
    }

    const rect = scrollElement.getBoundingClientRect();

    return {
      x: clientX - rect.left,
      y: clientY - rect.top,
    };
  }

  private getMapViewportContainerSize(): MapViewportSize | null {
    const scrollElement = this.mapScroll()?.nativeElement;

    if (!scrollElement || scrollElement.clientWidth <= 0 || scrollElement.clientHeight <= 0) {
      return null;
    }

    return {
      width: scrollElement.clientWidth,
      height: scrollElement.clientHeight,
    };
  }

  private getMapViewportMapSize(): MapViewportSize | null {
    const imageSize = this.getCoordinateImageSize();

    if (!imageSize || imageSize.width <= 0 || imageSize.height <= 0) {
      return null;
    }

    return imageSize;
  }

  private getCategoryValue(
    category: CampaignMapModel['category'],
  ): number {
    if (typeof category === 'number') {
      return category;
    }

    if (typeof category === 'string') {
      const numericCategory = Number(category);

      if (Number.isFinite(numericCategory)) {
        return numericCategory;
      }

      return CampaignMapCategory[category as keyof typeof CampaignMapCategory] ?? 0;
    }

    return 0;
  }

  private queueMapViewportSave(): void {
    if (!this.canSaveMapViewport()) {
      return;
    }

    if (this.saveViewportTimer) {
      clearTimeout(this.saveViewportTimer);
    }

    this.saveViewportTimer = setTimeout(() => {
      this.saveViewportTimer = undefined;
      this.flushMapViewport();
    }, MAP_VIEWPORT_SAVE_DEBOUNCE_MS);
  }

  private flushPendingMapViewportSave(): void {
    if (!this.saveViewportTimer) {
      return;
    }

    clearTimeout(this.saveViewportTimer);
    this.saveViewportTimer = undefined;
    this.flushMapViewport();
  }

  private flushMapViewport(): void {
    const cacheKey = this.getMapViewportCacheKey();

    if (!cacheKey) {
      return;
    }

    const viewport = this.mapViewport();

    this.browserCache.set<MapViewportCacheEntry>(cacheKey, {
      scale: viewport.scale,
      translateX: viewport.translateX,
      translateY: viewport.translateY,
    });
  }

  private canSaveMapViewport(): boolean {
    return this.hasAttemptedViewportRestore && !this.isRestoringViewport;
  }

  private isSupportedPinTool(pinTool: MapPinTool): boolean {
    return pinTool.targetType === MapPinTargetType.Placeholder
      || pinTool.targetType === MapPinTargetType.PlayersPosition
      || pinTool.targetType === MapPinTargetType.Map
      || pinTool.targetType === MapPinTargetType.Store
      || pinTool.targetType === MapPinTargetType.StoryBlock;
  }

  private getEditingMapLinkTargetMapId(): number | null {
    const editingPinId = this.editingPinId();
    const pin = editingPinId === null
      ? null
      : this.mapPins().find((item) => item.id === editingPinId) ?? null;
    const targetMapId = pin?.targetId ? Number(pin.targetId) : null;

    return targetMapId !== null && Number.isInteger(targetMapId)
      ? targetMapId
      : null;
  }

  private getMapLinkTargetMap(mapId: number): CampaignMapModel | null {
    return this.mapLinkTargetMaps().find((map) => map.id === mapId) ?? null;
  }

  private getEditingStoreTargetStoreId(): number | null {
    const editingPinId = this.editingPinId();
    const pin = editingPinId === null
      ? null
      : this.mapPins().find((item) => item.id === editingPinId) ?? null;
    const targetStoreId = pin?.targetId ? Number(pin.targetId) : null;

    return targetStoreId !== null && Number.isInteger(targetStoreId)
      ? targetStoreId
      : null;
  }

  private getStoreTargetStore(storeId: number): CampaignStoreModel | null {
    return this.stores().find((store) => store.storeId === storeId) ?? null;
  }

  private getEditingStoryBlockTargetStoryBlockId(): string | null {
    const editingPinId = this.editingPinId();
    const pin = editingPinId === null
      ? null
      : this.mapPins().find((item) => item.id === editingPinId) ?? null;

    return pin?.targetId && this.getStoryBlockTargetStoryBlock(pin.targetId)
      ? pin.targetId
      : null;
  }

  private getStoryBlockTargetStoryBlock(storyBlockId: string): StoryBlockModel | null {
    return this.storyBlocks().find((storyBlock) => storyBlock.storyBlockId === storyBlockId) ?? null;
  }

  private getPlaceholderPinTargetId(
    targetType: MapPinTargetType,
    formValues: PlaceholderPinFormValues,
  ): string | null {
    switch (targetType) {
      case MapPinTargetType.Map:
        return formValues.targetMapId !== null ? String(formValues.targetMapId) : null;
      case MapPinTargetType.Store:
        return formValues.targetStoreId !== null ? String(formValues.targetStoreId) : null;
      case MapPinTargetType.StoryBlock:
        return formValues.targetStoryBlockId;
      default:
        return null;
    }
  }

  private getPlaceholderPinFormValues(form: HTMLFormElement): PlaceholderPinFormValues {
    const formData = new FormData(form);
    const rawTargetMapId = this.getStringValue(formData, 'targetMapId');
    const rawTargetStoreId = this.getStringValue(formData, 'targetStoreId');
    const targetStoryBlockId = this.getStringValue(formData, 'targetStoryBlockId');
    const targetMapId = rawTargetMapId.length > 0
      ? Number(rawTargetMapId)
      : null;
    const targetStoreId = rawTargetStoreId.length > 0
      ? Number(rawTargetStoreId)
      : null;
    const targetMap = targetMapId === null
      ? null
      : this.getMapLinkTargetMap(targetMapId);
    const targetStore = targetStoreId === null
      ? null
      : this.getStoreTargetStore(targetStoreId);
    const targetStoryBlock = targetStoryBlockId.length === 0
      ? null
      : this.getStoryBlockTargetStoryBlock(targetStoryBlockId);
    const title = this.getStringValue(formData, 'title');

    const fallbackTitle = targetMap?.name
      ?? (targetStore ? this.storeDisplayName(targetStore) : null)
      ?? targetStoryBlock?.title
      ?? '';

    return {
      title: title.length > 0
        ? title
        : fallbackTitle,
      description: this.getStringValue(formData, 'description'),
      targetMapId: targetMapId !== null && Number.isInteger(targetMapId)
        ? targetMapId
        : null,
      targetStoreId: targetStoreId !== null && Number.isInteger(targetStoreId)
        ? targetStoreId
        : null,
      targetStoryBlockId: targetStoryBlock ? targetStoryBlock.storyBlockId : null,
    };
  }

  private validatePlaceholderPinForm(
    formValues: PlaceholderPinFormValues,
  ): PlaceholderPinFormErrors {
    const errors: PlaceholderPinFormErrors = {};

    if (formValues.title.length === 0) {
      errors.title = this.placeholderPinFormTargetType() === MapPinTargetType.Map
        ? 'Map link title is required.'
        : 'Placeholder title is required.';
    }

    if (this.placeholderPinFormTargetType() === MapPinTargetType.Map
      && (formValues.targetMapId === null || !this.getMapLinkTargetMap(formValues.targetMapId))) {
      errors.targetMapId = 'Choose the map this pin should link to.';
    }

    if (this.placeholderPinFormTargetType() === MapPinTargetType.Store
      && (formValues.targetStoreId === null || !this.getStoreTargetStore(formValues.targetStoreId))) {
      errors.targetStoreId = 'Choose the store this pin should link to.';
    }

    if (this.placeholderPinFormTargetType() === MapPinTargetType.StoryBlock
      && (formValues.targetStoryBlockId === null
        || !this.getStoryBlockTargetStoryBlock(formValues.targetStoryBlockId))) {
      errors.targetStoryBlockId = 'Choose the story block this pin should link to.';
    }

    return errors;
  }

  private getConnectionFormValues(form: HTMLFormElement): ConnectionFormValues {
    const formData = new FormData(form);
    const rawDistanceValue = this.getStringValue(formData, 'distanceValue');
    const rawDistanceUnit = Number(formData.get('distanceUnit'));
    const hasDistanceValue = rawDistanceValue.length > 0;
    const distanceValue = hasDistanceValue
      ? Number(rawDistanceValue)
      : null;

    return {
      distanceValue,
      distanceUnit: hasDistanceValue && this.isMapPinConnectionDistanceUnit(rawDistanceUnit)
        ? rawDistanceUnit
        : null,
    };
  }

  private validateConnectionForm(formValues: ConnectionFormValues): ConnectionFormErrors {
    const errors: ConnectionFormErrors = {};

    if (formValues.distanceValue !== null
      && (!Number.isFinite(formValues.distanceValue)
        || formValues.distanceValue <= 0
        || formValues.distanceUnit === null)) {
      errors.distanceValue = 'Duration must be greater than zero and include a unit.';
    }

    return errors;
  }

  private toCreateConnectionRequest(
    pinIds: { pinAId: number; pinBId: number },
    formValues: ConnectionFormValues,
  ): CreateMapPinConnectionRequest {
    return {
      mapPinAId: pinIds.pinAId,
      mapPinBId: pinIds.pinBId,
      distanceValue: formValues.distanceValue === null
        ? null
        : this.toBackendCoordinatePrecision(formValues.distanceValue),
      distanceUnit: formValues.distanceUnit,
    };
  }

  private toCreatePlaceholderPinRequest(
    formValues: PlaceholderPinFormValues,
  ): CreateMapPinRequest {
    const coordinates = this.pendingPlaceholderPinCoordinates()
      ?? this.getVisibleImageCenterCoordinates();
    const targetType = this.placeholderPinFormTargetType();

    return {
      xCoordinate: this.toBackendCoordinatePrecision(coordinates.x),
      yCoordinate: this.toBackendCoordinatePrecision(coordinates.y),
      label: formValues.title,
      description: formValues.description,
      targetType,
      targetId: this.getPlaceholderPinTargetId(targetType, formValues),
    };
  }

  private openPlaceholderPinForm(
    targetType: MapPinTargetType,
    coordinates: { x: number; y: number } | null,
  ): void {
    if (targetType === MapPinTargetType.Store && this.stores().length === 0) {
      this.loadStores();
    }

    if (targetType === MapPinTargetType.StoryBlock) {
      this.loadStoryBlocks();
    }

    this.placeholderPinValidationErrors.set({});
    this.placeholderPinFormMode.set('create');
    this.placeholderPinFormTargetType.set(targetType);
    this.editingPinId.set(null);
    this.pendingPlaceholderPinCoordinates.set(coordinates);
    this.placeholderPinForm()?.nativeElement.reset();
    this.isPlaceholderPinFormOpen.set(true);
  }

  private addPinToolDragListeners(): void {
    document.addEventListener('pointermove', this.documentPinToolMoveListener);
    document.addEventListener('pointerup', this.documentPinToolUpListener);
    document.addEventListener('pointercancel', this.documentPinToolCancelListener);
  }

  private removePinToolDragListeners(): void {
    document.removeEventListener('pointermove', this.documentPinToolMoveListener);
    document.removeEventListener('pointerup', this.documentPinToolUpListener);
    document.removeEventListener('pointercancel', this.documentPinToolCancelListener);
  }

  private getVisibleImageCenterCoordinates(): { x: number; y: number } {
    const containerSize = this.getMapViewportContainerSize();
    const imageSize = this.getCoordinateImageSize();

    if (!containerSize || !imageSize || imageSize.width <= 0 || imageSize.height <= 0) {
      return { x: 0, y: 0 };
    }

    const center = getMapCoordinateAtViewportPoint(
      this.mapViewport(),
      {
        x: containerSize.width / 2,
        y: containerSize.height / 2,
      },
    );

    return {
      x: this.clampCoordinate(center.x, imageSize.width),
      y: this.clampCoordinate(center.y, imageSize.height),
    };
  }

  private startMovePointPlacement(event: PointerEvent): void {
    const movingPin = this.movingPin();

    if (!movingPin || event.button !== 0) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
    (event.currentTarget as HTMLElement).setPointerCapture(event.pointerId);
    this.movingPin.set({
      ...movingPin,
      pointerId: event.pointerId,
    });
    this.moveDraftPinToPointer(event);
  }

  private moveDraftPinToPointer(event: PointerEvent): void {
    const movingPin = this.movingPin();
    const coordinates = this.getImageCoordinatesFromPointer(event, true);

    if (!movingPin || !coordinates) {
      return;
    }

    this.movingPin.set({
      ...movingPin,
      x: coordinates.x,
      y: coordinates.y,
    });
  }

  private getRenderedPinX(pin: MapPinDetailsModel): number {
    const movingPin = this.movingPin();

    return movingPin?.pinId === pin.id
      ? movingPin.x
      : Number(pin.xCoordinate);
  }

  private getRenderedPinY(pin: MapPinDetailsModel): number {
    const movingPin = this.movingPin();

    return movingPin?.pinId === pin.id
      ? movingPin.y
      : Number(pin.yCoordinate);
  }

  private getPinTargetMapId(pin: MapPinDetailsModel): number | null {
    if (this.getPinTargetTypeValue(pin.targetType) !== MapPinTargetType.Map || !pin.targetId) {
      return null;
    }

    const targetMapId = Number(pin.targetId);

    return Number.isInteger(targetMapId)
      ? targetMapId
      : null;
  }

  private getPinTargetStoreId(pin: MapPinDetailsModel): number | null {
    if (this.getPinTargetTypeValue(pin.targetType) !== MapPinTargetType.Store || !pin.targetId) {
      return null;
    }

    const targetStoreId = Number(pin.targetId);

    return Number.isInteger(targetStoreId)
      ? targetStoreId
      : null;
  }

  private openPinTargetStore(targetStoreId: number): void {
    const campaignId = this.campaignId();
    const mapId = this.mapId();

    if (!campaignId || mapId === null) {
      return;
    }

    void this.router.navigate(
      ['/campaigns', campaignId, 'campaign-content', 'campaign-stores', targetStoreId],
      { queryParams: { fromMapId: mapId } },
    );
  }

  private movePinToPointer(pinId: number, event: PointerEvent): void {
    const coordinates = this.getImageCoordinatesFromPointer(event, true);

    if (!coordinates) {
      return;
    }

    this.mapPins.update((pins) => pins.map((pin) => (
      pin.id === pinId
        ? {
          ...pin,
          xCoordinate: coordinates.x,
          yCoordinate: coordinates.y,
        }
        : pin
    )));
  }

  private getImageCoordinatesFromPointer(
    event: PointerEvent,
    clampToImage = false,
  ): { x: number; y: number } | null {
    const imageSize = this.getCoordinateImageSize();
    const viewportPoint = this.getViewportPointFromClientPoint(event.clientX, event.clientY);

    if (!viewportPoint || !imageSize || imageSize.width <= 0 || imageSize.height <= 0) {
      return null;
    }

    const mapPoint = getMapCoordinateAtViewportPoint(this.mapViewport(), viewportPoint);
    const relativeX = mapPoint.x / imageSize.width;
    const relativeY = mapPoint.y / imageSize.height;

    if (!clampToImage && (relativeX < 0 || relativeX > 1 || relativeY < 0 || relativeY > 1)) {
      return null;
    }

    return {
      x: this.clampCoordinate(relativeX * imageSize.width, imageSize.width),
      y: this.clampCoordinate(relativeY * imageSize.height, imageSize.height),
    };
  }

  private clampCoordinate(value: number, maximumValue: number): number {
    return Math.max(0, Math.min(maximumValue, this.toBackendCoordinatePrecision(value)));
  }

  private toBackendCoordinatePrecision(value: number): number {
    return Math.round(value * 10000) / 10000;
  }

  private getConnectionPinCoordinate(pinId: number, axis: 'x' | 'y'): number {
    const pin = this.mapPins().find((item) => item.id === pinId);

    if (!pin) {
      return 0;
    }

    return axis === 'x'
      ? Number(pin.xCoordinate)
      : Number(pin.yCoordinate);
  }

  private getConnectionDistanceUnitLabel(
    distanceUnit: MapPinConnectionModel['distanceUnit'],
  ): string {
    const unitValue = this.getConnectionDistanceUnitValue(distanceUnit);

    switch (unitValue) {
      case MapPinConnectionDistanceUnit.Minutes:
        return 'minutes';
      case MapPinConnectionDistanceUnit.Hours:
        return 'hours';
      case MapPinConnectionDistanceUnit.Days:
        return 'days';
      case MapPinConnectionDistanceUnit.Weeks:
        return 'weeks';
      default:
        return '';
    }
  }

  private getEditingConnection(): MapPinConnectionModel | null {
    const editingConnectionId = this.editingConnectionId();

    return editingConnectionId === null
      ? null
      : this.mapPinConnections().find((connection) => connection.id === editingConnectionId) ?? null;
  }

  private persistPinPosition(pinId: number): void {
    const campaignId = this.campaignId();
    const mapId = this.mapId();
    const pin = this.mapPins().find((item) => item.id === pinId);

    if (!campaignId || mapId === null || !pin) {
      return;
    }

    this.campaignApiService.updateCampaignMapPin(
      campaignId,
      mapId,
      pin.id,
      this.toUpdateMapPinRequest(pin),
    ).subscribe({
      next: (response) => {
        if (!response.data) {
          return;
        }

        this.mapPins.update((pins) => pins.map((item) => (
          item.id === response.data!.id
            ? {
              ...response.data!,
              targetData: item.targetId === response.data!.targetId
                ? item.targetData
                : null,
            }
            : item
        )));
      },
      error: (error: unknown) => {
        this.modalHelper.showError(this.getErrorMessage(error, 'Map pin could not be moved.'), {
          statusCode: this.getErrorStatusCode(error),
        });
        this.loadPins(true);
      },
    });
  }

  private updatePlaceholderPin(formValues: PlaceholderPinFormValues): void {
    const campaignId = this.campaignId();
    const mapId = this.mapId();
    const pinId = this.editingPinId();
    const pin = pinId === null
      ? null
      : this.mapPins().find((item) => item.id === pinId) ?? null;

    if (!campaignId || mapId === null || !pin) {
      return;
    }

    this.isCreatingPlaceholderPin.set(true);
    const targetType = this.placeholderPinFormTargetType();
    this.campaignApiService.updateCampaignMapPin(
      campaignId,
      mapId,
      pin.id,
      {
        ...this.toUpdateMapPinRequest(pin),
        label: formValues.title,
        description: formValues.description,
        targetType,
        targetId: this.getPlaceholderPinTargetId(targetType, formValues),
      },
    ).subscribe({
      next: (response) => {
        this.isCreatingPlaceholderPin.set(false);
        this.closePlaceholderPinForm();

        if (!response.data) {
          this.loadPins(true);
          return;
        }

        this.mapPins.update((pins) => pins.map((item) => (
          item.id === response.data!.id
            ? {
              ...response.data!,
              targetData: item.targetId === response.data!.targetId
                ? item.targetData
                : null,
            }
            : item
        )));
        this.loadStoryBlocks();
        this.modalHelper.showSuccess(response.message, {
          statusCode: response.statusCode,
        });
      },
      error: (error: unknown) => {
        this.isCreatingPlaceholderPin.set(false);
        this.modalHelper.showError(this.getErrorMessage(error, 'Map pin could not be updated.'), {
          statusCode: this.getErrorStatusCode(error),
        });
      },
    });
  }

  private toUpdateMapPinRequest(pin: MapPinDetailsModel): UpdateMapPinRequest {
    return {
      xCoordinate: Number(pin.xCoordinate),
      yCoordinate: Number(pin.yCoordinate),
      label: pin.label,
      description: pin.description,
      targetType: this.getPinTargetTypeValue(pin.targetType) as MapPinTargetType,
      targetId: pin.targetId,
    };
  }

  private getCoordinateImageSize(): ImageNaturalSize | null {
    const selectedMap = this.selectedMap();

    if (selectedMap && selectedMap.imageWidthPixels > 0 && selectedMap.imageHeightPixels > 0) {
      return {
        width: selectedMap.imageWidthPixels,
        height: selectedMap.imageHeightPixels,
      };
    }

    return this.imageNaturalSize();
  }

  private getStringValue(formData: FormData, key: string): string {
    const value = formData.get(key);

    return typeof value === 'string' ? value.trim() : '';
  }

  private normalizeText(value: string | null | undefined): string {
    return value?.trim() ?? '';
  }

  private getRouteMapId(): number | null {
    return this.normalizeRouteMapId(this.route.snapshot.paramMap.get('mapId'));
  }

  private normalizeRouteMapId(value: string | null): number | null {
    const mapId = Number(value);

    return Number.isFinite(mapId) ? mapId : null;
  }

  private getContextPin(): MapPinDetailsModel | null {
    const menu = this.pinContextMenu();

    return menu
      ? this.mapPins().find((pin) => pin.id === menu.pinId) ?? null
      : null;
  }

  private getContextPinTargetMapId(): number | null {
    const pin = this.getContextPin();

    return pin ? this.getPinTargetMapId(pin) : null;
  }

  private getContextPinTargetStoreId(): number | null {
    const pin = this.getContextPin();

    return pin ? this.getPinTargetStoreId(pin) : null;
  }

  private getPinTargetTypeValue(
    targetType: MapPinDetailsModel['targetType'],
  ): number {
    if (typeof targetType === 'number') {
      return targetType;
    }

    if (typeof targetType === 'string') {
      const numericTargetType = Number(targetType);

      if (Number.isFinite(numericTargetType)) {
        return numericTargetType;
      }

      return MapPinTargetType[targetType as keyof typeof MapPinTargetType] ?? -1;
    }

    return -1;
  }

  private getConnectionDistanceUnitValue(
    distanceUnit: MapPinConnectionModel['distanceUnit'],
  ): number {
    if (typeof distanceUnit === 'number') {
      return distanceUnit;
    }

    if (typeof distanceUnit === 'string') {
      const numericDistanceUnit = Number(distanceUnit);

      if (Number.isFinite(numericDistanceUnit)) {
        return numericDistanceUnit;
      }

      return MapPinConnectionDistanceUnit[distanceUnit as keyof typeof MapPinConnectionDistanceUnit] ?? -1;
    }

    return -1;
  }

  private isMapPinConnectionDistanceUnit(value: number): value is MapPinConnectionDistanceUnit {
    return Object.values(MapPinConnectionDistanceUnit)
      .filter((unit): unit is number => typeof unit === 'number')
      .includes(value);
  }

  private restoreMapViewport(): void {
    const cacheKey = this.getMapViewportCacheKey();
    const containerSize = this.getMapViewportContainerSize();
    const mapSize = this.getMapViewportMapSize();

    if (!cacheKey || !containerSize || !mapSize) {
      return;
    }

    const viewport = this.browserCache.get<unknown>(cacheKey);

    if (!this.isMapViewportCacheEntry(viewport)) {
      this.resetMapViewport();
      this.finishViewportRestore();
      return;
    }

    this.mapViewport.set(clampMapViewport(
      viewport,
      containerSize,
      mapSize,
    ));

    this.scheduleViewportRestoreFinish();
  }

  private isMapViewportCacheEntry(value: unknown): value is MapViewportCacheEntry {
    return typeof value === 'object'
      && value !== null
      && 'scale' in value
      && 'translateX' in value
      && 'translateY' in value
      && typeof value.scale === 'number'
      && Number.isFinite(value.scale)
      && value.scale > 0
      && typeof value.translateX === 'number'
      && Number.isFinite(value.translateX)
      && typeof value.translateY === 'number'
      && Number.isFinite(value.translateY);
  }

  private scheduleViewportRestoreFinish(): void {
    if (this.restoreUnlockTimer) {
      clearTimeout(this.restoreUnlockTimer);
    }

    this.restoreUnlockTimer = setTimeout(() => this.finishViewportRestore(), 250);
  }

  private finishViewportRestore(): void {
    this.hasAttemptedViewportRestore = true;
    this.isRestoringViewport = false;
  }

  private getMapViewportCacheKey(): string | null {
    const campaignId = this.campaignId();
    const mapId = this.mapId();

    if (!campaignId || mapId === null) {
      return null;
    }

    return `campaigns.${campaignId}.maps.${mapId}.viewport`;
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return this.isApiError(error) ? error.message : fallback;
  }

  private getErrorStatusCode(error: unknown): number | undefined {
    return this.isApiError(error) ? error.status : undefined;
  }

  private isApiError(error: unknown): error is ApiError {
    return typeof error === 'object'
      && error !== null
      && 'status' in error
      && 'message' in error
      && typeof error.status === 'number'
      && typeof error.message === 'string';
  }
}
