export interface LogoFile {
  id: string;
  orderId: string;
  fileName: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  fileType: FileType;
  fileCategory?: FileCategory | string | null;
  fileStatus?: FileStatus | string;
  isFinalVersion: boolean;
  isVisibleToClient: boolean;
  isAdminApproved: boolean;
  versionNumber: number;
  description?: string;
  uploadedBy: string;
  uploadedByName: string;
  uploadedByRole?: string; // Role of user who uploaded
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

export enum FileCategory {
  Source = 'Source',
  Print = 'Print',
  Web = 'Web',
  Embroidery = 'Embroidery'
}

export enum FileStatus {
  Draft = 'Draft',
  Final = 'Final',
  Approved = 'Approved'
}

export interface LogoGroup {
  orderId: string;
  logoName: string;
  clientId: string;
  clientName?: string;
  clientCompanyName?: string;
  createdAt: Date;
  fileCount: number;
  files: LogoFile[];
}

export interface ClientGroup {
  clientId: string;
  clientName: string;
  clientCompanyName?: string;
  clientEmail?: string;
  logoCount: number;
  totalFiles: number;
  logos: LogoGroup[];
}

export interface GroupedFilesResponse {
  logos: LogoGroup[];
  totalLogos: number;
  totalFiles: number;
  totalPages: number;
  pageNumber: number;
  pageSize: number;
}