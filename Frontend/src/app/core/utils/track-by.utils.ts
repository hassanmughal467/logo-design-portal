/** Stable *ngFor trackBy for items with string `id`. */
export function trackById<T extends { id: string }>(_index: number, item: T): string {
  return item.id;
}
