// The API sends calendar dates, such as a promised date, as "yyyy-mm-dd" with no time or
// time zone, and timestamps in UTC. Timestamps are shown, and today is worked out, in the
// browser's time zone, which is the shop's.

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

/** "Tue 6 Oct, 19:42" in Ontario for a UTC timestamp such as "2026-10-06T23:42:12Z". */
export function formatDateTime(utcTimestamp: string): string {
	return new Date(utcTimestamp).toLocaleString('en-GB', {
		weekday: 'short',
		day: 'numeric',
		month: 'short',
		hour: '2-digit',
		minute: '2-digit'
	});
}

/** "1 h 15 min" for 75 minutes. */
export function formatDuration(minutes: number): string {
	const hours = Math.floor(minutes / 60);
	const remainingMinutes = minutes % 60;
	if (hours === 0) {
		return `${remainingMinutes} min`;
	}
	return remainingMinutes === 0 ? `${hours} h` : `${hours} h ${remainingMinutes} min`;
}
