export interface GalleryItem {
  id: string;
  orderId: string;
  orderTitle: string;
  previewImagePath: string;
  fileName: string;
  originalFileName: string;
  filePath: string;
  contentType: string;
  format?: string;
  approvedAt: Date;
  createdAt: Date;
}
