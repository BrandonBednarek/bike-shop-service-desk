<!--
	The notes on a job, oldest first, such as what the customer said on the phone or why it's on
	hold. Notes are never edited or removed, so they read as the job's history.
-->
<script lang="ts">
	import type { JobNote, User } from '#lib/api/types.js';
	import { formatDateTime } from '#lib/dates.js';
	import { displayNameOf } from '#lib/user-names.js';

	let { notes, users }: { notes: JobNote[]; users: User[] } = $props();
</script>

<section class="mt-8">
	<h2 class="text-lg font-semibold text-slate-900">Notes</h2>
	{#if notes.length === 0}
		<p class="mt-2 text-sm text-slate-600">No notes yet.</p>
	{:else}
		<ul class="mt-2 space-y-2">
			{#each notes as note (note.id)}
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
</section>
