export interface Order {
  id: string;
  clientId: string;
  designerId?: string;
  title: string;
  description: string;
  status: OrderStatus;
  priority?: OrderPriority;
  price: number;
  /** Proposed client-facing amount when admin requests price approval (before client accepts). */
  clientPrice?: number;
  clientBasePrice?: number;
  clientChargePrice?: number;
  currencyCode?: string;
  standardPrice?: number;
  designerProposedPrice?: number;
  designerApprovedPrice?: number;
  proposedPrice?: number;
  approvedPrice?: number;
  priceApprovalStatus?: string;
  requiresPriceApproval: boolean;
  priceApproved: boolean;
  /** Latest note entered when Admin/SuperAdmin requested client price approval. */
  priceApprovalRequestNotes?: string;
  /** Display of who requested client price approval (e.g. "Hawk Merchandising / Admin") */
  priceApprovalRequestedByDisplay?: string;
  priceApprovalRequestedAt?: Date;
  /** Client counter-offer / reject notes (admin sees when status is price pending). */
  clientPriceResponseNotes?: string;
  clientPriceResponseAt?: Date;
  /** Role of user who last updated client charge price (Admin, SuperAdmin, Client, Designer) */
  priceUpdatedByRole?: string;
  /** When the client charge price was last updated */
  priceUpdatedAt?: Date;
  priceUpdatedByUserId?: string;
  priceUpdatedByName?: string;
  designCategory?: string;
  designType?: string;
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
  /** Max revisions allowed for this package. Null/undefined = unlimited. */
  revisionLimit?: number;
  /** True when client has used all revisions and cannot request more (unless admin approves extra). */
  revisionLimitExceeded?: boolean;
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
    userId?: string;
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
  orderSource?: OrderSource;
}

export enum OrderSource {
  Portal = 'Portal',
  ManualCompleted = 'ManualCompleted',
  Quote = 'Quote'
}

/** Order lifecycle: WaitingForAdminApproval → PriceApprovalPending → InProgress →
 * PreviewDelivered → RevisionRequested → ClientApproved → Completed */
export enum OrderStatus {
  WaitingForAdminApproval = 'WaitingForAdminApproval',
  PriceApprovalPending = 'PriceApprovalPending',
  InProgress = 'InProgress',
  PreviewDelivered = 'PreviewDelivered',
  RevisionRequested = 'RevisionRequested',
  /** Client approved design; Admin reviews before marking Completed */
  ClientApproved = 'ClientApproved',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  CancelledByUser = 'CancelledByUser',
  CancelledByAdmin = 'CancelledByAdmin',
  Refunded = 'Refunded',
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
  requiredFormats?: string;
  requirements?: string;
  colorPreferences?: string;
  stylePreferences?: string;
  designCategory?: number;
  designType?: number;
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
