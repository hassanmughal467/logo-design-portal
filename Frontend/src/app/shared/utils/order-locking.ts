/**
 * Terminal order statuses that make an order read-only.
 * When an order reaches any of these statuses, no modifications are allowed.
 */
const TERMINAL_ORDER_STATUSES = [
  'Completed',
  'FinalApproved',
  'Cancelled',
  'CancelledByUser',
  'CancelledByAdmin',
  'Refunded'
];

/**
 * Returns true if the order status is terminal (locked).
 * Locked orders cannot be modified by Client, Designer, or Admin.
 */
export function isOrderLocked(orderStatus: string | undefined | null): boolean {
  if (!orderStatus) {
    return false;
  }
  return TERMINAL_ORDER_STATUSES.includes(orderStatus);
}
