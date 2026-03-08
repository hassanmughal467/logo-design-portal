export interface Order {
  id: string;
  clientId: string;
  designerId?: string;
  title: string;
  description: string;
  status: OrderStatus;
  priority?: OrderPriority;
  price: number;
  proposedPrice?: number;
  requiresPriceApproval: boolean;
  priceApproved: boolean;
  createdAt: Date;
  updatedAt?: Date;
  dueDate?: Date;
  deadline?: Date;
  instructions?: string;
  requiredFormats?: string;
  requirements?: string;
  colorPreferences?: string;
  stylePreferences?: string;
  fileCount: number;
  visibleFileCount: number;
  revisionCount: number;
  commentCount: number;
  client?: {
    id: string;
    companyName: string;
    firstName: string;
    lastName: string;
    email?: string;
    phoneNumber?: string;
  };
  designer?: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
  };
  /** For Client view: masked display when designer assigned (e.g. "Company Design Team") */
  assignedDesignerDisplayName?: string;
  
  // Cancellation fields
  cancellationReason?: string;
  cancelledAt?: Date;
  isCancelledByUser?: boolean;
  
  // Archive fields
  isArchived?: boolean;
  archivedAt?: Date;
  
  // Refund fields
  isRefunded?: boolean;
  refundedAt?: Date;
  refundAmount?: number;
  refundReason?: string;
  
  // Invoice fields
  hasInvoice?: boolean;
  
  // Upload control fields
  allowUploads?: boolean;
}

export enum OrderStatus {
  // Original statuses (keeping for backward compatibility)
  WaitingForAdminApproval = 'WaitingForAdminApproval',
  PriceApprovalPending = 'PriceApprovalPending',
  InProgress = 'InProgress',
  PreviewDelivered = 'PreviewDelivered',
  RevisionRequested = 'RevisionRequested',
  FinalApproved = 'FinalApproved',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  
  // New professional statuses
  Pending = 'Pending',
  Paid = 'Paid',
  Processing = 'Processing',
  CancelledByUser = 'CancelledByUser',
  CancelledByAdmin = 'CancelledByAdmin',
  Refunded = 'Refunded',
  Failed = 'Failed',
  Archived = 'Archived'
}

export enum OrderPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Urgent = 'Urgent'
}

export interface CreateOrderRequest {
  title: string;
  description: string;
  price: number;
  priority?: number; // Backend expects integer: 1=Low, 2=Medium, 3=High, 4=Urgent
  deadline?: Date;
  instructions?: string;
  requiredFormats?: string;
  requirements?: string;
  colorPreferences?: string;
  stylePreferences?: string;
}

export interface AssignOrderRequest {
  designerId: string;
  notes?: string;
}

export interface UpdateOrderStatusRequest {
  status: OrderStatus;
  notes?: string;
}

export interface RequestPriceApprovalRequest {
  proposedPrice: number;
  notes?: string;
}

export interface ApprovePriceRequest {
  approved: boolean;
  comment?: string;
}
