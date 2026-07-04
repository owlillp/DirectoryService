import { deprtmentsApi } from "@/src/entities/departments/api";
import { DepartmentWithChildren } from "@/src/entities/departments/types";
import { useQuery } from "@tanstack/react-query";

type UseRootDepartmentsResult = {
  departments: DepartmentWithChildren[];
  totalCount: number;
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
};

export function useRootDepartments(
  page = 1,
  size = 10,
  prefetch = 1,
): UseRootDepartmentsResult {
  const { data, isLoading, error, refetch } = useQuery({
    queryFn: () => deprtmentsApi.getRoots({ page, size, prefetch }),
    queryKey: ["root-departments", page, size, prefetch],
  });

  return {
    departments: data?.records ?? [],
    totalCount: data?.totalCount ?? 0,
    isLoading: isLoading,
    error: error?.message ?? null,
    refetch,
  };
}
