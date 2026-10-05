/** The RFC 9457 "problem details" body the API returns for every error. */
interface ProblemDetails {
	title?: string;
	detail?: string;
	traceId?: string;
}

/** A failed API call, carrying the status code and the API's human-readable message. */
export class ApiError extends Error {
	readonly status: number;
	readonly detail: string | undefined;
	readonly traceId: string | undefined;

	private constructor(status: number, problem: ProblemDetails) {
		super(problem.title ?? `The request failed (${status}).`);
		this.status = status;
		this.detail = problem.detail;
		this.traceId = problem.traceId;
	}

	static async fromResponse(response: Response): Promise<ApiError> {
		return new ApiError(response.status, await readProblem(response));
	}

	/** What to show the user: the API's message, plus a reference for failures on the server. */
	get userMessage(): string {
		const reference =
			this.status >= 500 && this.traceId ? `Reference: ${this.traceId}` : undefined;
		return [this.message, this.detail, reference].filter(Boolean).join(' ');
	}
}

async function readProblem(response: Response): Promise<ProblemDetails> {
	try {
		return (await response.json()) as ProblemDetails;
	} catch {
		// An empty or non-JSON body, such as a proxy's error page.
		return {};
	}
}
