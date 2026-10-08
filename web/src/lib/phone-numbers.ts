// The same rule as the API's PhoneNumbers.ToDigits, so the board finds a phone number the
// same way check-in does: digits only, without a North American "1" in front.
export function toPhoneDigits(phone: string): string {
	const digits = phone.replace(/\D/g, '');
	return digits.length === 11 && digits.startsWith('1') ? digits.slice(1) : digits;
}
