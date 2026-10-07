const canadianDollars = new Intl.NumberFormat('en-CA', { style: 'currency', currency: 'CAD' });

/** "$160.75" for 16075. */
export function formatCents(cents: number): string {
	return canadianDollars.format(cents / 100);
}

export function toCents(dollars: number): number {
	return Math.round(dollars * 100);
}
