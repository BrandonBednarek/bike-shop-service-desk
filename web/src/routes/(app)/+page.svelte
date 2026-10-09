<!--
	The job board, at /. It lists every open job, soonest promised first, so the counter and
	the bench can see what's in the shop. +page.ts loads the jobs and the staff list before
	the page is drawn. The status tabs, the search box and "Assigned to me" all filter that
	list in the browser, so they don't call the API again. Clicking a row opens the job.
-->
<script lang="ts">
	import type { WorkOrder } from '#lib/api/types.js';
	import Badge from '#lib/components/Badge.svelte';
	import { formatShortDate, todayAsIsoDate } from '#lib/dates.js';
	import { toPhoneDigits } from '#lib/phone-numbers.js';
	import { displayNameOf } from '#lib/user-names.js';
	import { holdReasonLabels, jobTypeLabels, statusLabels } from '#lib/work-orders/labels.js';
	import { jobBoardPage, rememberListPage } from '#lib/work-orders/list-pages.js';
	import { openJobFromRow } from '#lib/work-orders/open-job.js';
	import { isOverdue } from '#lib/work-orders/overdue.js';
	import { snapshot } from '$app/navigation';
	import type { PageProps } from './$types';

	// data is what +page.ts returned, plus currentUser from the (app) layout's guard.
	let { data }: PageProps = $props();

	rememberListPage(jobBoardPage);

	type StatusTab = 'AllOpen' | 'CheckedIn' | 'InProgress' | 'OnHold' | 'ReadyForPickup';
	const statusTabs: StatusTab[] = [
		'AllOpen',
		'CheckedIn',
		'InProgress',
		'OnHold',
		'ReadyForPickup'
	];

	// The filters, each kept in step with its control further down.
	let selectedTab = $state<StatusTab>('AllOpen');
	let search = $state('');
	let assignedToMeOnly = $state(false);

	// Puts the filters back when the browser's Back button returns here from a job.
	snapshot({
		capture: () => ({ selectedTab, search, assignedToMeOnly }),
		restore: (filters) => {
			selectedTab = filters.selectedTab;
			search = filters.search;
			assignedToMeOnly = filters.assignedToMeOnly;
		}
	});

	const today = todayAsIsoDate();

	// $derived values are worked out again whenever anything they read changes. The tab counts
	// come from this list, so each count matches what its tab would show.
	let matchingWorkOrders = $derived(
		data.workOrders.filter(
			(workOrder) => matchesSearch(workOrder) && (!assignedToMeOnly || isMine(workOrder))
		)
	);
	let visibleWorkOrders = $derived(
		matchingWorkOrders.filter((workOrder) => isOnTab(workOrder, selectedTab))
	);

	function matchesSearch(workOrder: WorkOrder): boolean {
		const text = search.trim().toLowerCase();
		return (
			text === '' ||
			workOrder.customerName.toLowerCase().includes(text) ||
			matchesNumber(workOrder, text)
		);
	}

	// Numbers are matched on their digits, so "#1001" finds the job and "(416) 123" or
	// "+1 416 123 4567" finds the customer's phone.
	function matchesNumber(workOrder: WorkOrder, text: string): boolean {
		const digits = toPhoneDigits(text);
		return (
			digits !== '' &&
			(String(workOrder.id).includes(digits) ||
				toPhoneDigits(workOrder.customerPhone).includes(digits))
		);
	}

	function isMine(workOrder: WorkOrder): boolean {
		return workOrder.assignedToUserId === data.currentUser.id;
	}

	function isOnTab(workOrder: WorkOrder, tab: StatusTab): boolean {
		return tab === 'AllOpen' || workOrder.status === tab;
	}

	function countOnTab(tab: StatusTab): number {
		return matchingWorkOrders.filter((workOrder) => isOnTab(workOrder, tab)).length;
	}

	function tabLabel(tab: StatusTab): string {
		return tab === 'AllOpen' ? 'All open' : statusLabels[tab];
	}
</script>

<svelte:head>
	<title>Job board · Bike Shop Service Desk</title>
</svelte:head>

<div class="flex items-center justify-between">
	<h1 class="text-2xl font-semibold text-slate-900">Job board</h1>
	<a
		href="/check-in"
		class="rounded-md bg-slate-900 px-4 py-2 text-sm font-medium text-white hover:bg-slate-800"
	>
		Check in a bike
	</a>
