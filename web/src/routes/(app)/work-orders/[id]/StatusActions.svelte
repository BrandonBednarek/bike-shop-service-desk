<!--
	The buttons that move a job through its statuses. Which buttons show depends on the status,
	following the same table as the API's WorkOrderStatusTransitions (allowsAction); the API
	still checks every change. Starting work and marking a job ready happen straight away,
	because they're easy to undo. Hold, Collect, Cancel and Reopen open a dialog first: Hold
	needs a reason, Collect the POS receipt number and Cancel a reason, and asking first stops a
	misclick closing a job. After any change, changeWorkOrder reloads the job so the page shows
	its new status.
-->
<script lang="ts">
	import { describeFailure } from '#lib/api/api-error.js';
	import type { CurrentUser, HoldReason, WorkOrder, WorkOrderStatus } from '#lib/api/types.js';
	import {
		cancelWorkOrder,
		collectWorkOrder,
		holdWorkOrder,
		markWorkOrderReady,
		reopenWorkOrder,
		startWorkOrder
	} from '#lib/api/work-orders.js';
	import ActionDialog from '#lib/components/ActionDialog.svelte';
	import { formatCents } from '#lib/money.js';
	import { holdReasonLabels, statusLabels } from '#lib/work-orders/labels.js';
	import { changeWorkOrder } from '#lib/work-orders/change.js';
	import { allowsAction, canChange } from '#lib/work-orders/status.js';

	let { workOrder, currentUser }: { workOrder: WorkOrder; currentUser: CurrentUser } = $props();

	type DialogAction = 'hold' | 'collect' | 'cancel' | 'reopen';

	// "Start" means something different depending on where the job is coming from.
	const startLabels: Partial<Record<WorkOrderStatus, string>> = {
		CheckedIn: 'Start work',
		OnHold: 'Resume work',
		ReadyForPickup: 'Back to work'
	};

	const holdReasons = Object.keys(holdReasonLabels) as HoldReason[];

	let openDialog = $state<DialogAction | null>(null);

	// The dialogs' fields, cleared each time a dialog opens.
	let holdReason = $state<HoldReason>('WaitingForParts');
	let holdNote = $state('');
	let posReceiptNumber = $state('');
	let cancellationReason = $state('');

	let errorMessage = $state('');
	let busy = $state(false);

	function showDialog(action: DialogAction) {
		holdReason = 'WaitingForParts';
		holdNote = '';
		posReceiptNumber = '';
		cancellationReason = '';
		errorMessage = '';
		openDialog = action;
	}

	// For the buttons without a dialog: shows the API's message here if the change fails.
	async function changeNow(callApi: () => Promise<WorkOrder>) {
		busy = true;
		errorMessage = '';
		try {
			await changeWorkOrder(callApi);
		} catch (error) {
			errorMessage = describeFailure(error);
		} finally {
			busy = false;
		}
	}
</script>

