import type { WorkOrder, WorkOrderStatus } from '../api/types';

const notReadyStatuses: WorkOrderStatus[] = ['CheckedIn', 'InProgress', 'OnHold'];

/** Past its promised date and still not ready. "yyyy-mm-dd" dates compare correctly as text. */
export function isOverdue(workOrder: WorkOrder, today: string): boolean {
	return workOrder.promisedOn < today && notReadyStatuses.includes(workOrder.status);
}
