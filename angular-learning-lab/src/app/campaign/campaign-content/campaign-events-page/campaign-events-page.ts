import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { finalize, forkJoin, map, of, switchMap } from 'rxjs';

import {
  ApiError,
  CampaignEventModel,
  CampaignEventOptionModel,
  CampaignEventOptionRequest,
  CampaignEventRequest,
  CampaignEventType,
  CampaignEventsApiService,
} from '../../../Infrastructure';
import { ModalHelper } from '../../../shared/helpers/modal.helper';

interface CampaignEventOptionDraft {
  draftId: number;
  id: string | null;
  key: string;
  label: string;
  description: string;
}

@Component({
  selector: 'app-campaign-events-page',
  imports: [FormsModule],
  templateUrl: './campaign-events-page.html',
  styleUrl: './campaign-events-page.css',
})
export class CampaignEventsPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly campaignEventsApiService = inject(CampaignEventsApiService);
  private readonly modalHelper = inject(ModalHelper);

  protected readonly events = signal<CampaignEventModel[]>([]);
  protected readonly CampaignEventType = CampaignEventType;
  protected readonly isLoading = signal(false);
  protected readonly isSaving = signal(false);
  protected readonly deletingEventId = signal<string | null>(null);
  protected readonly editingEventId = signal<string | null>(null);
  protected readonly nameDraft = signal('');
  protected readonly keyDraft = signal('');
  protected readonly descriptionDraft = signal('');
  protected readonly typeDraft = signal<CampaignEventType>(CampaignEventType.BooleanFlag);
  protected readonly optionDrafts = signal<CampaignEventOptionDraft[]>([]);
  private nextOptionDraftId = 1;
  protected readonly eventTypes = [
    { value: CampaignEventType.BooleanFlag, label: 'Boolean Flag' },
    { value: CampaignEventType.SingleChoice, label: 'Single Choice' },
    { value: CampaignEventType.NumericValue, label: 'Numeric' },
    { value: CampaignEventType.TextValue, label: 'Text' },
  ];
  protected readonly sortedEvents = computed(() => (
    [...this.events()].sort((first, second) => (
      first.name.localeCompare(second.name) || first.key.localeCompare(second.key)
    ))
  ));
  protected readonly canSave = computed(() => (
    this.normalizeText(this.nameDraft()).length > 0 &&
    this.normalizeText(this.keyDraft()).length > 0 &&
    this.hasValidOptionDrafts() &&
    !this.isSaving()
  ));

  ngOnInit(): void {
    this.loadEvents();
  }

  refreshCampaignPage(): boolean {
    this.loadEvents();
    return true;
  }

  isRefreshingCampaignPage(): boolean {
    return this.isLoading();
  }

  protected startCreate(): void {
    this.editingEventId.set(null);
    this.nameDraft.set('');
    this.keyDraft.set('');
    this.descriptionDraft.set('');
    this.typeDraft.set(CampaignEventType.BooleanFlag);
    this.optionDrafts.set([]);
  }

  protected startEdit(event: CampaignEventModel): void {
    this.editingEventId.set(event.id);
    this.nameDraft.set(event.name);
    this.keyDraft.set(event.key);
    this.descriptionDraft.set(event.description ?? '');
    this.typeDraft.set(this.toCampaignEventType(event.eventType ?? event.type));
    this.optionDrafts.set(this.toOptionDrafts(event.options));
  }

  protected saveEvent(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || !this.canSave()) {
      return;
    }

    const request = this.toRequest();
    const editingEventId = this.editingEventId();
    const saveRequest = editingEventId
      ? this.campaignEventsApiService.updateCampaignEvent(campaignId, editingEventId, request)
      : this.campaignEventsApiService.createCampaignEvent(campaignId, request);

    this.isSaving.set(true);
    saveRequest
      .pipe(
        switchMap((response) => {
          const savedEvent = response.data;

          if (!savedEvent) {
            return of(response);
          }

          return this.syncEventOptions(campaignId, savedEvent).pipe(
            map((options) => ({
              ...response,
              data: {
                ...savedEvent,
                options,
              },
            })),
          );
        }),
      )
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (response) => {
          const savedEvent = response.data;

          if (savedEvent) {
            this.events.update((events) => {
              const others = events.filter((event) => event.id !== savedEvent.id);
              return [...others, savedEvent];
            });
          }

          this.startCreate();
          this.modalHelper.showSuccess(response.message);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign event could not be saved.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected deleteEvent(event: CampaignEventModel): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.deletingEventId()) {
      return;
    }

    this.deletingEventId.set(event.id);
    this.campaignEventsApiService
      .deleteCampaignEvent(campaignId, event.id)
      .pipe(finalize(() => this.deletingEventId.set(null)))
      .subscribe({
        next: (response) => {
          this.events.update((events) => events.filter((candidate) => candidate.id !== event.id));
          this.modalHelper.showSuccess(response.message);

          if (this.editingEventId() === event.id) {
            this.startCreate();
          }
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign event could not be deleted.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected setNameDraft(value: string): void {
    this.nameDraft.set(value);

    if (!this.editingEventId()) {
      this.keyDraft.set(this.toEventKey(value));
    }
  }

  protected setTypeDraft(value: CampaignEventType): void {
    this.typeDraft.set(value);

    if (value === CampaignEventType.SingleChoice && this.optionDrafts().length === 0) {
      this.optionDrafts.set([
        this.createOptionDraft(),
        this.createOptionDraft(),
      ]);
    }
  }

  protected isSingleChoiceDraft(): boolean {
    return this.typeDraft() === CampaignEventType.SingleChoice;
  }

  protected addOptionDraft(): void {
    this.optionDrafts.update((drafts) => [
      ...drafts,
      this.createOptionDraft(),
    ]);
  }

  protected removeOptionDraft(draftId: number): void {
    this.optionDrafts.update((drafts) => (
      drafts.filter((draft) => draft.draftId !== draftId)
    ));
  }

  protected setOptionLabelDraft(draftId: number, value: string): void {
    this.optionDrafts.update((drafts) => drafts.map((draft) => {
      if (draft.draftId !== draftId) {
        return draft;
      }

      const label = value;
      const key = this.normalizeText(draft.key).length > 0
        ? draft.key
        : this.toEventKey(label);

      return {
        ...draft,
        label,
        key,
      };
    }));
  }

  protected setOptionKeyDraft(draftId: number, value: string): void {
    this.optionDrafts.update((drafts) => drafts.map((draft) => (
      draft.draftId === draftId ? { ...draft, key: value } : draft
    )));
  }

  protected setOptionDescriptionDraft(draftId: number, value: string): void {
    this.optionDrafts.update((drafts) => drafts.map((draft) => (
      draft.draftId === draftId ? { ...draft, description: value } : draft
    )));
  }

  protected typeLabel(event: CampaignEventModel): string {
    return this.eventTypes.find((type) => (
      type.value === this.toCampaignEventType(event.eventType ?? event.type)
    ))?.label ?? 'Event';
  }

  private loadEvents(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoading()) {
      return;
    }

    this.isLoading.set(true);
    this.campaignEventsApiService
      .fetchCampaignEvents(campaignId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          this.events.set(response.data ?? []);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign events could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private toRequest(): CampaignEventRequest {
    return {
      name: this.normalizeText(this.nameDraft()),
      key: this.normalizeText(this.keyDraft()),
      description: this.normalizeNullableText(this.descriptionDraft()),
      eventType: this.typeDraft(),
      isRepeatable: false,
    };
  }

  private syncEventOptions(
    campaignId: string,
    event: CampaignEventModel,
  ) {
    if (this.toCampaignEventType(event.eventType ?? event.type) !== CampaignEventType.SingleChoice) {
      return of(event.options ?? []);
    }

    const drafts = this.optionDrafts();
    const existingOptions = event.options ?? [];
    const draftIds = new Set(
      drafts
        .map((draft) => draft.id)
        .filter((id): id is string => id !== null),
    );
    const deleteRequests = existingOptions
      .filter((option) => !draftIds.has(option.id))
      .map((option) => this.campaignEventsApiService.deleteCampaignEventOption(
        campaignId,
        event.id,
        option.id,
      ));
    const saveRequests = drafts.map((draft, index) => {
      const request = this.toOptionRequest(draft, index + 1);

      return draft.id
        ? this.campaignEventsApiService.updateCampaignEventOption(campaignId, event.id, draft.id, request)
        : this.campaignEventsApiService.createCampaignEventOption(campaignId, event.id, request);
    });
    const deleteRequest = deleteRequests.length > 0 ? forkJoin(deleteRequests) : of([]);
    const saveRequest = saveRequests.length > 0
      ? forkJoin(saveRequests).pipe(map((responses) => responses
        .map((response) => response.data)
        .filter((option): option is CampaignEventOptionModel => option !== null)))
      : of([] as CampaignEventOptionModel[]);

    return deleteRequest.pipe(switchMap(() => saveRequest));
  }

  private toOptionRequest(
    draft: CampaignEventOptionDraft,
    sortOrder: number,
  ): CampaignEventOptionRequest {
    return {
      key: this.normalizeText(draft.key),
      label: this.normalizeText(draft.label),
      description: this.normalizeNullableText(draft.description),
      sortOrder,
    };
  }

  private toOptionDrafts(options: CampaignEventOptionModel[] | undefined): CampaignEventOptionDraft[] {
    return [...(options ?? [])]
      .sort((first, second) => first.sortOrder - second.sortOrder || first.label.localeCompare(second.label))
      .map((option) => ({
        draftId: this.nextOptionDraftId++,
        id: option.id,
        key: option.key,
        label: option.label,
        description: option.description ?? '',
      }));
  }

  private createOptionDraft(): CampaignEventOptionDraft {
    return {
      draftId: this.nextOptionDraftId++,
      id: null,
      key: '',
      label: '',
      description: '',
    };
  }

  private hasValidOptionDrafts(): boolean {
    if (!this.isSingleChoiceDraft()) {
      return true;
    }

    const drafts = this.optionDrafts();
    const keys = drafts.map((draft) => this.normalizeText(draft.key).toLowerCase());

    return drafts.length > 0 &&
      drafts.every((draft) => (
        this.normalizeText(draft.label).length > 0 &&
        this.normalizeText(draft.key).length > 0
      )) &&
      new Set(keys).size === keys.length;
  }

  protected toCampaignEventType(value: CampaignEventModel['eventType'] | CampaignEventModel['type']): CampaignEventType {
    if (typeof value === 'number') {
      return value as CampaignEventType;
    }

    const parsedValue = Number(value);

    if (Number.isFinite(parsedValue)) {
      return parsedValue as CampaignEventType;
    }

    return CampaignEventType[value as keyof typeof CampaignEventType] ?? CampaignEventType.BooleanFlag;
  }

  private toEventKey(value: string): string {
    return this.normalizeText(value)
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '_')
      .replace(/^_+|_+$/g, '');
  }

  private getCampaignId(): string | null {
    return this.route.parent?.snapshot.paramMap.get('campaignId') ?? null;
  }

  private normalizeNullableText(value: string): string | null {
    const normalizedValue = this.normalizeText(value);

    return normalizedValue.length > 0 ? normalizedValue : null;
  }

  private normalizeText(value: string | null | undefined): string {
    return (value ?? '').trim();
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return this.isApiError(error) ? error.message : fallback;
  }

  private getErrorStatus(error: unknown): number | undefined {
    return this.isApiError(error) ? error.status : undefined;
  }

  private isApiError(error: unknown): error is ApiError {
    return (
      typeof error === 'object' &&
      error !== null &&
      'message' in error &&
      typeof error.message === 'string' &&
      'status' in error &&
      typeof error.status === 'number'
    );
  }
}
