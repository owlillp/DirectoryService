import { apiClient } from "@/src/shared/api/axios-instance";
import { Location } from "./types";

export type GetLocationsRequest = {
  departmentIds?: string[];
  search?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: string;
  pagination?: PaginationRequest;
  signal?: AbortSignal;
};

export type PaginationRequest = {
  page: number;
  pageSize: number;
};

export type ErrorItem = {
  code: string;
  message: string;
  type: ErrorType;
  invalidField?: string | null;
};

export type Errors = {
  errors: ErrorItem[];
};

export type Envelope<T = unknown> = {
  result?: T | null;
  errors?: Errors | null;
  timeGenerated: string;
  isFailure: boolean;
  isSuccess: boolean;
};

export type PagedResult<T> = {
  records: T[];
  totalCount: number;
};

export type ErrorType =
  | "validation"
  | "not_found"
  | "failure"
  | "conflict"
  | "canceled";

export const locationsApi = {
  getLocations: async (request: GetLocationsRequest) => {
    const { signal, ...params } = request;
    const response = await apiClient.get<Envelope<PagedResult<Location>>>(
      "/Locations",
      { params, signal },
    );
    return response.data.result;
  },
};
