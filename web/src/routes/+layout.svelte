<!--
	Outermost layout: wraps every page, signed in or not. It loads the global styles, sets
	the tab's icon, and draws the current page (or the (app) layout around it).
-->
<script lang="ts">
	import './layout.css';
	import favicon from '#lib/assets/favicon.svg';

	let { children } = $props();
</script>

<!-- Each page sets its own <title>: when a page closes, Svelte doesn't put the old one back. -->
<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

{@render children()}
