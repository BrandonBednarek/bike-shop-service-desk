<!--
	The parts used on a job. Their total is the "Parts used" line of the bill.
-->
<script lang="ts">
	import type { PartLine } from '#lib/api/types.js';
	import { formatDateTime } from '#lib/dates.js';
	import { formatCents } from '#lib/money.js';

	let { partLines }: { partLines: PartLine[] } = $props();
</script>

<section class="mt-8">
	<h2 class="text-lg font-semibold text-slate-900">Parts</h2>
	{#if partLines.length === 0}
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
					</tr>
				</thead>
				<tbody class="divide-y divide-slate-100">
					{#each partLines as part (part.id)}
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
						</tr>
					{/each}
				</tbody>
			</table>
		</div>
	{/if}
</section>
