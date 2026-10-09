import { redirect } from '@sveltejs/kit';
import { findCurrentUser } from '#lib/api/auth.js';
import { listDemoAccounts } from '#lib/api/demo.js';
import type { DemoAccount } from '#lib/api/types.js';
import type { PageLoad } from './$types';

// Someone already signed in has nothing to do here, so send them to the job board.
export const load: PageLoad = async ({ fetch }) => {
	const [currentUser, demoAccounts] = await Promise.all([
		findCurrentUser(fetch),
		// The demo panel is an extra, so if it can't load, the page shows the form without it.
		listDemoAccounts(fetch).catch((): DemoAccount[] => [])
	]);
	if (currentUser) {
		redirect(307, '/');
	}
	return { demoAccounts };
};
