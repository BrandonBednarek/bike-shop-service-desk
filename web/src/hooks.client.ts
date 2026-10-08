import type { HandleClientError } from '@sveltejs/kit/hooks';
import { ApiError } from '#lib/api/api-error.js';

// When a load function throws, this decides the message the error page shows. Without it, a
// failed API call there would show SvelteKit's generic "Internal Error".
export const handleError: HandleClientError = ({ kind, error }) => {
	if (kind !== 'unknown') {
		return;
	}
	if (error instanceof ApiError) {
		return { status: error.status, message: error.userMessage };
	}
	console.error(error);
	return { message: "Couldn't load this page. Check the connection and try again." };
};
