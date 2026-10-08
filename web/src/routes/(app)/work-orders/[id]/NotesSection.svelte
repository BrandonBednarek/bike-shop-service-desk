<!--
	The notes on a job, oldest first, such as what the customer said on the phone or why it's on
	hold. Notes are never edited or removed, so they read as the job's history. Anyone who can
	change the job can add one below the list; changeWorkOrder then reloads the job.
-->
<script lang="ts">
	import { describeFailure } from '#lib/api/api-error.js';
	import type { CurrentUser, User, WorkOrder } from '#lib/api/types.js';
	import { addNote } from '#lib/api/work-orders.js';
	import { formatDateTime } from '#lib/dates.js';
	import { displayNameOf } from '#lib/user-names.js';
	import { changeWorkOrder } from '#lib/work-orders/change.js';
	import { canChange } from '#lib/work-orders/status.js';

	interface Props {
		workOrder: WorkOrder;
		users: User[];
		currentUser: CurrentUser;
	}

	let { workOrder, users, currentUser }: Props = $props();

	let text = $state('');
	let errorMessage = $state('');
	let saving = $state(false);

	async function handleAddNote(event: SubmitEvent) {
		event.preventDefault();
		const noteText = text.trim();
		saving = true;
		errorMessage = '';
		try {
			await changeWorkOrder(() => addNote(fetch, workOrder.id, noteText));
			text = '';
		} catch (error) {
			errorMessage = describeFailure(error);
		} finally {
			saving = false;
		}
	}
</script>

<section class="mt-8">
	<h2 class="text-lg font-semibold text-slate-900">Notes</h2>
	{#if workOrder.notes.length === 0}
		<p class="mt-2 text-sm text-slate-600">No notes yet.</p>
	{:else}
		<ul class="mt-2 space-y-2">
			{#each workOrder.notes as note (note.id)}
				<li class="rounded-lg border border-slate-200 bg-white p-4 text-sm">
					<p class="whitespace-pre-line text-slate-900">{note.text}</p>
					<p class="mt-1 text-slate-500">
						{displayNameOf(users, note.writtenByUserId)} ·
						{formatDateTime(note.writtenAtUtc)}
					</p>
				</li>
			{/each}
		</ul>
	{/if}

	{#if canChange(workOrder, currentUser)}
		<form onsubmit={handleAddNote} class="mt-3 max-w-xl text-sm">
			<label class="block font-medium text-slate-700">
				Add a note
				<textarea
					bind:value={text}
					required
					maxlength="1000"
					rows="2"
					class="mt-1 block w-full rounded-md border-slate-300 text-sm"></textarea>
			</label>
			<button
				type="submit"
				disabled={saving}
				class="mt-2 rounded-md bg-slate-900 px-3 py-2 font-medium text-white hover:bg-slate-800 disabled:opacity-60"
			>
				Add note
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
