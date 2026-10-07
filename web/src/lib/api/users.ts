import { getJson, type Fetch } from './client';
import type { User } from './types';

/** Everyone, including inactive users, so old work still shows a name. */
export function listUsers(fetch: Fetch): Promise<User[]> {
	return getJson<User[]>(fetch, '/api/users');
}
