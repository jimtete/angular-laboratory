export interface LibraryFileModel {
  id: number;
  uploadedByUserId: string;
  parentFolderId: number | null;
  displayName: string;
  originalFileName: string;
  storedFileName: string;
  storagePath: string;
  contentType: string;
  fileSizeBytes: number;
  durationMilliseconds: number | null;
  createdAt: string;
  updatedAt: string;
}

export interface LibraryFolderModel {
  id: number;
  createdByUserId: string;
  parentFolderId: number | null;
  name: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateLibraryFolderRequest {
  parentFolderId: number | null;
  name: string;
}

export interface CreateLibraryFileRequest {
  parentFolderId: number | null;
  displayName: string | null;
  durationMilliseconds: number | null;
}

export interface UpdateLibraryFileRequest {
  parentFolderId: number | null;
  displayName: string | null;
  durationMilliseconds: number | null;
}
