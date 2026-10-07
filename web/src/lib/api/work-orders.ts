import { getJson, type Fetch } from './client';
import type { WorkOrder } from './types';

/** Every job not yet collected or cancelled, soonest promised first. */
export function listOpenWorkOrders(fetch: Fetch): Promise<WorkOrder[]> {
	return getJson<WorkOrder[]>(fetch, '/api/work-orders');
}
