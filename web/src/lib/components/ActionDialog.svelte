<!--
	A pop-up form that asks before a change is made, and collects anything the change needs,
	such as the receipt number for a collection. It uses the browser's own <dialog> element,
	which keeps focus inside it, dims the page behind it and closes on Esc. The parent shows it
	with {#if} and removes it again in onclose. If the change fails, the API's message shows
	here and the dialog stays open. While the change is saving the dialog can't be closed,
	because closing it wouldn't stop a request that has already been sent.
-->
<script lang="ts">
	import type { Snippet } from 'svelte';
	import { describeFailure } from '#lib/api/api-error.js';

	interface Props {
		title: string;
		confirmLabel: string;
		danger?: boolean;
		onconfirm: () => Promise<void>;
		onclose: () => void;
		// The dialog's fields and text, written between <ActionDialog> and </ActionDialog>.
		children?: Snippet;
	}

	let { title, confirmLabel, danger = false, onconfirm, onclose, children }: Props = $props();

	// A unique id for this dialog's heading, so aria-labelledby can give the dialog its name.
	const titleId = $props.id();

	// bind:this below fills this in with the <dialog> element once it's on the page.
	let dialog: HTMLDialogElement;
	let errorMessage = $state('');
	let saving = $state(false);

	async function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		saving = true;
		errorMessage = '';
		try {
			await onconfirm();
			dialog.close();
		} catch (error) {
			errorMessage = describeFailure(error);
		} finally {
			saving = false;
		}
	}
</script>

<!-- {@attach} runs once the element is on the page; showModal() opens it as a modal. Closing
	it, with a button or Esc, fires the close event, which calls onclose. Esc fires cancel first,
	and preventDefault() there keeps the dialog open while saving. -->
<dialog
	bind:this={dialog}
	{@attach (element) => element.showModal()}
	{onclose}
	oncancel={(event) => {
		if (saving) {
			event.preventDefault();
		}
	}}
	aria-labelledby={titleId}
	class="m-auto w-full max-w-md rounded-lg p-6 shadow-xl backdrop:bg-slate-900/40"
>
	<form onsubmit={handleSubmit}>
		<h2 id={titleId} class="text-lg font-semibold text-slate-900">{title}</h2>
		<div class="mt-4 space-y-4 text-sm text-slate-700">
			{@render children?.()}
		</div>

		{#if errorMessage}
			<p role="alert" class="mt-4 rounded-md bg-red-50 px-3 py-2 text-sm text-red-800">
				{errorMessage}
			</p>
		{/if}

		<div class="mt-6 flex justify-end gap-3 text-sm font-medium">
			<button
				type="button"
				disabled={saving}
				onclick={() => dialog.close()}
				class="rounded-md border border-slate-300 px-3 py-1.5 text-slate-700 hover:bg-slate-100 disabled:opacity-60"
			>
				Go back
			</button>
			<button
				type="submit"
				disabled={saving}
				class={[
					'rounded-md px-3 py-1.5 text-white disabled:opacity-60',
					danger ? 'bg-red-700 hover:bg-red-800' : 'bg-slate-900 hover:bg-slate-800'
				]}
			>
				{saving ? 'Saving…' : confirmLabel}
			</button>
		</div>
	</form>
</dialog>
