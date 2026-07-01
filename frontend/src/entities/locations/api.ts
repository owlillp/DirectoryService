import { apiClient } from "@/src/shared/api/axios-instance";

export type GetLocationsRequest = {
  DepartmentIds?: string[];
  Search?: string;
  IsActive?: boolean;
  SortBy?: string;
  SortDirection?: string;
  Pagination?: PaginationRequest;
};

export type PaginationRequest = {
  Page: number;
  PageSize: number;
};

export type ErrorItem = {
  Code: string;
  Message: string;
  Type: ErrorType;
  InvalidField?: string | null;
};

export type Errors = {
  Errors: ErrorItem[];
};

export type Envelope<T = unknown> = {
  Result?: T | null;
  Errors?: Errors | null;
  TimeGenerated: string;
  IsFailure: boolean;
  IsSuccess: boolean;
};

export type PagedResult<T> = {
  Records: T[];
  TotalCount: number;
};

export type ErrorType =
  | "validation"
  | "not_found"
  | "failure"
  | "conflict"
  | "canceled";

export const locationsApi = {
  getLocations: async (request: GetLocationsRequest) => {
    const response = await apiClient.get<Envelope<PagedResult<Location>>>(
      "/Locations",
      { params: request },
    );
    return response.data.Result;
  },
};
