import { goto } from '$app/navigation';

// Opens a job when its row in a table is clicked. Leaves a click on the job number to its own
// link, so the job isn't opened twice. Ignores a click that selected text, such as copying a
// phone number, or that held Ctrl, Cmd or Shift.
export function openJobFromRow(event: MouseEvent, workOrderId: number): void {
	const clickedLink = event.target instanceof Element && event.target.closest('a') !== null;
	const selectedText = (window.getSelection()?.toString() ?? '') !== '';
	const modifierHeld = event.ctrlKey || event.metaKey || event.shiftKey;
	if (clickedLink || selectedText || modifierHeld) {
		return;
	}
	goto(`/work-orders/${workOrderId}`);
}
