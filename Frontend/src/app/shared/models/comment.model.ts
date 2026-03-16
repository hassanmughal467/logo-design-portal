export interface OrderComment {
  id: string;
  orderId: string;
  content: string;
  createdBy: string;
  createdByName: string;
  createdByRole: string;
  isInternal: boolean;
  commentType: string;
  visibleToClient: boolean;
  isReadByClient: boolean;
  isReadByDesigner: boolean;
  isReadByAdmin: boolean;
  createdAt: Date;
}

export interface CreateCommentRequest {
  content: string;
  isInternal: boolean;
}

export interface OrderCommentUnreadCounts {
  unreadFiles: number;
  unreadRevisions: number;
  unreadComments: number;
}
