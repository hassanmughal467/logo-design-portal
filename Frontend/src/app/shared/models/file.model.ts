export interface LogoFile {
  id: string;
  orderId: string;
  fileName: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  fileType: FileType;
  isFinalVersion: boolean;
  isVisibleToClient: boolean;
  isAdminApproved: boolean;
  versionNumber: number;
  description?: string;
  uploadedBy: string;
  uploadedByName: string;
  approvedBy?: string;
  approvedByName?: string;
  approvedAt?: Date;
  createdAt: Date;
}

export enum FileType {
  Reference = 'Reference',
  Preview = 'Preview',
  Final = 'Final',
  Revision = 'Revision'
}

export interface FileUploadRequest {
  fileType: FileType;
  description?: string;
}

export interface ApproveFileRequest {
  approved: boolean;
  makeVisibleToClient: boolean;
}
