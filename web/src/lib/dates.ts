// The API sends calendar dates, such as a promised date, as "yyyy-mm-dd" with no time or
// time zone. The shop's time zone is the browser's.

export function todayAsIsoDate(): string {
	const now = new Date();
	const month = String(now.getMonth() + 1).padStart(2, '0');
	const day = String(now.getDate()).padStart(2, '0');
	return `${now.getFullYear()}-${month}-${day}`;
}

/** "Thu 8 Oct" for "2026-10-08". */
export function formatShortDate(isoDate: string): string {
	const [year, month, day] = isoDate.split('-').map(Number);
	// Not new Date(isoDate): that means midnight UTC, which is the evening before in Ontario.
	return new Date(year, month - 1, day).toLocaleDateString('en-GB', {
		weekday: 'short',
		day: 'numeric',
		month: 'short'
	});
}
