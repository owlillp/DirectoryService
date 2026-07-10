export type ErrorMessage = {
  code: string;
  message: string;
  type: ErrorType;
  invalidField?: string | null;
};

export type ErrorType =
  | "validation"
  | "not_found"
  | "failure"
  | "conflict"
  | "canceled";

export class EnvelopeError extends Error {
  public readonly errors: ErrorMessage[];
  public readonly type: ErrorType;

  constructor(errors: ErrorMessage[]) {
    const firstError = errors[0];

    super(firstError.message ?? "Неизвестная ошибка");

    this.name = "EnvelopeError";
    this.errors = errors;
    this.type = firstError.type;

    Object.setPrototypeOf(this, EnvelopeError.prototype);
  }

  get firstError(): ErrorMessage {
    return this.errors[0];
  }

  get allMessages(): string[] {
    return this.errors.map((er) => er.message);
  }
}

export function isEnvelopeError(error: unknown): error is EnvelopeError {
  return error instanceof EnvelopeError;
}
