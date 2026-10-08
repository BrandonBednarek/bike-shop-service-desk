import type { CurrentUser, WorkOrder, WorkOrderStatus } from '../api/types';

export type StatusAction = 'start' | 'hold' | 'markReady' | 'collect' | 'cancel' | 'reopen';

// The same moves as the API's WorkOrderStatusTransitions.
const actionsByStatus: Record<WorkOrderStatus, StatusAction[]> = {
	CheckedIn: ['start', 'hold', 'cancel'],
	InProgress: ['markReady', 'hold', 'cancel'],
	OnHold: ['start', 'markReady', 'cancel'],
	ReadyForPickup: ['collect', 'start'],
	Collected: ['reopen'],
	Cancelled: ['reopen']
};

export function allowsAction(workOrder: WorkOrder, action: StatusAction): boolean {
	return actionsByStatus[workOrder.status].includes(action);
}

/** Collected or cancelled: the same rule as the API's WorkOrder.IsClosed. */
export function isClosed(workOrder: WorkOrder): boolean {
	return workOrder.status === 'Collected' || workOrder.status === 'Cancelled';
}

/** Staff can change open jobs and the owner any job: the same rule as the API's ApplyAsync. */
export function canChange(workOrder: WorkOrder, user: CurrentUser): boolean {
	return !isClosed(workOrder) || user.role === 'Owner';
}