</div>

<div class="mt-6 flex flex-wrap items-center gap-4">
	<div class="flex gap-1 rounded-lg bg-slate-200/70 p-1" role="group" aria-label="Status">
		{#each statusTabs as tab (tab)}
			<!-- aria-pressed tells screen readers which tab is selected. -->
			<button
				type="button"
				aria-pressed={selectedTab === tab}
				onclick={() => (selectedTab = tab)}
				class={[
					'rounded-md px-3 py-1.5 text-sm font-medium',
					selectedTab === tab
						? 'bg-white text-slate-900 shadow-sm'
						: 'text-slate-600 hover:text-slate-900'
				]}
			>
				{tabLabel(tab)}
				<span class="ml-1 text-slate-600">{countOnTab(tab)}</span>
			</button>
		{/each}
	</div>

	<input
		type="search"
		bind:value={search}
		placeholder="Job #, name or phone"
		aria-label="Search jobs"
		class="w-64 rounded-md border-slate-300 text-sm"
	/>

	<label class="flex items-center gap-2 text-sm text-slate-700">
		<input type="checkbox" bind:checked={assignedToMeOnly} class="rounded border-slate-300" />
		Assigned to me
	</label>
</div>

{#if data.workOrders.length === 0}
	<p class="mt-8 text-slate-600">No bikes are in for service. Checked-in jobs will show here.</p>
{:else if visibleWorkOrders.length === 0}
	<p class="mt-8 text-slate-600">No jobs match these filters.</p>
{:else}
	<div class="mt-4 overflow-x-auto rounded-lg border border-slate-200 bg-white">
		<table class="min-w-full divide-y divide-slate-200 text-sm">
			<thead class="bg-slate-50 text-left text-xs font-medium text-slate-500 uppercase">
				<tr>
					<th scope="col" class="px-4 py-3">Job</th>
					<th scope="col" class="px-4 py-3">Customer</th>
					<th scope="col" class="px-4 py-3">Bike</th>
					<th scope="col" class="px-4 py-3">Job type</th>
					<th scope="col" class="px-4 py-3">Status</th>
					<th scope="col" class="px-4 py-3">Promised</th>
					<th scope="col" class="px-4 py-3">Assigned to</th>
				</tr>
			</thead>
			<tbody class="divide-y divide-slate-100">
				<!-- (workOrder.id) is the key Svelte uses to match rows when the list changes. -->
				{#each visibleWorkOrders as workOrder (workOrder.id)}
					<!-- The whole row opens the job for mouse users. Keyboard and screen reader users get the
						job number, which is a real link. -->
					<tr
						onclick={(event) => openJobFromRow(event, workOrder.id)}
						class="cursor-pointer align-top hover:bg-slate-50"
					>
						<td class="px-4 py-3">
							<a
								href="/work-orders/{workOrder.id}"
								class="font-medium text-slate-900 underline-offset-2 hover:underline"
							>
								#{workOrder.id}
							</a>
						</td>
						<td class="px-4 py-3">
							<div class="text-slate-900">{workOrder.customerName}</div>
							<div class="text-slate-500">{workOrder.customerPhone}</div>
						</td>
						<td class="px-4 py-3">
							<div class="text-slate-900">{workOrder.bikeMakeModel}</div>
							<div class="text-slate-500">{workOrder.bikeColour}</div>
						</td>
						<td class="px-4 py-3 text-slate-700">{jobTypeLabels[workOrder.jobType]}</td>
						<td class="px-4 py-3">
							<div class="flex items-center gap-2 text-slate-900">
								{statusLabels[workOrder.status]}
								{#if workOrder.isOverEstimate}
									<Badge text="Over estimate" tone="warning" />
								{/if}
							</div>
							{#if workOrder.holdReason}
								<div class="text-slate-500">
									{holdReasonLabels[workOrder.holdReason]}
								</div>
							{/if}
						</td>
						<td class="px-4 py-3 text-slate-900">
							<div class="flex items-center gap-2 whitespace-nowrap">
								{formatShortDate(workOrder.promisedOn)}
								{#if isOverdue(workOrder, today)}
									<Badge text="Overdue" tone="alert" />
								{/if}
							</div>
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
		</table>
	</div>
{/if}
