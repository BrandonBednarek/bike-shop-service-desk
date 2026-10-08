<!--
	A job's page, at /work-orders/1001. The folder's +layout.ts loads the job and the staff list
	before the page is drawn. It shows the job's details, an Edit details link, buttons to
	change its status (StatusActions), the estimate against the bill, and the labour, parts and
	notes recorded on it. An unknown job number goes to the error page instead.
-->
<script lang="ts">
	import Badge from '#lib/components/Badge.svelte';
	import PosSaleReminder from '#lib/components/PosSaleReminder.svelte';
	import { formatDateTime, formatDuration, formatShortDate, todayAsIsoDate } from '#lib/dates.js';
	import { formatCents } from '#lib/money.js';
	import { displayNameOf } from '#lib/user-names.js';
	import { holdReasonLabels, jobTypeLabels, statusLabels } from '#lib/work-orders/labels.js';
	import { isOverdue } from '#lib/work-orders/overdue.js';
	import { canChange, isClosed } from '#lib/work-orders/status.js';
	import LabourSection from './LabourSection.svelte';
	import NotesSection from './NotesSection.svelte';
	import PartsSection from './PartsSection.svelte';
	import StatusActions from './StatusActions.svelte';
	import type { PageProps } from './$types';

	let { data }: PageProps = $props();

	// $derived, not const: SvelteKit reuses this page when only the job number in the address
	// changes, so data can change under it.
	let workOrder = $derived(data.workOrder);
	const today = todayAsIsoDate();

	// The API sends the estimate and bill totals; their labour and parts amounts are worked out
	// from them, so they always add up.
	let estimatedLabourCents = $derived(
		workOrder.estimateTotalCents - workOrder.estimatedPartsCents
	);
	let partsUsedCents = $derived(workOrder.billCents - estimatedLabourCents);

	let statusDescription = $derived(
		workOrder.holdReason
			? `${statusLabels[workOrder.status]}: ${holdReasonLabels[workOrder.holdReason]}`
			: statusLabels[workOrder.status]
	);
</script>

<svelte:head>
	<title>Job #{workOrder.id} · Bike Shop Service Desk</title>
</svelte:head>

<a href="/" class="text-sm text-slate-600 hover:text-slate-900">← Job board</a>

