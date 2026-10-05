import { ApiError } from './api-error';

export type Fetch = typeof globalThis.fetch;

// The front end's only way to call the JSON API. Load functions pass the fetch SvelteKit gives
// them; components pass the browser's fetch.

export function getJson<T>(fetch: Fetch, path: string): Promise<T> {
	return send<T>(fetch, path, { method: 'GET' });
}

export function postJson<T>(fetch: Fetch, path: string, body?: unknown): Promise<T> {
	return send<T>(fetch, path, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: body === undefined ? undefined : JSON.stringify(body)
	});
}

async function send<T>(fetch: Fetch, path: string, init: RequestInit): Promise<T> {
	const response = await fetch(path, init);
	if (!response.ok) {
		throw await ApiError.fromResponse(response);
	}
	return readBody<T>(response);
}

async function readBody<T>(response: Response): Promise<T> {
	const text = await response.text();
	return (text ? JSON.parse(text) : undefined) as T;
}
