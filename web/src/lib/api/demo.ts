import { getJson, type Fetch } from './client';
import type { DemoAccount } from './types';

/** The accounts to list on the sign-in page; empty unless the API runs in demo mode. */
export function listDemoAccounts(fetch: Fetch): Promise<DemoAccount[]> {
	return getJson<DemoAccount[]>(fetch, '/api/demo-accounts');
}
