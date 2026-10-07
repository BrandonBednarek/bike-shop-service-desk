import type { HoldReason, JobType, WorkOrderStatus } from '../api/types';

export const statusLabels: Record<WorkOrderStatus, string> = {
	CheckedIn: 'Checked in',
	InProgress: 'In progress',
	OnHold: 'On hold',
	ReadyForPickup: 'Ready for pickup',
	Collected: 'Collected',
	Cancelled: 'Cancelled'
};

export const jobTypeLabels: Record<JobType, string> = {
	TuneUp: 'Tune-up',
	Overhaul: 'Overhaul',
	FlatRepair: 'Flat repair',
	Brakes: 'Brakes',
	Drivetrain: 'Drivetrain',
	Wheel: 'Wheel',
	Suspension: 'Suspension',
	EBike: 'E-bike',
	Assembly: 'Assembly / build',
	Other: 'Other'
};

export const holdReasonLabels: Record<HoldReason, string> = {
	WaitingForParts: 'Waiting for parts',
	WaitingForCustomer: 'Waiting for customer'
};
