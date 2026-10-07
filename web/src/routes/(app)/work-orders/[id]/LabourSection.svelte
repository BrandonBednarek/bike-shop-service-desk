<!--
	The labour logged on a job: who did it, how long it took and any note. The customer pays the
	estimated labour, so this is the shop's record of the real time, not part of the bill.
-->
<script lang="ts">
	import type { LabourEntry, User } from '#lib/api/types.js';
	import { formatDateTime, formatDuration } from '#lib/dates.js';
	import { displayNameOf } from '#lib/user-names.js';

	let { entries, users }: { entries: LabourEntry[]; users: User[] } = $props();
</script>

<section class="mt-8">
	<h2 class="text-lg font-semibold text-slate-900">Labour</h2>
	{#if entries.length === 0}
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
					</tr>
				</thead>
				<tbody class="divide-y divide-slate-100">
					{#each entries as entry (entry.id)}
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
						</tr>
					{/each}
				</tbody>
			</table>
		</div>
	{/if}
</section>
