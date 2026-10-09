<!--
	The demo accounts, listed under the sign-in form in demo mode, so a reviewer can try each
	role without looking up a password. Choosing one hands it to the sign-in page through
	onchoose, which fills in the form and signs in.
-->
<script lang="ts">
	import type { DemoAccount } from '#lib/api/types.js';

	let {
		accounts,
		onchoose
	}: { accounts: DemoAccount[]; onchoose: (account: DemoAccount) => void } = $props();
</script>

<section
	aria-labelledby="demo-accounts-heading"
	class="w-full max-w-sm rounded-lg border border-slate-200 bg-white p-6 shadow-sm"
>
	<h2 id="demo-accounts-heading" class="text-sm font-semibold text-slate-900">Demo accounts</h2>
	<ul class="mt-2 divide-y divide-slate-100">
		{#each accounts as account (account.username)}
			<li class="flex items-center justify-between gap-4 py-3">
				<div class="text-sm">
					<div class="font-medium text-slate-900">
						{account.displayName} · {account.role}
					</div>
					<div class="font-mono text-xs text-slate-600">
						{account.username} / {account.password}
					</div>
					{#if !account.isActive}
						<div class="text-xs text-slate-500">
							Deactivated, to show what a former staff member sees.
						</div>
					{/if}
				</div>
				<button
					type="button"
					onclick={() => onchoose(account)}
					class="shrink-0 rounded-md border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-100"
				>
					Sign in
					<span class="sr-only">as {account.displayName}</span>
				</button>
			</li>
		{/each}
	</ul>
</section>
