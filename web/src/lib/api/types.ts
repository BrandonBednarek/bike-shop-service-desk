export type UserRole = 'Owner' | 'Staff';

export interface CurrentUser {
	id: number;
	username: string;
	displayName: string;
	role: UserRole;
}

export interface Credentials {
	username: string;
	password: string;
}
