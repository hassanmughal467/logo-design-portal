/** Default when API omits currency (matches backend invoice settings fallback). */
export const DEFAULT_INVOICE_CURRENCY = 'USD';

export function localeForCurrency(code: string): string {
  const c = (code || DEFAULT_INVOICE_CURRENCY).toUpperCase();
  switch (c) {
    case 'GBP':
      return 'en-GB';
    case 'EUR':
      return 'de-DE';
    case 'USD':
      return 'en-US';
    case 'CAD':
      return 'en-CA';
    case 'AUD':
      return 'en-AU';
    case 'PKR':
      return 'en-PK';
    case 'INR':
      return 'en-IN';
    case 'JPY':
      return 'ja-JP';
    default:
      return 'en-US';
  }
}

/**
 * Formats a numeric amount with Intl (symbol + decimals). Unknown ISO codes fall back to USD.
 */
export function formatCurrencyAmount(amount: number, currencyCode?: string | null): string {
  const c =
    currencyCode && currencyCode.trim()
      ? currencyCode.trim().toUpperCase()
      : DEFAULT_INVOICE_CURRENCY;
  try {
    return new Intl.NumberFormat(localeForCurrency(c), {
      style: 'currency',
      currency: c
    }).format(amount);
  } catch {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: DEFAULT_INVOICE_CURRENCY
    }).format(amount);
  }
}
