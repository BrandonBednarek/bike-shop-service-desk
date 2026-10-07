import { listUsers } from '#lib/api/users.js';
import type { PageLoad } from './$types';

// The job form needs the staff list to offer who the job can be assigned to.
export const load: PageLoad = async ({ fetch }) => {
	return { users: await listUsers(fetch) };
};
