import { apiClient } from "@/src/shared/api/axios-instance";
import { Envelope, PagedResult } from "../locations/api";
import { DepartmentWithChildren } from "./types";

export type GetRootDepartmentsRequest = {
  page: number;
  size: number;
  prefetch: number;
};

export const deprtmentsApi = {
  getRoots: async (request: GetRootDepartmentsRequest) => {
    const response = await apiClient.get<
      Envelope<PagedResult<DepartmentWithChildren>>
    >("/Departments/roots", { params: request });
    return response.data.result;
  },
};
