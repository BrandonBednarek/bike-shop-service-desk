import { getJson, postJson, type Fetch } from './client';
import type { WorkOrder, WorkOrderDetails } from './types';

/** Every job not yet collected or cancelled, soonest promised first. */
export function listOpenWorkOrders(fetch: Fetch): Promise<WorkOrder[]> {
	return getJson<WorkOrder[]>(fetch, '/api/work-orders');
}

export function getWorkOrder(fetch: Fetch, id: number): Promise<WorkOrder> {
	return getJson<WorkOrder>(fetch, `/api/work-orders/${id}`);
}

export function checkIn(fetch: Fetch, details: WorkOrderDetails): Promise<WorkOrder> {
	return postJson<WorkOrder>(fetch, '/api/work-orders', details);
}
