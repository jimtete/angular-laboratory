import {
  AfterViewInit,
  Component,
  ElementRef,
  HostListener,
  OnDestroy,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LucideBell } from '@lucide/angular';

import {
  ApiLoadingService,
  AuthApiService,
  NotificationCacheService,
  NotificationModel,
  NotificationTypes,
  NotificationSocketService,
  TokenStorageService,
} from './Infrastructure';
import { ASSET_PATHS } from './shared/constants/asset-paths';
import { NotificationCard } from './shared/components/notification-card/notification-card';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet, LucideBell, NotificationCard],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements AfterViewInit, OnDestroy {
  private readonly appPage = viewChild<ElementRef<HTMLElement>>('appPage');
  private readonly appBackground = viewChild<ElementRef<HTMLImageElement>>('appBackground');
  private readonly elementRef = inject(ElementRef<HTMLElement>);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly authApiService = inject(AuthApiService);
  private readonly apiLoadingService = inject(ApiLoadingService);
  private readonly notificationCache = inject(NotificationCacheService);
  private readonly notificationSocket = inject(NotificationSocketService);
  private readonly router = inject(Router);
  private resizeObserver: ResizeObserver | undefined;
  private animationFrameId: number | undefined;

  protected readonly assets = ASSET_PATHS;
  protected readonly isMenuOpen = signal(false);
  protected readonly isNotificationsOpen = signal(false);
  protected readonly notifications = computed(() => this.notificationCache.notifications());
  protected readonly notificationCount = computed(() => this.notifications().length);
  protected readonly notificationBadgeText = computed(() => {
    const notificationCount = this.notificationCount();

    return notificationCount > 9 ? '9+' : notificationCount.toString();
  });
  protected readonly isApiLoading = computed(() => this.apiLoadingService.isLoading());
  protected readonly hasValidAccessToken = computed(() => this.tokenStorage.hasValidAccessToken());
  protected readonly hasCampaignAccess = computed(() =>
    this.tokenStorage.hasAnyRole('Master', 'Player'),
  );
  protected readonly isMaster = computed(() => this.tokenStorage.hasAnyRole('Master'));

  ngAfterViewInit(): void {
    window.addEventListener('scroll', this.scheduleBackgroundUpdate, { passive: true });
    window.addEventListener('resize', this.scheduleBackgroundUpdate, { passive: true });

    this.resizeObserver = new ResizeObserver(this.scheduleBackgroundUpdate);

    const page = this.appPage()?.nativeElement;
    const background = this.appBackground()?.nativeElement;

    if (page) {
      this.resizeObserver.observe(page);
    }

    if (background) {
      this.resizeObserver.observe(background);
    }

    this.scheduleBackgroundUpdate();
  }

  ngOnDestroy(): void {
    window.removeEventListener('scroll', this.scheduleBackgroundUpdate);
    window.removeEventListener('resize', this.scheduleBackgroundUpdate);
    this.resizeObserver?.disconnect();

    if (this.animationFrameId !== undefined) {
      cancelAnimationFrame(this.animationFrameId);
    }
  }

  @HostListener('document:click', ['$event'])
  protected closeNotificationsOnOutsideClick(event: MouseEvent): void {
    const target = event.target;

    if (
      !this.isNotificationsOpen() ||
      !(target instanceof Node) ||
      this.elementRef.nativeElement
        .querySelector('.notification-control')
        ?.contains(target)
    ) {
      return;
    }

    this.isNotificationsOpen.set(false);
  }

  protected toggleMenu(): void {
    this.isMenuOpen.update((isOpen) => !isOpen);
  }

  protected openNotificationsOnHover(): void {
    if (this.notificationCount() > 0) {
      this.isNotificationsOpen.set(true);
    }
  }

  protected toggleNotifications(): void {
    if (this.notificationCount() === 0) {
      return;
    }

    this.isNotificationsOpen.update((isOpen) => !isOpen);
  }

  protected closeNotifications(): void {
    this.isNotificationsOpen.set(false);
  }

  protected refreshGlobalBackground(): void {
    this.scheduleBackgroundUpdate();
  }

  protected selectNotification(notification: NotificationModel): void {
    this.closeNotifications();

    if (this.isCampaignInviteNotification(notification)) {
      void this.router.navigate(['/my-campaigns/invites']);
    }
  }

  protected logout(): void {
    this.authApiService.logout();
    this.isMenuOpen.set(false);
    this.isNotificationsOpen.set(false);
    void this.router.navigate(['/home']);
  }

  private isCampaignInviteNotification(notification: NotificationModel): boolean {
    return (
      notification.notificationType === NotificationTypes.CampaignInvite ||
      notification.notificationType === 'CampaignInvite' ||
      notification.notificationType === '1'
    );
  }

  private readonly scheduleBackgroundUpdate = (): void => {
    if (this.animationFrameId !== undefined) {
      return;
    }

    this.animationFrameId = requestAnimationFrame(() => {
      this.animationFrameId = undefined;
      this.updateGlobalBackground();
    });
  };

  private updateGlobalBackground(): void {
    const page = this.appPage()?.nativeElement;
    const background = this.appBackground()?.nativeElement;

    if (!page || !background || background.naturalHeight === 0) {
      return;
    }

    const navHeight = this.getNavHeight(page);
    const viewportHeight = Math.max(0, window.innerHeight - navHeight);
    const documentHeight = Math.max(
      page.offsetHeight + navHeight,
      document.documentElement.scrollHeight,
      document.body.scrollHeight,
    );
    const scrollDistance = Math.max(1, documentHeight - window.innerHeight);
    const scrollProgress = Math.min(1, Math.max(0, window.scrollY / scrollDistance));
    const translationRange = viewportHeight - background.offsetHeight;
    const translateY = translationRange * scrollProgress;

    background.style.transform = `translate3d(0, ${translateY}px, 0)`;
  }

  private getNavHeight(page: HTMLElement): number {
    const navHeight = Number.parseFloat(
      getComputedStyle(page).getPropertyValue('--nav-height'),
    );

    return Number.isFinite(navHeight) ? navHeight : 64;
  }
}
