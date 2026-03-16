export enum DesignCategory {
  EmbroideryDigitizing = 1,
  VectorScreenPrinting = 2,
  CustomPatch = 3
}

export enum DesignType {
  LeftChest = 1,
  JacketBack = 2,
  SimpleVector = 3,
  ComplexVector = 4
}

export interface DesignerPricingInfo {
  designCategory: string;
  designType: string;
  defaultPrice: number | null;
  requiresCustomPrice: boolean;
}

export interface SubmitDesignerPricingRequest {
  designCategory: DesignCategory;
  designType: DesignType;
  proposedPrice: number;
}

export interface ApproveDesignerPriceRequest {
  approvedPrice?: number;
  action: 'Approve' | 'Modify' | 'Reject';
}

export interface OrderPricingSummary {
  orderId: string;
  orderTitle: string;
  designCategory?: string;
  designType?: string;
  standardPrice?: number;
  proposedPrice?: number;
  approvedPrice?: number;
  priceApprovalStatus: string;
  priceApproved: boolean;
  completedDate?: Date;
}

export interface DesignerInvoiceItem {
  id: string;
  orderId: string;
  orderTitle: string;
  description: string;
  amount: number;
  designCategory?: string;
  designType?: string;
}

export interface DesignerInvoiceAdjustment {
  id: string;
  description: string;
  amount: number;
}

export interface DesignerInvoice {
  id: string;
  designerId: string;
  designerName: string;
  invoiceNumber: string;
  totalAmount: number;
  status: string;
  billingPeriod: string;
  issueDate: Date;
  paidDate?: Date;
  notes?: string;
  items: DesignerInvoiceItem[];
  adjustments?: DesignerInvoiceAdjustment[];
}
