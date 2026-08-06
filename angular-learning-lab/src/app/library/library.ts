import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  LucideFile,
  LucideFileText,
  LucideFolder,
  LucideFolderPlus,
  LucideImage,
  LucideMusic,
  LucideRefreshCw,
  LucideTrash2,
  LucideUpload,
  LucideX,
} from '@lucide/angular';
import { finalize, forkJoin } from 'rxjs';

import {
  ApiError,
  LibraryApiService,
  LibraryFileModel,
  LibraryFolderModel,
} from '../Infrastructure';
import { ModalHelper } from '../shared/helpers/modal.helper';

type DeleteConfirmation =
  | { kind: 'folder'; folder: LibraryFolderModel }
  | { kind: 'file'; file: LibraryFileModel };

@Component({
  selector: 'app-library',
  imports: [
    LucideFile,
    LucideFileText,
    LucideFolder,
    LucideFolderPlus,
    LucideImage,
    LucideMusic,
    LucideRefreshCw,
    LucideTrash2,
    LucideUpload,
    LucideX,
  ],
  templateUrl: './library.html',
  styleUrl: './library.css',
})
export class Library implements OnInit {
  private readonly libraryApiService = inject(LibraryApiService);
  private readonly modalHelper = inject(ModalHelper);

  protected readonly files = signal<LibraryFileModel[]>([]);
  protected readonly folders = signal<LibraryFolderModel[]>([]);
  protected readonly folderStack = signal<LibraryFolderModel[]>([]);
  protected readonly selectedFile = signal<LibraryFileModel | null>(null);
  protected readonly isLoadingContents = signal(false);
  protected readonly isUploadDialogOpen = signal(false);
  protected readonly isUploadingFile = signal(false);
  protected readonly selectedUploadFile = signal<File | null>(null);
  protected readonly displayNameDraft = signal('');
  protected readonly durationMillisecondsDraft = signal<number | null>(null);
  protected readonly isCreateFolderDialogOpen = signal(false);
  protected readonly folderNameDraft = signal('');
  protected readonly isCreatingFolder = signal(false);
  protected readonly deleteConfirmation = signal<DeleteConfirmation | null>(null);
  protected readonly isDeleting = signal(false);

  protected readonly currentFolderId = computed(() => {
    const folders = this.folderStack();

    return folders.length > 0 ? folders[folders.length - 1].id : null;
  });
  protected readonly currentFolderLabel = computed(() => {
    const folders = this.folderStack();

    return folders.length > 0 ? folders[folders.length - 1].name : 'Library';
  });
  protected readonly totalFileSize = computed(() => (
    this.files().reduce((total, file) => total + file.fileSizeBytes, 0)
  ));
  protected readonly canUploadFile = computed(() => (
    this.selectedUploadFile() !== null && !this.isUploadingFile()
  ));
  protected readonly canCreateFolder = computed(() => (
    this.normalizeText(this.folderNameDraft()).length > 0 && !this.isCreatingFolder()
  ));

  ngOnInit(): void {
    this.loadContents();
  }

