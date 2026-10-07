<!--
	A small coloured label, such as "Overdue" or "Unassigned", used on the job board and job page.
-->
<script lang="ts">
	let { text, tone }: { text: string; tone: 'warning' | 'alert' } = $props();

	const toneColours = {
		warning: 'bg-amber-100 text-amber-800',
		alert: 'bg-red-100 text-red-800'
	};
</script>

<span class="inline-flex rounded-full px-2 py-0.5 text-xs font-medium {toneColours[tone]}">
	{text}
</span>
