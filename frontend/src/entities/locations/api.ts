import { apiClient } from "@/src/shared/api/axios-instance";
import { Location, LocationAddress } from "./types";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";
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

export type UpdateLocationRequest = {
  locationId: string;
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
    return response.data;
  },

  deleteLocation: async (locationId: string) => {
    const response = await apiClient.delete<Envelope>(
      `/Locations/${locationId}`,
    );
    return response.data;
  },

  updateLocation: async ({
    locationId,
    name,
    timeZone,
    address,
  }: UpdateLocationRequest) => {
    const response = await apiClient.patch<Envelope>(
      `/Locations/${locationId}`,
      { name, timeZone, address },
    );
    return response.data;
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
          sortBy: "name",
          sortDirection: "desc",
        }),
      queryKey: [locationsQueryOptions.baseKey, { page, pageSize }],
    });
  },

  getLocationsInfiniteOptions: (pageSize: number) => {
    return infiniteQueryOptions({
      queryKey: [locationsQueryOptions.baseKey],
      queryFn: ({ pageParam }) =>
        locationsApi.getLocations({
          pagination: { page: pageParam ?? 1, pageSize },
          sortBy: "name",
          sortDirection: "asc",
        }),
      initialPageParam: 1,
      getNextPageParam: (response) => {
        return !response || response.page >= response.totalPages
          ? undefined
          : response.page + 1;
      },
      select: (data): PagedResult<Location> => ({
        records: data.pages.flatMap((page) => page?.records ?? []),
        totalCount: data.pages[data.pages.length - 1]?.totalCount ?? 0,
        page: data.pages[data.pages.length - 1]?.page ?? 0,
        pageSize: data.pages[data.pages.length - 1]?.pageSize ?? 0,
        totalPages: data.pages[data.pages.length - 1]?.totalPages ?? 0,
      }),
    });
  },
};
