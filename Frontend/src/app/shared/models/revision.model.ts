export interface OrderRevision {
  id: string;
  orderId: string;
  instructions: string;
  requestedBy: string;
  requestedByName: string;
  isResolved: boolean;
  resolvedAt?: Date;
  createdAt: Date;
  fileCount: number;
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
