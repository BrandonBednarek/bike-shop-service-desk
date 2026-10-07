import { listUsers } from '#lib/api/users.js';
import { listOpenWorkOrders } from '#lib/api/work-orders.js';
import type { PageLoad } from './$types';

// The board needs the open jobs, and everyone's name to show who each job is assigned to.
export const load: PageLoad = async ({ fetch }) => {
	const [workOrders, users] = await Promise.all([listOpenWorkOrders(fetch), listUsers(fetch)]);
	return { workOrders, users };
};
