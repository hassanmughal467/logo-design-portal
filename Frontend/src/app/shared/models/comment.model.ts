export interface OrderComment {
  id: string;
  orderId: string;
  content: string;
  createdBy: string;
  createdByName: string;
  createdByRole: string;
  isInternal: boolean;
  createdAt: Date;
}

export interface CreateCommentRequest {
  content: string;
  isInternal: boolean;
}
