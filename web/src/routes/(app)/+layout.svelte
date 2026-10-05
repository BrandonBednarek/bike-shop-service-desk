<!--
	Layout for every signed-in page (the (app) folder). SvelteKit runs the guard in
	+layout.ts first, then draws this header with the requested page inside <main>.
-->
<script lang="ts">
	import { goto } from '$app/navigation';
	import { signOut } from '#lib/api/auth.js';
	import type { LayoutProps } from './$types';

	// data is what the guard returned; children is the page being shown.
	let { data, children }: LayoutProps = $props();

	// Set to true to show the "Couldn't sign out" message in the header.
	let signOutFailed = $state(false);

	async function handleSignOut() {
		signOutFailed = false;
		try {
			await signOut(fetch);
			await goto('/sign-in');
		} catch {
			signOutFailed = true;
		}
	}
</script>

<div class="min-h-screen bg-slate-50">
	<header class="border-b border-slate-200 bg-white">
		<div class="mx-auto flex max-w-6xl items-center justify-between px-6 py-3">
			<a href="/" class="font-semibold text-slate-900">Bike Shop Service Desk</a>
			<div class="flex items-center gap-4 text-sm">
				{#if signOutFailed}
					<span role="alert" class="text-red-700">Couldn't sign out. Try again.</span>
				{/if}
				<span class="text-slate-600">
					{data.currentUser.displayName} · {data.currentUser.role}
				</span>
				<button
					type="button"
					onclick={handleSignOut}
					class="rounded-md border border-slate-300 px-3 py-1.5 font-medium text-slate-700 hover:bg-slate-100"
				>
					Sign out
				</button>
			</div>
		</div>
	</header>

	<main class="mx-auto max-w-6xl px-6 py-8">
		<!-- The page for the current address is drawn here. -->
		{@render children()}
	</main>
</div>
