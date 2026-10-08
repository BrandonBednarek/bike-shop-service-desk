import { listUsers } from '#lib/api/users.js';
import { listOpenWorkOrders } from '#lib/api/work-orders.js';
import type { PageLoad } from './$types';

// The board needs the open jobs, and everyone's name to show who each job is assigned to.
// Pages that change a job call invalidate('app:work-orders'), so the board never shows an old copy.
export const load: PageLoad = async ({ fetch, depends }) => {
	depends('app:work-orders');
	const [workOrders, users] = await Promise.all([listOpenWorkOrders(fetch), listUsers(fetch)]);
	return { workOrders, users };
};
