export interface OrderLog {
  id: string;
  orderId: string;
  action: string;
  previousStatus?: string;
  newStatus?: string;
  performedBy: string;
  performedById?: string;
  note?: string;
  metadata?: string;
  createdAt: Date;
}
