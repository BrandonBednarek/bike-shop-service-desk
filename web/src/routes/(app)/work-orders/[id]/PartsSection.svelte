<!--
	The parts used on a job. Their total is the "Parts used" line of the bill. Anyone who can
	change the job can add a part below the table, and remove one after a confirmation.
	If a new part would take the bill over the estimate, a dialog asks first: add it anyway (a
	judgement call for something small, like a $3 cable), or put the job on hold to ask the
	customer, with a note saying what to ask. The API itself never blocks a part. Each change
	goes through changeWorkOrder, which reloads the job.
-->
<script lang="ts">
	import { describeFailure } from '#lib/api/api-error.js';
	import type { CurrentUser, NewPart, PartLine, WorkOrder } from '#lib/api/types.js';
	import { addPart, holdWorkOrder, removePart } from '#lib/api/work-orders.js';
	import ActionDialog from '#lib/components/ActionDialog.svelte';
	import { formatDateTime } from '#lib/dates.js';
	import { formatCents, toCents } from '#lib/money.js';
	import { changeWorkOrder } from '#lib/work-orders/change.js';
	import { allowsAction, canChange } from '#lib/work-orders/status.js';

	let { workOrder, currentUser }: { workOrder: WorkOrder; currentUser: CurrentUser } = $props();

	let editable = $derived(canChange(workOrder, currentUser));
	let canHold = $derived(allowsAction(workOrder, 'hold'));

	let description = $state('');
	let quantity = $state<number | null>(1);
	let unitPriceDollars = $state<number | null>(null);
	let errorMessage = $state('');
	let saving = $state(false);

	let partToRemove = $state<PartLine | null>(null);
	// A part that would go over the estimate waits here while the dialog asks what to do.
	let partOverEstimate = $state<NewPart | null>(null);
	let overEstimateChoice = $state<'add' | 'hold'>('add');
	// If the job can't be put on hold, perhaps because someone else just did, adding is the only
	// choice left, whatever was picked before.
	let effectiveChoice = $derived(canHold ? overEstimateChoice : 'add');

	async function handleAddPart(event: SubmitEvent) {
		event.preventDefault();
		errorMessage = '';
		// The required checks have already run; this tells TypeScript the numbers are set.
		if (quantity === null || unitPriceDollars === null) {
			return;
		}
		const part = {
			description: description.trim(),
			quantity,
			unitPriceCents: toCents(unitPriceDollars)
		};
		if (billWith(part) > workOrder.estimateTotalCents) {
			overEstimateChoice = 'add';
			partOverEstimate = part;
			return;
		}
		saving = true;
		try {
			await changeWorkOrder(() => addPart(fetch, workOrder.id, part));
			clearForm();
		} catch (error) {
			errorMessage = describeFailure(error);
		} finally {
			saving = false;
		}
	}

	// Runs when the over-estimate dialog is confirmed. Errors are shown in the dialog.
	async function resolveOverEstimate(part: NewPart) {
		if (effectiveChoice === 'add') {
			await changeWorkOrder(() => addPart(fetch, workOrder.id, part));
		} else {
			await changeWorkOrder(() =>
				holdWorkOrder(fetch, workOrder.id, {
					reason: 'WaitingForCustomer',
					note: holdNoteFor(part)
				})
			);
		}
		clearForm();
	}

	function billWith(part: NewPart): number {
		return workOrder.billCents + costOf(part);
	}

	function costOf(part: NewPart): number {
		return part.quantity * part.unitPriceCents;
	}

	function amountOverEstimate(part: NewPart): number {
		return billWith(part) - workOrder.estimateTotalCents;
	}

	// "1 × Brake cable ($12.00)". A PartLine has the same fields, so it can be passed in too.
	function describePart(part: NewPart): string {
		return `${part.quantity} × ${part.description} (${formatCents(costOf(part))})`;
	}

	function holdNoteFor(part: NewPart): string {
		return (
			`Ask the customer about ${describePart(part)}. ` +
			`It would take the bill ${formatCents(amountOverEstimate(part))} over the estimate.`
		);
	}

	function clearForm() {
		description = '';
		quantity = 1;
		unitPriceDollars = null;
	}
</script>

