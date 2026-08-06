import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  ApiResponse,
  CreateLibraryFolderRequest,
  CreateLibraryFileRequest,
  LibraryFolderModel,
  LibraryFileModel,
  UpdateLibraryFileRequest,
} from '../models';
import { ApiClient } from './api-client.service';

@Injectable({
  providedIn: 'root',
})
export class LibraryApiService {
  private readonly apiClient = inject(ApiClient);

  fetchFolders(parentFolderId: number | null): Observable<ApiResponse<LibraryFolderModel[]>> {
    return this.apiClient.get<ApiResponse<LibraryFolderModel[]>>(
      this.withParentFolderQuery('/api/library/folders', parentFolderId),
    );
  }

  createFolder(
    request: CreateLibraryFolderRequest,
  ): Observable<ApiResponse<LibraryFolderModel>> {
    return this.apiClient.post<ApiResponse<LibraryFolderModel>, CreateLibraryFolderRequest>(
      '/api/library/folders',
      request,
    );
  }

  deleteFolder(folderId: number): Observable<ApiResponse<object>> {
    return this.apiClient.delete<ApiResponse<object>>(`/api/library/folders/${folderId}`);
  }

  fetchFiles(parentFolderId: number | null): Observable<ApiResponse<LibraryFileModel[]>> {
    return this.apiClient.get<ApiResponse<LibraryFileModel[]>>(
      this.withParentFolderQuery('/api/library/files', parentFolderId),
    );
  }

  fetchAllFiles(): Observable<ApiResponse<LibraryFileModel[]>> {
    return this.apiClient.get<ApiResponse<LibraryFileModel[]>>('/api/library/files?allFolders=true');
  }

  fetchFile(fileId: number): Observable<ApiResponse<LibraryFileModel>> {
    return this.apiClient.get<ApiResponse<LibraryFileModel>>(`/api/library/files/${fileId}`);
  }

  uploadFile(
    request: CreateLibraryFileRequest,
    file: File,
  ): Observable<ApiResponse<LibraryFileModel>> {
    const formData = new FormData();

    if (request.displayName !== null) {
      formData.append('DisplayName', request.displayName);
    }

    if (request.parentFolderId !== null) {
      formData.append('ParentFolderId', request.parentFolderId.toString());
    }

    if (request.durationMilliseconds !== null) {
      formData.append('DurationMilliseconds', request.durationMilliseconds.toString());
    }

    formData.append('file', file, file.name);

    return this.apiClient.post<ApiResponse<LibraryFileModel>, FormData>(
      '/api/library/files',
      formData,
    );
  }

  updateFile(
    fileId: number,
    request: UpdateLibraryFileRequest,
  ): Observable<ApiResponse<LibraryFileModel>> {
    return this.apiClient.put<ApiResponse<LibraryFileModel>, UpdateLibraryFileRequest>(
      `/api/library/files/${fileId}`,
      request,
    );
  }

  deleteFile(fileId: number): Observable<ApiResponse<object>> {
    return this.apiClient.delete<ApiResponse<object>>(`/api/library/files/${fileId}`);
  }

  private withParentFolderQuery(path: string, parentFolderId: number | null): string {
    return parentFolderId === null ? path : `${path}?parentFolderId=${parentFolderId}`;
  }
}
