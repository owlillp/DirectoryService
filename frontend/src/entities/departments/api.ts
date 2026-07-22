import { apiClient } from "@/src/shared/api/axios-instance";
import { DepartmentWithChildren, SearchDepartment } from "./types";
import { Envelope } from "@/src/shared/api/envelope";
import { PagedResult } from "@/src/shared/api/types";
import { infiniteQueryOptions } from "@tanstack/react-query";

export type GetRootDepartmentsRequest = {
  page: number;
  size: number;
  prefetch: number;
};

export type SearchDepartmentsRequest = {
  search?: string;
  isActive?: boolean;
  parentId?: string;
  locationIds?: string[];
  exludeIds?: string[];
  sortBy?: string;
  sortDirection?: string;
  page: number;
  pageSize: number;
};

export const departmentsApi = {
  getRoots: async (request: GetRootDepartmentsRequest) => {
    const response = await apiClient.get<
      Envelope<PagedResult<DepartmentWithChildren>>
    >("/Departments/roots", { params: request });
    return response.data.result;
  },

  search: async (request: SearchDepartmentsRequest) => {
    const response = await apiClient.get<
      Envelope<PagedResult<SearchDepartment>>
    >("/Departments", { params: request });
    return response.data.result;
  },
};

export type DepartmentsSearchInfiniteParams = {
  search?: string;
  isActive?: boolean;
  parentId?: string;
  locationIds?: string[];
  exludeIds?: string[];
  sortBy?: string;
  sortDirection?: string;
  pageSize: number;
};

export const departmentsQueryOptions = {
  baseKey: "departments",

  searchInfiniteOptions: (params: DepartmentsSearchInfiniteParams) => {
    return infiniteQueryOptions({
      queryKey: [departmentsQueryOptions.baseKey, params],
      queryFn: ({ pageParam }) =>
        departmentsApi.search({ ...params, page: pageParam ?? 1 }),
      initialPageParam: 1,
      getNextPageParam: (response) => {
        return !response || response.page >= response.totalPages
          ? undefined
          : response.page + 1;
      },
      select: (data): PagedResult<SearchDepartment> => ({
        records: data.pages.flatMap((page) => page?.records ?? []),
        totalCount: data.pages[data.pages.length - 1]?.totalCount ?? 0,
        page: data.pages[data.pages.length - 1]?.page ?? 0,
        pageSize: data.pages[data.pages.length - 1]?.pageSize ?? 0,
        totalPages: data.pages[data.pages.length - 1]?.totalPages ?? 0,
      }),
    });
  },
};
