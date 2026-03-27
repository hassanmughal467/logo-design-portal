import { isOrderLocked } from './order-locking';

describe('order-locking', () => {
  it('returns false for empty status', () => {
    expect(isOrderLocked('')).toBe(false);
    expect(isOrderLocked(null)).toBe(false);
    expect(isOrderLocked(undefined)).toBe(false);
  });

  it('returns false for in-progress statuses', () => {
    expect(isOrderLocked('InProgress')).toBe(false);
    expect(isOrderLocked('WaitingForAdminApproval')).toBe(false);
    expect(isOrderLocked('ClientApproved')).toBe(false);
  });

  it('returns true for terminal statuses', () => {
    expect(isOrderLocked('Completed')).toBe(true);
    expect(isOrderLocked('Cancelled')).toBe(true);
    expect(isOrderLocked('CancelledByUser')).toBe(true);
    expect(isOrderLocked('CancelledByAdmin')).toBe(true);
    expect(isOrderLocked('Refunded')).toBe(true);
  });

  it('is case-sensitive per backend enum strings', () => {
    expect(isOrderLocked('completed')).toBe(false);
    expect(isOrderLocked('Completed')).toBe(true);
  });
});
