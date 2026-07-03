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
/**
 * Picks one ISO code when all sources agree; otherwise marks mixed (caller may fall back to USD).
 */
export function resolveSingleCurrencyCode(
  sources: (string | null | undefined)[]
): { code: string; mixed: boolean } {
  const codes = [
    ...new Set(
      sources
        .map((c) => (c ?? '').trim().toUpperCase())
        .filter((c) => c.length === 3)
    )
  ];
  if (codes.length === 1) {
    return { code: codes[0], mixed: false };
  }
  if (codes.length > 1) {
    return { code: DEFAULT_INVOICE_CURRENCY, mixed: true };
  }
  return { code: DEFAULT_INVOICE_CURRENCY, mixed: false };
}

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

/**
 * PrimeIcons class for a given ISO currency. Falls back to a generic dollar
 * icon for unknown codes. Used by stat cards / KPIs that previously hard-coded
 * `pi pi-dollar` regardless of the underlying currency.
 */
/** Compact axis/tooltip label for charts (e.g. £13k). When mixed, omits symbol. */
export function compactCurrencyLabel(
  value: number,
  currencyCode?: string | null,
  mixed = false
): string {
  if (mixed) {
    return Math.abs(value) >= 1000 ? (value / 1000).toFixed(value >= 10000 ? 0 : 1) + 'k' : value.toString();
  }
  const code = currencyCode ?? DEFAULT_INVOICE_CURRENCY;
  if (Math.abs(value) >= 1000) {
    const sym = formatCurrencyAmount(0, code).replace(/[\d.,\s\u00a0]/g, '');
    return sym + (value / 1000).toFixed(value >= 10000 ? 0 : 1) + 'k';
  }
  return formatCurrencyAmount(value, code);
}

export function currencyIconClass(currencyCode?: string | null): string {
  const c = (currencyCode || DEFAULT_INVOICE_CURRENCY).trim().toUpperCase();
  switch (c) {
    case 'EUR':
      return 'pi pi-euro';
    case 'GBP':
      return 'pi pi-pound';
    case 'JPY':
      return 'pi pi-yen';
    case 'INR':
    case 'PKR':
      return 'pi pi-money-bill';
    default:
      return 'pi pi-dollar';
  }
}