<div class="mt-4 flex flex-wrap items-center gap-2 text-sm font-medium">
	{#if allowsAction(workOrder, 'collect')}
		<button
			type="button"
			disabled={busy}
			onclick={() => showDialog('collect')}
			class="rounded-md bg-slate-900 px-3 py-1.5 text-white hover:bg-slate-800 disabled:opacity-60"
		>
			Mark collected
		</button>
	{/if}
	{#if allowsAction(workOrder, 'start')}
		<button
			type="button"
			disabled={busy}
			onclick={() => changeNow(() => startWorkOrder(fetch, workOrder.id))}
			class="rounded-md bg-slate-900 px-3 py-1.5 text-white hover:bg-slate-800 disabled:opacity-60"
		>
			{startLabels[workOrder.status]}
		</button>
	{/if}
	{#if allowsAction(workOrder, 'markReady')}
		<button
			type="button"
			disabled={busy}
			onclick={() => changeNow(() => markWorkOrderReady(fetch, workOrder.id))}
			class="rounded-md bg-slate-900 px-3 py-1.5 text-white hover:bg-slate-800 disabled:opacity-60"
		>
			Mark ready for pickup
		</button>
	{/if}
	{#if allowsAction(workOrder, 'hold')}
		<button
			type="button"
			disabled={busy}
			onclick={() => showDialog('hold')}
			class="rounded-md border border-slate-300 bg-white px-3 py-1.5 text-slate-700 hover:bg-slate-100 disabled:opacity-60"
		>
			Put on hold
		</button>
	{/if}
	{#if allowsAction(workOrder, 'cancel')}
		<button
			type="button"
			disabled={busy}
			onclick={() => showDialog('cancel')}
			class="rounded-md border border-red-300 bg-white px-3 py-1.5 text-red-700 hover:bg-red-50 disabled:opacity-60"
		>
			Cancel job
		</button>
	{/if}
	{#if allowsAction(workOrder, 'reopen') && canChange(workOrder, currentUser)}
		<button
			type="button"
			disabled={busy}
			onclick={() => showDialog('reopen')}
			class="rounded-md border border-slate-300 bg-white px-3 py-1.5 text-slate-700 hover:bg-slate-100 disabled:opacity-60"
		>
			Reopen job
		</button>
	{/if}
</div>

{#if !canChange(workOrder, currentUser)}
	<p class="mt-2 text-sm text-slate-600">
		Collected and cancelled jobs can only be changed by the owner.
	</p>
{/if}

{#if errorMessage}
	<p role="alert" class="mt-2 max-w-xl rounded-md bg-red-50 px-3 py-2 text-sm text-red-800">
		{errorMessage}
	</p>
{/if}

{#if openDialog === 'hold'}
	<ActionDialog
		title="Put job #{workOrder.id} on hold"
		confirmLabel="Put on hold"
		onconfirm={() =>
			changeWorkOrder(() =>
				holdWorkOrder(fetch, workOrder.id, {
					reason: holdReason,
					note: holdNote.trim() || null
				})
			)}
		onclose={() => (openDialog = null)}
	>
		<label class="block font-medium text-slate-700">
			Reason
			<select
				bind:value={holdReason}
				class="mt-1 block w-full rounded-md border-slate-300 text-sm"
			>
				{#each holdReasons as reason (reason)}
					<option value={reason}>{holdReasonLabels[reason]}</option>
				{/each}
			</select>
		</label>
		<label class="block font-medium text-slate-700">
			Note (optional, saved to the job's notes)
			<textarea
				bind:value={holdNote}
				maxlength="1000"
				rows="3"
				class="mt-1 block w-full rounded-md border-slate-300 text-sm"></textarea>
		</label>
	</ActionDialog>
{:else if openDialog === 'collect'}
	<ActionDialog
		title="Mark job #{workOrder.id} collected"
		confirmLabel="Mark collected"
		onconfirm={() =>
			changeWorkOrder(() => collectWorkOrder(fetch, workOrder.id, posReceiptNumber.trim()))}
		onclose={() => (openDialog = null)}
	>
		<p>
			The bill is {formatCents(workOrder.billCents)}. Take payment in the POS, then enter its
			receipt number.
		</p>
		<label class="block font-medium text-slate-700">
			POS receipt number
			<input
				bind:value={posReceiptNumber}
				required
				maxlength="50"
				class="mt-1 block w-full rounded-md border-slate-300 text-sm"
			/>
		</label>
	</ActionDialog>
{:else if openDialog === 'cancel'}
	<ActionDialog
		title="Cancel job #{workOrder.id}"
		confirmLabel="Cancel job"
		danger
		onconfirm={() =>
			changeWorkOrder(() => cancelWorkOrder(fetch, workOrder.id, cancellationReason.trim()))}
		onclose={() => (openDialog = null)}
	>
		<label class="block font-medium text-slate-700">
			Why is it being cancelled?
			<textarea
				bind:value={cancellationReason}
				required
				maxlength="500"
				rows="3"
				class="mt-1 block w-full rounded-md border-slate-300 text-sm"></textarea>
		</label>
	</ActionDialog>
{:else if openDialog === 'reopen'}
	<ActionDialog
		title="Reopen job #{workOrder.id}"
		confirmLabel="Reopen job"
		onconfirm={() => changeWorkOrder(() => reopenWorkOrder(fetch, workOrder.id))}
		onclose={() => (openDialog = null)}
	>
		{#if workOrder.status === 'Collected'}
			<p>
				It goes back to {statusLabels.ReadyForPickup}, and its POS receipt number is
				cleared. Any refund or change to the sale is made in the POS.
			</p>
		{:else}
			<p>It goes back to {statusLabels.CheckedIn}, and its cancellation reason is cleared.</p>
		{/if}
	</ActionDialog>
{/if}