<div class="mt-2 flex items-center gap-3">
	<h1 class="text-2xl font-semibold text-slate-900">Job #{workOrder.id}</h1>
	<span class="rounded-full bg-slate-200 px-3 py-1 text-sm font-medium text-slate-800">
		{statusLabels[workOrder.status]}
	</span>
	{#if canChange(workOrder, data.currentUser)}
		<a
			href="/work-orders/{workOrder.id}/edit"
			class="ml-auto rounded-md border border-slate-300 bg-white px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-100"
		>
			Edit details
		</a>
	{/if}
</div>
<p class="mt-1 text-slate-600">{workOrder.customerName} · {workOrder.customerPhone}</p>

<StatusActions {workOrder} currentUser={data.currentUser} />

{#if canChange(workOrder, data.currentUser)}
	<PosSaleReminder {workOrder} />
{/if}

<dl
	class="mt-6 grid grid-cols-3 gap-x-8 gap-y-4 rounded-lg border border-slate-200 bg-white p-6 text-sm"
>
	<div>
		<dt class="text-slate-500">Bike</dt>
		<dd class="mt-1 text-slate-900">{workOrder.bikeMakeModel} · {workOrder.bikeColour}</dd>
	</div>
	<div>
		<dt class="text-slate-500">Main job type</dt>
		<dd class="mt-1 text-slate-900">{jobTypeLabels[workOrder.jobType]}</dd>
	</div>
	<div>
		<dt class="text-slate-500">Promised for</dt>
		<dd class="mt-1 flex items-center gap-2 text-slate-900">
			{formatShortDate(workOrder.promisedOn)}
			{#if isOverdue(workOrder, today)}
				<Badge text="Overdue" tone="alert" />
			{/if}
		</dd>
	</div>
	<div>
		<dt class="text-slate-500">Status</dt>
		<dd class="mt-1 text-slate-900">
			{statusDescription}, since {formatDateTime(workOrder.statusChangedAtUtc)}
		</dd>
	</div>
	<div>
		<dt class="text-slate-500">Assigned to</dt>
		<dd class="mt-1 text-slate-900">
			{#if workOrder.assignedToUserId !== null}
				{displayNameOf(data.users, workOrder.assignedToUserId)}
			{:else if isClosed(workOrder)}
				Nobody
			{:else}
				<Badge text="Unassigned" tone="warning" />
			{/if}
		</dd>
	</div>
	<div>
		<dt class="text-slate-500">Checked in</dt>
		<dd class="mt-1 text-slate-900">
			{formatDateTime(workOrder.checkedInAtUtc)} by {displayNameOf(
				data.users,
				workOrder.checkedInByUserId
			)}
		</dd>
	</div>
	<!-- These only exist for a collected or a cancelled job. -->
	{#if workOrder.posReceiptNumber}
		<div>
			<dt class="text-slate-500">POS receipt</dt>
			<dd class="mt-1 text-slate-900">{workOrder.posReceiptNumber}</dd>
		</div>
	{/if}
	{#if workOrder.cancellationReason}
		<div class="col-span-2">
			<dt class="text-slate-500">Why it was cancelled</dt>
			<dd class="mt-1 text-slate-900">{workOrder.cancellationReason}</dd>
		</div>
	{/if}
</dl>

<section class="mt-8">
	<h2 class="text-lg font-semibold text-slate-900">Work requested</h2>
	<!-- whitespace-pre-line keeps the line breaks staff typed. -->
	<p
		class="mt-2 rounded-lg border border-slate-200 bg-white p-4 text-sm whitespace-pre-line text-slate-900"
	>
		{workOrder.workRequested}
	</p>
</section>

<section class="mt-8 max-w-xl">
	<h2 class="text-lg font-semibold text-slate-900">Estimate and bill</h2>
	<dl
		class="mt-2 divide-y divide-slate-100 rounded-lg border border-slate-200 bg-white px-4 text-sm"
	>
		<div class="flex justify-between py-2">
			<dt class="text-slate-600">
				Labour: {formatDuration(workOrder.estimatedLabourMinutes)} at
				{formatCents(workOrder.labourRateCentsPerHour)} an hour
			</dt>
			<dd class="text-slate-900">{formatCents(estimatedLabourCents)}</dd>
		</div>
		<div class="flex justify-between py-2">
			<dt class="text-slate-600">Parts, as estimated</dt>
			<dd class="text-slate-900">{formatCents(workOrder.estimatedPartsCents)}</dd>
		</div>
		<div class="flex justify-between py-2 font-semibold">
			<dt class="text-slate-900">Estimate</dt>
			<dd class="text-slate-900">{formatCents(workOrder.estimateTotalCents)}</dd>
		</div>
		<div class="flex justify-between py-2">
			<dt class="text-slate-600">Parts used</dt>
			<dd class="text-slate-900">{formatCents(partsUsedCents)}</dd>
		</div>
		<div class="flex justify-between py-2 font-semibold">
			<dt class="text-slate-900">Bill: estimated labour plus parts used</dt>
			<dd class="text-slate-900">{formatCents(workOrder.billCents)}</dd>
		</div>
	</dl>
	{#if workOrder.isOverEstimate}
		<p class="mt-2 rounded-md bg-amber-50 px-3 py-2 text-sm text-amber-900">
			The bill is {formatCents(workOrder.billCents - workOrder.estimateTotalCents)} over the estimate.
		</p>
	{/if}
	<p class="mt-2 text-sm text-slate-600">
		Labour logged: {formatDuration(workOrder.loggedLabourMinutes)} of
		{formatDuration(workOrder.estimatedLabourMinutes)} estimated. The customer pays the estimated
		labour.
	</p>
</section>

<LabourSection {workOrder} users={data.users} currentUser={data.currentUser} />
<PartsSection {workOrder} currentUser={data.currentUser} />
<NotesSection {workOrder} users={data.users} currentUser={data.currentUser} />
