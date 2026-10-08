<!--
	Reminds the owner, when changing a collected job, that the sale in the POS doesn't change
	with it. A cancelled job never reached the till, so it gets no reminder.
-->
<script lang="ts">
	import type { WorkOrder } from '#lib/api/types.js';

	let { workOrder }: { workOrder: WorkOrder } = $props();
</script>

{#if workOrder.status === 'Collected'}
	<p class="mt-4 max-w-3xl rounded-md bg-amber-50 px-3 py-2 text-sm text-amber-900">
		This job was collected (POS receipt {workOrder.posReceiptNumber}). Changes here don't update
		that sale, so adjust the till separately.
	</p>
{/if}
