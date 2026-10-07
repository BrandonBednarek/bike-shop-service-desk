import { error } from '@sveltejs/kit';
import { listUsers } from '#lib/api/users.js';
import { getWorkOrder } from '#lib/api/work-orders.js';
import type { PageLoad } from './$types';

// The staff list turns the user IDs on the job into names. A job number that isn't digits gets
// the same 404 as an unknown one, without calling the API.
export const load: PageLoad = async ({ params, fetch }) => {
	if (!/^\d+$/.test(params.id)) {
		error(404, "There's no job with that number.");
	}
	const [workOrder, users] = await Promise.all([
		getWorkOrder(fetch, Number(params.id)),
		listUsers(fetch)
	]);
	return { workOrder, users };
};
