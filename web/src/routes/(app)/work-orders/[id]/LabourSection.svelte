<!--
	The labour logged on a job: who did it, how long it took and any note. The customer pays the
	estimated labour, so this is the shop's record of the real time, not part of the bill.
	Anyone who can change the job can log time below the table, and remove an entry logged by
	mistake after a confirmation. Each change goes through changeWorkOrder, which reloads the job.
-->
<script lang="ts">
	import { describeFailure } from '#lib/api/api-error.js';
	import type { CurrentUser, LabourEntry, User, WorkOrder } from '#lib/api/types.js';
	import { logLabour, removeLabour } from '#lib/api/work-orders.js';
	import ActionDialog from '#lib/components/ActionDialog.svelte';
	import { formatDateTime, formatDuration } from '#lib/dates.js';
	import { displayNameOf } from '#lib/user-names.js';
	import { changeWorkOrder } from '#lib/work-orders/change.js';
	import { canChange } from '#lib/work-orders/status.js';

	interface Props {
		workOrder: WorkOrder;
		users: User[];
		currentUser: CurrentUser;
	}

	let { workOrder, users, currentUser }: Props = $props();

	let editable = $derived(canChange(workOrder, currentUser));
	let otherMechanics = $derived(
		users.filter((user) => user.isActive && user.id !== currentUser.id)
	);

	// null means whoever is signed in, which the API fills in.
	let mechanicUserId = $state<number | null>(null);
	let hours = $state<number | null>(null);
	let note = $state('');
	let errorMessage = $state('');
	let saving = $state(false);

	let entryToRemove = $state<LabourEntry | null>(null);

	// "30 min by Lebis"
	function describeEntry(entry: LabourEntry): string {
		return `${formatDuration(entry.minutes)} by ${displayNameOf(users, entry.mechanicUserId)}`;
	}

	async function handleLogLabour(event: SubmitEvent) {
		event.preventDefault();
		// The required check has already run; this tells TypeScript hours is set.
		if (hours === null) {
			return;
		}
		const labour = {
			mechanicUserId,
			minutes: Math.round(hours * 60),
			note: note.trim() || null
		};
		saving = true;
		errorMessage = '';
		try {
			await changeWorkOrder(() => logLabour(fetch, workOrder.id, labour));
			mechanicUserId = null;
			hours = null;
			note = '';
		} catch (error) {
			errorMessage = describeFailure(error);
		} finally {
			saving = false;
		}
	}
</script>

<section class="mt-8">
	<h2 class="text-lg font-semibold text-slate-900">Labour</h2>
	{#if workOrder.labourEntries.length === 0}
		<p class="mt-2 text-sm text-slate-600">No labour logged yet.</p>
	{:else}
		<div class="mt-2 overflow-x-auto rounded-lg border border-slate-200 bg-white">
			<table class="min-w-full divide-y divide-slate-200 text-sm">
				<thead class="bg-slate-50 text-left text-xs font-medium text-slate-500 uppercase">
					<tr>
						<th scope="col" class="px-4 py-2">Mechanic</th>
						<th scope="col" class="px-4 py-2">Time</th>
						<th scope="col" class="px-4 py-2">Note</th>
						<th scope="col" class="px-4 py-2">Logged</th>
						{#if editable}
							<th scope="col" class="px-4 py-2"
								><span class="sr-only">Remove</span></th
							>
						{/if}
					</tr>
				</thead>
				<tbody class="divide-y divide-slate-100">
					{#each workOrder.labourEntries as entry (entry.id)}
						<tr>
							<td class="px-4 py-2 text-slate-900">
								{displayNameOf(users, entry.mechanicUserId)}
							</td>
							<td class="px-4 py-2 text-slate-900">
								{formatDuration(entry.minutes)}
							</td>
							<td class="px-4 py-2 text-slate-700">
								{entry.note}
							</td>
							<td class="px-4 py-2 text-slate-500">
								{formatDateTime(entry.loggedAtUtc)}
							</td>
							{#if editable}
								<td class="px-4 py-2 text-right">
									<button
										type="button"
										onclick={() => (entryToRemove = entry)}
										class="text-red-700 hover:underline"
									>
										Remove
										<span class="sr-only">{describeEntry(entry)}</span>
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
		<form onsubmit={handleLogLabour} class="mt-3 flex flex-wrap items-end gap-3 text-sm">
			<label class="block font-medium text-slate-700">
				Mechanic
				<!-- Option values can be numbers or null, not just text; bind:value gives back the same value. -->
				<select
					bind:value={mechanicUserId}
					class="mt-1 block rounded-md border-slate-300 text-sm"
				>
					<option value={null}>{currentUser.displayName} (you)</option>
					{#each otherMechanics as mechanic (mechanic.id)}
						<option value={mechanic.id}>{mechanic.displayName}</option>
					{/each}
				</select>
			</label>
			<label class="block font-medium text-slate-700">
				Hours
				<input
					type="number"
					bind:value={hours}
					required
					min="0.25"
					max="24"
					step="0.25"
					class="mt-1 block w-24 rounded-md border-slate-300 text-sm"
				/>
			</label>
			<label class="block min-w-48 flex-1 font-medium text-slate-700">
				Note (optional)
				<input
					bind:value={note}
					maxlength="500"
					class="mt-1 block w-full rounded-md border-slate-300 text-sm"
				/>
			</label>
			<button
				type="submit"
				disabled={saving}
				class="rounded-md bg-slate-900 px-3 py-2 font-medium text-white hover:bg-slate-800 disabled:opacity-60"
			>
				Log labour
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

{#if entryToRemove}
	<!-- {@const} names the entry for this block, so the closure below sees it as never null. -->
	{@const entry = entryToRemove}
	<ActionDialog
		title="Remove this labour?"
		confirmLabel="Remove"
		danger
		onconfirm={() => changeWorkOrder(() => removeLabour(fetch, workOrder.id, entry.id))}
		onclose={() => (entryToRemove = null)}
	>
		<p>{describeEntry(entry)}, logged {formatDateTime(entry.loggedAtUtc)}.</p>
	</ActionDialog>
{/if}