  protected loadContents(): void {
    if (this.isLoadingContents()) {
      return;
    }

    const parentFolderId = this.currentFolderId();

    this.isLoadingContents.set(true);
    forkJoin({
      folders: this.libraryApiService.fetchFolders(parentFolderId),
      files: this.libraryApiService.fetchFiles(parentFolderId),
    })
      .pipe(finalize(() => this.isLoadingContents.set(false)))
      .subscribe({
        next: ({ folders, files }) => {
          const loadedFiles = files.data ?? [];

          this.folders.set(folders.data ?? []);
          this.files.set(loadedFiles);
          this.selectedFile.update((selectedFile) => (
            selectedFile
              ? loadedFiles.find((file) => file.id === selectedFile.id) ?? null
              : null
          ));
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Library contents could not be loaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected openFolder(folder: LibraryFolderModel): void {
    this.folderStack.update((folders) => [...folders, folder]);
    this.selectedFile.set(null);
    this.loadContents();
  }

  protected openRoot(): void {
    this.folderStack.set([]);
    this.selectedFile.set(null);
    this.loadContents();
  }

  protected openBreadcrumb(index: number): void {
    this.folderStack.update((folders) => folders.slice(0, index + 1));
    this.selectedFile.set(null);
    this.loadContents();
  }

  protected selectFile(file: LibraryFileModel): void {
    this.selectedFile.set(file);
  }

  protected openCreateFolderDialog(): void {
    this.folderNameDraft.set('');
    this.isCreateFolderDialogOpen.set(true);
  }

  protected closeCreateFolderDialog(): void {
    if (!this.isCreatingFolder()) {
      this.isCreateFolderDialogOpen.set(false);
    }
  }

  protected setFolderNameDraft(event: Event): void {
    this.folderNameDraft.set((event.target as HTMLInputElement).value);
  }

  protected createFolder(): void {
    if (!this.canCreateFolder()) {
      return;
    }

    this.isCreatingFolder.set(true);
    this.libraryApiService
      .createFolder({
        parentFolderId: this.currentFolderId(),
        name: this.normalizeText(this.folderNameDraft()),
      })
      .pipe(finalize(() => this.isCreatingFolder.set(false)))
      .subscribe({
        next: () => {
          this.isCreateFolderDialogOpen.set(false);
          this.loadContents();
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Library folder could not be created.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected openUploadDialog(): void {
    this.selectedUploadFile.set(null);
    this.displayNameDraft.set('');
    this.durationMillisecondsDraft.set(null);
    this.isUploadDialogOpen.set(true);
  }

  protected closeUploadDialog(): void {
    if (!this.isUploadingFile()) {
      this.isUploadDialogOpen.set(false);
    }
  }

  protected setUploadFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    this.selectedUploadFile.set(file);

    if (file && !this.normalizeText(this.displayNameDraft())) {
      this.displayNameDraft.set(this.getNameWithoutExtension(file.name));
    }
  }

  protected setDisplayNameDraft(event: Event): void {
    this.displayNameDraft.set((event.target as HTMLInputElement).value);
  }

  protected setDurationMillisecondsDraft(event: Event): void {
    const value = Number((event.target as HTMLInputElement).value);

    this.durationMillisecondsDraft.set(Number.isFinite(value) && value > 0 ? value : null);
  }

  protected uploadFile(): void {
    const file = this.selectedUploadFile();

    if (!file || !this.canUploadFile()) {
      return;
    }

    this.isUploadingFile.set(true);
    this.libraryApiService
      .uploadFile(
        {
          parentFolderId: this.currentFolderId(),
          displayName: this.toNullableText(this.displayNameDraft()),
          durationMilliseconds: this.durationMillisecondsDraft(),
        },
        file,
      )
      .pipe(finalize(() => this.isUploadingFile.set(false)))
      .subscribe({
        next: (response) => {
          this.isUploadDialogOpen.set(false);
          this.loadContents();

          if (response.data) {
            this.selectedFile.set(response.data);
          }
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Library file could not be uploaded.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected confirmDeleteFolder(folder: LibraryFolderModel, event: Event): void {
    event.stopPropagation();
    this.deleteConfirmation.set({ kind: 'folder', folder });
  }

  protected confirmDeleteFile(file: LibraryFileModel, event?: Event): void {
    event?.stopPropagation();
    this.deleteConfirmation.set({ kind: 'file', file });
  }

  protected cancelDelete(): void {
    if (!this.isDeleting()) {
      this.deleteConfirmation.set(null);
    }
  }

  protected deleteConfirmed(): void {
    const confirmation = this.deleteConfirmation();

    if (!confirmation || this.isDeleting()) {
      return;
    }

    const request = confirmation.kind === 'folder'
      ? this.libraryApiService.deleteFolder(confirmation.folder.id)
      : this.libraryApiService.deleteFile(confirmation.file.id);

    this.isDeleting.set(true);
    request
      .pipe(finalize(() => this.isDeleting.set(false)))
      .subscribe({
        next: () => {
          if (confirmation.kind === 'file' && this.selectedFile()?.id === confirmation.file.id) {
            this.selectedFile.set(null);
          }

          this.deleteConfirmation.set(null);
          this.loadContents();
        },
        error: (error: unknown) => {
          this.modalHelper.showError(
            this.getErrorMessage(error, 'Library item could not be deleted.'),
            { statusCode: this.getErrorStatus(error) },
          );
        },
      });
  }

  protected deleteConfirmationTitle(confirmation: DeleteConfirmation): string {
    return confirmation.kind === 'folder' ? 'Delete Folder' : 'Delete File';
  }

  protected deleteConfirmationMessage(confirmation: DeleteConfirmation): string {
    return confirmation.kind === 'folder'
      ? `Delete "${confirmation.folder.name}" and everything inside it?`
      : `Delete "${confirmation.file.displayName || confirmation.file.originalFileName}"?`;
  }

  protected getFileKindLabel(file: LibraryFileModel): string {
    if (this.isImageFile(file)) {
      return 'Image';
    }

    if (this.isAudioFile(file)) {
      return 'Audio';
    }

    if (this.isDocumentFile(file)) {
      return 'Document';
    }

    return 'File';
  }

  protected isImageFile(file: LibraryFileModel): boolean {
    return file.contentType.toLowerCase().startsWith('image/');
  }

  protected isAudioFile(file: LibraryFileModel): boolean {
    return file.contentType.toLowerCase().startsWith('audio/');
  }

  protected isDocumentFile(file: LibraryFileModel): boolean {
    const contentType = file.contentType.toLowerCase();

    return contentType.includes('pdf') ||
      contentType.includes('text') ||
      contentType.includes('document') ||
      contentType.includes('json');
  }

  protected getFileSizeLabel(fileSizeBytes: number): string {
    if (fileSizeBytes < 1024) {
      return `${fileSizeBytes} B`;
    }

    const kilobytes = fileSizeBytes / 1024;

    if (kilobytes < 1024) {
      return `${kilobytes.toFixed(1)} KB`;
    }

    return `${(kilobytes / 1024).toFixed(1)} MB`;
  }

  protected getDurationLabel(durationMilliseconds: number | null): string {
    if (durationMilliseconds === null) {
      return 'None';
    }

    const totalSeconds = Math.ceil(durationMilliseconds / 1000);
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;

    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }

  protected formatDate(value: string): string {
    const date = new Date(value);

    return Number.isNaN(date.getTime())
      ? value
      : date.toLocaleDateString(undefined, {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
      });
  }

  private getNameWithoutExtension(fileName: string): string {
    const lastDotIndex = fileName.lastIndexOf('.');

    return lastDotIndex > 0 ? fileName.slice(0, lastDotIndex) : fileName;
  }

  private normalizeText(value: string | null | undefined): string {
    return value?.trim() ?? '';
  }

  private toNullableText(value: string | null | undefined): string | null {
    const normalizedValue = this.normalizeText(value);

    return normalizedValue || null;
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
