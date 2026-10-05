import { redirect } from '@sveltejs/kit';
import { findCurrentUser } from '#lib/api/auth.js';
import type { PageLoad } from './$types';

// Someone already signed in has nothing to do here, so send them to the job board.
export const load: PageLoad = async ({ fetch }) => {
	if (await findCurrentUser(fetch)) {
		redirect(307, '/');
	}
};
