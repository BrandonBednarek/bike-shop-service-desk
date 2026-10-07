import { getJson, postJson, type Fetch } from './client';
import type { Customer, NewCustomer } from './types';

/** The API returns at most this many matches. */
export const maxCustomerMatches = 20;

/** Customers whose name or phone number contains the search, sorted by name. */
export function searchCustomers(fetch: Fetch, search: string): Promise<Customer[]> {
	return getJson<Customer[]>(fetch, `/api/customers?${new URLSearchParams({ search })}`);
}

export function createCustomer(fetch: Fetch, customer: NewCustomer): Promise<Customer> {
	return postJson<Customer>(fetch, '/api/customers', customer);
}
