import { Component, EventEmitter, Input, Output, signal } from '@angular/core';

export type NotificationModalType = 'error' | 'success' | 'information';

@Component({
  selector: 'app-notification-modal',
  templateUrl: './notification-modal.html',
  styleUrl: './notification-modal.css',
})
export class NotificationModal {
  private readonly dismissAnimationDurationMs = 200;

  @Input() type: NotificationModalType = 'error';
  @Input() messages: string[] = [];
  @Input() statusCode?: number;

  @Output() closed = new EventEmitter<void>();

  protected readonly isDismissing = signal(false);

  protected dismiss(): void {
    if (this.isDismissing()) {
      return;
    }

    this.isDismissing.set(true);

    window.setTimeout(() => {
      this.closed.emit();
    }, this.dismissAnimationDurationMs);
  }
}
