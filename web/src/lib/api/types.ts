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

export interface Customer {
	id: number;
	name: string;
	phone: string;
	email: string | null;
}

export interface NewCustomer {
	name: string;
	phone: string;
	email: string | null;
}

export interface User {
	id: number;
	displayName: string;
	isActive: boolean;
}

export type WorkOrderStatus =
	'CheckedIn' | 'InProgress' | 'OnHold' | 'ReadyForPickup' | 'Collected' | 'Cancelled';

export type JobType =
	| 'TuneUp'
	| 'Overhaul'
	| 'FlatRepair'
	| 'Brakes'
	| 'Drivetrain'
	| 'Wheel'
	| 'Suspension'
	| 'EBike'
	| 'Assembly'
	| 'Other';

export type HoldReason = 'WaitingForParts' | 'WaitingForCustomer';

/** What staff fill in when they check a bike in. Money is in cents. */
export interface WorkOrderDetails {
	customerId: number;
	bikeMakeModel: string;
	bikeColour: string;
	jobType: JobType;
	workRequested: string;
	estimatedLabourMinutes: number;
	labourRateCentsPerHour: number;
	estimatedPartsCents: number;
	promisedOn: string;
	assignedToUserId: number | null;
}

/** Money is in cents, promisedOn is a "yyyy-mm-dd" date, and fields ending AtUtc are UTC timestamps. */
export interface WorkOrder {
	id: number;
	customerId: number;
	customerName: string;
	customerPhone: string;
	bikeMakeModel: string;
	bikeColour: string;
	jobType: JobType;
	workRequested: string;
	estimatedLabourMinutes: number;
	labourRateCentsPerHour: number;
	estimatedPartsCents: number;
	estimateTotalCents: number;
	billCents: number;
	isOverEstimate: boolean;
	loggedLabourMinutes: number;
	promisedOn: string;
	assignedToUserId: number | null;
	status: WorkOrderStatus;
	statusChangedAtUtc: string;
	holdReason: HoldReason | null;
	posReceiptNumber: string | null;
	cancellationReason: string | null;
	checkedInByUserId: number;
	checkedInAtUtc: string;
	labourEntries: LabourEntry[];
	partLines: PartLine[];
	notes: JobNote[];
}

export interface LabourEntry {
	id: number;
	mechanicUserId: number;
	minutes: number;
	note: string | null;
	loggedAtUtc: string;
}

export interface PartLine {
	id: number;
	description: string;
	quantity: number;
	unitPriceCents: number;
	totalCents: number;
	addedAtUtc: string;
}

export interface JobNote {
	id: number;
	text: string;
	writtenByUserId: number;
	writtenAtUtc: string;
}
