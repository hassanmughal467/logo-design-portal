export enum QuoteStatus {
  Pending = 'Pending',
  Responded = 'Responded',
  Accepted = 'Accepted',
  Rejected = 'Rejected',
  Expired = 'Expired',
  Converted = 'Converted'
}

export interface Quote {
  id: string;
  clientId: string;
  logoName: string;
  description: string;
  attachments: string[];
  requestedBudget?: number;
  adminQuotedPrice?: number;
  adminNotes?: string;
  status: QuoteStatus;
  createdAt: string;
  convertedOrderId?: string;
}
