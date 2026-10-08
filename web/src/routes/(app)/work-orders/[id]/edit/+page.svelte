<!--
	Edit a job, at /work-orders/1001/edit: fix the bike or job details, or revise the estimate
	when the customer agrees to more work. The folder's +layout.ts has already loaded the job,
	and the form is check-in's, filled in from the job. Saving sends the details to the API,
	reloads the job and goes back to its page. A closed job can only be edited by the owner.
-->
<script lang="ts">
	import { goto, invalidate } from '$app/navigation';
	import type { WorkOrderDetails } from '#lib/api/types.js';
	import { updateWorkOrderDetails } from '#lib/api/work-orders.js';
	import PosSaleReminder from '#lib/components/PosSaleReminder.svelte';
	import WorkOrderForm from '#lib/components/WorkOrderForm.svelte';
	import { canChange } from '#lib/work-orders/status.js';
	import type { PageProps } from './$types';

	let { data }: PageProps = $props();

	let workOrder = $derived(data.workOrder);

	// Called by the form. If the API refuses, the error goes back to the form, which shows it.
	async function handleSave(details: WorkOrderDetails) {
		await updateWorkOrderDetails(fetch, workOrder.id, details);
		await invalidate('app:work-orders');
		await goto(`/work-orders/${workOrder.id}`);
	}
</script>

<svelte:head>
	<title>Edit job #{workOrder.id} · Bike Shop Service Desk</title>
</svelte:head>

<a href="/work-orders/{workOrder.id}" class="text-sm text-slate-600 hover:text-slate-900">
	← Job #{workOrder.id}
</a>
<h1 class="mt-2 text-2xl font-semibold text-slate-900">Edit job #{workOrder.id}</h1>
<p class="mt-1 text-slate-600">{workOrder.customerName} · {workOrder.customerPhone}</p>

{#if !canChange(workOrder, data.currentUser)}
	<p class="mt-6 text-slate-600">
		Collected and cancelled jobs can only be changed by the owner.
	</p>
{:else}
	<PosSaleReminder {workOrder} />
	<!-- A WorkOrder has every field WorkOrderDetails needs, so the job itself can be the form's
		starting values. SvelteKit reuses this page when the address moves to another job's edit
		page, so {#key} gives each job a fresh form instead of one still holding the last job. -->
	<div class="mt-6">
		{#key workOrder.id}
			<WorkOrderForm
				customerId={workOrder.customerId}
				users={data.users}
				initial={workOrder}
				submitLabel="Save changes"
				onsave={handleSave}
			/>
		{/key}
	</div>
{/if}
