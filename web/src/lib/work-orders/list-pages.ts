/** A page that lists jobs, which a job's page links back to. */
export interface ListPage {
	href: string;
	label: string;
}

export const jobBoardPage: ListPage = { href: '/', label: 'Job board' };
export const dashboardPage: ListPage = { href: '/dashboard', label: 'Dashboard' };

// Kept in memory, so it lasts through a trip to the edit page. A refresh, or a job opened in a
// new tab or from a pasted link, goes back to the job board. Module state like this is safe
// because pages only render in the browser; on a server it would be shared by everyone's requests.
let lastListPage: ListPage = jobBoardPage;

/** Called by the job board and the dashboard when they're shown. */
export function rememberListPage(listPage: ListPage): void {
	lastListPage = listPage;
}

/** The list a job's page links back to: the last one shown in this tab. */
export function listPageToReturnTo(): ListPage {
	return lastListPage;
}
