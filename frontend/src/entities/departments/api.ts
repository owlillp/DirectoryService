import { apiClient } from "@/src/shared/api/axios-instance";
import { DepartmentWithChildren } from "./types";
import { Envelope } from "@/src/shared/api/envelope";
import { PagedResult } from "@/src/shared/api/types";

export type GetRootDepartmentsRequest = {
  page: number;
  size: number;
  prefetch: number;
};

export const departmentsApi = {
  getRoots: async (request: GetRootDepartmentsRequest) => {
    const response = await apiClient.get<
      Envelope<PagedResult<DepartmentWithChildren>>
    >("/Departments/roots", { params: request });
    return response.data.result;
  },
};
