/** Reference uploads attached to a revision request (client). */
export interface RevisionAttachment {
  id: string;
  fileName: string;
  originalFileName: string;
  fileSize: number;
  contentType: string;
  createdAt: Date;
}

export interface OrderRevision {
  id: string;
  orderId: string;
  instructions: string;
  requestedBy: string;
  requestedByName: string;
  isResolved: boolean;
  resolvedAt?: Date;
  createdAt: Date;
  fileCount?: number;
  /** Populated from API `files` / `Files` */
  files?: RevisionAttachment[];
}

export interface CreateRevisionRequest {
  instructions: string;
}

export interface RevisionFile {
  id: string;
  revisionId: string;
  fileName: string;
  originalFileName: string;
  filePath: string;
  contentType: string;
  fileSize: number;
  fileType: RevisionFileType;
  description?: string;
}

export enum RevisionFileType {
  ReferenceImage = 'ReferenceImage',
  MachinePhoto = 'MachinePhoto',
  OutputPhoto = 'OutputPhoto'
}
