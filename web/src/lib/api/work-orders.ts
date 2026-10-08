import { deleteJson, getJson, postJson, putJson, type Fetch } from './client';
import type { HoldReason, NewPart, WorkOrder, WorkOrderDetails } from './types';

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

export function updateWorkOrderDetails(
	fetch: Fetch,
	id: number,
	details: WorkOrderDetails
): Promise<WorkOrder> {
	return putJson<WorkOrder>(fetch, `/api/work-orders/${id}`, details);
}

// Status changes. Each returns the job as it is after the change.

export function startWorkOrder(fetch: Fetch, id: number): Promise<WorkOrder> {
	return postJson<WorkOrder>(fetch, `/api/work-orders/${id}/start`);
}

export function holdWorkOrder(
	fetch: Fetch,
	id: number,
	hold: { reason: HoldReason; note: string | null }
): Promise<WorkOrder> {
	return postJson<WorkOrder>(fetch, `/api/work-orders/${id}/hold`, hold);
}

export function markWorkOrderReady(fetch: Fetch, id: number): Promise<WorkOrder> {
	return postJson<WorkOrder>(fetch, `/api/work-orders/${id}/mark-ready`);
}

export function collectWorkOrder(
	fetch: Fetch,
	id: number,
	posReceiptNumber: string
): Promise<WorkOrder> {
	return postJson<WorkOrder>(fetch, `/api/work-orders/${id}/collect`, { posReceiptNumber });
}

export function cancelWorkOrder(fetch: Fetch, id: number, reason: string): Promise<WorkOrder> {
	return postJson<WorkOrder>(fetch, `/api/work-orders/${id}/cancel`, { reason });
}

export function reopenWorkOrder(fetch: Fetch, id: number): Promise<WorkOrder> {
	return postJson<WorkOrder>(fetch, `/api/work-orders/${id}/reopen`);
}

// Labour, parts and notes. Each returns the job as it is after the change.

export function logLabour(
	fetch: Fetch,
	id: number,
	labour: { mechanicUserId: number | null; minutes: number; note: string | null }
): Promise<WorkOrder> {
	return postJson<WorkOrder>(fetch, `/api/work-orders/${id}/labour`, labour);
}

export function removeLabour(fetch: Fetch, id: number, entryId: number): Promise<WorkOrder> {
	return deleteJson<WorkOrder>(fetch, `/api/work-orders/${id}/labour/${entryId}`);
}

export function addPart(fetch: Fetch, id: number, part: NewPart): Promise<WorkOrder> {
	return postJson<WorkOrder>(fetch, `/api/work-orders/${id}/parts`, part);
}

export function removePart(fetch: Fetch, id: number, partId: number): Promise<WorkOrder> {
	return deleteJson<WorkOrder>(fetch, `/api/work-orders/${id}/parts/${partId}`);
}

export function addNote(fetch: Fetch, id: number, text: string): Promise<WorkOrder> {
	return postJson<WorkOrder>(fetch, `/api/work-orders/${id}/notes`, { text });
}
