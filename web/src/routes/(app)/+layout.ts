import { redirect } from '@sveltejs/kit';
import { findCurrentUser } from '#lib/api/auth.js';
import type { LayoutLoad } from './$types';

// Every page in the (app) group requires a signed-in user.
export const load: LayoutLoad = async ({ fetch }) => {
	const currentUser = await findCurrentUser(fetch);
	if (!currentUser) {
		redirect(307, '/sign-in');
	}
	return { currentUser };
};
