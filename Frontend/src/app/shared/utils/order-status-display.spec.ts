import { OrderStatus } from '@shared/models/order.model';
import {
  getOrderStatusLabel,
  getOrderStatusSeverity,
  getOrderStatusChartColor
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
});
