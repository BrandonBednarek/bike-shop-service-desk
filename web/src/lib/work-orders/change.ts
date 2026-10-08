import { invalidate } from '$app/navigation';
import { ApiError } from '../api/api-error';
import type { WorkOrder } from '../api/types';

// Makes a change to a job, then reloads the job so the page shows the change.
export async function changeWorkOrder(callApi: () => Promise<WorkOrder>): Promise<void> {
	try {
		await callApi();
	} catch (error) {
		// The API refused, perhaps because someone else changed the job first, so reload it to show
		// its real status beside the message. A failed connection would fail the reload too.
		if (error instanceof ApiError) {
			await invalidate('app:work-orders');
		}
		throw error;
	}
	await invalidate('app:work-orders');
}
