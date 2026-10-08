import type { User } from './api/types';

export function displayNameOf(users: User[], userId: number): string {
	return users.find((user) => user.id === userId)?.displayName ?? 'Unknown user';
}
