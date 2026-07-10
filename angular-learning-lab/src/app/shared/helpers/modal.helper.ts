import {
  ApplicationRef,
  ComponentRef,
  EnvironmentInjector,
  Injectable,
  createComponent,
  inject,
} from '@angular/core';
import { DOCUMENT } from '@angular/common';

import {
  NotificationModal,
  NotificationModalType,
} from '../components/notification-modal/notification-modal';

type ModalMessages = string | string[];

interface ModalOptions {
  statusCode?: number;
  onClose?: () => void;
}

@Injectable({ providedIn: 'root' })
export class ModalHelper {
  private readonly applicationRef = inject(ApplicationRef);
  private readonly environmentInjector = inject(EnvironmentInjector);
  private readonly document = inject(DOCUMENT);
  private currentModal?: ComponentRef<NotificationModal>;

  showError(messages: ModalMessages, options: ModalOptions = {}): void {
    this.show('error', messages, options);
  }

  showSuccess(messages: ModalMessages, options: ModalOptions = {}): void {
    this.show('success', messages, options);
  }

  private show(
    type: NotificationModalType,
    messages: ModalMessages,
    options: ModalOptions,
  ): void {
    this.closeCurrentModal();

    const modalRef = createComponent(NotificationModal, {
      environmentInjector: this.environmentInjector,
    });

    modalRef.setInput('type', type);
    modalRef.setInput('messages', Array.isArray(messages) ? messages : [messages]);
    modalRef.setInput('statusCode', options.statusCode);

    const subscription = modalRef.instance.closed.subscribe(() => {
      subscription.unsubscribe();
      this.closeModal(modalRef);
      options.onClose?.();
    });

    this.applicationRef.attachView(modalRef.hostView);
    this.document.body.appendChild(modalRef.location.nativeElement);
    this.currentModal = modalRef;
  }

  private closeCurrentModal(): void {
    if (!this.currentModal) {
      return;
    }

    this.closeModal(this.currentModal);
  }

  private closeModal(modalRef: ComponentRef<NotificationModal>): void {
    this.applicationRef.detachView(modalRef.hostView);
    modalRef.destroy();

    if (this.currentModal === modalRef) {
      this.currentModal = undefined;
    }
  }
}