<section class="mt-8">
	<h2 class="text-lg font-semibold text-slate-900">Parts</h2>
	{#if workOrder.partLines.length === 0}
		<p class="mt-2 text-sm text-slate-600">No parts added yet.</p>
	{:else}
		<div class="mt-2 overflow-x-auto rounded-lg border border-slate-200 bg-white">
			<table class="min-w-full divide-y divide-slate-200 text-sm">
				<thead class="bg-slate-50 text-left text-xs font-medium text-slate-500 uppercase">
					<tr>
						<th scope="col" class="px-4 py-2">Part</th>
						<th scope="col" class="px-4 py-2 text-right">Quantity</th>
						<th scope="col" class="px-4 py-2 text-right">Each</th>
						<th scope="col" class="px-4 py-2 text-right">Total</th>
						<th scope="col" class="px-4 py-2">Added</th>
						{#if editable}
							<th scope="col" class="px-4 py-2"
								><span class="sr-only">Remove</span></th
							>
						{/if}
					</tr>
				</thead>
				<tbody class="divide-y divide-slate-100">
					{#each workOrder.partLines as part (part.id)}
						<tr>
							<td class="px-4 py-2 text-slate-900">
								{part.description}
							</td>
							<td class="px-4 py-2 text-right text-slate-900">
								{part.quantity}
							</td>
							<td class="px-4 py-2 text-right text-slate-900">
								{formatCents(part.unitPriceCents)}
							</td>
							<td class="px-4 py-2 text-right text-slate-900">
								{formatCents(part.totalCents)}
							</td>
							<td class="px-4 py-2 text-slate-500">
								{formatDateTime(part.addedAtUtc)}
							</td>
							{#if editable}
								<td class="px-4 py-2 text-right">
									<button
										type="button"
										onclick={() => (partToRemove = part)}
										class="text-red-700 hover:underline"
									>
										Remove
										<span class="sr-only">{part.description}</span>
									</button>
								</td>
							{/if}
						</tr>
					{/each}
				</tbody>
			</table>
		</div>
	{/if}

	{#if editable}
		<form onsubmit={handleAddPart} class="mt-3 flex flex-wrap items-end gap-3 text-sm">
			<label class="block min-w-48 flex-1 font-medium text-slate-700">
				Part
				<input
					bind:value={description}
					required
					maxlength="200"
					class="mt-1 block w-full rounded-md border-slate-300 text-sm"
				/>
			</label>
			<label class="block font-medium text-slate-700">
				Quantity
				<input
					type="number"
					bind:value={quantity}
					required
					min="1"
					max="1000"
					step="1"
					class="mt-1 block w-24 rounded-md border-slate-300 text-sm"
				/>
			</label>
			<label class="block font-medium text-slate-700">
				Each ($)
				<input
					type="number"
					bind:value={unitPriceDollars}
					required
					min="0"
					max="100000"
					step="0.01"
					class="mt-1 block w-28 rounded-md border-slate-300 text-sm"
				/>
			</label>
			<button
				type="submit"
				disabled={saving}
				class="rounded-md bg-slate-900 px-3 py-2 font-medium text-white hover:bg-slate-800 disabled:opacity-60"
			>
				Add part
			</button>
		</form>
	{/if}
	<!-- Outside the form's block, so a refusal that closed the job meanwhile is still shown. -->
	{#if errorMessage}
		<p role="alert" class="mt-2 max-w-xl rounded-md bg-red-50 px-3 py-2 text-sm text-red-800">
			{errorMessage}
		</p>
	{/if}
</section>

<!-- {@const} names the part for its block, so the closures below see it as never null. -->
{#if partOverEstimate}
	{@const part = partOverEstimate}
	<ActionDialog
		title="This part goes over the estimate"
		confirmLabel={effectiveChoice === 'add' ? 'Add to the bill' : 'Put on hold'}
		onconfirm={() => resolveOverEstimate(part)}
		onclose={() => (partOverEstimate = null)}
	>
		<p>
			{describePart(part)} takes the bill to {formatCents(billWith(part))}, which is
			{formatCents(amountOverEstimate(part))} over the
			{formatCents(workOrder.estimateTotalCents)} estimate.
		</p>
		<!-- Holding is only offered when the job's status allows it. -->
		{#if canHold}
			<fieldset class="space-y-2">
				<legend class="sr-only">What to do</legend>
				<label class="flex items-start gap-2">
					<input
						type="radio"
						bind:group={overEstimateChoice}
						value="add"
						class="mt-0.5"
					/>
					<span>Add it to the bill. Fine for something small, like a $3 cable.</span>
				</label>
				<label class="flex items-start gap-2">
					<input
						type="radio"
						bind:group={overEstimateChoice}
						value="hold"
						class="mt-0.5"
					/>
					<span>
						Put the job on hold, waiting for the customer, and ask them first. A note
						says what to ask, and the part isn't added.
					</span>
				</label>
			</fieldset>
		{/if}
	</ActionDialog>
{:else if partToRemove}
	{@const part = partToRemove}
	<ActionDialog
		title="Remove this part?"
		confirmLabel="Remove"
		danger
		onconfirm={() => changeWorkOrder(() => removePart(fetch, workOrder.id, part.id))}
		onclose={() => (partToRemove = null)}
	>
		<p>{describePart(part)}. The bill goes down by {formatCents(part.totalCents)}.</p>
	</ActionDialog>
{/if}
