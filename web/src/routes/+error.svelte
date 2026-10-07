<!--
	Shown in place of any page that can't load, for example because the API is down or the
	address doesn't exist. It sits at the top of the routes folder so it also covers the
	sign-in check in (app)/+layout.ts, which means it has no header. For a failed API call the
	message comes from handleError in hooks.client.ts; for a missing page it's SvelteKit's own.
-->
<script lang="ts">
	import { page } from '$app/state';
</script>

<main class="flex min-h-screen items-center justify-center bg-slate-50 px-4">
	<div role="alert" class="w-full max-w-md rounded-lg border border-red-200 bg-red-50 p-6">
		<h1 class="text-lg font-semibold text-red-900">This page couldn't be loaded</h1>
		<p class="mt-2 text-sm text-red-800">{page.error?.message}</p>
		<div class="mt-4 flex items-center gap-4 text-sm font-medium">
			<!-- Only a server or connection failure might go away on a retry. Reloading runs the
				page's load again, and the sign-in check with it. -->
			{#if page.status >= 500}
				<button
					type="button"
					onclick={() => location.reload()}
					class="rounded-md border border-red-300 bg-white px-3 py-1.5 text-red-800 hover:bg-red-100"
				>
					Try again
				</button>
			{/if}
			<a href="/" class="text-red-800 underline hover:text-red-900">Go to the job board</a>
		</div>
	</div>
</main>
