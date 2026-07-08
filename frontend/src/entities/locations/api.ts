import { apiClient } from "@/src/shared/api/axios-instance";
import { Location, LocationAddress } from "./types";
import { queryOptions } from "@tanstack/react-query";

export type GetLocationsRequest = {
  departmentIds?: string[];
  search?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: string;
  pagination?: PaginationRequest;
};

export type CreateLocationsRequest = {
  name: string;
  address: LocationAddress;
  timeZone: string;
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
    const response = await apiClient.get<Envelope<PagedResult<Location>>>(
      "/Locations",
      { params: request },
    );
    return response.data.result;
  },
  createLocation: async (request: CreateLocationsRequest) => {
    const response = await apiClient.post<Envelope<string>>(
      "/Locations",
      request,
    );
    return response.data.result;
  },
};

export const locationsQueryOptions = {
  baseKey: "locations",
  getLocationsOptions: ({
    page,
    pageSize,
  }: {
    page: number;
    pageSize: number;
  }) => {
    return queryOptions({
      queryFn: () =>
        locationsApi.getLocations({
          pagination: { page, pageSize },
          sortBy: "created_at",
          sortDirection: "desc",
        }),
      queryKey: [locationsQueryOptions.baseKey, { page, pageSize }],
    });
  },
};
