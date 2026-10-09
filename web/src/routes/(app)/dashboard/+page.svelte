<!--
	The dashboard, at /dashboard: what's late, and what's finished but not paid for. Everyone
	can see it. +page.ts loads the open jobs, and both tables are worked out here from that list:
	jobs past their promised date that aren't ready yet, grouped by status and most overdue first
	(the order the API sends them), and bikes that are ready but haven't been collected, longest
	waiting first. As on the job board, clicking a row opens the job, and keyboard and screen
	reader users get the job number, which is a real link.
-->
<script lang="ts">
	import type { WorkOrder, WorkOrderStatus } from '#lib/api/types.js';
	import Badge from '#lib/components/Badge.svelte';
	import {
		daysBetween,
		formatDays,
		formatShortDate,
		isoDateOfTimestamp,
		todayAsIsoDate
	} from '#lib/dates.js';
	import { formatCents } from '#lib/money.js';
	import { displayNameOf } from '#lib/user-names.js';
	import { holdReasonLabels, statusLabels } from '#lib/work-orders/labels.js';
	import { dashboardPage, rememberListPage } from '#lib/work-orders/list-pages.js';
	import { openJobFromRow } from '#lib/work-orders/open-job.js';
	import { isOverdue, notReadyStatuses } from '#lib/work-orders/overdue.js';
	import type { PageProps } from './$types';

	let { data }: PageProps = $props();

	rememberListPage(dashboardPage);

	const today = todayAsIsoDate();

	let overdueWorkOrders = $derived(
		data.workOrders.filter((workOrder) => isOverdue(workOrder, today))
	);
	let readyWorkOrders = $derived(
		data.workOrders
			.filter((workOrder) => workOrder.status === 'ReadyForPickup')
			.sort((first, second) => daysWaiting(second) - daysWaiting(first))
	);
	let readyBillsCents = $derived(
		readyWorkOrders.reduce((total, workOrder) => total + workOrder.billCents, 0)
	);

	// Each status points to a different problem: jobs not started mean too much work booked in,
	// and jobs on hold mean waiting on parts or on the customer.
	function overdueWithStatus(status: WorkOrderStatus): WorkOrder[] {
		return overdueWorkOrders.filter((workOrder) => workOrder.status === status);
	}

	function daysOverdue(workOrder: WorkOrder): number {
		return daysBetween(workOrder.promisedOn, today);
	}

	// Calendar days since the job's status last changed: when it was marked ready, or when the
	// owner reopened it after it was marked collected by mistake.
	function daysWaiting(workOrder: WorkOrder): number {
		return daysBetween(isoDateOfTimestamp(workOrder.statusChangedAtUtc), today);
	}
</script>

