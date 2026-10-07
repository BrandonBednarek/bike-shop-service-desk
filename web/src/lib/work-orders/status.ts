import type { WorkOrder } from '../api/types';

/** Collected or cancelled: the same rule as the API's WorkOrder.IsClosed. */
export function isClosed(workOrder: WorkOrder): boolean {
	return workOrder.status === 'Collected' || workOrder.status === 'Cancelled';
}
