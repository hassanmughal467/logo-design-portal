import { OrderStatus } from '@shared/models/order.model';

/** PrimeNG p-tag severity for order lifecycle statuses. */
export type OrderStatusSeverity = 'success' | 'info' | 'warning' | 'danger' | 'secondary' | 'contrast';

/** Human-readable labels — single source of truth across dashboard, orders, analytics. */
const ORDER_STATUS_LABELS: Record<string, string> = {
  [OrderStatus.WaitingForAdminApproval]: 'Awaiting Admin',
  [OrderStatus.PriceApprovalPending]: 'Price Approval Pending',
  [OrderStatus.InProgress]: 'In Progress',
  [OrderStatus.PreviewDelivered]: 'Preview Delivered',
  [OrderStatus.RevisionRequested]: 'Revision Requested',
  [OrderStatus.ClientApproved]: 'Approved',
  [OrderStatus.Completed]: 'Completed',
  [OrderStatus.Cancelled]: 'Cancelled',
  [OrderStatus.CancelledByUser]: 'Cancelled By User',
  [OrderStatus.CancelledByAdmin]: 'Cancelled By Admin',
  [OrderStatus.Refunded]: 'Refunded',
  // Legacy / spaced forms from APIs or regex formatters
  'Waiting For Admin Approval': 'Awaiting Admin',
  'Price Approval Pending': 'Price Approval Pending',
  'Final Approved': 'Approved',
  Pending: 'Pending',
  Paid: 'Paid',
  Processing: 'Processing',
  Failed: 'Failed',
  Archived: 'Archived',
  Review: 'Review'
};

const ORDER_STATUS_SEVERITY: Record<string, OrderStatusSeverity> = {
  [OrderStatus.WaitingForAdminApproval]: 'warning',
  [OrderStatus.PriceApprovalPending]: 'info',
  [OrderStatus.InProgress]: 'info',
  [OrderStatus.PreviewDelivered]: 'success',
  [OrderStatus.RevisionRequested]: 'warning',
  [OrderStatus.ClientApproved]: 'success',
  [OrderStatus.Completed]: 'success',
  [OrderStatus.Cancelled]: 'danger',
  [OrderStatus.CancelledByUser]: 'danger',
  [OrderStatus.CancelledByAdmin]: 'danger',
  [OrderStatus.Refunded]: 'warning',
  Pending: 'warning',
  Failed: 'danger',
  Archived: 'secondary',
  Review: 'secondary'
};

/** Chart / doughnut colors aligned with warning (orange) for admin-queue status. */
const ORDER_STATUS_CHART_COLOR: Record<string, string> = {
  [OrderStatus.WaitingForAdminApproval]: '#f97316',
  [OrderStatus.PriceApprovalPending]: '#eab308',
  [OrderStatus.InProgress]: '#0d47a1',
  [OrderStatus.PreviewDelivered]: '#8b5cf6',
  [OrderStatus.RevisionRequested]: '#6366f1',
  [OrderStatus.ClientApproved]: '#14b8a6',
  [OrderStatus.Completed]: '#10b981',
  [OrderStatus.Cancelled]: '#ef4444',
  [OrderStatus.CancelledByUser]: '#dc2626',
  [OrderStatus.CancelledByAdmin]: '#b91c1c',
  [OrderStatus.Refunded]: '#6b7280',
  Pending: '#f59e0b',
  Paid: '#059669',
  Processing: '#06b6d4',
  Failed: '#991b1b',
  Archived: '#9ca3af',
  Review: '#8b5cf6'
};

/** Sort order for status dropdowns (matches order list). */
export const ORDER_STATUS_DISPLAY_ORDER: readonly OrderStatus[] = [
  OrderStatus.WaitingForAdminApproval,
  OrderStatus.PriceApprovalPending,
  OrderStatus.InProgress,
  OrderStatus.PreviewDelivered,
  OrderStatus.RevisionRequested,
  OrderStatus.ClientApproved,
  OrderStatus.Completed,
  OrderStatus.Cancelled,
  OrderStatus.CancelledByUser,
  OrderStatus.CancelledByAdmin,
  OrderStatus.Refunded
];

export function getOrderStatusLabel(status: string | OrderStatus | null | undefined): string {
  if (status === undefined || status === null || status === '') {
    return ORDER_STATUS_LABELS[OrderStatus.WaitingForAdminApproval];
  }
  const key = String(status).trim();
  if (ORDER_STATUS_LABELS[key]) {
    return ORDER_STATUS_LABELS[key];
  }
  const spaced = key.replace(/([A-Z])/g, ' $1').trim();
  return ORDER_STATUS_LABELS[spaced] ?? spaced;
}

export function getOrderStatusSeverity(status: string | OrderStatus | null | undefined): OrderStatusSeverity {
  if (status === undefined || status === null || status === '') {
    return 'secondary';
  }
  const key = String(status).trim();
  return ORDER_STATUS_SEVERITY[key] ?? 'secondary';
}

export function getOrderStatusChartColor(status: string): string {
  const key = String(status).trim();
  return ORDER_STATUS_CHART_COLOR[key] ?? '#64748b';
}

/** Status filter dropdown options (client dashboard order history, etc.). */
export function buildOrderStatusFilterOptions(
  statuses: readonly OrderStatus[] = ORDER_STATUS_DISPLAY_ORDER
): { label: string; value: string }[] {
  return [
    { label: 'All Statuses', value: '' },
    ...statuses.map((value) => ({
      label: getOrderStatusLabel(value),
      value
    }))
  ];
}
