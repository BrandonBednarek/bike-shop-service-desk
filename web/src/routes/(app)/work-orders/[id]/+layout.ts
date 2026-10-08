import { error } from '@sveltejs/kit';
import { listUsers } from '#lib/api/users.js';
import { getWorkOrder } from '#lib/api/work-orders.js';
import type { LayoutLoad } from './$types';

// Loads the job and the staff list for the job page and its edit page, so moving between them
// doesn't load them again. A job number that isn't digits gets the same 404 as an unknown one.
export const load: LayoutLoad = async ({ params, fetch, depends }) => {
	depends('app:work-orders');
	if (!/^\d+$/.test(params.id)) {
		error(404, "There's no job with that number.");
	}
	const [workOrder, users] = await Promise.all([
		getWorkOrder(fetch, Number(params.id)),
		listUsers(fetch)
	]);
	return { workOrder, users };
};
