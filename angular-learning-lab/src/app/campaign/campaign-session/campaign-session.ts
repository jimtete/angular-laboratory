import { Component, HostListener, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LucideCalendarDays, LucidePackage } from '@lucide/angular';
import { finalize } from 'rxjs';

import {
  ApiError,
  AchieveCampaignMilestoneRequest,
  AssetItemType,
  AssetModel,
  CampaignApiService,
  CampaignMilestoneModel,
  CampaignSessionModel,
  CampaignSessionSocketService,
  ImportantChoiceSessionNoteRequest,
  PendingChangesComponent,
  SessionNoteChoiceModel,
  SessionNoteModel,
  SessionNoteType,
} from '../../Infrastructure';
import { ModalHelper } from '../../shared/helpers/modal.helper';

interface NoteContextMenuState {
  note: SessionNoteModel;
  x: number;
  y: number;
}

interface ImportantChoiceDraft {
  draftId: number;
  choiceText: string;
  isChosen: boolean;
}

@Component({
  selector: 'app-campaign-session',
  imports: [LucideCalendarDays, LucidePackage],
  templateUrl: './campaign-session.html',
  styleUrl: './campaign-session.css',
})
export class CampaignSession implements OnInit, OnDestroy, PendingChangesComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly campaignApiService = inject(CampaignApiService);
  private readonly campaignSessionSocket = inject(CampaignSessionSocketService);
  private readonly modalHelper = inject(ModalHelper);
  private pendingDeactivateResolution: ((canDeactivate: boolean) => void) | null = null;

  protected readonly sessions = signal<CampaignSessionModel[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isSavingDate = signal(false);
  protected readonly isSavingDescription = signal(false);
  protected readonly isSavingNote = signal(false);
  protected readonly isDeletingNote = signal(false);
  protected readonly isLoadingAvailableMilestones = signal(false);
  protected readonly isLoadingAvailableItems = signal(false);
  protected readonly isDatePickerOpen = signal(false);
  protected readonly selectedDate = signal('');
  protected readonly isNoteTypeDialogOpen = signal(false);
  protected readonly isNoteEditorOpen = signal(false);
  protected readonly selectedNoteType = signal<SessionNoteType | null>(null);
  protected readonly noteDraft = signal('');
  protected readonly availableMilestones = signal<CampaignMilestoneModel[]>([]);
  protected readonly availableItems = signal<AssetModel[]>([]);
  protected readonly selectedMilestoneId = signal<number | null>(null);
  protected readonly selectedItemFoundAssetId = signal<number | null>(null);
  protected readonly importantChoiceDrafts = signal<ImportantChoiceDraft[]>([]);
  protected readonly descriptionDraft = signal('');
  protected readonly savedDescription = signal('');
  protected readonly isUnsavedChangesDialogOpen = signal(false);
  protected readonly noteContextMenu = signal<NoteContextMenuState | null>(null);
  protected readonly editingNote = signal<SessionNoteModel | null>(null);
  protected readonly deleteConfirmationNote = signal<SessionNoteModel | null>(null);
  private allowNativeContextMenuNoteId: number | null = null;
  protected readonly sessionNumber = computed(() => {
    const sessionNumber = Number(this.route.snapshot.paramMap.get('sessionNumber'));

    return Number.isInteger(sessionNumber) && sessionNumber > 0 ? sessionNumber : null;
  });
  protected readonly session = computed(() => {
    const sessionNumber = this.sessionNumber();

    return this.sessions().find((session) => session.sessionNumber === sessionNumber) ?? null;
  });
  protected readonly orderedNotes = computed(() => this.orderNotes(this.campaignSessionSocket.sessionNotes()));
  protected readonly headerText = computed(() => {
    const session = this.session();

    if (!session) {
      return 'Session';
    }

    const formattedDate = this.formatDisplayDate(session.sessionDate);

    return formattedDate
      ? `Session ${session.sessionNumber} - ${formattedDate}`
      : `Session ${session.sessionNumber}`;
  });
  protected readonly hasUnsavedDescriptionChanges = computed(() => (
    this.normalizeDescription(this.descriptionDraft()) !==
    this.normalizeDescription(this.savedDescription())
  ));
  protected readonly canAddNote = computed(() => (
    this.normalizeDescription(this.noteDraft()).length > 0 &&
    !this.isSavingNote() &&
    (
      !this.isImportantChoiceEditor() ||
      this.importantChoiceDrafts()
        .filter((choice) => this.normalizeDescription(choice.choiceText).length > 0)
        .length >= 2
    ) &&
    (
      !this.isCampaignMilestonePicker() ||
      this.selectedMilestoneId() !== null
    ) &&
    (
      !this.isItemFoundPicker() ||
      this.selectedItemFoundAssetId() !== null
    )
  ));
  protected readonly isImportantChoiceEditor = computed(() => (
    this.selectedNoteType() === SessionNoteType.ImportantChoice
  ));
  protected readonly isCampaignMilestoneEditor = computed(() => (
    this.selectedNoteType() === SessionNoteType.CampaignMilestone
  ));
  protected readonly isCampaignMilestonePicker = computed(() => (
    this.isCampaignMilestoneEditor() && !this.editingNote()
  ));
  protected readonly isItemFoundEditor = computed(() => (
    this.selectedNoteType() === SessionNoteType.ItemFound
  ));
  protected readonly isItemFoundPicker = computed(() => (
    this.isItemFoundEditor() && !this.editingNote()
  ));
  protected readonly noteEditorActionText = computed(() => {
    if (this.isSavingNote()) {
      return this.editingNote() ? 'Saving...' : 'Adding...';
    }

    return this.editingNote() ? 'Save Note' : 'Add Note';
  });
  protected readonly noteTypeOptions = [
    {
      type: SessionNoteType.GeneralNotes,
      label: 'Generic Type',
    },
    {
      type: SessionNoteType.ImportantChoice,
      label: 'Important Choice',
    },
    {
      type: SessionNoteType.CampaignMilestone,
      label: 'Campaign Milestone',
    },
    {
      type: SessionNoteType.ItemFound,
      label: 'Item Found',
    },
    {
      type: SessionNoteType.LevelUpOrMechanicsChange,
      label: 'Level Up / Mechanics Change',
    },
  ];
  private nextChoiceDraftId = 1;

  ngOnInit(): void {
    this.loadSession();
  }

  ngOnDestroy(): void {
    void this.campaignSessionSocket.disconnect();
  }

  canDeactivate(): boolean | Promise<boolean> {
    if (!this.hasUnsavedDescriptionChanges()) {
      return true;
    }

    this.isUnsavedChangesDialogOpen.set(true);

    return new Promise((resolve) => {
      this.pendingDeactivateResolution = resolve;
    });
  }

  @HostListener('window:beforeunload', ['$event'])
  protected warnBeforeBrowserUnload(event: BeforeUnloadEvent): void {
    if (!this.hasUnsavedDescriptionChanges()) {
      return;
    }

    event.preventDefault();
    event.returnValue = '';
  }

  @HostListener('document:click')
  protected closeNoteContextMenu(): void {
    this.noteContextMenu.set(null);
  }

  @HostListener('document:keydown.escape')
  protected closeNoteContextMenuWithEscape(): void {
    this.noteContextMenu.set(null);
  }

  protected openDatePicker(): void {
    this.selectedDate.set(this.toDateInputValue(this.session()?.sessionDate));
    this.isDatePickerOpen.set(true);
  }

  protected setSelectedDate(event: Event): void {
    this.selectedDate.set((event.target as HTMLInputElement).value);
  }

  protected saveSelectedDate(): void {
    const campaignId = this.getCampaignId();
    const session = this.session();

    if (!campaignId || !session || this.isSavingDate()) {
      return;
    }

    this.isSavingDate.set(true);

    this.campaignSessionSocket
      .updateSessionDate(campaignId, session.id, this.toSessionDateValue(this.selectedDate()))
      .then((updatedSession) => {
        this.upsertSession(updatedSession ?? {
          ...session,
          sessionDate: this.toSessionDateValue(this.selectedDate()),
        });
        this.isDatePickerOpen.set(false);
      })
      .catch((error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Session date could not be saved.'),
          { statusCode: this.getErrorStatus(error) },
        );
      })
      .finally(() => this.isSavingDate.set(false));
  }

  protected setDescriptionDraft(event: Event): void {
    this.descriptionDraft.set((event.target as HTMLTextAreaElement).value);
  }

  protected saveDescriptionIfDirty(): void {
    if (this.hasUnsavedDescriptionChanges()) {
      void this.saveDescription();
    }
  }

  protected saveAndLeave(): void {
    void this.saveDescription().then((saved) => {
      if (saved) {
        this.resolvePendingDeactivate(true);
      }
    });
  }

  protected discardAndLeave(): void {
    this.descriptionDraft.set(this.savedDescription());
    this.resolvePendingDeactivate(true);
  }

  protected cancelLeave(): void {
    this.resolvePendingDeactivate(false);
  }

  protected openNoteTypeDialog(): void {
    this.isNoteTypeDialogOpen.set(true);
  }

  protected closeNoteTypeDialog(): void {
    this.isNoteTypeDialogOpen.set(false);
    this.isNoteEditorOpen.set(false);
    this.editingNote.set(null);
    this.noteDraft.set('');
    this.selectedMilestoneId.set(null);
    this.selectedItemFoundAssetId.set(null);
    this.availableMilestones.set([]);
    this.availableItems.set([]);
    this.importantChoiceDrafts.set([]);
  }

  protected selectNoteType(noteType: SessionNoteType): void {
    this.selectedNoteType.set(noteType);

    if (
      noteType === SessionNoteType.GeneralNotes ||
      noteType === SessionNoteType.ImportantChoice ||
      noteType === SessionNoteType.CampaignMilestone ||
      noteType === SessionNoteType.ItemFound
    ) {
      this.noteDraft.set('');
      this.selectedMilestoneId.set(null);
      this.selectedItemFoundAssetId.set(null);
      this.importantChoiceDrafts.set([]);
      this.isNoteEditorOpen.set(true);

      if (noteType === SessionNoteType.CampaignMilestone) {
        this.loadAvailableMilestones();
      }

      if (noteType === SessionNoteType.ItemFound) {
        this.loadAvailableItems();
      }

      return;
    }

    this.closeNoteTypeDialog();
  }

  protected setNoteDraft(event: Event): void {
    this.noteDraft.set((event.target as HTMLTextAreaElement).value);
  }

  protected isImportantChoiceNote(note: SessionNoteModel): boolean {
    return this.toSessionNoteType(note.type) === SessionNoteType.ImportantChoice;
  }

  protected isCampaignMilestoneNote(note: SessionNoteModel): boolean {
    return this.toSessionNoteType(note.type) === SessionNoteType.CampaignMilestone;
  }

  protected isItemFoundNote(note: SessionNoteModel): boolean {
    return this.toSessionNoteType(note.type) === SessionNoteType.ItemFound;
  }

  protected addImportantChoiceOption(): void {
    this.importantChoiceDrafts.update((choices) => [
      ...choices,
      {
        draftId: this.nextChoiceDraftId++,
        choiceText: '',
        isChosen: false,
      },
    ]);
  }

  protected setImportantChoiceText(choiceId: number, event: Event): void {
    const choiceText = (event.target as HTMLTextAreaElement).value;

    this.importantChoiceDrafts.update((choices) => choices.map((choice) => (
      choice.draftId === choiceId ? { ...choice, choiceText } : choice
    )));
  }

  protected setImportantChoiceChosen(choiceId: number, event: Event): void {
    const isChosen = (event.target as HTMLInputElement).checked;

    this.importantChoiceDrafts.update((choices) => choices.map((choice) => (
      choice.draftId === choiceId ? { ...choice, isChosen } : choice
    )));
  }

  protected removeImportantChoiceOption(choiceId: number): void {
    this.importantChoiceDrafts.update((choices) => choices.filter((choice) => choice.draftId !== choiceId));
  }

  protected selectCampaignMilestone(milestone: CampaignMilestoneModel): void {
    this.selectedMilestoneId.set(milestone.id);
    this.noteDraft.set(milestone.title);
  }

  protected selectItemFoundAsset(item: AssetModel): void {
    this.selectedItemFoundAssetId.set(item.id);
    const itemType = this.getAssetItemTypeLabel(item);

    this.noteDraft.set(item.description ? `${item.name}\n${itemType}\n${item.description}` : `${item.name}\n${itemType}`);
  }

  protected getAssetItemTypeLabel(item: AssetModel): string {
    const itemType = this.toAssetItemType(item.itemType);

    return itemType === null
      ? 'Item'
      : AssetItemType[itemType] ?? 'Item';
  }

  protected getItemFoundTitle(note: SessionNoteModel): string {
    return this.getItemFoundLines(note)[0] ?? note.content;
  }

  protected getItemFoundType(note: SessionNoteModel): string {
    const lines = this.getItemFoundLines(note);

    return lines.length > 2 ? lines[1] : 'Item';
  }

  protected getItemFoundDescription(note: SessionNoteModel): string {
    const lines = this.getItemFoundLines(note);

    return lines.length > 2 ? lines.slice(2).join('\n') : lines.slice(1).join('\n');
  }

  protected addNote(): void {
    const campaignId = this.getCampaignId();
    const session = this.session();
    const content = this.normalizeDescription(this.noteDraft());
    const editingNote = this.editingNote();

    if (!campaignId || !session || !content || this.isSavingNote()) {
      return;
    }

    this.isSavingNote.set(true);

    const saveNote = this.isCampaignMilestonePicker()
      ? this.saveCampaignMilestoneNote(campaignId, session.id, content)
      : this.isItemFoundPicker()
      ? this.campaignSessionSocket.createItemFoundSessionNote(campaignId, session.id, content)
      : this.isImportantChoiceEditor()
      ? this.saveImportantChoiceNote(campaignId, session.id, content, editingNote)
      : editingNote
        ? this.campaignSessionSocket.updateSessionNote(campaignId, session.id, editingNote.id, content)
        : this.campaignSessionSocket.createGenericSessionNote(campaignId, session.id, content);

    saveNote
      .then((updatedSession) => {
        if (updatedSession) {
          this.upsertSession(updatedSession);
        }

        this.closeNoteTypeDialog();
      })
      .catch((error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Session note could not be added.'),
          { statusCode: this.getErrorStatus(error) },
        );
      })
      .finally(() => this.isSavingNote.set(false));
  }

  protected openNoteContextMenu(event: MouseEvent, note: SessionNoteModel): void {
    if (this.allowNativeContextMenuNoteId === note.id) {
      this.allowNativeContextMenuNoteId = null;
      this.noteContextMenu.set(null);
      return;
    }

    event.preventDefault();
    event.stopPropagation();

    this.noteContextMenu.set({
      note,
      x: Math.max(8, Math.min(event.clientX, window.innerWidth - 190)),
      y: Math.max(8, Math.min(event.clientY, window.innerHeight - 96)),
    });
  }

  protected editNote(note: SessionNoteModel): void {
    this.noteContextMenu.set(null);
    this.editingNote.set(note);
    this.selectedNoteType.set(this.toSessionNoteType(note.type));
    this.noteDraft.set(note.content);
    this.selectedMilestoneId.set(null);
    this.selectedItemFoundAssetId.set(null);
    this.availableMilestones.set([]);
    this.availableItems.set([]);
    this.importantChoiceDrafts.set(this.toImportantChoiceDrafts(note.choices ?? []));
    this.isNoteTypeDialogOpen.set(true);
    this.isNoteEditorOpen.set(true);
  }

  protected confirmDeleteNote(note: SessionNoteModel): void {
    this.noteContextMenu.set(null);
    this.deleteConfirmationNote.set(note);
  }

  protected cancelDeleteNote(): void {
    if (this.isDeletingNote()) {
      return;
    }

    this.deleteConfirmationNote.set(null);
  }

  protected deleteNote(): void {
    const campaignId = this.getCampaignId();
    const session = this.session();
    const note = this.deleteConfirmationNote();

    if (!campaignId || !session || !note || this.isDeletingNote()) {
      return;
    }

    this.isDeletingNote.set(true);

    this.campaignSessionSocket
      .deleteSessionNote(campaignId, session.id, note.id)
      .then((updatedSession) => {
        if (updatedSession) {
          this.upsertSession(updatedSession);
        }

        this.deleteConfirmationNote.set(null);
      })
      .catch((error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Session note could not be deleted.'),
          { statusCode: this.getErrorStatus(error) },
        );
      })
      .finally(() => this.isDeletingNote.set(false));
  }

  protected allowMoreNoteOptions(note: SessionNoteModel): void {
    this.allowNativeContextMenuNoteId = note.id;
    this.noteContextMenu.set(null);

    window.setTimeout(() => {
      if (this.allowNativeContextMenuNoteId === note.id) {
        this.allowNativeContextMenuNoteId = null;
      }
    }, 4000);
  }

  private loadSession(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId) {
      this.modalHelper.showError('Campaign id was not found.');
      return;
    }

    this.isLoading.set(true);

    this.campaignApiService
      .fetchCampaignSessions(campaignId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          this.sessions.set(response.data ?? []);
          this.syncDraftFromSession();
          void this.campaignSessionSocket
            .connect(campaignId)
            .then(() => this.loadSessionNotes(campaignId))
            .catch((error: unknown) => {
              this.modalHelper.showError(
                this.getErrorMessage(error, 'Campaign session socket could not connect.'),
                { statusCode: this.getErrorStatus(error) },
              );
            });
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign session could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private async saveDescription(): Promise<boolean> {
    const campaignId = this.getCampaignId();
    const session = this.session();

    if (!campaignId || !session || this.isSavingDescription()) {
      return false;
    }

    this.isSavingDescription.set(true);

    try {
      const description = this.toNullableDescription(this.descriptionDraft());
      const updatedSession = await this.campaignSessionSocket.updateSessionDescription(
        campaignId,
        session.id,
        description,
      );

      this.upsertSession(updatedSession ?? {
        ...session,
        description,
      });
      this.savedDescription.set(description ?? '');
      this.descriptionDraft.set(description ?? '');

      return true;
    } catch (error: unknown) {
      this.modalHelper.showError(
        this.getErrorMessage(error, 'Session description could not be saved.'),
        { statusCode: this.getErrorStatus(error) },
      );

      return false;
    } finally {
      this.isSavingDescription.set(false);
    }
  }

  private syncDraftFromSession(): void {
    const description = this.session()?.description ?? '';

    this.savedDescription.set(description);
    this.descriptionDraft.set(description);
  }

  private loadSessionNotes(campaignId: string): void {
    const session = this.session();

    if (!session) {
      return;
    }

    void this.campaignSessionSocket
      .getSessionNotes(campaignId, session.id)
      .then((notes) => this.setSessionNotes(session.id, notes))
      .catch((error: unknown) => {
        this.modalHelper.showError(
          this.getErrorMessage(error, 'Session notes could not be loaded.'),
          { statusCode: this.getErrorStatus(error) },
        );
      });
  }

  private loadAvailableMilestones(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingAvailableMilestones()) {
      return;
    }

    this.isLoadingAvailableMilestones.set(true);

    this.campaignApiService
      .fetchUnachievedCampaignMilestones(campaignId)
      .pipe(finalize(() => this.isLoadingAvailableMilestones.set(false)))
      .subscribe({
        next: (response) => {
          this.availableMilestones.set(response.data ?? []);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign milestones could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private loadAvailableItems(): void {
    const campaignId = this.getCampaignId();

    if (!campaignId || this.isLoadingAvailableItems()) {
      return;
    }

    this.isLoadingAvailableItems.set(true);

    this.campaignApiService
      .fetchAvailableCampaignItems(campaignId)
      .pipe(finalize(() => this.isLoadingAvailableItems.set(false)))
      .subscribe({
        next: (response) => {
          this.availableItems.set(response.data ?? []);
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Campaign items could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  private setSessionNotes(sessionId: number, notes: SessionNoteModel[]): void {
    const session = this.sessions().find((existingSession) => existingSession.id === sessionId);

    if (!session) {
      return;
    }

    this.upsertSession({
      ...session,
      notes: this.orderNotes(notes),
    });
  }

  private upsertSession(session: CampaignSessionModel): void {
    this.sessions.update((sessions) => sessions.map((existingSession) => (
      existingSession.id === session.id ? session : existingSession
    )));
    this.campaignSessionSocket.upsertSession(session);
  }

  private orderNotes(notes: SessionNoteModel[]): SessionNoteModel[] {
    return [...notes].sort((first, second) => (
      first.order - second.order ||
      first.id - second.id
    ));
  }

  private saveImportantChoiceNote(
    campaignId: string,
    sessionId: number,
    content: string,
    editingNote: SessionNoteModel | null,
  ): Promise<CampaignSessionModel | null> {
    const request: ImportantChoiceSessionNoteRequest = {
      content,
      choices: this.importantChoiceDrafts()
        .map((choice) => ({
          choiceText: this.normalizeDescription(choice.choiceText),
          isChosen: choice.isChosen,
        }))
        .filter((choice) => choice.choiceText.length > 0),
    };

    return editingNote
      ? this.campaignSessionSocket.updateImportantChoiceSessionNote(
        campaignId,
        sessionId,
        editingNote.id,
        request,
      )
      : this.campaignSessionSocket.createImportantChoiceSessionNote(
        campaignId,
        sessionId,
        request,
      );
  }

  private saveCampaignMilestoneNote(
    campaignId: string,
    sessionId: number,
    content: string,
  ): Promise<CampaignSessionModel | null> {
    const milestoneId = this.selectedMilestoneId();

    if (!milestoneId) {
      return Promise.resolve(null);
    }

    const request: AchieveCampaignMilestoneRequest = {
      milestoneId,
      content,
    };

    return this.campaignSessionSocket.achieveCampaignMilestone(
      campaignId,
      sessionId,
      request,
    );
  }

  private toImportantChoiceDrafts(choices: SessionNoteChoiceModel[]): ImportantChoiceDraft[] {
    return [...choices]
      .sort((first, second) => first.order - second.order || first.id - second.id)
      .map((choice) => ({
        draftId: this.nextChoiceDraftId++,
        choiceText: choice.choiceText,
        isChosen: choice.isChosen,
      }));
  }

  private toSessionNoteType(noteType: SessionNoteModel['type']): SessionNoteType | null {
    if (typeof noteType === 'number') {
      return noteType in SessionNoteType ? noteType as SessionNoteType : null;
    }

    return SessionNoteType[noteType as keyof typeof SessionNoteType] ?? null;
  }

  private getItemFoundLines(note: SessionNoteModel): string[] {
    return note.content
      .split(/\r?\n/)
      .map((line) => line.trim())
      .filter((line) => line.length > 0);
  }

  private toAssetItemType(itemType: AssetModel['itemType']): AssetItemType | null {
    if (itemType === null) {
      return null;
    }

    if (typeof itemType === 'number') {
      return itemType in AssetItemType ? itemType as AssetItemType : null;
    }

    return AssetItemType[itemType as keyof typeof AssetItemType] ?? null;
  }

  private getCampaignId(): string | null {
    return this.route.parent?.snapshot.paramMap.get('campaignId') ?? null;
  }

  private resolvePendingDeactivate(canDeactivate: boolean): void {
    this.isUnsavedChangesDialogOpen.set(false);
    this.pendingDeactivateResolution?.(canDeactivate);
    this.pendingDeactivateResolution = null;
  }

  private formatDisplayDate(sessionDate: string | null): string {
    if (!sessionDate) {
      return '';
    }

    const date = new Date(sessionDate);

    if (Number.isNaN(date.getTime())) {
      return '';
    }

    return new Intl.DateTimeFormat('en-GB', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    }).format(date);
  }

  private toDateInputValue(sessionDate: string | null | undefined): string {
    if (!sessionDate) {
      return '';
    }

    const date = new Date(sessionDate);

    return Number.isNaN(date.getTime())
      ? ''
      : date.toISOString().slice(0, 10);
  }

  private toSessionDateValue(date: string): string | null {
    return date ? `${date}T00:00:00.000Z` : null;
  }

  private normalizeDescription(description: string | null | undefined): string {
    return description?.trim() ?? '';
  }

  private toNullableDescription(description: string): string | null {
    const normalizedDescription = this.normalizeDescription(description);

    return normalizedDescription ? normalizedDescription : null;
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (this.isApiError(error) || error instanceof Error) {
      return error.message;
    }

    return fallback;
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
