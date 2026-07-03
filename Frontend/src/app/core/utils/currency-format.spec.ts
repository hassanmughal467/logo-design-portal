import {
  DEFAULT_INVOICE_CURRENCY,
  currencyIconClass,
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

  describe('currencyIconClass', () => {
    it('returns pi-pound for GBP', () => {
      expect(currencyIconClass('GBP')).toBe('pi pi-pound');
      expect(currencyIconClass('gbp')).toBe('pi pi-pound');
    });

    it('returns pi-euro for EUR', () => {
      expect(currencyIconClass('EUR')).toBe('pi pi-euro');
    });

    it('returns pi-yen for JPY', () => {
      expect(currencyIconClass('JPY')).toBe('pi pi-yen');
    });

    it('returns pi-money-bill for INR/PKR', () => {
      expect(currencyIconClass('INR')).toBe('pi pi-money-bill');
      expect(currencyIconClass('PKR')).toBe('pi pi-money-bill');
    });

    it('falls back to pi-dollar for USD/unknown/null', () => {
      expect(currencyIconClass('USD')).toBe('pi pi-dollar');
      expect(currencyIconClass('XYZ')).toBe('pi pi-dollar');
      expect(currencyIconClass(null)).toBe('pi pi-dollar');
      expect(currencyIconClass(undefined)).toBe('pi pi-dollar');
      expect(currencyIconClass('')).toBe('pi pi-dollar');
    });
  });
});
