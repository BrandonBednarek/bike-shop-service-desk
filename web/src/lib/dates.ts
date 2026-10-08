// The API sends calendar dates, such as a promised date, as "yyyy-mm-dd" with no time or
// time zone, and timestamps in UTC. Timestamps are shown, and today is worked out, in the
// browser's time zone, which is the shop's.

const millisecondsPerDay = 24 * 60 * 60 * 1000;

export function todayAsIsoDate(): string {
	return toIsoDate(new Date());
}

/** The shop's date when a UTC timestamp happened, as "yyyy-mm-dd". */
export function isoDateOfTimestamp(utcTimestamp: string): string {
	return toIsoDate(new Date(utcTimestamp));
}

/** Whole days from one "yyyy-mm-dd" date to a later one. */
export function daysBetween(earlierIsoDate: string, laterIsoDate: string): number {
	const difference = fromIsoDate(laterIsoDate).getTime() - fromIsoDate(earlierIsoDate).getTime();
	// Rounded, because a day that changes the clocks is an hour shorter or longer.
	return Math.round(difference / millisecondsPerDay);
}

/** "Thu 8 Oct" for "2026-10-08". */
export function formatShortDate(isoDate: string): string {
	return fromIsoDate(isoDate).toLocaleDateString('en-GB', {
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

/** "today", "1 day" or "3 days". */
export function formatDays(days: number): string {
	if (days === 0) {
		return 'today';
	}
	return days === 1 ? '1 day' : `${days} days`;
}

function toIsoDate(date: Date): string {
	const month = String(date.getMonth() + 1).padStart(2, '0');
	const day = String(date.getDate()).padStart(2, '0');
	return `${date.getFullYear()}-${month}-${day}`;
}

// Not new Date(isoDate): that means midnight UTC, which is the evening before in Ontario.
function fromIsoDate(isoDate: string): Date {
	const [year, month, day] = isoDate.split('-').map(Number);
	return new Date(year, month - 1, day);
}
