<!--
	Check in a bike, at /check-in. Step one finds or adds the customer (CustomerPicker), and
	step two records the job (WorkOrderForm). Once the API has saved the job, the page swaps
	to a confirmation with the job number, which staff write on the bike's tag.
-->
<script lang="ts">
	import { invalidate } from '$app/navigation';
	import type { Customer, WorkOrder, WorkOrderDetails } from '#lib/api/types.js';
	import { checkIn } from '#lib/api/work-orders.js';
	import WorkOrderForm from '#lib/components/WorkOrderForm.svelte';
	import { formatShortDate } from '#lib/dates.js';
	import { formatCents } from '#lib/money.js';
	import CustomerPicker from './CustomerPicker.svelte';
	import type { PageProps } from './$types';

	let { data }: PageProps = $props();

	let customer = $state<Customer | null>(null);
	let checkedInWorkOrder = $state<WorkOrder | null>(null);

	// Called by the form. If the API refuses, the error goes back to the form, which shows it.
	async function handleSave(details: WorkOrderDetails) {
		checkedInWorkOrder = await checkIn(fetch, details);
		// Hovering a link to the board preloads it, so throw away any copy loaded before this job.
		await invalidate('app:work-orders');
	}

	// Clearing both brings back an empty form: the {#if} below draws fresh components.
	function startAnotherCheckIn() {
		customer = null;
		checkedInWorkOrder = null;
	}
</script>

<svelte:head>
	<title>Check in a bike · Bike Shop Service Desk</title>
</svelte:head>

{#if checkedInWorkOrder}
	<div class="max-w-xl rounded-lg border border-green-200 bg-green-50 p-6">
		<div id="check-in-result">
			<h1 class="text-xl font-semibold text-green-900">
				Job #{checkedInWorkOrder.id} is checked in
			</h1>
			<p class="mt-2 text-green-800">
				Write <span class="font-semibold">{checkedInWorkOrder.id}</span> on the bike's tag.
			</p>
			<p class="mt-1 text-green-800">
				Estimate {formatCents(checkedInWorkOrder.estimateTotalCents)}, promised for
				{formatShortDate(checkedInWorkOrder.promisedOn)}.
			</p>
		</div>
		<div class="mt-4 flex items-center gap-4 text-sm font-medium">
			<!-- Focus lands here, so Enter starts the next check-in. aria-describedby makes a screen
				reader read the job number with the button. -->
			<!-- svelte-ignore a11y_autofocus -->
			<button
				type="button"
				onclick={startAnotherCheckIn}
				autofocus
				aria-describedby="check-in-result"
				class="rounded-md bg-slate-900 px-4 py-2 text-white hover:bg-slate-800"
			>
				Check in another bike
			</button>
			<a href="/" class="text-slate-700 underline hover:text-slate-900"
				>Back to the job board</a
			>
		</div>
	</div>
{:else}
	<a href="/" class="text-sm text-slate-600 hover:text-slate-900">← Job board</a>
	<h1 class="mt-2 text-2xl font-semibold text-slate-900">Check in a bike</h1>

	<section class="mt-6">
		<h2 class="mb-3 text-lg font-semibold text-slate-900">Customer</h2>
		{#if customer}
			<div
				class="flex max-w-xl items-center justify-between rounded-md border border-slate-200 bg-white px-4 py-3 text-sm"
			>
				<div>
					<div class="font-medium text-slate-900">{customer.name}</div>
					<div class="text-slate-600">{customer.phone}</div>
				</div>
				<!-- The button that picked the customer has gone, so focus moves here; the next Tab
					reaches the job form. -->
				<!-- svelte-ignore a11y_autofocus -->
				<button
					type="button"
					onclick={() => (customer = null)}
					autofocus
					class="font-medium text-slate-700 underline hover:text-slate-900"
				>
					Change
				</button>
			</div>
		{:else}
			<CustomerPicker onselect={(picked) => (customer = picked)} />
		{/if}
	</section>

	<section class="mt-8">
		<h2 class="mb-3 text-lg font-semibold text-slate-900">Job</h2>
		<!-- The form stays on screen while the customer changes, so nothing typed is lost. -->
		<WorkOrderForm
			customerId={customer?.id ?? null}
			users={data.users}
			submitLabel="Check in"
			onsave={handleSave}
		/>
	</section>
{/if}
