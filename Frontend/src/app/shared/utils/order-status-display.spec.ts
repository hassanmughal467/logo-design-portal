import { OrderStatus } from '@shared/models/order.model';
import {
  buildOrderStatusFilterOptions,
  getOrderStatusLabel,
  getOrderStatusSeverity,
  getOrderStatusChartColor,
  ORDER_STATUS_DISPLAY_ORDER
} from './order-status-display';

describe('order-status-display', () => {
  it('uses Awaiting Admin label for WaitingForAdminApproval', () => {
    expect(getOrderStatusLabel(OrderStatus.WaitingForAdminApproval)).toBe('Awaiting Admin');
    expect(getOrderStatusLabel('Waiting For Admin Approval')).toBe('Awaiting Admin');
  });

  it('uses warning severity and orange chart color for admin queue status', () => {
    expect(getOrderStatusSeverity(OrderStatus.WaitingForAdminApproval)).toBe('warning');
    expect(getOrderStatusChartColor(OrderStatus.WaitingForAdminApproval)).toBe('#f97316');
  });

  it('returns secondary severity for empty status', () => {
    expect(getOrderStatusSeverity(null)).toBe('secondary');
    expect(getOrderStatusSeverity(undefined)).toBe('secondary');
    expect(getOrderStatusSeverity('')).toBe('secondary');
  });

  it('maps lifecycle statuses to labels and severities', () => {
    expect(getOrderStatusLabel(OrderStatus.Completed)).toBe('Completed');
    expect(getOrderStatusSeverity(OrderStatus.Completed)).toBe('success');
    expect(getOrderStatusSeverity(OrderStatus.ApprovedUnassigned)).toBe('danger');
    expect(getOrderStatusSeverity(OrderStatus.Refunded)).toBe('warning');
    expect(getOrderStatusSeverity('UnknownStatus')).toBe('secondary');
  });

  it('falls back to spaced label when key is unknown', () => {
    expect(getOrderStatusLabel('CustomStatus')).toBe('Custom Status');
  });

  it('uses default chart color for unknown status', () => {
    expect(getOrderStatusChartColor('UnknownStatus')).toBe('#64748b');
  });

  it('builds filter options with All Statuses first', () => {
    const options = buildOrderStatusFilterOptions([OrderStatus.Completed]);
    expect(options[0]).toEqual({ label: 'All Statuses', value: '' });
    expect(options[1]).toEqual({ label: 'Completed', value: OrderStatus.Completed });
  });

  it('uses default display order when statuses omitted', () => {
    const options = buildOrderStatusFilterOptions();
    expect(options.length).toBe(ORDER_STATUS_DISPLAY_ORDER.length + 1);
  });

  it('returns Awaiting Admin label when status is empty', () => {
    expect(getOrderStatusLabel(null)).toBe('Awaiting Admin');
    expect(getOrderStatusLabel('')).toBe('Awaiting Admin');
  });

  it('maps additional chart colors for common statuses', () => {
    expect(getOrderStatusChartColor(OrderStatus.Completed)).toBe('#10b981');
    expect(getOrderStatusChartColor(OrderStatus.Cancelled)).toBe('#ef4444');
    expect(getOrderStatusChartColor('Paid')).toBe('#059669');
  });
});
