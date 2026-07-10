import { ErrorMessage } from "./errors";

export type Envelope<T = unknown> = {
  result?: T | null;
  errors?: ErrorMessage[] | null;
  timeGenerated: string;
  isFailure: boolean;
  isSuccess: boolean;
};
