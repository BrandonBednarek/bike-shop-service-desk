<!--
	Outermost layout: wraps every page, signed in or not. It loads the global styles, sets
	the tab's icon and title, and draws the current page (or the (app) layout around it).
-->
<script lang="ts">
	import './layout.css';
	import favicon from '#lib/assets/favicon.svg';

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
	<title>Bike Shop Service Desk</title>
</svelte:head>

{@render children()}