<!-- The first three cells of a row in both tables, the same as on the job board. -->
{#snippet jobCells(workOrder: WorkOrder)}
	<td class="px-4 py-3">
		<a
			href="/work-orders/{workOrder.id}"
			class="font-medium text-slate-900 underline-offset-2 hover:underline"
		>
			#{workOrder.id}
		</a>
		{#if workOrder.holdReason}
			<div class="text-slate-500">{holdReasonLabels[workOrder.holdReason]}</div>
		{/if}
	</td>
	<td class="px-4 py-3">
		<div class="text-slate-900">{workOrder.customerName}</div>
		<div class="text-slate-500">{workOrder.customerPhone}</div>
	</td>
	<td class="px-4 py-3">
		<div class="text-slate-900">{workOrder.bikeMakeModel}</div>
		<div class="text-slate-500">{workOrder.bikeColour}</div>
	</td>
{/snippet}

<svelte:head>
	<title>Dashboard · Bike Shop Service Desk</title>
</svelte:head>

<h1 class="text-2xl font-semibold text-slate-900">Dashboard</h1>
<p class="mt-1 text-slate-600">
	{data.workOrders.length}
	{data.workOrders.length === 1 ? 'bike' : 'bikes'} in the shop.
</p>

<!-- aria-labelledby names each table after its heading, for screen readers that jump between tables. -->
<section class="mt-8">
	<h2 id="overdue-heading" class="text-lg font-semibold text-slate-900">
		Overdue ({overdueWorkOrders.length})
	</h2>
	<p class="text-sm text-slate-600">Past the promised date and not ready yet.</p>
	{#if overdueWorkOrders.length === 0}
		<p class="mt-3 text-sm text-slate-600">Nothing is overdue.</p>
	{:else}
		<div class="mt-3 overflow-x-auto rounded-lg border border-slate-200 bg-white">
			<table aria-labelledby="overdue-heading" class="min-w-full text-sm">
				<thead class="bg-slate-50 text-left text-xs font-medium text-slate-500 uppercase">
					<tr>
						<th scope="col" class="px-4 py-3">Job</th>
						<th scope="col" class="px-4 py-3">Customer</th>
						<th scope="col" class="px-4 py-3">Bike</th>
						<th scope="col" class="px-4 py-3">Promised</th>
						<th scope="col" class="px-4 py-3">Overdue by</th>
						<th scope="col" class="px-4 py-3">Assigned to</th>
					</tr>
				</thead>
				<!-- One <tbody> per status, each with a heading row that labels its group. -->
				{#each notReadyStatuses as status (status)}
					{@const groupWorkOrders = overdueWithStatus(status)}
					{#if groupWorkOrders.length > 0}
						<tbody class="divide-y divide-slate-100 border-t border-slate-200">
							<tr>
								<th
									scope="rowgroup"
									colspan="6"
									class="bg-slate-50 px-4 py-2 text-left font-medium text-slate-700"
								>
									{statusLabels[status]} ({groupWorkOrders.length})
								</th>
							</tr>
							{#each groupWorkOrders as workOrder (workOrder.id)}
								<tr
									onclick={(event) => openJobFromRow(event, workOrder.id)}
									class="cursor-pointer align-top hover:bg-slate-50"
								>
									{@render jobCells(workOrder)}
									<td class="px-4 py-3 whitespace-nowrap text-slate-700">
										{formatShortDate(workOrder.promisedOn)}
									</td>
									<td
										class="px-4 py-3 font-medium whitespace-nowrap text-red-800"
									>
										{formatDays(daysOverdue(workOrder))}
									</td>
									<td class="px-4 py-3 text-slate-700">
										{#if workOrder.assignedToUserId === null}
											<Badge text="Unassigned" tone="warning" />
										{:else}
											{displayNameOf(data.users, workOrder.assignedToUserId)}
										{/if}
									</td>
								</tr>
							{/each}
						</tbody>
					{/if}
				{/each}
			</table>
		</div>
	{/if}
</section>

<section class="mt-10">
	<h2 id="ready-heading" class="text-lg font-semibold text-slate-900">
		Ready but not collected ({readyWorkOrders.length})
	</h2>
	<p class="text-sm text-slate-600">Finished and waiting for the customer to pick up and pay.</p>
	{#if readyWorkOrders.length === 0}
		<p class="mt-3 text-sm text-slate-600">No bikes are waiting to be collected.</p>
	{:else}
		<div class="mt-3 overflow-x-auto rounded-lg border border-slate-200 bg-white">
			<table
				aria-labelledby="ready-heading"
				class="min-w-full divide-y divide-slate-200 text-sm"
			>
				<thead class="bg-slate-50 text-left text-xs font-medium text-slate-500 uppercase">
					<tr>
						<th scope="col" class="px-4 py-3">Job</th>
						<th scope="col" class="px-4 py-3">Customer</th>
						<th scope="col" class="px-4 py-3">Bike</th>
						<th scope="col" class="px-4 py-3">Waiting</th>
						<th scope="col" class="px-4 py-3 text-right">Bill</th>
					</tr>
				</thead>
				<tbody class="divide-y divide-slate-100">
					{#each readyWorkOrders as workOrder (workOrder.id)}
						<tr
							onclick={(event) => openJobFromRow(event, workOrder.id)}
							class="cursor-pointer align-top hover:bg-slate-50"
						>
							{@render jobCells(workOrder)}
							<td class="px-4 py-3 whitespace-nowrap text-slate-700">
								{formatDays(daysWaiting(workOrder))}
							</td>
							<td class="px-4 py-3 text-right text-slate-900">
								{formatCents(workOrder.billCents)}
							</td>
						</tr>
					{/each}
				</tbody>
				<!-- A <tfoot> row keeps the total in the table, under the column it adds up. -->
				<tfoot class="bg-slate-50">
					<tr>
						<th
							scope="row"
							colspan="4"
							class="px-4 py-3 text-right font-medium text-slate-700"
						>
							Waiting to be paid for
						</th>
						<td class="px-4 py-3 text-right font-semibold text-slate-900">
							{formatCents(readyBillsCents)}
						</td>
					</tr>
				</tfoot>
			</table>
		</div>
	{/if}
</section>
