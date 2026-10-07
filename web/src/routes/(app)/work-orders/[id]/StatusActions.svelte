<!--
	The buttons that move a job through its statuses. Which buttons show depends on the status,
	following the same table as the API's WorkOrderStatusTransitions; the API still checks every
	change. Starting work and marking a job ready happen straight away, because they're easy to
	undo. Hold, Collect, Cancel and Reopen open a dialog first: Hold needs a reason, Collect the
	POS receipt number and Cancel a reason, and asking first stops a misclick closing a job.
	After any change, invalidate('app:work-orders') reloads this page and throws away any copy of
	the job board loaded earlier, so the board loads fresh next time it's opened.
-->
<script lang="ts">
	import { invalidate } from '$app/navigation';
	import { ApiError, describeFailure } from '#lib/api/api-error.js';
	import type { HoldReason, WorkOrder, WorkOrderStatus } from '#lib/api/types.js';
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
	import { isClosed } from '#lib/work-orders/status.js';

	let { workOrder, isOwner }: { workOrder: WorkOrder; isOwner: boolean } = $props();

	type StatusAction = 'start' | 'hold' | 'markReady' | 'collect' | 'cancel' | 'reopen';
	type DialogAction = 'hold' | 'collect' | 'cancel' | 'reopen';

	const actionsByStatus: Record<WorkOrderStatus, StatusAction[]> = {
		CheckedIn: ['start', 'hold', 'cancel'],
		InProgress: ['markReady', 'hold', 'cancel'],
		OnHold: ['start', 'markReady', 'cancel'],
		ReadyForPickup: ['collect', 'start'],
		Collected: ['reopen'],
		Cancelled: ['reopen']
	};

	// "Start" means something different depending on where the job is coming from.
	const startLabels: Partial<Record<WorkOrderStatus, string>> = {
		CheckedIn: 'Start work',
		OnHold: 'Resume work',
		ReadyForPickup: 'Back to work'
	};

	const holdReasons = Object.keys(holdReasonLabels) as HoldReason[];

	let availableActions = $derived(actionsByStatus[workOrder.status]);
	let openDialog = $state<DialogAction | null>(null);

	// The dialogs' fields, cleared each time a dialog opens.
	let holdReason = $state<HoldReason>('WaitingForParts');
	let holdNote = $state('');
	let posReceiptNumber = $state('');
	let cancellationReason = $state('');

	let errorMessage = $state('');
	let busy = $state(false);

	function canDo(action: StatusAction): boolean {
		return availableActions.includes(action);
	}

	function showDialog(action: DialogAction) {
		holdReason = 'WaitingForParts';
		holdNote = '';
		posReceiptNumber = '';
		cancellationReason = '';
		errorMessage = '';
		openDialog = action;
	}

	// Makes the change, then reloads the job so the page shows its new status.
	async function change(callApi: () => Promise<WorkOrder>) {
		try {
			await callApi();
		} catch (error) {
			// The API refused, perhaps because someone else changed the job first, so reload it to show
			// its real status next to the message. A failed connection would fail the reload too.
			if (error instanceof ApiError) {
				await invalidate('app:work-orders');
			}
			throw error;
		}
		await invalidate('app:work-orders');
	}

	// For the buttons without a dialog: shows the API's message here if the change fails.
	async function changeNow(callApi: () => Promise<WorkOrder>) {
		busy = true;
		errorMessage = '';
		try {
			await change(callApi);
		} catch (error) {
			errorMessage = describeFailure(error);
		} finally {
			busy = false;
		}
	}
</script>

<div class="mt-4 flex flex-wrap items-center gap-2 text-sm font-medium">
	{#if canDo('collect')}
		<button
			type="button"
			disabled={busy}
			onclick={() => showDialog('collect')}
			class="rounded-md bg-slate-900 px-3 py-1.5 text-white hover:bg-slate-800 disabled:opacity-60"
		>
			Mark collected
		</button>
	{/if}
	{#if canDo('start')}
		<button
			type="button"
			disabled={busy}
			onclick={() => changeNow(() => startWorkOrder(fetch, workOrder.id))}
			class="rounded-md bg-slate-900 px-3 py-1.5 text-white hover:bg-slate-800 disabled:opacity-60"
		>
			{startLabels[workOrder.status]}
		</button>
	{/if}
	{#if canDo('markReady')}
		<button
			type="button"
			disabled={busy}
			onclick={() => changeNow(() => markWorkOrderReady(fetch, workOrder.id))}
			class="rounded-md bg-slate-900 px-3 py-1.5 text-white hover:bg-slate-800 disabled:opacity-60"
		>
			Mark ready for pickup
		</button>
	{/if}
	{#if canDo('hold')}
		<button
			type="button"
			disabled={busy}
			onclick={() => showDialog('hold')}
			class="rounded-md border border-slate-300 bg-white px-3 py-1.5 text-slate-700 hover:bg-slate-100 disabled:opacity-60"
		>
			Put on hold
		</button>
	{/if}
	{#if canDo('cancel')}
		<button
			type="button"
			disabled={busy}
			onclick={() => showDialog('cancel')}
			class="rounded-md border border-red-300 bg-white px-3 py-1.5 text-red-700 hover:bg-red-50 disabled:opacity-60"
		>
			Cancel job
		</button>
	{/if}
	{#if canDo('reopen') && isOwner}
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

{#if isClosed(workOrder) && !isOwner}
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
			change(() =>
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
			change(() => collectWorkOrder(fetch, workOrder.id, posReceiptNumber.trim()))}
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
			change(() => cancelWorkOrder(fetch, workOrder.id, cancellationReason.trim()))}
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
		onconfirm={() => change(() => reopenWorkOrder(fetch, workOrder.id))}
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
