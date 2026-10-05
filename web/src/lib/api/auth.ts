import { ApiError } from './api-error';
import { getJson, postJson, type Fetch } from './client';
import type { CurrentUser, Credentials } from './types';

/** The signed-in user, or null when nobody is signed in. */
export async function findCurrentUser(fetch: Fetch): Promise<CurrentUser | null> {
	try {
		return await getJson<CurrentUser>(fetch, '/api/auth/me');
	} catch (error) {
		if (error instanceof ApiError && error.status === 401) {
			return null;
		}
		throw error;
	}
}

export function signIn(fetch: Fetch, credentials: Credentials): Promise<CurrentUser> {
	return postJson<CurrentUser>(fetch, '/api/auth/sign-in', credentials);
}

export function signOut(fetch: Fetch): Promise<void> {
	return postJson<void>(fetch, '/api/auth/sign-out');
}
