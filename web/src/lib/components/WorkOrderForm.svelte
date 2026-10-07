<!--
	The job's details: the bike, the work wanted, the estimate, the promised date and who it's
	assigned to. Check-in uses it now, and the job page will reuse it to edit a job. Staff type
	hours and dollars, which become the minutes and cents the API stores as they type. Saving
	hands the details to the page through onsave.
-->
<script lang="ts">
	import { describeFailure } from '#lib/api/api-error.js';
	import type { JobType, User, WorkOrderDetails } from '#lib/api/types.js';
	import { todayAsIsoDate } from '#lib/dates.js';
	import { formatCents, toCents } from '#lib/money.js';
	import { jobTypeLabels } from '#lib/work-orders/labels.js';

	interface Props {
		// null until a customer is chosen; the save button stays disabled until then.
		customerId: number | null;
		users: User[];
		submitLabel: string;
		onsave: (details: WorkOrderDetails) => Promise<void>;
	}

	let { customerId, users, submitLabel, onsave }: Props = $props();

	const defaultLabourRateDollars = 95;
	const jobTypes = Object.keys(jobTypeLabels) as JobType[];
	const today = todayAsIsoDate();

	// One $state per input, kept in step with it through bind:value. Number inputs give null
	// while they're empty.
	let bikeMakeModel = $state('');
	let bikeColour = $state('');
	let jobType = $state<JobType | ''>('');
	let workRequested = $state('');
	let labourHours = $state<number | null>(null);
	let labourRateDollars = $state<number | null>(defaultLabourRateDollars);
	let partsDollars = $state<number | null>(null);
	let promisedOn = $state('');
	let assignedToUserId = $state<number | null>(null);

	let errorMessage = $state('');
	let saving = $state(false);

	let activeUsers = $derived(users.filter((user) => user.isActive));

	let estimatedLabourMinutes = $derived(Math.round((labourHours ?? 0) * 60));
	let labourRateCentsPerHour = $derived(toCents(labourRateDollars ?? 0));
	let estimatedPartsCents = $derived(toCents(partsDollars ?? 0));

	// The estimate updates as staff type, so they can tell the customer the price. It rounds the
	// same way as the API, so it matches the estimate the job is saved with.
	let estimatedLabourCents = $derived(
		Math.round((estimatedLabourMinutes * labourRateCentsPerHour) / 60)
	);

	async function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		// The disabled button and the required job type rule these out already; this just tells
		// TypeScript the values are set.
		if (customerId === null || jobType === '') {
			return;
		}
		saving = true;
		errorMessage = '';
		try {
			await onsave({
				customerId,
				bikeMakeModel: bikeMakeModel.trim(),
				bikeColour: bikeColour.trim(),
				jobType,
				workRequested: workRequested.trim(),
				estimatedLabourMinutes,
				labourRateCentsPerHour,
				estimatedPartsCents,
				promisedOn,
				assignedToUserId
			});
		} catch (error) {
			errorMessage = describeFailure(error);
		} finally {
			saving = false;
		}
	}
</script>

<form onsubmit={handleSubmit} class="grid max-w-3xl grid-cols-2 gap-4">
	<label class="block text-sm font-medium text-slate-700">
		Bike make and model
		<input
			bind:value={bikeMakeModel}
			required
			maxlength="100"
			placeholder="Trek FX 2"
			class="mt-1 block w-full rounded-md border-slate-300 text-sm"
		/>
	</label>

	<label class="block text-sm font-medium text-slate-700">
		Colour
		<input
			bind:value={bikeColour}
			required
			maxlength="50"
			class="mt-1 block w-full rounded-md border-slate-300 text-sm"
		/>
	</label>

	<label class="block text-sm font-medium text-slate-700">
		Main job type
		<select
			bind:value={jobType}
			required
			class="mt-1 block w-full rounded-md border-slate-300 text-sm"
		>
			<option value="" disabled>Choose a job type</option>
			{#each jobTypes as type (type)}
				<option value={type}>{jobTypeLabels[type]}</option>
			{/each}
		</select>
	</label>

	<label class="block text-sm font-medium text-slate-700">
		Assign to
		<!-- Option values can be numbers or null, not just text; bind:value gives back the same value. -->
		<select
			bind:value={assignedToUserId}
			class="mt-1 block w-full rounded-md border-slate-300 text-sm"
		>
			<option value={null}>Unassigned</option>
			{#each activeUsers as user (user.id)}
				<option value={user.id}>{user.displayName}</option>
			{/each}
		</select>
	</label>

	<label class="col-span-2 block text-sm font-medium text-slate-700">
		Work requested, in the customer's words
		<textarea
			bind:value={workRequested}
			required
			maxlength="2000"
			rows="3"
			class="mt-1 block w-full rounded-md border-slate-300 text-sm"></textarea>
	</label>

	<div class="col-span-2 grid grid-cols-4 gap-4">
		<label class="block text-sm font-medium text-slate-700">
			Labour hours
			<input
				type="number"
				bind:value={labourHours}
				required
				min="0"
				max="100"
				step="0.25"
				class="mt-1 block w-full rounded-md border-slate-300 text-sm"
			/>
		</label>

		<label class="block text-sm font-medium text-slate-700">
			Rate per hour ($)
			<input
				type="number"
				bind:value={labourRateDollars}
				required
				min="0"
				max="1000"
				step="0.01"
				class="mt-1 block w-full rounded-md border-slate-300 text-sm"
			/>
		</label>

		<label class="block text-sm font-medium text-slate-700">
			Parts ($)
			<input
				type="number"
				bind:value={partsDollars}
				required
				min="0"
				max="100000"
				step="0.01"
				class="mt-1 block w-full rounded-md border-slate-300 text-sm"
			/>
		</label>

		<label class="block text-sm font-medium text-slate-700">
			Promised for
			<input
				type="date"
				bind:value={promisedOn}
				required
				min={today}
				max="9999-12-31"
				class="mt-1 block w-full rounded-md border-slate-300 text-sm"
			/>
		</label>
	</div>

	<p class="col-span-2 text-sm text-slate-700">
		Estimate:
		<span class="font-semibold text-slate-900">
			{formatCents(estimatedLabourCents + estimatedPartsCents)}
		</span>
		({formatCents(estimatedLabourCents)} labour + {formatCents(estimatedPartsCents)} parts)
	</p>

	{#if errorMessage}
		<p role="alert" class="col-span-2 rounded-md bg-red-50 px-3 py-2 text-sm text-red-800">
			{errorMessage}
		</p>
	{/if}

	<div class="col-span-2 flex items-center gap-4">
		<button
			type="submit"
			disabled={saving || customerId === null}
			class="rounded-md bg-slate-900 px-4 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-60"
		>
			{saving ? 'Saving…' : submitLabel}
		</button>
		{#if customerId === null}
			<span class="text-sm text-slate-600">Choose the customer first.</span>
		{/if}
	</div>
</form>
