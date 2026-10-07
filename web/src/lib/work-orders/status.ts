import type { CurrentUser, WorkOrder } from '../api/types';

/** Collected or cancelled: the same rule as the API's WorkOrder.IsClosed. */
export function isClosed(workOrder: WorkOrder): boolean {
	return workOrder.status === 'Collected' || workOrder.status === 'Cancelled';
}

/** Staff can change open jobs and the owner any job: the same rule as the API's ApplyAsync. */
export function canChange(workOrder: WorkOrder, user: CurrentUser): boolean {
	return !isClosed(workOrder) || user.role === 'Owner';
}
