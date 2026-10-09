<!--
	The sign-in page, at /sign-in. It sits outside the (app) folder, so it has no header, and
	+page.ts sends anyone already signed in to the job board. On success it goes to the job
	board; on failure it shows the API's message, or a connection message when the API can't
	be reached. In demo mode the demo accounts are listed under the form.
-->
<script lang="ts">
	import { goto } from '$app/navigation';
	import { describeFailure } from '#lib/api/api-error.js';
	import { signIn } from '#lib/api/auth.js';
	import type { DemoAccount } from '#lib/api/types.js';
	import DemoAccounts from './DemoAccounts.svelte';
	import type { PageProps } from './$types';

	let { data }: PageProps = $props();

	// Kept in step with the two inputs below through bind:value.
	let username = $state('');
	let password = $state('');

	let errorMessage = $state('');
	let submitting = $state(false);

	async function handleSubmit(event: SubmitEvent) {
		// Handle the form here instead of letting the browser post it and reload the page.
		event.preventDefault();
		await submitCredentials();
	}

	// Fills in the form first, so it's clear which account was used. The demo buttons stay
	// enabled, so keyboard focus stays on them, and a click during a sign-in is ignored here.
	async function signInAs(account: DemoAccount) {
		if (submitting) {
			return;
		}
		username = account.username;
		password = account.password;
		await submitCredentials();
	}

	async function submitCredentials() {
		submitting = true;
		errorMessage = '';
		try {
			await signIn(fetch, { username, password });
			await goto('/');
		} catch (error) {
			errorMessage = describeFailure(error);
		} finally {
			submitting = false;
		}
	}
</script>

<svelte:head>
	<title>Sign in · Bike Shop Service Desk</title>
</svelte:head>

<main class="flex min-h-screen flex-col items-center justify-center gap-4 bg-slate-50 px-4 py-8">
	<div class="w-full max-w-sm rounded-lg border border-slate-200 bg-white p-8 shadow-sm">
		<h1 class="text-xl font-semibold text-slate-900">Bike Shop Service Desk</h1>
		<p class="mt-1 text-sm text-slate-600">Sign in to continue.</p>

		<form class="mt-6 space-y-4" onsubmit={handleSubmit}>
			<div>
				<label for="username" class="block text-sm font-medium text-slate-700"
					>Username</label
				>
				<!-- svelte-ignore a11y_autofocus: signing in is the only thing to do on this page -->
				<input
					id="username"
					bind:value={username}
					required
					autocomplete="username"
					autofocus
					class="mt-1 block w-full rounded-md border-slate-300"
				/>
			</div>

			<div>
				<label for="password" class="block text-sm font-medium text-slate-700"
					>Password</label
				>
				<input
					id="password"
					type="password"
					bind:value={password}
					required
					autocomplete="current-password"
					class="mt-1 block w-full rounded-md border-slate-300"
				/>
			</div>

			{#if errorMessage}
				<p role="alert" class="rounded-md bg-red-50 px-3 py-2 text-sm text-red-800">
					{errorMessage}
				</p>
			{/if}

			<button
				type="submit"
				disabled={submitting}
				class="w-full rounded-md bg-slate-900 px-4 py-2 font-medium text-white hover:bg-slate-800 disabled:opacity-60"
			>
				{submitting ? 'Signing in…' : 'Sign in'}
			</button>
		</form>
	</div>

	{#if data.demoAccounts.length > 0}
		<DemoAccounts accounts={data.demoAccounts} onchoose={signInAs} />
	{/if}
</main>
