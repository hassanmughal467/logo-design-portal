import {
  DEFAULT_INVOICE_CURRENCY,
  formatCurrencyAmount,
  resolveSingleCurrencyCode
} from './currency-format';

describe('currency-format', () => {
  it('resolveSingleCurrencyCode returns GBP when all sources are GBP', () => {
    const r = resolveSingleCurrencyCode(['GBP', 'gbp']);
    expect(r.mixed).toBe(false);
    expect(r.code).toBe('GBP');
  });

  it('resolveSingleCurrencyCode marks mixed when currencies differ', () => {
    const r = resolveSingleCurrencyCode(['GBP', 'USD']);
    expect(r.mixed).toBe(true);
    expect(r.code).toBe(DEFAULT_INVOICE_CURRENCY);
  });

  it('formatCurrencyAmount formats GBP with pound symbol', () => {
    const s = formatCurrencyAmount(3, 'GBP');
    expect(s).toContain('3');
    expect(s).not.toContain('$');
  });
});
