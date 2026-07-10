import { apiClient } from "@/src/shared/api/axios-instance";
import { Location, LocationAddress } from "./types";
import { queryOptions } from "@tanstack/react-query";
import { PagedResult, PaginationRequest } from "@/src/shared/api/types";
import { Envelope } from "@/src/shared/api/envelope";

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
