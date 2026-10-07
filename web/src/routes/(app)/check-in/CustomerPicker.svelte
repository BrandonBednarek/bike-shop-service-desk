<!--
	The customer step of check-in. Staff search by phone number or name first, so a returning
	customer isn't added twice; "Add a new customer" only appears after a search. Picking a
	match, or adding someone new, hands the customer to the check-in page through onselect.
-->
<script lang="ts">
	import { describeFailure } from '#lib/api/api-error.js';
	import { createCustomer, maxCustomerMatches, searchCustomers } from '#lib/api/customers.js';
	import type { Customer } from '#lib/api/types.js';

	let { onselect }: { onselect: (customer: Customer) => void } = $props();

	let search = $state('');
	// null until the first search, so "No customers match" can't show before one.
	let matches = $state<Customer[] | null>(null);

	let addingNewCustomer = $state(false);
	let newName = $state('');
	let newPhone = $state('');
	let newEmail = $state('');

	let errorMessage = $state('');
	let busy = $state(false);

	async function handleSearch(event: SubmitEvent) {
		event.preventDefault();
		await callApi(async () => {
			matches = await searchCustomers(fetch, search.trim());
		});
	}

	// Starts the form with what was searched for: digits are most likely a phone number,
	// anything else a name.
	function startNewCustomer() {
		const text = search.trim();
		const looksLikePhone = /\d/.test(text);
		newPhone = looksLikePhone ? text : '';
		newName = looksLikePhone ? '' : text;
		addingNewCustomer = true;
	}

	async function handleAddCustomer(event: SubmitEvent) {
		event.preventDefault();
		await callApi(async () => {
			const customer = await createCustomer(fetch, {
				name: newName.trim(),
				phone: newPhone.trim(),
				email: newEmail.trim() || null
			});
			onselect(customer);
		});
	}

	// Disables the buttons while the call runs, and shows its message if it fails.
	async function callApi(call: () => Promise<void>) {
		busy = true;
		errorMessage = '';
		try {
			await call();
		} catch (error) {
			errorMessage = describeFailure(error);
		} finally {
			busy = false;
		}
	}
</script>

<form onsubmit={handleSearch} class="flex gap-2">
	<!-- svelte-ignore a11y_autofocus: finding the customer is the first thing to do on this page -->
	<input
		type="search"
		bind:value={search}
		required
		autofocus
		placeholder="Phone number or name"
		aria-label="Find a customer by phone number or name"
		class="w-72 rounded-md border-slate-300 text-sm"
	/>
	<button
		type="submit"
		disabled={busy}
		class="rounded-md border border-slate-300 bg-white px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-100 disabled:opacity-60"
	>
		Find
	</button>
</form>

{#if matches}
	{#if matches.length === 0}
		<p class="mt-3 text-sm text-slate-600">No customers match.</p>
	{:else}
		<ul
			class="mt-3 max-w-xl divide-y divide-slate-100 rounded-md border border-slate-200 bg-white"
		>
			{#each matches as match (match.id)}
				<li>
					<button
						type="button"
						onclick={() => onselect(match)}
						class="flex w-full justify-between gap-4 px-4 py-2 text-left text-sm hover:bg-slate-50"
					>
						<span class="font-medium text-slate-900">{match.name}</span>
						<span class="text-slate-600">{match.phone}</span>
					</button>
				</li>
			{/each}
		</ul>
		{#if matches.length === maxCustomerMatches}
			<p class="mt-2 text-sm text-slate-600">
				Showing the first {maxCustomerMatches} matches. Type more of the name, or search by phone
				number.
			</p>
		{/if}
	{/if}

	{#if addingNewCustomer}
		<form
			onsubmit={handleAddCustomer}
			class="mt-4 grid max-w-xl grid-cols-2 gap-4 rounded-md border border-slate-200 bg-white p-4"
		>
			<!-- The "Add a new customer" button has gone, so focus moves to the first empty field. -->
			<label class="block text-sm font-medium text-slate-700">
				Name
				<!-- svelte-ignore a11y_autofocus -->
				<input
					bind:value={newName}
					required
					maxlength="100"
					autofocus={newName === ''}
					class="mt-1 block w-full rounded-md border-slate-300 text-sm"
				/>
			</label>

			<label class="block text-sm font-medium text-slate-700">
				Phone
				<!-- svelte-ignore a11y_autofocus -->
				<input
					type="tel"
					bind:value={newPhone}
					required
					maxlength="30"
					autofocus={newName !== ''}
					class="mt-1 block w-full rounded-md border-slate-300 text-sm"
				/>
			</label>

			<label class="col-span-2 block text-sm font-medium text-slate-700">
				Email (optional)
				<input
					type="email"
					bind:value={newEmail}
					maxlength="200"
					class="mt-1 block w-full rounded-md border-slate-300 text-sm"
				/>
			</label>

			<div class="col-span-2">
				<button
					type="submit"
					disabled={busy}
					class="rounded-md bg-slate-900 px-4 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-60"
				>
					Add customer
				</button>
			</div>
		</form>
	{:else}
		<button
			type="button"
			onclick={startNewCustomer}
			class="mt-3 text-sm font-medium text-slate-700 underline hover:text-slate-900"
		>
			Add a new customer
		</button>
	{/if}
{/if}

{#if errorMessage}
	<p role="alert" class="mt-3 max-w-xl rounded-md bg-red-50 px-3 py-2 text-sm text-red-800">
		{errorMessage}
	</p>
{/if}
